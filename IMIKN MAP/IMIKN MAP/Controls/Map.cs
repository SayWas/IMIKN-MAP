using System;
using System.IO;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace IMIKN_MAP.Controls
{
    class Map : Frame
    {
        private GestureParameters gestureParameters;

        private readonly SKCanvasView _canvasView = new SKCanvasView();
        private SKPicture[] _svgPictures;
        private int current_floor;

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: SourceChanged);
        public static readonly BindableProperty FloorsProperty = BindableProperty.Create(
            nameof(Floors), typeof(int), typeof(Map), default(int), propertyChanged: FloorsChanged);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        public int Floors
        {
            get => (int)GetValue(FloorsProperty);
            set => SetValue(FloorsProperty, value);
        }
        public int CurrentFloor
        {
            get { return current_floor; }
            set { current_floor = value; }
        }

        public Map()
        {
            CurrentFloor = 1;
            Padding = new Thickness(0);
            BackgroundColor = Color.Transparent;
            HasShadow = false;
            Content = _canvasView;
            _canvasView.PaintSurface += CanvasViewOnPaintSurface;
            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGesture);
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

        private static void FloorsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Map map = bindable as Map;
            map._svgPictures = new SKPicture[map.Floors];
            if (string.IsNullOrEmpty(map.Source))
                return;

            map.LoadSvgPicture();
            map._canvasView.InvalidateSurface();
        }
        private static void SourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            Map map = bindable as Map;
            if (map._svgPictures == null)
                return;

            map?.LoadSvgPicture();
            map?._canvasView.InvalidateSurface();
        }
        private void LoadSvgPicture()
        {
            if (_svgPictures == null)
                return;

            for (int i = 0; i < Floors; i++)
            {
                using (Stream stream = GetType().Assembly.GetManifestResourceStream("IMIKN_MAP.Resources." + Source + ".Floor" + (i + 1) + ".svg"))
                {
                    SKSvg svg = new SKSvg();
                    svg.Load(stream);

                    _svgPictures[i] = svg.Picture;
                }
            }

            gestureParameters = new GestureParameters(_svgPictures[0].CullRect.Width, _svgPictures[0].CullRect.Height);
        }
        private void CanvasViewOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (string.IsNullOrEmpty(Source))
                return;

            if (_svgPictures == null)
                return;

            int currentFloor = CurrentFloor - 1;
            SKImageInfo info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            SKRect bounds = _svgPictures[currentFloor].CullRect;

            float ratio = info.Height - bounds.Height < info.Width - bounds.Width
                ? info.Height / bounds.Height
                : info.Width / bounds.Width;

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);
            canvas.Translate((float)gestureParameters.TranslationScale[0], (float)gestureParameters.TranslationScale[1]);
            canvas.Translate((float)gestureParameters.TranslationMove[0], (float)gestureParameters.TranslationMove[1]);
            canvas.Scale((float)gestureParameters.Scale);

            canvas.DrawPicture(_svgPictures[currentFloor]);
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    gestureParameters.StartScale = gestureParameters.Scale;
                    gestureParameters.Offset[0] += gestureParameters.Coordinates[0];
                    gestureParameters.Offset[1] += gestureParameters.Coordinates[1];
                    gestureParameters.TranslationMove[0] = 0;
                    gestureParameters.TranslationMove[1] = 0;
                    break;

                case GestureStatus.Running:
                    gestureParameters.CurrentScale += (e.Scale - 1) * gestureParameters.StartScale;
                    gestureParameters.CurrentScale = Math.Max(1, gestureParameters.CurrentScale);

                    double renderedX = Content.X + gestureParameters.Offset[0];
                    double deltaX = renderedX / gestureParameters.Size[0];
                    double deltaWidth = gestureParameters.Size[0] / (gestureParameters.Size[0] * gestureParameters.StartScale);
                    double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                    double renderedY = Content.Y + gestureParameters.Offset[1];
                    double deltaY = renderedY / gestureParameters.Size[1];
                    double deltaHeight = gestureParameters.Size[1] / (gestureParameters.Size[1] * gestureParameters.StartScale);
                    double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                    double targetX = gestureParameters.Offset[0] - (originX * gestureParameters.Size[0]) * (gestureParameters.CurrentScale - gestureParameters.StartScale);
                    double targetY = gestureParameters.Offset[1] - (originY * gestureParameters.Size[1]) * (gestureParameters.CurrentScale - gestureParameters.StartScale);

                    gestureParameters.TranslationScale[0] = targetX.Clamp(-gestureParameters.Size[0] * (gestureParameters.CurrentScale - 1), 0);
                    gestureParameters.TranslationScale[1] = targetY.Clamp(-gestureParameters.Size[1] * (gestureParameters.CurrentScale - 1), 0);

                    gestureParameters.Scale = gestureParameters.CurrentScale;
                    _canvasView.InvalidateSurface();
                    break;

                case GestureStatus.Completed:
                    gestureParameters.Offset[0] = gestureParameters.TranslationScale[0];
                    gestureParameters.Offset[1] = gestureParameters.TranslationScale[1];
                    gestureParameters.Coordinates[0] = 0;
                    gestureParameters.Coordinates[1] = 0;
                    gestureParameters.WasScaled = true;
                    break;
            }
        }
        void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if (gestureParameters.WasScaled)
                        break;

                    gestureParameters.TranslationMove[0] = gestureParameters.Coordinates[0] + (e.TotalX * 2.2);
                    gestureParameters.TranslationMove[1] = gestureParameters.Coordinates[1] + (e.TotalY * 2.2);
                    gestureParameters.TranslationMove[0] = gestureParameters.TranslationMove[0].Clamp(-(gestureParameters.Size[0] * gestureParameters.Scale - Math.Abs(gestureParameters.Offset[0]) - gestureParameters.Size[0]), Math.Abs(gestureParameters.Offset[0]));
                    gestureParameters.TranslationMove[1] = gestureParameters.TranslationMove[1].Clamp(-(gestureParameters.Size[1] * gestureParameters.Scale - Math.Abs(gestureParameters.Offset[1]) - gestureParameters.Size[1]), Math.Abs(gestureParameters.Offset[1]));

                    _canvasView.InvalidateSurface();
                    break;

                case GestureStatus.Completed:
                    gestureParameters.Coordinates[0] = gestureParameters.TranslationMove[0];
                    gestureParameters.Coordinates[1] = gestureParameters.TranslationMove[1];
                    gestureParameters.WasScaled = false;
                    break;
            }
        }

        private struct GestureParameters
        {
            public GestureParameters(double Width, double Height)
            {
                StartScale = CurrentScale = Scale = 1;
                Offset = new double[2] { 0, 0 };
                Coordinates = new double[2] { 0, 0 };
                TranslationScale = new double[2] { 0, 0 };
                TranslationMove = new double[2] { 0, 0 };
                Size = new double[2] { Width, Height };
                WasScaled = false;
            }
            public bool WasScaled { get; set; }
            public double StartScale { get; set; }
            public double CurrentScale { get; set; }
            public double[] Offset { get; set; }
            public double[] Coordinates { get; set; }
            public double[] Size { get; set; }
            public double Scale { get; set; }
            public double[] TranslationScale { get; set; }
            public double[] TranslationMove { get; set; }
        }
    }
}