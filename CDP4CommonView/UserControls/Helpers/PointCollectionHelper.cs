// -------------------------------------------------------------------------------------------------
// <copyright file="PointCollectionHelper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Handles a collection of points and convinience methods.
    /// </summary>
    public class PointCollectionHelper
    {
        private PointCollection pointCollection;
        private Point startPoint;
        private Double[] yValues;
        private Double[] xValues;
        private bool areXValuesGenerated;
        private bool areYValuesGenerated;
        private Dictionary<double, List<Double>> xRanges = new Dictionary<double, List<Double>>();
        private Dictionary<double, List<Double>> yRanges = new Dictionary<double, List<Double>>();
        private bool areXRangesGenerated;
        private bool areYRangesGenerated;

        public PointCollectionHelper(PointCollection pointCollection, Point StartPoint)
        {
            this.pointCollection = pointCollection;
            this.startPoint = StartPoint;
        }

        /// <summary>
        /// Ensures that the x values are computed if they havn't been already.
        /// </summary>
        private void GenerateXValues()
        {
            if (!this.areXValuesGenerated)
            {
                var distinct = new HashSet<double>();
                
                foreach (var p in pointCollection)
                {
                    distinct.Add(p.X);
                }

                if (this.startPoint != null)
                {
                    distinct.Add(this.startPoint.X);
                }

                this.xValues = new Double[distinct.Count];
                distinct.CopyTo(this.xValues);
                Array.Sort(this.xValues);
                
                this.areXValuesGenerated = true;
            }

        }

        /// <summary>
        /// Ensures that the y values are computed if they havn't been already.
        /// </summary>
        private void GenerateYValues()
        {
            if (!this.areYValuesGenerated)
            {
                var distinct = new HashSet<double>();

                foreach (var p in pointCollection)
                {
                    distinct.Add(p.Y);
                }

                if (this.startPoint != null)
                {
                    distinct.Add(this.startPoint.Y);
                }

                this.yValues = new Double[distinct.Count];
                distinct.CopyTo(this.yValues);
                Array.Sort(this.yValues);
                
                this.areYValuesGenerated = true;
            }
        }

        /// <summary>
        /// Generates the x ranges.
        /// </summary>
        private void GenerateXRanges()
        {
            if (!this.areXRangesGenerated)
            {
                this.GenerateXValues();
                
                foreach (var d in xValues)
                {
                    xRanges.Add(d, this.YAtXInternal(d));
                }

                this.areXRangesGenerated = true;
            }

        }

        /// <summary>
        /// Calculates the y points at a given x point.
        /// </summary>
        /// <param name="x">The x coordinate used to look up the y's</param>
        /// <returns>The list of y coordinates at this x.</returns>
        private List<double> YAtXInternal(double x)
        {
            var yVals = new List<double>();
            
            foreach (var point in pointCollection)
            {
                if (point.X == x)
                {
                    yVals.Add(point.Y);
                }
            }

            if (this.startPoint != null)
            {
                if (this.startPoint.X == x)
                {
                    if (!yVals.Contains(this.startPoint.Y))
                    {
                        yVals.Add(this.startPoint.Y);
                    }
                }
            }

            yVals.Sort();
            
            return yVals;
        }

        /// <summary>
        /// Generates the x ranges.
        /// </summary>
        private void GenerateYRanges()
        {
            if (!this.areYRangesGenerated)
            {
                this.GenerateYValues();
                
                foreach (var d in yValues)
                {
                    this.yRanges.Add(d, this.XAtYInternal(d));
                }

                this.areYRangesGenerated = true;
            }
        }

        /// <summary>
        /// Calculates the x points at a given y point.
        /// </summary>
        /// <param name="y">The y coordinate used to look up the x's</param>
        /// <returns>The list of x coordinates at this y.</returns>
        private List<double> XAtYInternal(Double y)
        {
            var xVals = new List<double>();
            
            foreach (var point in pointCollection)
            {
                if (point.Y == y)
                {
                    if (!xVals.Contains(point.X))
                    {
                        xVals.Add(point.X);
                    }
                }
            }

            if (this.startPoint != null)
            {
                if (this.startPoint.Y == y)
                {
                    if (!xVals.Contains(this.startPoint.X))
                    {
                        xVals.Add(this.startPoint.X);
                    }
                }
            }

            xVals.Sort();
            
            return xVals;
        }

        /// <summary>
        /// Gets the x points at a given y point.
        /// </summary>
        /// <param name="y">The y coordinate used to look up the x's</param>
        /// <returns>The list of x coordinates at this y.</returns>
        public List<double> XAtY(double y)
        {
            this.GenerateYRanges();
            return this.yRanges[y];

        }

        /// <summary>
        /// Calculates the x points at two given y points.
        /// </summary>
        /// <param name="y1">The first y coordinate used to look up the x's.</param>
        /// <param name="y2">The second y coordinate used to look up the x's.</param>
        /// <returns>The list of x coordinates at this y.</returns>
        public List<double> XAtY(double y1, double y2)
        {
            this.GenerateYRanges();
            var y1Hash = new HashSet<double>();
            
            foreach (var d in this.XAtY(y1))
            {
                y1Hash.Add(d);
            }

            var matches = y1Hash.Intersect(XAtY(y2));
            var matchList = new List<double>();
            
            matchList.AddRange(matches);
            
            return matchList;
        }

        /// <summary>
        /// Gets the local maximum x value
        /// </summary>
        public double MaxX
        {
            get
            {
                if (this.pointCollection.Count == 0)
                {
                    return double.NaN;
                }
                
                this.GenerateXValues();
                
                return this.xValues[xValues.Length - 1];
            }
        }

        /// <summary>
        /// Gets the local maximum y value
        /// </summary>
        public double MaxY
        {
            get
            {
                if (this.pointCollection.Count == 0)
                {
                    return Double.NaN;
                }

                this.GenerateYValues();
                
                return this.xValues[yValues.Length - 1];
            }
        }

        /// <summary>
        /// Gets the local minimum x value.
        /// </summary>
        public double MinX
        {
            get
            {
                this.GenerateXValues();
                return this.xValues[0];
            }
        }

        /// <summary>
        /// Gets the local minimum y value.
        /// </summary>
        public double MinY
        {
            get
            {
                this.GenerateXValues();
                return this.yValues[0];
            }


        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public double Height
        {
            get
            {
                return this.MaxY - this.MinY;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        public double Width
        {
            get
            {
                return this.MaxX - this.MinX;
            }
        }

        /// <summary>
        /// Gets the destinct y values
        /// </summary>
        public List<Double> DistinctY
        {
            get
            {
                this.GenerateYValues();
                var distinctY = new List<Double>();
                
                distinctY.AddRange(this.yValues);
                return distinctY;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle.
        /// </summary>
        /// <param name="pc">The point collection represented by the bounding box.</param>
        /// <returns>The bounding <see cref="Rect"/></returns>
        public Rect BoundingRectangle(PointCollection pc)
        {
            var rect = new Rect(this.MinX, this.MinY, this.Width, this.Height);
            return rect;
        }
    }
}
