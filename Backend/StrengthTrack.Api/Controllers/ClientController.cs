using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrengthTrack.Api.Entities;
using StrengthTrack.Api.HiveMQ;
using StrengthTrack.Api.Models;
using StrengthTrack.Api.Persistence;

namespace StrengthTrack.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController : ControllerBase
{
    private static int _currentClientId = 0;
    private static IList<int> _values = new List<int>();
    private static HiveMQClient? _hiveMqClient = null;

    private StrengthTrackDbContext _context;

    public ClientController(StrengthTrackDbContext context)
    {
        _context = context;
    }

    [HttpGet()]
    public async Task<IActionResult> Get()
    {
        var clients = await (
            from c in _context.Clients
            select new ClientSimpleViewModel(c.Id, c.Name, c.Cpf)
        ).AsNoTracking()
        .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var client = await (
            from c in _context.Clients.Include(c => c.Sessions).ThenInclude(s => s.Results)
            where c.Id == id
            select c
        ).AsNoTracking()
        .FirstOrDefaultAsync();

        if (client is null) 
        {
            return NotFound($"We could not find a customer with id: {id}");
        }

        var sessions = new List<SessionViewModel>();
        foreach (var session in client.Sessions.OrderBy(c => c.Date))
        {
            var results = new List<ResultViewModel>();
            foreach (var result in session.Results)
            {
                results.Add(new ResultViewModel(result.Id, result.SessionId, result.Value));
            }

            sessions.Add(new SessionViewModel(session.Id, session.ClientId, sessions.Count + 1, (int)results.Average(r => r.Value), session.Date, results));
        }

        var clientViewModel = new ClientViewModel(client.Id, client.Name, client.Cpf, sessions);
        return Ok(clientViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Include(ClientInputModel inputModel)
    {
        if (string.IsNullOrWhiteSpace(inputModel.Name) || inputModel.Name.Length > 50)
        {
            return BadRequest($"The propertie \"{nameof(inputModel.Name)}\" is invalid. The name length must be between 1 and 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(inputModel.Cpf) || inputModel.Cpf.Length != 11 || inputModel.Cpf.Any(c => !char.IsDigit(c)))
        {
            return BadRequest($"The propertie \"{nameof(inputModel.Cpf)}\" is invalid! The cpf must have 11 digits.");
        }

        var isClientAlreadyRegistered = await _context.Clients.AnyAsync(c => c.Cpf == inputModel.Cpf);
        if (isClientAlreadyRegistered)
        {
            return BadRequest($"There is already a registered user this CPF ({inputModel.Cpf}).");
        }

        var client = new Client(0, inputModel.Name, inputModel.Cpf, null!);

        _context.Clients.Add(client);
        var failure = (await _context.SaveChangesAsync()) == 0;
        if (failure) 
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An gerenar error has ocurred!");
        }

        var clientSimpleViewModel = new ClientSimpleViewModel(client.Id, client.Name, client.Cpf);
        return CreatedAtAction(nameof(GetById), new { Id = client.Id }, clientSimpleViewModel);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ClientInputModel inputModel)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (client is null)
        {
            return NotFound($"We could not find a customer with id: {id}");
        }

        if (string.IsNullOrWhiteSpace(inputModel.Name) || inputModel.Name.Length > 50)
        {
            return BadRequest($"The propertie \"{nameof(inputModel.Name)}\" is invalid. The name length must be between 1 and 50 characters.");
        }

        client.Name = inputModel.Name;

        _context.Clients.Update(client);
        var failure = (await _context.SaveChangesAsync()) == 0;
        if (failure) 
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An gerenar error has ocurred!");
        }

        var clientSimpleViewModel = new ClientSimpleViewModel(client.Id, client.Name, client.Cpf);
        return Ok(clientSimpleViewModel);
    }

    [HttpPost("StartMeasurement/{id:int}")]
    public async Task<IActionResult> StartMeasurement(int id)
    {
        if (_currentClientId != 0)
        {
            var client = await _context.Clients.FirstAsync(c => c.Id == _currentClientId);
            return BadRequest($"The current measurement corresponds to client: \"{client.Name}\" ({client.Cpf}). You must end this measurement before starting a new one.");
        }

        var isClientRegistered = await _context.Clients.AnyAsync(c => c.Id == id);
        if (isClientRegistered)
        {
            HiveMqResponse hiveMq = ConnectToClusterHiveMq();
            if (hiveMq.Success)
            {
                _currentClientId = id;
                _hiveMqClient = hiveMq.Client;
                
                return Ok($"Measurement started for client with id: {id}.");
            }

            return StatusCode(StatusCodes.Status500InternalServerError, hiveMq.Result);
        }
        
        return NotFound($"We could not find a customer with id: {id}");
    }

    [HttpPost("StopMeasurement/{id:int}")]
    public async Task<IActionResult> StopMeasurement(int id)
    {
        if (_currentClientId == 0) 
        {
            return BadRequest("There is no measurement in progress. You must start a measurement before you can stop it."); 
        }
        
        if (id != _currentClientId)
        {
            var currentClient = await _context.Clients.FirstAsync(c => c.Id == _currentClientId);
            return BadRequest($"The current measurement corresponds to another client: \"{currentClient.Name}\" ({currentClient.Cpf}). You must end his measurement before starting a new one."); 
        }

        var client = await _context.Clients
            .Include(c => c.Sessions)
            .ThenInclude(s => s.Results)
            .FirstAsync(c => c.Id == id);

        if (client is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An gerenar error has ocurred!");
        }

        HiveMqResponse hiveMq = DisconnectToClusterHiveMq(_hiveMqClient!);
        if (!hiveMq.Success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, hiveMq.Result);
        }

        var results = new List<Result>();
        foreach (var value in _values)
        {
            results.Add(new Result(0, 0, value));
        };

        client.Sessions.Add(new Session(0, client.Id, DateTimeOffset.UtcNow, results));

        _context.Clients.Update(client);
        var failure = (await _context.SaveChangesAsync()) == 0;
        if (failure) 
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An gerenar error has ocurred!");
        }        
        
