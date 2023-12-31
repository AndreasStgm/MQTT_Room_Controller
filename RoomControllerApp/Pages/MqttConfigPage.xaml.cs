using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using RoomControllerApp.HelperClasses;
using System.Text.RegularExpressions;

namespace RoomControllerApp;

public partial class MqttConfigPage : ContentPage
{
    private bool initialLoad;

    public MqttConfigPage()
    {
        InitializeComponent(); //TODO: Stutter when loading window -> maybe fix with asynchronous behaviour?

        ipEntry.TextChanged += IpEntry_TextChanged;
        portEntry.TextChanged += PortEntry_TextChanged;
        tlsSwitch.Toggled += TlsSwitch_Toggled;
        credentialsSwitch.Toggled += CredentialsSwitch_Toggled;
        usernameEntry.TextChanged += CredentialEntry_TextChanged;
        passwordEntry.TextChanged += CredentialEntry_TextChanged;
        saveButton.Clicked += SaveButton_Clicked;
        connectButton.Clicked += ConnectButton_Clicked;

        initialLoad = true;

        string statusMessage;
        if (MqttClientHelper.Config != null)
        {
            ipEntry.Text = MqttClientHelper.Config.BrokerIp;
            portEntry.Text = Convert.ToString(MqttClientHelper.Config.Port);
            tlsSwitch.IsToggled = MqttClientHelper.Config.IsTlsEnabled;
            credentialsSwitch.IsToggled = MqttClientHelper.Config.IsCredentialsEnabled;
            ToggleCredentialsVisibility(MqttClientHelper.Config.IsCredentialsEnabled);
            usernameEntry.Text = MqttClientHelper.Config.Username;
            passwordEntry.Text = MqttClientHelper.Config.Password;

            connectButton.IsEnabled = true;
            statusMessage = "Previous configuration loaded";
        }
        else
        {
            ToggleCredentialsVisibility(false);
            connectButton.IsEnabled = false;
            statusMessage = "No previous config found";
        }
        Toast.Make(statusMessage, ToastDuration.Short).Show();

        initialLoad = false;

        MqttClientHelper.SetConnectionLabelAndButton(connectionLabel, connectButton);
    }

    private void IpEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ConfigurationChanged();

