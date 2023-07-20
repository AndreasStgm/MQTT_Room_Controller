using MQTTnet;
using MQTTnet.Client;

namespace RoomControllerApp.HelperClasses
{
    public static class MqttClientHelper
    {
        static public MqttClient Client { get; private set; }
        static public bool IsConnected { get; private set; }

        static public async Task AttemptConnectClient()
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithCredentials("username", "password")
                    .WithTcpServer("brokerIP")
                    .WithTls(o =>
                    {
                        //    // The used public broker sometimes has invalid certificates. This sample accepts all
                        //    // certificates. This should not be used in live environments.
                        //    o.CertificateValidationHandler = _ => true;

                        //    // The default value is determined by the OS. Set manually to force version.
                        //    o.SslProtocol = SslProtocols.Tls12;
                    }).Build();

                using (var timeout = new CancellationTokenSource(5000))
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, timeout.Token);

                    Console.WriteLine("The MQTT client is connected.");
                }
            }
        }
    }
}
