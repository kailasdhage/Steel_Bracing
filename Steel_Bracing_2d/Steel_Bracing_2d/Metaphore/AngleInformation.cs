namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Xml;

    [Serializable]
    public class AngleInformation
    {
        #region Fields

        private double _boltLine = 40;

        private double _cGLine = 20.6;

        private string _description = "75x75x6";

        private string _prefix = "L";

        private double _thickness = 6;

        private double _weight = 6.8;

        private double _width = 75;

        #endregion

        #region Constructors and Destructors

        public AngleInformation()
        {
        }

        public AngleInformation(XmlNode nodeData)
        {
            this._description = nodeData.InnerText.Trim().ToLower().Replace(" ", "");
            string[] arr = this._description.Split(new char[] { 'x' });
            try
            {
                this._width = Convert.ToDouble(arr[0]);
                this._thickness = Convert.ToDouble(arr[1]);
                this._weight = Convert.ToDouble(nodeData.Attributes["Weight"].Value);
                this._cGLine = Convert.ToDouble(nodeData.Attributes["CGLine"].Value);
                this._boltLine = Convert.ToDouble(nodeData.Attributes["BoltLine"].Value);
                this._boltLine = this._width - this._boltLine;

                this._description = arr[0] + "x" + this._description;
            }
            catch
            {

            }
        }

        #endregion

        #region Public Properties

        public double BoltLine
        {
            get { return this._boltLine; }
            set { this._boltLine = value; }
        }

        public double CGLine
        {
            get { return this._cGLine; }
            set { this._cGLine = value; }
        }

        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        public string Prefix
        {
            get { return this._prefix; }
            set { this._prefix = value; }
        }

        public double Thickness
        {
            get { return this._thickness; }
            set { this._thickness = value; }
        }

        public double Weight
        {
            get { return this._weight; }
            set { this._weight = value; }
        }

        public double Width
        {
            get { return this._width; }
            set { this._width = value; }
        }

        #endregion

        #region Public Methods and Operators

        public override bool Equals(object obj)
        {
            AngleInformation ai = obj as AngleInformation;
            if (Math.Abs(ai._boltLine - this._boltLine) > 0.01) return false;
            if (Math.Abs(ai._cGLine - this._cGLine) > 0.01) return false;
            if (Math.Abs(ai._thickness - this._thickness) > 0.01) return false;
            if (Math.Abs(ai._weight - this._weight) > 0.01) return false;
            if (Math.Abs(ai._width - this._width) > 0.01) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string strRet = "";
            strRet += "Weight = " + this._weight.ToString();
            strRet += "\nCGLine = " + this._cGLine.ToString();
            strRet += "\nBoltLine = " + this._boltLine.ToString();
            strRet += "\nDescription = " + this._width.ToString() + "x" + this._thickness.ToString();
            return strRet;
        }

        #endregion
    }
}
