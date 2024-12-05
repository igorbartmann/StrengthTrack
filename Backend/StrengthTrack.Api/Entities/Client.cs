using System;

namespace StrengthTrack.Api.Entities
{
    public class Client
    {
        #pragma warning disable CS8618
        protected Client() { }
        #pragma warning restore CS8618

        public Client(int id, string name, string cpf, IList<Session> sessions) 
        {
            Id = id;
            Name = name;
            Cpf = cpf;
            Sessions = sessions ?? new List<Session>();
        }

        public int Id {get;set;}
        public string Name {get;set;}
        public string Cpf {get;set;}

        public virtual ICollection<Session> Sessions {get;set;}
    }
}