namespace Steel_Bracing_2d.DetailParts
{
    using System;
    using System.Collections.Generic;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class EqualAngleDrawCommand
    {
        #region Fields

        private readonly AngleInformation _ai = new AngleInformation();

        private readonly double _hole_center_y = 35;

        private string _centerLineLayer = "center";

        private string _continuousLineLayer = "objects";

        private string _descTextLayer = "text";

        private string _descTextStyle = "brc25";

        private bool _drawCenterLine = false;

        private string _hiddenLineLayer = "hidden";

        private double _hole_center_x = 50;

        private double _hole_dia = 20;

        private double _hole_pitch = 50;

        private int _no_of_holes = 2;

        private double _oblongOffset = 0;

        private double _plateOffset = 20;

        private string _viewType = "Front";

        #endregion

        #region Constructors and Destructors

        public EqualAngleDrawCommand()
        {
        }

        public EqualAngleDrawCommand(AngleInformation ai)
        {
            this._ai = ai;
            this._hole_center_y = this._ai.BoltLine;
        }

        #endregion

        #region Public Properties

        public string CenterLineLayer
        {
            get { return this._centerLineLayer; }
            set { this._centerLineLayer = value; }
        }

        public string ContinuousLineLayer
        {
            get { return this._continuousLineLayer; }
            set { this._continuousLineLayer = value; }
        }

        public string DescTextLayer
        {
            get { return this._descTextLayer; }
            set { this._descTextLayer = value; }
        }

        public string DescTextStyle
        {
            get { return this._descTextStyle; }
            set { this._descTextStyle = value; }
        }

        public bool DrawCenterLine
        {
            get { return this._drawCenterLine; }
            set { this._drawCenterLine = value; }
        }

        public string HiddenLineLayer
        {
            get { return this._hiddenLineLayer; }
            set { this._hiddenLineLayer = value; }
        }

        public double Hole_center_x
        {
            get { return this._hole_center_x; }
            set { this._hole_center_x = value; }
        }

        public double Hole_dia
        {
            get { return this._hole_dia; }
            set { this._hole_dia = value; }
        }

        public double Hole_pitch
        {
            get { return this._hole_pitch; }
            set { this._hole_pitch = value; }
        }

        public int No_of_holes
        {
            get { return this._no_of_holes; }
            set { this._no_of_holes = value; }
        }

        public double OblongOffset
        {
            get { return this._oblongOffset; }
            set { this._oblongOffset = value; }
        }

        public double PlateOffset
        {
            get { return this._plateOffset; }
            set { this._plateOffset = value; }
        }

        public string ViewType
        {
            get { return this._viewType; }
            set { this._viewType = value; }
        }

        #endregion

        #region Public Methods and Operators

        public void Draw()
        {
            PromptPointResult ppr = CadApplication.CurrentEditor.GetPoint("\nPick Start Point: ");
            if (ppr.Status != PromptStatus.OK) return;

            Point3d pt1 = ppr.Value;

            ////EqualAngleJig jig = new EqualAngleJig(pt1);
            ////PromptResult prAng = CADApplication.CurrentEditor.Drag(jig);
            ////if (prAng.Status != PromptStatus.OK) return;
            ////Point3d pt2 = jig.SecondPoint;


            PromptPointOptions ppo = new PromptPointOptions("\nPick Second Point: ");
            ppo.UseBasePoint = true;
            ppo.BasePoint = pt1;
            ppr = CadApplication.CurrentEditor.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;
            Point3d pt2 = ppr.Value;

            DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);

            using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
            {
                ObjectId layerId;
                if (!currDwg.IsLayerExists(this._continuousLineLayer, out layerId))
                {
                    currDwg.AddLayer("objects", 2, "continuous");
                    currDwg.AddLayer("center", 1, "center");
                    currDwg.AddLayer("hidden", 3, "hidden");
                    currDwg.AddLayer("dimension", 4, "continuous");
                    currDwg.AddLayer("text", 7, "continuous");
                }
            }
            
            if (this.ViewType == "Front")
            {
                ObjectIdCollection ids = new ObjectIdCollection();
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    Point3d p1 = DwgGeometry.GetPointPolar(pt1, ang, this._plateOffset);
                    Point3d p2 = DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad180, this._plateOffset);
                    double sectionLength = p1.DistanceTo(p2);
                    if (this._drawCenterLine)
                    {
                        Line centerLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad180, 3 * currDwg.Dimscale),
                            DwgGeometry.GetPointPolar(p2, ang, 3 * currDwg.Dimscale));
                        centerLine.Layer = this._centerLineLayer;
                        ids.Add(currDwg.AddEntity(centerLine));
                    }

                    Line topLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._ai.CGLine),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._ai.CGLine));
                    topLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(topLine));

                    Line thkLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._ai.CGLine - this._ai.Thickness),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._ai.CGLine - this._ai.Thickness));
                    thkLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(thkLine));

                    Line bottomLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad270, this._ai.Width - this._ai.CGLine),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad270, this._ai.Width - this._ai.CGLine));
                    bottomLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(bottomLine));

                    Line sideLine1 = new Line(topLine.StartPoint, bottomLine.StartPoint);
                    sideLine1.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(sideLine1));

                    Line sideLine2 = new Line(topLine.EndPoint, bottomLine.EndPoint);
                    sideLine2.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(sideLine2));

                    this.DrawHoles(bottomLine.StartPoint, p1, p2, DwgGeometry.kRad90, 0);
                    this.DrawHoles(bottomLine.EndPoint, p2, p1, DwgGeometry.kRad270, this._oblongOffset);

                    //add text here
                    DBText angDescText = new DBText();
                    angDescText.SetDatabaseDefaults(CadApplication.CurrentDatabase);
                    angDescText.TextString = "L" + this._ai.Description;
                    angDescText.SetTextStyleId(currDwg.GetTextStyleId(this._descTextStyle));
                    TextStyleTableRecord styRec = currDwg.GetObject(angDescText.TextStyleId) as TextStyleTableRecord;
                    angDescText.Layer = this._descTextLayer;
                    double textOffset = this._ai.CGLine + currDwg.Dimscale;
                    angDescText.Position = DwgGeometry.GetPointPolar(
                        DwgGeometry.GetPointPolar(p1, DwgGeometry.AngleFromXAxis(p1, p2), p1.DistanceTo(p2) * 0.25),
                        DwgGeometry.AngleFromXAxis(p1, p2) + DwgGeometry.kRad90, textOffset);
                    angDescText.Rotation = DwgGeometry.AngleFromXAxis(p1, p2);
                    angDescText.Height = styRec.TextSize;
                    currDwg.AddEntity(angDescText);

                    this.AttachSteelSectionInformation(ids, new Line(p1, p2), sectionLength);
                }
            }
            else if (this.ViewType == "Side")
            {
                ObjectIdCollection ids = new ObjectIdCollection();
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    Point3d p1 = DwgGeometry.GetPointPolar(pt1, ang, this._plateOffset);
                    Point3d p2 = DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad180, this._plateOffset);
                    double sectionLength = p1.DistanceTo(p2);

                    Line topLine = new Line(p1, p2);
                    topLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(topLine));

                    Line thkLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._ai.Thickness),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._ai.Thickness));
                    thkLine.Layer = this._hiddenLineLayer;
                    ids.Add(currDwg.AddEntity(thkLine));

                    Line bottomLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._ai.Width),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._ai.Width));
                    bottomLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(bottomLine));

                    Line sideLine1 = new Line(topLine.StartPoint, bottomLine.StartPoint);
                    sideLine1.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(sideLine1));

                    Line sideLine2 = new Line(topLine.EndPoint, bottomLine.EndPoint);
                    sideLine2.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(sideLine2));
                }
            }
            else
            {
                //top view goes here
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    List<Entity> entities = new List<Entity>();
                    Point3d pt3 = DwgGeometry.GetRectPoint(pt1, this._ai.Width, 0);
                    Line l1 = new Line(pt1, pt3);
                    entities.Add(l1);

                    Point3d pt4 = DwgGeometry.GetRectPoint(pt3, 0, this._ai.Thickness * 0.5);
                    Line l2 = new Line(pt3, pt4);
                    entities.Add(l2);

                    Point3d cpt1 = DwgGeometry.GetRectPoint(pt4, this._ai.Thickness * -0.5, 0);
                    Point3d pt5 = DwgGeometry.GetRectPoint(cpt1, 0, this._ai.Thickness * 0.5);
                    Point3d pt7 = DwgGeometry.GetRectPoint(pt1, this._ai.Thickness, this._ai.Thickness);
                    Point3d pt6 = DwgGeometry.GetRectPoint(pt7, this._ai.Thickness, 0);
                    Point3d pt8 = DwgGeometry.GetRectPoint(pt7, 0, this._ai.Thickness);
                    Point3d pt9 = DwgGeometry.GetRectPoint(pt1, this._ai.Thickness, this._ai.Width - this._ai.Thickness * 0.5);
                    Point3d cpt3 = DwgGeometry.GetRectPoint(pt9, this._ai.Thickness * -0.5, 0);
                    Point3d pt11 = DwgGeometry.GetRectPoint(pt1, 0, this._ai.Width);
                    Point3d pt10 = DwgGeometry.GetRectPoint(pt11, this._ai.Thickness * 0.5, 0);

                    Line l3 = new Line(pt5, pt6);
                    entities.Add(l3);

                    Line l4 = new Line(pt8, pt9);
                    entities.Add(l4);

                    Line l5 = new Line(pt11, pt10);
                    entities.Add(l5);

                    Line l6 = new Line(pt1, pt11);
                    entities.Add(l6);

                    Arc a1 = new Arc(cpt1, this._ai.Thickness * 0.5, 0, DwgGeometry.kRad90);
                    entities.Add(a1);

                    Arc a2 = new Arc(cpt3, this._ai.Thickness * 0.5, 0, DwgGeometry.kRad90);
                    entities.Add(a2);

                    Arc a3 = new Arc(DwgGeometry.GetRectPoint(pt7, this._ai.Thickness, this._ai.Thickness), this._ai.Thickness, DwgGeometry.kRad180, DwgGeometry.kRad270);
                    entities.Add(a3);

                    Matrix3d mat = Matrix3d.Rotation(DwgGeometry.AngleFromXAxis(pt1, pt2), new Vector3d(0, 0, 1), pt1);
                    foreach (Entity ent in entities)
                    {
                        ent.Layer = this._continuousLineLayer;
                        ent.TransformBy(mat);
                        currDwg.AddEntity(ent);
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void AttachSteelSectionInformation(ObjectIdCollection ids, Line centLine, double memberLength)
        {
            if (ids.Count > 0)
            {
                double cenLineAngle = DwgGeometry.AngleFromXAxis(centLine.StartPoint, centLine.EndPoint);
                if (cenLineAngle > DwgGeometry.kRad180) cenLineAngle -= DwgGeometry.kRad180;

                SteelSectionInformation steelSect = new SteelSectionInformation();
                steelSect.Id = Guid.NewGuid().ToString();
                steelSect.SteelMemeberLength = memberLength;
                steelSect.CenterLineX = centLine.StartPoint.X;
                steelSect.CenterLineY = centLine.StartPoint.Y;
                steelSect.CenterLineAngle = cenLineAngle;
                steelSect.HoleDiameter = this._hole_dia;
                steelSect.XHoleOffset = this._hole_center_x;
                steelSect.YHoleOffset = this._hole_center_y;
                steelSect.HolePitch = this._hole_pitch;
                steelSect.OblongOffset = this._oblongOffset;

                steelSect.MemberInfo = this._ai;
                steelSect.AngleType = 0;

                foreach (ObjectId id in ids)
                {
                    steelSect.HandleList.Add(id.Handle.Value);
                }

                DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);

                foreach (ObjectId id in ids)
                {
                    currDwg.SaveEntityData<SteelSectionInformation>(id, steelSect, DrawingDatabase.ObjectDataDictionaryKey);
                }
            }
        }

        private void DrawHoles(Point3d kpt, Point3d p1, Point3d p2, double angSide, double oblongOffset)
        {
            DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
            double ang = DwgGeometry.AngleFromXAxis(p1, p2);
            Point3d cpt = DwgGeometry.GetPointPolar(DwgGeometry.GetPointPolar(kpt, ang, this._hole_center_x),
                ang + angSide, this._hole_center_y);

            for (int k = 0; k < this._no_of_holes; k++)
            {
                if (oblongOffset > 0)
                {
                    //can we draw poly line here
                    Point3d cp1 = DwgGeometry.GetPointPolar(cpt, ang + DwgGeometry.kRad180, oblongOffset * 0.5);
                    Point3d cp2 = DwgGeometry.GetPointPolar(cpt, ang, oblongOffset * 0.5);

                    Point3d px1 = DwgGeometry.GetPointPolar(cp1, ang + DwgGeometry.kRad90, this._hole_dia * 0.5);
                    Point3d px2 = DwgGeometry.GetPointPolar(cp2, ang + DwgGeometry.kRad90, this._hole_dia * 0.5);
                    Point3d px3 = DwgGeometry.GetPointPolar(cp2, ang + DwgGeometry.kRad270, this._hole_dia * 0.5);
                    Point3d px4 = DwgGeometry.GetPointPolar(cp1, ang + DwgGeometry.kRad270, this._hole_dia * 0.5);

                    Polyline plEnt = new Polyline();

                    plEnt.AddVertexAt(0, new Point2d(px1.X, px1.Y), 0, 0, 0);
                    plEnt.AddVertexAt(1, new Point2d(px2.X, px2.Y), -1, 0, 0);
                    plEnt.AddVertexAt(2, new Point2d(px3.X, px3.Y), 0, 0, 0);
                    plEnt.AddVertexAt(3, new Point2d(px4.X, px4.Y), -1, 0, 0);
                    plEnt.Closed = true;

                    plEnt.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(plEnt);

                    Line xcLine = new Line(DwgGeometry.GetPointPolar(cp1, ang + DwgGeometry.kRad180, this._hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cp2, ang, this._hole_dia * 0.75));
                    xcLine.Layer = this._centerLineLayer;
                    currDwg.AddEntity(xcLine);

                    Line xyLine = new Line(DwgGeometry.GetPointPolar(cp1, ang + DwgGeometry.kRad90, this._hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cp1, ang + DwgGeometry.kRad270, this._hole_dia * 0.75));
                    xyLine.Layer = this._centerLineLayer;
                    currDwg.AddEntity(xyLine);

                    Line xyLine1 = new Line(DwgGeometry.GetPointPolar(cp2, ang + DwgGeometry.kRad90, this._hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cp2, ang + DwgGeometry.kRad270, this._hole_dia * 0.75));
                    xyLine1.Layer = this._centerLineLayer;
                    currDwg.AddEntity(xyLine1);
                }
                else
                {
                    Circle hole = new Circle(cpt, new Vector3d(0, 0, 1), this._hole_dia * 0.5);
                    hole.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(hole);

                    Line xcLine = new Line(DwgGeometry.GetPointPolar(cpt, ang + DwgGeometry.kRad180, this._hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cpt, ang, this._hole_dia * 0.75));
                    xcLine.Layer = this._centerLineLayer;
                    currDwg.AddEntity(xcLine);

                    Line xyLine = new Line(DwgGeometry.GetPointPolar(cpt, ang + DwgGeometry.kRad90, this._hole_dia * 0.75),
                        DwgGeometry.GetPointPolar(cpt, ang + DwgGeometry.kRad270, this._hole_dia * 0.75));
                    xyLine.Layer = this._centerLineLayer;
                    currDwg.AddEntity(xyLine);
                }

                cpt = DwgGeometry.GetPointPolar(cpt, ang, this._hole_pitch);
            }
        }

        #endregion
    }
}
