using System.Text;
using System.Text.Json;

namespace RoomControllerApp.HelperClasses
{
    public class MqttConfigHelper
    {
        private static string CONFIG_LOCATION = Path.Combine(FileSystem.Current.CacheDirectory, "mqtt_config.json");

        public string BrokerIp { get; set; }
        public int Port { get; set; }
        public bool IsTlsEnabled { get; set; }
        public bool IsCredentialsEnabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public MqttConfigHelper(string brokerIp, int port, bool isTlsEnabled, bool isCredentialsEnabled, string username, string password)
        {
            BrokerIp = brokerIp;
            Port = port;
            IsTlsEnabled = isTlsEnabled;
            IsCredentialsEnabled = isCredentialsEnabled;
            Username = username;
            Password = password;
        }

        public MqttConfigHelper(string brokerIp, int port, bool isTlsEnabled, bool isCredentialsEnabled)
            : this(brokerIp, port, isTlsEnabled, isCredentialsEnabled, string.Empty, string.Empty)
        { }

        public MqttConfigHelper()
            : this(string.Empty, 0, false, false)
        { }

        public bool WriteConfigToFile() //TODO: configure result to reflect success and check result
        {
            string jsonString = JsonSerializer.Serialize<MqttConfigHelper>(this, new JsonSerializerOptions { WriteIndented = true });

            using (StreamWriter writer = new StreamWriter(CONFIG_LOCATION))
            {
                writer.Write(jsonString);
            }
            return true;
        }

        public static MqttConfigHelper ReadConfigFromFile() //TODO: configure result to reflect success and check result
        {
            using (StreamReader reader = File.OpenText(CONFIG_LOCATION))
            {
                string jsonString = reader.ReadToEnd();

                return JsonSerializer.Deserialize<MqttConfigHelper>(jsonString);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("Broker IP:\t").AppendLine(BrokerIp)
                .Append("Port:\t").AppendLine(Port.ToString())
                .Append("TLS:\t").AppendLine(IsTlsEnabled.ToString())
                .Append("Credentials:\t").AppendLine(IsCredentialsEnabled.ToString())
                .Append("Username:\t").AppendLine(Username)
                .Append("Password:\t").AppendLine(Password);

            return result.ToString();
        }
    }
}
