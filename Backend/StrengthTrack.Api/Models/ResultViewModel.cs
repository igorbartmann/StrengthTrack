using System;

namespace StrengthTrack.Api.Models
{
    public class ResultViewModel
    {
        public ResultViewModel(int id, int sessionId, int value) 
        {
            Id = id;
            SessionId = sessionId;
            Value = value;
        }

        public int Id {get;set;}
        public int SessionId {get;set;}
        public int Value {get;set;}
    }
}