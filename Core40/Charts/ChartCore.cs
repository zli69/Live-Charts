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
using System.Linq;
using LiveCharts.Definitions.Charts;
using LiveCharts.Definitions.Series;
using LiveCharts.Dtos;

namespace LiveCharts.Charts
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ChartCore
    {
        #region Fields 

        private AxesCollection _previousXAxis;
        private AxesCollection _previousYAxis;
        private SeriesCollection _previousSeriesCollection;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartCore"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="updater">The updater.</param>
        protected ChartCore(IChartView view, ChartUpdater updater)
        {
            View = view;
            Updater = updater;
            updater.Chart = this;
        }

        /// <summary>
        /// Initializes the <see cref="ChartCore"/> class.
        /// </summary>
        static ChartCore()
        {
            Configurations = new Charting();
            Randomizer = new Random();
        }

        #endregion

        #region Properties 

        /// <summary>
        /// Gets or sets the randomizer.
        /// </summary>
        /// <value>
        /// The randomizer.
        /// </value>
        public static Random Randomizer { get; set; }
        /// <summary>
        /// Gets or sets the configurations.
        /// </summary>
        /// <value>
        /// The configurations.
        /// </value>
        public static Charting Configurations { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [series initialized].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [series initialized]; otherwise, <c>false</c>.
        /// </value>
        public bool SeriesInitialized { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [are components loaded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [are components loaded]; otherwise, <c>false</c>.
        /// </value>
        public bool AreComponentsLoaded { get; set; }
        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public IChartView View { get; set; }
        /// <summary>
        /// Gets or sets the updater.
        /// </summary>
        /// <value>
        /// The updater.
        /// </value>
        public ChartUpdater Updater { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has unitary points.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has unitary points; otherwise, <c>false</c>.
        /// </value>
        public bool HasUnitaryPoints { get; set; }
        /// <summary>
        /// Gets a value indicating whether [requires hover shape].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires hover shape]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresHoverShape
        {
            get
            {
                return View != null &&
                       (View.HasTooltip || View.HasDataClickEventAttached || View.Hoverable);
            }
        }

        /// <summary>
        /// Gets or sets the x limit.
        /// </summary>
        /// <value>
        /// The x limit.
        /// </value>
        public CoreLimit XLimit { get; set; }
        /// <summary>
        /// Gets or sets the y limit.
        /// </summary>
        /// <value>
        /// The y limit.
        /// </value>
        public CoreLimit YLimit { get; set; }
        /// <summary>
        /// Gets or sets the w limit.
        /// </summary>
        /// <value>
        /// The w limit.
        /// </value>
        public CoreLimit WLimit { get; set; }

        /// <summary>
        /// Gets or sets the index of the current color.
        /// </summary>
        /// <value>
        /// The index of the current color.
        /// </value>
        public int CurrentColorIndex { get; set; }

        /// <summary>
        /// Gets or sets the pan origin.
        /// </summary>
        /// <value>
        /// The pan origin.
        /// </value>
        public CorePoint PanOrigin { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Prepares the axes.
        /// </summary>
        public virtual void PrepareAxes()
        {
            var xAxis = View.AxisX;

            for (var index = 0; index < xAxis.Count; index++)
            {
                SetAxisLimits(
                    xAxis[index].Model,
                    View.ActualSeries
                        // ReSharper disable once AccessToModifiedClosure
                        .Where(series => series.Values != null && series.ScalesXAt == index).ToArray(),
                    AxisOrientation.X);
            }

            var yAxis = View.AxisY;

            for (var index = 0; index < yAxis.Count; index++)
            {
                SetAxisLimits(
                    yAxis[index].Model,
                    View.ActualSeries
                        // ReSharper disable once AccessToModifiedClosure
                        .Where(series => series.Values != null && series.ScalesYAt == index).ToArray(),
                    AxisOrientation.Y);
            }
        }

        /// <summary>
        /// Runs the specialized chart components.
        /// </summary>
        public virtual void RunSpecializedChartComponents()
        {
            
        }
        
        /// <summary>
        /// Calculates the components and margin.
        /// </summary>
        public void CalculateComponentsAndMargin()
        {
            var controlSize = View.ControlSize;

            var curSize = new CoreRectangle(0, 0, View.ControlSize.Width, controlSize.Height);

            curSize = PlaceLegend(curSize);

            const double padding = 4;

            var yAxis = View.AxisY;

            for (int index = 0; index < yAxis.Count; index++)
            {
                var ax = yAxis[index];
                var titleSize = ax.UpdateTitle(this, -90d);
                var biggest = ax.Model.PrepareChart(AxisOrientation.Y, this);

                var x = curSize.Left;

                if (ax.Position == AxisPosition.LeftBottom)
                {
                    ax.SetTitleLeft(x);
                    curSize.Left += titleSize.Height + biggest.Width + padding;
                    curSize.Width -= titleSize.Height + biggest.Width + padding;
                    ax.Model.Tab = curSize.Left;
                }
                else
                {
                    ax.SetTitleLeft(x + curSize.Width - titleSize.Height);
                    curSize.Width -= (titleSize.Height + biggest.Width + padding);
                    ax.Model.Tab = curSize.Left + curSize.Width;
                }

                var uw = ax.Model.EvaluatesUnitWidth ? ChartFunctions.GetUnitWidth(AxisOrientation.Y, this, index)/2 : 0;

                var topE = biggest.Top - uw;
                if (topE> curSize.Top)
                {
                    var dif = topE - curSize.Top;
                    curSize.Top += dif;
                    curSize.Height -= dif;
                }

                var botE = biggest.Bottom - uw;
                if (botE > controlSize.Height - (curSize.Top + curSize.Height))
                {
                    var dif = botE - (controlSize.Height - (curSize.Top + curSize.Height));
                    curSize.Height -= dif;
                }
            }

            var xAxis = View.AxisX;

            for (var index = 0; index < xAxis.Count; index++)
            {
                var xi = xAxis[index];
                var titleSize = xi.UpdateTitle(this);
                var biggest = xi.Model.PrepareChart(AxisOrientation.X, this);
                var top = curSize.Top;

                if (xi.Position == AxisPosition.LeftBottom)
                {
                    xi.SetTitleTop(top + curSize.Height - titleSize.Height);
                    curSize.Height -= (titleSize.Height + biggest.Height);
                    xi.Model.Tab = curSize.Top + curSize.Height;
                }
                else
                {
                    xi.SetTitleTop(top);
                    curSize.Top += titleSize.Height + biggest.Height;
                    curSize.Height -= (titleSize.Height + biggest.Height);
                    xi.Model.Tab = curSize.Top;
                }

                //Notice the unit width is not exact at this point...
                var uw = xi.Model.EvaluatesUnitWidth ? ChartFunctions.GetUnitWidth(AxisOrientation.X, this, index)/2 : 0;

                var leftE = biggest.Left - uw > 0 ? biggest.Left - uw : 0;
                if (leftE > curSize.Left)
                {
                    var dif = leftE - curSize.Left;
                    curSize.Left += dif;
                    curSize.Width -= dif;
                    foreach (var correctedAxis in View.AxisY
                        .Where(correctedAxis => correctedAxis.Position == AxisPosition.LeftBottom))
                    {
                        correctedAxis.Model.Tab += dif;
                    }
                }

                var rightE = biggest.Right - uw > 0 ? biggest.Right - uw : 0;
                if (rightE > controlSize.Width - (curSize.Left + curSize.Width))
                {
                    var dif = rightE - (controlSize.Width - (curSize.Left + curSize.Width));
                    curSize.Width -= dif;
                    foreach (var correctedAxis in View.AxisY
                        .Where(correctedAxis => correctedAxis.Position == AxisPosition.RightTop))
                    {
                        correctedAxis.Model.Tab -= dif;
                    }
                }
            }

            View.DrawMarginTop = curSize.Top;
            View.DrawMarginLeft = curSize.Left;
            View.DrawMarginWidth = curSize.Width;
            View.DrawMarginHeight = curSize.Height;

            for (var index = 0; index < yAxis.Count; index++)
            {
                var ax = yAxis[index];
                var pr = ChartFunctions.FromPlotArea(ax.Model.MaxPointRadius, AxisOrientation.Y, this, index) -
                         ChartFunctions.FromPlotArea(0, AxisOrientation.Y, this, index);
                if (!double.IsNaN(pr))
                {
                    ax.Model.BotLimit += pr;
                    ax.Model.TopLimit -= pr;
                }
                ax.Model.UpdateSeparators(AxisOrientation.Y, this, index);
                ax.SetTitleTop(curSize.Top + curSize.Height*.5 + ax.GetLabelSize().Width*.5);
            }

            for (var index = 0; index < xAxis.Count; index++)
            {
                var xi = xAxis[index];
                var pr = ChartFunctions.FromPlotArea(xi.Model.MaxPointRadius, AxisOrientation.X, this, index) -
                         ChartFunctions.FromPlotArea(0, AxisOrientation.X, this, index);
                if (!double.IsNaN(pr))
                {
                    xi.Model.BotLimit -= pr;
                    xi.Model.TopLimit += pr;
                }
                xi.Model.UpdateSeparators(AxisOrientation.X, this, index);
                xi.SetTitleLeft(curSize.Left + curSize.Width*.5 - xi.GetLabelSize().Width*.5);
            }
        }

        /// <summary>
        /// Places the legend.
        /// </summary>
        /// <param name="drawMargin">The draw margin.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public CoreRectangle PlaceLegend(CoreRectangle drawMargin)
        {
            var legendSize = View.LoadLegend();
            var controlSize = View.ControlSize;

            const int padding = 10;

            switch (View.LegendLocation)
            {
                case LegendLocation.None:
                    View.HideLegend();
                    break;
                case LegendLocation.Top:
                    drawMargin.Top += legendSize.Height;
                    drawMargin.Height -= legendSize.Height;
                    View.ShowLegend(new CorePoint(controlSize.Width * .5 - legendSize.Width * .5, 0));
                    break;
                case LegendLocation.Bottom:
                    var bot = new CorePoint(controlSize.Width*.5 - legendSize.Width*.5,
                        controlSize.Height - legendSize.Height);
                    drawMargin.Height -= legendSize.Height;
                    View.ShowLegend(new CorePoint(bot.X, controlSize.Height - legendSize.Height));
                    break;
                case LegendLocation.Left:
                    drawMargin.Left = drawMargin.Left + legendSize.Width;
                    View.ShowLegend(new CorePoint(0, controlSize.Height*.5 - legendSize.Height*.5));
                    break;
                case LegendLocation.Right:
                    drawMargin.Width -= legendSize.Width + padding;
                    View.ShowLegend(new CorePoint(controlSize.Width - legendSize.Width,
                        controlSize.Height*.5 - legendSize.Height*.5));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return drawMargin;
        }

        /// <summary>
        /// Zooms a unit in.
        /// </summary>
        /// <param name="pivot">The pivot.</param>
        public void ZoomIn(CorePoint pivot)
        {
            var xAxis = View.AxisX;
            var yAxis = View.AxisY;

            if (xAxis== null || yAxis == null) return;

            View.HideTooltip();

            var speed = View.ZoomingSpeed < 0.1 ? 0.1 : (View.ZoomingSpeed > 0.95 ? 0.95 : View.ZoomingSpeed);

            if (View.Zoom == ZoomingOptions.X || View.Zoom == ZoomingOptions.Xy)
            {
                for (var index = 0; index < xAxis.Count; index++)
                {
                    var xi = xAxis[index];

                    var px = ChartFunctions.FromPlotArea(pivot.X, AxisOrientation.X, this, index);

                    var max = double.IsNaN(xi.MaxValue) ? xi.Model.TopLimit : xi.MaxValue;
                    var min = double.IsNaN(xi.MinValue) ? xi.Model.BotLimit : xi.MinValue;
                    var l = max - min;

                    var rMin = (px - min) / l;
                    var rMax = 1 - rMin;

                    var target = l * speed;
                    if (target < xi.Model.View.MinRange) return;
                    var mint = px - target * rMin;
                    var maxt = px + target * rMax;
                    xi.SetRange(mint, maxt);
                }
            }

            if (View.Zoom == ZoomingOptions.Y || View.Zoom == ZoomingOptions.Xy)
            {
                for (var index = 0; index < yAxis.Count; index++)
                {
                    var ax = yAxis[index];

                    var py = ChartFunctions.FromPlotArea(pivot.Y, AxisOrientation.Y, this, index);

                    var max = double.IsNaN(ax.MaxValue) ? ax.Model.TopLimit : ax.MaxValue;
                    var min = double.IsNaN(ax.MinValue) ? ax.Model.BotLimit : ax.MinValue;
                    var l = max - min;
                    var rMin = (py - min) / l;
                    var rMax = 1 - rMin;

                    var target = l * speed;
                    if (target < ax.MinRange) return;
                    var mint = py - target * rMin;
                    var maxt = py + target * rMax;
                    ax.SetRange(mint, maxt);
                }
            }
        }

        /// <summary>
        /// Zooms a unit out.
        /// </summary>
        /// <param name="pivot">The pivot.</param>
        public void ZoomOut(CorePoint pivot)
        {
            View.HideTooltip();

            var xAxis = View.AxisX;
            var yAxis = View.AxisY;

            var speed = View.ZoomingSpeed < 0.1 ? 0.1 : (View.ZoomingSpeed > 0.95 ? 0.95 : View.ZoomingSpeed);

            if (View.Zoom == ZoomingOptions.X || View.Zoom == ZoomingOptions.Xy)
            {
                for (var index = 0; index < xAxis.Count; index++)
                {
                    var xi = xAxis[index];

                    var px = ChartFunctions.FromPlotArea(pivot.X, AxisOrientation.X, this, index);

                    var max = double.IsNaN(xi.MaxValue) ? xi.Model.TopLimit : xi.MaxValue;
                    var min = double.IsNaN(xi.MinValue) ? xi.Model.BotLimit : xi.MinValue;
                    var l = max - min;
                    var rMin = (px - min) / l;
                    var rMax = 1 - rMin;

                    var target = l * (1 / speed);
                    if (target > xi.MaxRange) return;
                    var mint = px- target * rMin;
                    var maxt = px + target * rMax;
                    xi.SetRange(mint, maxt);
                }
            }

            if (View.Zoom == ZoomingOptions.Y || View.Zoom == ZoomingOptions.Xy)
            {
                for (var index = 0; index < yAxis.Count; index++)
                {
                    var ax = yAxis[index];

                    var py = ChartFunctions.FromPlotArea(pivot.Y, AxisOrientation.Y, this, index);
                    
                    var max = double.IsNaN(ax.MaxValue) ? ax.Model.TopLimit : ax.MaxValue;
                    var min = double.IsNaN(ax.MinValue) ? ax.Model.BotLimit : ax.MinValue;
                    var l = max - min;
                    var rMin = (py - min) / l;
                    var rMax = 1 - rMin;

                    var target = l * (1 / speed);
                    if (target > ax.MaxRange) return;
                    var mint = py - target * rMin;
                    var maxt = py + target * rMax;
                    ax.SetRange(mint, maxt);
                }
            }
        }

        /// <summary>
        /// Clears the zoom.
        /// </summary>
        public void ClearZoom()
        {
            foreach (var ax in View.AxisX) ax.SetRange(double.NaN, double.NaN);
            foreach (var ax in View.AxisY) ax.SetRange(double.NaN, double.NaN);
        }

        /// <summary>
        /// Drags the specified delta.
        /// </summary>
        /// <param name="delta">The delta.</param>
        public void Drag(CorePoint delta)
        {
            if (View.Pan == PanningOptions.Unset && View.Zoom == ZoomingOptions.None ||
                View.Pan == PanningOptions.None) return;

            var px = View.Pan == PanningOptions.Unset &&
                     (View.Zoom == ZoomingOptions.X || View.Zoom == ZoomingOptions.Xy);
            px = px || View.Pan == PanningOptions.X || View.Pan == PanningOptions.Xy;

            var xAxis = View.AxisX;
            var yAxis = View.AxisY;

            if (px)
            {
                for (var index = 0; index < xAxis.Count; index++)
                {
                    var xi = xAxis[index];
                    var dx = ChartFunctions.FromPlotArea(delta.X, AxisOrientation.X, this, index) -
                             ChartFunctions.FromPlotArea(0, AxisOrientation.X, this, index);

                    xi.SetRange((double.IsNaN(xi.MinValue) ? xi.Model.BotLimit : xi.MinValue) + dx,
                        (double.IsNaN(xi.MaxValue) ? xi.Model.TopLimit : xi.MaxValue) + dx);
                }
            }

            var py = View.Pan == PanningOptions.Unset &&
                     (View.Zoom == ZoomingOptions.Y || View.Zoom == ZoomingOptions.Xy);
            py = py || View.Pan == PanningOptions.Y || View.Pan == PanningOptions.Xy;
            if (py)
            {
                for (var index = 0; index < yAxis.Count; index++)
                {
                    var ax = yAxis[index];
                    var dy = ChartFunctions.FromPlotArea(delta.Y, AxisOrientation.Y, this, index) -
                             ChartFunctions.FromPlotArea(0, AxisOrientation.Y, this, index);

                    ax.SetRange((double.IsNaN(ax.MinValue) ? ax.Model.BotLimit : ax.MinValue) + dy,
                        (double.IsNaN(ax.MaxValue) ? ax.Model.TopLimit : ax.MaxValue) + dy);
                }
            }
        }

        /// <summary>
        /// Gets the default color of the next.
        /// </summary>
        /// <returns></returns>
        public object GetNextDefaultColor()
        {
            if (View.Series.CurrentSeriesIndex == int.MaxValue) View.Series.CurrentSeriesIndex = 0;
            var i = View.Series.CurrentSeriesIndex;
            View.Series.CurrentSeriesIndex++;

            if (View.SeriesColors != null)
            {
                var rsc = View.RandomizeStartingColor
                    ? Randomizer.Next(0, View.SeriesColors.Count)
                    : 0;
                return View.SeriesColors[(i + rsc) % View.SeriesColors.Count];
            }

            var r = View.RandomizeStartingColor ? Randomizer.Next(0, View.Colors.Count) : 0;
            return View.Colors[(i + r) % View.Colors.Count];
        }

        /// <summary>
        /// Notifies the series collection changed.
        /// </summary>
        public void NotifySeriesCollectionChanged()
        {
            if (View.Series != null)
            {
                View.Series.Chart = this;
                foreach (var series in View.Series)
                {
                    series.Core.Chart = View.Core;
                }
            }

            if (_previousSeriesCollection != null)
            {
                foreach (var series in _previousSeriesCollection)
                {
                    series.Erase(true);
                }
            }

            Updater.EnqueueUpdate();
            _previousSeriesCollection = View.Series;
        }

        /// <summary>
        /// Notifies the axis instance changed.
        /// </summary>
        public void NotifyAxisInstanceChanged(AxisOrientation orientation)
        {
            var ax = orientation == AxisOrientation.X ? _previousXAxis : _previousYAxis;

            if (ax != null)
            {
                foreach (var axis in ax)
                {
                    axis.Clean();
                }
            }

            if (orientation == AxisOrientation.X)
            {
                _previousXAxis = View.AxisX;
            }
            else
            {
                _previousYAxis = View.AxisY;
            }

            Updater.EnqueueUpdate();
        }

        /// <summary>
        /// Notifies the updater frequency changed.
        /// </summary>
        public void NotifyUpdaterFrequencyChanged()
        {
            var freq = View.DisableAnimations ? TimeSpan.FromMilliseconds(10) : View.AnimationsSpeed;

            if (Updater == null) return;

            Updater.SetFrequency(freq);
            Updater.EnqueueUpdate(true);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Stacks the points.
        /// </summary>
        /// <param name="stackables">The stackables.</param>
        /// <param name="stackAt">The stack at.</param>
        /// <param name="stackIndex">Index of the stack.</param>
        /// <param name="mode">The mode.</param>
        protected void StackPoints(IEnumerable<ISeriesView> stackables, AxisOrientation stackAt, int stackIndex,
            StackMode mode = StackMode.Values)
        {
            var stackedColumns = stackables.SelectMany(x => x.ActualValues.GetPoints(x))
                .GroupBy(x => stackAt == AxisOrientation.X ? x.Y : x.X);

            double mostLeft = 0, mostRight = 0;

            foreach (var column in stackedColumns)
            {
                double sumLeft = 0, sumRight = 0;

                foreach (var item in column)
                {
                    var s = stackAt == AxisOrientation.X ? item.X : item.Y;
                    if (s < 0)
                        sumLeft += s;
                    else
                        sumRight += s;
                }

                var lastLeft = 0d;
                var lastRight = 0d;
                var leftPart = 0d;
                var rightPart = 0d;

                foreach (var point in column)
                {
                    var pulled = stackAt == AxisOrientation.X ? point.X : point.Y;

                    //notice using (pulled < 0) or (pulled <= 0) could cause an issue similar to
                    //https://github.com/beto-rodriguez/Live-Charts/issues/231
                    //from that issue I changed <= to <
                    //only because it is more common to use positive values than negative
                    //you could face a similar issue if you are stacking only negative values
                    //a work around is forcing (pulled < 0) to be true,
                    //instead of using zero values, use -0.000000001/

                    if (pulled < 0)
                    {
                        point.From = lastLeft;
                        point.To = lastLeft + pulled;
                        point.Sum = sumLeft;
                        point.Participation = (point.To - point.From) / point.Sum;
                        point.Participation = double.IsNaN(point.Participation)
                            ? 0
                            : point.Participation;
                        leftPart += point.Participation;
                        point.StackedParticipation = leftPart;

                        lastLeft = point.To;
                    }
                    else
                    {
                        point.From = lastRight;
                        point.To = lastRight + pulled;
                        point.Sum = sumRight;
                        point.Participation = (point.To - point.From) / point.Sum;
                        point.Participation = double.IsNaN(point.Participation)
                            ? 0
                            : point.Participation;
                        rightPart += point.Participation;
                        point.StackedParticipation = rightPart;

                        lastRight = point.To;
                    }
                }

                if (sumLeft < mostLeft) mostLeft = sumLeft;
                if (sumRight > mostRight) mostRight = sumRight;
            }

            var xAxis = View.AxisX;
            var yAxis = View.AxisY;

            if (stackAt == AxisOrientation.X)
            {
                var ax = xAxis[stackIndex];

                if (mode == StackMode.Percentage)
                {
                    if (double.IsNaN(ax.MinValue)) ax.Model.BotLimit = 0;
                    if (double.IsNaN(ax.MaxValue)) ax.Model.TopLimit = 1;
                }
                else
                {
                    if (mostLeft < ax.Model.BotLimit)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (double.IsNaN(ax.MinValue))
                            ax.Model.BotLimit = mostLeft == 0
                                ? 0
                                : ((int) (mostLeft/ax.Model.S) - 1)*ax.Model.S;
                    if (mostRight > ax.Model.TopLimit)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (double.IsNaN(ax.MaxValue))
                            ax.Model.TopLimit = mostRight == 0
                                ? 0
                                : ((int) (mostRight/ax.Model.S) + 1)*ax.Model.S;
                }
            }

            if (stackAt == AxisOrientation.Y)
            {
                var ay = yAxis[stackIndex];

                if (mode == StackMode.Percentage)
                {
                    if (double.IsNaN(ay.MinValue)) ay.Model.BotLimit = 0;
                    if (double.IsNaN(ay.MaxValue)) ay.Model.TopLimit = 1;
                }
                else
                {
                    if (mostLeft < ay.Model.BotLimit)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (double.IsNaN(ay.MinValue))
                            ay.Model.BotLimit = mostLeft == 0
                                ? 0
                                : ((int) (mostLeft/ay.Model.S) - 1)*ay.Model.S;
                    if (mostRight > ay.Model.TopLimit)
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        if (double.IsNaN(ay.MaxValue))
                            ay.Model.TopLimit = mostRight == 0
                                ? 0
                                : ((int) (mostRight/ay.Model.S) + 1)*ay.Model.S;
                }
            }
        }
        #endregion

        #region Privates
        private static void SetAxisLimits(AxisCore ax, IList<ISeriesView> series, AxisOrientation orientation)
        {
            var first = new CoreLimit();
            var firstR = 0d;

            if (series.Count > 0)
            {
                first = orientation == AxisOrientation.X
                    ? series[0].Values.GetTracker(series[0]).XLimit
                    : series[0].Values.GetTracker(series[0]).YLimit;
                var view = series[0] as IAreaPoint;
                firstR = view != null ? view.PointDiameter : 0;
            }
            
            //                     [ max, min, pointRadius ]
            var boundries = new[] {first.Max, first.Min, firstR};

            for (var index = 1; index < series.Count; index++)
            {
                var seriesView = series[index];
                var tracker = seriesView.Values.GetTracker(seriesView);
                var limit = orientation == AxisOrientation.X ? tracker.XLimit : tracker.YLimit;
                var view = seriesView as IAreaPoint;
                var radius = view != null ? view.PointDiameter : 0;

                if (limit.Max > boundries[0]) boundries[0] = limit.Max;
                if (limit.Min < boundries[1]) boundries[1] = limit.Min;
                if (radius > boundries[2]) boundries[2] = radius;
            }

            ax.TopSeriesLimit = boundries[0];
            ax.BotSeriesLimit = boundries[1];

            ax.TopLimit = double.IsNaN(ax.View.MaxValue) ? boundries[0] : ax.View.MaxValue;
            ax.BotLimit = double.IsNaN(ax.View.MinValue) ? boundries[1] : ax.View.MinValue;

            ax.MaxPointRadius = boundries[2];
        }
        #endregion
    }
}