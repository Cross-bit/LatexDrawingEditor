using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using System.Collections.Generic;
using SkiaSharp;

namespace LatexDrawingEditor.Views
{
    public partial class RenderingTestView : UserControl
    {

            #region Mouse movement handling 
            // todo: probably move somewhere else?
            double _mouseX = 0;
            double _mouseY = 0;
            bool _isMouseHeld = false;

            double MouseX => _mouseX; 
            double MouseY => _mouseY;
            bool IsMouseHeld => _isMouseHeld;

            #endregion

            #region Panning offsets

            double _offsetX = 0;
            double _offsetY = 0;
            double _startPanX = 0;
            double _startPanY = 0;

        #endregion
            

            // Avalonia property registration
            public static readonly StyledProperty<List<(int x, int y)>> VerteciesProperty =
            AvaloniaProperty.Register<RenderingTestView, List<(int x, int y)>>(nameof(Background), new List<(int x, int y)>());

            public List<(int x, int y)> Vertecies { get => GetValue(VerteciesProperty); set => GetValue(VerteciesProperty); }

            public RenderingTestView() {

            var textBlock = new TextBlock();
            var text = textBlock.GetObservable(TextBlock.TextProperty);
            text.Subscribe(value => Console.WriteLine(value + " Changed"));

            ClipToBounds = true;

            this.PointerMoved += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                _mouseX = point.Position.X;
                _mouseY = point.Position.Y;
                if (_isMouseHeld) {
                    _offsetX -= (_mouseX - _startPanX);// move only of new offset set by mouseX/mouseY
                    _offsetY -= (_mouseY - _startPanY);

                    _startPanX = _mouseX; // reset initial mouse offset each frame
                    _startPanY = _mouseY;
                }
            };

            this.PointerPressed += (sender, args) => {
                var point = args.GetCurrentPoint(this);
                if (point.Properties.IsLeftButtonPressed) {
                    _startPanX = point.Position.X;
                    _startPanY = point.Position.Y;
                    _isMouseHeld = true;
                }
            };

            this.PointerReleased += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                if (point.Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.LeftButtonReleased) {
                    _isMouseHeld = false;
                }
            };

            InitializeComponent();
        }

        public (int x, int y) WorldToScreen(float fworldX, float fworldY) {
            return ((int)(fworldX - _offsetX), (int)(fworldY - _offsetY));
        }

        public (double x, double y) ScreenToWorld(int fscreenX, int fscreenY) {
            return ((fscreenX + _offsetX), (fscreenY + _offsetY));
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            #region Other
            public void Dispose() { /*No-op*/ }

            public Rect Bounds { get; }

            private RenderingTestView _renderView;

            private List<(int x, int y)> _vertsToDraw = new List<(int x, int y)>();

            public CustomDrawOp(Rect bounds, RenderingTestView renderView, List<(int x, int y)> vertsToDraw) {
                Bounds = bounds;
                _renderView = renderView;
                _vertsToDraw = vertsToDraw;
            }

            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;
            #endregion


            private void NoSkiaFallback(IDrawingContextImpl context) {
                Dispatcher.UIThread.Post(() => {
                    using (var tmpCtx = new DrawingContext(context, false))
                    {
                        var noSkia = new FormattedText("Current rendering API is not Skia", Typeface.Default, 20, TextAlignment.Center,
                        TextWrapping.Wrap, new Size(Bounds.Width, 200));
                        // todo: fix explodes on window resize, maybe remove completely
                        tmpCtx.DrawText(new SolidColorBrush(Colors.Red), new Point(100, 10), noSkia);
                    }
                });
            }

            public void Render(IDrawingContextImpl context) {

                SKCanvas? canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

                if (canvas == null)
                    NoSkiaFallback(context);
                else
                {
                    canvas.Save();

                    using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0))
                    using (var paint = new SKPaint { Shader = shader })

                    canvas.DrawPaint(paint);

                    var vertPaint = new SKPaint();
                    vertPaint.StrokeWidth = 5;
                    vertPaint.IsAntialias = true;

                    foreach (var vert in _vertsToDraw) {
                        vertPaint.Color = new (255, 0, 0);

                        (int X, int Y) screenPos = _renderView.WorldToScreen(vert.x, vert.y);

                        canvas.DrawCircle((screenPos.X) + (float)Bounds.Width/2, (screenPos.Y) + (float)Bounds.Height / 2, 20, vertPaint);
                    }

                    canvas.Restore();
                }
            }
        }

        // Runs each frame
        public override void Render(DrawingContext context)
        {
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), this, Vertecies));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
