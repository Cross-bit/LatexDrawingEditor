using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using LatexDrawingEditor.ViewModels.Documents;

namespace LatexDrawingEditor.ViewModels.Docks
{

    /// <summary>
    /// View model for canvases docking feature.
    /// </summary>
    internal class CanvasesDocumentDock : Dock.Model.ReactiveUI.Controls.DocumentDock
    {
        public CanvasesDocumentDock() {
            CreateDocument = ReactiveCommand.Create(CreateNewCanvas);
        }

        private void CreateNewCanvas() {
            if (!CanCreateDocument) {
                return;
            }

            var document = new CanvasViewModel() { };

            Factory?.AddDockable(this, document);
            Factory?.SetActiveDockable(document);
            Factory?.SetFocusedDockable(this, document);
        }


    }
}
