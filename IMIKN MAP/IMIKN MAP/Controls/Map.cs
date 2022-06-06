using IMIKN_MAP.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using IMIKN_MAP.Services;
using System.Collections.Generic;

namespace IMIKN_MAP.Controls
{
    class Map : Frame
    {
        static Map Current;
        private GestureParameters gestureParameters;

        private readonly SKCanvasView _canvasView = new SKCanvasView();

        private Graph graph;
        private SKPicture[] _svgPictures;
        private List<List<SKPoint[]>> pathpoints;

        #region BindableProterties
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: SourceChanged);
        public static readonly BindableProperty FloorsProperty = BindableProperty.Create(
            nameof(Floors), typeof(int), typeof(Map), default(int), propertyChanged: FloorsChanged);
        public static readonly BindableProperty ScaleValueProperty = BindableProperty.Create(
            nameof(ScaleValue), typeof(double), typeof(Map), default(double), propertyChanged: ScaleValueChanged);
        public static readonly BindableProperty PathIdsProperty = BindableProperty.Create(
            nameof(PathIds), typeof(string[]), typeof(Map), null, propertyChanged: PathIdsChanged);
        public static readonly BindableProperty CurrentFloorProperty = BindableProperty.Create(
            nameof(CurrentFloor), typeof(int), typeof(Map), default(int), propertyChanged: CurrentFloorChanged);
        public static readonly BindableProperty OffsetXProperty = BindableProperty.Create(
            nameof(OffsetX), typeof(double), typeof(Map), default(double));
        public static readonly BindableProperty OffsetYProperty = BindableProperty.Create(
            nameof(OffsetY), typeof(double), typeof(Map), default(double));
        public static readonly BindableProperty OriginScaleProperty = BindableProperty.Create(
            nameof(OriginScale), typeof(double), typeof(Map), default(double));
        public static readonly BindableProperty CurrentScaleProperty = BindableProperty.Create(
            nameof(CurrentScale), typeof(double), typeof(Map), default(double));
        public static readonly BindableProperty OriginYOffsetProperty = BindableProperty.Create(
            nameof(OriginYOffset), typeof(double), typeof(Map), default(double));

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
            get => (int)GetValue(CurrentFloorProperty);
            set => SetValue(CurrentFloorProperty, value);
        }
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        public string[] PathIds
        {
            get => (string[])GetValue(PathIdsProperty);
            set => SetValue(PathIdsProperty, value);
        }
        public double OffsetX {
            get => (double)GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }
        public double OffsetY
        {
            get => (double)GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }
        public double OriginScale
        {
            get => (double)GetValue(OriginScaleProperty);
            set => SetValue(OriginScaleProperty, value);
        }
        public double CurrentScale
        {
            get => (double)GetValue(CurrentScaleProperty);
            set => SetValue(CurrentScaleProperty, value);
        }
        public double OriginYOffset
        {
            get => (double)GetValue(OriginYOffsetProperty);
            set => SetValue(OriginYOffsetProperty, value);
        }
        #endregion

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
            Current = this;
        }

        private static void FloorsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Map map = bindable as Map;
            map._svgPictures = new SKPicture[map.Floors];
            if (string.IsNullOrEmpty(map.Source))
                return;

            map.pathpoints = new List<List<SKPoint[]>>();
            for (int i = 0; i < map.Floors; i++)
                map.pathpoints.Add(new List<SKPoint[]>());
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
        private static void ScaleValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Map map = bindable as Map;
            if (map._svgPictures == null || map.ScaleValue == 0)
                return;
            map.OnPinchUpdated(map, new PinchGestureUpdatedEventArgs(GestureStatus.Started,(double)newValue, new Point(0.5, 0.5)));
            map.OnPinchUpdated(map, new PinchGestureUpdatedEventArgs(GestureStatus.Running, (double)newValue, new Point(0.5, 0.5)));
            map.OnPinchUpdated(map, new PinchGestureUpdatedEventArgs(GestureStatus.Completed, (double)newValue, new Point(0.5, 0.5)));
            map.gestureParameters.WasScaled = false;
        }
        private async static void PathIdsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Map map = bindable as Map;
            foreach (var item in map.pathpoints)
                item.Clear();
            if (map.graph == null)
                map.graph = new Graph((string)App.Current.Properties["Dots"]);
            try
            {
                if (map.PathIds[0] == "Мое местоположение")
                {
                    double[] location = await LocationTools.GetCurrentLocation();
                    map.graph.AddDot((float)location[0], (float)location[1], map.CurrentFloor);
                }
            }
            catch { return; }
                Dot[] path = map.graph.GetPath(map.PathIds[0], map.PathIds[1]);
            int i = 1, j = 0;
            for (; i < path.Length; i++)
            {
                while (path[i].Floor == path[i - 1].Floor && i < path.Length - 1)
                    i++;
                if (i == path.Length - 1) i++;

                SKPoint[] points = new SKPoint[i - j];
                for (int q = 0; q < i - j; q++)
                {
                    points[q] = new SKPoint((float)path[j + q].X, (float)path[j + q].Y);
                }
                if (points.Length != 1)
                    map.pathpoints[path[i - 1].Floor - 1].Add(points);
                j = i;
                map.CurrentFloor = path[path.Length - 1].Floor;
                map.ScaleValue = 0.0000001;
                map.ScaleValue = 0;
            }
        }
        private static void CurrentFloorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Map map = bindable as Map;
            map._canvasView.InvalidateSurface();
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

            SKImageInfo info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);
            SKRect bounds = _svgPictures[CurrentFloor - 1].CullRect;
            float ratio = info.Height - bounds.Height < info.Width - bounds.Width ? info.Height / bounds.Height : info.Width / bounds.Width;
            canvas.Scale(ratio);
            if (OriginScale == 0 || OriginYOffset == 0)
            {
                OriginScale = ratio;
                OriginYOffset = (info.Height / 2f) - bounds.MidY;
            }

            canvas.Translate(-bounds.MidX + (float)gestureParameters.TranslationScale[0] + (float)gestureParameters.TranslationMove[0], -bounds.MidY + (float)gestureParameters.TranslationScale[1] + (float)gestureParameters.TranslationMove[1]);
            canvas.Scale((float)gestureParameters.Scale);

            canvas.DrawPicture(_svgPictures[CurrentFloor - 1]);

            if (pathpoints[CurrentFloor - 1].Count != 0)
            {
                foreach (var path in pathpoints[CurrentFloor - 1])
                {
                    canvas.DrawPoints(SKPointMode.Polygon, path, new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = SKColors.Orange,
                        StrokeWidth = 3,
                        StrokeCap = SKStrokeCap.Round
                    });
                    canvas.DrawCircle(path[0].X, path[0].Y, 4, new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = SKColors.Blue,
                        StrokeWidth = 1
                    });
                    canvas.DrawCircle(path[path.Length-1].X, path[path.Length - 1].Y, 5, new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = SKColors.Red,
                            StrokeWidth = 1
                        });
                }
            }
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
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
                    OffsetX = gestureParameters.TranslationScale[0];
                    OffsetY = gestureParameters.TranslationScale[1];
                    CurrentScale = gestureParameters.CurrentScale;

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
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if (gestureParameters.WasScaled)
                        break;
                        
                    gestureParameters.TranslationMove[0] = gestureParameters.Coordinates[0] + (e.TotalX * DeviceDisplay.MainDisplayInfo.Density);
                    gestureParameters.TranslationMove[1] = gestureParameters.Coordinates[1] + (e.TotalY * DeviceDisplay.MainDisplayInfo.Density);
                    gestureParameters.TranslationMove[0] = gestureParameters.TranslationMove[0].Clamp(-(gestureParameters.Size[0] * gestureParameters.Scale - Math.Abs(gestureParameters.Offset[0]) - gestureParameters.Size[0]), Math.Abs(gestureParameters.Offset[0]));
                    gestureParameters.TranslationMove[1] = gestureParameters.TranslationMove[1].Clamp(-(gestureParameters.Size[1] * gestureParameters.Scale - Math.Abs(gestureParameters.Offset[1]) - gestureParameters.Size[1]), Math.Abs(gestureParameters.Offset[1]));
                    OffsetX = gestureParameters.TranslationMove[0] + gestureParameters.TranslationScale[0];
                    OffsetY = gestureParameters.TranslationMove[1] + gestureParameters.TranslationScale[1];
                    CurrentScale += 1;
                    CurrentScale -= 1;
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