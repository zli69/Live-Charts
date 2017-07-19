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
using LiveCharts.Charts;
using LiveCharts.Helpers;

namespace LiveCharts
{
    /// <summary>
    /// Defines the ChartUpdater logic.
    /// </summary>
    public abstract class ChartUpdater
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is updating.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is updating; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdating { get; set; }
        /// <summary>
        /// Gets or sets the chart.
        /// </summary>
        /// <value>
        /// The chart.
        /// </value>
        public ChartCore Chart { get; set; }

        /// <summary>
        /// Runs the specified restart view.
        /// </summary>
        /// <param name="restartView">if set to <c>true</c> [restart view].</param>
        /// <param name="updateNow">if set to <c>true</c> [update now].</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void QueueUpdate(bool restartView = false, bool updateNow = false)   
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the frequency.
        /// </summary>
        /// <param name="freq">The freq.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void SetFrequency(TimeSpan freq)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public virtual void Unload()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the specified restarts animations.
        /// </summary>
        /// <param name="restartsAnimations">if set to <c>true</c> [restarts animations].</param>
        /// <param name="force"></param>
        protected virtual void Update(bool restartsAnimations = false, bool force = false)   
        {
            if (!force && Chart.View.UpdaterState == UpdaterState.Paused) return;
             
            if (!force && !Chart.View.IsControlLoaded) return;

            Chart.View.SetParentsTree();

            if (restartsAnimations)
                Chart.View.ActualSeries.ForEach(s =>
                {
                    if (s.ActualValues == null) return;
                    s.ActualValues.GetPoints(s).ForEach(p =>
                    {
                        if (p == null || p.View == null) return;
                        p.View.RemoveFromView(Chart);
                        p.View = null;
                    });
                });

            foreach (var series in Chart.View.ActualSeries)
            {
                Chart.View.EnsureElementBelongsToCurrentView(series);
                series.ActualValues.Initialize(series);
                series.InitializeColors();
                series.DrawSpecializedElements();
            }

            Chart.PrepareAxes();
            Chart.RunSpecializedChartComponents();

            foreach (var series in Chart.View.ActualSeries)
            {
                series.OnSeriesUpdateStart();
                series.ActualValues.InitializeStep(series);
                series.Model.Update();
                series.ActualValues.CollectGarbage(series);
                series.OnSeriesUpdatedFinish();
                series.PlaceSpecializedElements();
            }
        }
    }
}