namespace Steel_Bracing_2d.DetailParts
{
    using System.Collections.Generic;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;
    using Steel_Bracing_2d.Metaphore;

    public class DrawPipeBend
    {
        private double _bendRadius;
        public double BendRadius
        {
            get { return this._bendRadius; }
            set { this._bendRadius = value; }
        }

        private DuctInformation _firstDuct;
        public DuctInformation FirstDuct
        {
            get { return this._firstDuct; }
            set { this._firstDuct = value; }
        }

        private DuctInformation _secondDuct;
        public DuctInformation SecondDuct
        {
            get { return this._secondDuct; }
            set { this._secondDuct = value; }
        }

        private bool _drawCenterLine = true;
        public bool DrawCenterLine
        {
            get { return this._drawCenterLine; }
            set { this._drawCenterLine = value; }
        }

        private string _centerLineLayer = "center";
        public string CenterLineLayer
        {
            get { return this._centerLineLayer; }
            set { this._centerLineLayer = value; }
        }

        private string _continuousLineLayer = "objects";
        public string ContinuousLineLayer
        {
            get { return this._continuousLineLayer; }
            set { this._continuousLineLayer = value; }
        }

        private string _hiddenLineLayer = "hidden";
        public string HiddenLineLayer
        {
            get { return this._hiddenLineLayer; }
            set { this._hiddenLineLayer = value; }
        }

        public DrawPipeBend()
        {
        }

        public DrawPipeBend(double rad, DuctInformation duct1, DuctInformation duct2)
        {
            this._bendRadius = rad;
            this._firstDuct = duct1;
            this._secondDuct = duct2;
        }

