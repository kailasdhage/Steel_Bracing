using Autodesk.AutoCAD.Runtime;

namespace Steel_Bracing_2d
{
    using System;

    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;

    public class MeCommands
    {
        #region Static Fields
        public static FrmBracing BracingForm = new FrmBracing();
        #endregion

        #region Public Methods and Operators

        [CommandMethod( "brc" )]
        [CommandMethod( "bracing" )]
        public static void CommandBracing()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            if( BracingForm == null )
            {
                BracingForm = new FrmBracing();
            }

            if( BracingForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel )
            {
                return;
            }

            IBracingInput userInput = BracingForm.GetUserInput();

            #region edge selection
            var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );

            //select vertical edge
            ObjectId vertEdgeId;
            if( CommandLine.SelectEntity( "\nSelect first vertical edge:", typeof( Line ), out vertEdgeId ) == false )
            {
                CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                return;
            }

            Line vertLine1 = currDwg.GetEntity( vertEdgeId ) as Line;
            if( vertLine1 != null && Math.Abs( vertLine1.StartPoint.Y - vertLine1.EndPoint.Y ) < 1.0 )
            {
                CadApplication.CurrentEditor.WriteMessage( "\nNot a vertical line" );
                return;
            }

            //select horizontal edge
            ObjectId horzEdgeId;
            if( CommandLine.SelectEntity( "\nSelect first horizontal edge:", typeof( Line ), out horzEdgeId ) == false )
            {
                CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                return;
            }

            Line horzLine1 = currDwg.GetEntity( horzEdgeId ) as Line;
            if( horzLine1 != null && Math.Abs( horzLine1.StartPoint.Y - horzLine1.EndPoint.Y ) > 0.01 )
            {
                CadApplication.CurrentEditor.WriteMessage( "\nNot a horzontal line" );
                return;
            }

            ObjectId vertEdgeId1;
            Line vertLine2 ;
            ObjectId horzEdgeId1;
            Line horzLine2;
            if( userInput.BracingType != BracingLayoutType.HalfMemberLayout )
            {
                //select vertical edge
                if( CommandLine.SelectEntity( "\nSelect second vertical edge:", typeof( Line ), out vertEdgeId1 ) == false || vertEdgeId1 == vertEdgeId )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNo selection or Invalid line" );
                    return;
                }

