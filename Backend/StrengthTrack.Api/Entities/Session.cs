using System;

namespace StrengthTrack.Api.Entities
{
    public class Session
    {
        #pragma warning disable CS8618
        protected Session() {}
        #pragma warning restore CS8618
        
        public Session(int id, int clientId, DateTimeOffset date, IList<Result> results) 
        {   
            Id = id;
            ClientId = clientId;
            Date = date;
            Client = null;
            Results = results ?? new List<Result>();
        }

        public int Id {get;set;}
        public int ClientId {get;set;}
        public DateTimeOffset Date {get;set;}

        public virtual Client? Client {get;set;}
        public virtual ICollection<Result> Results {get;set;} = new List<Result>();
    }
}