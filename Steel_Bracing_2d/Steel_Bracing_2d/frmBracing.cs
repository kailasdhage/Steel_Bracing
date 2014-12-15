namespace Steel_Bracing_2d
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Xml;

    using Steel_Bracing_2d.Metaphore;

    public partial class FrmBracing : Form
    {
        #region Fields

        public List<string> TextStyleList = new List<string>();

        private bool addLengthDimension = true;

        private string angleDescTextStyle = "standard";

        private string angleTextLayer = "0";

        private BracingLayoutType bracingType = BracingLayoutType.None;

        private double centerPlateOffset = 70;

        private BracingMemberType crossMemberType = BracingMemberType.SimpleAngle;

        private string dimLayer = "0";

        private string hiddenLayer = "0";

        private double holeCenterX = 50;

        private double holeDia = 20;

        private double holePitch = 50;

        private int noOfHoles = 2;

        private double oblongHoleCenterOffset = 12;

        private double plateExtendOffset = 20;

        private double starPlateOffset = 500;

        private double starPlateThickness = 8;

        private double starPlateWidth = 75;

        private AngleInformation steelAngle = new AngleInformation();

        private List<AngleInformation> steelAngleList = new List<AngleInformation>();

        private ChannelInformation steelChannel = new ChannelInformation();

        private List<ChannelInformation> steelChannelList = new List<ChannelInformation>();

        private PipeInformation steelPipe = new PipeInformation();

        private List<PipeInformation> steelPipeList = new List<PipeInformation>();

        private double edgeOffset = 20;

        private double steelMemberOverlap = 250;

        #endregion

        #region Constructors and Destructors

        public FrmBracing()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public bool AddLengthDimension
        {
            get
            {
                return this.addLengthDimension;
            }
            set
            {
                this.addLengthDimension = value;
            }
        }

        public string AngleDescTextStyle
        {
            get
            {
                return this.angleDescTextStyle;
            }
            set
            {
                this.angleDescTextStyle = value;
            }
        }

        public string AngleTextLayer
        {
            get
            {
                return this.angleTextLayer;
            }
            set
            {
                this.angleTextLayer = value;
            }
        }

        public BracingLayoutType BracingType
        {
            get
            {
                return this.bracingType;
            }
            set
            {
                this.bracingType = value;
            }
        }

        public double CenterPlateOffset
        {
            get
            {
                return this.centerPlateOffset;
            }
            set
            {
                this.centerPlateOffset = value;
            }
        }

        public BracingMemberType CrossMemberType
        {
            get
            {
                return this.crossMemberType;
            }
            set
            {
                this.crossMemberType = value;
            }
        }

        public string DimLayer
        {
            get
            {
                return this.dimLayer;
            }
            set
            {
                this.dimLayer = value;
            }
        }

        public double EdgeOffset
        {
            get
            {
                return this.edgeOffset;
            }
            set
            {
                this.edgeOffset = value;
            }
        }

        public string HiddenLayer
        {
            get
            {
                return this.hiddenLayer;
            }
            set
            {
                this.hiddenLayer = value;
            }
        }

        public double HoleCenterX
        {
            get
            {
                return this.holeCenterX;
            }
            set
            {
                this.holeCenterX = value;
            }
        }

        public double HoleDia
        {
            get
            {
                return this.holeDia;
            }
            set
            {
                this.holeDia = value;
            }
        }

        public double HolePitch
        {
            get
            {
                return this.holePitch;
            }
            set
            {
                this.holePitch = value;
            }
        }

        public string MemberDescription
        {
            get
            {
                return string.Empty;
            }
        }

        public string MemberDescriptionTextStyle
        {
            get
            {
                return string.Empty;
            }
        }

        public int NoOfHoles
        {
            get
            {
                return this.noOfHoles;
            }
            set
            {
                this.noOfHoles = value;
            }
        }

        public double OblongHoleCenterOffset
        {
            get
            {
                return this.oblongHoleCenterOffset;
            }
            set
            {
                this.oblongHoleCenterOffset = value;
            }
        }

        public double PlateExtendOffset
        {
            get
            {
                return this.plateExtendOffset;
            }
            set
            {
                this.plateExtendOffset = value;
            }
        }

        public double StarPlateOffset
        {
            get
            {
                return this.starPlateOffset;
            }
            set
            {
                this.starPlateOffset = value;
            }
        }

        public double StarPlateThickness
        {
            get
            {
                return this.starPlateThickness;
            }
            set
            {
                this.starPlateThickness = value;
            }
        }

        public double StarPlateWidth
        {
            get
            {
                return this.starPlateWidth;
            }
            set
            {
                this.starPlateWidth = value;
            }
        }

        public AngleInformation SteelAngle
        {
            get
            {
                return this.steelAngle;
            }
            set
            {
                this.steelAngle = value;
            }
        }

        public List<AngleInformation> SteelAngleList
        {
            get
            {
                return this.steelAngleList;
            }
            set
            {
                this.steelAngleList = value;
            }
        }

        public ChannelInformation SteelChannel
        {
            get
            {
                return this.steelChannel;
            }
            set
            {
                this.steelChannel = value;
            }
        }

        public List<ChannelInformation> SteelChannelList
        {
            get
            {
                return this.steelChannelList;
            }
            set
            {
                this.steelChannelList = value;
            }
        }

        public PipeInformation SteelPipe
        {
            get
            {
                return this.steelPipe;
            }
            set
            {
                this.steelPipe = value;
            }
        }

        public List<PipeInformation> SteelPipeList
        {
            get
            {
                return this.steelPipeList;
            }
            set
            {
                this.steelPipeList = value;
            }
        }

        public double SteelMemberOverlap
        {
            get
            {
                return this.steelMemberOverlap;
            }
            set
            {
                this.steelMemberOverlap = value;
            }
        }

        #endregion

        #region Methods

        private void btnOk_Click( object sender, EventArgs e )
        {
            bool channelSelected = this.radBackToBackChannel.Checked || this.radBoxChannel.Checked;
            bool angleSelected = this.radStarAngle.Checked || this.radBackToBackAngle.Checked ||
                                 this.radSimpleAngle.Checked;
            string steelAngText = this.cmbAngle.Text.Trim()
                .ToLower();
            if( channelSelected )
            {
                steelAngText = this.cmbChannel.Text.Trim()
                    .ToLower();
            }
            else if( this.radPipe.Checked )
            {
                steelAngText = this.cmbPipe.Text;
            }

            if( steelAngText.Length == 0 )
            {
                MessageBox.Show( "Angle / Channel is not valid, Use format like 75x75x6", "Steel Detailing Tools" );
                return;
            }

            string[] angleArr = steelAngText.Split( new[] { 'x' } );
            if( angleArr.Length < 2 && ( channelSelected || angleSelected ) )
            {
                MessageBox.Show( "Angle / Channel is not valid, Use format like 75x75x6", "Steel Detailing Tools" );
                return;
            }
            double dblTemp = 0;
            if( channelSelected || angleSelected )
            {
                if( !double.TryParse( angleArr[0], out dblTemp ) || dblTemp <= 0 )
                {
                    MessageBox.Show( "Angle / Channel width is not valid", "Steel Detailing" );
                    return;
                }
            }

            if( channelSelected )
            {
                this.steelChannel.Depth = dblTemp;
                this.steelChannel.Breadth = Convert.ToDouble( angleArr[1] );
            }
            else if( this.radPipe.Checked )
            {
                this.steelPipe.Description = this.steelPipeList[this.cmbPipe.SelectedIndex].Description;
                this.steelPipe.OutsideDiameter = this.steelPipeList[this.cmbPipe.SelectedIndex].OutsideDiameter;
                this.steelPipe.Thickness = this.steelPipeList[this.cmbPipe.SelectedIndex].Thickness;
                this.steelPipe.Weight = this.steelPipeList[this.cmbPipe.SelectedIndex].Weight;
            }
            else
            {
                this.steelAngle.Width = dblTemp;
                this.steelAngle.Thickness = Convert.ToDouble( angleArr[2] );
                this.steelAngle.CGLine = Convert.ToDouble( this.txtCGOffset.Text );
                this.steelAngle.BoltLine = Convert.ToDouble( this.txtYOffset.Text );
            }

            this.steelMemberOverlap = Convert.ToDouble( this.txtOverlap.Text );
            this.edgeOffset = Convert.ToDouble( this.txtEdgeOffset.Text );
            this.HoleCenterX = Convert.ToDouble( this.txtXOffset.Text );
            this.HoleDia = Convert.ToDouble( this.txtHoleDia.Text );

            if( this.radType1.Checked )
            {
                this.bracingType = BracingLayoutType.CrossLayout;
            }
            else if( this.radType2.Checked )
            {
                this.bracingType = BracingLayoutType.VeeLayout;
            }
            else if( this.radType3.Checked )
            {
                this.bracingType = BracingLayoutType.SingleMemberLayout;
            }
            else if( this.radType4.Checked )
            {
                this.bracingType = BracingLayoutType.HalfMemberLayout;
            }
            else
            {
                this.bracingType = BracingLayoutType.None;
            }

            if( !double.TryParse( this.txtCenterPlateOffset.Text, out dblTemp ) || dblTemp <= 0 )
            {
                if( this.bracingType == BracingLayoutType.VeeLayout )
                {
                    MessageBox.Show( "Center plate offset is not valid", "Steel Detailing" );
                    return;
                }
            }

            this.centerPlateOffset = dblTemp;

            if( !double.TryParse( this.txtPlateOffset.Text, out dblTemp ) || dblTemp <= 0 )
            {
                if( this.bracingType == BracingLayoutType.VeeLayout )
                {
                    MessageBox.Show( "Plate extend offset is not valid", "Steel Detailing" );
                    return;
                }
            }

            this.plateExtendOffset = dblTemp;

            if( this.radSimpleAngle.Checked )
            {
                this.crossMemberType = BracingMemberType.SimpleAngle;
            }
            else if( this.radStarAngle.Checked )
            {
                this.crossMemberType = BracingMemberType.StarAngle;
            }
            else if( this.radBackToBackAngle.Checked )
            {
                this.crossMemberType = BracingMemberType.BackToBackAngle;
            }
            else if( this.radBoxChannel.Checked )
            {
                this.crossMemberType = BracingMemberType.BoxChannel;
            }
            else if( this.radBackToBackChannel.Checked )
            {
                this.crossMemberType = BracingMemberType.BackToBackChannel;
            }
            else if( this.radPipe.Checked )
            {
                this.crossMemberType = BracingMemberType.Pipe;
            }

            if( this.crossMemberType != BracingMemberType.SimpleAngle )
            {
                if( !double.TryParse( this.txtStarPlateThk.Text, out dblTemp ) || dblTemp <= 0 )
                {
                    MessageBox.Show( "Star plate thickness is not valid", "Steel Detailing" );
                    return;
                }

                this.starPlateThickness = dblTemp;


                if( !double.TryParse( this.txtStarPlateOffset.Text, out dblTemp ) || dblTemp <= 0 )
                {
                    MessageBox.Show( "Star plate offset is not valid", "Steel Detailing" );
                    return;
                }

                this.starPlateOffset = dblTemp;

                if( !double.TryParse( this.txtStarPlateWidth.Text, out dblTemp ) || dblTemp <= 0 )
                {
                    MessageBox.Show( "Star plate width is not valid", "Steel Detailing" );
                    return;
                }

                this.starPlateWidth = dblTemp;
            }

            this.noOfHoles = Convert.ToInt32( this.txtNoOfHoles.Text );
            this.holePitch = Convert.ToDouble( this.txtPitch.Text );

            this.addLengthDimension = this.chkAddLengthDimensions.Checked;

            this.oblongHoleCenterOffset = 0;
            if( this.chkOblongHole.Checked )
            {
                this.oblongHoleCenterOffset = Convert.ToInt32( this.txtOblongCenterOffset.Text );
            }

            this.DialogResult = DialogResult.OK;
        }

        private void chkOblongHole_CheckedChanged( object sender, EventArgs e )
        {
            this.txtOblongCenterOffset.Enabled = this.chkOblongHole.Checked;
        }

        private void chkStarMember_CheckedChanged( object sender, EventArgs e )
        {
            bool channelSelected = this.radBoxChannel.Checked || this.radBackToBackChannel.Checked;
            bool angleSelected = this.radStarAngle.Checked || this.radBackToBackAngle.Checked ||
                                 this.radSimpleAngle.Checked;

            this.lblStarPlateThk.Enabled = this.radStarAngle.Checked || this.radBackToBackAngle.Checked ||
                                           this.radBackToBackChannel.Checked || this.radBoxChannel.Checked;
            this.txtStarPlateThk.Enabled = angleSelected;
            this.lblStartPlateWidth.Enabled = angleSelected;
            this.txtStarPlateWidth.Enabled = angleSelected;
            this.lblStarPlateOffset.Enabled = angleSelected;
            this.txtStarPlateOffset.Enabled = angleSelected;


            this.cmbAngle.Visible = angleSelected;
            this.cmbChannel.Visible = channelSelected;
            this.cmbPipe.Visible = this.radPipe.Checked;
            this.txtCGOffset.Enabled = angleSelected;
            this.lblCGOffset.Enabled = angleSelected;

            if( channelSelected )
            {
                this.lblSteelSection.Text = "Channel:";
            }
            else if( this.radPipe.Checked )
            {
                this.lblSteelSection.Text = "Pipe:";
            }
            else if( angleSelected )
            {
                this.lblSteelSection.Text = "Angle:";
            }
        }

        private void cmbAngle_SelectedIndexChanged( object sender, EventArgs e )
        {
            string angleDesc = this.cmbAngle.Text.ToLower()
                .Trim()
                .Replace( " ", "" );
            foreach( AngleInformation ai in this.steelAngleList )
            {
                if( ai.Description == angleDesc )
                {
                    this.steelAngle = ai;
                    break;
                }
            }

            this.txtCGOffset.Text = this.steelAngle.CGLine.ToString();
            this.lblCGOffset.Text = "CG Back Off(" + this.txtCGOffset.Text + ")";
            this.txtYOffset.Text = this.steelAngle.BoltLine.ToString();
        }

        private void cmbChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            string angleDesc = this.cmbChannel.Text.ToLower()
                .Trim()
                .Replace( " ", "" );
            foreach( ChannelInformation ci in this.steelChannelList )
            {
                if( ci.Description == angleDesc )
                {
                    this.steelChannel = ci;
                    break;
                }
            }

            this.txtCGOffset.Text = ( this.steelChannel.Breadth * 0.5 ).ToString();
            this.lblCGOffset.Text = "CG Back Off(" + this.txtCGOffset.Text + ")";
            this.txtYOffset.Text = this.steelChannel.FlangeHole.ToString();
        }

        private void frmBracing_Load( object sender, EventArgs e )
        {
            if( this.cmbAngle.Items.Count > 0 )
                return;

            //angles
            this.steelAngleList = new List<AngleInformation>();
            string path = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly()
                .Location );
            XmlDocument doc = new XmlDocument();
            doc.Load( path + "\\AngleData.xml" );
            foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
            {
                AngleInformation ai = new AngleInformation( nde );
                this.steelAngleList.Add( ai );
                this.cmbAngle.Items.Add( ai.Description );
            }

            this.steelChannelList = new List<ChannelInformation>();
            doc = new XmlDocument();
            doc.Load( path + "\\ChannelData.xml" );
            foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
            {
                ChannelInformation ci = new ChannelInformation( nde );
                this.steelChannelList.Add( ci );
                this.cmbChannel.Items.Add( ci.Description );
            }

            this.steelPipeList = new List<PipeInformation>();
            doc = new XmlDocument();
            doc.Load( path + "\\PipeData.xml" );
            foreach( XmlNode nde in doc.DocumentElement.ChildNodes )
            {
                PipeInformation pi = new PipeInformation( nde );
                this.steelPipeList.Add( pi );
                this.cmbPipe.Items.Add( pi.Description );
            }

            this.cmbChannel.Location = new Point( this.cmbAngle.Location.X + 10, this.cmbAngle.Location.Y );
            this.cmbChannel.Width = this.cmbAngle.Width - 10;
            this.cmbPipe.Location = new Point( this.cmbAngle.Location.X + 10, this.cmbAngle.Location.Y );
            this.cmbPipe.Width = this.cmbAngle.Width - 10;
        }

        private void picType1_Click( object sender, EventArgs e )
        {
            this.radType1.Checked = true;
        }

        private void picType2_Click( object sender, EventArgs e )
        {
            this.radType2.Checked = true;
        }

        private void picType3_Click( object sender, EventArgs e )
        {
            this.radType3.Checked = true;
        }

        private void picType4_Click( object sender, EventArgs e )
        {
            this.radType4.Checked = true;
        }

        private void radBackToBackAngle_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radBackToBackChannel_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radBoxChannel_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radPipe_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radSimpleAngle_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radStarAngle_CheckedChanged( object sender, EventArgs e )
        {
            this.chkStarMember_CheckedChanged( sender, e );
        }

        private void radType1_CheckedChanged( object sender, EventArgs e )
        {
            this.lblType2.Enabled = this.radType1.Checked;
            this.txtCenterPlateOffset.Enabled = this.radType1.Checked;
        }

        private void radType2_CheckedChanged( object sender, EventArgs e )
        {
            this.lblType2.Enabled = this.radType1.Checked;
            this.txtCenterPlateOffset.Enabled = this.radType1.Checked;
        }

        private void radType3_CheckedChanged( object sender, EventArgs e )
        {
            this.lblType2.Enabled = this.radType1.Checked;
            this.txtCenterPlateOffset.Enabled = this.radType1.Checked;
        }

        private void radType4_CheckedChanged( object sender, EventArgs e )
        {
            this.lblType2.Enabled = this.radType1.Checked;
            this.txtCenterPlateOffset.Enabled = this.radType1.Checked;
        }

        #endregion

        public IBracingInput GetUserInput()
        {
            return new BracingInput
                   {
                       AddLengthDimension = this.AddLengthDimension,
                       BracingType = this.BracingType,
                       CenterPlateOffset = this.CenterPlateOffset,
                       CrossMemberType = this.CrossMemberType,
                       EdgeOffset = this.EdgeOffset,
                       HoleCenterX = this.HoleCenterX,
                       HoleDia = this.HoleDia,
                       HolePitch = this.HolePitch,
                       NoOfHoles = this.NoOfHoles,
                       OblongHoleCenterOffset = this.OblongHoleCenterOffset,
                       PlateExtendOffset = this.PlateExtendOffset,
                       StarPlateOffset = this.StarPlateOffset,
                       StarPlateThickness = this.StarPlateThickness,
                       StarPlateWidth = this.StarPlateWidth,
                       SteelAngle = this.SteelAngle,
                       SteelChannel = this.SteelChannel,
                       SteelPipe = this.SteelPipe,
                       SteelMemberOverlap = this.SteelMemberOverlap
                   };
        }
    }
}