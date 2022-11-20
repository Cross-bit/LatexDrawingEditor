using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LatexDrawingEditor.ViewModels;
using LatexDrawingEditor.Views;
using LatexDrawingEditor.Styles.Themes;

namespace LatexDrawingEditor
{
    public partial class App : Application
    {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
            LoadTheme();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void LoadTheme()
        {
            Styles.Add(new DarkTheme());
        }
    }
}
