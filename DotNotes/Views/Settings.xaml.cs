using CommunityToolkit.Maui.Views;
using DotNotes.Models;
using DotNotes.Popups;
using DotNotes.Util;
using System.Collections.ObjectModel;

namespace DotNotes.Views;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
        stepperFontSize.Value = Preferences.Get("editorSize", 15);
	}

    private async void btnDelete_Clicked(object sender, EventArgs e)
    {
        List<Note> notes = NotesIO.loadNotes();

        if (notes.Count == 0)
        {
            await DisplayAlert("Löschen", "Es wurden keine Notizen gefunden!", "OK");
            return;
        }

        var popup = new CheckBoxPrompt("Notizen löschen", new ObservableCollection<Note>(NotesIO.loadNotes()))
        {
            VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center,
            HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center,
            Size = new Size(300, 300),
            Color = Color.FromArgb("#F9F9F9")
        };

        var selectedNotesObj = await this.ShowPopupAsync(popup);
        if (selectedNotesObj is not List<Note>) return;

        List<Note> selectedNotes = (List<Note>) selectedNotesObj;
        if (selectedNotes.Count < 1) return;

        bool lastChance = await DisplayAlert("Löschen", "Möchtest du die ausgewählte(n) Notiz(en) wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden!", "Löschen", "Abbrechen");

        if (lastChance)
        {
            bool success = NotesIO.deleteNotes(selectedNotes);
            await DisplayAlert("Löschen", success ? "Die ausgewählte(n) Notiz(en) wurden erfolgreich gelöscht!" : "Beim Löschen der ausgewählte(n) Notiz(en) ist mindestens ein Fehler aufgetreten.", "OK");
        }
    }

    private async void btnExport_Clicked(object sender, EventArgs e)
    {
        List<Note> notes = NotesIO.loadNotes();

        if (notes.Count == 0)
        {
            await DisplayAlert("Exportieren", "Es wurden keine Notizen gefunden!", "OK");
            return;
        }

        var popup = new CheckBoxPrompt("Notizen exportieren", new ObservableCollection<Note>(NotesIO.loadNotes()))
        {
            VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center,
            HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center,
            Size = new Size(300, 300),
            Color = Color.FromArgb("#F9F9F9")
        };

        var selectedNotesObj = await this.ShowPopupAsync(popup);
        if (selectedNotesObj is not List<Note>) return;

        List<Note> selectedNotes = (List<Note>)selectedNotesObj;
        if (selectedNotes.Count < 1) return;

        var resultObj = NotesIO.shareNotes(selectedNotes);
        if (resultObj is not string)
        {
            await DisplayAlert("Exportieren", "Beim Exportieren der ausgewählte(n) Notiz(en) ist mindestens ein Fehler aufgetreten.", "OK");
            return;
        }
        string result = (string) resultObj;

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Notizen-Paket freigeben",
            File = new ShareFile(result.Replace("/", "\\"))
        });
    }

    private async void btnImport_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Notizen-Paket importieren"
            });

            if (result == null) return;
            if (!result.FileName.EndsWith("zip"))
            {
                await DisplayAlert("Importieren", "Erlaubt sind nur .zip-Dateien!", "OK");
                return;
            }

            bool importResult = NotesIO.importNotes(result.FullPath);

            await DisplayAlert("Importieren", importResult ? "Die ausgewählte(n) Notiz(en) wurden erfolgreich importiert!" : "Die ausgewählte(n) Notiz(en) wurden teilweise / nicht erfolgreich importiert! (Tipp: Nur .xml-Dateien sind erlaubt und die Dateien müssen valid sein.)", "OK");
        }
        catch
        {
            await DisplayAlert("Importieren", "Beim Importieren der ausgewählte(n) Notiz(en) ist mindestens ein Fehler aufgetreten.", "OK");
            return;
        }
    }

    private async void btnFolderOpen_Clicked(object sender, EventArgs e)
    {
        string notesFolder = $"{FileSystem.Current.AppDataDirectory}/NoteStorage";
        if (!Directory.Exists(notesFolder)) Directory.CreateDirectory(notesFolder);
        await Browser.Default.OpenAsync(notesFolder);
    }

    private void stepperFontSize_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        int value = Convert.ToInt32(stepperFontSize.Value);
        Preferences.Set("editorSize", value);
    }
}