namespace RoomControllerApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private void FlashButton_Clicked(object sender, EventArgs e)
    {
		DisplayAlert("Test DisplayAlert", "Cringe", "OK");
    }
}