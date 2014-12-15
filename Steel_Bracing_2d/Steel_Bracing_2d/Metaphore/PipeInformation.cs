namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Xml;

    [Serializable]
    public class PipeInformation
    {
        #region Fields

        private string _description = "50 NB";

        private double _outsideDiameter = 75;

        private double _thickness = 6;

        private double _weight = 1.0;

        #endregion

        #region Constructors and Destructors

        public PipeInformation()
        {
        }

        public PipeInformation(XmlNode nodeData)
        {
            this._description = nodeData.InnerText.Trim();
            try
            {
                this._outsideDiameter = Convert.ToDouble(nodeData.Attributes["OD"].Value);
                this._thickness = Convert.ToDouble(nodeData.Attributes["Thickness"].Value);
                this._weight = Convert.ToDouble(nodeData.Attributes["Weight"].Value);
            }
            catch
            {

            }
        }

        #endregion

        #region Public Properties

        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        public double OutsideDiameter
        {
            get { return this._outsideDiameter; }
            set { this._outsideDiameter = value; }
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

        #endregion

        #region Public Methods and Operators

        public override bool Equals(object obj)
        {
            PipeInformation ai = obj as PipeInformation;
            if (Math.Abs(ai._thickness - this._thickness) > 0.01) return false;
            if (Math.Abs(ai._weight - this._weight) > 0.01) return false;
            if (Math.Abs(ai._outsideDiameter - this._outsideDiameter) > 0.01) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this._description;
        }

        #endregion
    }
}
