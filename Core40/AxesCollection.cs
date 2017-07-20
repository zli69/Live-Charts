using System.Collections.Generic;
using LiveCharts.Charts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Helpers;

namespace LiveCharts
{
    /// <summary>
    /// Stores a collection of axis.
    /// </summary>
    public class AxesCollection : NoisyCollection<IAxisView>
    {
        /// <summary>
        /// Initializes a new instance of AxisCollection class
        /// </summary>
        public AxesCollection()
        {
            NoisyCollectionChanged += OnNoisyCollectionChanged;
        }

        /// <summary>
        /// Gets the chart that owns the series.
        /// </summary>
        /// <value>
        /// The chart.
        /// </value>
        public ChartCore Chart { get; internal set; }

        private void OnNoisyCollectionChanged(IEnumerable<IAxisView> oldItems, IEnumerable<IAxisView> newItems)
        {
            if (Chart != null)
                Chart.Updater.EnqueueUpdate();

            if (oldItems == null) return;

            foreach (var oldAxis in oldItems)
            {
                oldAxis.Clean();
                if (oldAxis.Model == null) continue;
                var chart = oldAxis.Model.Chart.View;
                if (chart == null) continue;
                chart.RemoveFromView(oldAxis);
                chart.RemoveFromView(oldAxis.Separator);
            }
        }
    }
}