        _currentClientId = 0;
        _values.Clear();
        _hiveMqClient = null;

        return Ok($"Measurement completed successfully for customer: {client.Name} ({client.Cpf}).");
    }

    #region Connect To HiveMQ by MQTT
    private static HiveMqResponse ConnectToClusterHiveMq()
    {
        var options = new HiveMQClientOptions
        {
            Host = HiveMqConfiguration.HOST,
            Port = HiveMqConfiguration.PORT,
            UseTLS = HiveMqConfiguration.USETLS,
            UserName = HiveMqConfiguration.USERNAME,
            Password = HiveMqConfiguration.PASSWORD,
        };

        var client = new HiveMQClient(options);

        var currentAttemptNumber = 0;
        ConnectResult? connectResult = null;
        while (currentAttemptNumber < HiveMqConfiguration.MAX_ATTEMPTS_NUMBER && (connectResult is null || connectResult.ReasonCode != ConnAckReasonCode.Success))
        {
            try
            {
                connectResult = client.ConnectAsync().Result;
                if (connectResult.ReasonCode != ConnAckReasonCode.Success)
                {
                    Thread.Sleep(1000);
                }
            }
            catch
            {
                // Do not break the flow;
            }
            finally
            {
                currentAttemptNumber++;
            }
        }

        if (connectResult!.ReasonCode != ConnAckReasonCode.Success)
        {
            return new HiveMqResponse(client, false, $"An error occurred while attempting to communicate with HiveMQ (Reason: {Enum.GetName(typeof(ConnAckReasonCode), connectResult.ReasonCode)}).");
        }

        client.OnMessageReceived += (sender, args) =>
        {
            _values.Add(Convert.ToInt32(args.PublishMessage.PayloadAsString));
        };

        var subscribeResult = client.SubscribeAsync($"{HiveMqConfiguration.TOPIC}/#").Result;
        
        if (!subscribeResult.Subscriptions.Any())
        {
            return new HiveMqResponse(client, false, $"An error occurred while attempting to subscribe to topic \"{HiveMqConfiguration.TOPIC}\".");
        }

        return new HiveMqResponse(client, true, string.Empty);
    }

    private static HiveMqResponse DisconnectToClusterHiveMq(HiveMQClient client)
    {
        var unsubscribeResult = client.UnsubscribeAsync($"{HiveMqConfiguration.TOPIC}/#").Result;
        // if (unsubscribeResult.Subscriptions.Any())
        // {
        //     return new HiveMqResponse(client, false, $"An error occurred while attempting to unsubscribe to topic \"{HiveMqConfiguration.TOPIC}\".");
        // }

        var disconnectResult = client.DisconnectAsync().Result;
        if (!disconnectResult)
        {
            return new HiveMqResponse(client, false, $"An error occurred while attempting to unsubscribe to topic \"{HiveMqConfiguration.TOPIC}\".");
        }

        return new HiveMqResponse(client, true, string.Empty);
    }
    #endregion
}
