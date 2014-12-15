namespace Steel_Bracing_2d.AcFramework
{
    using System;
    using System.Collections.Generic;

    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public class DwgGeometry
    {
        // predefined constants for common angles

        #region Constants

        public const double kHalfPi = Math.PI / 2.0;

        public const double kPi = Math.PI;

        public const double kRad0 = 0.0;

        public const double kRad135 = (Math.PI * 3.0) / 4.0;
        public const double kRad180 = Math.PI;
        public const double kRad270 = Math.PI * 1.5;
        public const double kRad360 = Math.PI * 2.0;

        public const double kRad45 = Math.PI / 4.0;
        public const double kRad90 = Math.PI / 2.0;

        public const double kTwoPi = Math.PI * 2.0;

        #endregion

        // predefined values for common Points and Vectors

        #region Static Fields

        public static readonly Point3d kOrigin = new Point3d(0.0, 0.0, 0.0);
        public static readonly Vector3d kXAxis = new Vector3d(1.0, 0.0, 0.0);
        public static readonly Vector3d kYAxis = new Vector3d(0.0, 1.0, 0.0);
        public static readonly Vector3d kZAxis = new Vector3d(0.0, 0.0, 1.0);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Return angle in radians
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static double AngleFromXAxis(Point3d pt1, Point3d pt2)
        {
            //if you have many vectors on a plane and want to measure angles in the same sense:
            //e.g anticlockwise, use two vectors to set up the sense   
            Vector3d oXDir = new Vector3d(1, 0, 0);
            Vector3d oYDir = new Vector3d(0, 1, 0);
            Vector3d oZDir = new Vector3d(0, 0, 1);

            //set up direction as anti-clockwise
            Vector3d oDirSense = oXDir.CrossProduct(oYDir);

            //use above or reverse it for clockwise:
            //    oDirSense.X = -(oDirSense.X)
            //    oDirSense.Y = -(oDirSense.Y)
            //    oDirSense.Z = -(oDirSense.Z)

            //now use the sense to measure angles between any other vectors on the plane
            //define the first vector
            Vector3d oVector1 = new Vector3d(1, 0, 0);

            //define the second vector
            Vector3d oVector2 = pt2 - pt1;

            //get the crossproduct
            Vector3d oCrossProd = oVector1.CrossProduct(oVector2);

            //now determine the angle between Vector1 and Vector2 in the anti-clockwise direction
            Double angInRadians = 0;
            if (oCrossProd.DotProduct(oDirSense) < 0)
            {
                angInRadians = 2 * Math.PI - oVector1.GetAngleTo(oVector2);
            }
            else
            {
                angInRadians = oVector1.GetAngleTo(oVector2);
            }

            return angInRadians;
        }

        /// <summary>
        /// Returns angle from X-Axis
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double AngleWithXAxis(Point3d p1, Point3d p2)
        {
            return new Vector2d(p2.X - p1.X, p2.Y - p1.Y).Angle;
        }

        /// <summary>
        /// calculate bounding min / max points, in zero orientation
        /// </summary>
        /// <param name="pnlEntity"></param>
        /// <param name="rotAngle"></param>
        /// <param name="minPoint"></param>
        /// <param name="maxPoint"></param>
        public static void CalculateBoundingBox(Entity pnlEntity, double rotAngle, out Point3d minPoint, out Point3d maxPoint)
        {
            //matrix to rotate panel to align to x - axis
            Matrix3d matRotation = Matrix3d.Rotation(DwgGeometry.dtr(rotAngle) * -1.0, new Vector3d(0, 0, 1), Point3d.Origin);

            DBObjectCollection entitySet = new DBObjectCollection();
            pnlEntity.Explode(entitySet);

            Extents3d pnlExtents = new Extents3d(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
            bool isExtentsSet = false;

            foreach (Entity ent in entitySet)
            {
                ent.TransformBy(matRotation);

                if (isExtentsSet)
                {
                    pnlExtents.AddExtents(ent.GeometricExtents);
                }
                else
                {
                    pnlExtents.Set(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint);
                    isExtentsSet = true;
                }
            }

            //clear memory of exploded objects
            foreach (DBObject dbObj in entitySet)
            {
                dbObj.Dispose();
            }

            minPoint = pnlExtents.MinPoint;
            maxPoint = pnlExtents.MaxPoint;
        }

        /// <summary>
        /// Get line bounding box of panel like thickness line, reveal line
        /// </summary>
        /// <param name="lineEntity"></param>
        /// <param name="rotAngle"></param>
        /// <param name="minPoint"></param>
        /// <param name="maxPoint"></param>
        public static void CalculateBoundingBox(Line lineEntity, double rotAngle, out Point3d minPoint, out Point3d maxPoint)
        {
            //matrix to rotate panel to align to x - axis
            Matrix3d matRotation = Matrix3d.Rotation(DwgGeometry.dtr(rotAngle) * -1.0, new Vector3d(0, 0, 1), Point3d.Origin);

            lineEntity.TransformBy(matRotation);

            minPoint = lineEntity.GeometricExtents.MinPoint;
            maxPoint = lineEntity.GeometricExtents.MaxPoint;
        }

        /// <summary>
        /// calculate bounding min / max points, in rotate by angle
        /// used in place corner panels
        /// </summary>
        /// <param name="pnlEntity"></param>
        /// <param name="rotAngle"></param>
        /// <param name="minPoint"></param>
        /// <param name="maxPoint"></param>
        /// <param name="minPoint1"></param>
        /// <param name="maxPoint1"></param>
        public static void CalculateBoundingBox(Entity pnlEntity, double rotAngle, out Point3d minPoint, out Point3d maxPoint, out Point3d minPoint1, out Point3d maxPoint1)
        {
            //matrix to rotate panel to align to x - axis
            Matrix3d matRotation = Matrix3d.Rotation(DwgGeometry.dtr(rotAngle) * -1.0, new Vector3d(0, 0, 1), Point3d.Origin);

            DBObjectCollection entitySet = new DBObjectCollection();
            pnlEntity.Explode(entitySet);

            Extents3d pnlExtents = new Extents3d(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
            bool isExtentsSet = false;

            foreach (Entity ent in entitySet)
            {
                ent.TransformBy(matRotation);

                if (isExtentsSet)
                {
                    pnlExtents.AddExtents(ent.GeometricExtents);
                }
                else
                {
                    pnlExtents.Set(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint);
                    isExtentsSet = true;
                }
            }

            //clear memory of exploded objects
            foreach (DBObject dbObj in entitySet)
            {
                dbObj.Dispose();
            }

            Point3d pt2 = new Point3d(pnlExtents.MaxPoint.X, pnlExtents.MinPoint.Y, 0);
            Point3d pt4 = new Point3d(pnlExtents.MinPoint.X, pnlExtents.MaxPoint.Y, 0);

            minPoint1 = pt4.TransformBy(matRotation.Inverse());
            maxPoint1 = pt2.TransformBy(matRotation.Inverse());

            minPoint = pnlExtents.MinPoint.TransformBy(matRotation.Inverse());
            maxPoint = pnlExtents.MaxPoint.TransformBy(matRotation.Inverse());
        }

        public static Point3d GetCenOf3Pt(Point3d startPoint, Point3d pointOnArc, Point3d endPoint, out double radius)
        {
            CircularArc3d arcd = new CircularArc3d(startPoint, pointOnArc, endPoint);
            radius = arcd.Radius;
            return arcd.Center;
        }

        public static Point3d GetClosestPoint(Point3d p, Point3d p1, Point3d p2)
        {
            return p.DistanceTo(p1) < p.DistanceTo(p2) ? p1 : p2;
        }

        /// <summary>
        /// This is the AutoCAD Arbitrary Axis Algorithm.  Given a normal vector,
        /// establish the ECS matrix that corresponds.
        /// </summary>
        /// <param name="origin">Origin point</param>
        /// <param name="zAxis">Normal vector of the entity</param>
        /// <returns>ECS Matrix</returns>

        public static Matrix3d GetEcsToWcsMatrix(Point3d origin, Vector3d zAxis)
        {
            const double kArbBound = 0.015625;         //  1/64th

            // short circuit if in WCS already
            if (zAxis == DwgGeometry.kZAxis)
            {
                return Matrix3d.Identity;
            }

            Vector3d xAxis, yAxis;

            //Debug.Assert(zAxis.IsUnitLength(Tolerance.Global));

            if ((Math.Abs(zAxis.X) < kArbBound) && (Math.Abs(zAxis.Y) < kArbBound))
                xAxis = DwgGeometry.kYAxis.CrossProduct(zAxis);
            else
                xAxis = DwgGeometry.kZAxis.CrossProduct(zAxis);

            xAxis = xAxis.GetNormal();
            yAxis = zAxis.CrossProduct(xAxis).GetNormal();

            return Matrix3d.AlignCoordinateSystem(DwgGeometry.kOrigin, DwgGeometry.kXAxis, DwgGeometry.kYAxis, DwgGeometry.kZAxis,
                                        origin, xAxis, yAxis, zAxis);
        }

        public static Matrix3d GetEcsToWcsTransform(Entity entity)
        {
            return Matrix3d.PlaneToWorld(entity.GetPlane().Normal);
        }

        /// <summary>
        /// Get the X-Axis relative to an entities ECS (In other words, what it considers the
        /// X-Axis.  This is crucial for Entities like Dimensions and DBPoints.  The X-Axis is
        /// determined by the Arbitrary Axis algorithm.
        /// </summary>
        /// <param name="ecsZAxis">The normal vector of the entity</param>
        /// <returns>The X-Axis for this ECS</returns>

        public static Vector3d GetEcsXAxis(Vector3d ecsZAxis)
        {
            Matrix3d arbMat = GetEcsToWcsMatrix(DwgGeometry.kOrigin, ecsZAxis);

            return arbMat.CoordinateSystem3d.Xaxis;
        }

        /// <summary>
        /// Get cordinates of the region entity
        /// </summary>
        /// <param name="pnlEntity"></param>
        /// <param name="rotAngle"></param>
        /// <returns></returns>
        public static List<Point3d> GetEntityCoordinates(Entity pnlEntity)
        {
            List<Point3d> retResult = new List<Point3d>();

            DBObjectCollection entitySet = new DBObjectCollection();
            pnlEntity.Explode(entitySet);

            foreach (Entity ent in entitySet)
            {
                if (ent is Line)
                {
                    retResult.Add((ent as Line).StartPoint);
                }
            }

            //clear memory of exploded objects
            foreach (DBObject dbObj in entitySet)
            {
                dbObj.Dispose();
            }

            return retResult;
        }

        public static Point3d GetMidPoint(Point3d pt1, Point3d pt2)
        {
            return new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, 0);
        }

        public static Point3d GetMidPoint(Curve curve)
        {
            double d1 = curve.GetDistanceAtParameter(curve.StartParam);
            double d2 = curve.GetDistanceAtParameter(curve.EndParam);
            return curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));
        }

        public static Point3d GetPerpendicularPoint(Point3d sp, Point3d ep, Point3d p)
        {
            LineSegment2d seg = new LineSegment2d(new Point2d(sp.X, sp.Y), new Point2d(ep.X, ep.Y));
            Line2d pSeg = seg.GetPerpendicularLine(new Point2d(p.X, p.Y));
            Point2d[] pts = pSeg.IntersectWith(seg.GetLine(),new Tolerance(0.1, 0.1));
            return new Point3d(pts[0].X, pts[0].Y, 0);
        }

        public static Point3d GetPointPolar(Point3d ptBase, double angle, double length)
        {
            return new Point3d(ptBase.X + (length * Math.Cos(angle)), ptBase.Y + (length * Math.Sin(angle)), 0);
        }

        public static Point3d GetRectPoint(Point3d pointStart, double x, double y)
        {
            return new Point3d(pointStart.X + x, pointStart.Y + y, 0);
        }

        public static Matrix3d GetWcsToEcsTransform(Entity entity)
        {
            return Matrix3d.WorldToPlane(entity.GetPlane().Normal);
        }

        public static Point3d Inters(Point3d p1, Point3d p2, Point3d p3, Point3d p4)
        {
            Entity l1 = new Line(p1, p2);
            Entity l2 = new Line(p3, p4);

            Point3dCollection intPts = new Point3dCollection();
            l1.IntersectWith(l2, Intersect.ExtendBoth, intPts, new IntPtr(1), new IntPtr(1));
            if (intPts != null && intPts.Count > 0)
            {
                return intPts[0];
            }

            l1.Dispose();
            l1 = null;
            l2.Dispose();
            l2 = null;

            return Point3d.Origin;
        }

        public static bool IsLineOnPolyline(Line lnSegment, Polyline plineEntity)
        {
            bool retResult = false;
            try
            {
                DBObjectCollection entitySet = new DBObjectCollection();
                plineEntity.Explode(entitySet);


                LineSegment2d lineSeg1 = new LineSegment2d(
                    new Point2d(lnSegment.StartPoint.X, lnSegment.StartPoint.Y),
                    new Point2d(lnSegment.EndPoint.X, lnSegment.EndPoint.Y));

                foreach (DBObject dbObj in entitySet)
                {
                    if (!(dbObj is Line)) continue;
                    Line lineEntity = dbObj as Line;

                    LineSegment2d lineSeg = new LineSegment2d(
                        new Point2d(lineEntity.StartPoint.X, lineEntity.StartPoint.Y),
                        new Point2d(lineEntity.EndPoint.X, lineEntity.EndPoint.Y));

                    if (lineSeg.IsColinearTo(lineSeg1))
                    {
                        retResult = true;
                        break;
                    }
                }

                //clear memory
                foreach (DBObject obj in entitySet)
                {
                    obj.Dispose();
                }

            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        public static bool IsPlineFullyInsidePline(Polyline plineToTest, Polyline plineContainer)
        {
            Point3dCollection plPoints = new Point3dCollection();
            int vn = plineToTest.NumberOfVertices;
            for (int i = 0; i < vn; i++)
            {
                plPoints.Add(plineToTest.GetPoint3dAt(i));
            }

            bool retResult = true;
            foreach (Point3d ptx in plPoints)
            {
                if (!DwgGeometry.IsPointOnPolyline(ptx, plineContainer))
                {
                    if (!DwgGeometry.IsPointInsidePolyline(ptx, plineContainer))
                    {
                        retResult = false;
                    }
                }
            }

            return retResult;
        }

        public static bool IsPointInside(Point3d cpt, Entity objEntity)
        {
            Point3d pt2 = DwgGeometry.GetPointPolar(cpt, 0, int.MaxValue * 0.125);

            Entity lnSegment1 = new Line(cpt, pt2);

            Point3dCollection pts = new Point3dCollection();
            lnSegment1.IntersectWith(objEntity, Intersect.OnBothOperands, pts, new IntPtr(0), new IntPtr(0));

            int cnt = pts.Count % 2;

            //free line segment
            lnSegment1.Dispose();
            lnSegment1 = null;

            return (cnt == 1);
        }

        public static bool IsPointInsidePolygon(Point3dCollection vertices, Point3d p)
        {
            bool retResult = false;
            int counter = 0;
            int i;
            double xinters;
            Point3d pt1, pt2;
            int maxPoints = vertices.Count; //number of points in polygon
            pt1 = vertices[0];
            for (i = 1; i <= maxPoints; i++)
            {
                pt2 = vertices[i % maxPoints];
                if (p.Y > Math.Min(pt1.Y, pt2.Y))
                {
                    if (p.Y <= Math.Max(pt1.Y, pt2.Y))
                    {
                        if (p.X <= Math.Max(pt1.X, pt2.X))
                        {
                            if (pt1.Y != pt2.Y)
                            {
                                xinters = (p.Y - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y) + pt1.X;
                                if (pt1.X == pt2.X || p.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }

                pt1 = new Point3d(pt2.X, pt2.Y, 0);
            }

            retResult = ((counter % 2) == 1);

            return retResult;
        }

        public static bool IsPointInsidePolyline(Point3d cpt, Polyline plineEntity)
        {
            double len = plineEntity.GeometricExtents.MinPoint.DistanceTo(plineEntity.GeometricExtents.MaxPoint) * 10;
            Point3d pt2 = DwgGeometry.GetPointPolar(cpt, Math.PI * 0.25, len);
            Entity lnSegment1 = new Line(cpt, pt2);

            Point3dCollection pts = new Point3dCollection();
            lnSegment1.IntersectWith((Entity)plineEntity, Intersect.OnBothOperands, pts, new IntPtr(0), new IntPtr(0));

            if (pts.Count == 0)
            {
                return false;
            }

            int cnt = pts.Count % 2;

            //free line object
            lnSegment1.Dispose();
            lnSegment1 = null;

            return (cnt == 1);
        }

        public static bool IsPointOnLineSegment(Point3d lineStartPoint, Point3d lineEndPoint, Point3d p)
        {
            bool retResult = false;
            LineSegment2d lineSeg = new LineSegment2d(new Point2d(lineStartPoint.X, lineStartPoint.Y),
                new Point2d(lineEndPoint.X, lineEndPoint.Y));

            if (lineSeg.IsOn(new Point2d(p.X, p.Y), new Tolerance(0.0001, 0.0001)))
            {
                retResult = true;
            }

            return retResult;
        }

        public static bool IsPointOnPolyline(Point3d testPoint, Polyline plineEntity)
        {
            Point2d ptTest = new Point2d(testPoint.X, testPoint.Y);

            bool retResult = false;
            try
            {
                DBObjectCollection entitySet = new DBObjectCollection();
                plineEntity.Explode(entitySet);

                foreach (DBObject dbObj in entitySet)
                {
                    if (!(dbObj is Line)) continue;
                    Line lineEntity = dbObj as Line;

                    LineSegment2d lineSeg = new LineSegment2d(
                        new Point2d(lineEntity.StartPoint.X, lineEntity.StartPoint.Y),
                        new Point2d(lineEntity.EndPoint.X, lineEntity.EndPoint.Y));

                    if (lineSeg.IsOn(ptTest, new Tolerance(0.0001, 0.0001)))
                    {
                        retResult = true;
                        break;
                    }
                }

                //clear memory
                foreach (DBObject obj in entitySet)
                {
                    obj.Dispose();
                }
            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        /// <summary>
        /// Return true if point is on right side of the line formed start point and end point
        /// </summary>
        /// <param name="startpoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="TestPoint"></param>
        /// <returns></returns>
        public static bool IsPointOnRightSide(Point3d startpoint, Point3d endPoint, Point3d TestPoint)
        {
            Vector3d vecLine = endPoint - startpoint;
            Vector3d vecPerLine = vecLine.GetPerpendicularVector();
            Vector3d vecSide = TestPoint - startpoint;
            double d = vecSide.DotProduct(vecPerLine);

            bool retResult = false;
            if (Math.Sign(d) == -1)
            {
                retResult = true;
            }

            return retResult;
        }

        /// <summary>
        /// Check two regions are touching each other or not
        /// </summary>
        /// <param name="sourceRegion"></param>
        /// <param name="targetRegion"></param>
        /// <returns></returns>
        public static bool IsRegionTouchingToRegion(Region sourceRegion, Region targetRegion)
        {
            bool retResult = false;
            try
            {
                //check the region are intersecting or not by inflating with 10 unit
                RealRectangle rrSource = new RealRectangle(sourceRegion.GeometricExtents.MinPoint, sourceRegion.GeometricExtents.MaxPoint);
                RealRectangle rrTarget = new RealRectangle(targetRegion.GeometricExtents.MinPoint, targetRegion.GeometricExtents.MaxPoint);
                rrSource.Inflate(10, 10);
                rrTarget.Inflate(10, 10);
                if (!rrTarget.IntersectsWith(rrSource)) return retResult;
                DBObjectCollection entitySetSource = new DBObjectCollection();
                sourceRegion.Explode(entitySetSource);

                DBObjectCollection entitySetTarget = new DBObjectCollection();
                targetRegion.Explode(entitySetTarget);

                foreach (DBObject dbObj in entitySetSource)
                {
                    if (!(dbObj is Line)) continue;
                    Line lineEntity = dbObj as Line;

                    LineSegment2d lineSeg = new LineSegment2d(
                        new Point2d(lineEntity.StartPoint.X, lineEntity.StartPoint.Y),
                        new Point2d(lineEntity.EndPoint.X, lineEntity.EndPoint.Y));

                    foreach (DBObject dbObjTarget in entitySetTarget)
                    {
                        if (!(dbObjTarget is Line)) continue;
                        Line lineEntityTarget = dbObjTarget as Line;

                        LineSegment2d lineSegTarget = new LineSegment2d(
                            new Point2d(lineEntityTarget.StartPoint.X, lineEntityTarget.StartPoint.Y),
                            new Point2d(lineEntityTarget.EndPoint.X, lineEntityTarget.EndPoint.Y));

                        LinearEntity2d overlapRes = lineSeg.Overlap(lineSegTarget, new Tolerance(0.01, 0.01));
                        if (overlapRes != null && overlapRes.StartPoint.GetDistanceTo(overlapRes.EndPoint) > 0.1)
                        {
                            retResult = true;
                            break;
                        }
                    }

                    if (retResult == true)
                    {
                        break;
                    }
                }

                //clear memory
                foreach (DBObject obj in entitySetSource)
                {
                    obj.Dispose();
                }

                //clear memory
                foreach (DBObject obj in entitySetTarget)
                {
                    obj.Dispose();
                }
            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        public static Matrix3d ModelSpaceToPaperSpaceTransformMatrix(Viewport pVp)
        {
            // first get all the data
            Vector3d viewDirection = pVp.ViewDirection;
            Point3d viewCenter = new Point3d(pVp.ViewCenter.X, pVp.ViewCenter.Y, 0);
            Point3d viewTarget = pVp.ViewTarget;
            Point3d centerPoint = pVp.CenterPoint;

            double twistAngle = -pVp.TwistAngle;
            double viewHeight = pVp.ViewHeight;
            double height = pVp.Height;
            double width = pVp.Width;
            double scaling = viewHeight / height;
            double lensLength = pVp.LensLength;

            // prepare the transformation
            Vector3d zAxis = viewDirection.GetNormal();
            Vector3d xAxis = Vector3d.ZAxis.CrossProduct(viewDirection);
            Vector3d yAxis;

            if (!xAxis.IsZeroLength())
            {
                xAxis = xAxis.GetNormal();
                yAxis = zAxis.CrossProduct(xAxis);
            }
            else if (zAxis.Z < 0)
            {
                xAxis = Vector3d.XAxis * -1;
                yAxis = Vector3d.YAxis;
                zAxis = Vector3d.ZAxis * -1;
            }
            else
            {
                xAxis = Vector3d.XAxis;
                yAxis = Vector3d.YAxis;
                zAxis = Vector3d.ZAxis;
            }

            Matrix3d dcs2wcs; // display coordinate system (DCS) to world coordinate system (WCS)
            Matrix3d ps2Dcs;  // paperspace to DCS


            // First initialise with a transformation to centerPoint
            ps2Dcs = Matrix3d.Displacement(Point3d.Origin - centerPoint);

            // then scale for the view
            ps2Dcs = ps2Dcs * Matrix3d.Scaling(scaling, centerPoint);

            // then adjust to the viewCenter
            dcs2wcs = Matrix3d.Displacement(viewCenter - Point3d.Origin);

            // Then transform for the view direction 
            Matrix3d matCoords = Matrix3d.AlignCoordinateSystem(Point3d.Origin,
                Vector3d.XAxis,
                Vector3d.YAxis,
                Vector3d.ZAxis,
                Point3d.Origin,
                xAxis,
                yAxis,
                zAxis);

            dcs2wcs = matCoords * dcs2wcs;

            // Then adjust for the viewTarget
            dcs2wcs = Matrix3d.Displacement(viewTarget - Point3d.Origin) * dcs2wcs;

            // Then the twist angle
            dcs2wcs = Matrix3d.Rotation(twistAngle, zAxis, viewTarget) * dcs2wcs;


            Matrix3d perspMat = Matrix3d.Identity;

            if (pVp.PerspectiveOn)
            {
                // we do special perspective handling
                double viewSize = viewHeight;
                double aspectRatio = width / height;

                double adjustFactor = 1.0 / 42.0;
                double adjustedLensLength = viewSize * lensLength * Math.Sqrt(1.0 + aspectRatio * aspectRatio) * adjustFactor;

                double eyeDistance = viewDirection.Length;
                double lensDistance = eyeDistance - adjustedLensLength;

                double ed = eyeDistance;
                double ll = adjustedLensLength;
                double l = lensDistance;

                double[] data = new double[] {1,0,0,0,
                                              0,1,0,0,
                                              0,0, (ll - l ) / ll, l * ( ed - ll ) / ll,
                                              0,0, -1.0/ll, ed/ll};

                perspMat = new Matrix3d(data);
            }

            Matrix3d resultMat = ps2Dcs.Inverse() * perspMat * dcs2wcs.Inverse();

            return resultMat;
        }

        public static Matrix3d PaperSpaceToModelSpaceTransformMatrix(Viewport pVp)
        {
            // Keep life simple, just invert the other way 
            return ModelSpaceToPaperSpaceTransformMatrix(pVp).Inverse();
        }

        /// <summary>
        /// Degrees to radian
        /// </summary>
        /// <param name="ang"></param>
        /// <returns></returns>
        public static double dtr(double ang)
        {
            return ((Math.PI * ang) / 180.0);
        }

        /// <summary>
        /// Check that polylines intersect or not
        /// these polyline intersect but they do not have any common area and that is why these 
        /// polylines need to be indentified.
        /// </summary>
        /// <param name="pline1"></param>
        /// <param name="pline2"></param>
        /// <returns></returns>
        public static bool isPlineIntersectsWithOtherPline(Polyline pline1, Polyline pline2)
        {
            Point3dCollection pts = new Point3dCollection();
            ((Entity)pline1).IntersectWith(pline2, Intersect.OnBothOperands, pts, new IntPtr(0), new IntPtr(0));

            if (pts.Count == 0) return false;


            if (AreAllPoinstAsVertex(pts, pline1) || AreAllPoinstAsVertex(pts, pline2))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Radians to degrees
        /// </summary>
        /// <param name="ang"></param>
        /// <returns></returns>
        public static double rtd(double ang)
        {
            return ((180.0 * ang) / Math.PI);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check that all given points are same as pline vertices
        /// this routine helps to get only polyline that intersecting on edges and does not have any common area
        /// </summary>
        /// <param name="ptList"></param>
        /// <param name="plEntity"></param>
        /// <returns></returns>
        private static bool AreAllPoinstAsVertex(Point3dCollection ptList, Polyline plEntity)
        {
            Point3dCollection plPoints = new Point3dCollection();
            int vn = plEntity.NumberOfVertices;
            for (int i = 0; i < vn; i++)
            {
                plPoints.Add(plEntity.GetPoint3dAt(i));
            }


            //remove vertex points
            Point3dCollection ptsToRemove = new Point3dCollection();
            foreach (Point3d ptx in ptList)
            {
                if (IsPointContainsInPointList(ptx, plPoints))
                {
                    ptsToRemove.Add(ptx);
                }
            }
            //remove points those are vertex of polyline
            foreach (Point3d ptx in ptsToRemove)
            {
                ptList.Remove(ptx);
            }

            bool retResult = true;
            foreach (Point3d ptx in ptList)
            {
                if (!IsPointContainsInPointList(ptx, plPoints))
                {
                    retResult = false;
                    break;
                }
            }

            return retResult;
        }

        /// <summary>
        /// Check that given point contains in the point list or not
        /// </summary>
        /// <param name="ptTest"></param>
        /// <param name="plPoints"></param>
        /// <returns></returns>
        private static bool IsPointContainsInPointList(Point3d ptTest, Point3dCollection plPoints)
        {
            foreach (Point3d ptx in plPoints)
            {
                if (ptTest.DistanceTo(ptx) < 0.0001)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// this class is used in drawing reveal for panels
    /// </summary>
    public class Point3DLength : IComparable
    {
        #region Fields

        private double m_length;

        private Point3d m_point;

        #endregion

        #region Constructors and Destructors

        public Point3DLength()
        {
        }

        public Point3DLength(Point3d pt1, Point3d refPoint)
        {
            this.m_point = pt1;
            this.m_length = refPoint.DistanceTo(pt1);
        }

        #endregion

        #region Public Properties

        public double Length
        {
            get { return this.m_length; }
            set { this.m_length = value; }
        }

        public Point3d Point
        {
            get { return this.m_point; }
            set { this.m_point = value; }
        }

        #endregion

        #region Public Methods and Operators

        public int CompareTo(object obj)
        {
            int ret = 0;

            if (!(obj is Point3DLength)) return 0;

            Point3DLength ptl = obj as Point3DLength;

            if (this.m_length < ptl.m_length)
            {
                ret = 1;
            }
            else if (this.m_length > ptl.m_length)
            {
                ret = -1;
            }

            return ret;
        }

        #endregion
    }

}
