namespace Steel_Bracing_2d.DetailParts
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class DrawCornerGussetPlate
    {
        #region Fields

        private CornerGusset m_cornerPlate = null;

        private DrawingDatabase m_currDwg = null;

        private Point3d m_location;

        #endregion

        #region Constructors and Destructors

        public DrawCornerGussetPlate()
        {
        }

        #endregion

        #region Public Properties

        public CornerGusset CornerPlate
        {
            get { return this.m_cornerPlate; }
            set { this.m_cornerPlate = value; }
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
            if (this.m_cornerPlate.Quadrant == 5)
            {
                Line horzLine = null;
                Line vertLine1 = null;
                Line vertLine2 = null;
                Line angLine1 = null;
                Line angLine2 = null;
                foreach (long entHandle in this.m_cornerPlate.HandleList)
                {
                    ObjectId entId = ObjectId.Null;
                    if (this.m_currDwg.HandleToObjectId(new Handle(entHandle), out entId) == false) continue;
                    Line lineSeg = this.m_currDwg.GetEntity(entId) as Line;
                    double ang1 = DwgGeometry.AngleFromXAxis(lineSeg.StartPoint, lineSeg.EndPoint);
                    double ang2 = DwgGeometry.AngleFromXAxis(lineSeg.EndPoint, lineSeg.StartPoint);
                    double cenPerAng = this.m_cornerPlate.CenterLineAngle + DwgGeometry.kRad90;
                    if (Math.Abs(cenPerAng - ang1) < 0.0001 || Math.Abs(cenPerAng - ang2) < 0.0001)
                    {
                        angLine1 = lineSeg;
                    }

                    double cenPerAng1 = this.m_cornerPlate.VerticalEdgeAngle + DwgGeometry.kRad90;
                    if (Math.Abs(cenPerAng1 - ang1) < 0.0001 || Math.Abs(cenPerAng1 - ang2) < 0.0001)
                    {
                        angLine2 = lineSeg;
                    }

                    if (Math.Abs(lineSeg.StartPoint.X - lineSeg.EndPoint.X) < 0.0001)
                    {
                        if (vertLine1 == null)
                        {
                            vertLine1 = lineSeg;
                        }
                        else
                        {
                            vertLine2 = lineSeg;
                        }
                    }

                    if (Math.Abs(lineSeg.StartPoint.Y - lineSeg.EndPoint.Y) < 0.0001)
                    {
                        horzLine = lineSeg;
                    }
                }

                if (horzLine == null || vertLine1 == null || angLine1 == null || vertLine2 == null || angLine2 == null) return;
                if (vertLine1.StartPoint.X > vertLine2.StartPoint.X)
                {
                    Line tmp = vertLine1;
                    vertLine1 = vertLine2;
                    vertLine2 = tmp;
                }

                if (angLine1.StartPoint.X > angLine2.StartPoint.X)
                {
                    Line tmp = angLine1;
                    angLine1 = angLine2;
                    angLine2 = tmp;
                }

                Line cenLine1 = new Line(new Point3d(this.m_cornerPlate.OriginX, this.m_cornerPlate.OriginY, 0), Point3d.Origin);
                cenLine1.EndPoint = DwgGeometry.GetPointPolar(cenLine1.StartPoint, this.m_cornerPlate.CenterLineAngle, this.m_cornerPlate.WeldLength);
                Line cenLine2 = new Line(new Point3d(this.m_cornerPlate.CenterLineX, this.m_cornerPlate.CenterLineY, 0), Point3d.Origin);
                cenLine2.EndPoint = DwgGeometry.GetPointPolar(cenLine2.StartPoint, this.m_cornerPlate.VerticalEdgeAngle, this.m_cornerPlate.WeldLength);

                Point3d px1 = DwgGeometry.Inters(cenLine1.StartPoint, cenLine1.EndPoint, angLine1.StartPoint, angLine1.EndPoint);
                Point3d px2 = new Point3d(this.m_cornerPlate.OriginX, this.m_cornerPlate.OriginY, 0);
                Point3d px3 = DwgGeometry.Inters(cenLine2.StartPoint, cenLine2.EndPoint, angLine2.StartPoint, angLine2.EndPoint);
                Point3d px4 = new Point3d(this.m_cornerPlate.CenterLineX, this.m_cornerPlate.CenterLineY, 0);

                Line horzLineBase = new Line(px2, px4);
                Point3d pt1 = DwgGeometry.Inters(horzLineBase.StartPoint, horzLineBase.EndPoint, vertLine1.StartPoint, vertLine1.EndPoint);
                Point3d pt2 = DwgGeometry.Inters(horzLineBase.StartPoint, horzLineBase.EndPoint, vertLine2.StartPoint, vertLine2.EndPoint);
                Point3d p1 = pt1;
                Point3d p2 = pt2;
                if (pt1.X > pt2.X)
                {
                    p1 = pt2;
                    p2 = pt1;
                }

                Point3d p3 = DwgGeometry.Inters(vertLine2.StartPoint, vertLine2.EndPoint, angLine2.StartPoint, angLine2.EndPoint);
                Point3d p4 = DwgGeometry.Inters(horzLine.StartPoint, horzLine.EndPoint, angLine2.StartPoint, angLine2.EndPoint);
                Point3d p5 = DwgGeometry.Inters(horzLine.StartPoint, horzLine.EndPoint, angLine1.StartPoint, angLine1.EndPoint);
                Point3d p6 = DwgGeometry.Inters(vertLine1.StartPoint, vertLine1.EndPoint, angLine1.StartPoint, angLine1.EndPoint);

                using (DocumentLock docLoc = CadApplication.CurrentDocument.LockDocument())
                {
                    Polyline plEnt = new Polyline();
                    plEnt.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
                    plEnt.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
                    plEnt.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
                    plEnt.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
                    plEnt.AddVertexAt(4, new Point2d(p5.X, p5.Y), 0, 0, 0);
                    plEnt.AddVertexAt(5, new Point2d(p6.X, p6.Y), 0, 0, 0);
                    plEnt.Closed = true;

                    Matrix3d transform = Matrix3d.Displacement(DwgGeometry.GetMidPoint(p1,p2).GetVectorTo(this.m_location));
                    plEnt.TransformBy(transform);
                    this.m_currDwg.AddEntity(plEnt);

                    //draw holes here for angle1
                    double ang1 = DwgGeometry.AngleFromXAxis(px2, px1);
                    Point3d cpt = DwgGeometry.GetPointPolar(px1, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.WeldLength - this.m_cornerPlate.Hole_center_x);
                    cpt = DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.AngleInfo.Width - this.m_cornerPlate.AngleInfo.CGLine - this.m_cornerPlate.AngleInfo.BoltLine);

                    for (int k = 0; k < this.m_cornerPlate.No_of_holes; k++)
                    {
                        if (this.m_cornerPlate.OblongHoleCenterOffset > 0)
                        {
                            //can we draw poly line here
                            Point3d cp1 = DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);
                            Point3d cp2 = DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);

                            Point3d q1 = DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q2 = DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q3 = DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q4 = DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);

                            Polyline plEnt1 = new Polyline();

                            plEnt1.AddVertexAt(0, new Point2d(q1.X, q1.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(1, new Point2d(q2.X, q2.Y), -1, 0, 0);
                            plEnt1.AddVertexAt(2, new Point2d(q3.X, q3.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(3, new Point2d(q4.X, q4.Y), -1, 0, 0);
                            plEnt1.Closed = true;

                            plEnt1.TransformBy(transform);
                            this.m_currDwg.AddEntity(plEnt1);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, ang1, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);

                            Line xyLine1 = new Line(DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine1.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine1);
                        }
                        else
                        {

                            Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this.m_cornerPlate.Hole_dia * 0.5);
                            hole.TransformBy(transform);
                            this.m_currDwg.AddEntity(hole);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);
                        }
                        cpt = DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.Hole_pitch);
                    }

                    //draw holes here for angle2
                    ang1 = DwgGeometry.AngleFromXAxis(px4, px3);
                    cpt = DwgGeometry.GetPointPolar(px3, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.WeldLength - this.m_cornerPlate.Hole_center_x);
                    cpt = DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.AngleInfo.Width - this.m_cornerPlate.AngleInfo.CGLine - this.m_cornerPlate.AngleInfo.BoltLine);

                    for (int k = 0; k < this.m_cornerPlate.No_of_holes; k++)
                    {
                        if (this.m_cornerPlate.OblongHoleCenterOffset > 0)
                        {
                            //can we draw poly line here
                            Point3d cp1 = DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);
                            Point3d cp2 = DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);

                            Point3d q1 = DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q2 = DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q3 = DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q4 = DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);

                            Polyline plEnt1 = new Polyline();

                            plEnt1.AddVertexAt(0, new Point2d(q1.X, q1.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(1, new Point2d(q2.X, q2.Y), -1, 0, 0);
                            plEnt1.AddVertexAt(2, new Point2d(q3.X, q3.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(3, new Point2d(q4.X, q4.Y), -1, 0, 0);
                            plEnt1.Closed = true;

                            plEnt1.TransformBy(transform);
                            this.m_currDwg.AddEntity(plEnt1);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, ang1, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp1, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);

                            Line xyLine1 = new Line(DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine1.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine1);
                        }
                        else
                        {

                            Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this.m_cornerPlate.Hole_dia * 0.5);
                            hole.TransformBy(transform);
                            this.m_currDwg.AddEntity(hole);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, ang1 + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);
                        }
                        cpt = DwgGeometry.GetPointPolar(cpt, ang1, this.m_cornerPlate.Hole_pitch);
                    }
                }
            }
            else
            {
                Line horzLine = null;
                Line vertLine = null;
                Line angLine = null;

                foreach (long entHandle in this.m_cornerPlate.HandleList)
                {
                    ObjectId entId = ObjectId.Null;
                    if (this.m_currDwg.HandleToObjectId(new Handle(entHandle), out entId) == false) continue;
                    Line lineSeg = this.m_currDwg.GetEntity(entId) as Line;
                    double ang1 = DwgGeometry.AngleFromXAxis(lineSeg.StartPoint, lineSeg.EndPoint);
                    double ang2 = DwgGeometry.AngleFromXAxis(lineSeg.EndPoint, lineSeg.StartPoint);
                    double cenPerAng = this.m_cornerPlate.CenterLineAngle + DwgGeometry.kRad90;
                    if (Math.Abs(cenPerAng - ang1) < 0.0001 || Math.Abs(cenPerAng - ang2) < 0.0001)
                    {
                        angLine = lineSeg;
                    }

                    if (Math.Abs(lineSeg.StartPoint.X - lineSeg.EndPoint.X) < 0.0001)
                    {
                        vertLine = lineSeg;
                    }

                    if (Math.Abs(lineSeg.StartPoint.Y - lineSeg.EndPoint.Y) < 0.0001)
                    {
                        horzLine = lineSeg;
                    }
                }

                if (horzLine == null || vertLine == null || angLine == null) return;
                Line cenLine = new Line(new Point3d(this.m_cornerPlate.CenterLineX, this.m_cornerPlate.CenterLineY, 0), Point3d.Origin);
                cenLine.EndPoint = DwgGeometry.GetPointPolar(cenLine.StartPoint, this.m_cornerPlate.CenterLineAngle, this.m_cornerPlate.WeldLength);
                Point3d px1 = DwgGeometry.Inters(cenLine.StartPoint, cenLine.EndPoint, angLine.StartPoint, angLine.EndPoint);
                Point3d px2 = DwgGeometry.Inters(cenLine.StartPoint, cenLine.EndPoint, horzLine.StartPoint, horzLine.EndPoint);

                using (DocumentLock docLoc = CadApplication.CurrentDocument.LockDocument())
                {
                    Point3d p1 = new Point3d(this.m_cornerPlate.OriginX, this.m_cornerPlate.OriginY, 0);
                    Point3d p2 = vertLine.StartPoint;
                    Point3d p3 = vertLine.EndPoint;
                    if (p1.DistanceTo(vertLine.StartPoint) > p1.DistanceTo(vertLine.EndPoint))
                    {
                        p2 = vertLine.EndPoint;
                        p3 = vertLine.StartPoint;
                    }

                    Point3d p4 = horzLine.StartPoint;
                    Point3d p5 = horzLine.EndPoint;
                    if (p1.DistanceTo(horzLine.StartPoint) < p1.DistanceTo(horzLine.EndPoint))
                    {
                        p4 = horzLine.EndPoint;
                        p5 = horzLine.StartPoint;
                    }

                    Polyline plEnt = new Polyline();
                    plEnt.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0, 0, 0);
                    plEnt.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0, 0, 0);
                    plEnt.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0, 0, 0);
                    plEnt.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0, 0, 0);
                    plEnt.AddVertexAt(4, new Point2d(p5.X, p5.Y), 0, 0, 0);
                    plEnt.Closed = true;

                    Matrix3d transform = Matrix3d.Displacement(p1.GetVectorTo(this.m_location));
                    plEnt.TransformBy(transform);
                    this.m_currDwg.AddEntity(plEnt);

                    //draw holes here
                    // here logic will be based on the quadrant.
                    double cenAngle = DwgGeometry.AngleFromXAxis(px1, px2);
                    Point3d cpt = DwgGeometry.GetPointPolar(px1, cenAngle + DwgGeometry.kRad180, this.m_cornerPlate.WeldLength);
                    if (this.m_cornerPlate.Quadrant == 1 || this.m_cornerPlate.Quadrant == 4)
                    {
                        cpt = DwgGeometry.GetPointPolar(cpt, cenAngle - DwgGeometry.kRad90,
                            this.m_cornerPlate.AngleInfo.Width - this.m_cornerPlate.AngleInfo.CGLine - this.m_cornerPlate.AngleInfo.BoltLine);
                    }
                    else if (this.m_cornerPlate.Quadrant == 2 || this.m_cornerPlate.Quadrant == 3)
                    {
                        cpt = DwgGeometry.GetPointPolar(cpt, cenAngle + DwgGeometry.kRad90,
                            this.m_cornerPlate.AngleInfo.Width - this.m_cornerPlate.AngleInfo.CGLine - this.m_cornerPlate.AngleInfo.BoltLine);
                    }

                    cpt = DwgGeometry.GetPointPolar(cpt, cenAngle, this.m_cornerPlate.Hole_center_x);

                    for (int k = 0; k < this.m_cornerPlate.No_of_holes; k++)
                    {
                        if (this.m_cornerPlate.OblongHoleCenterOffset > 0 && (this.m_cornerPlate.Quadrant == 4 || this.m_cornerPlate.Quadrant == 3))
                        {
                            //can we draw poly line here
                            Point3d cp1 = DwgGeometry.GetPointPolar(cpt, cenAngle + DwgGeometry.kRad180, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);
                            Point3d cp2 = DwgGeometry.GetPointPolar(cpt, cenAngle, this.m_cornerPlate.OblongHoleCenterOffset * 0.5);

                            Point3d q1 = DwgGeometry.GetPointPolar(cp1, cenAngle + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q2 = DwgGeometry.GetPointPolar(cp2, cenAngle + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q3 = DwgGeometry.GetPointPolar(cp2, cenAngle + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);
                            Point3d q4 = DwgGeometry.GetPointPolar(cp1, cenAngle + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.5);

                            Polyline plEnt1 = new Polyline();

                            plEnt1.AddVertexAt(0, new Point2d(q1.X, q1.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(1, new Point2d(q2.X, q2.Y), -1, 0, 0);
                            plEnt1.AddVertexAt(2, new Point2d(q3.X, q3.Y), 0, 0, 0);
                            plEnt1.AddVertexAt(3, new Point2d(q4.X, q4.Y), -1, 0, 0);
                            plEnt1.Closed = true;

                            plEnt1.TransformBy(transform);
                            this.m_currDwg.AddEntity(plEnt1);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cp1, cenAngle + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, cenAngle, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cp1, cenAngle + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp1, cenAngle + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);

                            Line xyLine1 = new Line(DwgGeometry.GetPointPolar(cp2, cenAngle + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cp2, cenAngle + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine1.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine1);
                        }
                        else
                        {

                            Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this.m_cornerPlate.Hole_dia * 0.5);
                            hole.TransformBy(transform);
                            this.m_currDwg.AddEntity(hole);

                            Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, cenAngle + DwgGeometry.kRad180, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, cenAngle, this.m_cornerPlate.Hole_dia * 0.75));
                            xcLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xcLine);

                            Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, cenAngle + DwgGeometry.kRad90, this.m_cornerPlate.Hole_dia * 0.75),
                                DwgGeometry.GetPointPolar(cpt, cenAngle + DwgGeometry.kRad270, this.m_cornerPlate.Hole_dia * 0.75));
                            xyLine.TransformBy(transform);
                            this.m_currDwg.AddEntity(xyLine);
                        }
                        cpt = DwgGeometry.GetPointPolar(cpt, cenAngle, this.m_cornerPlate.Hole_pitch);
                    }
                }
            }
        }

        #endregion
    }
}