                vertLine2 = currDwg.GetEntity( vertEdgeId1 ) as Line;
                if( vertLine2 != null && Math.Abs( vertLine2.StartPoint.Y - vertLine2.EndPoint.Y ) < 1.0 )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNot a vertical line" );
                    return;
                }

                if( vertEdgeId1 == vertEdgeId )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nBoth selected vertical lines are same" );
                    return;
                }

                //select horizontal edge
                if( CommandLine.SelectEntity( "\nSelect second horizontal edge:", typeof( Line ), out horzEdgeId1 ) == false || horzEdgeId1 == horzEdgeId )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                    return;
                }

                horzLine2 = currDwg.GetEntity( horzEdgeId1 ) as Line;
                if( horzLine2 != null && Math.Abs( horzLine2.StartPoint.Y - horzLine2.EndPoint.Y ) > 0.01 )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNot a horzontal line" );
                    return;
                }

                if( horzEdgeId1 == horzEdgeId )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nBoth selected horizontal lines are same" );
                    return;
                }
            }
            else
            {
                //select bottom edge1
                if( CommandLine.SelectEntity( "\nSelect first bottom edge:", typeof( Line ), out horzEdgeId1 ) == false )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                    return;
                }
                horzLine2 = currDwg.GetEntity( horzEdgeId1 ) as Line;

                if( CommandLine.SelectEntity( "\nSelect second bottom edge:", typeof( Line ), out vertEdgeId1 ) == false )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNo selection or Invalid line" );
                    return;
                }

                vertLine2 = currDwg.GetEntity( vertEdgeId1 ) as Line;
            }

            //select center line
            ObjectId cenEdgeId;
            if( CommandLine.SelectEntity( "\nSelect first center line:", typeof( Line ), out cenEdgeId ) == false )
            {
                CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                return;
            }
            Line centLine1 = currDwg.GetEntity( cenEdgeId ) as Line;

            Line centLine2 = null;
            if( userInput.BracingType == BracingLayoutType.CrossLayout || userInput.BracingType == BracingLayoutType.VeeLayout )
            {
                ObjectId cenEdgeId1;
                if( CommandLine.SelectEntity( "\nSelect second center line:", typeof( Line ), out cenEdgeId1 ) == false )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nNo selection" );
                    return;
                }

                if( cenEdgeId1 == cenEdgeId )
                {
                    CadApplication.CurrentEditor.WriteMessage( "\nBoth selected center lines are same" );
                    return;
                }

                centLine2 = currDwg.GetEntity( cenEdgeId1 ) as Line;
            }
            
            #endregion

            userInput.HorzLine1 = horzLine1;
            userInput.HorzLine2 = horzLine2;
            userInput.VertLine1 = vertLine1;
            userInput.VertLine2 = vertLine2;
            userInput.CentLine1 = centLine1;
            userInput.CentLine2 = centLine2;

            var cmdBracing = new BracingCommand
                             {
                                 BracingUserInput = userInput
                             };
            cmdBracing.ExecuteCommand();
        }

        #if DEBUG
        [CommandMethod( "but1" )]
        public static void CommandBracingUnitTest1()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.CrossLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.SimpleAngle,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ai in input1.SteelAngleList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                input1.HorzLine1 = new Line( pt1, pt2 );
                input1.HorzLine2 = new Line( pt4, pt3 );
                input1.VertLine1 = new Line( pt1, pt4 );
                input1.VertLine2 = new Line( pt2, pt3 );
                input1.CentLine1 = new Line( pt1, pt3 );
                input1.CentLine2 = new Line( pt4, pt2 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );
                currDwg.AddEntity( input1.CentLine2 );


                input1.SteelAngle = ai;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }

        [CommandMethod( "but2" )]
        public static void CommandBracingUnitTest2()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.VeeLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.BackToBackChannel,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ci in input1.SteelChannelList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                var pt5 = DwgGeometry.GetMidPoint( pt4, pt3 );

                input1.HorzLine1 = new Line( pt1, pt2 );
                input1.HorzLine2 = new Line( pt4, pt3 );
                input1.VertLine1 = new Line( pt1, pt4 );
                input1.VertLine2 = new Line( pt2, pt3 );
                input1.CentLine1 = new Line( pt1, pt5 );
                input1.CentLine2 = new Line( pt5, pt2 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );
                currDwg.AddEntity( input1.CentLine2 );


                input1.SteelChannel = ci;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }

        [CommandMethod( "but3" )]
        public static void CommandBracingUnitTest3()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.SingleMemberLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.BoxChannel,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ci in input1.SteelChannelList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                input1.HorzLine1 = new Line( pt1, pt2 );
                input1.HorzLine2 = new Line( pt4, pt3 );
                input1.VertLine1 = new Line( pt1, pt4 );
                input1.VertLine2 = new Line( pt2, pt3 );
                input1.CentLine1 = new Line( pt1, pt3 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );

                input1.SteelChannel = ci;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }

        [CommandMethod( "but4" )]
        public static void CommandBracingUnitTest4()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.SingleMemberLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.Pipe,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ci in input1.SteelPipeList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                input1.HorzLine1 = new Line( pt1, pt2 );
                input1.HorzLine2 = new Line( pt4, pt3 );
                input1.VertLine1 = new Line( pt1, pt4 );
                input1.VertLine2 = new Line( pt2, pt3 );
                input1.CentLine1 = new Line( pt2, pt4 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );

                input1.SteelPipe = ci;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }

        [CommandMethod( "but5" )]
        public static void CommandBracingUnitTest5()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.HalfMemberLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.BoxChannel,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ci in input1.SteelChannelList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                double ang = DwgGeometry.AngleFromXAxis( pt1, pt3 ) + DwgGeometry.kRad90;
                var p1 = DwgGeometry.GetPointPolar( pt1, ang, ci.Breadth * 0.5 );
                var p2 = DwgGeometry.GetPointPolar( pt3, ang, ci.Breadth * 0.5 );
                var p3 = DwgGeometry.GetPointPolar( pt3, ang + Math.PI, ci.Breadth * 0.5 );
                var p4 = DwgGeometry.GetPointPolar( pt1, ang + Math.PI, ci.Breadth * 0.5 );

                input1.HorzLine1 = new Line( pt3, pt4 );
                input1.VertLine1 = new Line( pt1, pt4 );
                input1.CentLine1 = new Line( pt2, pt4 );

                //top edge of cross member
                input1.HorzLine2 = new Line( p1, p2 );

                //bottom edge of cross member
                input1.VertLine2 = new Line( p4, p3 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );

                input1.SteelChannel = ci;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }

        [CommandMethod( "but6" )]
        public static void CommandBracingUnitTest6()
        {
            LayerManager.CreateLayers();
            LayerManager.CreateTextStyles( CadApplication.CurrentDocument.Database.Dimscale );

            var input1 = new BracingInput
                         {
                             AddLengthDimension = false,
                             BracingType = BracingLayoutType.HalfMemberLayout,
                             CenterPlateOffset = 70,
                             CrossMemberType = BracingMemberType.SimpleAngle,
                             EdgeOffset = 20,
                             HoleCenterX = 50,
                             HoleDia = 20,
                             HolePitch = 50,
                             NoOfHoles = 2,
                             OblongHoleCenterOffset = 12,
                             PlateExtendOffset = 20,
                             StarPlateOffset = 500,
                             StarPlateThickness = 8,
                             StarPlateWidth = 75,
                             SteelAngle = null,
                             SteelChannel = null,
                             SteelPipe = null,
                             SteelMemberOverlap = 250
                         };

            input1.LoadSteelSections();

            var vOff = 0;

            foreach( var ci in input1.SteelAngleList )
            {
                var pt1 = new Point3d( 0, vOff, 0 );
                var pt3 = new Point3d( 1000, 1200+vOff, 0 );
                var pt2 = new Point3d( pt3.X, pt1.Y, 0 );
                var pt4 = new Point3d( pt1.X, pt3.Y, 0 );

                double ang = DwgGeometry.AngleFromXAxis( pt4, pt2 ) + DwgGeometry.kRad90;
                var p1 = DwgGeometry.GetPointPolar( pt4, ang, ci.Width * 0.5 );
                var p2 = DwgGeometry.GetPointPolar( pt2, ang, ci.Width * 0.5 );
                var p3 = DwgGeometry.GetPointPolar( pt4, ang + Math.PI, ci.Width * 0.5 );
                var p4 = DwgGeometry.GetPointPolar( pt2, ang + Math.PI, ci.Width * 0.5 );

                input1.HorzLine1 = new Line( pt3, pt4 );
                input1.VertLine1 = new Line( pt2, pt3 );
                input1.CentLine1 = new Line( pt1, pt3 );

                //top edge of cross member
                input1.HorzLine2 = new Line( p1, p2 );

                //bottom edge of cross member
                input1.VertLine2 = new Line( p4, p3 );

                var currDwg = new DrawingDatabase( CadApplication.CurrentDocument );
                currDwg.AddEntity( input1.HorzLine1 );
                currDwg.AddEntity( input1.HorzLine2 );
                currDwg.AddEntity( input1.VertLine1 );
                currDwg.AddEntity( input1.VertLine2 );
                currDwg.AddEntity( input1.CentLine1 );

                input1.SteelAngle = ci;

                var cmdBracing1 = new BracingCommand
                                  {
                                      BracingUserInput = input1
                                  };
                cmdBracing1.ExecuteCommand();

                vOff += 1500;
            }

        }
        #endif
        #endregion
    }
}
