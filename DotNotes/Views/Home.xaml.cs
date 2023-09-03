using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DotNotes.CustomElements;
using DotNotes.Models;
using DotNotes.Popups;
using DotNotes.Util;

namespace DotNotes.Views;

public partial class Home : ContentPage
{
    public static KeyValuePair<string, object> navigationOption = new KeyValuePair<string, object>();
    private Note currNote { get; set; }

    public Home()
    {
        InitializeComponent();
        updateFieldHeight(500);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        switch (navigationOption.Key)
        {
            case "openNote":
                Note note = (Note)navigationOption.Value;
                OpenNote(note);
                break;
            case "newNote":
                navigationOption = new KeyValuePair<string, object>();
                CreateNote();
                break;
            case "closeNote":
                currNote = null;
                CheckNoteOpened();
                break;
            default:
                break;
        }

        navigationOption = new KeyValuePair<string, object>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        updateLbl();
        CheckNoteOpened();
    }

    private async void SaveNote()
    {
        if (currNote == null)
        {
            await DisplayAlert("Notiz speichern", "Die Notiz konnte nicht gespeichert werden.", "OK");
            return;
        }

        Note note = new Note(9999, entryTitle.Text, new DateOnly(pickerDate.Date.Year, pickerDate.Date.Month, pickerDate.Date.Day), editorMain.Text, currNote.FileInfo);

        currNote = note;

        bool result = NotesIO.saveNote(note);
        if (!result)
        {
            await DisplayAlert("Notiz speichern", "Die Notiz konnte nicht gespeichert werden.", "OK");
        }
    }

    private async void CreateNote()
    {
        string name = await DisplayPromptAsync("Neue Notiz", "Name der Notiz", "OK", "Abbrechen");
        Note result = NotesIO.createNote(name);

        if (result is null)
        {
            await DisplayAlert("Neue Notiz", "Die Notiz konnte nicht erstellt werden.", "OK");
            return;
        }

        OpenNote(result);
    }

    private void OpenNote(Note note)
    {
        currNote = note;
        CheckNoteOpened();
        entryTitle.Text = note.Name;
        pickerDate.Date = new DateTime(note.Date.Year, note.Date.Month, note.Date.Day, 0, 0, 0);
        lblMain.Text = markdownToHtml(note.Body);
        editorMain.Text = note.Body;
        switchEditMode.IsToggled = true;
        updateFieldHeight(500);
    }

    private void CheckNoteOpened()
    {
        if (currNote == null)
        {
            editorLayout.IsVisible = false;
            newLayout.IsVisible = true;
        }
        else
        {
            editorLayout.IsVisible = true;
            newLayout.IsVisible = false;
            updateFieldHeight(500);
        }
    }

    /// <summary>
    /// This method adds / deletes markdown to / from the selected text of the mainEditor.
    /// </summary>
    /// <param name="startSymbol">Opening char</param>
    /// <param name="endSymbol">Closing char</param>
    private void AddMarkdown(char startSymbol, char endSymbol)
    {
        int selLen = editorMain.SelectionLength;
        int selStart = editorMain.CursorPosition;
        int selEnd = selStart + selLen;
        string content = editorMain.Text;

        if (selStart < selEnd) /* If TextSelection */
        {
            if (selStart > 0 && content[selStart - 1] == startSymbol && content[selEnd] == endSymbol) /* SmartSelect (if not selected but relevant) */
            {
                selLen += 2;
                selStart -= 1;
                selEnd = selStart + selLen;
            }

            string preStr = content.Substring(0, selStart);
            string selStr = content.Substring(selStart, selLen);
            string endStr = content.Substring(selEnd);

            /* Reset pos to trigger MAUI-Event, BugFix of MAUI */
            editorMain.CursorPosition = (selStart > 0)?0:1;
            editorMain.SelectionLength = 0;

            if (selStr.First() == startSymbol && selStr.Last() == endSymbol)
            {
                editorMain.Text = $"{preStr}{selStr.Substring(1, selStr.Length - 2)}{endStr}";
                editorMain.CursorPosition = selStart;
                editorMain.SelectionLength = selLen - 2;
            }
            else
            {
                editorMain.Text = $"{preStr}{startSymbol}{selStr}{endSymbol}{endStr}";
                editorMain.CursorPosition = selStart;
                editorMain.SelectionLength = selLen + 2;
            }
        }
    }

