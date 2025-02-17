namespace CRM.BackgroundJob.Core.Models
{
    public class RedisConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public int DefaultDatabase { get; set; }
        public string ApplicationName { get; set; }
        public int ConnectRetry { get; set; }
    }
}