//The MIT License(MIT)

//Copyright(c) 2016 Alberto Rodriguez & LiveCharts Contributors

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LiveCharts.Definitions.Points;
using LiveCharts.Definitions.Series;
using LiveCharts.Dtos;
using LiveCharts.Helpers;
using LiveCharts.SeriesAlgorithms;
using LiveCharts.Wpf.Components;
using LiveCharts.Wpf.Points;

namespace LiveCharts.Wpf
{
    /// <summary>
    /// The line series displays trends between points, you must add this series to a cartesian chart. 
    /// </summary>
    public class LineSeries : Series, ILineSeriesView, IAreaPoint, IFondeable
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of LineSeries class
        /// </summary>
        public LineSeries() 
        {
            Core = new LineCore(this);
            InitializeDefuaults();
        }

        /// <summary>
        /// Initializes a new instance of LineSeries class with a given mapper
        /// </summary>
        /// <param name="configuration"></param>
        public LineSeries(object configuration)
        {
            Core = new LineCore(this);
            Configuration = configuration;
            InitializeDefuaults();
        }

        #endregion

        #region Private Properties

        protected PathFigure Figure { get; set; }
        internal Path Path { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is path initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is path initialized; otherwise, <c>false</c>.
        /// </value>
        protected bool IsPathInitialized { get; set; }
        internal List<LineSeriesPathHelper> Splitters { get; set; }
        /// <summary>
        /// Gets or sets the active splitters.
        /// </summary>
        /// <value>
        /// The active splitters.
        /// </value>
        protected int ActiveSplitters { get; set; }
        /// <summary>
        /// Gets or sets the splitters collector.
        /// </summary>
        /// <value>
        /// The splitters collector.
        /// </value>
        protected int SplittersCollector { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is new; otherwise, <c>false</c>.
        /// </value>
        protected bool IsNew { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The point geometry size property
        /// </summary>
        public static readonly DependencyProperty PointGeometrySizeProperty = DependencyProperty.Register(
            "PointGeometrySize", typeof (double), typeof (LineSeries), 
            new PropertyMetadata(default(double), EnqueueUpdateCallback));
        /// <summary>
        /// Gets or sets the point geometry size, increasing this property will make the series points bigger
        /// </summary>
        public double PointGeometrySize
        {
            get { return (double) GetValue(PointGeometrySizeProperty); }
            set { SetValue(PointGeometrySizeProperty, value); }
        }

        /// <summary>
        /// The point foreground property
        /// </summary>
        public static readonly DependencyProperty PointForegroundProperty = DependencyProperty.Register(
            "PointForeground", typeof (Brush), typeof (LineSeries), 
            new PropertyMetadata(default(Brush)));
        /// <summary>
        /// Gets or sets the point shape foreground.
        /// </summary>
        public Brush PointForeground
        {
            get { return (Brush) GetValue(PointForegroundProperty); }
            set { SetValue(PointForegroundProperty, value); }
        }

        /// <summary>
        /// The line smoothness property
        /// </summary>
        public static readonly DependencyProperty LineSmoothnessProperty = DependencyProperty.Register(
            "LineSmoothness", typeof (double), typeof (LineSeries), 
            new PropertyMetadata(default(double), EnqueueUpdateCallback));
        /// <summary>
        /// Gets or sets line smoothness, this property goes from 0 to 1, use 0 to draw straight lines, 1 really curved lines.
        /// </summary>
        public double LineSmoothness
        {
            get { return (double) GetValue(LineSmoothnessProperty); }
            set { SetValue(LineSmoothnessProperty, value); }
        }

        /// <summary>
        /// The area limit property
        /// </summary>
        public static readonly DependencyProperty AreaLimitProperty = DependencyProperty.Register(
            "AreaLimit", typeof(double), typeof(LineSeries), new PropertyMetadata(double.NaN));
        /// <summary>
        /// Gets or sets the limit where the fill area changes orientation
        /// </summary>
        public double AreaLimit
        {
            get { return (double) GetValue(AreaLimitProperty); }
            set { SetValue(AreaLimitProperty, value); }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// This method runs when the update starts
        /// </summary>
        protected override void OnSeriesUpdateStart()
        {
            ActiveSplitters = 0;

            if (SplittersCollector == int.MaxValue - 1)
            {
                //just in case!
                Splitters.ForEach(s => s.SplitterCollectorIndex = 0);
                SplittersCollector = 0;
            }
        }

        /// <summary>
        /// Gets the view of a given point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        protected override IChartPointView GetPointView(ChartPoint point, string label)
        {
            var mhr = PointGeometrySize < 10 ? 10 : PointGeometrySize;

            var pbv = (HorizontalBezierPointView) point.View;

            if (pbv == null)
            {
                pbv = new HorizontalBezierPointView
                {
                    Segment = new BezierSegment(),
                    ShadowContainer = Splitters[ActiveSplitters].ShadowFigure,
                    LineContainer = Splitters[ActiveSplitters].LineFigure,
                    IsNew = true
                };
            }
            else
            {
                pbv.IsNew = false;
                point.SeriesView.Core.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.Shape);
                point.SeriesView.Core.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.HoverShape);
                point.SeriesView.Core.Chart.View
                    .EnsureElementBelongsToCurrentDrawMargin(pbv.DataLabel);
            }

            if (Core.Chart.RequiresHoverShape && pbv.HoverShape == null)
            {
                pbv.HoverShape = new Rectangle
                {
                    Fill = Brushes.Transparent,
                    StrokeThickness = 0,
                    Width = mhr,
                    Height = mhr
                };

                Panel.SetZIndex(pbv.HoverShape, int.MaxValue);
                Core.Chart.View.EnableHoveringFor(pbv.HoverShape);
                Core.Chart.View.AddToDrawMargin(pbv.HoverShape);
            }

            if (pbv.HoverShape != null) pbv.HoverShape.Visibility = Visibility;

            if (PointGeometry != null && Math.Abs(PointGeometrySize) > 0.1 && pbv.Shape == null)
            {
                if (PointGeometry != null)
                {
                    pbv.Shape = new Path
                    {
                        Stretch = Stretch.Fill,
                        StrokeThickness = StrokeThickness
                    };
                }

                Core.Chart.View.AddToDrawMargin(pbv.Shape);
            }

            if (pbv.Shape != null)
            {
                pbv.Shape.Fill = PointForeground;
                pbv.Shape.Stroke = Stroke;
                pbv.Shape.StrokeThickness = StrokeThickness;
                pbv.Shape.Width = PointGeometrySize;
                pbv.Shape.Height = PointGeometrySize;
                pbv.Shape.Data = PointGeometry;
                pbv.Shape.Visibility = Visibility;
                Panel.SetZIndex(pbv.Shape, Panel.GetZIndex(this) + 1);

                if (point.Stroke != null) pbv.Shape.Stroke = (Brush) point.Stroke;
                if (point.Fill != null) pbv.Shape.Fill = (Brush) point.Fill;
            }

            if (DataLabels)
            {
                pbv.DataLabel = UpdateLabelContent(new DataLabelViewModel
                {
                    FormattedText = label,
                    Point = point
                }, pbv.DataLabel);
            }

            if (!DataLabels && pbv.DataLabel != null)
            {
                Core.Chart.View.RemoveFromDrawMargin(pbv.DataLabel);
                pbv.DataLabel = null;
            }

            return pbv;
        }

        /// <summary>
        /// This method runs when the update finishes
        /// </summary>
        protected override void OnSeriesUpdatedFinish()
        {
            base.OnSeriesUpdatedFinish();

            //foreach (var inactive in Splitters
            //    .Where(s => s.SplitterCollectorIndex < SplittersCollector).ToList())
            //{
            //    ((PathGeometry)Path.Data).Figures[0].Segments.Remove(inactive.Left);
            //    ((PathGeometry)Path.Data).Figures[0].Segments.Remove(inactive.Bottom);
            //    ((PathGeometry)Path.Data).Figures[0].Segments.Remove(inactive.Right);
            //    Splitters.Remove(inactive);
            //}
        }

        /// <summary>
        /// Erases series
        /// </summary>
        /// <param name="removeFromView"></param>
        protected override void Erase(bool removeFromView = true)
        {
            ((ISeriesView) this).ActualValues.GetPoints(this).ForEach(p =>
            {
                if (p.View != null)
                    p.View.RemoveFromView(Core.Chart);
            });
            if (removeFromView)
            {
                Core.Chart.View.RemoveFromDrawMargin(Path);
                Core.Chart.View.RemoveFromView(this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the segment.
        /// </summary>
        /// <param name="atIndex">At index.</param>
        /// <param name="location">The location.</param>
        public virtual void StartSegment(int atIndex, CorePoint location)
        {
            InitializeNewPath(location, 0);

            var splitter = Splitters[ActiveSplitters];
            splitter.SplitterCollectorIndex = SplittersCollector;

            var animSpeed = Core.Chart.View.AnimationsSpeed;
            var noAnim = Core.Chart.View.DisableAnimations;

            var areaLimit = ChartFunctions.ToDrawMargin(double.IsNaN(AreaLimit)
                ? Core.Chart.View.AxisY[ScalesYAt].Model.FirstSeparator
                : AreaLimit, AxisOrientation.Y, Core.Chart, ScalesYAt);

            Splitters[ActiveSplitters].LineFigure.BeginAnimation(PathFigure.StartPointProperty,
                new PointAnimation(new Point(location.X, location.Y), animSpeed));

            if (Core.Chart.View.DisableAnimations || IsNew)
            {
                Splitters[ActiveSplitters].ShadowFigure.StartPoint = new Point(location.X, areaLimit);
            }
            else
            {
                Splitters[ActiveSplitters].ShadowFigure.BeginAnimation(PathFigure.StartPointProperty,
                    new PointAnimation(new Point(location.X, areaLimit), animSpeed));
            }

            atIndex = 0; //ToDo: Verify it!

            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Bottom);

            if (splitter.IsNew)
            {
                splitter.Bottom.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
                splitter.Left.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
            }

            if (noAnim)
                splitter.Bottom.Point = new Point(location.X, areaLimit);
            else
                splitter.Bottom.BeginAnimation(LineSegment.PointProperty,
                    new PointAnimation(new Point(location.X, areaLimit), animSpeed));
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(atIndex, splitter.Bottom);

            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Left);
            if (noAnim)
                splitter.Left.Point = location.AsPoint();
            else
                splitter.Left.BeginAnimation(LineSegment.PointProperty,
                    new PointAnimation(location.AsPoint(), animSpeed));
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(atIndex + 1, splitter.Left);

        }

        /// <summary>
        /// Ends the segment.
        /// </summary>
        /// <param name="atIndex">At index.</param>
        /// <param name="location">The location.</param>
        public virtual void EndSegment(int atIndex, CorePoint location)
        {
             var splitter = Splitters[ActiveSplitters];

            var animSpeed = Core.Chart.View.AnimationsSpeed;
            var noAnim = Core.Chart.View.DisableAnimations;

            var areaLimit = ChartFunctions.ToDrawMargin(double.IsNaN(AreaLimit)
                 ? Core.Chart.View.AxisY[ScalesYAt].Model.FirstSeparator
                 : AreaLimit, AxisOrientation.Y, Core.Chart, ScalesYAt);

            var uw = Core.Chart.View.AxisX[ScalesXAt].Model.EvaluatesUnitWidth
                ? ChartFunctions.GetUnitWidth(AxisOrientation.X, Core.Chart, ScalesXAt)/2
                : 0;
            location.X -= uw;

            if (splitter.IsNew)
            {
                splitter.Right.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
            }

            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Right);
            if (noAnim)
                splitter.Right.Point = new Point(location.X, areaLimit);
            else
                splitter.Right.BeginAnimation(LineSegment.PointProperty,
                    new PointAnimation(new Point(location.X, areaLimit), animSpeed));
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(atIndex, splitter.Right);

            splitter.IsNew = false;
            ActiveSplitters++;
        }
        #endregion

        #region Series View Implementation

        double ILineSeriesView.LineSmoothness { get { return LineSmoothness; } }

        double ILineSeriesView.AreaLimit { get { return AreaLimit; } }

        double IAreaPoint.PointDiameter { get { return (PointGeometry == null ? 0 : PointGeometrySize) / 2; } }

        void ILineSeriesView.StartSegment(CorePoint location, double areaLimit)
        {
            InitializeNewPath(location, areaLimit);

            var splitter = Splitters[ActiveSplitters];
            splitter.SplitterCollectorIndex = SplittersCollector;

            Splitters[ActiveSplitters].LineFigure.StartPoint = new Point(location.X, location.Y);
            Splitters[ActiveSplitters].ShadowFigure.StartPoint = new Point(location.X, areaLimit);

            if (splitter.IsNew)
            {
                splitter.Bottom.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
                splitter.Left.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
            }

            splitter.Bottom.Point = new Point(location.X, areaLimit);
            splitter.Left.Point = location.AsPoint();
        }

        void ILineSeriesView.StartAnimatedSegment(CorePoint location, double areaLimit, TimeSpan animationsSpeed)
        {
            InitializeNewPath(location, areaLimit);

            var splitter = Splitters[ActiveSplitters];
            splitter.SplitterCollectorIndex = SplittersCollector;

            Splitters[ActiveSplitters]
                .LineFigure.BeginAnimation(
                    PathFigure.StartPointProperty,
                    new PointAnimation(new Point(location.X, location.Y), animationsSpeed));

            Splitters[ActiveSplitters]
                .ShadowFigure.BeginAnimation(
                    PathFigure.StartPointProperty,
                    new PointAnimation(new Point(location.X, areaLimit), animationsSpeed));

            splitter.Bottom.BeginAnimation(LineSegment.PointProperty,
                new PointAnimation(new Point(location.X, areaLimit), animationsSpeed));

            splitter.Left.BeginAnimation(LineSegment.PointProperty,
                new PointAnimation(location.AsPoint(), animationsSpeed));
        }

        void ILineSeriesView.EndSegment(int atIndex, CorePoint location)
        {
            var splitter = Splitters[ActiveSplitters];

            var animSpeed = Core.Chart.View.AnimationsSpeed;
            var noAnim = Core.Chart.View.DisableAnimations;

            var areaLimit = ChartFunctions.ToDrawMargin(double.IsNaN(AreaLimit)
                ? Core.Chart.View.AxisY[ScalesYAt].Model.FirstSeparator
                : AreaLimit, AxisOrientation.Y, Core.Chart, ScalesYAt);

            var uw = Core.Chart.View.AxisX[ScalesXAt].Model.EvaluatesUnitWidth
                ? ChartFunctions.GetUnitWidth(AxisOrientation.X, Core.Chart, ScalesXAt) / 2
                : 0;
            location.X -= uw;

            if (splitter.IsNew)
            {
                splitter.Right.Point = new Point(location.X, Core.Chart.View.DrawMarginHeight);
            }

            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Right);
            if (noAnim)
                splitter.Right.Point = new Point(location.X, areaLimit);
            else
                splitter.Right.BeginAnimation(LineSegment.PointProperty,
                    new PointAnimation(new Point(location.X, areaLimit), animSpeed));
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(atIndex, splitter.Right);

            splitter.IsNew = false;
            ActiveSplitters++;
        }

        #endregion

        #region Private Methods

        private void InitializeNewPath(CorePoint location, double areaLimit)
        {
            if (Splitters.Count > ActiveSplitters) return;

            var shadow = new Path
            {
                Fill = Fill,
                Visibility = Visibility,
                StrokeDashArray = StrokeDashArray,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(location.X, areaLimit)
                        }
                    }
                },
                
            };

            Panel.SetZIndex(shadow, Panel.GetZIndex(this));
            Panel.SetZIndex(shadow, Panel.GetZIndex(this));
            Core.Chart.View.EnsureElementBelongsToCurrentDrawMargin(shadow);

            var line = new Path
            {
                Visibility = Visibility,
                StrokeDashArray = StrokeDashArray,
                StrokeThickness = StrokeThickness,
                Stroke = Stroke,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(location.X, areaLimit)
                        }
                    }
                }
            };

            Panel.SetZIndex(line, Panel.GetZIndex(this));
            Panel.SetZIndex(line, Panel.GetZIndex(this));
            Core.Chart.View.EnsureElementBelongsToCurrentDrawMargin(line);

            Splitters.Add(new LineSeriesPathHelper(location, areaLimit)
            {
                IsNew = true,
                ShadowFigure = ((PathGeometry)shadow.Data).Figures[0],
                LineFigure = ((PathGeometry)line.Data).Figures[0]
            });

            SplittersCollector++;

            var splitter = Splitters[ActiveSplitters];
            splitter.SplitterCollectorIndex = SplittersCollector;
            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Bottom);
            Splitters[ActiveSplitters].ShadowFigure.Segments.Remove(splitter.Left);
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(0, splitter.Bottom);
            Splitters[ActiveSplitters].ShadowFigure.Segments.Insert(1, splitter.Left);
        }

        private void InitializeDefuaults()
        {
            Path = new Path();

            //var geometry = new PathGeometry();
            //geometry.Figures.Add(new PathFigure());
            //Path.Data = geometry;

            SetCurrentValue(LineSmoothnessProperty, .7d);
            SetCurrentValue(PointGeometrySizeProperty, 8d);
            SetCurrentValue(PointForegroundProperty, Brushes.White);
            SetCurrentValue(StrokeThicknessProperty, 2d);

            Func<ChartPoint, string> defaultLabel = x => Core.CurrentYAxis.GetFormatter()(x.Y);
            SetCurrentValue(LabelPointProperty, defaultLabel);

            DefaultFillOpacity = 0.15;
            Splitters = new List<LineSeriesPathHelper>();

            IsNew = true;
        }

        #endregion
    }
}