    /// <summary>
    /// HTML-opening/-closing tag and the HTML-ALT code with the given markdown-declaring char
    /// </summary>
    private Dictionary<char, List<string>> markdownSyntaxes = new Dictionary<char, List<string>>
    {
        { '*', new List<string> { "<b>", "</b>", "&#42;" } },
        { '~', new List<string> { "<i>", "</i>", "&#126;" } },
        { '_', new List<string> { "<u>", "</u>", "&#95;" } },
        { '§', new List<string> { "<u><b>", "</b></u>", "&#167;" } }
    };

    /// <summary>
    /// Replacements for illegal chars
    /// </summary>
    private Dictionary<string, string> illegalCharReplacements = new Dictionary<string, string> {
        { "&", "&#38;" },
        { "<", "&#60;" },
        { ">", "&#62;" }
    };

    /// <summary>
    /// Converts Markdown to HTML (+ Validation)
    /// </summary>
    /// <param name="input">Markdown text</param>
    /// <returns>HTML text</returns>
    private string markdownToHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Die Notiz ist leer." + input; /* Converter and validation not necessary */

        foreach (string s in illegalCharReplacements.Keys) /* Chars that will crash the program */
        {
            input = input.Replace(s, illegalCharReplacements[s]);
        }

        string output = ""; /* Output HTML string */
        char[] inputChars = input.ToCharArray();

        Dictionary<int, string> closingTags = new Dictionary<int, string>(); /* Contains the HTML closing Tags */

        for (int i = 0; i < inputChars.Length; i++)
        {
            char openingChar = inputChars[i]; /* Possible style-declaring char */

            if (inputChars[i] != '\\')
            {
                if (inputChars[i] == '\r' )
                {
                    output += "<br />";
                }
                else
                {
                    if (markdownSyntaxes.ContainsKey(openingChar) && (i < 1 || inputChars[i - 1] != '\\')) /* If style-declaring char and not escaped */
                    {
                        if (closingTags.ContainsKey(i)) /* If style-closing char */
                        {
                            output += closingTags[i];
                            closingTags.Remove(i);
                        }
                        else /* If style-opening char */
                        {
                            bool isClosed = false; /* Style-declaration is closed */

                            for (int j = i + 1; j < inputChars.Length; j++) /* Check for style-closing char */
                            {
                                char closingChar = inputChars[j]; /* Possible style-closing char */
                                if (closingTags.ContainsKey(j)) /* VALIDATION: If illegal nesting, overlapping of HTML Tags */
                                {
                                    isClosed = false;
                                    break;
                                }
                                if (closingChar == openingChar) /* Style-closing char */
                                {
                                    if (j > 0 && inputChars[j - 1] == '\\') /* Last check for illegal HTML, escaped chars */
                                    {
                                        isClosed = false;
                                        break;
                                    }
                                    isClosed = true;
                                    closingTags.Add(j, markdownSyntaxes[openingChar][1]); /* Add style-closing tag to the queue */
                                    break;
                                }
                            }

                            output += markdownSyntaxes[openingChar][(isClosed)?0:2]; /* Opened (and closed) */
                        }
                    }
                    else
                    {
                        output += openingChar;
                    }
                }

            }
            else if (inputChars.Length - 1 <= i || !markdownSyntaxes.ContainsKey(inputChars[i + 1])) /* Backslash used for escaping */
            {
                output += '\\';
            }
        }
        return output;
    }

    /// <summary>
    /// Updates the height of the lblMain and editorMain.
    /// </summary>
    /// <param name="delay">Wait for X milliseconds</param>
    private async void updateFieldHeight(int delay = 0)
    {
        await Task.Delay(delay);
        double calcHeight = pageHome.Height - gridHeader.Height - pageHome.Padding.Top - (2 * pageHome.Padding.Bottom);
        editorMain.HeightRequest = calcHeight;
        scrollViewLblMain.HeightRequest = calcHeight;
    }

    private void updateLbl()
    {
        lblMain.Text = markdownToHtml(editorMain.Text);
        int fontSize = Preferences.Get("editorSize", 15);
        editorMain.FontSize = fontSize;
        lblMain.FontSize = fontSize;
    }

    private void markdownButton_Clicked(object sender, EventArgs e)
    {
        MarkdownControlItem item = (MarkdownControlItem)sender;
        AddMarkdown(item.Char, item.Char);
    }

    private void editorMain_TextChanged(object sender, TextChangedEventArgs e)
    {
        updateLbl();
        SaveNote();
    }

    private void pageHome_SizeChanged(object sender, EventArgs e)
    {
        updateFieldHeight();
    }

    private void pickerDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        SaveNote();
    }

    private void btnCreNote_Clicked(object sender, EventArgs e)
    {
        CreateNote();
    }

    private void entryTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveNote();
    }
}