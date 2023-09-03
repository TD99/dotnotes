using System.Reflection;

namespace DotNotes.Views;

public partial class Info : ContentPage
{
    public static bool isNavigated = false;
    public Info()
	{
		InitializeComponent();
        setContentText();
    }

    private async void btnReview_Clicked(object sender, EventArgs e)
    {
        isNavigated = true;
        await Shell.Current.GoToAsync("Review");
    }

    private void setContentText()
    {
        lblVersion.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        lblName.Text = $"Name: {AppInfo.Current.Name}";
        lblNamespaceName.Text = $"Namespace: {nameof(DotNotes)}" ;
    }

    private async void btnIntro_Clicked(object sender, EventArgs e)
    {
        Home.navigationOption = new KeyValuePair<string, object>("closeNote", null);
        await Shell.Current.GoToAsync("//pageHome");
    }
}