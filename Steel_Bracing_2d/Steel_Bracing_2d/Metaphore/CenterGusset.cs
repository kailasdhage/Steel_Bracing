namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Steel_Bracing_2d.AcFramework;

    [Serializable]
    public class CenterGusset : IObjectBrowser, IObjectInformation
    {
        #region Fields

        private AngleInformation m_angleInfo = new AngleInformation();

        private double m_centerLineAngle = 0;

        private double m_centerLineX = 0;

        private double m_centerLineY = 0;

        private double m_chamferDistance = 0;

        private long m_handle = 0;

        private ArrayList m_handleList = new ArrayList();

        private double m_hole_center_x = 0;

        private double m_hole_dia = 0;

        private double m_hole_pitch = 0;

        private string m_id = "";

        private int m_no_of_holes = 0;

        private double m_oblongHoleCenterOffset = 0;

        private double m_plateExtendDistance = 0;

        private double m_weldLength = 0;

        #endregion

        #region Public Properties

        public AngleInformation AngleInfo
        {
            get { return this.m_angleInfo; }
            set { this.m_angleInfo = value; }
        }

        public double CenterLineAngle
        {
            get { return this.m_centerLineAngle; }
            set { this.m_centerLineAngle = value; }
        }

        public double CenterLineX
        {
            get { return this.m_centerLineX; }
            set { this.m_centerLineX = value; }
        }

        public double CenterLineY
        {
            get { return this.m_centerLineY; }
            set { this.m_centerLineY = value; }
        }

        public double ChamferDistance
        {
            get { return this.m_chamferDistance; }
            set { this.m_chamferDistance = value; }
        }

        public ArrayList HandleList
        {
            get { return this.m_handleList; }
            set { this.m_handleList = value; }
        }

        public double Hole_center_x
        {
            get { return this.m_hole_center_x; }
            set { this.m_hole_center_x = value; }
        }

        public double Hole_dia
        {
            get { return this.m_hole_dia; }
            set { this.m_hole_dia = value; }
        }

        public double Hole_pitch
        {
            get { return this.m_hole_pitch; }
            set { this.m_hole_pitch = value; }
        }

        public string Id
        {
            get { return this.m_id; }
            set { this.m_id = value; }
        }

        public int No_of_holes
        {
            get { return this.m_no_of_holes; }
            set { this.m_no_of_holes = value; }
        }

        public long ObjectHandle
        {
            get
            {
                return this.m_handle;
            }
            set
            {
                this.m_handle = value;
            }
        }

        public double OblongHoleCenterOffset
        {
            get { return this.m_oblongHoleCenterOffset; }
            set { this.m_oblongHoleCenterOffset = value; }
        }

        public double PlateExtendDistance
        {
            get { return this.m_plateExtendDistance; }
            set { this.m_plateExtendDistance = value; }
        }

        public double WeldLength
        {
            get { return this.m_weldLength; }
            set { this.m_weldLength = value; }
        }

        #endregion

        #region Public Methods and Operators

        public List<long> GetEntityHandles()
        {
            List<long> handles = new List<long>();
            foreach (long id in this.m_handleList)
            {
                handles.Add(id);
            }

            return handles;
        }

        public string GetToolTip()
        {
            return "Center Gusset Plate, Weld Length : " + this.m_weldLength.ToString("##.0#");
        }

        public override string ToString()
        {
            string strRet = "";
            strRet += " Weld Length = " + this.m_weldLength.ToString();
            return strRet;
        }

        #endregion
    }
}
