using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

namespace RoomControllerApp;

public partial class MqttConfig : ContentPage
{
	public MqttConfig()
	{
		InitializeComponent();
	}

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
		//Display a confirmation message to make it more user-friendly
		Toast.Make("Configuration Saved", ToastDuration.Short, 14).Show();
    }
}