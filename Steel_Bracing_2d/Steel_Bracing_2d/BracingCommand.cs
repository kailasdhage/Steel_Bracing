namespace Steel_Bracing_2d
{
    using System;
    using System.Windows.Forms;

    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class BracingCommand
    {
        #region private members
        private double holeDia = 20;

        private double holeCenterX = 50;

        private double holeCenterY = 35;

        private int noOfHoles = 2;

        private double holePitch = 50;

        private double oblongHoleCenterOffset = 12;

        private readonly DrawingDatabase currDwg;

        private Line vertLine;

        private Line horzLine;

        private Line centLine;

        private Line otherCenterLine;

        private Point3d centerLineEndPoint;

        private Point3d xpt1;

        private double steelMemberAngle;

        private double edgeOffset = 20;

        private double steelAngleWidth = 75;

        private double steelMemberWidth = 75;

        private double steelMemberHalfWidth = 55;

        private double steelMemberFlangeThk = 6;

        private double steelMemberWeldLength = 250;

        private ObjectId edgeLineId1 = ObjectId.Null;

        private ObjectId edgeLineId2 = ObjectId.Null;

        private BracingLayoutType bracingType = BracingLayoutType.None;

        private bool isChannelSelected;

        private bool isAngleSelected;

        private bool isPipeSelected;

        private double centerPlateOffset = 50;

        private double starPlateThickness = 8;

        private double starPlateWidth = 75;

        private double starPlateOffset = 600;

        private double plateExtendOffset = 20;

        private BracingMemberType crossMemberType = BracingMemberType.SimpleAngle;

        private string _objectsLayer = "objects";

        private string _hiddenLayer = "hidden";

        private string _dimLayer = "dimension";

        private string _textStyle = "brc25";

        private string _textLayer = "text";

        private bool addLengthDimension = true;

        private AngleInformation steelAngle = new AngleInformation();

        private ChannelInformation steelChannel = new ChannelInformation();

        private PipeInformation steelPipe = new PipeInformation();

        private Line fullAngleBottomLine;

        private Line fullAngleTopLine;

        private readonly ObjectIdCollection steelCrossMemberIds1 = new ObjectIdCollection();

        private readonly ObjectIdCollection steelCrossMemberIds2 = new ObjectIdCollection();

        private readonly ObjectIdCollection steelCrossMemberIds3 = new ObjectIdCollection();

        private readonly ObjectIdCollection cornerGussetPlateIds1 = new ObjectIdCollection();

        private readonly ObjectIdCollection cornerGussetPlateIds2 = new ObjectIdCollection();

        private readonly ObjectIdCollection cornerGussetPlateIds3 = new ObjectIdCollection();

        private readonly ObjectIdCollection cornerGussetPlateIds4 = new ObjectIdCollection();

        private readonly ObjectIdCollection centerGussetPlateIds = new ObjectIdCollection();

        private double steelMemberLength1;

        private double steelMemberLength2;

        private double steelMemberLength3;
        #endregion

        #region properties
        public IBracingInput BracingUserInput
        {
            get;
            set;
        }
        #endregion

        #region constructor
        public BracingCommand()
        {
            this.currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
            this.currDwg.GetHiddenLayers();
        }
        #endregion

        #region public methods
        public void ExecuteCommand()
        {
            this.bracingType = this.BracingUserInput.BracingType;
            this.isChannelSelected = this.BracingUserInput.CrossMemberType == BracingMemberType.BoxChannel ||
                                     this.BracingUserInput.CrossMemberType == BracingMemberType.BackToBackChannel;

            this.isAngleSelected = this.BracingUserInput.CrossMemberType == BracingMemberType.SimpleAngle ||
                                   this.BracingUserInput.CrossMemberType == BracingMemberType.StarAngle ||
                                   this.BracingUserInput.CrossMemberType == BracingMemberType.BackToBackAngle;
            this.isPipeSelected = this.BracingUserInput.CrossMemberType == BracingMemberType.Pipe;

            Line vertLine1 = this.BracingUserInput.VertLine1;
            Line vertLine2 = this.BracingUserInput.VertLine2;

            Line horzLine1 = this.BracingUserInput.HorzLine1;
            Line horzLine2 = this.BracingUserInput.HorzLine2;

            Line centLine1 = this.BracingUserInput.CentLine1;
            Line centLine2 = this.BracingUserInput.CentLine2;

            //make normalization here
            double tmpAng1 = 0;
            if( centLine1 != null )
            {
                tmpAng1 = DwgGeometry.AngleFromXAxis( centLine1.StartPoint, centLine1.EndPoint );
                if( tmpAng1 > DwgGeometry.kRad180 )
                {
                    tmpAng1 = tmpAng1 - DwgGeometry.kRad180;
                }
            }

            double tmpAng2 = 0;
            if( centLine2 != null )
            {
                tmpAng2 = DwgGeometry.AngleFromXAxis( centLine2.StartPoint, centLine2.EndPoint );
                if( tmpAng2 > DwgGeometry.kRad180 )
                {
                    tmpAng2 = tmpAng2 - DwgGeometry.kRad180;
                }
            }

            if( ( this.bracingType == BracingLayoutType.CrossLayout && tmpAng2 > tmpAng1 ) || ( this.bracingType == BracingLayoutType.VeeLayout && tmpAng2 < tmpAng1 ) )
            {
                Line tmpLine = centLine1;
                centLine1 = centLine2;
                centLine2 = tmpLine;
            }

            if( this.bracingType != BracingLayoutType.HalfMemberLayout )
            {
                if( vertLine1.StartPoint.X > vertLine2.StartPoint.X )
                {
                    Line tmpLine = vertLine1;
                    vertLine1 = vertLine2;
                    vertLine2 = tmpLine;
                }

                if( horzLine1.StartPoint.Y > horzLine2.StartPoint.Y )
                {
                    Line tmpLine = horzLine1;
                    horzLine1 = horzLine2;
                    horzLine2 = tmpLine;
                }
            }

            //Get Dimensions
            this.steelAngle = null;
            this.steelPipe = null;
            this.steelChannel = null;
            this.crossMemberType = this.BracingUserInput.CrossMemberType;
            if( this.isChannelSelected )
            {
                this.steelChannel = this.BracingUserInput.SteelChannel;
            }
            else if( this.isAngleSelected )
            {
                this.steelAngle = this.BracingUserInput.SteelAngle;
            }
            else if( this.isPipeSelected )
            {
                this.steelPipe = this.BracingUserInput.SteelPipe;
            }

            this.edgeOffset = this.BracingUserInput.EdgeOffset;
            if( this.edgeOffset <= 0 )
            {
                return;
            }

            if( this.isChannelSelected )
            {
                var channelInformation = this.steelChannel;
                if( channelInformation != null )
                {
                    this.steelMemberWidth = channelInformation.Depth;
                }
                this.steelMemberHalfWidth = this.steelMemberWidth * 0.5;
                this.steelMemberFlangeThk = this.BracingUserInput.SteelChannel.FlangeThickness;
            }
            else if( this.isAngleSelected )
            {
                this.steelMemberWidth = this.BracingUserInput.SteelAngle.Width;
                this.steelMemberHalfWidth = this.BracingUserInput.SteelAngle.Width - this.BracingUserInput.SteelAngle.CGLine;
                this.steelMemberFlangeThk = this.BracingUserInput.SteelAngle.Thickness;
            }
            else if( this.isPipeSelected )
            {
                this.steelMemberWidth = this.BracingUserInput.SteelPipe.OutsideDiameter;
                this.steelMemberHalfWidth = this.steelMemberWidth * 0.5;
                this.steelMemberFlangeThk = this.BracingUserInput.SteelPipe.Thickness;
            }

            if( this.steelMemberWidth <= 0 )
            {
                return;
            }
            if( this.steelMemberHalfWidth <= 0 )
            {
                return;
            }
            if( this.steelMemberFlangeThk <= 0 )
            {
                return;
            }

            this.steelMemberWeldLength = this.BracingUserInput.SteelMemberOverlap;
            if( this.steelMemberWeldLength <= 0 )
            {
                return;
            }

            this.noOfHoles = this.BracingUserInput.NoOfHoles;
            if( this.noOfHoles > 0 )
            {
                this.holeCenterX = this.BracingUserInput.HoleCenterX;
                if( this.holeCenterX <= 0 )
                {
                    return;
                }

                if( this.isChannelSelected )
                {
                    this.holeCenterY = this.BracingUserInput.SteelChannel.Depth * 0.5;
                }
                else if( this.isAngleSelected )
                {
                    this.holeCenterY = this.BracingUserInput.SteelAngle.BoltLine;
                }
                else if( this.isPipeSelected )
                {
                    this.holeCenterY = this.BracingUserInput.SteelPipe.OutsideDiameter * 0.5;
                }

                if( this.holeCenterY <= 0 )
                {
                    return;
                }

                this.holeDia = this.BracingUserInput.HoleDia;
                if( this.holeDia <= 0 )
                {
                    return;
                }

                this.holePitch = this.BracingUserInput.HolePitch;
                if( this.holePitch <= 0 )
                {
                    return;
                }
            }

            this.bracingType = this.BracingUserInput.BracingType;

            this.centerPlateOffset = this.BracingUserInput.CenterPlateOffset;
            if( this.centerPlateOffset <= 0 && this.bracingType == BracingLayoutType.VeeLayout )
            {
                return;
            }

            this.starPlateThickness = this.BracingUserInput.StarPlateThickness;
            this.starPlateOffset = this.BracingUserInput.StarPlateOffset;
            this.starPlateWidth = this.BracingUserInput.StarPlateWidth;
            this.plateExtendOffset = this.BracingUserInput.PlateExtendOffset;

            this.addLengthDimension = this.BracingUserInput.AddLengthDimension;
            this.oblongHoleCenterOffset = this.BracingUserInput.OblongHoleCenterOffset;
            //star member selected
            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                this.steelAngleWidth = this.steelMemberWidth;
                this.steelMemberWidth = this.steelAngleWidth + this.steelAngleWidth + this.starPlateThickness;
                this.steelMemberHalfWidth = this.steelMemberWidth * 0.5;
            }

            Line topHorzLine = horzLine1;
            Line bottomHorzLine = horzLine2;

            if( this.bracingType == BracingLayoutType.HalfMemberLayout )
            {
                //extra bracing goes here
                this.DrawBracing5( vertLine1, horzLine1, centLine1, horzLine2, vertLine2 );
            }
            else if( this.bracingType == BracingLayoutType.SingleMemberLayout )
            {
                //single member for gallery
                double ang = DwgGeometry.AngleFromXAxis( centLine1.StartPoint, centLine1.EndPoint );
                if( ang > DwgGeometry.kRad180 )
                {
                    ang = ang - DwgGeometry.kRad180;
                }

                topHorzLine = horzLine1;
                bottomHorzLine = horzLine2;
                if( ang < DwgGeometry.kRad90 && topHorzLine.StartPoint.Y < bottomHorzLine.StartPoint.Y )
                {
                    topHorzLine = horzLine2;
                    bottomHorzLine = horzLine1;
                }

                this.DrawBracingType3( vertLine1, bottomHorzLine, topHorzLine, centLine1, vertLine2, this.steelCrossMemberIds1, ref this.steelMemberLength1 );
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    this.currDwg.Erase( this.edgeLineId1 );
                    this.currDwg.Erase( this.edgeLineId2 );
                }
            }
            else if( this.bracingType == BracingLayoutType.VeeLayout )
            {
                // Triangle arrangement goes here
                Point3d ccpt = this.GetLineIntersectionPoint( centLine1, centLine2 );
                Point3d tmp1 = this.GetLineIntersectionPoint( centLine1, horzLine1 );
                Point3d tmp2 = this.GetLineIntersectionPoint( centLine1, horzLine2 );
                topHorzLine = horzLine1;
                bottomHorzLine = horzLine2;
                if( ccpt.DistanceTo( tmp1 ) > ccpt.DistanceTo( tmp2 ) )
                {
                    topHorzLine = horzLine2;
                    bottomHorzLine = horzLine1;
                }

                var centerVerticalLine = new Line( ccpt, DwgGeometry.GetPointPolar( ccpt, DwgGeometry.kRad270, 10000 ) );

                this.DrawBracingType3( vertLine1, bottomHorzLine, topHorzLine, centLine1, centerVerticalLine, this.steelCrossMemberIds1, ref this.steelMemberLength1 );
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    this.currDwg.Erase( this.edgeLineId1 );
                    this.currDwg.Erase( this.edgeLineId2 );
                }
                this.edgeLineId1 = ObjectId.Null;
                this.edgeLineId2 = ObjectId.Null;

                this.DrawBracingType3( vertLine2, bottomHorzLine, topHorzLine, centLine2, centerVerticalLine, this.steelCrossMemberIds2, ref this.steelMemberLength2 );
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    this.currDwg.Erase( this.edgeLineId1 );
                    this.currDwg.Erase( this.edgeLineId2 );
                }
            }
            else if( this.bracingType == BracingLayoutType.CrossLayout )
            {
                //cross arrangement goes here
                this.DrawBracing( vertLine1, horzLine1, vertLine2, horzLine2, centLine1, centLine2 );
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    this.currDwg.Erase( this.edgeLineId1 );
                    this.currDwg.Erase( this.edgeLineId2 );
                }

                this.edgeLineId1 = ObjectId.Null;
                this.edgeLineId2 = ObjectId.Null;

                this.DrawBracing( vertLine1, horzLine1, vertLine2, horzLine2, centLine2, centLine1 );
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    this.currDwg.Erase( this.edgeLineId1 );
                    this.currDwg.Erase( this.edgeLineId2 );
                }
            }
            else
            {
                //we have got bad bracing type 
                return;
            }
        }
        #endregion

        #region private methods

        private void DrawBracing( Line vertLine1, Line horzLine1, Line vertLine2, Line horzLine2, Line centLine1, Line otherCenterLine )
        {
            //# _vertLine1, _centLine1 , _horzLineF1 
            //# _vertLine2, _centLine1 , _horzLineF2
            //We have to decide which horz line to be selected in above group, we have no confusion about vert and center lines.
            Line horzLineF1 = null;
            Line horzLineF2 = null;

            Point3d p1 = this.GetLineIntersectionPoint( centLine1, horzLine1 );
            Point3d p2 = this.GetLineIntersectionPoint( centLine1, horzLine2 );

            try
            {
                Point3d c1 = this.GetLineIntersectionPoint( centLine1, vertLine1 );
                Point3d c2 = this.GetLineIntersectionPoint( centLine1, vertLine2 );

                if( c1.DistanceTo( p1 ) < c1.DistanceTo( p2 ) )
                {
                    horzLineF1 = horzLine1;
                    horzLineF2 = horzLine2;
                }
                else
                {
                    horzLineF1 = horzLine2;
                    horzLineF2 = horzLine1;
                }

                //#1
                this.vertLine = vertLine2;
                this.horzLine = horzLineF2;
                this.centLine = centLine1;
                this.otherCenterLine = otherCenterLine;
                this.edgeLineId1 = ObjectId.Null;
                this.edgeLineId2 = ObjectId.Null;
                int quad1 = this.DrawBrace();

                //#2
                this.vertLine = vertLine1;
                this.horzLine = horzLineF1;
                this.centLine = centLine1;

                int quad2 = this.DrawBrace();

                //draw steel member and center line
                var l1 = this.currDwg.GetEntity( this.edgeLineId1 ) as Line;
                var l2 = this.currDwg.GetEntity( this.edgeLineId2 ) as Line;

                if( DwgGeometry.AngleFromXAxis( l1.StartPoint, l2.StartPoint ) > DwgGeometry.AngleFromXAxis( l2.StartPoint, l1.StartPoint ) )
                {
                    var tmp = new Line( l1.StartPoint, l1.EndPoint );
                    l1 = new Line( l2.StartPoint, l2.EndPoint );
                    l2 = tmp;
                }

                if( this.bracingType == BracingLayoutType.None || ( this.bracingType == BracingLayoutType.CrossLayout && ( quad1 == 2 || quad1 == 4 ) && ( quad2 == 2 || quad2 == 4 ) ) )
                {
                    if( this.crossMemberType == BracingMemberType.StarAngle )
                    {
                        Point3d spt1 = this.GetLineIntersectionPoint( l1, this.centLine );
                        Point3d ept1 = this.GetLineIntersectionPoint( l2, this.centLine );

                        double angOtherCenLine = DwgGeometry.AngleFromXAxis( otherCenterLine.StartPoint, otherCenterLine.EndPoint );
                        if( angOtherCenLine > DwgGeometry.kRad180 )
                        {
                            angOtherCenLine = angOtherCenLine - DwgGeometry.kRad180;
                        }
                        double cenOffset = ( this.steelMemberWidth * 0.5 ) + this.plateExtendOffset + this.centerPlateOffset;
                        var centerOffSet1 = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, angOtherCenLine + DwgGeometry.kRad90, cenOffset ),
                            DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, angOtherCenLine + DwgGeometry.kRad90, cenOffset ) );

                        var centerOffSet2 = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, angOtherCenLine - DwgGeometry.kRad90, cenOffset ),
                            DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, angOtherCenLine - DwgGeometry.kRad90, cenOffset ) );

                        //top side angle
                        double ang1 = DwgGeometry.AngleFromXAxis( spt1, ept1 ) + DwgGeometry.kRad90;
                        var baseLine1 = new Line( DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( spt1, ang1 - DwgGeometry.kRad90, this.steelMemberWeldLength ), ang1, this.starPlateThickness * 0.5 ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ept1, ang1, this.starPlateThickness * 0.5 ), ang1 + DwgGeometry.kRad90, this.steelMemberWeldLength ) );

                        Point3d mpt1 = this.GetLineIntersectionPoint( baseLine1, centerOffSet1 );
                        Point3d mpt2 = this.GetLineIntersectionPoint( baseLine1, centerOffSet2 );

                        baseLine1.EndPoint = mpt1;
                        baseLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLine1 ) );

                        var baseLine11 = new Line( DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( spt1, ang1 - DwgGeometry.kRad90, this.steelMemberWeldLength ), ang1, this.starPlateThickness * 0.5 ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ept1, ang1, this.starPlateThickness * 0.5 ), ang1 + DwgGeometry.kRad90, this.steelMemberWeldLength ) );
                        baseLine11.StartPoint = mpt2;
                        baseLine11.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLine11 ) );

                        var baseLineMiddle = new Line( mpt1, mpt2 );
                        baseLineMiddle.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLineMiddle ) );

                        var baseLineH1 = new Line( DwgGeometry.GetPointPolar( spt1, ang1, this.starPlateThickness * 0.5 ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( spt1, ang1 - DwgGeometry.kRad90, this.steelMemberWeldLength ), ang1, this.starPlateThickness * 0.5 ) );
                        baseLineH1.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLineH1 ) );

                        var baseLineH2 = new Line( DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ept1, ang1, this.starPlateThickness * 0.5 ), ang1 + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( ept1, ang1, this.starPlateThickness * 0.5 ) );
                        baseLineH2.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLineH2 ) );

                        var thkLine1 = new Line( DwgGeometry.GetPointPolar( baseLineH1.StartPoint, ang1, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( baseLineH2.EndPoint, ang1, this.steelMemberFlangeThk ) );
                        thkLine1.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( thkLine1 ) );

                        if( this.isChannelSelected || this.isPipeSelected )
                        {
                            var channelThkLine1 = new Line( DwgGeometry.GetPointPolar( baseLineH1.StartPoint, ang1 + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ),
                                DwgGeometry.GetPointPolar( baseLineH2.EndPoint, ang1 + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ) );
                            channelThkLine1.Layer = this._hiddenLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( channelThkLine1 ) );
                        }

                        var topLine1 = new Line( DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( spt1, ang1 - DwgGeometry.kRad90, this.steelMemberWeldLength ), ang1, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                            DwgGeometry.GetPointPolar( baseLine1.EndPoint, ang1, this.steelAngleWidth ) );

                        mpt1 = this.GetLineIntersectionPoint( topLine1, centerOffSet1 );
                        mpt2 = this.GetLineIntersectionPoint( topLine1, centerOffSet2 );

                        topLine1.EndPoint = mpt1;
                        topLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( topLine1 ) );

                        var topLine11 = new Line(
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ept1, DwgGeometry.AngleFromXAxis( ept1, spt1 ), this.steelMemberWeldLength ), ang1, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                            DwgGeometry.GetPointPolar( baseLine1.EndPoint, ang1, this.steelAngleWidth ) );
                        topLine11.EndPoint = mpt2;
                        topLine11.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( topLine11 ) );

                        var baseLineMiddle1 = new Line( mpt1, mpt2 );
                        baseLineMiddle1.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLineMiddle1 ) );

                        var topLineH1 = new Line( DwgGeometry.GetPointPolar( spt1, ang1, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( spt1, ang1 - DwgGeometry.kRad90, this.steelMemberWeldLength ), ang1, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                        topLineH1.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( topLineH1 ) );

                        var topLineH2 = new Line( DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ept1, ang1, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth ), ang1 + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( ept1, ang1, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth ) );
                        topLineH2.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( topLineH2 ) );

                        var edgeLine1 = new Line( baseLineH1.StartPoint, topLineH1.StartPoint );
                        edgeLine1.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( edgeLine1 ) );

                        var edgeLine2 = new Line( baseLineH2.EndPoint, topLineH2.EndPoint );
                        edgeLine2.Layer = this._hiddenLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( edgeLine2 ) );

                        //bottom side angle
                        double ang2 = DwgGeometry.AngleFromXAxis( ept1, spt1 ) + DwgGeometry.kRad90;
                        var baseLine2 = new Line( DwgGeometry.GetPointPolar( spt1, ang2, this.starPlateThickness * 0.5 ),
                            DwgGeometry.GetPointPolar( ept1, ang2, this.starPlateThickness * 0.5 ) );
                        baseLine2.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( baseLine2 ) );

                        var thkLine2 = new Line( DwgGeometry.GetPointPolar( baseLine2.StartPoint, ang2, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( baseLine2.EndPoint, ang2, this.steelMemberFlangeThk ) );
                        thkLine2.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( thkLine2 ) );

                        if( this.isChannelSelected || this.isPipeSelected )
                        {
                            var channelThkLine2 = new Line( DwgGeometry.GetPointPolar( baseLine2.StartPoint, ang2 + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ),
                                DwgGeometry.GetPointPolar( baseLine2.EndPoint, ang2 + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ) );
                            channelThkLine2.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( channelThkLine2 ) );
                        }

                        var topLine2 = new Line( DwgGeometry.GetPointPolar( baseLine2.StartPoint, ang2, this.steelAngleWidth ),
                            DwgGeometry.GetPointPolar( baseLine2.EndPoint, ang2, this.steelAngleWidth ) );
                        topLine2.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( topLine2 ) );

                        var edgeLine11 = new Line( baseLine2.StartPoint, topLine2.StartPoint );
                        edgeLine11.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( edgeLine11 ) );

                        var edgeLine22 = new Line( baseLine2.EndPoint, topLine2.EndPoint );
                        edgeLine22.Layer = this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( edgeLine22 ) );

                        //draw star plates
                        this.DrawStarPlates( spt1, DwgGeometry.GetMidPoint( spt1, ept1 ) );
                        this.DrawStarPlates( DwgGeometry.GetMidPoint( spt1, ept1 ), ept1 );

                        this.steelMemberLength3 = topLine2.StartPoint.DistanceTo( topLine2.EndPoint );
                        if( this.addLengthDimension )
                        {
                            double lineAng = DwgGeometry.AngleFromXAxis( topLine2.StartPoint, topLine2.EndPoint );
                            Point3d ipt = DwgGeometry.Inters( topLine2.EndPoint, DwgGeometry.GetPointPolar( topLine2.EndPoint, lineAng + DwgGeometry.kRad90, this.steelAngleWidth ),
                                centLine1.StartPoint, centLine1.EndPoint );
                            double dimAng = DwgGeometry.AngleFromXAxis( ipt, topLine2.EndPoint );
                            //add dimesion here
                            var dimFullLength = new AlignedDimension( topLine2.StartPoint, topLine2.EndPoint,
                                DwgGeometry.GetPointPolar( topLine2.EndPoint, dimAng,
                                    this.steelMemberWeldLength * 1.25 ), "", ObjectId.Null );
                            dimFullLength.Layer = this._dimLayer;
                            this.currDwg.AddEntity( dimFullLength );
                        }
                    }
                    else
                    {
                        if( l1.StartPoint.DistanceTo( l2.StartPoint ) < l1.StartPoint.DistanceTo( l2.EndPoint ) )
                        {
                            this.fullAngleBottomLine = new Line( l1.StartPoint, l2.StartPoint );
                            this.fullAngleBottomLine.Layer = this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( this.fullAngleBottomLine ) );
                        }
                        else
                        {
                            this.fullAngleBottomLine = new Line( l1.EndPoint, l2.StartPoint );
                            this.fullAngleBottomLine.Layer = this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( this.fullAngleBottomLine ) );
                        }

                        if( l1.EndPoint.DistanceTo( l2.EndPoint ) < l1.EndPoint.DistanceTo( l2.StartPoint ) )
                        {
                            this.fullAngleTopLine = new Line( l1.EndPoint, l2.EndPoint );
                            this.fullAngleTopLine.Layer = this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( this.fullAngleTopLine ) );
                        }
                        else
                        {
                            this.fullAngleTopLine = new Line( l1.StartPoint, l2.EndPoint );
                            this.fullAngleTopLine.Layer = this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( this.fullAngleTopLine ) );
                        }

                        Point3d px1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                        Point3d px2 = this.GetLineIntersectionPoint( this.centLine, l2 );
                        Point3d pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        Point3d pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );

                        double tempDist = l1.StartPoint.DistanceTo( pt1 ) < l1.EndPoint.DistanceTo( pt1 ) ? l1.StartPoint.DistanceTo( pt1 ) : l1.EndPoint.DistanceTo( pt1 );
                        if( Math.Abs( tempDist - this.steelMemberFlangeThk ) > 0.001 )
                        {
                            pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                            pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        }

                        this.steelMemberLength3 = px1.DistanceTo( px2 );

                        var angleThkLine = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, px1 ), this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, px2 ), this.steelMemberFlangeThk ) );
                        angleThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( angleThkLine ) );

                        if( this.isChannelSelected || this.isPipeSelected )
                        {
                            var channelThkLine = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, px1 ), this.steelMemberWidth - this.steelMemberFlangeThk ),
                                DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, px2 ), this.steelMemberWidth - this.steelMemberFlangeThk ) );
                            channelThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                            this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( channelThkLine ) );
                        }

                        if( this.addLengthDimension )
                        {
                            double lineAng = DwgGeometry.AngleFromXAxis( this.fullAngleTopLine.StartPoint, this.fullAngleTopLine.EndPoint );
                            Point3d ipt = DwgGeometry.Inters( this.fullAngleTopLine.EndPoint, DwgGeometry.GetPointPolar( this.fullAngleTopLine.EndPoint, lineAng + DwgGeometry.kRad90, this.steelAngleWidth ),
                                centLine1.StartPoint, centLine1.EndPoint );
                            double dimAng = DwgGeometry.AngleFromXAxis( ipt, this.fullAngleTopLine.EndPoint );
                            //add dimesion here
                            var dimFullLength = new AlignedDimension( this.fullAngleTopLine.StartPoint, this.fullAngleTopLine.EndPoint,
                                DwgGeometry.GetPointPolar( this.fullAngleTopLine.EndPoint, dimAng,
                                    this.steelMemberWeldLength * 1.25 ), "", ObjectId.Null );
                            dimFullLength.Layer = this._dimLayer;
                            this.currDwg.AddEntity( dimFullLength );
                        }
                    }

                    Point3d pk1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                    Point3d pk2 = this.GetLineIntersectionPoint( this.centLine, l2 );

                    //add dimesion here
                    var dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( pk1, DwgGeometry.AngleFromXAxis( pk1, pk2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                        DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pk1, DwgGeometry.AngleFromXAxis( pk1, pk2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( pk1, pk2 ), this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( pk1, DwgGeometry.AngleFromXAxis( pk1, pk2 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                        this.steelMemberWeldLength.ToString(), ObjectId.Null );

                    dimWeldLength.Layer = this._dimLayer;
                    this.currDwg.AddEntity( dimWeldLength );

                    dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( pk2, DwgGeometry.AngleFromXAxis( pk2, pk1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                        DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pk2, DwgGeometry.AngleFromXAxis( pk2, pk1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( pk2, pk1 ), this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( pk2, DwgGeometry.AngleFromXAxis( pk2, pk1 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                        this.steelMemberWeldLength.ToString(), ObjectId.Null );

                    dimWeldLength.Layer = this._dimLayer;
                    this.currDwg.AddEntity( dimWeldLength );

                    //add text here
                    var angDescText = new DBText();
                    angDescText.SetDatabaseDefaults( CadApplication.CurrentDatabase );
                    angDescText.TextString = this.GetCrossMemberDescription();
                    angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
                    angDescText.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
                    angDescText.Layer = this._textLayer;
                    angDescText.Position = DwgGeometry.GetPointPolar(
                        DwgGeometry.GetPointPolar( pk1, DwgGeometry.AngleFromXAxis( pk1, pk2 ), pk1.DistanceTo( pk2 ) * 0.25 ),
                        DwgGeometry.AngleFromXAxis( pk1, pk2 ) + DwgGeometry.kRad90, ( this.steelMemberWidth - this.steelMemberHalfWidth ) + this.currDwg.Dimscale );
                    angDescText.Rotation = DwgGeometry.AngleFromXAxis( pk1, pk2 );
                    if( angDescText.Rotation > DwgGeometry.kRad90 && angDescText.Rotation < DwgGeometry.kRad270 )
                    {
                        angDescText.Rotation -= DwgGeometry.kRad180;
                    }
                    this.currDwg.AddEntity( angDescText );
                }
                else if( this.bracingType == BracingLayoutType.CrossLayout && ( quad1 == 1 || quad1 == 3 ) && ( quad2 == 1 || quad2 == 3 ) )
                {
                    if( this.crossMemberType == BracingMemberType.StarAngle )
                    {
                        //need draw center plate  as well
                        Point3d spt1 = this.GetLineIntersectionPoint( l1, this.centLine );
                        Point3d ept1 = this.GetLineIntersectionPoint( l2, this.centLine );

                        Point3d endPt1 = this.GetSteelMemberStarEndPoint( centLine1, otherCenterLine );
                        Point3d startPt1 = this.GetSteelMemberStarStartPoint( centLine1, otherCenterLine );

                        if( spt1.DistanceTo( endPt1 ) > spt1.DistanceTo( startPt1 ) )
                        {
                            Point3d swapTemp = endPt1;
                            endPt1 = startPt1;
                            startPt1 = swapTemp;
                        }

                        //bottom side angle which is easy 
                        this.DrawStarBottomAngle( spt1, endPt1, this.steelCrossMemberIds1 );
                        this.DrawStarBottomAngle( startPt1, ept1, this.steelCrossMemberIds2 );

                        //top-lower angle 
                        this.DrawStarTopAngle( spt1, endPt1, this.steelCrossMemberIds1 );
                        this.DrawStarTopAngle( startPt1, ept1, this.steelCrossMemberIds2 );

                        //draw star plates
                        this.DrawStarPlates( spt1, endPt1 );
                        this.DrawStarPlates( startPt1, ept1 );

                        //draw center plate for Star angle
                        this.DrawStarCenterPlate( centLine1, otherCenterLine, endPt1, startPt1 );

                        //end of first angle
                        this.DrawCenterPlateHoles( endPt1, DwgGeometry.AngleFromXAxis( endPt1, spt1 ), 3 );

                        //start of other angle
                        this.DrawCenterPlateHoles( startPt1, DwgGeometry.AngleFromXAxis( startPt1, ept1 ), 1 );

                        //determine bottom and top lines here
                        Point3d px1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                        Point3d px2 = this.GetLineIntersectionPoint( this.centLine, l2 );

                        //add dimesion here
                        var dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px1, px2 ), this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            this.steelMemberWeldLength.ToString(), ObjectId.Null );

                        dimWeldLength.Layer = this._dimLayer;
                        this.currDwg.AddEntity( dimWeldLength );

                        dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            this.steelMemberWeldLength.ToString(), ObjectId.Null );

                        dimWeldLength.Layer = this._dimLayer;
                        this.currDwg.AddEntity( dimWeldLength );
                    }
                    else
                    {
                        //draw upper side of angle
                        Point3d ccpt1 = this.GetLineIntersectionPoint( this.centLine, this.otherCenterLine );

                        double ang = DwgGeometry.AngleFromXAxis( this.otherCenterLine.StartPoint, this.otherCenterLine.EndPoint );
                        if( ang > DwgGeometry.kRad180 )
                        {
                            ang = ang - DwgGeometry.kRad180;
                        }

                        var loff1 = new Line( DwgGeometry.GetPointPolar( ccpt1, ang + DwgGeometry.kRad90, this.steelMemberHalfWidth + this.plateExtendOffset ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ccpt1, ang + DwgGeometry.kRad90, this.steelMemberHalfWidth + this.plateExtendOffset ), ang, 1000 ) );

                        //determine bottom and top lines here
                        Point3d px1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                        Point3d px2 = this.GetLineIntersectionPoint( this.centLine, l2 );
                        Point3d pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        Point3d pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );

                        //add dimesion here
                        var dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px1, px2 ), this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            this.steelMemberWeldLength.ToString(), ObjectId.Null );

                        dimWeldLength.Layer = this._dimLayer;
                        this.currDwg.AddEntity( dimWeldLength );

                        dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                            this.steelMemberWeldLength.ToString(), ObjectId.Null );

                        dimWeldLength.Layer = this._dimLayer;
                        this.currDwg.AddEntity( dimWeldLength );

                        var angleBottomLine = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, px1 ), this.steelMemberWidth ),
                            DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, px2 ), this.steelMemberWidth ) );
                        var angleTopLine = new Line( pt1, pt2 );

                        Point3d q1 = this.GetLineIntersectionPoint( angleTopLine, loff1 );
                        Point3d q2 = this.GetLineIntersectionPoint( angleBottomLine, loff1 );

                        double curr_ang = DwgGeometry.AngleFromXAxis( centLine1.StartPoint, centLine1.EndPoint );
                        if( curr_ang > DwgGeometry.kRad180 )
                        {
                            curr_ang = curr_ang - DwgGeometry.kRad180;
                        }

                        Line bottomEdge = l2;
                        Line topEdge = l1;
                        if( DwgGeometry.AngleFromXAxis( l1.StartPoint, q1 ) < DwgGeometry.AngleFromXAxis( l2.StartPoint, q1 ) )
                        {
                            bottomEdge = l1;
                            topEdge = l2;
                        }

                        Point3d edgePoint1 = q1;
                        if( q2.DistanceTo( DwgGeometry.GetClosestPoint( q2, bottomEdge.StartPoint, bottomEdge.EndPoint ) ) <
                            q1.DistanceTo( DwgGeometry.GetClosestPoint( q1, bottomEdge.StartPoint, bottomEdge.EndPoint ) ) )
                        {
                            edgePoint1 = q2;
                        }

                        var tmpEdgeDisp = new Line( edgePoint1, DwgGeometry.GetPointPolar( edgePoint1, curr_ang + DwgGeometry.kRad90, 1000 ) );

                        Point3d midQpt = this.GetLineIntersectionPoint( centLine1, tmpEdgeDisp );
                        Point3d edgePoint2 = DwgGeometry.GetPointPolar( edgePoint1, DwgGeometry.AngleFromXAxis( edgePoint1, midQpt ), this.steelMemberWidth );

                        var angEdgeLine1 = new Line( edgePoint1, edgePoint2 );
                        angEdgeLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds1.Add( this.currDwg.AddEntity( angEdgeLine1 ) );

                        px1 = this.GetLineIntersectionPoint( this.centLine, angEdgeLine1 );
                        px2 = this.GetLineIntersectionPoint( this.centLine, bottomEdge );

                        //add text here
                        var angDescText = new DBText();
                        angDescText.SetDatabaseDefaults( CadApplication.CurrentDatabase );
                        angDescText.TextString = this.GetCrossMemberDescription();
                        angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
                        angDescText.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
                        angDescText.Layer = this._textLayer;
                        angDescText.Position = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( px2, px1 ),
                            DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, ( this.steelMemberWidth - this.steelMemberHalfWidth ) + this.currDwg.Dimscale );
                        angDescText.Rotation = DwgGeometry.AngleFromXAxis( px2, px1 );
                        if( angDescText.Rotation > DwgGeometry.kRad90 && angDescText.Rotation < DwgGeometry.kRad270 )
                        {
                            angDescText.Rotation -= DwgGeometry.kRad180;
                        }
                        this.currDwg.AddEntity( angDescText );

                        pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );

                        if( DwgGeometry.AngleFromXAxis( px1, px2 ) > DwgGeometry.AngleFromXAxis( px2, px1 ) )
                        {
                            pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                            pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        }

                        this.steelMemberLength1 = pt1.DistanceTo( pt2 );

                        var angTopLine1 = new Line( pt1, pt2 );
                        angTopLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds1.Add( this.currDwg.AddEntity( angTopLine1 ) );

                        var angThkLine = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberFlangeThk ) );
                        angThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds1.Add( this.currDwg.AddEntity( angThkLine ) );

                        if( this.isChannelSelected || this.isPipeSelected )
                        {
                            var channelThkLine = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ),
                                DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ) );
                            channelThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                            this.steelCrossMemberIds1.Add( this.currDwg.AddEntity( channelThkLine ) );
                        }

                        var angBottomLine1 = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth ),
                            DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth ) );
                        angBottomLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds1.Add( this.currDwg.AddEntity( angBottomLine1 ) );

                        var angOverlapEdgeLine1 = new Line( DwgGeometry.GetPointPolar( angEdgeLine1.StartPoint, curr_ang + DwgGeometry.kRad180, this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( angEdgeLine1.EndPoint, curr_ang + DwgGeometry.kRad180, this.steelMemberWeldLength ) );
                        angOverlapEdgeLine1.Layer = this._hiddenLayer;
                        this.centerGussetPlateIds.Add( this.currDwg.AddEntity( angOverlapEdgeLine1 ) );

                        //draw holes here
                        this.DrawCenterPlateHoles( px1, DwgGeometry.AngleFromXAxis( px1, px2 ), 3 );

                        if( addLengthDimension )
                        {
                            this.AddLengthDimesion( angBottomLine1, true );
                        }

                        //draw upper side of angle
                        var loff2 = new Line( DwgGeometry.GetPointPolar( ccpt1, ang - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth + this.plateExtendOffset ),
                            DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( ccpt1, ang - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth + this.plateExtendOffset ), ang, 1000 ) );

                        q1 = this.GetLineIntersectionPoint( angleTopLine, loff2 );
                        q2 = this.GetLineIntersectionPoint( angleBottomLine, loff2 );

                        edgePoint1 = q1;
                        if( q2.DistanceTo( DwgGeometry.GetClosestPoint( q2, topEdge.StartPoint, topEdge.EndPoint ) ) <
                            q1.DistanceTo( DwgGeometry.GetClosestPoint( q1, topEdge.StartPoint, topEdge.EndPoint ) ) )
                        {
                            edgePoint1 = q2;
                        }

                        tmpEdgeDisp = new Line( edgePoint1, DwgGeometry.GetPointPolar( edgePoint1, curr_ang + DwgGeometry.kRad90, 1000 ) );

                        midQpt = this.GetLineIntersectionPoint( centLine1, tmpEdgeDisp );
                        edgePoint2 = DwgGeometry.GetPointPolar( edgePoint1, DwgGeometry.AngleFromXAxis( edgePoint1, midQpt ), this.steelMemberWidth );

                        angEdgeLine1 = new Line( edgePoint1, edgePoint2 );
                        angEdgeLine1.Layer = this._objectsLayer;
                        this.steelCrossMemberIds2.Add( this.currDwg.AddEntity( angEdgeLine1 ) );

                        px1 = this.GetLineIntersectionPoint( this.centLine, angEdgeLine1 );
                        px2 = this.GetLineIntersectionPoint( this.centLine, topEdge );

                        //add text here
                        angDescText = new DBText();
                        angDescText.SetDatabaseDefaults( CadApplication.CurrentDatabase );
                        angDescText.TextString = this.GetCrossMemberDescription();
                        angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
                        angDescText.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
                        angDescText.Layer = this._textLayer;
                        angDescText.Position = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( px1, px2 ),
                            DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, ( this.steelMemberWidth - this.steelMemberHalfWidth ) + this.currDwg.Dimscale );
                        angDescText.Rotation = DwgGeometry.AngleFromXAxis( px1, px2 );
                        if( angDescText.Rotation > DwgGeometry.kRad90 && angDescText.Rotation < DwgGeometry.kRad270 )
                        {
                            angDescText.Rotation -= DwgGeometry.kRad180;
                        }
                        this.currDwg.AddEntity( angDescText );

                        pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                        pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );

                        this.steelMemberLength2 = px1.DistanceTo( px2 );
                        var angTopLine2 = new Line( pt1, pt2 );
                        angTopLine2.Layer = this._objectsLayer;
                        this.steelCrossMemberIds2.Add( this.currDwg.AddEntity( angTopLine2 ) );

                        angThkLine = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberFlangeThk ) );
                        angThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds2.Add( this.currDwg.AddEntity( angThkLine ) );

                        if( this.isChannelSelected || this.isPipeSelected )
                        {
                            var channelThkLine = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ),
                                DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth - this.steelMemberFlangeThk ) );
                            channelThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                            this.steelCrossMemberIds2.Add( this.currDwg.AddEntity( channelThkLine ) );
                        }
                        var angBottomLine2 = new Line( DwgGeometry.GetPointPolar( pt1, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth ),
                            DwgGeometry.GetPointPolar( pt2, curr_ang + DwgGeometry.kRad270, this.steelMemberWidth ) );
                        angBottomLine2.Layer = this._objectsLayer;
                        this.steelCrossMemberIds2.Add( this.currDwg.AddEntity( angBottomLine2 ) );

                        var angOverlapEdgeLine2 = new Line( DwgGeometry.GetPointPolar( angEdgeLine1.StartPoint, curr_ang, this.steelMemberWeldLength ),
                            DwgGeometry.GetPointPolar( angEdgeLine1.EndPoint, curr_ang, this.steelMemberWeldLength ) );
                        angOverlapEdgeLine2.Layer = this._hiddenLayer;
                        this.centerGussetPlateIds.Add( this.currDwg.AddEntity( angOverlapEdgeLine2 ) );

                        //draw holes here
                        this.DrawCenterPlateHoles( px1, DwgGeometry.AngleFromXAxis( px1, px2 ), 1 );

                        if( addLengthDimension )
                        {
                            this.AddLengthDimesion( angBottomLine2, true );
                        }

                        //draw center plate logic goes here
                        this.DrawCenterPlate( angTopLine2, angBottomLine2, curr_ang, angOverlapEdgeLine1, angOverlapEdgeLine2 );
                    }
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( "Error: " + ex.Message + "\n" + ex.StackTrace );
            }
        }

        private void DrawBracing5( Line vertLine1, Line horzLine1, Line centLine1, Line bottomLine1, Line BottomLine2 )
        {
            try
            {
                //#1
                this.vertLine = vertLine1;
                this.horzLine = horzLine1;
                this.centLine = centLine1;
                this.otherCenterLine = null;
                this.edgeLineId1 = ObjectId.Null;
                this.edgeLineId2 = ObjectId.Null;
                int quad1 = this.DrawBrace();

                var edgeLine1 = this.currDwg.GetEntity( this.edgeLineId1 ) as Line;
                this.DrawBracingType5( bottomLine1, BottomLine2, centLine1, edgeLine1, quad1 );
            }
            catch( Exception ex )
            {
                MessageBox.Show( "Error: " + ex.Message + "\n" + ex.StackTrace );
            }
        }

        private void DrawStarCenterPlate( Line thisCenterLine, Line otherCenterLine, Point3d pt1, Point3d pt2 )
        {
            pt1 = DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt2, pt1 ), this.steelMemberWeldLength );
            pt2 = DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt1, pt2 ), this.steelMemberWeldLength );

            double thisAng = DwgGeometry.AngleFromXAxis( thisCenterLine.StartPoint, thisCenterLine.EndPoint );
            if( thisAng > DwgGeometry.kRad180 )
            {
                thisAng = thisAng - DwgGeometry.kRad180;
            }

            double otherAng = DwgGeometry.AngleFromXAxis( otherCenterLine.StartPoint, otherCenterLine.EndPoint );
            if( otherAng > DwgGeometry.kRad180 )
            {
                otherAng = otherAng - DwgGeometry.kRad180;
            }

            var l1 = new Line( pt1, DwgGeometry.GetPointPolar( pt1, thisAng + DwgGeometry.kRad90, this.steelAngleWidth ) );
            var l2 = new Line( pt2, DwgGeometry.GetPointPolar( pt2, thisAng + DwgGeometry.kRad90, this.steelAngleWidth ) );

            double cenOffset = ( this.steelMemberWidth * 0.5 ) + this.plateExtendOffset + this.centerPlateOffset;
            var centerOffSet1 = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng + DwgGeometry.kRad90, cenOffset ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng + DwgGeometry.kRad90, cenOffset ) );

            var centerOffSet2 = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng - DwgGeometry.kRad90, cenOffset ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng - DwgGeometry.kRad90, cenOffset ) );

            Point3d p1 = this.GetLineIntersectionPoint( l1, centerOffSet2 );
            Point3d p2 = this.GetLineIntersectionPoint( l2, centerOffSet2 );
            Point3d p3 = this.GetLineIntersectionPoint( l2, centerOffSet1 );
            Point3d p4 = this.GetLineIntersectionPoint( l1, centerOffSet1 );

            //chamfer lines goes here
            Point3d p11 = DwgGeometry.GetPointPolar( p1, DwgGeometry.AngleFromXAxis( p1, p2 ), this.centerPlateOffset );
            Point3d p111 = DwgGeometry.GetPointPolar( p1, DwgGeometry.AngleFromXAxis( p1, p4 ), this.centerPlateOffset );

            Point3d p22 = DwgGeometry.GetPointPolar( p2, DwgGeometry.AngleFromXAxis( p2, p3 ), this.centerPlateOffset );
            Point3d p222 = DwgGeometry.GetPointPolar( p2, DwgGeometry.AngleFromXAxis( p2, p1 ), this.centerPlateOffset );

            Point3d p33 = DwgGeometry.GetPointPolar( p3, DwgGeometry.AngleFromXAxis( p3, p4 ), this.centerPlateOffset );
            Point3d p333 = DwgGeometry.GetPointPolar( p3, DwgGeometry.AngleFromXAxis( p3, p2 ), this.centerPlateOffset );

            Point3d p44 = DwgGeometry.GetPointPolar( p4, DwgGeometry.AngleFromXAxis( p4, p1 ), this.centerPlateOffset );
            Point3d p444 = DwgGeometry.GetPointPolar( p4, DwgGeometry.AngleFromXAxis( p4, p3 ), this.centerPlateOffset );

            var pl1 = new Line( p11, p111 );
            pl1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( pl1 ) );

            var pl2 = new Line( p22, p222 );
            pl2.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( pl2 ) );

            var pl3 = new Line( p33, p333 );
            pl3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( pl3 ) );

            var pl4 = new Line( p44, p444 );
            pl4.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( pl4 ) );

            var otherLineOff1 = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, otherAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ),
                DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, otherAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ) );
            var otherLineOff2 = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, otherAng + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, otherAng + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ) );
            var bottomLine = new Line( p1, p2 );
            var topLine = new Line( p3, p4 );

            Point3d mpt1 = this.GetLineIntersectionPoint( bottomLine, otherLineOff1 );
            Point3d mpt11 = this.GetLineIntersectionPoint( topLine, otherLineOff1 );

            Point3d mpt2 = this.GetLineIntersectionPoint( bottomLine, otherLineOff2 );
            Point3d mpt22 = this.GetLineIntersectionPoint( topLine, otherLineOff2 );

            var bl1 = new Line( p11, mpt1 );
            bl1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bl1 ) );

            var bl2 = new Line( mpt1, mpt2 );
            bl2.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bl2 ) );

            var bl3 = new Line( mpt2, p222 );
            bl3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bl3 ) );

            Point3d mpt111 = DwgGeometry.GetPointPolar( mpt22, DwgGeometry.AngleFromXAxis( mpt11, mpt22 ), mpt11.DistanceTo( mpt22 ) + this.starPlateThickness );
            //top side goes here
            var tl1 = new Line( p444, mpt11 );
            tl1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tl1 ) );

            var tl2 = new Line( mpt22, mpt11 );
            tl2.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tl2 ) );

            var tl3 = new Line( mpt22, p33 );
            tl3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tl3 ) );

            //lower vertical 3 more lines here

            var bvLine1 = new Line( p111, DwgGeometry.GetPointPolar( p111, DwgGeometry.AngleFromXAxis( p111, p44 ), this.plateExtendOffset ) );
            bvLine1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bvLine1 ) );

            var bvLine2 = new Line( DwgGeometry.GetPointPolar( p111, DwgGeometry.AngleFromXAxis( p111, p44 ), this.plateExtendOffset ),
                DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, p111 ), this.starPlateThickness * 0.5 ) );
            bvLine2.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bvLine2 ) );

            var bvLine3 = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, p111 ), this.starPlateThickness * 0.5 ), p44 );
            bvLine3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( bvLine3 ) );

            //top vertical 3 more lines here
            var tvLine1 = new Line( p22, DwgGeometry.GetPointPolar( p22, DwgGeometry.AngleFromXAxis( p22, p333 ), this.plateExtendOffset ) );
            tvLine1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tvLine1 ) );

            var tvLine2 = new Line( DwgGeometry.GetPointPolar( p22, DwgGeometry.AngleFromXAxis( p22, p333 ), this.plateExtendOffset ),
                DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, p22 ), this.starPlateThickness * 0.5 ) );
            tvLine2.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tvLine2 ) );
            var tvLine3 = new Line( DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, p22 ), this.starPlateThickness * 0.5 ), p333 );
            tvLine3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( tvLine3 ) );
        }

        private void DrawStarTopAngle( Point3d spt1, Point3d endPt1, ObjectIdCollection ids )
        {
            double ang = DwgGeometry.AngleFromXAxis( spt1, endPt1 );
            Point3d angEpt1 = DwgGeometry.GetPointPolar( spt1, ang, this.steelMemberWeldLength );
            var thkLine1 = new Line( DwgGeometry.GetPointPolar( spt1, ang + DwgGeometry.kRad90, this.steelMemberFlangeThk + ( this.starPlateThickness * 0.5 ) ),
                DwgGeometry.GetPointPolar( endPt1, ang + DwgGeometry.kRad90, this.steelMemberFlangeThk + ( this.starPlateThickness * 0.5 ) ) );
            thkLine1.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( thkLine1 ) );

            var baseLine1 = new Line( DwgGeometry.GetPointPolar( spt1, ang + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                DwgGeometry.GetPointPolar( angEpt1, ang + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ) );
            baseLine1.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( baseLine1 ) );

            var topLine1 = new Line( DwgGeometry.GetPointPolar( baseLine1.StartPoint, ang + DwgGeometry.kRad90, this.steelAngleWidth ),
                DwgGeometry.GetPointPolar( baseLine1.EndPoint, ang + DwgGeometry.kRad90, this.steelAngleWidth ) );
            topLine1.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( topLine1 ) );

            var edgeLine1 = new Line( baseLine1.StartPoint, topLine1.StartPoint );
            edgeLine1.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( edgeLine1 ) );

            //top-lower - other end angle 
            Point3d angEpt11 = DwgGeometry.GetPointPolar( endPt1, ang + DwgGeometry.kRad180, this.steelMemberWeldLength );
            var baseLine11 = new Line( DwgGeometry.GetPointPolar( endPt1, ang + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                DwgGeometry.GetPointPolar( angEpt11, ang + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ) );
            baseLine11.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( baseLine11 ) );

            var topLine11 = new Line( DwgGeometry.GetPointPolar( baseLine11.StartPoint, ang + DwgGeometry.kRad90, this.steelAngleWidth ),
                DwgGeometry.GetPointPolar( baseLine11.EndPoint, ang + DwgGeometry.kRad90, this.steelAngleWidth ) );
            topLine11.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( topLine11 ) );

            var edgeLine11 = new Line( baseLine11.StartPoint, topLine11.StartPoint );
            edgeLine11.Layer = this._hiddenLayer;
            ids.Add( this.currDwg.AddEntity( edgeLine11 ) );

            var baseMiddleLine = new Line( baseLine1.EndPoint, baseLine11.EndPoint );
            baseMiddleLine.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( baseMiddleLine ) );

            var topMiddleLine = new Line( topLine1.EndPoint, topLine11.EndPoint );
            topMiddleLine.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( topMiddleLine ) );

            if( addLengthDimension )
            {
                this.AddLengthDimesion( topLine11, false );
            }
        }

        private void DrawStarBottomAngle( Point3d spt1, Point3d endPt1, ObjectIdCollection ids )
        {
            double ang2 = DwgGeometry.AngleFromXAxis( endPt1, spt1 ) + DwgGeometry.kRad90;
            var baseLine1 = new Line( DwgGeometry.GetPointPolar( spt1, ang2, this.starPlateThickness * 0.5 ),
                DwgGeometry.GetPointPolar( endPt1, ang2, this.starPlateThickness * 0.5 ) );
            baseLine1.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( baseLine1 ) );

            var thkLin1 = new Line( DwgGeometry.GetPointPolar( baseLine1.StartPoint, ang2, this.steelMemberFlangeThk ),
                DwgGeometry.GetPointPolar( baseLine1.EndPoint, ang2, this.steelMemberFlangeThk ) );
            thkLin1.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( thkLin1 ) );

            var topLine1 = new Line( DwgGeometry.GetPointPolar( baseLine1.StartPoint, ang2, this.steelAngleWidth ),
                DwgGeometry.GetPointPolar( baseLine1.EndPoint, ang2, this.steelAngleWidth ) );
            topLine1.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( topLine1 ) );

            var edgeLine1 = new Line( baseLine1.StartPoint, topLine1.StartPoint );
            edgeLine1.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( edgeLine1 ) );

            var edgeLine2 = new Line( baseLine1.EndPoint, topLine1.EndPoint );
            edgeLine2.Layer = this._objectsLayer;
            ids.Add( this.currDwg.AddEntity( edgeLine2 ) );

            if( addLengthDimension )
            {
                this.AddLengthDimesion( topLine1, true );
            }
        }

        private void DrawCenterPlate( Line angTopLine, Line angBottomLine, double currAng, Line angOverlapEdgeLine1, Line angOverlapEdgeLine2 )
        {
            //draw center plate logic goes here
            var angTopLineOff = new Line( DwgGeometry.GetPointPolar( angTopLine.StartPoint, currAng + DwgGeometry.kRad90, this.plateExtendOffset + this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( angTopLine.EndPoint, currAng + DwgGeometry.kRad90, this.plateExtendOffset + this.centerPlateOffset ) );

            var angBottomLineOff = new Line( DwgGeometry.GetPointPolar( angBottomLine.StartPoint, currAng + DwgGeometry.kRad270, this.plateExtendOffset + this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( angBottomLine.EndPoint, currAng + DwgGeometry.kRad270, this.plateExtendOffset + this.centerPlateOffset ) );

            Point3d cp1 = this.GetLineIntersectionPoint( angOverlapEdgeLine1, angBottomLineOff );
            Point3d cp2 = this.GetLineIntersectionPoint( angOverlapEdgeLine1, angTopLineOff );
            Point3d cp3 = this.GetLineIntersectionPoint( angOverlapEdgeLine2, angBottomLineOff );
            Point3d cp4 = this.GetLineIntersectionPoint( angOverlapEdgeLine2, angTopLineOff );

            var cpBottomLine = new Line( DwgGeometry.GetPointPolar( cp1, DwgGeometry.AngleFromXAxis( cp1, cp3 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp1 ), this.centerPlateOffset ) );
            Point3d ipt1 = this.GetLineIntersectionPoint( cpBottomLine, this.fullAngleBottomLine );
            Point3d ipt2 = this.GetLineIntersectionPoint( cpBottomLine, this.fullAngleTopLine );

            var hiddenLine1 = new Line( ipt1, ipt2 );
            hiddenLine1.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( hiddenLine1 ) );

            cpBottomLine.Layer = this._objectsLayer;
            cpBottomLine.EndPoint = ipt1;
            if( ipt1.DistanceTo( cpBottomLine.StartPoint ) > ipt2.DistanceTo( cpBottomLine.StartPoint ) )
            {
                cpBottomLine.EndPoint = ipt2;
            }

            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpBottomLine ) );

            if( ipt1.DistanceTo( cp3 ) > ipt2.DistanceTo( cp3 ) )
            {
                cpBottomLine = new Line( DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp1 ), this.centerPlateOffset ), ipt2 );
            }
            else
            {
                cpBottomLine = new Line( DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp1 ), this.centerPlateOffset ), ipt1 );
            }

            cpBottomLine.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpBottomLine ) );

            var cpTopLine = new Line( DwgGeometry.GetPointPolar( cp2, DwgGeometry.AngleFromXAxis( cp2, cp4 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp2 ), this.centerPlateOffset ) );
            ipt1 = this.GetLineIntersectionPoint( cpTopLine, this.fullAngleBottomLine );
            ipt2 = this.GetLineIntersectionPoint( cpTopLine, this.fullAngleTopLine );

            hiddenLine1 = new Line( ipt1, ipt2 );
            hiddenLine1.Layer = this._hiddenLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( hiddenLine1 ) );

            cpTopLine.Layer = this._objectsLayer;
            cpTopLine.EndPoint = ipt1;
            if( ipt1.DistanceTo( cpTopLine.StartPoint ) > ipt2.DistanceTo( cpTopLine.StartPoint ) )
            {
                cpTopLine.EndPoint = ipt2;
            }
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpTopLine ) );

            if( ipt1.DistanceTo( cp4 ) > ipt2.DistanceTo( cp4 ) )
            {
                cpTopLine = new Line( DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp2 ), this.centerPlateOffset ), ipt2 );
            }
            else
            {
                cpTopLine = new Line( DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp2 ), this.centerPlateOffset ), ipt1 );
            }

            cpTopLine.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpTopLine ) );

            var cpEdgeLine1 = new Line( DwgGeometry.GetPointPolar( cp1, DwgGeometry.AngleFromXAxis( cp1, cp2 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( cp1, DwgGeometry.AngleFromXAxis( cp1, cp2 ), this.centerPlateOffset ), DwgGeometry.AngleFromXAxis( cp1, cp2 ), this.plateExtendOffset ) );
            cpEdgeLine1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpEdgeLine1 ) );

            var cpEdgeLine2 = new Line( DwgGeometry.GetPointPolar( cp2, DwgGeometry.AngleFromXAxis( cp2, cp1 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( cp2, DwgGeometry.AngleFromXAxis( cp2, cp1 ), this.centerPlateOffset ), DwgGeometry.AngleFromXAxis( cp2, cp1 ), this.plateExtendOffset ) );
            cpEdgeLine2.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpEdgeLine2 ) );

            var cpEdgeLine3 = new Line( DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp4 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp4 ), this.centerPlateOffset ), DwgGeometry.AngleFromXAxis( cp3, cp4 ), this.plateExtendOffset ) );
            cpEdgeLine3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpEdgeLine3 ) );

            var cpEdgeLine4 = new Line( DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp3 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp3 ), this.centerPlateOffset ), DwgGeometry.AngleFromXAxis( cp4, cp3 ), this.plateExtendOffset ) );
            cpEdgeLine4.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( cpEdgeLine4 ) );

            var champerLine1 = new Line( DwgGeometry.GetPointPolar( cp1, DwgGeometry.AngleFromXAxis( cp1, cp3 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp1, DwgGeometry.AngleFromXAxis( cp1, cp2 ), this.centerPlateOffset ) );
            champerLine1.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( champerLine1 ) );

            var champerLine2 = new Line( DwgGeometry.GetPointPolar( cp2, DwgGeometry.AngleFromXAxis( cp2, cp1 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp2, DwgGeometry.AngleFromXAxis( cp2, cp4 ), this.centerPlateOffset ) );
            champerLine2.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( champerLine2 ) );

            var champerLine3 = new Line( DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp4 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp3, DwgGeometry.AngleFromXAxis( cp3, cp1 ), this.centerPlateOffset ) );
            champerLine3.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( champerLine3 ) );

            var champerLine4 = new Line( DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp2 ), this.centerPlateOffset ),
                DwgGeometry.GetPointPolar( cp4, DwgGeometry.AngleFromXAxis( cp4, cp3 ), this.centerPlateOffset ) );
            champerLine4.Layer = this._objectsLayer;
            this.centerGussetPlateIds.Add( this.currDwg.AddEntity( champerLine4 ) );
        }

        private void DrawBracingType3( Line vertSegment, Line bottomHorzLine, Line topHorzLine, Line centerSegment, Line centerVerticalLine, ObjectIdCollection ids, ref double segmentLength )
        {
            this.vertLine = vertSegment;
            this.horzLine = bottomHorzLine;
            this.centLine = centerSegment;
            int quad1 = this.DrawBrace();

            this.vertLine = centerVerticalLine;
            this.horzLine = topHorzLine;
            this.centLine = centerSegment;
            int quad2 = this.DrawBrace();

            //draw steel member and center line
            var l1 = this.currDwg.GetEntity( this.edgeLineId1 ) as Line;
            var l2 = this.currDwg.GetEntity( this.edgeLineId2 ) as Line;

            if( DwgGeometry.AngleFromXAxis( l1.StartPoint, l2.StartPoint ) > DwgGeometry.AngleFromXAxis( l2.StartPoint, l1.StartPoint ) )
            {
                var tmp = new Line( l1.StartPoint, l1.EndPoint );
                l1 = new Line( l2.StartPoint, l2.EndPoint );
                l2 = tmp;
            }

            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                //draw one here
                //need draw center plate  as well
                Point3d spt1 = this.GetLineIntersectionPoint( l1, this.centLine );
                Point3d ept1 = this.GetLineIntersectionPoint( l2, this.centLine );

                //bottom side angle which is easy 
                this.DrawStarBottomAngle( spt1, ept1, ids );

                //top side angle which is NOT easy 
                this.DrawStarTopAngle( spt1, ept1, ids );

                //draw star plates
                this.DrawStarPlates( spt1, ept1 );

                Point3d px1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                Point3d px2 = this.GetLineIntersectionPoint( this.centLine, l2 );
                segmentLength = px1.DistanceTo( px2 );

                //add dimesion here
                var dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                    DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px1, px2 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                    this.steelMemberWeldLength.ToString(), ObjectId.Null )
                                    {
                                        Layer = this._dimLayer
                                    };

                this.currDwg.AddEntity( dimWeldLength );

                dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                    DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                    this.steelMemberWeldLength.ToString(), ObjectId.Null )
                                {
                                    Layer = this._dimLayer
                                };

                this.currDwg.AddEntity( dimWeldLength );

                //add text here
                var angDescText = new DBText();
                angDescText.SetDatabaseDefaults( CadApplication.CurrentDatabase );
                angDescText.TextString = this.GetCrossMemberDescription();
                angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
                angDescText.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
                angDescText.Layer = this._textLayer;
                angDescText.Position = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( px1, px2 ),
                    DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, ( this.steelMemberWidth - this.steelMemberHalfWidth ) + this.currDwg.Dimscale );
                angDescText.Rotation = DwgGeometry.AngleFromXAxis( px1, px2 );
                if( angDescText.Rotation > DwgGeometry.kRad90 && angDescText.Rotation < DwgGeometry.kRad270 )
                {
                    angDescText.Rotation -= DwgGeometry.kRad180;
                }
                this.currDwg.AddEntity( angDescText );
            }
            else
            {
                Line angleBottomLine = null;
                if( l1.StartPoint.DistanceTo( l2.StartPoint ) < l1.StartPoint.DistanceTo( l2.EndPoint ) )
                {
                    angleBottomLine = new Line( l1.StartPoint, l2.StartPoint );
                    angleBottomLine.Layer = this._objectsLayer;
                    ids.Add( this.currDwg.AddEntity( angleBottomLine ) );
                }
                else
                {
                    angleBottomLine = new Line( l1.EndPoint, l2.StartPoint );
                    angleBottomLine.Layer = this._objectsLayer;
                    ids.Add( this.currDwg.AddEntity( angleBottomLine ) );
                }

                Line angleTopLine = null;
                if( l1.EndPoint.DistanceTo( l2.EndPoint ) < l1.EndPoint.DistanceTo( l2.StartPoint ) )
                {
                    angleTopLine = new Line( l1.EndPoint, l2.EndPoint );
                    angleTopLine.Layer = this._objectsLayer;
                    ids.Add( this.currDwg.AddEntity( angleTopLine ) );
                }
                else
                {
                    angleTopLine = new Line( l1.StartPoint, l2.EndPoint );
                    angleTopLine.Layer = this._objectsLayer;
                    ids.Add( this.currDwg.AddEntity( angleTopLine ) );
                }

                Point3d px1 = this.GetLineIntersectionPoint( this.centLine, l1 );
                Point3d px2 = this.GetLineIntersectionPoint( this.centLine, l2 );
                segmentLength = px1.DistanceTo( px2 );
                Point3d pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                Point3d pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );

                double tempDist = l1.StartPoint.DistanceTo( pt1 ) < l1.EndPoint.DistanceTo( pt1 ) ? l1.StartPoint.DistanceTo( pt1 ) : l1.EndPoint.DistanceTo( pt1 );
                if( Math.Abs( tempDist - this.steelMemberFlangeThk ) > 0.001 && tempDist > 0.001 )
                {
                    pt1 = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                    pt2 = DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px1, px2 ) - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth );
                }

                var angleThkLine = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, px1 ), this.steelMemberFlangeThk ),
                    DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, px2 ), this.steelMemberFlangeThk ) );
                angleThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                ids.Add( this.currDwg.AddEntity( angleThkLine ) );
                if( this.isChannelSelected || this.isPipeSelected )
                {
                    var channelThkLine = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.AngleFromXAxis( pt1, px1 ), this.steelMemberWidth - this.steelMemberFlangeThk ),
                        DwgGeometry.GetPointPolar( pt2, DwgGeometry.AngleFromXAxis( pt2, px2 ), this.steelMemberWidth - this.steelMemberFlangeThk ) );
                    channelThkLine.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                    ids.Add( this.currDwg.AddEntity( channelThkLine ) );
                }

                //add dimesion here
                var dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                    DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px1, px2 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                    this.steelMemberWeldLength.ToString(), ObjectId.Null );

                dimWeldLength.Layer = this._dimLayer;
                this.currDwg.AddEntity( dimWeldLength );

                dimWeldLength = new AlignedDimension( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                    DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberHalfWidth ), DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, this.steelMemberWeldLength ),
                    this.steelMemberWeldLength.ToString(), ObjectId.Null );

                dimWeldLength.Layer = this._dimLayer;
                this.currDwg.AddEntity( dimWeldLength );

                //add text here
                var angDescText = new DBText();
                angDescText.SetDatabaseDefaults( CadApplication.CurrentDatabase );
                angDescText.TextString = this.GetCrossMemberDescription();
                angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
                angDescText.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
                angDescText.Layer = this._textLayer;

                Point3d mp1 = DwgGeometry.GetMidPoint( px1, px2 );
                Point3d mp2 = DwgGeometry.GetMidPoint( angleThkLine.StartPoint, angleThkLine.EndPoint );
                angDescText.Position = DwgGeometry.GetPointPolar( mp1,
                    DwgGeometry.AngleFromXAxis( mp1, mp2 ), ( this.steelMemberWidth - this.steelMemberHalfWidth ) + this.currDwg.Dimscale );
                angDescText.Rotation = DwgGeometry.AngleFromXAxis( px1, px2 );
                if( angDescText.Rotation > DwgGeometry.kRad90 && angDescText.Rotation < DwgGeometry.kRad270 )
                {
                    angDescText.Rotation -= DwgGeometry.kRad180;
                }
                this.currDwg.AddEntity( angDescText );

                if( this.addLengthDimension )
                {
                    double lineAng = DwgGeometry.AngleFromXAxis( angleBottomLine.StartPoint, angleBottomLine.EndPoint );
                    Point3d ipt = DwgGeometry.Inters( angleBottomLine.EndPoint, DwgGeometry.GetPointPolar( angleBottomLine.EndPoint, lineAng + DwgGeometry.kRad90, this.steelAngleWidth ),
                        centerSegment.StartPoint, centerSegment.EndPoint );
                    double dimAng = DwgGeometry.AngleFromXAxis( ipt, angleBottomLine.EndPoint );
                    //add dimesion here
                    dimWeldLength = new AlignedDimension( angleBottomLine.StartPoint, angleBottomLine.EndPoint,
                        DwgGeometry.GetPointPolar( angleBottomLine.EndPoint, dimAng,
                            this.steelMemberWeldLength * 1.25 ), "", ObjectId.Null );
                    dimWeldLength.Layer = this._dimLayer;
                    this.currDwg.AddEntity( dimWeldLength );
                }
            }
        }

        private int DrawBrace()
        {
            Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );
            this.centerLineEndPoint = this.centLine.StartPoint;
            if( pt1.DistanceTo( this.centLine.StartPoint ) < pt1.DistanceTo( this.centLine.EndPoint ) )
            {
                this.centerLineEndPoint = this.centLine.EndPoint;
            }

            this.xpt1 = this.GetLineIntersectionPoint( this.horzLine, this.vertLine );

            this.steelMemberAngle = DwgGeometry.AngleFromXAxis( pt1, this.centerLineEndPoint );

            if( this.steelMemberAngle > 0 && this.steelMemberAngle < DwgGeometry.kRad90 )
            {
                this.DrawBracingInQ1();
                return 1;
            }
            if( this.steelMemberAngle > DwgGeometry.kRad90 && this.steelMemberAngle < DwgGeometry.kRad180 )
            {
                this.DrawBracingInQ2();
                return 2;
            }
            if( this.steelMemberAngle > DwgGeometry.kRad180 && this.steelMemberAngle < DwgGeometry.kRad270 )
            {
                this.DrawBracingInQ3();
                return 3;
            }
            this.DrawBracingInQ4();
            return 4;
        }

        private void DrawBracingInQ1()
        {
            Point3d kpt;
            int testCase = this.DetermineCaseInQ1( out kpt, this.steelMemberWidth, this.steelMemberHalfWidth );

            if( testCase == 0 )
            {
                #region "0"

                Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad90, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;

                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    this.edgeLineId1 = id;
                }
                else
                {
                    this.edgeLineId2 = id;
                }

                if( this.crossMemberType == BracingMemberType.SimpleAngle )
                {
                    this.steelCrossMemberIds1.Add( id );
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateVline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateHline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, 0, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad90, steel_plate_height );

                double ratio = steel_plate_width / steel_plate_height;
                if( ratio > 2.0 )
                {
                    xpt4 = new Point3d( mLine1.EndPoint.X - this.plateExtendOffset, xpt4.Y, 0 );
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );

                    var plateVline2 = new Line( new Point3d( xpt4.X, this.xpt1.Y, 0 ), xpt4 );
                    plateVline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline2 ) );
                }
                else
                {
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );
                }

                var plateVline = new Line( xpt2, plateVline1.EndPoint );
                plateVline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );

                #endregion
            }
            else if( testCase == 1 )
            {
                #region "1"

                Point3d pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad270, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    this.edgeLineId1 = id;
                }
                else
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle && ( this.bracingType == BracingLayoutType.VeeLayout || this.bracingType == BracingLayoutType.SingleMemberLayout ) )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    else if( this.crossMemberType == BracingMemberType.SimpleAngle && this.bracingType == BracingLayoutType.CrossLayout )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    this.edgeLineId2 = id;
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline1 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                double steel_plate_width = Math.Abs( this.xpt1.X - plateHline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateVline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, 0, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad90, steel_plate_height );

                var plateHline = new Line( xpt4, plateVline1.EndPoint );
                plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                plateHline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );

                double ratio = steel_plate_height / steel_plate_width;
                if( ratio > 2.0 )
                {
                    xpt2 = new Point3d( xpt2.X, mLine1.EndPoint.Y - this.plateExtendOffset, 0 );
                    var tmpDist = (int)Math.Abs( xpt4.Y - xpt2.Y );
                    int diff = tmpDist % 5;
                    tmpDist = tmpDist - diff;

                    if( diff > 1 )
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist + 5 );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }
                    else
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }

                    var plateVline = new Line( plateHline1.EndPoint,
                        DwgGeometry.GetPointPolar( plateHline1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );

                    var plateHline2 = new Line( new Point3d( this.xpt1.X, xpt2.Y, 0 ), xpt2 );

                    Point3d ptInt = this.GetLineIntersectionPoint( plateVline, plateHline2 );
                    plateVline.EndPoint = ptInt;
                    plateHline2.EndPoint = ptInt;

                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );

                    plateHline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline2 ) );
                }
                else
                {
                    var plateVline = new Line( xpt2, plateHline1.EndPoint );
                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );
                }

                #endregion
            }

            this.DrawHoles( kpt, testCase, 1, 0 );
        }

        private void DrawBracingInQ2()
        {
            Point3d kpt;
            int testCase = this.DetermineCaseInQ2( out kpt, this.steelMemberWidth, this.steelMemberHalfWidth );

            if( testCase == 0 )
            {
                #region "0"

                Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad270, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    this.edgeLineId1 = id;
                }
                else
                {
                    this.edgeLineId2 = id;
                }
                ;

                if( this.crossMemberType == BracingMemberType.SimpleAngle )
                {
                    if( this.bracingType == BracingLayoutType.SingleMemberLayout )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    else
                    {
                        this.steelCrossMemberIds3.Add( id );
                    }
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateVline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateHline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad180, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad90, steel_plate_height );

                double ratio = steel_plate_width / steel_plate_height;
                if( ratio > 2.0 )
                {
                    xpt4 = new Point3d( mLine1.EndPoint.X - this.plateExtendOffset, xpt4.Y, 0 );
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );

                    var plateVline2 = new Line( new Point3d( xpt4.X, this.xpt1.Y, 0 ), xpt4 );
                    plateVline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline2 ) );
                }
                else
                {
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );
                }

                var plateVline = new Line( xpt2, plateVline1.EndPoint );
                plateVline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateVline ) );

                #endregion
            }
            else if( testCase == 1 )
            {
                #region "1"

                Point3d pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad90, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    this.edgeLineId1 = id;
                }
                else
                {
                    this.edgeLineId2 = id;
                }
                ;

                if( this.crossMemberType == BracingMemberType.SimpleAngle )
                {
                    if( this.bracingType == BracingLayoutType.VeeLayout )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    else
                    {
                        this.steelCrossMemberIds3.Add( id );
                    }
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateVline1 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateHline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateHline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateVline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad180, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad90, steel_plate_height );

                var plateHline = new Line( xpt4, plateVline1.EndPoint );
                plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                plateHline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds2.Add( this.currDwg.AddEntity( plateHline ) );

                double ratio = steel_plate_height / steel_plate_width;
                if( ratio > 2.0 )
                {
                    xpt2 = new Point3d( xpt2.X, mLine1.EndPoint.Y - this.plateExtendOffset, 0 );
                    var tmpDist = (int)Math.Abs( xpt4.Y - xpt2.Y );
                    int diff = tmpDist % 5;
                    tmpDist = tmpDist - diff;

                    if( diff > 1 )
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist + 5 );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }
                    else
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }

                    var plateVline = new Line( plateHline1.EndPoint,
                        DwgGeometry.GetPointPolar( plateHline1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );

                    var plateHline2 = new Line( new Point3d( this.xpt1.X, xpt2.Y, 0 ), xpt2 );

                    Point3d ptInt = this.GetLineIntersectionPoint( plateVline, plateHline2 );
                    plateVline.EndPoint = ptInt;
                    plateHline2.EndPoint = ptInt;

                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );

                    plateHline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline2 ) );
                }
                else
                {
                    var plateVline = new Line( xpt2, plateHline1.EndPoint );
                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );
                }

                #endregion
            }

            this.DrawHoles( kpt, testCase, 2, 0 );
        }

        private void DrawBracingInQ3()
        {
            Point3d kpt;
            int testCase = this.DetermineCaseInQ3( out kpt, this.steelMemberWidth, this.steelMemberHalfWidth );

            if( testCase == 0 )
            {
                #region "0"

                Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad90, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }

                    this.edgeLineId1 = id;
                }
                else
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle && ( this.bracingType == BracingLayoutType.VeeLayout || this.bracingType == BracingLayoutType.SingleMemberLayout ) )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    else if( this.crossMemberType == BracingMemberType.SimpleAngle && this.bracingType == BracingLayoutType.CrossLayout )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    this.edgeLineId2 = id;
                }
                ;

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateVline1 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateHline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                double steel_plate_width = Math.Abs( this.xpt1.X - plateVline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateHline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad180, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad270, steel_plate_height );

                double ratio = steel_plate_width / steel_plate_height;
                if( ratio > 2.0 && this.bracingType != BracingLayoutType.VeeLayout )
                {
                    xpt4 = new Point3d( mLine1.EndPoint.X + this.plateExtendOffset, xpt4.Y, 0 );
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );

                    var plateVline2 = new Line( new Point3d( xpt4.X, this.xpt1.Y, 0 ), xpt4 );
                    plateVline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline2 ) );
                }
                else
                {
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );
                }

                var plateVline = new Line( xpt2, plateVline1.EndPoint );
                plateVline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateVline ) );

                #endregion
            }
            else if( testCase == 1 )
            {
                #region "1"

                Point3d pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad270, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    this.edgeLineId1 = id;
                }
                else
                {
                    if( this.crossMemberType == BracingMemberType.SimpleAngle && ( this.bracingType == BracingLayoutType.VeeLayout || this.bracingType == BracingLayoutType.SingleMemberLayout ) )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    else if( this.crossMemberType == BracingMemberType.SimpleAngle && this.bracingType == BracingLayoutType.CrossLayout )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    this.edgeLineId2 = id;
                }
                ;

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateHline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateVline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad180, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad270, steel_plate_height );

                var plateHline = new Line( xpt4, plateVline1.EndPoint );
                plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                plateHline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds3.Add( this.currDwg.AddEntity( plateHline ) );

                double ratio = steel_plate_height / steel_plate_width;
                if( ratio > 2.0 && this.bracingType != BracingLayoutType.VeeLayout )
                {
                    xpt2 = new Point3d( xpt2.X, mLine1.EndPoint.Y - this.plateExtendOffset, 0 );
                    var tmpDist = (int)Math.Abs( xpt4.Y - xpt2.Y );
                    int diff = tmpDist % 5;
                    tmpDist = tmpDist - diff;

                    if( diff > 1 )
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist + 5 );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }
                    else
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }

                    var plateVline = new Line( plateHline1.EndPoint,
                        DwgGeometry.GetPointPolar( plateHline1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );

                    var plateHline2 = new Line( new Point3d( this.xpt1.X, xpt2.Y, 0 ), xpt2 );

                    Point3d ptInt = this.GetLineIntersectionPoint( plateVline, plateHline2 );
                    plateVline.EndPoint = ptInt;
                    plateHline2.EndPoint = ptInt;

                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );

                    plateHline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline2 ) );

                    ////xpt2 = new Point3d(xpt2.X, mLine1.EndPoint.Y + _plateExtendOffset, 0);
                    ////Line plateVline = new Line(xpt2, plateHline1.EndPoint);
                    ////plateVline.Layer = _objectsLayer;
                    ////m_cornerGussetPlateIds1.Add(_currDwg.AddEntity(plateVline));

                    ////Line plateHline2 = new Line(xpt2, new Point3d(_xpt1.X, xpt2.Y, 0));
                    ////plateHline2.Layer = _objectsLayer;
                    ////m_cornerGussetPlateIds1.Add(_currDwg.AddEntity(plateHline2));
                }
                else
                {
                    var plateVline = new Line( xpt2, plateHline1.EndPoint );
                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );
                }

                #endregion
            }

            this.DrawHoles( kpt, testCase, 3, 0 );
        }

        private void DrawBracingInQ4()
        {
            Point3d kpt;
            int testCase = this.DetermineCaseInQ4( out kpt, this.steelMemberWidth, this.steelMemberHalfWidth );

            if( testCase == 0 )
            {
                #region "0"

                Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad270, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    this.edgeLineId1 = id;
                }
                else
                {
                    this.edgeLineId2 = id;
                }
                ;

                if( this.crossMemberType == BracingMemberType.SimpleAngle )
                {
                    if( this.bracingType == BracingLayoutType.SingleMemberLayout )
                    {
                        this.steelCrossMemberIds1.Add( id );
                    }
                    else if( this.bracingType == BracingLayoutType.VeeLayout )
                    {
                        this.steelCrossMemberIds2.Add( id );
                    }
                    else
                    {
                        this.steelCrossMemberIds3.Add( id );
                    }
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateVline1 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateHline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateVline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateHline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, 0, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad270, steel_plate_height );

                double ratio = steel_plate_width / steel_plate_height;
                if( ratio > 2.0 && this.bracingType != BracingLayoutType.VeeLayout )
                {
                    xpt4 = new Point3d( mLine1.EndPoint.X - this.plateExtendOffset, xpt4.Y, 0 );
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );

                    var plateVline2 = new Line( new Point3d( xpt4.X, this.xpt1.Y, 0 ), xpt4 );
                    plateVline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline2 ) );
                }
                else
                {
                    var plateHline = new Line( xpt4, plateHline1.EndPoint );
                    plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                    plateHline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline ) );
                }

                var plateVline = new Line( xpt2, plateVline1.EndPoint );
                plateVline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateVline ) );

                #endregion
            }
            else if( testCase == 1 )
            {
                #region "1"

                Point3d pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

                var mLine1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, this.steelMemberAngle + DwgGeometry.kRad90, this.steelMemberWidth ) );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );

                if( this.edgeLineId1 == ObjectId.Null )
                {
                    this.edgeLineId1 = id;
                }
                else
                {
                    this.edgeLineId2 = id;
                }
                ;

                if( this.crossMemberType == BracingMemberType.SimpleAngle )
                {
                    this.steelCrossMemberIds3.Add( id );
                }

                Line mLine2 = null;
                Line plateHline1 = null;
                Line plateVline1 = null;
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    Point3d ptm = DwgGeometry.GetPointPolar( DwgGeometry.GetMidPoint( mLine1.StartPoint, mLine1.EndPoint ), this.steelMemberAngle, this.steelMemberWeldLength );
                    mLine2 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.starPlateThickness * 0.5 ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle + DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) + this.steelAngleWidth + this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) ),
                        DwgGeometry.GetPointPolar( ptm, this.steelMemberAngle - DwgGeometry.kRad90, this.steelAngleWidth + ( this.starPlateThickness * 0.5 ) + this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateVline1 ) );
                }
                else
                {
                    mLine2 = new Line( DwgGeometry.GetPointPolar( mLine1.StartPoint, this.steelMemberAngle, this.steelMemberWeldLength ),
                        DwgGeometry.GetPointPolar( mLine1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );
                    mLine2.Layer = this._hiddenLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( mLine2 ) );

                    plateHline1 = new Line( mLine2.EndPoint, DwgGeometry.GetPointPolar( mLine2.EndPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.StartPoint, mLine2.EndPoint ), this.plateExtendOffset ) );
                    plateHline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateHline1 ) );

                    plateVline1 = new Line( mLine2.StartPoint, DwgGeometry.GetPointPolar( mLine2.StartPoint,
                        DwgGeometry.AngleFromXAxis( mLine2.EndPoint, mLine2.StartPoint ), this.plateExtendOffset ) );
                    plateVline1.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateVline1 ) );
                }

                double steel_plate_width = Math.Abs( this.xpt1.X - plateHline1.EndPoint.X );
                double steel_plate_height = Math.Abs( this.xpt1.Y - plateVline1.EndPoint.Y );

                Point3d xpt2 = DwgGeometry.GetPointPolar( this.xpt1, 0, steel_plate_width );
                Point3d xpt4 = DwgGeometry.GetPointPolar( this.xpt1, DwgGeometry.kRad270, steel_plate_height );

                var plateHline = new Line( xpt4, plateVline1.EndPoint );
                plateHline.StartPoint = this.GetLineIntersectionPoint( this.vertLine, plateHline );
                plateHline.Layer = this._objectsLayer;
                this.cornerGussetPlateIds4.Add( this.currDwg.AddEntity( plateHline ) );

                double ratio = steel_plate_height / steel_plate_width;
                if( ratio > 2.0 && this.bracingType != BracingLayoutType.VeeLayout )
                {
                    xpt2 = new Point3d( xpt2.X, mLine1.EndPoint.Y - this.plateExtendOffset, 0 );
                    var tmpDist = (int)Math.Abs( xpt4.Y - xpt2.Y );
                    int diff = tmpDist % 5;
                    tmpDist = tmpDist - diff;

                    if( diff > 1 )
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist + 5 );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }
                    else
                    {
                        Point3d tmpPt = DwgGeometry.GetPointPolar( xpt4, DwgGeometry.AngleFromXAxis( plateHline1.EndPoint, xpt2 ), tmpDist );
                        xpt2 = new Point3d( xpt2.X, tmpPt.Y, 0 );
                    }

                    var plateVline = new Line( plateHline1.EndPoint,
                        DwgGeometry.GetPointPolar( plateHline1.EndPoint, this.steelMemberAngle, this.steelMemberWeldLength ) );

                    var plateHline2 = new Line( new Point3d( this.xpt1.X, xpt2.Y, 0 ), xpt2 );

                    Point3d ptInt = this.GetLineIntersectionPoint( plateVline, plateHline2 );
                    plateVline.EndPoint = ptInt;
                    plateHline2.EndPoint = ptInt;

                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );

                    plateHline2.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateHline2 ) );

                    ////xpt2 = new Point3d(xpt2.X, mLine1.EndPoint.Y + _plateExtendOffset, 0);
                    ////Line plateVline = new Line(xpt2, plateHline1.EndPoint);
                    ////plateVline.Layer = _objectsLayer;
                    ////m_cornerGussetPlateIds1.Add(_currDwg.AddEntity(plateVline));

                    ////Line plateHline2 = new Line(xpt2, new Point3d(_xpt1.X, xpt2.Y, 0));
                    ////plateHline2.Layer = _objectsLayer;
                    ////m_cornerGussetPlateIds1.Add(_currDwg.AddEntity(plateHline2));
                }
                else
                {
                    var plateVline = new Line( xpt2, plateHline1.EndPoint );
                    plateVline.Layer = this._objectsLayer;
                    this.cornerGussetPlateIds1.Add( this.currDwg.AddEntity( plateVline ) );
                }

                #endregion
            }

            this.DrawHoles( kpt, testCase, 4, 0 );
        }

        private void DrawBracingType5( Line bottomLine1, Line bottomLine2, Line centerLine, Line edgeLine1, int quad1 )
        {
            Point3d kpt = Point3d.Origin;
            Point3d p1 = this.GetLineIntersectionPoint( centerLine, edgeLine1 );
            Point3d pt2 = this.GetLineIntersectionPoint( centerLine, bottomLine1 );
            Point3d pt3 = this.GetLineIntersectionPoint( centerLine, bottomLine2 );
            Point3d p2 = Point3d.Origin;
            Point3d p3 = Point3d.Origin;
            Line bLine1 = null;
            Line bLine2 = null;
            if( p1.DistanceTo( pt2 ) < p1.DistanceTo( pt3 ) )
            {
                bLine1 = bottomLine1;
                bLine2 = bottomLine2;
                p2 = pt2;
                p3 = pt3;
            }
            else
            {
                bLine1 = bottomLine2;
                bLine2 = bottomLine1;
                p2 = pt3;
                p3 = pt2;
            }

            double center_line_ang = DwgGeometry.AngleFromXAxis( p2, p1 );
            Point3d p4 = DwgGeometry.GetPointPolar( p2, center_line_ang, this.edgeOffset );

            Point3d kpt1;
            if( quad1 == 4 )
            {
                kpt = DwgGeometry.GetPointPolar( p4, center_line_ang + DwgGeometry.kRad90, this.steelMemberHalfWidth );
                var el1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, center_line_ang, this.steelMemberWidth ) );
                kpt = this.GetLineIntersectionPoint( bLine1, el1 );
                kpt = DwgGeometry.GetPointPolar( kpt, center_line_ang, this.edgeOffset );

                kpt1 = DwgGeometry.GetPointPolar( kpt, center_line_ang - DwgGeometry.kRad90, this.steelMemberWidth );

                var mLine1 = new Line( kpt, kpt1 );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );
                this.edgeLineId2 = id;

                this.DrawHoles( kpt1, 1, 2, center_line_ang );
            }
            else
            {
                kpt = DwgGeometry.GetPointPolar( p4, center_line_ang - DwgGeometry.kRad90, this.steelMemberHalfWidth );
                var el1 = new Line( kpt, DwgGeometry.GetPointPolar( kpt, center_line_ang, this.steelMemberWidth ) );
                kpt = this.GetLineIntersectionPoint( bLine1, el1 );
                kpt = DwgGeometry.GetPointPolar( kpt, center_line_ang, this.edgeOffset );

                kpt1 = DwgGeometry.GetPointPolar( kpt, center_line_ang + DwgGeometry.kRad90, this.steelMemberWidth );

                var mLine1 = new Line( kpt, kpt1 );
                mLine1.Layer = this._objectsLayer;
                ObjectId id = this.currDwg.AddEntity( mLine1 );
                this.edgeLineId2 = id;

                this.DrawHoles( kpt, 0, 1, center_line_ang );
            }

            //draw bottom bracing here
            double baseLineAng = DwgGeometry.AngleFromXAxis( bLine1.StartPoint, bLine1.EndPoint );
            if( baseLineAng > Math.PI )
            {
                baseLineAng = baseLineAng - Math.PI;
            }

            var edgeLine2 = this.currDwg.GetEntity( this.edgeLineId2 ) as Line;
            Point3d px1 = this.GetLineIntersectionPoint( centerLine, edgeLine1 );
            Point3d px2 = this.GetLineIntersectionPoint( centerLine, edgeLine2 );

            Point3d b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b20, b21;
            if( quad1 == 4 )
            {
                b1 = DwgGeometry.GetPointPolar( kpt, center_line_ang, this.steelMemberWeldLength );
                b2 = DwgGeometry.GetPointPolar( b1, center_line_ang + DwgGeometry.kRad90, this.edgeOffset );

                b12 = DwgGeometry.GetPointPolar( kpt1, center_line_ang, this.steelMemberWeldLength );
                b11 = DwgGeometry.GetPointPolar( b12, center_line_ang - DwgGeometry.kRad90, this.edgeOffset );

                var offLine = new Line( DwgGeometry.GetPointPolar( bLine1.StartPoint, baseLineAng + DwgGeometry.kRad90, this.edgeOffset ),
                    DwgGeometry.GetPointPolar( bLine1.EndPoint, baseLineAng + DwgGeometry.kRad90, this.edgeOffset ) );
                Point3d b99 = this.GetLineIntersectionPoint( centerLine, offLine );

                b3 = DwgGeometry.GetPointPolar( b99, baseLineAng + DwgGeometry.kRad180, this.steelMemberWeldLength * 0.5 );
                b10 = DwgGeometry.GetPointPolar( b99, baseLineAng, this.steelMemberWeldLength * 0.5 );
                var vertLine1 = new Line( b3, DwgGeometry.GetPointPolar( b3, baseLineAng + DwgGeometry.kRad90, this.steelMemberWeldLength * 0.5 ) );
                var vertLine2 = new Line( b10, DwgGeometry.GetPointPolar( b10, baseLineAng + DwgGeometry.kRad90, this.steelMemberWeldLength * 0.5 ) );

                b4 = this.GetLineIntersectionPoint( vertLine1, bLine1 );
                b9 = this.GetLineIntersectionPoint( vertLine2, bLine1 );

                b5 = this.GetLineIntersectionPoint( vertLine1, bLine2 );
                b8 = this.GetLineIntersectionPoint( vertLine2, bLine2 );

                b6 = DwgGeometry.GetPointPolar( b5, baseLineAng - DwgGeometry.kRad90, this.edgeOffset );
                b7 = DwgGeometry.GetPointPolar( b8, baseLineAng - DwgGeometry.kRad90, this.edgeOffset );
                b20 = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength );
            }
            else
            {
                b1 = DwgGeometry.GetPointPolar( kpt1, center_line_ang, this.steelMemberWeldLength );
                b2 = DwgGeometry.GetPointPolar( b1, center_line_ang + DwgGeometry.kRad90, this.edgeOffset );

                b12 = DwgGeometry.GetPointPolar( kpt, center_line_ang, this.steelMemberWeldLength );
                b11 = DwgGeometry.GetPointPolar( b12, center_line_ang - DwgGeometry.kRad90, this.edgeOffset );

                var offLine = new Line( DwgGeometry.GetPointPolar( bLine1.StartPoint, baseLineAng - DwgGeometry.kRad90, this.edgeOffset ),
                    DwgGeometry.GetPointPolar( bLine1.EndPoint, baseLineAng - DwgGeometry.kRad90, this.edgeOffset ) );
                Point3d b99 = this.GetLineIntersectionPoint( centerLine, offLine );

                b10 = DwgGeometry.GetPointPolar( b99, baseLineAng + DwgGeometry.kRad180, this.steelMemberWeldLength * 0.5 );
                b3 = DwgGeometry.GetPointPolar( b99, baseLineAng, this.steelMemberWeldLength * 0.5 );
                var vertLine1 = new Line( b3, DwgGeometry.GetPointPolar( b3, baseLineAng + DwgGeometry.kRad90, this.steelMemberWeldLength * 0.5 ) );
                var vertLine2 = new Line( b10, DwgGeometry.GetPointPolar( b10, baseLineAng + DwgGeometry.kRad90, this.steelMemberWeldLength * 0.5 ) );

                b4 = this.GetLineIntersectionPoint( vertLine1, bLine1 );
                b9 = this.GetLineIntersectionPoint( vertLine2, bLine1 );

                b5 = this.GetLineIntersectionPoint( vertLine1, bLine2 );
                b8 = this.GetLineIntersectionPoint( vertLine2, bLine2 );

                b6 = DwgGeometry.GetPointPolar( b5, baseLineAng + DwgGeometry.kRad90, this.edgeOffset );
                b7 = DwgGeometry.GetPointPolar( b8, baseLineAng + DwgGeometry.kRad90, this.edgeOffset );

                b20 = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( px2, DwgGeometry.AngleFromXAxis( px2, px1 ) - DwgGeometry.kRad90, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength );
            }

            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                if( quad1 == 3 )
                {
                    var bl1 = new Line( b12, b11 );
                    bl1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( bl1 );

                    var bl2 = new Line( b12, b20 );
                    bl2.Layer = this._hiddenLayer;
                    this.currDwg.AddEntity( bl2 );

                    var bl20 = new Line( b20, b2 );
                    bl20.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( bl20 );
                }
                else
                {
                    var bl1 = new Line( b20, b11 );
                    bl1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( bl1 );

                    var bl2 = new Line( b1, b20 );
                    bl2.Layer = this._hiddenLayer;
                    this.currDwg.AddEntity( bl2 );

                    var bl20 = new Line( b1, b2 );
                    bl20.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( bl20 );
                }
            }
            else
            {
                var bl1 = new Line( b1, b12 );
                bl1.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( bl1 );

                var bl2 = new Line( b1, b2 );
                bl2.Layer = this._objectsLayer;
                this.currDwg.AddEntity( bl2 );

                var bl20 = new Line( b12, b11 );
                bl20.Layer = this._objectsLayer;
                this.currDwg.AddEntity( bl20 );
            }

            var bl3 = new Line( b2, b3 );
            bl3.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl3 );

            var bl4 = new Line( b3, b4 );
            bl4.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl4 );

            var bl5 = new Line( b4, b5 );
            bl5.Layer = this._hiddenLayer;
            this.currDwg.AddEntity( bl5 );

            var bl6 = new Line( b5, b6 );
            bl6.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl6 );

            var bl7 = new Line( b6, b7 );
            bl7.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl7 );

            var bl8 = new Line( b7, b8 );
            bl8.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl8 );

            var bl9 = new Line( b8, b9 );
            bl9.Layer = this._hiddenLayer;
            this.currDwg.AddEntity( bl9 );

            var bl10 = new Line( b9, b10 );
            bl10.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl10 );

            var bl11 = new Line( b10, b11 );
            bl11.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl11 );

            var bl12 = new Line( b11, b12 );
            bl12.Layer = this._objectsLayer;
            this.currDwg.AddEntity( bl12 );

            //draw the topline
            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                double upAng = center_line_ang - DwgGeometry.kRad90;
                double dnAng = center_line_ang + DwgGeometry.kRad90;
                if( quad1 == 3 )
                {
                    upAng = center_line_ang + DwgGeometry.kRad90;
                    dnAng = center_line_ang - DwgGeometry.kRad90;
                }

                Point3d pq1 = DwgGeometry.GetPointPolar( px1, upAng, this.steelMemberWidth - this.steelMemberHalfWidth );
                Point3d pq2 = DwgGeometry.GetPointPolar( px2, upAng, this.steelMemberWidth - this.steelMemberHalfWidth );

                //top side
                var topL1 = new Line( DwgGeometry.GetPointPolar( pq1, DwgGeometry.AngleFromXAxis( pq1, pq2 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( pq2, DwgGeometry.AngleFromXAxis( pq2, pq1 ), this.steelMemberWeldLength ) );
                topL1.Layer = this._objectsLayer;
                this.currDwg.AddEntity( topL1 );

                var topHidL1 = new Line( pq1, DwgGeometry.GetPointPolar( pq1, DwgGeometry.AngleFromXAxis( pq1, pq2 ), this.steelMemberWeldLength ) );
                topHidL1.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( topHidL1 );

                var topHidL2 = new Line( pq2, DwgGeometry.GetPointPolar( pq2, DwgGeometry.AngleFromXAxis( pq2, pq1 ), this.steelMemberWeldLength ) );
                topHidL2.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( topHidL2 );

                Point3d pq3 = DwgGeometry.GetPointPolar( px1, upAng, this.starPlateThickness * 0.5 );
                Point3d pq4 = DwgGeometry.GetPointPolar( px2, upAng, this.starPlateThickness * 0.5 );

                var topL2 = new Line( DwgGeometry.GetPointPolar( pq3, DwgGeometry.AngleFromXAxis( pq3, pq4 ), this.steelMemberWeldLength ),
                    DwgGeometry.GetPointPolar( pq4, DwgGeometry.AngleFromXAxis( pq4, pq3 ), this.steelMemberWeldLength ) );
                topL2.Layer = this._objectsLayer;
                this.currDwg.AddEntity( topL2 );

                var topHidL22 = new Line( pq3, DwgGeometry.GetPointPolar( pq3, DwgGeometry.AngleFromXAxis( pq3, pq4 ), this.steelMemberWeldLength ) );
                topHidL22.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( topHidL22 );

                var topHidL222 = new Line( pq4, DwgGeometry.GetPointPolar( pq4, DwgGeometry.AngleFromXAxis( pq4, pq3 ), this.steelMemberWeldLength ) );
                topHidL222.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( topHidL222 );

                var topThk = new Line( DwgGeometry.GetPointPolar( px1, upAng, ( this.starPlateThickness * 0.5 ) + this.steelMemberFlangeThk ),
                    DwgGeometry.GetPointPolar( px2, upAng, ( this.starPlateThickness * 0.5 ) + this.steelMemberFlangeThk ) );
                topThk.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( topThk );

                //draw split edges here
                var el1 = new Line( DwgGeometry.GetPointPolar( px1, upAng, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.GetPointPolar( px1, upAng, this.steelMemberWidth - this.steelMemberHalfWidth ) );
                el1.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( el1 );

                var el2 = new Line( DwgGeometry.GetPointPolar( px2, upAng, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.GetPointPolar( px2, upAng, this.steelMemberWidth - this.steelMemberHalfWidth ) );
                el2.Layer = this._hiddenLayer;
                this.currDwg.AddEntity( el2 );

                var el3 = new Line( DwgGeometry.GetPointPolar( px1, dnAng, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.GetPointPolar( px1, dnAng, this.steelMemberWidth - this.steelMemberHalfWidth ) );
                el3.Layer = this._objectsLayer;
                this.currDwg.AddEntity( el3 );

                var el4 = new Line( DwgGeometry.GetPointPolar( px2, dnAng, ( this.starPlateThickness * 0.5 ) ),
                    DwgGeometry.GetPointPolar( px2, dnAng, this.steelMemberWidth - this.steelMemberHalfWidth ) );
                el4.Layer = this._objectsLayer;
                this.currDwg.AddEntity( el4 );

                //erase edges
                this.currDwg.Erase( this.edgeLineId1 );
                this.currDwg.Erase( this.edgeLineId2 );

                //bottom side
                var botL1 = new Line( DwgGeometry.GetPointPolar( px1, dnAng, this.steelMemberHalfWidth ),
                    DwgGeometry.GetPointPolar( px2, dnAng, this.steelMemberHalfWidth ) );
                botL1.Layer = this._objectsLayer;
                this.currDwg.AddEntity( botL1 );

                var botL2 = new Line( DwgGeometry.GetPointPolar( px1, dnAng, this.starPlateThickness * 0.5 ),
                    DwgGeometry.GetPointPolar( px2, dnAng, this.starPlateThickness * 0.5 ) );
                botL2.Layer = this._objectsLayer;
                this.currDwg.AddEntity( botL2 );

                var botThk = new Line( DwgGeometry.GetPointPolar( px1, dnAng, ( this.starPlateThickness * 0.5 ) + this.steelMemberFlangeThk ),
                    DwgGeometry.GetPointPolar( px2, dnAng, ( this.starPlateThickness * 0.5 ) + this.steelMemberFlangeThk ) );
                botThk.Layer = this._objectsLayer;
                this.currDwg.AddEntity( botThk );
            }
            else
            {
                //thk line
                if( quad1 == 4 )
                {
                    var topL1 = new Line( DwgGeometry.GetPointPolar( px1, center_line_ang - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth ),
                        DwgGeometry.GetPointPolar( px2, center_line_ang - DwgGeometry.kRad90, this.steelMemberWidth - this.steelMemberHalfWidth ) );
                    topL1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( topL1 );

                    var thkLine1 = new Line( DwgGeometry.GetPointPolar( topL1.StartPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) + DwgGeometry.kRad90, this.steelMemberFlangeThk ),
                        DwgGeometry.GetPointPolar( topL1.EndPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) + DwgGeometry.kRad90, this.steelMemberFlangeThk ) );
                    thkLine1.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                    this.currDwg.AddEntity( thkLine1 );

                    var botL1 = new Line( DwgGeometry.GetPointPolar( px1, center_line_ang + DwgGeometry.kRad90, this.steelMemberHalfWidth ),
                        DwgGeometry.GetPointPolar( px2, center_line_ang + DwgGeometry.kRad90, this.steelMemberHalfWidth ) );
                    botL1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( botL1 );

                    if( this.isChannelSelected || this.isPipeSelected )
                    {
                        var thkLine2 = new Line( DwgGeometry.GetPointPolar( botL1.StartPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) - DwgGeometry.kRad90, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( botL1.EndPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) - DwgGeometry.kRad90, this.steelMemberFlangeThk ) );
                        thkLine2.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( thkLine2 ) );
                    }
                }
                else
                {
                    var topL1 = new Line( edgeLine1.StartPoint, edgeLine2.EndPoint );
                    topL1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( topL1 );

                    var thkLine1 = new Line( DwgGeometry.GetPointPolar( topL1.StartPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) - DwgGeometry.kRad90, this.steelMemberFlangeThk ),
                        DwgGeometry.GetPointPolar( topL1.EndPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) - DwgGeometry.kRad90, this.steelMemberFlangeThk ) );
                    thkLine1.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                    this.currDwg.AddEntity( thkLine1 );

                    var botL1 = new Line( edgeLine1.EndPoint, edgeLine2.StartPoint );
                    botL1.Layer = this._objectsLayer;
                    this.currDwg.AddEntity( botL1 );

                    if( this.isChannelSelected || this.isPipeSelected )
                    {
                        var thkLine2 = new Line( DwgGeometry.GetPointPolar( botL1.StartPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) + DwgGeometry.kRad90, this.steelMemberFlangeThk ),
                            DwgGeometry.GetPointPolar( botL1.EndPoint, DwgGeometry.AngleFromXAxis( p2, p1 ) + DwgGeometry.kRad90, this.steelMemberFlangeThk ) );
                        thkLine2.Layer = this.crossMemberType == BracingMemberType.BoxChannel || this.crossMemberType == BracingMemberType.Pipe ? this._hiddenLayer : this._objectsLayer;
                        this.steelCrossMemberIds3.Add( this.currDwg.AddEntity( thkLine2 ) );
                    }
                }
            }

            //add text here
            var angLbl = new DBText();
            angLbl.SetDatabaseDefaults( CadApplication.CurrentDatabase );
            angLbl.TextString = this.GetCrossMemberDescription();
            angLbl.Height = this.currDwg.GetTextStyleHeight( this._textStyle );
            angLbl.SetTextStyleId( this.currDwg.GetTextStyleId( this._textStyle ) );
            angLbl.Layer = this._textLayer;
            angLbl.Position = DwgGeometry.GetPointPolar( px1, DwgGeometry.AngleFromXAxis( px1, px2 ), px1.DistanceTo( px2 ) * 0.5 );
            if( quad1 == 4 )
            {
                angLbl.Position = DwgGeometry.GetPointPolar( angLbl.Position, DwgGeometry.AngleFromXAxis( px2, px1 ) - DwgGeometry.kRad90,
                    this.steelMemberWidth - this.steelMemberHalfWidth + CadApplication.CurrentDatabase.Dimscale );
            }
            else
            {
                angLbl.Position = DwgGeometry.GetPointPolar( angLbl.Position, DwgGeometry.AngleFromXAxis( px2, px1 ) + DwgGeometry.kRad90,
                    this.steelMemberWidth - this.steelMemberHalfWidth + CadApplication.CurrentDatabase.Dimscale );
            }

            angLbl.Rotation = DwgGeometry.AngleFromXAxis( px2, px1 );
            if( angLbl.Rotation > DwgGeometry.kRad90 && angLbl.Rotation < DwgGeometry.kRad270 )
            {
                angLbl.Rotation -= DwgGeometry.kRad180;
            }
            this.currDwg.AddEntity( angLbl );

            //dimensions

            double upAng1 = DwgGeometry.AngleFromXAxis( px1, px2 ) - DwgGeometry.kRad90;
            if( quad1 == 4 )
            {
                upAng1 = DwgGeometry.AngleFromXAxis( px1, px2 ) + DwgGeometry.kRad90;
            }

            Point3d dpt1 = DwgGeometry.GetPointPolar( px1, upAng1, this.steelMemberWidth - this.steelMemberHalfWidth );
            Point3d dpt2 = DwgGeometry.GetPointPolar( px2, upAng1, this.steelMemberWidth - this.steelMemberHalfWidth );

            if( this.addLengthDimension )
            {
                //add dimesion here
                var dimLength = new AlignedDimension( dpt1, dpt2,
                    DwgGeometry.GetPointPolar( dpt1, upAng1, this.currDwg.Dimscale * 4 ), "", ObjectId.Null );
                dimLength.Layer = this._dimLayer;
                this.currDwg.AddEntity( dimLength );
            }

            var dimWeld1 = new AlignedDimension( dpt1, DwgGeometry.GetPointPolar( dpt1, DwgGeometry.AngleFromXAxis( px1, px2 ), this.steelMemberWeldLength ),
                DwgGeometry.GetPointPolar( dpt1, upAng1, this.currDwg.Dimscale * 2 ), "", ObjectId.Null );
            dimWeld1.Layer = this._dimLayer;
            this.currDwg.AddEntity( dimWeld1 );

            var dimWeld2 = new AlignedDimension( dpt2, DwgGeometry.GetPointPolar( dpt2, DwgGeometry.AngleFromXAxis( px2, px1 ), this.steelMemberWeldLength ),
                DwgGeometry.GetPointPolar( dpt1, upAng1, this.currDwg.Dimscale * 2 ), "", ObjectId.Null );
            dimWeld2.Layer = this._dimLayer;
            this.currDwg.AddEntity( dimWeld2 );
        }

        private int DetermineCaseInQ1( out Point3d kpt, double steel_member_width, double steel_member_half_width )
        {
            //part #1
            Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );
            var hOff1 = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad90, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad90, this.edgeOffset ), 0, this.edgeOffset ) );

            var cOff1 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_half_width ) );
            Point3d kpt1 = this.GetLineIntersectionPoint( hOff1, cOff1 );

            var mLine1 = new Line( kpt1, DwgGeometry.GetPointPolar( kpt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width ) );

            //part #2
            double vang = DwgGeometry.AngleFromXAxis( this.vertLine.StartPoint, this.vertLine.EndPoint );
            if( vang > DwgGeometry.kRad180 )
            {
                vang = vang - DwgGeometry.kRad180;
            }

            pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );
            var hOff2 = new Line( DwgGeometry.GetPointPolar( pt1, vang - DwgGeometry.kRad90, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, vang - DwgGeometry.kRad90, this.edgeOffset ), vang, this.edgeOffset ) );
            Point3d ept1 = this.GetLineIntersectionPoint( mLine1, hOff2 );
            double dist1 = ept1.DistanceTo( kpt1 );

            var cOff2 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width - steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width - steel_member_half_width ) );

            Point3d kpt2 = this.GetLineIntersectionPoint( hOff2, cOff2 );

            var mLine2 = new Line( kpt2, DwgGeometry.GetPointPolar( kpt2, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width ) );

            Point3d ept2 = this.GetLineIntersectionPoint( mLine2, hOff1 );
            double dist2 = ept2.DistanceTo( kpt2 );

            hOff1.Dispose();
            hOff2.Dispose();
            cOff1.Dispose();
            cOff2.Dispose();
            mLine1.Dispose();
            mLine2.Dispose();

            if( dist1 < dist2 )
            {
                kpt = kpt2;
                return 1;
            }
            kpt = kpt1;
            return 0;
        }

        private int DetermineCaseInQ2( out Point3d kpt, double steel_member_width, double steel_member_half_width )
        {
            //part #1
            Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );
            var hOff1 = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad90, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad90, this.edgeOffset ), DwgGeometry.kRad180, this.edgeOffset ) );

            var cOff1 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_half_width ) );

            Point3d kpt1 = this.GetLineIntersectionPoint( hOff1, cOff1 );

            var mLine1 = new Line( kpt1, DwgGeometry.GetPointPolar( kpt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width ) );

            //part #2
            double vang = DwgGeometry.AngleFromXAxis( this.vertLine.StartPoint, this.vertLine.EndPoint );
            if( vang > DwgGeometry.kRad180 )
            {
                vang = vang - DwgGeometry.kRad180;
            }
            pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );
            var hOff2 = new Line( DwgGeometry.GetPointPolar( pt1, vang + DwgGeometry.kRad90, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, vang + DwgGeometry.kRad90, this.edgeOffset ), vang, this.edgeOffset ) );

            Point3d ept1 = this.GetLineIntersectionPoint( mLine1, hOff2 );
            double dist1 = ept1.DistanceTo( kpt1 );

            var cOff2 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width - steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width - steel_member_half_width ) );

            Point3d kpt2 = this.GetLineIntersectionPoint( hOff2, cOff2 );

            var mLine2 = new Line( kpt2, DwgGeometry.GetPointPolar( kpt2, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width ) );

            Point3d ept2 = this.GetLineIntersectionPoint( mLine2, hOff1 );
            double dist2 = ept2.DistanceTo( kpt2 );

            hOff1.Dispose();
            hOff2.Dispose();
            cOff1.Dispose();
            cOff2.Dispose();
            mLine1.Dispose();
            mLine2.Dispose();

            if( dist1 < dist2 )
            {
                kpt = kpt2;
                return 1;
            }

            kpt = kpt1;
            return 0;
        }

        private int DetermineCaseInQ3( out Point3d kpt, double steel_member_width, double steel_member_half_width )
        {
            //part #1
            Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );
            var hOff1 = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad270, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad270, this.edgeOffset ), 0, this.edgeOffset ) );

            var cOff1 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width - steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width - steel_member_half_width ) );

            Point3d kpt1 = this.GetLineIntersectionPoint( hOff1, cOff1 );

            var mLine1 = new Line( kpt1, DwgGeometry.GetPointPolar( kpt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width ) );

            //part #2
            double vang = DwgGeometry.AngleFromXAxis( this.vertLine.StartPoint, this.vertLine.EndPoint );
            if( vang > DwgGeometry.kRad180 )
            {
                vang = vang - DwgGeometry.kRad180;
            }
            pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

            double tempOff = this.edgeOffset;
            if( this.bracingType == BracingLayoutType.VeeLayout )
            {
                tempOff = tempOff * 0.5;
            }

            var hOff2 = new Line( DwgGeometry.GetPointPolar( pt1, vang + DwgGeometry.kRad90, tempOff ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, vang + DwgGeometry.kRad90, tempOff ), vang, tempOff ) );

            Point3d ept1 = this.GetLineIntersectionPoint( mLine1, hOff2 );
            double dist1 = ept1.DistanceTo( kpt1 );

            var cOff2 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_half_width ) );

            Point3d kpt2 = this.GetLineIntersectionPoint( hOff2, cOff2 );

            var mLine2 = new Line( kpt2, DwgGeometry.GetPointPolar( kpt2, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width ) );

            Point3d ept2 = this.GetLineIntersectionPoint( mLine2, hOff1 );
            double dist2 = ept2.DistanceTo( kpt2 );

            hOff1.Dispose();
            hOff2.Dispose();
            cOff1.Dispose();
            cOff2.Dispose();
            mLine1.Dispose();
            mLine2.Dispose();

            if( dist1 < dist2 )
            {
                kpt = kpt2;
                return 1;
            }

            kpt = kpt1;
            return 0;
        }

        private int DetermineCaseInQ4( out Point3d kpt, double steel_member_width, double steel_member_half_width )
        {
            //part #1
            Point3d pt1 = this.GetLineIntersectionPoint( this.horzLine, this.centLine );
            var hOff1 = new Line( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad270, this.edgeOffset ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, DwgGeometry.kRad270, this.edgeOffset ), 0, this.edgeOffset ) );

            var cOff1 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width - steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width - steel_member_half_width ) );

            Point3d kpt1 = this.GetLineIntersectionPoint( hOff1, cOff1 );

            var mLine1 = new Line( kpt1, DwgGeometry.GetPointPolar( kpt1, this.steelMemberAngle + DwgGeometry.kRad90, steel_member_width ) );

            //part #2
            double vang = DwgGeometry.AngleFromXAxis( this.vertLine.StartPoint, this.vertLine.EndPoint );
            if( vang > DwgGeometry.kRad180 )
            {
                vang = vang - DwgGeometry.kRad180;
            }
            pt1 = this.GetLineIntersectionPoint( this.vertLine, this.centLine );

            double tempOff = this.edgeOffset;
            if( this.bracingType == BracingLayoutType.VeeLayout )
            {
                //this is suggested by Rohidas, 
                //the distance between two angles should be equal to edge offset, now same for each side
                tempOff = tempOff * 0.5;
            }

            var hOff2 = new Line( DwgGeometry.GetPointPolar( pt1, vang - DwgGeometry.kRad90, tempOff ),
                DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( pt1, vang - DwgGeometry.kRad90, tempOff ), vang, tempOff ) );

            Point3d ept1 = this.GetLineIntersectionPoint( mLine1, hOff2 );
            double dist1 = ept1.DistanceTo( kpt1 );

            var cOff2 = new Line( DwgGeometry.GetPointPolar( pt1, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_half_width ),
                DwgGeometry.GetPointPolar( this.centerLineEndPoint, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_half_width ) );

            Point3d kpt2 = this.GetLineIntersectionPoint( hOff2, cOff2 );

            var mLine2 = new Line( kpt2, DwgGeometry.GetPointPolar( kpt2, this.steelMemberAngle + DwgGeometry.kRad270, steel_member_width ) );

            Point3d ept2 = this.GetLineIntersectionPoint( mLine2, hOff1 );
            double dist2 = ept2.DistanceTo( kpt2 );

            hOff1.Dispose();
            hOff2.Dispose();
            cOff1.Dispose();
            cOff2.Dispose();
            mLine1.Dispose();
            mLine2.Dispose();

            if( dist1 < dist2 )
            {
                kpt = kpt2;
                return 1;
            }
            kpt = kpt1;
            return 0;
        }

        private double GetBracingDimension( string msg, Point3d basePt, double defaultValue )
        {
            Editor ed = CadApplication.CurrentEditor;
            var pdo = new PromptDistanceOptions( msg );
            pdo.BasePoint = basePt;
            pdo.UseBasePoint = true;
            pdo.DefaultValue = defaultValue;
            pdo.AllowZero = false;
            pdo.AllowNegative = false;
            PromptDoubleResult pdr = ed.GetDistance( pdo );
            if( pdr.Status == PromptStatus.Cancel )
            {
                return 0;
            }
            return pdr.Value;
        }

        private Point3d GetLineIntersectionPoint( Line l1, Line l2 )
        {
            var intPts = new Point3dCollection();
            l1.IntersectWith( l2, Intersect.ExtendBoth, intPts, new IntPtr( 1 ), new IntPtr( 1 ) );
            if( intPts != null && intPts.Count > 0 )
            {
                return intPts[0];
            }

            return Point3d.Origin;
        }

        private void DrawHoles( Point3d kpt, int intersectWithHorz, int quad, double srcAng )
        {
            double ang = this.steelMemberAngle;
            if( srcAng > 0 )
            {
                ang = srcAng;
            }
            if( intersectWithHorz == 1 && quad == 1 )
            {
                kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad270, this.steelMemberWidth );
            }

            if( intersectWithHorz == 1 && quad == 2 )
            {
                kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelMemberWidth );
            }

            if( intersectWithHorz == 0 && quad == 4 )
            {
                kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad270, this.steelMemberWidth );
            }

            if( intersectWithHorz == 0 && quad == 3 )
            {
                kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelMemberWidth );
            }

            Point3d cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                ang + DwgGeometry.kRad90, this.holeCenterY );

            if( quad == 2 )
            {
                cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                    ang + DwgGeometry.kRad270, this.holeCenterY );
            }

            if( quad == 3 )
            {
                cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                    ang + DwgGeometry.kRad270, this.holeCenterY );
            }

            var holeEntities = new ObjectIdCollection();
            for( int k = 0; k < this.noOfHoles; k++ )
            {
                if( ( quad == 4 || quad == 3 ) && this.oblongHoleCenterOffset > 0 )
                {
                    //can we draw poly line here
                    Point3d cp1 = DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad180, this.oblongHoleCenterOffset * 0.5 );
                    Point3d cp2 = DwgGeometry.GetPointPolar( cpt, ang, this.oblongHoleCenterOffset * 0.5 );

                    Point3d p1 = DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad90, this.holeDia * 0.5 );
                    Point3d p2 = DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad90, this.holeDia * 0.5 );
                    Point3d p3 = DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad270, this.holeDia * 0.5 );
                    Point3d p4 = DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad270, this.holeDia * 0.5 );

                    var plEnt = new Polyline();

                    plEnt.AddVertexAt( 0, new Point2d( p1.X, p1.Y ), 0, 0, 0 );
                    plEnt.AddVertexAt( 1, new Point2d( p2.X, p2.Y ), -1, 0, 0 );
                    plEnt.AddVertexAt( 2, new Point2d( p3.X, p3.Y ), 0, 0, 0 );
                    plEnt.AddVertexAt( 3, new Point2d( p4.X, p4.Y ), -1, 0, 0 );
                    plEnt.Closed = true;

                    plEnt.Layer = this._objectsLayer;
                    holeEntities.Add( this.currDwg.AddEntity( plEnt ) );

                    var xcLine = new Line( DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad180, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp2, ang, this.holeDia * 0.75 ) );
                    xcLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xcLine ) );

                    var xyLine = new Line( DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine ) );

                    var xyLine1 = new Line( DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine1.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine1 ) );
                }
                else
                {
                    var hole = new Circle( cpt, new Vector3d( 0, 0, 1 ), this.holeDia * 0.5 );
                    hole.Layer = this._objectsLayer;
                    holeEntities.Add( this.currDwg.AddEntity( hole ) );

                    var xcLine = new Line( DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad180, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cpt, ang, this.holeDia * 0.75 ) );
                    xcLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xcLine ) );

                    var xyLine = new Line( DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine ) );
                }

                cpt = DwgGeometry.GetPointPolar( cpt, ang, this.holePitch );
            }

            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                //mirror entities here
                foreach( ObjectId id in holeEntities )
                {
                    this.currDwg.Mirror( id, this.centLine.StartPoint, this.centLine.EndPoint, false );
                }
            }
        }

        private void DrawCenterPlateHoles( Point3d kpt, double ang, int quad )
        {
            Point3d cpt = Point3d.Origin;
            if( quad == 1 )
            {
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad270, this.steelMemberWidth * 0.5 );
                    cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                        ang + DwgGeometry.kRad90, this.holeCenterY );
                }
                else
                {
                    kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad270, this.steelMemberHalfWidth );
                    cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                        ang + DwgGeometry.kRad90, this.holeCenterY );
                }
            }
            if( quad == 3 )
            {
                if( this.crossMemberType == BracingMemberType.StarAngle )
                {
                    kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 );
                    cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                        ang + DwgGeometry.kRad270, this.holeCenterY );
                }
                else
                {
                    kpt = DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelMemberHalfWidth );
                    cpt = DwgGeometry.GetPointPolar( DwgGeometry.GetPointPolar( kpt, ang, this.holeCenterX ),
                        ang + DwgGeometry.kRad270, this.holeCenterY );
                }
            }

            var holeEntities = new ObjectIdCollection();
            for( int k = 0; k < this.noOfHoles; k++ )
            {
                if( quad == 3 && this.oblongHoleCenterOffset > 0 )
                {
                    //can we draw poly line here
                    Point3d cp1 = DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad180, this.oblongHoleCenterOffset * 0.5 );
                    Point3d cp2 = DwgGeometry.GetPointPolar( cpt, ang, this.oblongHoleCenterOffset * 0.5 );

                    Point3d p1 = DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad90, this.holeDia * 0.5 );
                    Point3d p2 = DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad90, this.holeDia * 0.5 );
                    Point3d p3 = DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad270, this.holeDia * 0.5 );
                    Point3d p4 = DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad270, this.holeDia * 0.5 );

                    var plEnt = new Polyline();

                    plEnt.AddVertexAt( 0, new Point2d( p1.X, p1.Y ), 0, 0, 0 );
                    plEnt.AddVertexAt( 1, new Point2d( p2.X, p2.Y ), -1, 0, 0 );
                    plEnt.AddVertexAt( 2, new Point2d( p3.X, p3.Y ), 0, 0, 0 );
                    plEnt.AddVertexAt( 3, new Point2d( p4.X, p4.Y ), -1, 0, 0 );
                    plEnt.Closed = true;

                    plEnt.Layer = this._objectsLayer;
                    holeEntities.Add( this.currDwg.AddEntity( plEnt ) );

                    var xcLine = new Line( DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad180, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp2, ang, this.holeDia * 0.75 ) );
                    xcLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xcLine ) );

                    var xyLine = new Line( DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp1, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine ) );

                    var xyLine1 = new Line( DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cp2, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine1.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine1 ) );
                }
                else
                {
                    var hole = new Circle( cpt, new Vector3d( 0, 0, 1 ), this.holeDia * 0.5 );
                    hole.Layer = this._objectsLayer;
                    holeEntities.Add( this.currDwg.AddEntity( hole ) );

                    var xcLine = new Line( DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad180, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cpt, ang, this.holeDia * 0.75 ) );
                    xcLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xcLine ) );

                    var xyLine = new Line( DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad90, this.holeDia * 0.75 ),
                        DwgGeometry.GetPointPolar( cpt, ang + DwgGeometry.kRad270, this.holeDia * 0.75 ) );
                    xyLine.LayerId = this.centLine.LayerId;
                    holeEntities.Add( this.currDwg.AddEntity( xyLine ) );
                }

                cpt = DwgGeometry.GetPointPolar( cpt, ang, this.holePitch );
            }

            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                //mirror entities here
                foreach( ObjectId id in holeEntities )
                {
                    this.currDwg.Mirror( id, this.centLine.StartPoint, this.centLine.EndPoint, false );
                }
            }
        }

        private Point3d GetSteelMemberStarEndPoint( Line thisCenterLine, Line otherCenterLine )
        {
            double thisAng = DwgGeometry.AngleFromXAxis( thisCenterLine.StartPoint, thisCenterLine.EndPoint );
            if( thisAng > DwgGeometry.kRad180 )
            {
                thisAng = thisAng - DwgGeometry.kRad180;
            }

            var thisLine = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ) );
            double otherAng = DwgGeometry.AngleFromXAxis( otherCenterLine.StartPoint, otherCenterLine.EndPoint );
            if( otherAng > DwgGeometry.kRad180 )
            {
                otherAng = otherAng - DwgGeometry.kRad180;
            }

            var otherLine = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, otherAng + DwgGeometry.kRad90, ( this.steelMemberWidth * 0.5 ) + this.edgeOffset ),
                DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, otherAng + DwgGeometry.kRad90, ( this.steelMemberWidth * 0.5 ) + this.edgeOffset ) );
            Point3d pt1 = this.GetLineIntersectionPoint( thisLine, otherLine );

            var tmpLine = new Line( pt1, DwgGeometry.GetPointPolar( pt1, thisAng + DwgGeometry.kRad90, this.steelMemberWidth ) );

            Point3d p1 = this.GetLineIntersectionPoint( tmpLine, thisCenterLine );

            var thisLine1 = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng + DwgGeometry.kRad270, this.steelMemberWidth * 0.5 ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng + DwgGeometry.kRad270, this.steelMemberWidth * 0.5 ) );

            Point3d pt2 = this.GetLineIntersectionPoint( thisLine1, otherLine );

            var tmpLine1 = new Line( pt2, DwgGeometry.GetPointPolar( pt2, thisAng + DwgGeometry.kRad270, this.steelMemberWidth ) );

            Point3d p2 = this.GetLineIntersectionPoint( tmpLine1, thisCenterLine );

            Point3d p3 = this.GetLineIntersectionPoint( thisCenterLine, otherCenterLine );

            if( p3.DistanceTo( p1 ) < p3.DistanceTo( p2 ) )
            {
                return p2;
            }
            return p1;
        }

        private Point3d GetSteelMemberStarStartPoint( Line thisCenterLine, Line otherCenterLine )
        {
            double thisAng = DwgGeometry.AngleFromXAxis( thisCenterLine.StartPoint, thisCenterLine.EndPoint );
            if( thisAng > DwgGeometry.kRad180 )
            {
                thisAng = thisAng - DwgGeometry.kRad180;
            }

            var thisLine = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng - DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng - DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ) );

            double otherAng = DwgGeometry.AngleFromXAxis( otherCenterLine.StartPoint, otherCenterLine.EndPoint );
            if( otherAng > DwgGeometry.kRad180 )
            {
                otherAng = otherAng - DwgGeometry.kRad180;
            }

            var otherLine = new Line( DwgGeometry.GetPointPolar( otherCenterLine.StartPoint, otherAng - DwgGeometry.kRad90, ( this.steelMemberWidth * 0.5 ) + this.edgeOffset ),
                DwgGeometry.GetPointPolar( otherCenterLine.EndPoint, otherAng - DwgGeometry.kRad90, ( this.steelMemberWidth * 0.5 ) + this.edgeOffset ) );

            Point3d pt1 = this.GetLineIntersectionPoint( thisLine, otherLine );

            var tmpLine = new Line( pt1, DwgGeometry.GetPointPolar( pt1, thisAng - DwgGeometry.kRad90, this.steelMemberWidth ) );

            Point3d p1 = this.GetLineIntersectionPoint( tmpLine, thisCenterLine );

            var thisLine1 = new Line( DwgGeometry.GetPointPolar( thisCenterLine.StartPoint, thisAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ),
                DwgGeometry.GetPointPolar( thisCenterLine.EndPoint, thisAng + DwgGeometry.kRad90, this.steelMemberWidth * 0.5 ) );

            Point3d pt2 = this.GetLineIntersectionPoint( thisLine1, otherLine );

            var tmpLine1 = new Line( pt2, DwgGeometry.GetPointPolar( pt2, thisAng + DwgGeometry.kRad270, this.steelMemberWidth ) );

            Point3d p2 = this.GetLineIntersectionPoint( tmpLine1, thisCenterLine );

            Point3d p3 = this.GetLineIntersectionPoint( thisCenterLine, otherCenterLine );

            if( p3.DistanceTo( p1 ) < p3.DistanceTo( p2 ) )
            {
                return p2;
            }
            return p1;
        }

        private void AddLengthDimesion( Line bottomLine, bool addLengthDim )
        {
            //add length dimension here
            if( this.addLengthDimension && addLengthDim )
            {
                double lineAng = DwgGeometry.AngleFromXAxis( bottomLine.StartPoint, bottomLine.EndPoint );
                Point3d ipt = DwgGeometry.Inters( bottomLine.EndPoint, DwgGeometry.GetPointPolar( bottomLine.EndPoint, lineAng + DwgGeometry.kRad90, this.steelAngleWidth ),
                    this.centLine.StartPoint, this.centLine.EndPoint );
                double dimAng = DwgGeometry.AngleFromXAxis( ipt, bottomLine.EndPoint );

                //add dimesion here
                var dimWeldLength = new AlignedDimension( bottomLine.StartPoint, bottomLine.EndPoint,
                    DwgGeometry.GetPointPolar( bottomLine.EndPoint, dimAng, this.steelMemberWeldLength ), "", ObjectId.Null );
                dimWeldLength.Layer = this._dimLayer;
                this.currDwg.AddEntity( dimWeldLength );
            }
        }

        private void DrawStarPlates( Point3d pt1, Point3d pt2 )
        {
            if( pt1.DistanceTo( pt2 ) - ( this.steelMemberWeldLength * 2 ) < ( this.starPlateOffset * 2 ) )
            {
                return;
            }
            Point3d mpt = DwgGeometry.GetMidPoint( pt1, pt2 );

            double ang = DwgGeometry.AngleFromXAxis( pt1, pt2 );
            double totalDistance = ( pt1.DistanceTo( pt2 ) - this.steelMemberWeldLength - this.steelMemberWeldLength ) * 0.5;
            double distanceCounter = 0;
            int k = 0;
            bool topView = true;
            while( totalDistance > ( distanceCounter + this.starPlateOffset ) )
            {
                Point3d kpt = DwgGeometry.GetPointPolar( mpt, ang, this.starPlateOffset * k );
                var cLine = new Line( DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelAngleWidth + this.starPlateThickness * 2 ),
                    DwgGeometry.GetPointPolar( kpt, ang - DwgGeometry.kRad90, this.steelAngleWidth + this.starPlateThickness * 2 ) );
                cLine.LayerId = this.centLine.LayerId;
                this.currDwg.AddEntity( cLine );

                this.DrawStarPlate( kpt, ang, topView );
                topView = !topView;
                distanceCounter += this.starPlateOffset;
                k++;
            }

            distanceCounter = this.starPlateOffset;
            k = 1;
            topView = false;
            while( totalDistance > ( distanceCounter + this.starPlateOffset ) )
            {
                Point3d kpt = DwgGeometry.GetPointPolar( mpt, ang + DwgGeometry.kRad180, this.starPlateOffset * k );
                var cLine = new Line( DwgGeometry.GetPointPolar( kpt, ang + DwgGeometry.kRad90, this.steelAngleWidth + this.starPlateThickness * 2 ),
                    DwgGeometry.GetPointPolar( kpt, ang - DwgGeometry.kRad90, this.steelAngleWidth + this.starPlateThickness * 2 ) );
                cLine.LayerId = this.centLine.LayerId;
                this.currDwg.AddEntity( cLine );

                this.DrawStarPlate( kpt, ang, topView );
                topView = !topView;
                distanceCounter += this.starPlateOffset;
                k++;
            }
        }

        private string GetCrossMemberDescription()
        {
            string prefix = "L";
            if( this.crossMemberType == BracingMemberType.StarAngle )
            {
                prefix = "Star";
            }
            else if( this.crossMemberType == BracingMemberType.BackToBackAngle || this.crossMemberType == BracingMemberType.BackToBackChannel )
            {
                prefix = "B/B";
            }
            else if( this.crossMemberType == BracingMemberType.BoxChannel )
            {
                prefix = "BX";
            }
            else if( this.crossMemberType == BracingMemberType.Pipe )
            {
                prefix = "";
            }

            if( this.isChannelSelected )
            {
                prefix = prefix + this.steelChannel.Description;
            }
            else if( this.isAngleSelected )
            {
                prefix = prefix + " L" + this.steelAngle.Description;
            }
            else if( this.isPipeSelected )
            {
                prefix = prefix + this.steelPipe.Description;
            }

            return prefix;
        }

