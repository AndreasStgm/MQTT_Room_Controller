using RoomControllerApp.HelperClasses;
using System.Diagnostics;

namespace RoomControllerApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        try
        {
            MqttConfigHelper config = MqttConfigHelper.ReadConfigFromFile();
            MqttClientHelper.Config = config;

            MqttClientHelper.AttemptBuildAndConnectClient(10);
        }
        catch (Exception)
        {
            Debug.WriteLine("No previous config found.");
        }
    }
}
