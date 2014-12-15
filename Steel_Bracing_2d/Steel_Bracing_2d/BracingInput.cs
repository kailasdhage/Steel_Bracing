namespace Steel_Bracing_2d
{
    using System.Collections.Generic;
    using System.Xml;

    using Autodesk.AutoCAD.DatabaseServices;

    using Steel_Bracing_2d.Metaphore;

    public interface IBracingInput
    {
        BracingLayoutType BracingType
        {
            get;
            set;
        }

        BracingMemberType CrossMemberType
        {
            get;
            set;
        }

        ChannelInformation SteelChannel
        {
            get;
            set;
        }

        AngleInformation SteelAngle
        {
            get;
            set;
        }

        PipeInformation SteelPipe
        {
            get;
            set;
        }

        double SteelMemberOverlap
        {
            get;
            set;
        }

        int NoOfHoles
        {
            get;
            set;
        }

        double HoleCenterX
        {
            get;
            set;
        }

        double HoleDia
        {
            get;
            set;
        }

        double HolePitch
        {
            get;
            set;
        }

        double CenterPlateOffset
        {
            get;
            set;
        }

        double StarPlateThickness
        {
            get;
            set;
        }

        double StarPlateOffset
        {
            get;
            set;
        }

        double StarPlateWidth
        {
            get;
            set;
        }

        double PlateExtendOffset
        {
            get;
            set;
        }

        bool AddLengthDimension
        {
            get;
            set;
        }

        double OblongHoleCenterOffset
        {
            get;
            set;
        }

        double EdgeOffset
        {
            get;
            set;
        }

        Line VertLine1
        {
            get;
            set;
        }

        Line VertLine2
        {
            get;
            set;
        }

        Line HorzLine1
        {
            get;
            set;
        }

        Line HorzLine2
        {
            get;
            set;
        }

        Line CentLine1
        {
            get;
            set;
        }

        Line CentLine2
        {
            get;
            set;
        }
    }

    public class BracingInput : IBracingInput
    {
        private BracingLayoutType bracingType;

        private BracingMemberType crossMemberType;

        private ChannelInformation steelChannel;

        private AngleInformation steelAngle;

        private PipeInformation steelPipe;

        private double steelMemberOverlap;

        private int noOfHoles;

        private double holeCenterX;

        private double holeDia;

        private double holePitch;

        private double centerPlateOffset;

        private double starPlateThickness;

        private double starPlateOffset;

        private double starPlateWidth;

        private double plateExtendOffset;

        private bool addLengthDimension;

        private double oblongHoleCenterOffset;

        private double edgeOffset;

        private Line vertLine1;

        private Line vertLine2;

        private Line horzLine1;

        private Line horzLine2;

        private Line centLine1;

        private Line centLine2;

        public BracingLayoutType BracingType
        {
            get
            {
                return bracingType;
            }
            set
            {
                bracingType = value;
            }
        }

        public BracingMemberType CrossMemberType
        {
            get
            {
                return crossMemberType;
            }
            set
            {
                crossMemberType = value;
            }
        }

        public ChannelInformation SteelChannel
        {
            get
            {
                return steelChannel;
            }
            set
            {
                steelChannel = value;
            }
        }

        public AngleInformation SteelAngle
        {
            get
            {
                return steelAngle;
            }
            set
            {
                steelAngle = value;
            }
        }

        public PipeInformation SteelPipe
        {
            get
            {
                return steelPipe;
            }
            set
            {
                steelPipe = value;
            }
        }

        public double SteelMemberOverlap
        {
            get
            {
                return steelMemberOverlap;
            }
            set
            {
                steelMemberOverlap = value;
            }
        }

        public int NoOfHoles
        {
            get
            {
                return noOfHoles;
            }
            set
            {
                noOfHoles = value;
            }
        }

        public double HoleCenterX
        {
            get
            {
                return holeCenterX;
            }
            set
            {
                holeCenterX = value;
            }
        }

        public double HoleDia
        {
            get
            {
                return holeDia;
            }
            set
            {
                holeDia = value;
            }
        }

        public double HolePitch
        {
            get
            {
                return holePitch;
            }
            set
            {
                holePitch = value;
            }
        }

        public double CenterPlateOffset
        {
            get
            {
                return centerPlateOffset;
            }
            set
            {
                centerPlateOffset = value;
            }
        }

        public double StarPlateThickness
        {
            get
            {
                return starPlateThickness;
            }
            set
            {
                starPlateThickness = value;
            }
        }

        public double StarPlateOffset
        {
            get
            {
                return starPlateOffset;
            }
            set
            {
                starPlateOffset = value;
            }
        }

        public double StarPlateWidth
        {
            get
            {
                return starPlateWidth;
            }
            set
            {
                starPlateWidth = value;
            }
        }

        public double PlateExtendOffset
        {
            get
            {
                return plateExtendOffset;
            }
            set
            {
                plateExtendOffset = value;
            }
        }

        public bool AddLengthDimension
        {
            get
            {
                return addLengthDimension;
            }
            set
            {
                addLengthDimension = value;
            }
        }

        public double OblongHoleCenterOffset
        {
            get
            {
                return oblongHoleCenterOffset;
            }
            set
            {
                oblongHoleCenterOffset = value;
            }
        }

        public double EdgeOffset
        {
            get
            {
                return edgeOffset;
            }
            set
            {
                edgeOffset = value;
            }
        }

        public Line VertLine1
        {
            get
            {
                return this.vertLine1;
            }
            set
            {
                this.vertLine1 = value;
            }
        }

        public Line VertLine2
        {
            get
            {
                return this.vertLine2;
            }
            set
            {
                this.vertLine2 = value;
            }
        }

        public Line HorzLine1
        {
            get
            {
                return this.horzLine1;
            }
            set
            {
                this.horzLine1 = value;
            }
        }

        public Line HorzLine2
        {
            get
            {
                return this.horzLine2;
            }
            set
            {
                this.horzLine2 = value;
            }
        }

        public Line CentLine1
        {
            get
            {
                return this.centLine1;
            }
            set
            {
                this.centLine1 = value;
            }
        }

        public Line CentLine2
        {
            get
            {
                return this.centLine2;
            }
            set
            {
                this.centLine2 = value;
            }
        }

        public List<AngleInformation> SteelAngleList
        {
            get;
            set;
        }

         public List<ChannelInformation> SteelChannelList
        {
            get;
            set;
        }

         public List<PipeInformation> SteelPipeList
        {
            get;
            set;
        }

        public void LoadSteelSections()
        {
            this.SteelAngleList = new List<AngleInformation>();
            string path = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            XmlDocument doc = new XmlDocument();
            doc.Load( path + "\\AngleData.xml" );
            if( doc.DocumentElement != null )
            {
                foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
                {
                    AngleInformation ai = new AngleInformation( nde );
                    this.SteelAngleList.Add( ai );
                }
            }

            this.SteelChannelList = new List<ChannelInformation>();
            doc = new XmlDocument();
            doc.Load( path + "\\ChannelData.xml" );
            if( doc.DocumentElement != null )
            {
                foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
                {
                    ChannelInformation ci = new ChannelInformation( nde );
                    this.SteelChannelList.Add( ci );
                }

                this.SteelPipeList = new List<PipeInformation>();
                doc = new XmlDocument();
                doc.Load( path + "\\PipeData.xml" );
                if( doc.DocumentElement != null )
                {
                    foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
                    {
                        PipeInformation pi = new PipeInformation( nde );
                        this.SteelPipeList.Add( pi );
                    }
                }
            }
        }
    }
}