#if DEBUG
        private void DrawText( Point3d pt, string txt )
        {
            var angDescText = new DBText();
            angDescText.TextString = txt;
            angDescText.Height = this.currDwg.GetTextStyleHeight( this._textStyle ) * 0.25;
            angDescText.TextStyleId = this.currDwg.GetTextStyleId( this._textStyle );
            angDescText.Position = pt;
            this.currDwg.AddEntity( angDescText );
        }
#endif

        private void DrawStarPlate( Point3d p, double ang, bool topView )
        {
            if( topView )
            {
                Point3d p1 = DwgGeometry.GetPointPolar(
                    DwgGeometry.GetPointPolar( p, ang, this.starPlateWidth * 0.5 ), ang + DwgGeometry.kRad90, this.starPlateThickness * 0.5 );
                Point3d p2 = DwgGeometry.GetPointPolar( p1, ang + DwgGeometry.kRad180, this.starPlateWidth );
                Point3d p3 = DwgGeometry.GetPointPolar( p2, ang - DwgGeometry.kRad90, this.starPlateThickness );
                Point3d p4 = DwgGeometry.GetPointPolar( p3, ang, this.starPlateWidth );

                var plEnt = new Polyline();

                plEnt.AddVertexAt( 0, new Point2d( p1.X, p1.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 1, new Point2d( p2.X, p2.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 2, new Point2d( p3.X, p3.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 3, new Point2d( p4.X, p4.Y ), 0, 0, 0 );
                plEnt.Closed = true;

                plEnt.Layer = this._objectsLayer;
                this.currDwg.AddEntity( plEnt );
            }
            else
            {
                double plateLen = this.steelMemberWidth + this.starPlateThickness + this.starPlateThickness;
                Point3d p1 = DwgGeometry.GetPointPolar(
                    DwgGeometry.GetPointPolar( p, ang, this.starPlateWidth * 0.5 ), ang + DwgGeometry.kRad90, plateLen * 0.5 );
                Point3d p2 = DwgGeometry.GetPointPolar( p1, ang + DwgGeometry.kRad180, this.starPlateWidth );
                Point3d p3 = DwgGeometry.GetPointPolar( p2, ang - DwgGeometry.kRad90, plateLen );
                Point3d p4 = DwgGeometry.GetPointPolar( p3, ang, this.starPlateWidth );

                var plEnt = new Polyline();

                plEnt.AddVertexAt( 0, new Point2d( p1.X, p1.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 1, new Point2d( p2.X, p2.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 2, new Point2d( p3.X, p3.Y ), 0, 0, 0 );
                plEnt.AddVertexAt( 3, new Point2d( p4.X, p4.Y ), 0, 0, 0 );
                plEnt.Closed = true;

                plEnt.Layer = this._objectsLayer;
                this.currDwg.AddEntity( plEnt );
            }
        }

        #endregion

    }

    public enum BracingMemberType
    {
        SimpleAngle,

        StarAngle,

        BackToBackAngle,

        BoxChannel,

        BackToBackChannel,

        Pipe
    }

    public enum BracingLayoutType
    {
        None,
        CrossLayout,
        VeeLayout,
        SingleMemberLayout,
        HalfMemberLayout
    }
}