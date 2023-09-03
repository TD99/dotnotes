using CommunityToolkit.Maui.Views;
using DotNotes.Popups;
using DotNotes.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DotNotes;

public partial class AppShell : Shell
{

    public AppShell()
	{
        InitializeComponent();
    }

    private async void menuItemNotesList_Clicked(object sender, EventArgs e)
    {
        var popup = new AddMenu()
        {
            VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.End,
            HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Start,
            Size = new Size(this.CurrentPage.Width / 4, this.CurrentPage.Height + 4),
            Color = Color.FromRgba(0, 0, 0, 0.5)
        };
        await this.CurrentPage.ShowPopupAsync(popup);
        this.CurrentItem = this.menuItemInfo; /* BugFix */
        this.CurrentItem = this.menuItemHome;
    }
}
