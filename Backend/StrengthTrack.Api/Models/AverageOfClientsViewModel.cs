using System;

namespace StrengthTrack.Api.Models
{
    public class AverageOfClientsViewModel
    {
        public AverageOfClientsViewModel(int averageForce, double averageSessions)
        {
            AverageForce = averageForce;
            AverageSessions = averageSessions;
        }

        public int AverageForce {get;set;}
        public double AverageSessions {get;set;}
    }
}