        IsIpWithinRange();
    }

    private void PortEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ConfigurationChanged();

        IsPortWithinRange();
    }

    private void TlsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        ConfigurationChanged();
    }

    private void CredentialsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        ConfigurationChanged();

        if (credentialsSwitch.IsToggled)
        {
            ToggleCredentialsVisibility(true);
        }
        else
        {
            ToggleCredentialsVisibility(false);
        }
    }

    private void CredentialEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ConfigurationChanged();

        Entry entry = (Entry)sender;
        IsCredentialEmpty(entry);
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        ConfigurationChanged();

        UnfocusChildElements(fullPage);
        UnfocusChildElements(credentialsSection);
        ipEntry.HideKeyboardAsync(CancellationToken.None); //Apparently hiding the keyboard for one entry that isn't even in focus hides the keyboard successfully

        string statusMessage;
        MqttConfigHelper config;

        if (!IsIpWithinRange())
        {
            statusMessage = "Incorrect IP configuration";
            ipEntry.Focus();
        }
        else if (!IsPortWithinRange())
        {
            statusMessage = "Incorrect port configuration";
            portEntry.Focus();
        }
        else if (credentialsSwitch.IsToggled)
        {
            if (IsCredentialEmpty(usernameEntry))
            {
                statusMessage = "Incorrect username configuration";
                usernameEntry.Focus();
            }
            else if (IsCredentialEmpty(passwordEntry))
            {
                passwordEntry.Focus();
                statusMessage = "Incorrect password configuration";
            }
            else
            {
                DisplayErrorMessage(credentialsErrorLabel, false, string.Empty);

                config = new MqttConfigHelper(
                    ipEntry.Text.Trim(),
                    Convert.ToInt32(portEntry.Text.Trim()),
                    tlsSwitch.IsToggled,
                    credentialsSwitch.IsToggled,
                    usernameEntry.Text.Trim(),
                    passwordEntry.Text.Trim());

                config.WriteConfigToFile(); //TODO: configure result to reflect success and check result

                connectButton.IsEnabled = true;
                MqttClientHelper.Config = config;
                statusMessage = "Configuration saved";
            }
        }
        else
        {
            config = new MqttConfigHelper(
                    ipEntry.Text.Trim(),
                    Convert.ToInt32(portEntry.Text.Trim()),
                    tlsSwitch.IsToggled,
                    credentialsSwitch.IsToggled);

            config.WriteConfigToFile(); //TODO: configure result to reflect success and check result

            connectButton.IsEnabled = true;
            MqttClientHelper.Config = config;
            statusMessage = "Configuration saved";
        }
        //Display a confirmation message to make it more user-friendly
        Toast.Make(statusMessage, ToastDuration.Short).Show();
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        string result;
        if (!MqttClientHelper.IsConnected)
        {
            result = await MqttClientHelper.AttemptBuildAndConnectClient(10);
        }
        else
        {
            result = await MqttClientHelper.AttemptDisconnectClient(10);
        }

        MqttClientHelper.SetConnectionLabelAndButton(connectionLabel, connectButton);

        Toast.Make(result, ToastDuration.Long).Show();
    }

    //Checks if the input IP address is correctly formatted using a regular expression
    private bool IsIpWithinRange()
    {
        bool condition = Regex.IsMatch(ipEntry.Text.Trim(), @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
        if (condition)
        {
            DisplayErrorMessage(brokerErrorLabel, false, string.Empty);
        }
        else
        {
            DisplayErrorMessage(brokerErrorLabel, true, "IP must be within range '255.255.255.255'");
        }
        return condition;
    }

    //Checks if the input port is a correct value within the valid TCP port range
    private bool IsPortWithinRange()
    {
        bool condition = portEntry.Text.Trim() != string.Empty;
        if (condition)
        {
            int portNumber = Convert.ToInt32(portEntry.Text.Trim());
            condition = portNumber >= 0 && portNumber <= 65535;
            if (condition)
            {
                DisplayErrorMessage(brokerErrorLabel, false, string.Empty);
                return condition;
            }
        }
        DisplayErrorMessage(brokerErrorLabel, true, "Port must be within range 0-65535");
        return condition;
    }

    private void DisplayErrorMessage(Label errorLabel, bool visibility, string message)
    {
        errorLabel.IsVisible = visibility;
        errorLabel.Text = message;
    }

    private void ToggleCredentialsVisibility(bool state)
    {
        credentialsSection.IsVisible = state;

        //Clear the entry fields
        usernameEntry.Text = string.Empty;
        passwordEntry.Text = string.Empty;
    }

    private bool IsCredentialEmpty(Entry entry)
    {
        bool condition = entry.Text.Trim() == string.Empty;
        if (condition)
        {
            DisplayErrorMessage(credentialsErrorLabel, true, $"{string.Concat(entry.Placeholder.Substring(0, 1).ToUpper(), entry.Placeholder.Substring(1))} cannot be empty when using credentials for authentication");
        }
        else
        {
            DisplayErrorMessage(credentialsErrorLabel, false, string.Empty);
        }
        return condition;
    }

    //A very ugly, but functioning way to unfocus all entry elements on the page
    private void UnfocusChildElements(VerticalStackLayout layoutPiece)
    {
        foreach (var item in layoutPiece.Children)
        {
            if (item.GetType() == typeof(HorizontalStackLayout))
            {
                foreach (var childItem in ((HorizontalStackLayout)item).Children)
                {
                    if (childItem.GetType() == typeof(Entry))
                    {
                        childItem.Unfocus();
                    }
                }
            }
        }
    }

    private async void ConfigurationChanged()
    {
        if (!initialLoad)
        {
            connectButton.IsEnabled = false;

            if (MqttClientHelper.IsConnected)
            {
                await MqttClientHelper.AttemptDisconnectClient(10);

                Toast.Make("Disconnected due to configuration change").Show();
            }

            MqttClientHelper.SetConnectionLabelAndButton(connectionLabel, connectButton);
        }
    }
}