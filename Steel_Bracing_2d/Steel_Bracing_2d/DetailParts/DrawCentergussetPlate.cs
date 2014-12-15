namespace Steel_Bracing_2d.DetailParts
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class DrawCenterGussetPlate
    {
        #region Fields

        private CenterGusset m_centerPlate = null;

        private DrawingDatabase m_currDwg = null;

        private Point3d m_location;

        #endregion

        #region Constructors and Destructors

        public DrawCenterGussetPlate()
        {
        }

        #endregion

        #region Public Properties

        public CenterGusset CenterPlate
        {
            get { return this.m_centerPlate; }
            set { this.m_centerPlate = value; }
        }

        public DrawingDatabase CurrDwg
        {
            get { return this.m_currDwg; }
            set { this.m_currDwg = value; }
        }

        public Point3d Location
        {
            get { return this.m_location; }
            set { this.m_location = value; }
        }

        #endregion

        #region Public Methods and Operators

        public void DrawPart()
        {
            Line edge1 = null;
            Line edge2 = null;

            foreach (long entHandle in this.m_centerPlate.HandleList)
            {
                ObjectId entId = ObjectId.Null;
                if (this.m_currDwg.HandleToObjectId(new Handle(entHandle), out entId) == false) continue;
                Line lineSeg = this.m_currDwg.GetEntity(entId) as Line;
                double ang1 = DwgGeometry.AngleFromXAxis(lineSeg.StartPoint, lineSeg.EndPoint);
                double ang2 = DwgGeometry.AngleFromXAxis(lineSeg.EndPoint, lineSeg.StartPoint);
                double cenPerAng = this.m_centerPlate.CenterLineAngle + DwgGeometry.kRad90;
                if (Math.Abs(cenPerAng - ang1) < 0.0001 || Math.Abs(cenPerAng - ang2) < 0.0001)
                {
                    if (edge1 == null)
                    {
                        edge1 = lineSeg;
                    }
                    else
                    {
                        edge2 = lineSeg;
                    }
                }
            }

            if (edge1 == null || edge2 == null) return;
            Line cenLine = new Line(new Point3d(this.m_centerPlate.CenterLineX, this.m_centerPlate.CenterLineY, 0), Point3d.Origin);
            cenLine.EndPoint = DwgGeometry.GetPointPolar(cenLine.StartPoint, this.m_centerPlate.CenterLineAngle, this.m_centerPlate.WeldLength);
            Point3d px1 = DwgGeometry.Inters(cenLine.StartPoint, cenLine.EndPoint, edge1.StartPoint, edge1.EndPoint);
            Point3d px2 = DwgGeometry.Inters(cenLine.StartPoint, cenLine.EndPoint, edge2.StartPoint, edge2.EndPoint);

            double plateLength = px1.DistanceTo(px2);

            using (DocumentLock docLoc = CadApplication.CurrentDocument.LockDocument())
            {
                Point3d ptOrigin = Point3d.Origin;
                Point3d pt1 = new Point3d(0, this.m_centerPlate.AngleInfo.CGLine + this.m_centerPlate.PlateExtendDistance + this.m_centerPlate.ChamferDistance, 0);
                Point3d pt2 = new Point3d(0, -1 * ((this.m_centerPlate.AngleInfo.Width - this.m_centerPlate.AngleInfo.CGLine) + this.m_centerPlate.PlateExtendDistance + this.m_centerPlate.ChamferDistance), 0);
                Point3d pt11 = new Point3d(plateLength, this.m_centerPlate.AngleInfo.CGLine + this.m_centerPlate.PlateExtendDistance + this.m_centerPlate.ChamferDistance, 0);
                Point3d pt22 = new Point3d(plateLength, -1 * ((this.m_centerPlate.AngleInfo.Width - this.m_centerPlate.AngleInfo.CGLine) + this.m_centerPlate.PlateExtendDistance + this.m_centerPlate.ChamferDistance), 0);

                Point3d p1 = DwgGeometry.GetPointPolar(pt1, DwgGeometry.kRad270, this.m_centerPlate.ChamferDistance);
                Point3d p2 = DwgGeometry.GetPointPolar(pt1, 0, this.m_centerPlate.ChamferDistance);

                Point3d p3 = DwgGeometry.GetPointPolar(pt11, DwgGeometry.kRad180, this.m_centerPlate.ChamferDistance);
                Point3d p4 = DwgGeometry.GetPointPolar(pt11, DwgGeometry.kRad270, this.m_centerPlate.ChamferDistance);

                Point3d p5 = DwgGeometry.GetPointPolar(pt22, DwgGeometry.kRad90, this.m_centerPlate.ChamferDistance);
                Point3d p6 = DwgGeometry.GetPointPolar(pt22, DwgGeometry.kRad180, this.m_centerPlate.ChamferDistance);

                Point3d p7 = DwgGeometry.GetPointPolar(pt2, 0, this.m_centerPlate.ChamferDistance);
                Point3d p8 = DwgGeometry.GetPointPolar(pt2, DwgGeometry.kRad90, this.m_centerPlate.ChamferDistance);

                Polyline plEnt = new Polyline();
                plEnt.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
                plEnt.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
                plEnt.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
                plEnt.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
                plEnt.AddVertexAt(4, new Point2d(p5.X, p5.Y), 0, 0, 0);
                plEnt.AddVertexAt(5, new Point2d(p6.X, p6.Y), 0, 0, 0);
                plEnt.AddVertexAt(6, new Point2d(p7.X, p7.Y), 0, 0, 0);
                plEnt.AddVertexAt(7, new Point2d(p8.X, p8.Y), 0, 0, 0);
                plEnt.Closed = true;

                Matrix3d transform = Matrix3d.Displacement(Point3d.Origin.GetVectorTo(this.m_location));
                plEnt.TransformBy(transform);
                this.m_currDwg.AddEntity(plEnt);

                //draw holes here

                Point3d cpt = new Point3d(this.m_centerPlate.WeldLength - this.m_centerPlate.Hole_center_x,
                    (this.m_centerPlate.AngleInfo.Width - this.m_centerPlate.AngleInfo.BoltLine - this.m_centerPlate.AngleInfo.CGLine) * -1.0 , 0);

                for (int k = 0; k < this.m_centerPlate.No_of_holes; k++)
                {
                    if (this.m_centerPlate.OblongHoleCenterOffset > 0)
                    {
                        //can we draw poly line here
                        Point3d cp1 = DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad180, this.m_centerPlate.OblongHoleCenterOffset * 0.5);
                        Point3d cp2 = DwgGeometry.GetPointPolar(cpt, 0, this.m_centerPlate.OblongHoleCenterOffset * 0.5);

                        Point3d q1 = DwgGeometry.GetPointPolar(cp1, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.5);
                        Point3d q2 = DwgGeometry.GetPointPolar(cp2, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.5);
                        Point3d q3 = DwgGeometry.GetPointPolar(cp2, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.5);
                        Point3d q4 = DwgGeometry.GetPointPolar(cp1, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.5);

                        Polyline plEnt1 = new Polyline();

                        plEnt1.AddVertexAt(0, new Point2d(q1.X, q1.Y), 0, 0, 0);
                        plEnt1.AddVertexAt(1, new Point2d(q2.X, q2.Y), -1, 0, 0);
                        plEnt1.AddVertexAt(2, new Point2d(q3.X, q3.Y), 0, 0, 0);
                        plEnt1.AddVertexAt(3, new Point2d(q4.X, q4.Y), -1, 0, 0);
                        plEnt1.Closed = true;

                        plEnt1.TransformBy(transform);
                        this.m_currDwg.AddEntity(plEnt1);

                        Line xcLine = new Line(DwgGeometry.GetPointPolar(cp1, DwgGeometry.kRad180, this.m_centerPlate.Hole_dia * 0.75),
                            DwgGeometry.GetPointPolar(cp2, 0, this.m_centerPlate.Hole_dia * 0.75));
                        xcLine.TransformBy(transform);
                        this.m_currDwg.AddEntity(xcLine);

                        Line xyLine = new Line(DwgGeometry.GetPointPolar(cp1, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.75),
                            DwgGeometry.GetPointPolar(cp1, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.75));
                        xyLine.TransformBy(transform);
                        this.m_currDwg.AddEntity(xyLine);

                        Line xyLine1 = new Line(DwgGeometry.GetPointPolar(cp2, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.75),
                            DwgGeometry.GetPointPolar(cp2, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.75));
                        xyLine1.TransformBy(transform);
                        this.m_currDwg.AddEntity(xyLine1);
                    }
                    else
                    {

                        Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this.m_centerPlate.Hole_dia * 0.5);
                        hole.TransformBy(transform);
                        this.m_currDwg.AddEntity(hole);

                        Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad180, this.m_centerPlate.Hole_dia * 0.75),
                            DwgGeometry.GetPointPolar(cpt, 0, this.m_centerPlate.Hole_dia * 0.75));
                        xcLine.TransformBy(transform);
                        this.m_currDwg.AddEntity(xcLine);

                        Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.75),
                            DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.75));
                        xyLine.TransformBy(transform);
                        this.m_currDwg.AddEntity(xyLine);
                    }
                    cpt = DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad180, this.m_centerPlate.Hole_pitch);
                }

                cpt = new Point3d(plateLength - this.m_centerPlate.WeldLength + this.m_centerPlate.Hole_center_x,
                        (this.m_centerPlate.AngleInfo.Width - this.m_centerPlate.AngleInfo.BoltLine - this.m_centerPlate.AngleInfo.CGLine) * -1.0, 0);

                for (int k = 0; k < this.m_centerPlate.No_of_holes; k++)
                {
                    Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this.m_centerPlate.Hole_dia * 0.5);
                    hole.TransformBy(transform);
                    this.m_currDwg.AddEntity(hole);

                    Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad180, this.m_centerPlate.Hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cpt, 0, this.m_centerPlate.Hole_dia * 0.75));
                    xcLine.TransformBy(transform);
                    this.m_currDwg.AddEntity(xcLine);

                    Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad90, this.m_centerPlate.Hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cpt, DwgGeometry.kRad270, this.m_centerPlate.Hole_dia * 0.75));
                    xyLine.TransformBy(transform);
                    this.m_currDwg.AddEntity(xyLine);

                    cpt = DwgGeometry.GetPointPolar(cpt, 0, this.m_centerPlate.Hole_pitch);
                }
            }
        }

        #endregion
    }
}
