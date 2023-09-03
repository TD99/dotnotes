using CommunityToolkit.Maui.Views;
using DotNotes.Models;
using DotNotes.Util;
using DotNotes.Views;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;

namespace DotNotes.Popups;

public partial class AddMenu : Popup
{
    public ObservableCollection<Note> Notes;

    public AddMenu()
	{
		InitializeComponent();
		loadNotes();
	}

	private void loadNotes()
	{
        Notes = new ObservableCollection<Note>(NotesIO.loadNotes());
        listViewContent.SetBinding(ItemsView.ItemsSourceProperty, "Note");
        listViewContent.ItemsSource = Notes;
    }

    private async void btnAdd_Clicked(object sender, EventArgs e)
    {
        Home.navigationOption = new KeyValuePair<string, object>("newNote", null);
        await Shell.Current.GoToAsync("//pageInfo"); /* BugFix */
        await Shell.Current.GoToAsync("//pageHome");
    }

    private async void listViewContent_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        Note note = (Note)listViewContent.SelectedItem;

        Home.navigationOption = new KeyValuePair<string, object>("openNote", note);
        await Shell.Current.GoToAsync("//pageInfo"); /* BugFix */
        await Shell.Current.GoToAsync("//pageHome");
    }
}