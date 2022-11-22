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

        public RenderingTestView() {

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

            public CustomDrawOp(Rect bounds, RenderingTestView renderView) {
                Bounds = bounds;
                _renderView = renderView;
            }

            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;
            #endregion

            public List<(int x, int y)> verts = new List<(int x, int y)>() { (50, 40), (10, 10), (80, 0)};

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

                    foreach (var vert in verts) {
                        vertPaint.Color = new (255, 0, 0);

                        (int X, int Y) screenPos = _renderView.WorldToScreen(vert.x, vert.y);

                        canvas.DrawCircle((screenPos.X) + (float)Bounds.Width/2, (screenPos.Y) + (float)Bounds.Height / 2, 20, vertPaint);
                    }

                    canvas.Restore();
                    RenderTest(canvas);
                }
            }

            static Random rand = new Random();
            static int width = 600;
            static int height = 500;
            private void RenderTest(SKCanvas surface)
            {
                byte alpha = 128;
                var paint = new SKPaint();
                paint.StrokeWidth = 5;
                paint.IsAntialias = true;

                for (int i = 0; i < 1_0000; i++)
                {
                    float x1 = (float)(rand.NextDouble() * width);
                    float x2 = (float)(rand.NextDouble() * width);
                    float y1 = (float)(rand.NextDouble() * height);
                    float y2 = (float)(rand.NextDouble() * height);

                    paint.Color = new SKColor(
                        red: (byte)(rand.NextDouble() * 255),
                        green: (byte)(rand.NextDouble() * 255),
                        blue: (byte)(rand.NextDouble() * 255),
                        alpha: alpha
                    );

                    (int X1, int Y1) = _renderView.WorldToScreen(x1, y1);
                    (int X2, int Y2) = _renderView.WorldToScreen(x2, y2);

                    surface.DrawLine(X1, Y1, X2, Y2, paint);
                }
            }
        }

        // Runs each frame
        public override void Render(DrawingContext context)
        {
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), this));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
