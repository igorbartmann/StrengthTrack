using System;

namespace StrengthTrack.Api.Entities
{
    public class Result
    {
        protected Result() {}

        public Result(int id, int sessionId, int value) 
        {
            Id = id;
            SessionId = sessionId;
            Value = value;
            Session = null;
        }

        public int Id {get;set;}
        public int SessionId {get;set;}
        public int Value {get;set;}

        public virtual Session? Session {get;set;}
    }
}