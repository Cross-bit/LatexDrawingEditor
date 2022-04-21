using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LatexDrawingEditor.Views.Documents
{
    public partial class CanvasView : UserControl
    {
        public CanvasView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
