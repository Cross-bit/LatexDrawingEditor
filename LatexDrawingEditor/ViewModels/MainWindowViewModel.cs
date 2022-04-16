using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace LatexDrawingEditor.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IFactory? _factory;
        private IRootDock? _layout;

        public IRootDock? Layout {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public MainWindowViewModel() {
            
        }
    }
}
