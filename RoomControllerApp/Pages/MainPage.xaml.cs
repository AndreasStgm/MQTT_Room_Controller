using MQTTnet;
using RoomControllerApp.HelperClasses;

namespace RoomControllerApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void FlashButton_Clicked(object sender, EventArgs e)
    {
        if (MqttClientHelper.IsConnected)
        {
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic("flashTest")
                .WithPayload("FLASH")
                .Build();
            MqttClientHelper.Client.PublishAsync(message, CancellationToken.None);
        }
    }
}