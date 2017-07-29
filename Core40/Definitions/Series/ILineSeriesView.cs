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
using LiveCharts.Dtos;

namespace LiveCharts.Definitions.Series
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="LiveCharts.Definitions.Series.ISeriesView" />
    public interface ILineSeriesView : ISeriesView
    {
        /// <summary>
        /// Gets or sets the line smoothness.
        /// </summary>
        /// <value>
        /// The line smoothness.
        /// </value>
        double LineSmoothness { get; }

        /// <summary>
        /// Gets or sets the area limit.
        /// </summary>
        /// <value>
        /// The area limit.
        /// </value>
        double AreaLimit { get; }

        /// <summary>
        /// Starts the segment.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="areaLimit">The animations speed.</param>
        void StartSegment(CorePoint location, double areaLimit);

        /// <summary>
        /// Starts the animated segment.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="areaLimit">The area limit.</param>
        /// <param name="animationsSpeed">The animations speed.</param>
        void StartAnimatedSegment(CorePoint location, double areaLimit, TimeSpan animationsSpeed);

        /// <summary>
        /// Ends the segment.
        /// </summary>
        /// <param name="atIndex">At index.</param>
        /// <param name="location">The location.</param>
        void EndSegment(int atIndex, CorePoint location);
    }
}