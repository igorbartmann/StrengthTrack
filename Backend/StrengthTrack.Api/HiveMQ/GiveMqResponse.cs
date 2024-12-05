using System;
using HiveMQtt.Client;

namespace StrengthTrack.Api.HiveMQ
{
    public class HiveMqResponse
    {
        public HiveMqResponse(HiveMQClient client, bool success, string result)
        {
            Client = client;
            Success = success;
            Result = result;
        }

        public HiveMQClient Client;
        public bool Success {get;set;}
        public string? Result {get;set;}
    }
}