        public void ExecuteCommand()
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
            List<ObjectId> firstDuctLineIds = new List<ObjectId>();
            List<ObjectId> secondDuctLineIds = new List<ObjectId>();
            using (DocumentLock docLock = CadApplication.CurrentDocument.LockDocument())
            {
                foreach (long hnd in this._firstDuct.HandleList)
                {
                    ObjectId entId;
                    if (currDwg.HandleToObjectId(new Handle(hnd), out entId))
                    {
                        firstDuctLineIds.Add(entId);
                    }
                }

                foreach (long hnd in this._secondDuct.HandleList)
                {
                    ObjectId entId;
                    if (currDwg.HandleToObjectId(new Handle(hnd), out entId))
                    {
                        secondDuctLineIds.Add(entId);
                    }
                }

                Line l1 = currDwg.GetEntity(firstDuctLineIds[0]) as Line;
                Line l2 = currDwg.GetEntity(secondDuctLineIds[0]) as Line;

                Point3d ipt = DwgGeometry.Inters(l1.StartPoint, l1.EndPoint, l2.StartPoint, l2.EndPoint);
                Point3d ipt1 = ipt.DistanceTo(l1.StartPoint) < ipt.DistanceTo(l1.EndPoint) ? l1.EndPoint : l1.StartPoint;
                Point3d ipt2 = ipt.DistanceTo(l2.StartPoint) < ipt.DistanceTo(l2.EndPoint) ? l2.EndPoint : l2.StartPoint;
                Point3d mpt = DwgGeometry.GetMidPoint(ipt1, ipt2);

                Point3d perPt1 = DwgGeometry.GetPerpendicularPoint(l1.StartPoint, l1.EndPoint, mpt);
                Point3d perPt2 = DwgGeometry.GetPerpendicularPoint(l2.StartPoint, l2.EndPoint, mpt);

                Line l11 = currDwg.GetEntity(firstDuctLineIds[1]) as Line;
                Line l22= currDwg.GetEntity(secondDuctLineIds[1]) as Line;

                Point3d perPt11 = DwgGeometry.GetPerpendicularPoint(l11.StartPoint, l11.EndPoint, mpt);
                Point3d perPt22 = DwgGeometry.GetPerpendicularPoint(l22.StartPoint, l22.EndPoint, mpt);

                Line oLine1 = l1;
                Line oLine2 = l11;
                if (mpt.DistanceTo(perPt1) > mpt.DistanceTo(perPt11))
                {
                    oLine1 = l11;
                    oLine2 = l1;
                }

                Line oLine11 = l2;
                Line oLine22 = l22;
                if (mpt.DistanceTo(perPt2) > mpt.DistanceTo(perPt22))
                {
                    oLine11 = l22;
                    oLine22 = l2;
                }

                double od = this._firstDuct.DuctId + this._firstDuct.DuctThk + this._firstDuct.DuctThk;
                double off = this._bendRadius - (od * 0.5);

                Point3d p1 = DwgGeometry.GetPointPolar(oLine1.StartPoint, DwgGeometry.AngleFromXAxis(perPt1, mpt), off);
                Point3d p2 = DwgGeometry.GetPointPolar(oLine1.EndPoint, DwgGeometry.AngleFromXAxis(perPt1, mpt), off);
                Point3d p3 = DwgGeometry.GetPointPolar(oLine11.StartPoint, DwgGeometry.AngleFromXAxis(perPt2, mpt), off);
                Point3d p4 = DwgGeometry.GetPointPolar(oLine11.EndPoint, DwgGeometry.AngleFromXAxis(perPt2, mpt), off);

                Point3d cpt = DwgGeometry.Inters(p1, p2, p3, p4);

                Point3d ept1 = DwgGeometry.GetPerpendicularPoint(oLine1.StartPoint, oLine1.EndPoint, cpt);
                Point3d ept2 = DwgGeometry.GetPerpendicularPoint(oLine2.StartPoint, oLine2.EndPoint, cpt);

                Line edge1 = new Line(ept1, ept2);
                edge1.Layer = this._continuousLineLayer;
                currDwg.AddEntity(edge1);

                Point3d ept3 = DwgGeometry.GetPerpendicularPoint(oLine11.StartPoint, oLine11.EndPoint, cpt);
                Point3d ept4 = DwgGeometry.GetPerpendicularPoint(oLine22.StartPoint, oLine22.EndPoint, cpt);

                Line edge2 = new Line(ept3, ept4);
                edge2.Layer = this._continuousLineLayer;
                currDwg.AddEntity(edge2);

                Point3d apt = DwgGeometry.GetPointPolar(cpt, DwgGeometry.AngleFromXAxis(cpt, ipt), off);
                CircularArc3d carc = new CircularArc3d(ept1, apt, ept3);
                Vector3d norm = carc.Normal;
                Vector3d vec = carc.ReferenceVector;
                Plane pn = new Plane(cpt,norm);
                double angPn = vec.AngleOnPlane(pn);

                double ang1 = carc.StartAngle + angPn;
                double ang2 = carc.EndAngle + angPn;

                Arc arc1 = new Arc(cpt, norm, off, ang1, ang2);
                arc1.Layer = this._continuousLineLayer;
                currDwg.AddEntity(arc1);

                Arc arc2 = new Arc(cpt, norm, off + od, ang1, ang2);
                arc2.Layer = this._continuousLineLayer;
                currDwg.AddEntity(arc2);

                Arc arc3 = new Arc(cpt, norm, off + this._firstDuct.DuctThk, ang1, ang2);
                arc3.Layer = this._hiddenLineLayer;
                currDwg.AddEntity(arc3);

                Arc arc4 = new Arc(cpt, norm, (off + od) - this._firstDuct.DuctThk, ang1, ang2);
                arc4.Layer = this._hiddenLineLayer;
                currDwg.AddEntity(arc4);

                Arc arc5 = new Arc(cpt, norm, off + (od * 0.5), ang1, ang2);
                arc5.Layer = this._centerLineLayer;
                currDwg.AddEntity(arc5);

                //trim lines here
                foreach (ObjectId entId in firstDuctLineIds)
                {
                    Line tmpLine = currDwg.GetEntity(entId) as Line;
                    Point3d intPt = DwgGeometry.Inters(tmpLine.StartPoint, tmpLine.EndPoint, edge1.StartPoint, edge1.EndPoint);
                    if (ipt.DistanceTo(tmpLine.StartPoint) < ipt.DistanceTo(tmpLine.EndPoint))
                    {
                        currDwg.UpdateLinePoint(entId, intPt, 0);
                    }
                    else
                    {
                        currDwg.UpdateLinePoint(entId, intPt, 1);
                    }

                }

                foreach (ObjectId entId in secondDuctLineIds)
                {
                    Line tmpLine = currDwg.GetEntity(entId) as Line;
                    Point3d intPt = DwgGeometry.Inters(tmpLine.StartPoint, tmpLine.EndPoint, edge2.StartPoint, edge2.EndPoint);
                    if (ipt.DistanceTo(tmpLine.StartPoint) < ipt.DistanceTo(tmpLine.EndPoint))
                    {
                        currDwg.UpdateLinePoint(entId, intPt, 0);
                    }
                    else
                    {
                        currDwg.UpdateLinePoint(entId, intPt, 1);
                    }

                }
            }
        }

#if DEBUG
        private void DrawText(Point3d pt, string txt)
        {
            DrawingDatabase _currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
            DBText angDescText = new DBText();
            angDescText.TextString = txt;
            angDescText.Height = 5;
            angDescText.Position = pt;
            _currDwg.AddEntity(angDescText);
        }
#endif

        private void AttachInformation(ObjectIdCollection ids)
        {
            //if (ids.Count > 0)
            //{
            //    DuctInformation duct = new DuctInformation();
            //    duct.Id = Guid.NewGuid().ToString();
            //    duct.DuctId = _pipeID;
            //    duct.DuctThk = _pipeThk;
            //    foreach (ObjectId id in ids)
            //    {
            //        duct.HandleList.Add(id.Handle.Value);
            //    }

            //    DwgDatabase currDwg = new DwgDatabase(CADApplication.CurrentDocument);

            //    foreach (ObjectId id in ids)
            //    {
            //        currDwg.SaveEntityData<DuctInformation>(id, duct, DwgDatabase.ObjectDataDictionaryKey);
            //    }
            //}
        }

    }
}
