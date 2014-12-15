namespace Steel_Bracing_2d.AcFramework
{
    using System;
    using System.Collections.Generic;

    using Autodesk.AutoCAD.Geometry;

    public class RealPolygon
    {
        #region Fields

        private double m_maxx = 0;
        private double m_maxy = 0;

        private double m_minx = 0;
        private double m_miny = 0;

        private List<Point3d> m_pts = new List<Point3d>();

        private double m_xlength = 0;
        private double m_ylength = 0;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Creates a new instance of a PolygonF based on the Point3d[].
        /// </summary>
        /// <param name="pts">The array of Point3d used to create the PolygonF.</param>
        public RealPolygon(List<Point3d> pts)
        {
            this.Points = pts;
        }

        private RealPolygon()
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Calculates the Area of the Polygon.
        /// </summary>
        public decimal Area
        {
            get
            {
                decimal xy = 0M;
                for (int i = 0; i < this.m_pts.Count; i++)
                {
                    Point3d pt1;
                    Point3d pt2;
                    if (i == this.m_pts.Count - 1)
                    {
                        pt1 = this.m_pts[i];
                        pt2 = this.m_pts[0];
                    }
                    else
                    {
                        pt1 = this.m_pts[i];
                        pt2 = this.m_pts[i + 1];
                    }
                    xy += Convert.ToDecimal(pt1.X * pt2.Y);
                    xy -= Convert.ToDecimal(pt1.Y * pt2.X);
                }

                decimal area = Convert.ToDecimal(Math.Abs(xy)) * 0.5M;

                return area;
            }
        }

        /// <summary>
        /// The Rectangular Bounds of the Polygon.
        /// </summary>
        public RealRectangle Bounds
        {
            get
            {
                return new RealRectangle(this.m_minx, this.m_miny, this.m_maxx - this.m_minx, this.m_maxy - this.m_miny);
            }
        }

        /// <summary>
        /// NOT YET IMPLEMENTED.  Currently returns the same as CenterPointOfBounds.
        /// This is intended to be the Visual Center of the Polygon, and will be implemented
        /// once I can figure out how to calculate that Point.
        /// </summary>
        public Point3d CenterPoint
        {
            get
            {
                Point3d pt = this.CenterPointOfBounds;


                return pt;
            }
        }

        /// <summary>
        /// Returns the Point3d that represents the center of the Rectangular Bounds of the Polygon.
        /// </summary>
        public Point3d CenterPointOfBounds
        {
            get
            {
                double x = this.m_minx + (this.m_xlength / 2);
                double y = this.m_miny + (this.m_ylength / 2);
                return new Point3d(x, y, 0);
            }
        }

        /// <summary>
        /// The Maximum X coordinate value in the Point3d collection.
        /// </summary>
        public double MaximumX
        {
            get { return this.m_maxx; }
        }

        /// <summary>
        /// The Maximum Y coordinate value in the Point3d collection.
        /// </summary>
        public double MaximumY
        {
            get { return this.m_maxy; }
        }

        /// <summary>
        /// The Minimum X coordinate value in the Point3d collection.
        /// </summary>
        public double MinimumX
        {
            get { return this.m_minx; }
        }

        /// <summary>
        /// The Minimum Y coordinate value in the Point3d collection.
        /// </summary>
        public double MinimumY
        {
            get { return this.m_miny; }
        }

        /// <summary>
        /// The number of Points in the Polygon.
        /// </summary>
        public int NumberOfPoints
        {
            get { return this.m_pts.Count; }
        }

        #endregion

        #region Properties

        private List<Point3d> Points
        {
            get { return this.m_pts; }
            set
            {
                this.m_pts = value;
                this.m_minx = this.m_pts[0].X;
                this.m_maxx = this.m_pts[0].X;
                this.m_miny = this.m_pts[0].Y;
                this.m_maxy = this.m_pts[0].Y;

                foreach (Point3d pt in this.m_pts)
                {
                    if (pt.X < this.m_minx)
                    {
                        this.m_minx = pt.X;
                    }

                    if (pt.X > this.m_maxx)
                    {
                        this.m_maxx = pt.X;
                    }

                    if (pt.Y < this.m_miny)
                    {
                        this.m_miny = pt.Y;
                    }

                    if (pt.Y > this.m_maxy)
                    {
                        this.m_maxy = pt.Y;
                    }
                }

                this.m_xlength = Math.Abs(this.m_maxx - this.m_minx);
                this.m_ylength = Math.Abs(this.m_maxy - this.m_miny);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Compares the supplied point and determines whether or not it is inside the Actual Bounds
        /// of the Polygon.
        /// </summary>
        /// <remarks>The calculation formula was converted from the C version available at
        /// http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
        /// </remarks>
        /// <param name="pt">The Point3d to compare.</param>
        /// <returns>True if the Point3d is within the Actual Bounds, False if it is not.</returns>
        public bool Contains(Point3d pt)
        {
            bool isIn = false;

            if (this.IsInBounds(pt))
            {
                int i, j = 0;

                // The following code is converted from a C version found at 
                // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
                for (i = 0, j = this.NumberOfPoints - 1; i < this.NumberOfPoints; j = i++)
                {
                    if (
                        (
                         ((this.m_pts[i].Y <= pt.Y) && (pt.Y < this.m_pts[j].Y)) || ((this.m_pts[j].Y <= pt.Y) && (pt.Y < this.m_pts[i].Y))
                        ) &&
                        (pt.X < (this.m_pts[j].X - this.m_pts[i].X) * (pt.Y - this.m_pts[i].Y) / (this.m_pts[j].Y - this.m_pts[i].Y) + this.m_pts[i].X)
                       )
                    {
                        isIn = !isIn;
                    }
                }
            }

            return isIn;
        }

        /// <summary>
        /// Checks polygon is inside other polygon
        /// </summary>
        /// <param name="rp"></param>
        /// <returns></returns>
        public bool Contains(RealPolygon rp)
        {
            bool retResult = true;

            foreach (Point3d pt in rp.Points)
            {
                if (this.Contains(pt)) continue;

                retResult = false;
                break;
            }

            return retResult;
        }

        /// <summary>
        /// Inflat the polygon by given offset
        /// </summary>
        /// <param name="offset"></param>
        public void Inflate(double offset)
        {
            Point3d ptCenter = this.CenterPoint;

            List<Point3d> pts = new List<Point3d>();
            foreach (Point3d pt in this.m_pts)
            {
                double ang = DwgGeometry.AngleFromXAxis(ptCenter, pt);
                Point3d ptx = DwgGeometry.GetPointPolar(pt, ang, offset);
                pts.Add(ptx);
            }

            this.Points = pts;
        }

        /// <summary>
        /// Compares the supplied point and determines whether or not it is inside the Rectangular Bounds
        /// of the Polygon.
        /// </summary>
        /// <param name="pt">The Point3d to compare.</param>
        /// <returns>True if the Point3d is within the Rectangular Bounds, False if it is not.</returns>
        public bool IsInBounds(Point3d pt)
        {
            return this.Bounds.Contains(pt);
        }

        #endregion
    }
}
