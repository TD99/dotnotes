using System.Diagnostics;
using System.Reflection;

namespace DotNotes.Views;

public partial class Review : ContentPage
{
    public Review()
    {
        InitializeComponent();
        InpFunction.SelectedIndex = 0;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        if (Info.isNavigated)
        {
            Info.isNavigated = false;
        }
        else
        {
            Shell.Current.SendBackButtonPressed();
        }
        base.OnNavigatedTo(args);
    }

    private void InpStars_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        InpStars.Value = Math.Round(e.NewValue);
    }


    private async void InpSend_Clicked(object sender, EventArgs e)
    {
        bool isValid = ValidateFields();
        if (!isValid)
        {
            await DisplayAlert("Fehler bei der Datenüberprüfung", "Pflichtfelder (*) müssen zwingend ausgefüllt werden und die Daten müssen im glütigen Wertebereich liegen.", "OK");
            return;
        }

        if (Email.Default.IsComposeSupported)
        {
            string subject = $"Rezension zu {AppInfo.Current.Name} ({nameof(DotNotes)}) v.{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            string classText = (InpFunction.SelectedIndex == 0) ? $", Klasse: {InpClass.Text}" : "";
            string body = $"Rezension zu {AppInfo.Current.Name}\nRezensent*in: {InpName.Text} ({InpFunction.SelectedItem} bei {InpSchool.Text}{classText})\nBewertung: {InpStars.Value}/{InpStars.Maximum} Sterne\n\nKommentar:\n{InpComment.Text}\n\nVielen Dank für dein ehrliches Feedback!\n\n{AppInfo.Current.Name} ({nameof(DotNotes)}) ist ein Produkt der School Solutions AG.";
            string[] recipients = new[] { "info@schoolsolutions.ch" };

            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string>(recipients)
            };

            await Email.Default.ComposeAsync(message);
        }
    }

    private bool ValidateFields()
    {
        if (InpFunction.SelectedItem == null) return false;
        if (string.IsNullOrEmpty(InpSchool.Text)) return false;
        if (string.IsNullOrEmpty(InpName.Text)) return false;
        if (InpStars.Value < 1 || InpStars.Value > 5) return false;

        return true;
    }
}