using System;

namespace StrengthTrack.Api.Models
{
    public class SessionViewModel
    {
         public SessionViewModel(int id, int clientId, int number, double averageResult, DateTimeOffset date, IList<ResultViewModel> results) 
        {   
            Id = id;
            ClientId = clientId;
            Number = number;
            AverageResult = averageResult;
            Date = date;
            Results = results ?? new List<ResultViewModel>();
        }

        public int Id {get;set;}
        public int ClientId {get;set;}
        public int Number {get;set;}
        public double AverageResult {get;set;}
        public DateTimeOffset Date {get;set;}

        public IList<ResultViewModel> Results {get;set;}
    }
}