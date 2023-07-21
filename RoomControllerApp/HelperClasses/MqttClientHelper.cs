using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using System.Diagnostics;

namespace RoomControllerApp.HelperClasses
{
    public static class MqttClientHelper
    {
        static public MqttConfigHelper Config { get; set; }
        static public MqttClient Client { get; private set; }
        static public bool IsConnected { get; private set; }

        static public async Task AttemptConnectClient()
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(Config.BrokerIp, Config.Port)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60));

                if (Config.IsTlsEnabled)
                {
                    mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithTls();
                }
                if (Config.IsCredentialsEnabled)
                {
                    mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithCredentials(Config.Username, Config.Password);
                }
                var mqttClientOptionsTest = mqttClientOptionsBuilder.Build();

                using (var timeout = new CancellationTokenSource(10000))
                {
                    try
                    {
                        var response = await mqttClient.ConnectAsync(mqttClientOptionsTest, timeout.Token);

                        IsConnected = true;
                        Debug.WriteLine("Connected Successfully");
                    }
                    catch (MqttCommunicationException e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    catch (OperationCanceledException e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        static public bool SetConnectionLabel(Label displayLabel)
        {
            string displayText;
            if (IsConnected)
            {
                displayText = "Connected";
                displayLabel.TextColor = Colors.LimeGreen;
            }
            else
            {
                displayText = "Disconnected";
                displayLabel.TextColor = Colors.Red;
            }
            displayLabel.Text = displayText;

            return IsConnected;
        }
    }
}
