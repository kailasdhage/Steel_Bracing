namespace Steel_Bracing_2d.Metaphore
{
    using System;
    using System.Xml;

    public class ChannelInformation : BeamInformation
    {
        #region Constructors and Destructors

        public ChannelInformation()
        {
        }

        public ChannelInformation(XmlNode nodeData)
        {
            this.Description = nodeData.InnerText.Trim().ToLower().Replace(" ", "");
            string[] arr = this.Description.Split(new[] { 'x' });
            try
            {
                this.Depth = Convert.ToDouble(arr[0]);
                this.Breadth = Convert.ToDouble(arr[1]);
                if( nodeData.Attributes != null )
                {
                    this.WebThickness = Convert.ToDouble(nodeData.Attributes["WebThickness"].Value);
                    this.FlangeThickness = Convert.ToDouble(nodeData.Attributes["FlangeThickness"].Value);
                    this.FlangeHole = Convert.ToDouble(nodeData.Attributes["FlangeHole"].Value);
                    this.Weight = Convert.ToDouble(nodeData.Attributes["Weight"].Value);
                }
            }
            catch( Exception )
            {

            }
        }

        #endregion
    }
}
