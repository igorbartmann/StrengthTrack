using System;

namespace StrengthTrack.Api.HiveMQ
{
    public class HiveMqConfiguration
    {
        #region Constants
        public const string HOST = "acd37fac47394e56a264fd86e4cb8366.s1.eu.hivemq.cloud";
        public const int PORT = 8883;
        public const bool USETLS = true;
        public const string USERNAME = "ServerUser";
        public const string PASSWORD = "ServerPassw0rd";
        public const string TOPIC = "topic_sensor_loadcell";
        public const int MAX_ATTEMPTS_NUMBER = 10;
        #endregion
    }
}