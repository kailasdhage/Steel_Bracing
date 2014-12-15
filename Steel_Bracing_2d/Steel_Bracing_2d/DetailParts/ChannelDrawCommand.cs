namespace Steel_Bracing_2d.DetailParts
{
    using System.Collections.Generic;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class ChannelDrawCommand
    {
        #region Fields

        private readonly ChannelInformation _ci = new ChannelInformation();

        private string _centerLineLayer = "center";

        private string _continuousLineLayer = "objects";

        private string _descTextLayer = "text";

        private string _descTextStyle = "brc25";

        private bool _drawCenterLine = false;

        private string _hiddenLineLayer = "hidden";

        private string _viewType = "Front";

        #endregion

        #region Constructors and Destructors

        public ChannelDrawCommand()
        {
        }

        public ChannelDrawCommand(ChannelInformation ai)
        {
            this._ci = ai;
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
            //check layer exists
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
            if (this._viewType == "Front")
            {
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    if (this._drawCenterLine)
                    {
                        Line centerLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad180, 3 * currDwg.Dimscale),
                            DwgGeometry.GetPointPolar(pt2, ang, 3 * currDwg.Dimscale));
                        centerLine.Layer = this._centerLineLayer;
                        currDwg.AddEntity(centerLine);
                    }

                    Line topLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this._ci.Depth * 0.5),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this._ci.Depth * 0.5));
                    topLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(topLine);

                    Line thkLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, (this._ci.Depth * 0.5) - this._ci.FlangeThickness),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, (this._ci.Depth * 0.5) - this._ci.FlangeThickness));
                    thkLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(thkLine);

                    Line thkLine1 = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad270, (this._ci.Depth * 0.5) - this._ci.FlangeThickness),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad270, (this._ci.Depth * 0.5) - this._ci.FlangeThickness));
                    thkLine1.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(thkLine1);

                    Line bottomLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad270, this._ci.Depth * 0.5),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad270, this._ci.Depth * 0.5));
                    bottomLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(bottomLine);

                    Line edge1 = new Line(topLine.StartPoint, bottomLine.StartPoint);
                    edge1.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(edge1);

                    Line edge2 = new Line(topLine.EndPoint, bottomLine.EndPoint);
                    edge2.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(edge2);

                    //add text here
                    DBText angDescText = new DBText();
                    angDescText.SetDatabaseDefaults(CadApplication.CurrentDatabase);
                    angDescText.TextString = "ISMC " + this._ci.Description;
                    angDescText.SetTextStyleId(currDwg.GetTextStyleId(this._descTextStyle));
                    TextStyleTableRecord styRec = currDwg.GetObject(angDescText.TextStyleId) as TextStyleTableRecord;
                    angDescText.Layer = this._descTextLayer;
                    double textOffset = (this._ci.Depth * 0.5) + currDwg.Dimscale;
                    angDescText.Position = DwgGeometry.GetPointPolar(
                        DwgGeometry.GetPointPolar(pt1, DwgGeometry.AngleFromXAxis(pt1, pt2), pt1.DistanceTo(pt2) * 0.25),
                        DwgGeometry.AngleFromXAxis(pt1, pt2) + DwgGeometry.kRad90, textOffset);
                    angDescText.Rotation = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    angDescText.Height = styRec.TextSize;
                    currDwg.AddEntity(angDescText);
                }
            }
            else if (this._viewType == "Side")
            {
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    Line topLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this._ci.Breadth),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this._ci.Breadth));
                    topLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(topLine);

                    Line thkLine1 = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this._ci.WebThickness),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this._ci.WebThickness));
                    thkLine1.Layer = this._hiddenLineLayer;
                    currDwg.AddEntity(thkLine1);

                    Line bottomLine = new Line(pt1, pt2);
                    bottomLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(bottomLine);

                    Line edge1 = new Line(topLine.StartPoint, bottomLine.StartPoint);
                    edge1.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(edge1);

                    Line edge2 = new Line(topLine.EndPoint, bottomLine.EndPoint);
                    edge2.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(edge2);
                }
            }
            else
            {
                //top view goes here
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    Point3d ptRotation = pt2;

                    pt2 = DwgGeometry.GetRectPoint(pt1, this._ci.Depth, 0);
                    Point3d pt3 = DwgGeometry.GetRectPoint(pt1, this._ci.Depth, this._ci.Breadth);
                    Point3d pt4 = DwgGeometry.GetRectPoint(pt3, this._ci.FlangeThickness * -0.5, 0);
                    Point3d cpt1 = DwgGeometry.GetRectPoint(pt4, 0, this._ci.FlangeThickness * -0.5);
                    Point3d pt5 = DwgGeometry.GetRectPoint(pt4, this._ci.FlangeThickness * -0.5, this._ci.FlangeThickness * -0.5);
                    Point3d pt6 = DwgGeometry.GetRectPoint(pt1, this._ci.Depth - this._ci.FlangeThickness, 2 * this._ci.FlangeThickness);
                    Point3d pt7 = DwgGeometry.GetRectPoint(pt6, this._ci.FlangeThickness * -1.0, this._ci.FlangeThickness * -1.0);
                    Point3d cpt2 = DwgGeometry.GetRectPoint(pt7, 0, this._ci.FlangeThickness);
                    Point3d pt8 = DwgGeometry.GetRectPoint(pt1, this._ci.FlangeThickness * 2.0, this._ci.FlangeThickness);
                    Point3d cpt3 = DwgGeometry.GetRectPoint(pt8, 0, this._ci.FlangeThickness);

                    Point3d pt12 = DwgGeometry.GetRectPoint(pt1, 0, this._ci.Breadth);
                    Point3d pt11 = DwgGeometry.GetRectPoint(pt12, this._ci.FlangeThickness * 0.5, 0);
                    Point3d pt9 = DwgGeometry.GetRectPoint(pt11, this._ci.FlangeThickness * 0.5, this._ci.FlangeThickness * -0.5);
                    Point3d pt10 = DwgGeometry.GetRectPoint(pt1, this._ci.FlangeThickness, this._ci.FlangeThickness * 2);
                    Point3d cpt4 = DwgGeometry.GetRectPoint(pt9, this._ci.FlangeThickness * -0.5, 0);

                    List<Entity> entities = new List<Entity>();
                    entities.Add(new Line(pt1, pt2));
                    entities.Add(new Line(pt2, pt3));
                    entities.Add(new Line(pt3, pt4));
                    entities.Add(new Line(pt5, pt6));
                    entities.Add(new Line(pt7, pt8));
                    entities.Add(new Line(pt9, pt10));
                    entities.Add(new Line(pt11, pt12));
                    entities.Add(new Line(pt12, pt1));
                    entities.Add(new Arc(cpt1, this._ci.FlangeThickness * 0.5, DwgGeometry.kRad90, DwgGeometry.kRad180));
                    entities.Add(new Arc(cpt2, this._ci.FlangeThickness, DwgGeometry.kRad270, 0));
                    entities.Add(new Arc(cpt3, this._ci.FlangeThickness, DwgGeometry.kRad180, DwgGeometry.kRad270));
                    entities.Add(new Arc(cpt4, this._ci.FlangeThickness * 0.5, 0, DwgGeometry.kRad90));

                    Matrix3d matRotate = Matrix3d.Rotation(DwgGeometry.AngleFromXAxis(pt1, ptRotation), new Vector3d(0, 0, 1), pt1);
                    Matrix3d matMove = Matrix3d.Displacement(pt1 - DwgGeometry.GetPointPolar(pt1, 0, this._ci.Depth * .5));
                    foreach (Entity ent in entities)
                    {
                        ent.Layer = this._continuousLineLayer;
                        ent.TransformBy(matMove);
                        ent.TransformBy(matRotate);
                        currDwg.AddEntity(ent);
                    }
                }
            }
        }

        #endregion
    }
}
