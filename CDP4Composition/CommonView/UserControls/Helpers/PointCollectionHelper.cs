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
        private readonly PointCollection pointCollection;
        private Point startPoint;
        private double[] yValues;
        private double[] xValues;
        private bool areXValuesGenerated;
        private bool areYValuesGenerated;
        private readonly Dictionary<double, List<double>> xRanges = new Dictionary<double, List<double>>();
        private readonly Dictionary<double, List<double>> yRanges = new Dictionary<double, List<double>>();
        private bool areXRangesGenerated;
        private bool areYRangesGenerated;

        /// <summary>
        /// Initializes a new <see cref="PointCollectionHelper"/>
        /// </summary>
        /// <param name="pointCollection">The initial <see cref="PointCollection"/></param>
        /// <param name="startPoint">The start <see cref="Point"/></param>
        public PointCollectionHelper(PointCollection pointCollection, Point startPoint)
        {
            this.pointCollection = pointCollection;
            this.startPoint = startPoint;
        }

        /// <summary>
        /// Ensures that the x values are computed if they havn't been already.
        /// </summary>
        private void GenerateXValues()
        {
            if (!this.areXValuesGenerated)
            {
                var distinct = new HashSet<double>();
                
                foreach (var p in this.pointCollection)
                {
                    distinct.Add(p.X);
                }

                distinct.Add(this.startPoint.X);

                this.xValues = new double[distinct.Count];
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

                foreach (var p in this.pointCollection)
                {
                    distinct.Add(p.Y);
                }

                distinct.Add(this.startPoint.Y);

                this.yValues = new double[distinct.Count];
                distinct.CopyTo(this.yValues);
                Array.Sort(this.yValues);
                
                this.areYValuesGenerated = true;
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
            
            foreach (var point in this.pointCollection)
            {
                if (point.X == x)
                {
                    yVals.Add(point.Y);
                }
            }

            if (this.startPoint.X == x && !yVals.Contains(this.startPoint.Y))
            {
                yVals.Add(this.startPoint.Y);
            }

            yVals.Sort();
            
            return yVals;
        }

        /// <summary>
        /// Generates the y ranges.
        /// </summary>
        private void GenerateYRanges()
        {
            if (!this.areYRangesGenerated)
            {
                this.GenerateYValues();
                
                foreach (var d in this.yValues)
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
        private List<double> XAtYInternal(double y)
        {
            var xVals = new List<double>();
            
            foreach (var point in this.pointCollection)
            {
                if (point.Y == y)
                {
                    if (!xVals.Contains(point.X))
                    {
                        xVals.Add(point.X);
                    }
                }
            }

            if (this.startPoint.Y == y && !xVals.Contains(this.startPoint.X))
            {
                xVals.Add(this.startPoint.X);
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

            var matches = y1Hash.Intersect(this.XAtY(y2));
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
                
                return this.xValues[this.xValues.Length - 1];
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
                    return double.NaN;
                }

                this.GenerateYValues();
                
                return this.xValues[this.yValues.Length - 1];
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
        public double Height => this.MaxY - this.MinY;

        /// <summary>
        /// Gets the width.
        /// </summary>
        public double Width => this.MaxX - this.MinX;

        /// <summary>
        /// Gets the destinct y values
        /// </summary>
        public List<double> DistinctY
        {
            get
            {
                this.GenerateYValues();
                var distinctY = new List<double>();
                
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
