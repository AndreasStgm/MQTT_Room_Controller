using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MQTTnet;
using MQTTnet.Client;
using System.Security.Authentication;

namespace RoomControllerApp;

public partial class MqttConfig : ContentPage
{
    public MqttConfig()
    {
        InitializeComponent();
    }

    private void CredentialsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (CredentialsSwitch.IsToggled)
        {
            UsernameLine.IsVisible = true;
            PasswordLine.IsVisible = true;
        }
        else
        {
            UsernameLine.IsVisible = false;
            PasswordLine.IsVisible = false;
        }
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        //Display a confirmation message to make it more user-friendly
        Toast.Make("Configuration Saved", ToastDuration.Short, 14).Show();
    }

    private async void CreateClient()
    {
        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithCredentials("username", "password")
                .WithTcpServer("brokerIP")
                .WithTls(o =>
                {
                    // The used public broker sometimes has invalid certificates. This sample accepts all
                    // certificates. This should not be used in live environments.
                    o.CertificateValidationHandler = _ => true;

                    // The default value is determined by the OS. Set manually to force version.
                    o.SslProtocol = SslProtocols.Tls12;
                }).Build();
            using (var timeout = new CancellationTokenSource(5000))
            {
                await mqttClient.ConnectAsync(mqttClientOptions, timeout.Token);

                Console.WriteLine("The MQTT client is connected.");
            }
        }
    }


}