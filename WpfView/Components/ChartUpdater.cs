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
using System.Windows.Threading;
using LiveCharts.Charts;
using LiveCharts.Wpf.Charts.Base;

namespace LiveCharts.Wpf.Components
{
    /// <summary>
    /// Defines the WPF chart updater.
    /// </summary>
    /// <seealso cref="LiveCharts.ChartUpdater" />
    public class ChartUpdater : LiveCharts.ChartUpdater
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartUpdater"/> class.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        public ChartUpdater(TimeSpan frequency)
        {
            Timer = new DispatcherTimer {Interval = frequency};
            Timer.Tick += OnTimerOnTick;
            Freq = frequency;
        }

        private DispatcherTimer Timer { get; set; }
        private bool RequiresRestart { get; set; }
        private TimeSpan Freq { get; set; }

        /// <summary>
        /// Runs the updater.
        /// </summary>
        /// <param name="restartView">if set to <c>true</c> [restart view].</param>
        /// <param name="updateNow">if set to <c>true</c> [update now].</param>
        public override void QueueUpdate(bool restartView = false, bool updateNow = false)
        {
            if (Timer == null)
            {
                Timer = new DispatcherTimer {Interval = Freq};
                Timer.Tick += OnTimerOnTick;
                IsUpdating = false;
            }

            if (updateNow)
            {
                UpdaterTick(restartView, true);
                return;
            }

            RequiresRestart = restartView || RequiresRestart;
            if (IsUpdating) return;

            IsUpdating = true;
            Timer.Start();
        }

        /// <summary>
        /// Updates the frequency.
        /// </summary>
        /// <param name="freq">The freq.</param>
        public override void SetFrequency(TimeSpan freq)
        {
            Timer.Interval = freq;
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public override void Unload()
        {
            if (Timer == null) return;
            Timer.Tick -= OnTimerOnTick;
            Timer.Stop();
            Timer.IsEnabled = false;
            Timer = null;
        }

        private void OnTimerOnTick(object sender, EventArgs args)
        {
            UpdaterTick(RequiresRestart, false);
        }

        private void UpdaterTick(bool restartView, bool force)
        {
            var wpfChart = (Chart) Chart.View;
            
            if (!force && !wpfChart.IsVisible) return;

            Timer.Stop();
            Update(restartView, force);
            IsUpdating = false;

            RequiresRestart = false;

            wpfChart.ChartUpdated();
            wpfChart.PrepareScrolBar();
        }
    }
}
