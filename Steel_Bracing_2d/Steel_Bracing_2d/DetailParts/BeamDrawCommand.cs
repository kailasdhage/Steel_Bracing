namespace Steel_Bracing_2d.DetailParts
{
    using System.Collections.Generic;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class BeamDrawCommand
    {
        #region Fields

        private readonly BeamInformation _bi = new BeamInformation();

        private string _centerLineLayer = "center";

        private string _continuousLineLayer = "objects";

        private string _descTextLayer = "text";

        private string _descTextStyle = "brc25";

        private bool _drawCenterLine = false;

        private string _hiddenLineLayer = "hidden";

        private string _viewType = "Front";

        #endregion

        #region Constructors and Destructors

        public BeamDrawCommand()
        {
        }

        public BeamDrawCommand(BeamInformation ai)
        {
            this._bi = ai;
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
            using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
            {
                //check layer exists
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

                    Line topLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this._bi.Depth * 0.5),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this._bi.Depth * 0.5));
                    topLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(topLine);

                    Line thkLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, (this._bi.Depth * 0.5) - this._bi.FlangeThickness),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, (this._bi.Depth * 0.5) - this._bi.FlangeThickness));
                    thkLine.Layer = this._continuousLineLayer;
                    currDwg.AddEntity(thkLine);

                    Line thkLine1 = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad270, (this._bi.Depth * 0.5) - this._bi.FlangeThickness),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad270, (this._bi.Depth * 0.5) - this._bi.FlangeThickness));
                    thkLine1.Layer = this._continuousLineLayer;

                    currDwg.AddEntity(thkLine1);
                    Line bottomLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad270, this._bi.Depth * 0.5),
                        DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad270, this._bi.Depth * 0.5));
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
                    angDescText.TextString = "ISMB " + this._bi.Description;

                    angDescText.SetTextStyleId(currDwg.GetTextStyleId(this._descTextStyle));
                    TextStyleTableRecord styRec = currDwg.GetObject(angDescText.TextStyleId) as TextStyleTableRecord;
                    angDescText.Layer = this._descTextLayer;
                    double textOffset = (this._bi.Depth * 0.5) + currDwg.Dimscale;
                    angDescText.Position = DwgGeometry.GetPointPolar(
                        DwgGeometry.GetPointPolar(pt1, DwgGeometry.AngleFromXAxis(pt1, pt2), pt1.DistanceTo(pt2) * 0.25),
                        DwgGeometry.AngleFromXAxis(pt1, pt2) + DwgGeometry.kRad90, textOffset);
                    angDescText.Rotation = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    angDescText.Height = styRec.TextSize;
                    currDwg.AddEntity(angDescText);
                }
            }
            else if (this.ViewType == "Side")
            {
                ObjectIdCollection ids = new ObjectIdCollection();
                using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
                {
                    double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                    Point3d p1 = pt1;
                    Point3d p2 = pt2;
                    double sectionLength = p1.DistanceTo(p2);

                    Line topLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._bi.Breadth * 0.5),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._bi.Breadth * 0.5));
                    topLine.Layer = this._continuousLineLayer;
                    ids.Add(currDwg.AddEntity(topLine));

                    Line thkLine = new Line(DwgGeometry.GetPointPolar(p1, ang + DwgGeometry.kRad90, this._bi.WebThickness * 0.5),
                        DwgGeometry.GetPointPolar(p2, ang + DwgGeometry.kRad90, this._bi.WebThickness * 0.5));
                    thkLine.Layer = this._hiddenLineLayer;
                    ids.Add(currDwg.AddEntity(thkLine));

                    Line thkLine1 = new Line(DwgGeometry.GetPointPolar(p1, ang - DwgGeometry.kRad90, this._bi.WebThickness * 0.5),
                        DwgGeometry.GetPointPolar(p2, ang - DwgGeometry.kRad90, this._bi.WebThickness * 0.5));
                    thkLine1.Layer = this._hiddenLineLayer;
                    ids.Add(currDwg.AddEntity(thkLine1));

                    Line bottomLine = new Line(DwgGeometry.GetPointPolar(p1, ang - DwgGeometry.kRad90, this._bi.Breadth * 0.5),
                        DwgGeometry.GetPointPolar(p2, ang - DwgGeometry.kRad90, this._bi.Breadth * 0.5));
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
                    Point3d ptRotation = pt2;
                    pt2 = DwgGeometry.GetPointPolar(pt1, 0, this._bi.Breadth);
                    Point3d mpt1 = DwgGeometry.GetRectPoint(pt1, this._bi.Breadth * 0.5, 0);
                    Point3d pt3 = DwgGeometry.GetPointPolar(pt2, DwgGeometry.kRad90, this._bi.FlangeThickness * 0.5);
                    Point3d pt4 = DwgGeometry.GetRectPoint(pt3, this._bi.FlangeThickness * -0.5, this._bi.FlangeThickness * 0.5);
                    Point3d pt5 = DwgGeometry.GetRectPoint(mpt1, this._bi.WebThickness * 1.5, this._bi.FlangeThickness);
                    Point3d pt6 = DwgGeometry.GetRectPoint(pt5, this._bi.WebThickness * -1.0, this._bi.WebThickness);
                    Point3d cpt1 = DwgGeometry.GetRectPoint(pt3, this._bi.FlangeThickness * -0.5, 0);
                    Point3d cpt2 = DwgGeometry.GetRectPoint(pt6, this._bi.WebThickness, 0);

                    List<Entity> entities = new List<Entity>();
                    Line l1 = new Line(pt2, pt3);
                    Line l2 = new Line(pt4, pt5);
                    Arc a1 = new Arc(cpt1, this._bi.FlangeThickness * 0.5, 0, DwgGeometry.kRad90);
                    Arc a2 = new Arc(cpt2, this._bi.WebThickness, DwgGeometry.kRad180, DwgGeometry.kRad270);
                    entities.AddRange(new List<Entity> { l1, l2, a1, a2 });

                    Line l11 = l1.Clone() as Line;
                    Line l22 = l2.Clone() as Line;
                    Arc a11 = a1.Clone() as Arc;
                    Arc a22 = a2.Clone() as Arc;
                    entities.AddRange(new List<Entity> {l11,l22,a11,a22});

                    //mirror the bottom part
                    Matrix3d matMirrorHorz = Matrix3d.Mirroring(new Line3d(DwgGeometry.GetRectPoint(mpt1,0, this._bi.Depth *0.5),DwgGeometry.GetRectPoint(mpt1,this._bi.Depth, this._bi.Depth *0.5)));
                    l11.TransformBy(matMirrorHorz);
                    l22.TransformBy(matMirrorHorz);
                    a22.TransformBy(matMirrorHorz);
                    a11.TransformBy(matMirrorHorz);


                    Line l111 = l1.Clone() as Line;
                    Line l222 = l2.Clone() as Line;
                    Arc a111 = a1.Clone() as Arc;
                    Arc a222 = a2.Clone() as Arc;
                    entities.AddRange(new List<Entity> { l111, l222, a111, a222 });


                    Line l1111 = l11.Clone() as Line;
                    Line l2222 = l22.Clone() as Line;
                    Arc a1111 = a11.Clone() as Arc;
                    Arc a2222 = a22.Clone() as Arc;
                    entities.AddRange(new List<Entity> { l1111, l2222, a1111, a2222 });

                    //mirror the bottom part
                    Matrix3d matMirrorVert = Matrix3d.Mirroring(new Line3d(mpt1, DwgGeometry.GetRectPoint(mpt1, 0, this._bi.Depth)));

                    l111.TransformBy(matMirrorVert);
                    l222.TransformBy(matMirrorVert);
                    a222.TransformBy(matMirrorVert);
                    a111.TransformBy(matMirrorVert);
                    l1111.TransformBy(matMirrorVert);
                    l2222.TransformBy(matMirrorVert);
                    a2222.TransformBy(matMirrorVert);
                    a1111.TransformBy(matMirrorVert);

                    //draw lines
                    entities.Add(new Line(pt1, pt2));
                    entities.Add(new Line(DwgGeometry.GetRectPoint(pt1, 0, this._bi.Depth), DwgGeometry.GetRectPoint(pt2, 0, this._bi.Depth)));
                    entities.Add(new Line(DwgGeometry.GetRectPoint(mpt1, this._bi.WebThickness * -0.5, this._bi.WebThickness + this._bi.FlangeThickness),
                        DwgGeometry.GetRectPoint(mpt1, this._bi.WebThickness * -0.5, this._bi.Depth - (this._bi.WebThickness + this._bi.FlangeThickness))));
                    entities.Add(new Line(DwgGeometry.GetRectPoint(mpt1, this._bi.WebThickness * 0.5, this._bi.WebThickness + this._bi.FlangeThickness),
                        DwgGeometry.GetRectPoint(mpt1, this._bi.WebThickness * 0.5, this._bi.Depth - (this._bi.WebThickness + this._bi.FlangeThickness))));

                    Matrix3d matRotate = Matrix3d.Rotation(DwgGeometry.AngleFromXAxis(pt1, ptRotation), new Vector3d(0, 0, 1), pt1);
                    Matrix3d matMove = Matrix3d.Displacement(pt1 - DwgGeometry.GetPointPolar(pt1, 0, this._bi.Breadth * .5));
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
