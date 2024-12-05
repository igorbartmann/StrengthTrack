using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StrengthTrack.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo() 
    {
        Title = "Strength API Documentation",
        Description = "Strength Web API", 
        Version = "v1" ,
        Contact = new OpenApiContact { Name = "Igor Bartmann", Email = "igorbartmann11@gmail.com" }
    });
});

builder.Services.AddDbContext<StrengthTrackDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("StrengthConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(opts => opts
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseAuthorization();

app.MapControllers();

app.Run();
