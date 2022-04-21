using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;
using LatexDrawingEditor.ViewModels.Documents;


namespace LatexDrawingEditor.ViewModels
{
    public class EditorWindowFactory : Dock.Model.ReactiveUI.Factory
    {
        private IRootDock? _rootDock;
        private IDocumentDock? _documentDock;

        public override IRootDock CreateLayout()
        {

            var canvasViewModel = new CanvasViewModel();


            // the main root dock 
            var rootDock = CreateRootDock();



            var windowLayout = CreateRootDock();
            windowLayout.Title = "Default";


            rootDock.IsCollapsable = true;
            rootDock.ActiveDockable = windowLayout;
            rootDock.DefaultDockable = windowLayout;
            rootDock.VisibleDockables = CreateList<IDockable>(windowLayout);

            return rootDock;
        }


    }
}
