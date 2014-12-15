namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Steel_Bracing_2d.AcFramework;

    [Serializable]
    public class SteelSectionInformation : IObjectBrowser, IObjectInformation
    {
        //this will be a guid, the unique id

        //this will be a list of entities used for highlight and draw profiles

        #region Fields

        private BracingMemberType m_angleType = BracingMemberType.SimpleAngle;

        private double m_centerLineAngle = 0;

        private double m_centerLineX = 0;

        private double m_centerLineY = 0;

        private long m_handle = 0;

        private ArrayList m_handleList = new ArrayList();

        private double m_holeDiameter = 0;

        private double m_holePitch = 0;

        private string m_id = "";

        private AngleInformation m_memberInfo = null;

        private int m_noOfHoles = 0;

        private double m_oblongOffset = 0;

        private int m_quantity = 1;

        private double m_steelMemeberLength = 0;

        private double m_xHoleOffset = 0;

        private double m_yHoleOffset = 0;

        #endregion

        #region Public Properties

        public BracingMemberType AngleType
        {
            get { return this.m_angleType; }
            set { this.m_angleType = value; }
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

        public ArrayList HandleList
        {
            get { return this.m_handleList; }
            set { this.m_handleList = value; }
        }

        public double HoleDiameter
        {
            get { return this.m_holeDiameter; }
            set { this.m_holeDiameter = value; }
        }

        public double HolePitch
        {
            get { return this.m_holePitch; }
            set { this.m_holePitch = value; }
        }

        public string Id
        {
            get { return this.m_id; }
            set { this.m_id = value; }
        }

        public AngleInformation MemberInfo
        {
            get { return this.m_memberInfo; }
            set { this.m_memberInfo = value; }
        }

        public int NoOfHoles
        {
            get { return this.m_noOfHoles; }
            set { this.m_noOfHoles = value; }
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

        public double OblongOffset
        {
            get { return this.m_oblongOffset; }
            set { this.m_oblongOffset = value; }
        }

        public int Quantity
        {
            get { return this.m_quantity; }
            set { this.m_quantity = value; }
        }

        public double SteelMemeberLength
        {
            get { return this.m_steelMemeberLength; }
            set { this.m_steelMemeberLength = value; }
        }

        public double XHoleOffset
        {
            get { return this.m_xHoleOffset; }
            set { this.m_xHoleOffset = value; }
        }

        public double YHoleOffset
        {
            get { return this.m_yHoleOffset; }
            set { this.m_yHoleOffset = value; }
        }

        #endregion

        #region Public Methods and Operators

        public override bool Equals(object obj)
        {
            SteelSectionInformation ssi = obj as SteelSectionInformation;
            if (Math.Abs(ssi.m_steelMemeberLength - this.m_steelMemeberLength) > 0.01) return false;
            if (ssi.m_angleType != this.m_angleType) return false;
            if (ssi.m_noOfHoles != this.m_noOfHoles) return false;
            if (this.m_noOfHoles > 0)
            {
                if (Math.Abs(ssi.m_holeDiameter - this.m_holeDiameter) > 0.01) return false;
                if (Math.Abs(ssi.m_oblongOffset - this.m_oblongOffset) > 0.01) return false;
                if (Math.Abs(ssi.m_holePitch - this.m_holePitch) > 0.01) return false;
                if (Math.Abs(ssi.m_xHoleOffset - this.m_xHoleOffset) > 0.01) return false;
                if (Math.Abs(ssi.m_yHoleOffset - this.m_yHoleOffset) > 0.01) return false;
            }

            if (!ssi.m_memberInfo.Equals(this.m_memberInfo)) return false;

            return true;
        }

        public List<long> GetEntityHandles()
        {
            List<long> handles = new List<long>();
            foreach (long id in this.m_handleList)
            {
                handles.Add(id);
            }

            return handles;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetToolTip()
        {
            return "Steel Section, Length : " + this.m_steelMemeberLength.ToString("##.0#") + "\n" + this.m_memberInfo.ToString();
        }

        public override string ToString()
        {
            string strRet = "";
            strRet += "Length = " + this.m_steelMemeberLength.ToString();
            strRet += "\nCenter Point = " + this.m_centerLineX.ToString() + "," + this.m_centerLineY.ToString();
            strRet += "\nCenter Line Angle = " + DwgGeometry.rtd(this.m_centerLineAngle).ToString();
            strRet += "\nAngle Information: " + this.m_memberInfo.ToString();

            return strRet;
        }

        #endregion
    }

    [Serializable]
    public class DuctInformation : IObjectBrowser, IObjectInformation
    {
        //this will be a guid, the unique id

        #region Fields

        private double _ductId;

        private double _ductThk;

        private long m_handle = 0;

        private ArrayList m_handleList = new ArrayList();

        private string m_id = "";

        private int m_quantity = 1;

        #endregion

        #region Public Properties

        public double DuctId
        {
            get { return this._ductId; }
            set { this._ductId = value; }
        }

        public double DuctThk
        {
            get { return this._ductThk; }
            set { this._ductThk = value; }
        }

        public ArrayList HandleList
        {
            get { return this.m_handleList; }
            set { this.m_handleList = value; }
        }

        public string Id
        {
            get { return this.m_id; }
            set { this.m_id = value; }
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

        public int Quantity
        {
            get { return this.m_quantity; }
            set { this.m_quantity = value; }
        }

        #endregion

        #region Public Methods and Operators

        public override bool Equals(object obj)
        {

            return true;
        }

        public List<long> GetEntityHandles()
        {
            List<long> handles = new List<long>();
            foreach (long id in this.m_handleList)
            {
                handles.Add(id);
            }

            return handles;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetToolTip()
        {
            return "Duct, Inside Diameter : " + this._ductId.ToString("##.0#") + "\nThickness : " + this._ductThk.ToString("##.0#");
        }

        public override string ToString()
        {
            string strRet = "";
            strRet += "\nInside Diameter = " + this._ductId.ToString();
            strRet += "\nThickness = " + this._ductThk.ToString();
            return strRet;
        }

        #endregion
    }
}
