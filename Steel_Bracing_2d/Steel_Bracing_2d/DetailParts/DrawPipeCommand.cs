namespace Steel_Bracing_2d.DetailParts
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class DrawPipeCommand
    {
        #region Fields

        private string _centerLineLayer = "center";

        private string _continuousLineLayer = "objects";

        private bool _drawCenterLine = true;

        private Point3d _endPoint;

        private string _hiddenLineLayer = "hidden";

        private double _pipeID = 50;

        private double _pipeThk = 3;

        private Point3d _startPoint;

        #endregion

        #region Constructors and Destructors

        public DrawPipeCommand()
        {
        }

        public DrawPipeCommand(Point3d pt1, Point3d pt2, double id, double thk)
        {
            this._startPoint = pt1;
            this._endPoint = pt2;
            this._pipeID = id;
            this._pipeThk = thk;
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

        public bool DrawCenterLine
        {
            get { return this._drawCenterLine; }
            set { this._drawCenterLine = value; }
        }

        public Point3d EndPoint
        {
            get { return this._endPoint; }
            set { this._endPoint = value; }
        }

        public string HiddenLineLayer
        {
            get { return this._hiddenLineLayer; }
            set { this._hiddenLineLayer = value; }
        }

        public double PipeID
        {
            get { return this._pipeID; }
            set { this._pipeID = value; }
        }

        public double PipeThk
        {
            get { return this._pipeThk; }
            set { this._pipeThk = value; }
        }

        public Point3d StartPoint
        {
            get { return this._startPoint; }
            set { this._startPoint = value; }
        }

        #endregion

        #region Public Methods and Operators

        public void ExecuteCommand()
        {
            double ang = DwgGeometry.AngleFromXAxis(this._startPoint, this._endPoint);
            double od = this._pipeID + this._pipeThk + this._pipeThk;
            Point3d p1 = DwgGeometry.GetPointPolar(this._startPoint, ang + DwgGeometry.kRad90, this._pipeID * 0.5);
            Point3d p2 = DwgGeometry.GetPointPolar(this._endPoint, ang + DwgGeometry.kRad90, this._pipeID * 0.5);

            Point3d p3 = DwgGeometry.GetPointPolar(this._startPoint, ang - DwgGeometry.kRad90, this._pipeID * 0.5);
            Point3d p4 = DwgGeometry.GetPointPolar(this._endPoint, ang - DwgGeometry.kRad90, this._pipeID * 0.5);

            Point3d p5 = DwgGeometry.GetPointPolar(this._startPoint, ang + DwgGeometry.kRad90, od * 0.5);
            Point3d p6 = DwgGeometry.GetPointPolar(this._endPoint, ang + DwgGeometry.kRad90, od * 0.5);

            Point3d p7 = DwgGeometry.GetPointPolar(this._startPoint, ang - DwgGeometry.kRad90, od * 0.5);
            Point3d p8 = DwgGeometry.GetPointPolar(this._endPoint, ang - DwgGeometry.kRad90, od * 0.5);

            ObjectIdCollection ids = new ObjectIdCollection();
            DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
            using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
            {

                Line l1 = new Line(p5, p6);
                l1.Layer = this._continuousLineLayer;
                ids.Add(currDwg.AddEntity(l1));

                l1 = new Line(p7, p8);
                l1.Layer = this._continuousLineLayer;
                ids.Add(currDwg.AddEntity(l1));

                l1 = new Line(p1, p2);
                l1.Layer = this._hiddenLineLayer;
                ids.Add(currDwg.AddEntity(l1));

                l1 = new Line(p3, p4);
                l1.Layer = this._hiddenLineLayer;
                ids.Add(currDwg.AddEntity(l1));

                if (this._drawCenterLine)
                {
                    l1 = new Line(this._startPoint, this._endPoint);
                    l1.Layer = this._centerLineLayer;
                    ids.Add(currDwg.AddEntity(l1));
                }

                this.AttachInformation(ids);
            }
        }

        #endregion

        #region Methods

        private void AttachInformation(ObjectIdCollection ids)
        {
            if (ids.Count > 0)
            {
                DuctInformation duct = new DuctInformation();
                duct.Id = Guid.NewGuid().ToString();
                duct.DuctId = this._pipeID;
                duct.DuctThk = this._pipeThk;
                foreach (ObjectId id in ids)
                {
                    duct.HandleList.Add(id.Handle.Value);
                }

                DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);

                foreach (ObjectId id in ids)
                {
                    currDwg.SaveEntityData<DuctInformation>(id, duct, DrawingDatabase.ObjectDataDictionaryKey);
                }
            }
        }

        #endregion
    }
}
