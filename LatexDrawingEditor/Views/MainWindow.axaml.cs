using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Dock.Avalonia;

namespace LatexDrawingEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
#if DEBUG
            this.AttachDevTools(); // F12 while debug
#endif

            DockProperties.SetIsDragEnabled(this, true);
            DockProperties.SetIsDropEnabled(this, true);
        }

        public override void Render(DrawingContext context)
        {
        }
    }
}
