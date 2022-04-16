using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LatexDrawingEditor.Views
{
    public partial class ToolStrip : UserControl
    {
        public ToolStrip()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
