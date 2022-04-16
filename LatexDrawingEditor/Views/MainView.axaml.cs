using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LatexDrawingEditor.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
