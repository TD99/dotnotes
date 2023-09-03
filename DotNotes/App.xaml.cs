using DotNotes.Views;

namespace DotNotes;

public partial class App : Application
{
    private string title = AppInfo.Current.Name;
    private const int WIDTH = 1080;
    private const int HEIGHT = 720;
#if WINDOWS
    private const int MINWIDTH = 640;
    private const int MINHEIGHT = 480;
#endif

    public App()
	{
        InitializeComponent();
		MainPage = new AppShell();
        RegisterRoutes();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);

        if (window == null) return null;

        window.Title = title;
        window.Width = WIDTH;
        window.Height = HEIGHT;
#if WINDOWS
        window.MinimumWidth = MINWIDTH;
        window.MinimumHeight = MINHEIGHT;
#endif

        return window;
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute("Review", typeof(Review));
    }
}
