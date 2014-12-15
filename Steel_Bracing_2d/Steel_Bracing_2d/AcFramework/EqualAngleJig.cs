namespace Steel_Bracing_2d.AcFramework
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.Metaphore;

    class EqualAngleJig : DrawJig
    {
        #region Fields

        private AngleInformation m_ai = new AngleInformation();

        private Point3d m_firstPoint = Point3d.Origin;

        private Point3d m_secondPoint = Point3d.Origin;

        #endregion

        #region Constructors and Destructors

        public EqualAngleJig(Point3d pt1)
        {
            this.m_firstPoint = pt1;
        }

        #endregion

        #region Public Properties

        public AngleInformation Ai
        {
            get { return this.m_ai; }
            set { this.m_ai = value; }
        }

        public Point3d FirstPoint
        {
            get { return this.m_firstPoint; }
            set { this.m_firstPoint = value; }
        }

        public Point3d SecondPoint
        {
            get { return this.m_secondPoint; }
            set { this.m_secondPoint = value; }
        }

        #endregion

        #region Methods

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpt = new JigPromptPointOptions("\rPick End Point:");
            jigOpt.UserInputControls = UserInputControls.Accept3dCoordinates;

            PromptPointResult res = prompts.AcquirePoint(jigOpt);

            if (res.Status != PromptStatus.OK)
                return SamplerStatus.Cancel;

            // set other corner here
            this.m_secondPoint = res.Value;

            // compare points
            if (res.Value.IsEqualTo(this.m_secondPoint, new Tolerance(0.1, 0.1)))
            {
                return SamplerStatus.NoChange;
            }

            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            try
            {
                Point3d pt1 = this.m_firstPoint;
                Point3d pt2 = this.m_secondPoint;

                double ang = DwgGeometry.AngleFromXAxis(pt1, pt2);
                Line centerLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad180, this.m_ai.Width),
                    DwgGeometry.GetPointPolar(pt2, ang, this.m_ai.Width));

                Line topLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this.m_ai.CGLine),
                    DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this.m_ai.CGLine));

                Line thkLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad90, this.m_ai.CGLine - this.m_ai.Thickness),
                    DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad90, this.m_ai.CGLine - this.m_ai.Thickness));

                Line bottomLine = new Line(DwgGeometry.GetPointPolar(pt1, ang + DwgGeometry.kRad270, this.m_ai.Width - this.m_ai.CGLine),
                    DwgGeometry.GetPointPolar(pt2, ang + DwgGeometry.kRad270, this.m_ai.Width - this.m_ai.CGLine));

                draw.Geometry.WorldLine(topLine.StartPoint, topLine.EndPoint);
                draw.Geometry.WorldLine(thkLine.StartPoint, thkLine.EndPoint);
                draw.Geometry.WorldLine(bottomLine.StartPoint, bottomLine.EndPoint);

                //draw.SubEntityTraits.Layer =  
                draw.Geometry.WorldLine(centerLine.StartPoint, centerLine.EndPoint);

                centerLine.Dispose();
                topLine.Dispose();
                thkLine.Dispose();
                bottomLine.Dispose();
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
