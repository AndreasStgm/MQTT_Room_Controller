using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using System.Text.RegularExpressions;

namespace RoomControllerApp;

public partial class MqttConfigPage : ContentPage
{
    public MqttConfigPage()
    {
        InitializeComponent();

        ipEntry.TextChanged += IpEntry_TextChanged;
        portEntry.TextChanged += PortEntry_TextChanged;
        credentialsSwitch.Toggled += CredentialsSwitch_Toggled;
        usernameEntry.TextChanged += CredentialEntry_TextChanged;
        passwordEntry.TextChanged += CredentialEntry_TextChanged;
        saveButton.Clicked += SaveButton_Clicked;

        ToggleCredentialsVisibility(false);
    }

    private void IpEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        IsIpWithinRange();
    }

    private void PortEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        IsPortWithinRange();
    }

    private void CredentialsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
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
        Entry entry = (Entry)sender;
        IsCredentialEmpty(entry);
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        UnfocusChildElements(fullPage);
        UnfocusChildElements(credentialsSection);
        ipEntry.HideKeyboardAsync(CancellationToken.None); //Apparently hiding the keyboard for one entry that isn't even in focus hides the keyboard successfully

        string toastMessage = string.Empty;

        if (!IsIpWithinRange())
        {
            toastMessage = "Incorrect IP configuration";
            ipEntry.Focus();
        }
        else if (!IsPortWithinRange())
        {
            toastMessage = "Incorrect port configuration";
            portEntry.Focus();
        }
        else if (credentialsSwitch.IsToggled)
        {
            if (IsCredentialEmpty(usernameEntry))
            {
                toastMessage = "Incorrect username configuration";
                usernameEntry.Focus();
            }
            else if (IsCredentialEmpty(passwordEntry))
            {
                passwordEntry.Focus();
                toastMessage = "Incorrect password configuration";
            }
            else
            {
                toastMessage = "Configuration Saved";
                DisplayErrorMessage(credentialsErrorLabel, false, string.Empty);
            }
        }
        else
        {
            toastMessage = "Configuration Saved";
        }
        //Display a confirmation message to make it more user-friendly
        Toast.Make(toastMessage, ToastDuration.Short, 14).Show();
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
}