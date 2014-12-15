namespace Steel_Bracing_2d
{
    using System;
    using System.Collections.Generic;

    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// class for rectangle with double values, we have build-in rectangle with integer and float.
    /// </summary>
    public struct RealRectangle
    {
        #region Static Fields

        public static readonly RealRectangle Empty;

        #endregion

        #region Fields

        private double m_height;

        private double m_width;

        private double m_x;
        private double m_y;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public RealRectangle(double x, double y, double width, double height)
        {
            this.m_x = x;
            this.m_y = y;
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        public RealRectangle(Point3d pt1, Point3d pt2)
        {
            // Normalize the rectangle.
            if (pt1.X < pt2.X)
            {
                this.m_x = pt1.X;
                this.m_width = pt2.X - pt1.X;
            }
            else
            {
                this.m_x = pt2.X;
                this.m_width = pt1.X - pt2.X;
            }

            if (pt1.Y < pt2.Y)
            {
                this.m_y = pt1.Y;
                this.m_height = pt2.Y - pt1.Y;
            }
            else
            {
                this.m_y = pt2.Y;
                this.m_height = pt1.Y - pt2.Y;
            }
        }

        #endregion

        #region Public Properties

        public double Height
        {
            get
            {
                return this.m_height;
            }
            set
            {
                this.m_height = value;
            }
        }

        public Point3d MaxPoint
        {
            get
            {
                return new Point3d(this.m_x + this.m_width, this.m_y + this.m_height, 0);
            }
        }

        public Point3d MinPoint
        {
            get
            {
                return new Point3d(this.m_x, this.m_y, 0);
            }
        }

        public double Width
        {
            get
            {
                return this.m_width;
            }
            set
            {
                this.m_width = value;
            }
        }

        public double X
        {
            get
            {
                return this.m_x;
            }
            set
            {
                this.m_x = value;
            }
        }

        public double Y
        {
            get
            {
                return this.m_y;
            }
            set
            {
                this.m_y = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static RealRectangle Inflate(RealRectangle rect, double x, double y)
        {
            RealRectangle ef = rect;
            ef.Inflate(x, y);
            return ef;
        }

        public static RealRectangle Intersect(RealRectangle a, RealRectangle b)
        {
            double x = Math.Max(a.X, b.X);
            double num2 = Math.Min((double)(a.X + a.Width), (double)(b.X + b.Width));
            double y = Math.Max(a.Y, b.Y);
            double num4 = Math.Min((double)(a.Y + a.Height), (double)(b.Y + b.Height));
            if ((num2 >= x) && (num4 >= y))
            {
                return new RealRectangle(x, y, num2 - x, num4 - y);
            }
            return Empty;
        }

        public static RealRectangle Union(RealRectangle a, RealRectangle b)
        {
            double x = Math.Min(a.X, b.X);
            double num2 = Math.Max((double)(a.X + a.Width), (double)(b.X + b.Width));
            double y = Math.Min(a.Y, b.Y);
            double num4 = Math.Max((double)(a.Y + a.Height), (double)(b.Y + b.Height));
            return new RealRectangle(x, y, num2 - x, num4 - y);
        }

        public static bool operator ==(RealRectangle left, RealRectangle right)
        {
            return ((((left.X == right.X) && (left.Y == right.Y)) && (left.Width == right.Width)) && (left.Height == right.Height));
        }

        public static bool operator !=(RealRectangle left, RealRectangle right)
        {
            return !(left == right);
        }

        public bool Contains(double x, double y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        /// <summary>
        /// check the point is inside rectangle or not
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool Contains(Point3d pt)
        {
            return this.Contains(pt.X, pt.Y);
        }

        /// <summary>
        /// check the rectangle is inside rectangle or not
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool Contains(RealRectangle rect)
        {
            return ((((this.X <= rect.X) && ((rect.X + rect.Width) <= (this.X + this.Width))) && (this.Y <= rect.Y)) && ((rect.Y + rect.Height) <= (this.Y + this.Height)));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RealRectangle))
            {
                return false;
            }
            RealRectangle ef = (RealRectangle)obj;
            return ((((ef.X == this.X) && (ef.Y == this.Y)) && (ef.Width == this.Width)) && (ef.Height == this.Height));
        }

        public override int GetHashCode()
        {
            return (int)(((((uint)this.X) ^ ((((uint)this.Y) << 13) | (((uint)this.Y) >> 0x13))) ^ ((((uint)this.Width) << 0x1a) | (((uint)this.Width) >> 6))) ^ ((((uint)this.Height) << 7) | (((uint)this.Height) >> 0x19)));
        }

        /// <summary>
        /// get the indices of overlapping edges
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public List<int> GetOverlappingEdges(RealRectangle rect)
        {
            List<int> retResult = new List<int>();

            Point2d pt1 = new Point2d(this.m_x, this.m_y);
            Point2d pt2 = new Point2d(this.m_x + this.m_width, this.m_y);
            Point2d pt3 = new Point2d(this.m_x + this.m_width, this.m_y + this.m_height);
            Point2d pt4 = new Point2d(this.m_x, this.m_y + this.m_height);

            LineSegment2d[] lineSegmentList = new LineSegment2d[4];

            lineSegmentList[0] = new LineSegment2d(pt1, pt2);
            lineSegmentList[1] = new LineSegment2d(pt2, pt3);
            lineSegmentList[2] = new LineSegment2d(pt3, pt4);
            lineSegmentList[3] = new LineSegment2d(pt4, pt1);

            pt1 = new Point2d(rect.m_x, rect.m_y);
            pt2 = new Point2d(rect.m_x + rect.m_width, rect.m_y);
            pt3 = new Point2d(rect.m_x + rect.m_width, rect.m_y + rect.m_height);
            pt4 = new Point2d(rect.m_x, rect.m_y + rect.m_height);

            LineSegment2d[] lineSegmentListTarget = new LineSegment2d[4];

            lineSegmentListTarget[0] = new LineSegment2d(pt1, pt2);
            lineSegmentListTarget[1] = new LineSegment2d(pt2, pt3);
            lineSegmentListTarget[2] = new LineSegment2d(pt3, pt4);
            lineSegmentListTarget[3] = new LineSegment2d(pt4, pt1);


            foreach (LineSegment2d segSrc in lineSegmentList)
            {
                int k = 1;
                foreach (LineSegment2d segTarget in lineSegmentListTarget)
                {
                    if (segSrc.Overlap(segTarget, new Tolerance(0.001, 0.001)) != null)
                    {
                        retResult.Add(k);
                        break;
                    }

                    k++;
                }
            }

            return retResult;
        }

        /// <summary>
        /// get the indices of overlapping edges
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public List<int> GetOverlappingEdges(Point2d ptModuleLocation)
        {
            List<int> retResult = new List<int>();

            Point2d pt1 = new Point2d(this.m_x, this.m_y);
            Point2d pt2 = new Point2d(this.m_x + this.m_width, this.m_y);
            Point2d pt3 = new Point2d(this.m_x + this.m_width, this.m_y + this.m_height);
            Point2d pt4 = new Point2d(this.m_x, this.m_y + this.m_height);

            LineSegment2d[] lineSegmentList = new LineSegment2d[4];

            lineSegmentList[0] = new LineSegment2d(pt1, pt2);
            lineSegmentList[1] = new LineSegment2d(pt2, pt3);
            lineSegmentList[2] = new LineSegment2d(pt3, pt4);
            lineSegmentList[3] = new LineSegment2d(pt4, pt1);

            int k = 1;
            foreach (LineSegment2d segSrc in lineSegmentList)
            {
                Point2d mPt = new Point2d((segSrc.StartPoint.X + segSrc.EndPoint.X) / 2, (segSrc.StartPoint.Y + segSrc.EndPoint.Y) / 2);
                LineSegment2d segTarget = new LineSegment2d(ptModuleLocation, mPt);
                if (segSrc.Overlap(segTarget, new Tolerance(0.001, 0.001)) != null)
                {
                    retResult.Add(k);
                }

                k++;
            }

            return retResult;
        }

        public void Inflate(double x, double y)
        {
            this.X -= x;
            this.Y -= y;
            this.Width += 2f * x;
            this.Height += 2f * y;
        }

        public void Inflate(double left, double bottom, double right, double top)
        {
            this.X -= left;
            this.Y -= bottom;
            this.Width += right;
            this.Height += top;
        }

        public void Intersect(RealRectangle rect)
        {
            RealRectangle ef = Intersect(rect, this);
            this.X = ef.X;
            this.Y = ef.Y;
            this.Width = ef.Width;
            this.Height = ef.Height;
        }

        public bool IntersectsWith(RealRectangle rect)
        {
            return ((((rect.X < (this.X + this.Width)) && (this.X < (rect.X + rect.Width))) && (rect.Y < (this.Y + this.Height))) && (this.Y < (rect.Y + rect.Height)));
        }

        /// <summary>
        /// Check rectangle is sharing at least one side or not.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool IsAdjacent(RealRectangle rect)
        {
            Point2d pt1 = new Point2d(this.m_x, this.m_y);
            Point2d pt2 = new Point2d(this.m_x + this.m_width, this.m_y);
            Point2d pt3 = new Point2d(this.m_x + this.m_width, this.m_y + this.m_height);
            Point2d pt4 = new Point2d(this.m_x, this.m_y + this.m_height);

            LineSegment2d[] lineSegmentList = new LineSegment2d[4];

            lineSegmentList[0] = new LineSegment2d(pt1, pt2);
            lineSegmentList[1] = new LineSegment2d(pt2, pt3);
            lineSegmentList[2] = new LineSegment2d(pt3, pt4);
            lineSegmentList[3] = new LineSegment2d(pt4, pt1);

            pt1 = new Point2d(rect.m_x, rect.m_y);
            pt2 = new Point2d(rect.m_x + rect.m_width, rect.m_y);
            pt3 = new Point2d(rect.m_x + rect.m_width, rect.m_y + rect.m_height);
            pt4 = new Point2d(rect.m_x, rect.m_y + rect.m_height);

            LineSegment2d[] lineSegmentListTarget = new LineSegment2d[4];

            lineSegmentListTarget[0] = new LineSegment2d(pt1, pt2);
            lineSegmentListTarget[1] = new LineSegment2d(pt2, pt3);
            lineSegmentListTarget[2] = new LineSegment2d(pt3, pt4);
            lineSegmentListTarget[3] = new LineSegment2d(pt4, pt1);

            foreach (LineSegment2d segSrc in lineSegmentList)
            {
                foreach (LineSegment2d segTarget in lineSegmentListTarget)
                {
                    if (segSrc.Overlap(segTarget) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Offset(double x, double y)
        {
            this.X += x;
            this.Y += y;
        }

        #endregion
    }
}
