using Avalonia;
using Avalonia.Controls;


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
        }
    }
}
