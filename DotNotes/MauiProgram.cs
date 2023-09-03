using CommunityToolkit.Maui;
using DotNotes.CustomElements;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace DotNotes;

public static class MauiProgram
{
    private static string[] fontNames = {
        "OpenSans-Regular", "OpenSans-Semibold", "Roboto-Black", "Roboto-BlackItalic", "Roboto-Bold",
        "Roboto-BoldItalic", "Roboto-Italic", "Roboto-Light", "Roboto-LightItalic", "Roboto-Medium",
        "Roboto-MediumItalic", "Roboto-Regular", "Roboto-Thin", "Roboto-ThinItalic"
    };
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
                foreach (string fontName in fontNames)
                {
                    fonts.AddFont(fontName + ".ttf", fontName);
                }
            })
            .ConfigureMauiHandlers(h =>
            {
                h.AddHandler<MarkdownControlItem, MenuFlyoutItemHandler>();
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
