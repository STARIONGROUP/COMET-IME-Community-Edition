// -------------------------------------------------------------------------------------------------
// <copyright file="StackedRectangleGeometryHelper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Helps generate geometry paths for decorators.
    /// </summary>
    public class StackedRectangleGeometryHelper
    {
        /// <summary>
        /// The original geometry.
        /// </summary>
        private readonly Geometry orginalGeometry;

        public StackedRectangleGeometryHelper(Geometry geom)
        {
            this.orginalGeometry = geom;
        }

        /// <summary>
        /// Creates List of geometries for underline
        /// </summary>
        /// <returns>List of Geometries</returns>
        public List<Geometry> BottomEdgeRectangleGeometries()
        {
            var geometries = new List<Geometry>();

            var pathGeometry = (PathGeometry)orginalGeometry;

            foreach (var figure in pathGeometry.Figures)
            {
                var polyLineSegment = (PolyLineSegment)figure.Segments[0];

                var pointCollectionHelper = new PointCollectionHelper(polyLineSegment.Points, figure.StartPoint);
                List<double> distinctY = pointCollectionHelper.DistinctY;

                for (int i = 0; i < distinctY.Count - 1; i++)
                {
                    var bottom = distinctY[i + 1] - 3;
                    var top = bottom + 2;

                    // ordered values of X that are present for both Y values
                    var matches = pointCollectionHelper.XAtY(distinctY[i], distinctY[i + 1]);
                    var left = matches[0];
                    var right = matches[matches.Count - 1];

                    var geometry = CreateGeometry(top, bottom, left, right);
                    geometries.Add(geometry);
                }
            }

            return geometries;
        }

        /// <summary>
        /// Creates List of geometries for Strikethrough
        /// </summary>
        /// <returns>List of Geometries</returns>
        public List<Geometry> CenterLineRectangleGeometries()
        {
            var geometries = new List<Geometry>();
            var pathGeometry = (PathGeometry)orginalGeometry;

            foreach (var pathFigure in pathGeometry.Figures)
            {
                var polyLineSegment = (PolyLineSegment)pathFigure.Segments[0];
                var pointCollectionHelper = new PointCollectionHelper(polyLineSegment.Points, pathFigure.StartPoint);
                
                var distinctY = pointCollectionHelper.DistinctY;
                
                for (int i = 0; i < distinctY.Count - 1; i++)
                {
                    var top = (distinctY[i] + distinctY[i + 1]) / 2 + 1;
                    var bottom = (distinctY[i] + distinctY[i + 1]) / 2 - 1;
                    
                    // ordered values of X that are present for both Y values
                    var matches = pointCollectionHelper.XAtY(distinctY[i], distinctY[i + 1]);
                    var left = matches[0];
                    var right = matches[matches.Count - 1];

                    var geometry = CreateGeometry(top, bottom, left, right);
                    geometries.Add(geometry);
                }
            }
            return geometries;
        }

        /// <summary>
        /// Creates the <see cref="Geometry"/> object from corners.
        /// </summary>
        /// <param name="top">The top corner.</param>
        /// <param name="bottom">The bottom corner.</param>
        /// <param name="left">The left corner.</param>
        /// <param name="right">The right corner.</param>
        /// <returns>The resulting path.</returns>
        private static PathGeometry CreateGeometry(double top, double bottom, double left, double right)
        {
            var polyLineSegment = new PolyLineSegment();

            polyLineSegment.Points.Add(new Point(right, top));
            polyLineSegment.Points.Add(new Point(right, bottom));
            polyLineSegment.Points.Add(new Point(left, bottom));

            var pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(left, top);
            pathFigure.Segments.Add(polyLineSegment);
            pathFigure.IsClosed = true;
            
            var geometry = new PathGeometry();
            geometry.Figures.Add(pathFigure);
            
            return geometry;
        }
    }
}
