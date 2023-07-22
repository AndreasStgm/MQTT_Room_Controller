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
        static public IMqttClient Client { get; private set; }
        static public bool IsConnected { get; private set; }

        static public async Task<string> AttemptBuildAndConnectClient(int cancellationTimoutDuration)
        {
            MqttClientOptions clientOptions;
            string statusMessage;

            if (Client == null)
            {
                var mqttFactory = new MqttFactory();

                Client = mqttFactory.CreateMqttClient();

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
                clientOptions = mqttClientOptionsBuilder.Build();
            }
            else
            {
                clientOptions = Client.Options;
                Debug.WriteLine("Already existing client");
            }

            if (!IsConnected)
            {
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(cancellationTimoutDuration)))
                {
                    try
                    {
                        var response = await Client.ConnectAsync(clientOptions, timeout.Token);

                        if (response.ResultCode == MqttClientConnectResultCode.Success)
                        {
                            IsConnected = true;
                            statusMessage = "Connection established";
                        }
                        else
                        {
                            statusMessage = $"Connection failed with code: {response.ResultCode}";
                        }
                    }
                    catch (MqttCommunicationException e)
                    {
                        statusMessage = "Couldn't connect to broker due to configuration error";
                    }
                    catch (OperationCanceledException e)
                    {
                        statusMessage = "Timeout due to no response from broker";
                    }
                    catch (Exception e)
                    {
                        statusMessage = $"Unknown Error: {e.Message}";
                    }
                }
            }
            else
            {
                statusMessage = "Already connected";
            }

            return statusMessage;
        }

        static public async Task<string> AttemptDisconnectClient(int cancellationTimoutDuration)
        {
            string statusMessage;

            try
            {
                await Client.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build(),
                new CancellationTokenSource(TimeSpan.FromSeconds(cancellationTimoutDuration)).Token);

                IsConnected = false;
                Client = null;
                statusMessage = "Connection terminated";
            }
            catch (OperationCanceledException e)
            {
                statusMessage = "Timeout due to no response from broker";
            }
            catch (Exception e)
            {
                statusMessage = $"Unknown Error: {e.Message}";
            }

            return statusMessage;
        }

        static public bool SetConnectionLabelAndButton(Label displayLabel, Button displayButton)
        {
            string displayText;
            if (IsConnected)
            {
                displayText = "Connected";
                displayLabel.TextColor = Colors.LimeGreen;
                displayButton.Text = "Disconnect from Broker";
            }
            else
            {
                displayText = "Disconnected";
                displayLabel.TextColor = Colors.Red;
                displayButton.Text = "Connect to Broker";
            }
            displayLabel.Text = displayText;

            return IsConnected;
        }
    }
}
