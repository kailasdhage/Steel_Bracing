namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Xml;

    public class BeamInformation
    {
        #region Fields

        private double _breadth = 75;

        private double _depth = 75;

        private string _description = "150x75";

        private double _flangeHole = 35;

        private double _flangeThickness = 8;

        private string _prefix = "ISMB";

        private double _webThickness = 5;

        private double _weight = 15;

        #endregion

        #region Constructors and Destructors

        public BeamInformation()
        {
        }

        public BeamInformation(XmlNode nodeData)
        {
            this._description = nodeData.InnerText.Trim().ToLower().Replace(" ", "");
            string[] arr = this._description.Split(new char[] { 'x' });
            try
            {
                this._depth = Convert.ToDouble(arr[0]);
                this._breadth = Convert.ToDouble(arr[1]);
                this._webThickness = Convert.ToDouble(nodeData.Attributes["WebThickness"].Value);
                this._flangeThickness = Convert.ToDouble(nodeData.Attributes["FlangeThickness"].Value);
                this._flangeHole = Convert.ToDouble(nodeData.Attributes["FlangeHole"].Value);
                this._weight = Convert.ToDouble(nodeData.Attributes["Weight"].Value);
            }
            catch
            {

            }
        }

        #endregion

        #region Public Properties

        public double Breadth
        {
            get { return this._breadth; }
            set { this._breadth = value; }
        }

        public double Depth
        {
            get { return this._depth; }
            set { this._depth = value; }
        }

        public string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        public double FlangeHole
        {
            get { return this._flangeHole; }
            set { this._flangeHole = value; }
        }

        public double FlangeThickness
        {
            get { return this._flangeThickness; }
            set { this._flangeThickness = value; }
        }

        public string Prefix
        {
            get { return this._prefix; }
            set { this._prefix = value; }
        }

        public double WebThickness
        {
            get { return this._webThickness; }
            set { this._webThickness = value; }
        }

        public double Weight
        {
            get { return this._weight; }
            set { this._weight = value; }
        }

        #endregion
    }
}
