using CommunityToolkit.Maui.Views;
using DotNotes.Models;
using System.Collections.ObjectModel;

namespace DotNotes.Popups;

public partial class CheckBoxPrompt : Popup
{
	public ObservableCollection<Note> Notes;

    public CheckBoxPrompt(string title, ObservableCollection<Note> notes)
	{
		InitializeComponent();
		lblTitle.Text = title;

		Notes = notes;

		listViewContent.SetBinding(ItemsView.ItemsSourceProperty, "Note");
		listViewContent.ItemsSource = Notes;
	}

    private void btnTrue_Clicked(object sender, EventArgs e)
    {
		List<Note> checkedNotes = new List<Note>();
		foreach (Note note in Notes)
		{
			if (note.IsSelected)
			{
				checkedNotes.Add(note);
			}
		}
		Close(checkedNotes);
    }

    private void btnFalse_Clicked(object sender, EventArgs e)
    {
		Close(false);
    }
}