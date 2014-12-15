namespace Steel_Bracing_2d.AcFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.PlottingServices;

    using Steel_Bracing_2d.Metaphore;

    public class DrawingDatabase
    {
        #region Constants

        public const string ObjectDataDictionaryKey = "PDMSDataKey";

        #endregion

        #region Fields

        protected Database DwgDatabase = null;

        protected string DwgFileName = "";

        protected Document MDwgDocument = null;

        private readonly bool HasObjectClass;

        private bool mIsReadonly = true;

        #endregion

        #region Constructors and Destructors

        public DrawingDatabase(Database db)
        {
            this.DwgDatabase = db;

            //find this version has ObjectClass property to ObjectId class
            this.HasObjectClass = this.HasObjectClassProperty();
        }

        public DrawingDatabase(string dwgFileName)
        {
            this.DwgFileName = dwgFileName;
            Database db = new Database(false, true);
            db.ReadDwgFile(this.DwgFileName, System.IO.FileShare.Read, true, null);
            this.DwgDatabase = db;

            //find this version has ObjectClass property to ObjectId class
            this.HasObjectClass = this.HasObjectClassProperty();
        }

        public DrawingDatabase(string dwgFileName, System.IO.FileShare openMode)
        {
            this.DwgFileName = dwgFileName;
            Database db = new Database(false, true);
            db.ReadDwgFile(this.DwgFileName, openMode, true, null);
            this.DwgDatabase = db;

            if (openMode == System.IO.FileShare.Read)
            {
                this.mIsReadonly = true;
            }
            else
            {
                this.mIsReadonly = false;
            }

            //find this version has ObjectClass property to ObjectId class
            this.HasObjectClass = this.HasObjectClassProperty();
        }

        public DrawingDatabase(Document currDocument)
        {
            this.DwgFileName = currDocument.Name;
            this.MDwgDocument = currDocument;
            this.DwgDatabase = this.MDwgDocument.Database;

            this.mIsReadonly = this.MDwgDocument.IsReadOnly;

            //find this version has ObjectClass property to ObjectId class
            this.HasObjectClass = this.HasObjectClassProperty();
        }

        #endregion

        #region Public Properties

        public ObjectId BlockTableId
        {
            get
            {
                return this.DwgDatabase.BlockTableId;
            }
        }

        public double Dimscale
        {
            get { return this.DwgDatabase.Dimscale; }
            set { this.DwgDatabase.Dimscale = value; }
        }

        public Document DwgDocument
        {
            get { return this.MDwgDocument; }
            set { this.MDwgDocument = value; }
        }

        public bool IsReadonly
        {
            get { return this.mIsReadonly; }
            set { this.mIsReadonly = value; }
        }

        public double Ltscale
        {
            get { return this.DwgDatabase.Ltscale; }
            set { this.DwgDatabase.Ltscale = value; }

        }

        public bool TileMode
        {
            get
            {
                return this.DwgDatabase.TileMode;
            }
            set
            {
                this.DwgDatabase.TileMode = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Add _dot block for adding leader with dots
        /// </summary>
        /// <returns></returns>
        public ObjectId AddDotBlock()
        {
            ObjectId retBlockId;

            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                //old layer 
                ObjectId oldLayerId = this.DwgDatabase.Clayer;

                //set layer to zero
                this.SetCurrentLayer("0");

                BlockTable tbl = transaction.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForWrite) as BlockTable;

                BlockTableRecord blkRec = new BlockTableRecord();
                blkRec.Name = "_dot";

                //add entities here
                blkRec.AppendEntity(new Line(Point3d.Origin, new Point3d(-1, 0, 0)));

                //add donut
                Point3dCollection vertices = new Point3dCollection();
                vertices.Add(DwgGeometry.GetPointPolar(Point3d.Origin, Math.PI, 0.25));
                vertices.Add(DwgGeometry.GetPointPolar(Point3d.Origin, 0, 0.25));
                vertices.Add(DwgGeometry.GetPointPolar(Point3d.Origin, Math.PI, 0.25));
                vertices.Add(DwgGeometry.GetPointPolar(Point3d.Origin, 0, 0.25));

                DoubleCollection blges = new DoubleCollection();
                blges.Add(0);
                blges.Add(-1);
                blges.Add(0);
                blges.Add(1);

                blkRec.AppendEntity(new Polyline2d(Poly2dType.SimplePoly, vertices, 0, true, 0.5, 0.5, blges));

                //add to block table
                retBlockId = tbl.Add(blkRec);

                transactionManager.AddNewlyCreatedDBObject(blkRec, true);
                transaction.Commit();

                //restore layer here
                this.DwgDatabase.Clayer = oldLayerId;
            }

            return retBlockId;
        }

        /// <summary>
        /// Add layout
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public ObjectId AddDrawingLayout(string layoutName)
        {
            ObjectId retResult = ObjectId.Null;
            try
            {
                HostApplicationServices.WorkingDatabase = this.DwgDatabase;
                retResult = LayoutManager.Current.CreateLayout(layoutName);
            }
            catch
            {
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = CadApplication.CurrentDocument.Database;
            }

            return retResult;
        }

        public ObjectId AddEntity(Entity ent)
        {
            ObjectId id = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                BlockTable table = (BlockTable)transactionManager.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead, false);
                id = ((BlockTableRecord)transactionManager.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false)).AppendEntity(ent);
                transactionManager.AddNewlyCreatedDBObject(ent, true);
                transaction.Commit();
            }
            return id;
        }

        public bool AddEntityIntoGroup(ObjectId groupId, ObjectId entIdToAdd)
        {
            bool retResult = true;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    Group grp = transaction.GetObject(groupId, OpenMode.ForWrite) as Group;
                    grp.Append(entIdToAdd);

                    transaction.Commit();
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        public ObjectId AddGroup(Group gp, string gpName)
        {
            return this.AddDictionaryObject(gpName, gp, this.DwgDatabase.GroupDictionaryId);
        }

        public void AddHyperLink(ObjectId objId, string hyperLinkText)
        {
            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = (Entity)trans.GetObject(objId, OpenMode.ForWrite);

                        HyperLink hpLink = new HyperLink();
                        hpLink.Name = "PDMSHPLink";
                        hpLink.Description = hyperLinkText;
                        ent.Hyperlinks.Insert(0, hpLink);

                        trans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public void AddHyperLink(ObjectId objId, string hyperLinkName, string hyperLinkText)
        {
            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = (Entity)trans.GetObject(objId, OpenMode.ForWrite);

                        HyperLink hpLink = new HyperLink();
                        hpLink.Name = hyperLinkText;
                        hpLink.Description = hyperLinkName;
                        ent.Hyperlinks.Insert(0, hpLink);

                        trans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {

                    }
                }
            }
            catch
            {
            }
        }

        public ObjectId AddLayer(LayerTableRecord layer)
        {
            return this.AddSymbolTableRecord(layer, this.DwgDatabase.LayerTableId);
        }

        public void AddLayer(string laName, long laColor, string laLineType, bool isPlottable)
        {
            try
            {
                LayerTableRecord layerRec = new LayerTableRecord();
                layerRec.Name = laName.Trim();
                layerRec.IsPlottable = isPlottable;
                layerRec.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)laColor);

                ObjectId lineTypeId = ObjectId.Null;
                this.GetLineTypeId(laLineType, out lineTypeId);
                if (lineTypeId != ObjectId.Null)
                {
                    layerRec.LinetypeObjectId = lineTypeId;
                }

                this.AddSymbolTableRecord(layerRec, this.DwgDatabase.LayerTableId);
            }
            catch (Exception ex)
            {
            }
        }

        public void AddLayer(string laName, long laColor, string laLineType)
        {
            try
            {
                LayerTableRecord layerRec = new LayerTableRecord();
                layerRec.Name = laName.Trim();
                layerRec.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)laColor);

                ObjectId lineTypeId = ObjectId.Null;
                this.GetLineTypeId(laLineType, out lineTypeId);
                if (lineTypeId != ObjectId.Null)
                {
                    layerRec.LinetypeObjectId = lineTypeId;
                }

                this.AddSymbolTableRecord(layerRec, this.DwgDatabase.LayerTableId);
            }
            catch (Exception ex)
            {
            }
        }

        public bool AddRegisteredApplicationTableRecord(string regAppName)
        {
            bool retResult = true;
            try
            {
                using (Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    RegAppTable rat = tr.GetObject(this.DwgDatabase.RegAppTableId, OpenMode.ForRead, false) as RegAppTable;
                    if (!rat.Has(regAppName))
                    {
                        rat.UpgradeOpen();
                        RegAppTableRecord ratr = new RegAppTableRecord();
                        ratr.Name = regAppName;
                        rat.Add(ratr);
                        tr.AddNewlyCreatedDBObject(ratr, true);
                    }

                    tr.Commit();
                }
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        public ObjectId AddSymbolTableRecord(SymbolTableRecord str, ObjectId symbolTableId)
        {
            ObjectId objectId = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)transactionManager.GetObject(symbolTableId, OpenMode.ForWrite, false);
                if (!table.Has(str.Name))
                {
                    table.Add(str);
                    transactionManager.AddNewlyCreatedDBObject(str, true);
                    objectId = str.ObjectId;
                }
                else
                {
                    objectId = str.ObjectId;
                }
                transaction.Commit();
            }
            return objectId;
        }

        /// <summary>
        /// Add view
        /// </summary>
        /// <param name="vtr"></param>
        /// <returns></returns>
        public ObjectId AddView(ViewTableRecord vtr)
        {
            return this.AddSymbolTableRecord(vtr, this.DwgDatabase.ViewTableId);
        }

        /// <summary>
        /// Get panel segment dimensions
        /// </summary>
        /// <param name="pnlObjectId"></param>
        /// <param name="rotAngle"></param>
        /// <param name="panelLength"></param>
        /// <returns></returns>
        public ArrayList CalculateSegmentDimensions(ObjectId pnlObjectId, double rotAngle, double panelLength)
        {
            Entity pnlEntity = this.GetEntity(pnlObjectId);
            Point3d minPt;
            Point3d maxPt;
            DwgGeometry.CalculateBoundingBox(pnlEntity, rotAngle, out minPt, out maxPt);

            List<Point3d> segmentPointList = new List<Point3d>();

            //add start point of panel
            segmentPointList.Add(minPt);

            Point3d pnlStartPoint = segmentPointList[0];
            Point3d pnlEndPoint = maxPt;

            List<Group> lstGroups = this.GetGroupsFromEntity(pnlObjectId);

            foreach (ObjectId id in lstGroups[0].GetAllEntityIds())
            {
                Entity ent = this.GetEntity(id);
                if (ent is Polyline)
                {
                    DwgGeometry.CalculateBoundingBox(ent, rotAngle, out minPt, out maxPt);

                    Point3d segmentPoint = DwgGeometry.GetMidPoint(minPt, maxPt);

                    segmentPointList.Add(segmentPoint);
                }
            }

            //add end point of panel
            segmentPointList.Add(pnlEndPoint);

            //sort points along x-axis
            segmentPointList.Sort(delegate(Point3d a, Point3d b) { return a.X.CompareTo(b.X); });

            //calculate dimensions:
            System.Collections.ArrayList segmentDimensions = new System.Collections.ArrayList();
            Point3d lastPoint = segmentPointList[0];
            for (int k = 1; k < segmentPointList.Count; ++k)
            {
                segmentDimensions.Add(Math.Abs(lastPoint.X - segmentPointList[k].X));
                lastPoint = segmentPointList[k];
            }

            return segmentDimensions;
        }

        /// <summary>
        /// Get panel segment dimensions along with move segments
        /// </summary>
        /// <param name="pnlObjectId"></param>
        /// <param name="rotAngle"></param>
        /// <param name="panelLength"></param>
        /// <param name="matDisplacement"></param>
        /// <param name="segmentToMove"></param>
        /// <param name="isValidOP"></param>
        /// <returns></returns>
        public ArrayList CalculateSegmentDimensions(ObjectId pnlObjectId, double rotAngle, double panelLength, Matrix3d matDisplacement, List<ObjectId> segmentsToMove, out bool isValidOP)
        {
            isValidOP = true;

            Entity pnlEntity = this.GetEntity(pnlObjectId);
            Point3d minPt;
            Point3d maxPt;
            DwgGeometry.CalculateBoundingBox(pnlEntity, rotAngle, out minPt, out maxPt);

            List<Point3d> segmentPointList = new List<Point3d>();

            //add start point of panel
            segmentPointList.Add(minPt);

            Point3d pnlStartPoint = segmentPointList[0];
            Point3d pnlEndPoint = maxPt;

            List<Group> lstGroups = this.GetGroupsFromEntity(pnlObjectId);

            //matrix to rotate panel to align to x - axis
            Matrix3d matRotation = Matrix3d.Rotation(DwgGeometry.dtr(rotAngle), new Vector3d(0, 0, 1), Point3d.Origin);

            foreach (ObjectId id in lstGroups[0].GetAllEntityIds())
            {
                Entity ent = this.GetEntity(id);
                if (ent is Polyline)
                {
                    DwgGeometry.CalculateBoundingBox(ent, rotAngle, out minPt, out maxPt);

                    Point3d segmentPoint = DwgGeometry.GetMidPoint(minPt, maxPt);

                    if (segmentsToMove.Contains(id))
                    {
                        //un rotate point to back 
                        segmentPoint = segmentPoint.TransformBy(matRotation);
                        segmentPoint = segmentPoint.TransformBy(matDisplacement);

                        //now point should be ok
                        segmentPoint = segmentPoint.TransformBy(matRotation.Inverse());
                        if (segmentPoint.X > pnlStartPoint.X && segmentPoint.X < pnlEndPoint.X)
                        {
                            segmentPointList.Add(segmentPoint);
                        }
                        else
                        {
                            isValidOP = false;
                            return null;
                        }
                    }
                    else
                    {
                        segmentPointList.Add(segmentPoint);
                    }
                }
            }

            //add end point of panel
            segmentPointList.Add(pnlEndPoint);

            //sort points along x-axis
            segmentPointList.Sort(delegate(Point3d a, Point3d b) { return a.X.CompareTo(b.X); });

            //calculate dimensions:
            System.Collections.ArrayList segmentDimensions = new System.Collections.ArrayList();
            Point3d lastPoint = segmentPointList[0];
            for (int k = 1; k < segmentPointList.Count; ++k)
            {
                segmentDimensions.Add(Math.Abs(lastPoint.X - segmentPointList[k].X));
                lastPoint = segmentPointList[k];
            }

            return segmentDimensions;
        }

        public bool ClearBlockAttributeValues(ObjectId blkId)
        {
            bool retResult = false;
            try
            {
                using (Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    using (BlockReference blkRef = tr.GetObject(blkId, OpenMode.ForWrite) as BlockReference)
                    {
                        using (BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForWrite))
                            if (blkObj.HasAttributeDefinitions)
                            {
                                foreach (ObjectId attId in blkRef.AttributeCollection)
                                {
                                    AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForWrite);
                                    attRef.TextString = "";
                                }
                            }
                    }
                    tr.Commit();
                }

                retResult = true;
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        public ObjectId Copy(Entity ent)
        {
            return this.Copy(ent.ObjectId);
        }

        public ObjectId Copy(ObjectId idCopy)
        {
            ObjectId id;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(idCopy, OpenMode.ForRead, true);
                Entity ent = (Entity)entity.Clone();
                id = this.AddEntity(ent);
                transaction.Commit();
            }
            return id;
        }

        public ObjectId CreateBlock(string blockName, Point3d insPt, DBObjectCollection entities)
        {
            BlockTableRecord blkRec = new BlockTableRecord();
            blkRec.Name = blockName;
            foreach (Entity ent in entities)
            {
                blkRec.AppendEntity(ent);
            }

            blkRec.Origin = insPt;

            return this.AddSymbolTableRecord(blkRec, this.DwgDatabase.BlockTableId);
        }

        /// <summary>
        /// Add extension dictionary
        /// </summary>
        /// <param name="id"></param>
        public void CreateExtensionDictionary(ObjectId id)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                DBObject entity = transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.CreateExtensionDictionary();
                transaction.Commit();
            }
        }

        public void CreateWBlock(ObjectId objId, string nameToBeSave)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (DocumentLock docLock = this.MDwgDocument.LockDocument())
            {
                try
                {
                    ObjectIdCollection objIds = new ObjectIdCollection();
                    objIds.Add(objId);
                    Database newDb = new Database();
                    db.Wblock(newDb, objIds, Point3d.Origin, DuplicateRecordCloning.Replace);

                    //check extention is added 
                    if (!nameToBeSave.ToLower().Contains(".dwg"))
                    {
                        nameToBeSave = nameToBeSave + ".dwg";
                    }
                    string FileName = nameToBeSave;

                    newDb.SaveAs(FileName, DwgVersion.Current);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Unexpected Error: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Delete layout
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public bool DeleteDrawingLayout(string layoutName)
        {
            bool retResult = true;
            try
            {
                HostApplicationServices.WorkingDatabase = this.DwgDatabase;
                LayoutManager.Current.DeleteLayout(layoutName);
            }
            catch
            {
                retResult = false;
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = CadApplication.CurrentDocument.Database;
            }

            return retResult;
        }

        /// <summary>
        /// Remove attached data from the entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="strApplicationDictKey"></param>
        /// <returns></returns>
        public bool DeleteEntityCustomData(ObjectId entityID, string strApplicationDictKey)
        {
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                DBObject ent = (DBObject)trans.GetObject(entityID, OpenMode.ForWrite, true);
                DBDictionary entExtDict = null;
                try
                {
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite, false);
                }
                catch
                {
                    //There is no data at all, so exit from here
                    return true;
                }

                //Try removing data, remove only the data that we have attached
                try
                {
                    entExtDict.Remove(strApplicationDictKey);
                }
                catch
                {
                    //there is no data that we have added, so exit from here
                    return true;
                }

                trans.Commit();
            }

            return true;
        }

        /// <summary>
        /// Delete the view with given name
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool DeleteView(string viewName)
        {
            bool retResult = false;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.ViewTableId, OpenMode.ForWrite, false);
                    if (table.Has(viewName) == true)
                    {
                        //erase record here
                        transactionManager.GetObject(table[viewName], OpenMode.ForWrite, true).Erase();
                        retResult = true;
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Delete Xref instance bu given name
        /// </summary>
        /// <returns></returns>
        public bool DeleteXrefInstance(string xrefName)
        {
            bool retResult = true;

            //delete xref instance here
            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            //we can skip all the objects except block references
                            if (this.HasObjectClass &&
                                !this.IsValidObjectClass(objId, new List<string>() { "BLOCKREFERENCE", "INSERT" }))
                            {
                                continue;
                            }

                            Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            if (ent is BlockReference)
                            {
                                BlockReference blkRef = ent as BlockReference;

                                BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                                if (blkObj.IsFromExternalReference && blkObj.Name.ToUpper() == xrefName.ToUpper())
                                {
                                    ent.UpgradeOpen();
                                    ent.Erase();
                                    break;
                                }
                            }
                        }
                    }
                }

                tr.Commit();
            }

            //delete xref view also
            this.DeleteView(xrefName);

            return retResult;
        }

        public void Erase(Entity ent)
        {
            this.Erase(ent.ObjectId);
        }

        public void Erase(ObjectId id)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                transactionManager.GetObject(id, OpenMode.ForWrite, true).Erase();
                transaction.Commit();
            }
        }

        public void EraseGroupByEntityId(ObjectId entityId)
        {
            List<Group> grpList = this.GetGroupsFromEntity(entityId);
            if (grpList.Count == 0)
            {
                this.Erase(entityId);
                return;
            }

            foreach (Group grp in grpList)
            {
                ObjectId[] ids = grp.GetAllEntityIds();
                foreach (ObjectId entId in ids)
                {
                    this.Erase(entId);
                }
            }
        }

        /// <summary>
        /// Get All attribute tag names
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllAttributeNames()
        {
            List<string> attList = new List<string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            DBObject dbObj = this.GetObject(objId);
                            if (dbObj is AttributeDefinition)
                            {
                                attList.Add((dbObj as AttributeDefinition).Tag);
                            }
                        }
                    }
                }
            }

            return attList;
        }

        /// <summary>
        /// get all entity ids from the model space
        /// </summary>
        /// <returns></returns>
        public ObjectIdCollection GetAllEntityIds()
        {
            ObjectIdCollection objectIdList = new ObjectIdCollection();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            objectIdList.Add(objId);
                        }
                    }
                }
            }

            return objectIdList;
        }

        public List<string> GetAllLayers()
        {
            List<string> lstLayers = new List<string>();
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.LayerTableId, OpenMode.ForRead, false);
                    foreach (ObjectId id in table)
                    {
                        LayerTableRecord la = this.DwgDatabase.TransactionManager.GetObject(id, OpenMode.ForRead, true) as LayerTableRecord;
                        lstLayers.Add(la.Name);
                    }
                }
            }
            catch (System.Exception e)
            {
            }

            return lstLayers;
        }

        /// <summary>
        /// Get all support from the current drawing
        /// </summary>
        /// <param name="strSuppLayer"></param>
        /// <param name="supportEntityList"></param>
        /// <returns></returns>
        public int GetAllSupports(string strSuppLayer, out ObjectIdCollection supportEntityList)
        {
            int suppCount = 0;
            supportEntityList = new ObjectIdCollection();

            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                    using (bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        using (btr)
                        {
                            foreach (ObjectId objId in btr)
                            {
                                if (this.HasObjectClass &&
                                    !this.IsValidObjectClass(objId, new List<string>() { "POLYLINE", "LWPOLYLINE" }))
                                {
                                    continue;
                                }

                                Entity dbEntity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                                if (dbEntity == null) continue;

                                if (dbEntity.Layer.ToUpper() != strSuppLayer.ToUpper()) continue;

                                if (!(dbEntity as Polyline).Closed) continue;

                                suppCount++;
                                supportEntityList.Add(objId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return suppCount;
        }

        public List<string> GetAllTextStyles()
        {
            List<string> lstStyles = new List<string>();
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.TextStyleTableId, OpenMode.ForRead, false);
                    foreach (ObjectId id in table)
                    {
                        TextStyleTableRecord sty = this.DwgDatabase.TransactionManager.GetObject(id, OpenMode.ForRead, true) as TextStyleTableRecord;
                        lstStyles.Add(sty.Name);
                    }
                }
            }
            catch (System.Exception e)
            {
            }

            return lstStyles;
        }

        /// <summary>
        /// get all used block names
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllUsedFSAndSeries(out List<string> fabDwgList)
        {
            List<string> lstSeries = new List<string>();
            fabDwgList = new List<string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            if (this.HasObjectClass && !this.IsValidObjectClass(objId, new List<string>() { "BLOCKREFERENCE", "INSERT" }))
                            {
                                continue;
                            }

                            Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            if (ent is BlockReference)
                            {
                                BlockReference blkRef = ent as BlockReference;

                                BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                                if (blkObj.HasAttributeDefinitions)
                                {
                                    BlockTableRecord dynRecordBtr = tr.GetObject(blkRef.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                                    string blkName = dynRecordBtr.Name.ToUpper();

                                    //ignore all the annonymous blocks here
                                    if (blkName.StartsWith("*")) continue;

                                    //get block attributes and check for various addfab attributes
                                    Dictionary<string, string> attValues = this.GetBlockAttributes(objId);
                                    foreach (string key in attValues.Keys)
                                    {
                                        if (key.StartsWith("ADDFAB"))
                                        {
                                            string shopDwg = attValues[key].Trim();
                                            if (shopDwg.Length > 0 && !fabDwgList.Contains(shopDwg))
                                            {
                                                fabDwgList.Add(shopDwg.ToUpper());
                                            }
                                        }

                                        //grab the series and put into list
                                        if (key == "SERCOL" || key == "SERIES" || key == "MARKNUM" || key == "MARK")
                                        {
                                            string series = attValues[key].Trim();

                                            if (key == "MARKNUM" || key == "MARK")
                                            {
                                                try
                                                {
                                                    int seriesNum = Convert.ToInt32(series);
                                                    int result = 0;
                                                    seriesNum = Math.DivRem(seriesNum, 100, out result) * 100;
                                                    lstSeries.Add(seriesNum.ToString());
                                                }
                                                catch
                                                {
                                                }
                                            }
                                            else
                                            {
                                                if (series.Length > 0 && !lstSeries.Contains(series))
                                                {
                                                    lstSeries.Add(series);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return lstSeries;
        }

        public string GetAttachedClassName(ObjectId entityID)
        {
            string strAttachedClassName = "";

            this.GetEntityProperty(entityID, "DataClassName", out strAttachedClassName);

            return strAttachedClassName;
        }

        public Dictionary<string, string> GetBlockAttributes(string strBlockName)
        {
            Dictionary<string, string> blkAttDictionary = new Dictionary<string, string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            //we can skip all the objects except block references
                            if (this.HasObjectClass &&
                                !this.IsValidObjectClass(objId, new List<string>() { "BLOCKREFERENCE", "INSERT" }))
                            {
                                continue;
                            }

                            Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            if (ent is BlockReference)
                            {
                                BlockReference blkRef = ent as BlockReference;

                                BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                                if (blkObj.HasAttributeDefinitions)
                                {
                                    if (blkObj.Name.ToUpper() == strBlockName.ToUpper())
                                    {
                                        foreach (ObjectId attId in blkRef.AttributeCollection)
                                        {
                                            AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                                            blkAttDictionary[attRef.Tag] = attRef.TextString;
                                        }

                                        return blkAttDictionary;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return blkAttDictionary;
        }

        public Dictionary<string, string> GetBlockAttributes(ObjectId blockId)
        {
            Dictionary<string, string> blkAttDictionary = new Dictionary<string, string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockReference blkRef = tr.GetObject(blockId, OpenMode.ForRead) as BlockReference;
                BlockTableRecord blkObj = tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                if (blkObj.HasAttributeDefinitions)
                {
                    foreach (ObjectId attId in blkRef.AttributeCollection)
                    {
                        AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                        blkAttDictionary[attRef.Tag] = attRef.TextString;
                    }

                    return blkAttDictionary;
                }
            }

            return blkAttDictionary;
        }

        /// <summary>
        /// return the block of selected entity, applicable to block reference only
        /// </summary>
        /// <param name="blkRefId"></param>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public bool GetBlockName(ObjectId blkRefId, out string blockName)
        {
            bool retResult = true;
            blockName = "";
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager manager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = manager.StartTransaction())
                {
                    DBObject objEnt = this.GetObject(blkRefId);
                    BlockReference blkRefEnt = objEnt as BlockReference;
                    BlockTableRecord blkObj = (BlockTableRecord)transaction.GetObject(blkRefEnt.BlockTableRecord, OpenMode.ForRead);
                    blockName = blkObj.Name;
                }
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        public bool GetBubbleCircle(ObjectId blkRefId, out Circle bubbleCirc)
        {
            bool retResult = false;
            bubbleCirc = null;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager manager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = manager.StartTransaction())
                {
                    DBObject objEnt = this.GetObject(blkRefId);
                    BlockReference blkRefEnt = objEnt as BlockReference;
                    BlockTableRecord blkObj = (BlockTableRecord)transaction.GetObject(blkRefEnt.BlockTableRecord, OpenMode.ForRead);

                    foreach (ObjectId objId in blkObj)
                    {
                        DBObject dbObj = transaction.GetObject(objId, OpenMode.ForRead);
                        if (dbObj is Circle)
                        {
                            bubbleCirc = dbObj as Circle;
                            retResult = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        public Color GetColor(Entity ent)
        {
            return ent.Color;
        }

        public Color GetColor(ObjectId id)
        {
            return this.GetEntity(id).Color;
        }

        public int GetColorIndex(Entity ent)
        {
            return ent.ColorIndex;
        }

        public int GetColorIndex(ObjectId id)
        {
            return this.GetEntity(id).ColorIndex;
        }

        /// <summary>
        /// Get the list of constant block attributes here
        /// </summary>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetConstantBlockAttributeValues(string blockName)
        {
            Dictionary<string, string> retResult = new Dictionary<string, string>();
            try
            {
                using (Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead, false);
                    if (false == table.Has(blockName)) return retResult;

                    ObjectId blkId = table[blockName];
                    using (BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkId, OpenMode.ForRead))
                    {
                        if (false == blkObj.HasAttributeDefinitions) return retResult;

                        foreach (ObjectId objId in blkObj)
                        {
                            DBObject acObj = tr.GetObject(objId, OpenMode.ForRead);
                            if (acObj is AttributeDefinition && true == (acObj as AttributeDefinition).Constant)
                            {
                                AttributeDefinition attEnt = acObj as AttributeDefinition;
                                retResult[attEnt.Tag] = attEnt.TextString;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        /// <summary>
        /// Returns the current layout name
        /// </summary>
        /// <returns></returns>
        public string GetCurrentDrawingLayout()
        {
            string retResult = "";
            try
            {
                HostApplicationServices.WorkingDatabase = this.DwgDatabase;
                retResult = LayoutManager.Current.CurrentLayout;
            }
            catch
            {
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = CadApplication.CurrentDocument.Database;
            }

            return retResult;
        }

        /// <summary>
        /// Get Current visual style name
        /// </summary>
        /// <returns></returns>
        public string GetCurrentVisualStyle()
        {
            string visualStyle = "";
            try
            {
                DBVisualStyle objStyle = this.GetObject(CadApplication.CurrentEditor.GetCurrentView().VisualStyleId) as DBVisualStyle;
                visualStyle = objStyle.Description;
            }
            catch (Exception ex)
            {
            }

            return visualStyle;
        }

        public DBObject GetDBObject(ObjectId id)
        {
            DBObject obj2;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                obj2 = transactionManager.GetObject(id, OpenMode.ForRead, true);
                transaction.Commit();
            }
            return obj2;
        }

        public List<string> GetDimesionStyleNameList()
        {
            return this.GetSymbolNameList(this.DwgDatabase.DimStyleTableId);
        }

        /// <summary>
        /// Get the _dot block object id for panel tagging with leaders
        /// </summary>
        /// <returns></returns>
        public ObjectId GetDotBlockId()
        {
            return this.GetSymbolRecordId("_dot", this.DwgDatabase.BlockTableId);
        }

        public Dictionary<string, string> GetDrawingCustomProperties()
        {
            Dictionary<string, string> dwgProps = new Dictionary<string, string>();

            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                IDictionaryEnumerator dictIter = this.DwgDatabase.SummaryInfo.CustomProperties;
                while (dictIter.MoveNext())
                {
                    dwgProps[dictIter.Key.ToString()] = dictIter.Value.ToString();
                }

                trans.Commit();
            }

            return dwgProps;
        }

        /// <summary>
        /// Return model space drawing extents
        /// </summary>
        /// <returns></returns>
        public Extents3d GetDrawingExtents()
        {
            Extents3d retExtents = new Extents3d(Point3d.Origin, Point3d.Origin);

            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                    using (bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        using (btr)
                        {
                            bool isExtentYetToSet = true;

                            foreach (ObjectId objId in btr)
                            {
                                Entity dbEntity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                                if (dbEntity == null) continue;

                                //ignore text with blanks
                                if (dbEntity is DBText)
                                {
                                    if ((dbEntity as DBText).TextString.Trim().Length == 0)
                                    {
                                        continue;
                                    }
                                }

                                if (dbEntity is MText)
                                {
                                    if ((dbEntity as MText).Contents.Trim().Length == 0)
                                    {
                                        continue;
                                    }
                                }

                                if (isExtentYetToSet)
                                {
                                    retExtents.Set(dbEntity.GeometricExtents.MinPoint, dbEntity.GeometricExtents.MaxPoint);
                                    isExtentYetToSet = false;
                                }
                                else
                                {
                                    retExtents.AddExtents(dbEntity.GeometricExtents);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return retExtents;
        }

        public bool GetDrawingProperty(string dictName, string strPropertyName, out string strPropertyValue)
        {
            strPropertyValue = "";
            bool bResult = true;

            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction()) //Start the transaction.
            {
                try
                {
                    //get named object dictionary
                    DBDictionary NOD = (DBDictionary)trans.GetObject(this.DwgDatabase.NamedObjectsDictionaryId, OpenMode.ForRead, false);

                    //get pdms dictionary
                    DBDictionary pdmsDict = (DBDictionary)trans.GetObject(NOD.GetAt(dictName), OpenMode.ForRead);

                    //get property record
                    Xrecord propXRec = (Xrecord)trans.GetObject(pdmsDict.GetAt(strPropertyName), OpenMode.ForRead);
                    TypedValue resBuf = propXRec.Data.AsArray()[0];
                    strPropertyValue = String.Format("{0}", resBuf.Value);
                }
                catch (Exception ex)
                {
                    bResult = false;
                }
            }

            return bResult;
        }

        public string GetDrawingText()
        {
            System.Text.StringBuilder cb = new System.Text.StringBuilder();
            cb.Append("\r\nDrawing Text");

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            if (ent is MText)
                            {
                                MText mtxt = ent as MText;
                                cb.Append("\r\n" + mtxt.Contents);
                            }
                            if (ent is DBText)
                            {
                                DBText dtxt = ent as DBText;
                                cb.Append("\r\n" + dtxt.TextString);
                            }

                        }
                    }
                }
            }

            return cb.ToString();
        }

        public Entity GetEntity(ObjectId id)
        {
            Entity entity;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                entity = (Entity)this.DwgDatabase.TransactionManager.GetObject(id, OpenMode.ForRead, true);
                transaction.Commit();
            }
            return entity;
        }

        public bool GetEntityProperty(ObjectId id, string strPropertyName, out string strPropertyValue)
        {
            bool bRet = true;

            strPropertyValue = "";
            DBDictionary entExtDict;
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject ent = (DBObject)trans.GetObject(id, OpenMode.ForRead, true);
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    Xrecord propXRec = (Xrecord)trans.GetObject(entExtDict.GetAt(strPropertyName), OpenMode.ForRead);
                    TypedValue resBuf = propXRec.Data.AsArray()[0];
                    strPropertyValue = String.Format("{0}", resBuf.Value);
                }
                catch (Exception ex)
                {
                    bRet = false;
                }

                trans.Commit();
            }

            return bRet;
        }

        public bool GetEntityProperty(ObjectId id, out Dictionary<string, string> lstValues)
        {
            bool bRet = true;

            lstValues = new Dictionary<string, string>();

            DBDictionary entExtDict;
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead, true);
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    foreach (string strKey in ((IDictionary)entExtDict).Keys)
                    {
                        Xrecord propXRec = (Xrecord)trans.GetObject(entExtDict.GetAt(strKey), OpenMode.ForRead);
                        TypedValue resBuf = propXRec.Data.AsArray()[0];

                        lstValues.Add(strKey, resBuf.Value.ToString());
                    }
                }
                catch (Exception ex)
                {
                    bRet = false;
                }

                trans.Commit();
            }

            return bRet;
        }

        public Point3d GetExtmax()
        {
            return this.DwgDatabase.Extmax;
        }

        public Point3d GetExtmin()
        {
            return this.DwgDatabase.Extmin;
        }

        public ObjectId GetGroupId(string grpName)
        {
            return this.GetDictionaryObject(grpName, this.DwgDatabase.GroupDictionaryId);
        }

        public List<Group> GetGroupsFromEntity(ObjectId objectId)
        {
            List<Group> groupCollection = new List<Group>();
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                ObjectIdCollection tmpCollection = transaction.GetObject(objectId, OpenMode.ForRead).GetPersistentReactorIds();

                if (tmpCollection != null)
                {
                    foreach (ObjectId entId in tmpCollection)
                    {
                        DBObject groupObject = transaction.GetObject(entId, OpenMode.ForRead);
                        if (groupObject is Group)
                        {
                            groupCollection.Add(groupObject as Group);
                        }
                    }
                }

                transaction.Commit();
            }

            return groupCollection;
        }

        public List<string> GetHiddenLayers()
        {
            List<string> lstLayers = new List<string>();
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.LayerTableId, OpenMode.ForRead, false);
                    foreach (ObjectId id in table)
                    {
                        LayerTableRecord la = this.DwgDatabase.TransactionManager.GetObject(id, OpenMode.ForRead, true) as LayerTableRecord;
                        LinetypeTableRecord lt = this.DwgDatabase.TransactionManager.GetObject(la.LinetypeObjectId, OpenMode.ForRead, true) as LinetypeTableRecord;
                        if (string.Compare(lt.Name, "hidden", true) == 0)
                        {
                            lstLayers.Add(la.Name);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
            }

            return lstLayers;
        }

        public DBObjectCollection GetIteratorForSymbolTable(ObjectId id)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            DBObjectCollection objects = new DBObjectCollection();
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                IEnumerator enumerator = ((SymbolTable)transactionManager.GetObject(id, OpenMode.ForRead, false)).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    id = (ObjectId)enumerator.Current;
                    ids.Add(id);
                }
                transaction.Commit();
            }
            foreach (ObjectId id2 in ids)
            {
                objects.Add(this.GetDBObject(id2));
            }
            return objects;
        }

        public ObjectIdCollection GetIteratorForSymbolTableID(ObjectId id)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                IEnumerator enumerator = ((SymbolTable)transactionManager.GetObject(id, OpenMode.ForRead, false)).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    id = (ObjectId)enumerator.Current;
                    ids.Add(id);
                }
                transaction.Commit();
                return ids;
            }
        }

        public long GetLastEntityHandle()
        {
            long lastEntHandle = 0;

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            lastEntHandle = objId.Handle.Value;
                        }
                    }
                }
            }

            return lastEntHandle;
        }

        public string GetLayer(Entity ent)
        {
            return ent.Layer;
        }

        public string GetLayer(ObjectId id)
        {
            return this.GetEntity(id).Layer;
        }

        public List<string> GetLayoutNameList()
        {
            List<string> layoutNameList = new List<string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);
                ObjectIdCollection layoutsToPlot = new ObjectIdCollection();
                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                    if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                    {
                        BlockTableRecord btrLayout = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                        Layout lo = (Layout)tr.GetObject(btrLayout.LayoutId, OpenMode.ForRead);
                        layoutNameList.Add(lo.LayoutName);
                    }
                }
            }

            return layoutNameList;
        }

        public bool GetLineTypeId(string linetypeName, out ObjectId linetypeId)
        {
            bool retResult = false;
            linetypeId = ObjectId.Null;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.LinetypeTableId, OpenMode.ForRead, false);
                    retResult = table.Has(linetypeName);
                    if (retResult == false)
                    {
                        this.DwgDatabase.LoadLineTypeFile(linetypeName, "acad.lin");
                        if (table.Has(linetypeName))
                        {
                            linetypeId = table[linetypeName];
                            retResult = true;
                        }
                        else
                        {
                            retResult = false;
                        }
                    }
                    else
                    {
                        linetypeId = table[linetypeName];
                    }
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        public LineWeight GetLineWeight(Entity ent)
        {
            return ent.LineWeight;
        }

        public LineWeight GetLineWeight(ObjectId id)
        {
            return this.GetEntity(id).LineWeight;
        }

        public string GetLinetype(Entity ent)
        {
            return ent.Linetype;
        }

        public string GetLinetype(ObjectId id)
        {
            return this.GetEntity(id).Linetype;
        }

        public double GetLinetypeScale(Entity ent)
        {
            return ent.LinetypeScale;
        }

        public double GetLinetypeScale(ObjectId id)
        {
            return this.GetEntity(id).LinetypeScale;
        }

        /// <summary>
        /// Return ltscale of the current drawing
        /// </summary>
        /// <returns></returns>
        public double GetLtscale()
        {
            return this.DwgDatabase.Ltscale;
        }

        public ObjectIdCollection GetNextEntityObjectIds(long startHandle)
        {
            ObjectIdCollection retObjectIds = new ObjectIdCollection();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            if (startHandle < objId.Handle.Value)
                            {
                                retObjectIds.Add(objId);
                            }
                        }
                    }
                }
            }

            return retObjectIds;
        }

        public DBObject GetObject(ObjectId id)
        {
            DBObject entity;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                entity = (DBObject)this.DwgDatabase.TransactionManager.GetObject(id, OpenMode.ForRead, true);
                transaction.Commit();
            }
            return entity;
        }

        /// <summary>
        /// Return the object Ids staring given handle, used to find out newly copied objects
        /// </summary>
        /// <param name="startHandle"></param>
        /// <returns></returns>
        public ObjectIdCollection GetObjectIds(Handle startHandle)
        {
            ObjectIdCollection objectIdList = new ObjectIdCollection();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            if (objId.Handle.Value >= startHandle.Value)
                            {
                                objectIdList.Add(objId);
                            }
                        }
                    }
                }
            }

            return objectIdList;
        }

        public string GetObjectName(ObjectId id)
        {
            string name;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                name = transactionManager.GetObject(id, OpenMode.ForRead, true).GetType().Name;
                transaction.Commit();
            }
            return name;
        }

        /// <summary>
        /// Get specified blocks from the drawing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstPDMSObjects"></param>
        /// <returns></returns>
        public bool GetPDMSObjects<T>(out List<T> lstPDMSObjects) where T : new()
        {
            bool retResult = true;
            lstPDMSObjects = new List<T>();
            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                    using (bt)
                    {
                        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                        using (btr)
                        {
                            foreach (ObjectId objId in btr)
                            {
                                T pdmsObject = new T();
                                if (this.LoadEntityData<T>(objId, ObjectDataDictionaryKey, out pdmsObject))
                                {
                                    lstPDMSObjects.Add(pdmsObject);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Get all objects that have data attached by PDMS
        /// </summary>
        /// <param name="lstPDMSObjects"></param>
        /// <returns></returns>
        public bool GetPDMSObjects(out List<object> lstPDMSObjects)
        {
            bool retResult = true;
            lstPDMSObjects = new List<object>();
            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                    using (bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        using (btr)
                        {
                            foreach (ObjectId objId in btr)
                            {
                                object pdmsObject = new object();
                                if (this.LoadEntityData(objId, ObjectDataDictionaryKey, out pdmsObject))
                                {
                                    lstPDMSObjects.Add(pdmsObject);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Get all objects that have data attached by PDMS
        /// </summary>
        /// <param name="lstPDMSObjects"></param>
        /// <returns></returns>
        public bool GetPDMSObjects(ObjectId[] ids, out List<object> lstPDMSObjects)
        {
            bool retResult = true;
            lstPDMSObjects = new List<object>();
            try
            {
                foreach (ObjectId objId in ids)
                {
                    object pdmsObject = new object();
                    if (this.LoadEntityData(objId, ObjectDataDictionaryKey, out pdmsObject))
                    {
                        lstPDMSObjects.Add(pdmsObject);
                    }
                }
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        public List<string> GetPageSetupList()
        {
            List<string> pageSetupList = new List<string>();

            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction()) //Start the transaction.
            {
                try
                {
                    //get named object dictionary
                    DBDictionary NOD = (DBDictionary)trans.GetObject(this.DwgDatabase.NamedObjectsDictionaryId, OpenMode.ForRead, false);

                    //get pdms dictionary
                    DBDictionary pdmsDict = (DBDictionary)trans.GetObject(NOD.GetAt("ACAD_PLOTSETTINGS"), OpenMode.ForRead);

#if AC2006

#else
                    foreach (DBDictionaryEntry objSetup in pdmsDict)
                    {
                        pageSetupList.Add(objSetup.Key);
                    }
#endif

                }
                catch (Exception ex)
                {
                }
            }

            return pageSetupList;
        }

        /// <summary>
        /// Returns panel hatch object id
        /// </summary>
        /// <param name="panelId"></param>
        /// <returns></returns>
        public ObjectId GetPanelHatchId(ObjectId panelId)
        {
            ObjectId retHatchId = ObjectId.Null;

            List<Group> grpList = this.GetGroupsFromEntity(panelId);
            if (grpList.Count == 0)
            {
                return retHatchId;
            }

            foreach (Group grp in grpList)
            {
                ObjectId[] ids = grp.GetAllEntityIds();
                foreach (ObjectId entId in ids)
                {
                    if (entId == panelId) continue;
                    Entity ent = this.GetEntity(entId);

                    if (ent is Hatch)
                    {
                        retHatchId = entId;
                        break;
                    }
                }
            }

            return retHatchId;
        }

        /// <summary>
        /// Returns panel region id by specifying one of the entity id from group
        /// </summary>
        /// <param name="hatchId"></param>
        /// <returns></returns>
        public ObjectId GetPanelRegionId(ObjectId hatchId)
        {
            ObjectId retPanelRegionId = ObjectId.Null;

            List<Group> grpList = this.GetGroupsFromEntity(hatchId);
            if (grpList.Count == 0)
            {
                return retPanelRegionId;
            }

            foreach (Group grp in grpList)
            {
                ObjectId[] ids = grp.GetAllEntityIds();
                foreach (ObjectId entId in ids)
                {
                    if (entId == hatchId) continue;
                    Entity ent = this.GetEntity(entId);

                    if (ent is Region)
                    {
                        retPanelRegionId = entId;
                        break;
                    }
                }
            }

            return retPanelRegionId;
        }

        /// <summary>
        /// Get panel segment ids
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="SegmentIds"></param>
        /// <returns></returns>
        public int GetPanelSegmentIds(ObjectId entityId, out ObjectIdCollection segmentIds)
        {
            segmentIds = new ObjectIdCollection();
            int segmentCount = 0;
            List<Group> grpList = this.GetGroupsFromEntity(entityId);
            if (grpList.Count == 0)
            {
                return segmentCount;
            }

            foreach (Group grp in grpList)
            {
                ObjectId[] ids = grp.GetAllEntityIds();
                foreach (ObjectId entId in ids)
                {
                    if (entId == entityId) continue;
                    Entity ent = this.GetEntity(entId);

                    //ignore corner panel thickness line
                    if (ent.Linetype.ToLower() == "hidden") continue;

                    //segment are drawn as polylines only
                    if (ent is Polyline)
                    {
                        segmentIds.Add(entId);
                        segmentCount++;
                    }
                }
            }

            return segmentCount;
        }

        public string GetPlotStyleName(Entity ent)
        {
            return ent.PlotStyleName;
        }

        public string GetPlotStyleName(ObjectId id)
        {
            return this.GetEntity(id).PlotStyleName;
        }

        /// <summary>
        /// Count supports for detail mark line
        /// </summary>
        /// <param name="lineEnt"></param>
        /// <param name="strSuppLayer"></param>
        /// <returns></returns>
        public int GetSupportCount(Entity containerEnt, string strSuppLayer, ObjectIdCollection allSuppEntityIds, out ObjectIdCollection supportEntityList)
        {
            int suppCount = 0;
            supportEntityList = new ObjectIdCollection();

            //to check that entity is fully inside or not
            RealRectangle rectRegion = new RealRectangle(containerEnt.GeometricExtents.MinPoint, containerEnt.GeometricExtents.MaxPoint);

            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                    using (bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        using (btr)
                        {
                            foreach (ObjectId objId in allSuppEntityIds)
                            {
                                Entity dbEntity = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                                if (dbEntity == null) continue;

                                if (!(dbEntity is Polyline)) continue;

                                if (!(dbEntity as Polyline).Closed) continue;

                                if (dbEntity.Layer.ToUpper() != strSuppLayer.ToUpper()) continue;

                                Point3dCollection pts = new Point3dCollection();
                                dbEntity.IntersectWith(containerEnt, Intersect.OnBothOperands, pts, new IntPtr(0), new IntPtr(0));

                                if (pts != null && pts.Count > 0)
                                {
                                    suppCount++;
                                    supportEntityList.Add(objId);
                                }
                                else
                                {
                                    if (!(containerEnt is Line))
                                    {
                                        //if this is not line then see that this support is fully inside the other entity
                                        //bounding box

                                        RealRectangle suppRect = new RealRectangle(dbEntity.GeometricExtents.MinPoint, dbEntity.GeometricExtents.MaxPoint);
                                        if (rectRegion.Contains(suppRect))
                                        {
                                            suppCount++;
                                            supportEntityList.Add(objId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return suppCount;
        }

        /// <summary>
        /// Get the layer id, style id , line type id etc
        /// you will need to pass symbol name, symbol table id
        /// it will return null id, if not found
        /// </summary>
        /// <param name="strSymbolName"></param>
        /// <param name="symbolTableId"></param>
        /// <returns></returns>
        public ObjectId GetSymbolRecordId(string strSymbolName, ObjectId symbolTableId)
        {
            ObjectId objectId = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)transactionManager.GetObject(symbolTableId, OpenMode.ForRead, false);
                if (table.Has(strSymbolName))
                {
                    objectId = table[strSymbolName];
                }
            }

            return objectId;
        }

        public double GetTextStyleHeight(string sty)
        {
            double textHeight = 1;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.TextStyleTableId, OpenMode.ForRead, false);
                if (table.Has(sty))
                {
                    TextStyleTableRecord styRec = this.DwgDatabase.TransactionManager.GetObject(table[sty], OpenMode.ForRead, true) as TextStyleTableRecord;
                    textHeight = styRec.TextSize;
                }
            }

            return textHeight;
        }

        public ObjectId GetTextStyleId(string sty)
        {
            return this.GetSymbolRecordId(sty, this.DwgDatabase.TextStyleTableId);
        }

        public Autodesk.AutoCAD.DatabaseServices.TransactionManager GetTransactionManager()
        {
            return this.DwgDatabase.TransactionManager;
        }

        /// <summary>
        /// Figure out the current UCS matrix for the given database.  If
        /// PaperSpace is active, it will return the UCS for PaperSpace.
        /// Otherwise, it will return the UCS for the current view port in 
        /// ModelSpace.
        /// </summary>
        /// <returns>UCS Matrix for the specified database</returns>
        public Matrix3d GetUcsMatrix()
        {
            Debug.Assert(this.DwgDatabase != null, "AutoCAD Current database can not be null");

            Point3d origin;
            Vector3d xAxis, yAxis, zAxis;

            if (this.IsPaperSpace())
            {
                origin = this.DwgDatabase.Pucsorg;
                xAxis = this.DwgDatabase.Pucsxdir;
                yAxis = this.DwgDatabase.Pucsydir;
            }
            else
            {
                origin = this.DwgDatabase.Ucsorg;
                xAxis = this.DwgDatabase.Ucsxdir;
                yAxis = this.DwgDatabase.Ucsydir;
            }

            zAxis = xAxis.CrossProduct(yAxis);

            return Matrix3d.AlignCoordinateSystem(DwgGeometry.kOrigin, DwgGeometry.kXAxis, DwgGeometry.kYAxis, DwgGeometry.kZAxis, origin, xAxis, yAxis, zAxis);
        }

        /// <summary>
        /// Get the Plane that is defined by the current UCS
        /// </summary>
        /// <param name="m_dwgDatabase">Database to use</param>
        /// <returns>Plane defined by the current UCS</returns>

        public Plane GetUcsPlane()
        {
            Matrix3d m = this.GetUcsMatrix();
            CoordinateSystem3d coordSys = m.CoordinateSystem3d;

            return new Plane(coordSys.Origin, coordSys.Xaxis, coordSys.Yaxis);
        }


        /// <summary>
        /// Get the Matrix that is the Xform between UCS and WCS Origin.  This is useful
        /// for operations like creating a block definition.  For those cases you want the
        /// origin of the block to be in a reasonable spot.
        /// </summary>
        /// <param name="wcsBasePt">Base point to use as the origin</param>
        /// <param name="m_dwgDatabase">Specific database to use</param>
        /// <returns>Xform between UCS and WCS Origin</returns>

        public Matrix3d GetUcsToWcsOriginMatrix(Point3d wcsBasePt)
        {
            Matrix3d m = this.GetUcsMatrix();

            Point3d origin = m.CoordinateSystem3d.Origin;
            origin += wcsBasePt.GetAsVector();

            m = Matrix3d.AlignCoordinateSystem(origin,
                m.CoordinateSystem3d.Xaxis,
                m.CoordinateSystem3d.Yaxis,
                m.CoordinateSystem3d.Zaxis,
                DwgGeometry.kOrigin, DwgGeometry.kXAxis, DwgGeometry.kYAxis, DwgGeometry.kZAxis);

            return m;
        }

        /// <summary>
        /// Get the UCS Z Axis for the given database
        /// </summary>
        /// <param name="m_dwgDatabase">Specific database to use</param>
        /// <returns>UCS Z Axis</returns>

        public Vector3d GetUcsZAxis()
        {
            Matrix3d m = this.GetUcsMatrix();

            return m.CoordinateSystem3d.Zaxis;
        }

        public bool GetVarialbleValues(string variable, out double dwgValue)
        {
            bool retResult = false;
            dwgValue = 1.0;
            string inputVariable = variable.ToUpper();
            try
            {
                switch (inputVariable)
                {
                    case "LTSCALE":
                        dwgValue = this.DwgDatabase.Ltscale;
                        break;
                    case "DIMSCALE":
                        dwgValue = this.DwgDatabase.Dimscale;
                        break;
                    case "CELTSCALE":
                        dwgValue = this.DwgDatabase.Celtscale;
                        break;
                    default:
                        dwgValue = this.DwgDatabase.Ltscale;
                        break;
                }

                if (this.DwgDatabase.Insunits == UnitsValue.Millimeters)
                {
                    dwgValue *= 25.4;
                }
                retResult = true;
            }
            catch (Exception ex)
            {
            }
            return retResult;
        }

        /// <summary>
        /// Get XREF Block Names
        /// </summary>
        /// <returns></returns>
        public List<string> GetXRefBlockNames()
        {
            List<string> lstBlockInfo = new List<string>();

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                using (bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    using (btr)
                    {
                        foreach (ObjectId objId in btr)
                        {
                            //we can skip all the objects except block references
                            if (this.HasObjectClass &&
                                !this.IsValidObjectClass(objId, new List<string>() { "BLOCKREFERENCE", "INSERT" }))
                            {
                                continue;
                            }

                            Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                            if (ent is BlockReference)
                            {
                                BlockReference blkRef = ent as BlockReference;

                                BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);
                                if (blkObj.IsFromExternalReference)
                                {
                                    lstBlockInfo.Add(blkObj.Name);
                                }
                            }
                        }
                    }
                }
            }

            return lstBlockInfo;
        }

        public bool HandleToObjectId(Handle hnd, out ObjectId entId)
        {
            bool retResult = false;

            entId = ObjectId.Null;
            try
            {
                entId = this.DwgDatabase.GetObjectId(false, hnd, 0);

                if (!entId.IsErased) retResult = true;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
            }

            return retResult;
        }

        /// <summary>
        /// Import external drawing into current drawing
        /// </summary>
        /// <param name="strBlockFileName"></param>
        /// <returns></returns>
        public bool ImportBlock(string strBlockFileName)
        {
            bool retResult = true;

            ObjectId retBlkId = ObjectId.Null;
            try
            {
                string blkName = System.IO.Path.GetFileName(strBlockFileName.ToLower());
                blkName = blkName.Replace(".dwg", "");

                using (Database db = new Database(false, false))
                {
                    //read external drawing
                    db.ReadDwgFile(strBlockFileName, System.IO.FileShare.Read, true, null);
                    using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                    {
                        //insert it as a new block
                        retBlkId = this.DwgDatabase.Insert(blkName, db, false);
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Insert block with attributes with values and set dynamic properties as well
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="insPoint"></param>
        /// <param name="blkScale"></param>
        /// <param name="rotAng"></param>
        /// <param name="attributeValues"></param>
        /// <param name="dynamicParameterValues"></param>
        /// <returns></returns>
        public ObjectId InsertBlock(string blockName, Point3d insPoint, double blkScale, double rotAng,
            Dictionary<string, string> attributeValues, Dictionary<string, int> dynamicParameterValues)
        {
            ObjectId retBlockID = ObjectId.Null;

            ObjectId blkID = ObjectId.Null;

            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)(trans.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForWrite));
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    if (bt.Has(blockName))
                    {
                        blkID = bt[blockName];
                    }
                    else
                    {
                        return retBlockID;
                    }

                    BlockReference br = new BlockReference(insPoint, blkID);

                    int no_of_notches = 0;
                    //set dynamic properties here
                    if (dynamicParameterValues.Keys.Count > 0)
                    {
                        foreach (DynamicBlockReferenceProperty dbrProp in br.DynamicBlockReferencePropertyCollection)
                        {
                            //the no of notches to be visible and the property name is "Visibility"
                            if (dynamicParameterValues.ContainsKey(dbrProp.PropertyName))
                            {
                                no_of_notches = dynamicParameterValues[dbrProp.PropertyName];
                            }
                        }
                    }

                    List<AttributeReference> lstAR = new List<AttributeReference>();
                    // Iterate the block defination and find the attribute definition
                    BlockTableRecord empBtr = (BlockTableRecord)trans.GetObject(blkID, OpenMode.ForRead);

                    foreach (ObjectId id in empBtr)
                    {
                        Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead, false);
                        // Use it to open the current object! 
                        if (ent is AttributeDefinition)  // We use .NET's RunTimeTypeInformation (RTTI) to establish type.
                        {
                            // Set the properties from the attribute definition on our attribute reference
                            AttributeDefinition attDef = ((AttributeDefinition)(ent));

                            if (!attDef.Constant)
                            {
                                AttributeReference attRef = new AttributeReference();
                                attRef.SetDatabaseDefaults(this.DwgDatabase);
                                attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                                //control the visibility of the attribute values
                                if (attRef.Visible == false)
                                {
                                    if (attRef.Tag.StartsWith("NOTCH"))
                                    {
                                        string notch_no = attRef.Tag.Substring(5);
                                        try
                                        {
                                            int notch_num = Convert.ToInt32(notch_no);
                                            if (notch_num <= no_of_notches)
                                            {
                                                attRef.Visible = true;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    else if (attRef.Tag.StartsWith("ADDFAB2"))
                                    {
                                        if (no_of_notches > 0)
                                        {
                                            attRef.Visible = true;
                                        }
                                    }
                                }

                                if (attributeValues.ContainsKey(attRef.Tag))
                                {
                                    attRef.TextString = attributeValues[attRef.Tag];
                                }
                                else
                                {
                                    attRef.TextString = attDef.TextString;
                                }

                                //requires the working database - Kailas Dhage
                                attRef.AdjustAlignment(HostApplicationServices.WorkingDatabase);

                                lstAR.Add(attRef);
                            }
                        }
                    }

                    // Add the reference to ModelSpace
                    retBlockID = btr.AppendEntity(br);
                    trans.AddNewlyCreatedDBObject(br, true);

                    foreach (AttributeReference attRef in lstAR)
                    {
                        // Add the attribute reference to the block reference
                        br.AttributeCollection.AppendAttribute(attRef);

                        // let the transaction know
                        trans.AddNewlyCreatedDBObject(attRef, true);
                    }

                    Entity blkInstance = br as Entity;
                    if (blkInstance != null)
                    {
                        //Insertion with proper attribute has completed Scale the block as per Ltscale
                        Matrix3d transform = Matrix3d.Scaling(blkScale, insPoint);
                        blkInstance.TransformBy(transform);

                        //rotate block if rotation angle is not zero
                        transform = Matrix3d.Rotation(rotAng, DwgGeometry.kZAxis, insPoint);
                        blkInstance.TransformBy(transform);
                    }

                    //set dynamic properties here
                    if (dynamicParameterValues.Keys.Count > 0)
                    {
                        foreach (DynamicBlockReferenceProperty dbrProp in br.DynamicBlockReferencePropertyCollection)
                        {
                            //the no of notches to be visible and the property name is "Visibility"
                            if (dynamicParameterValues.ContainsKey(dbrProp.PropertyName))
                            {
                                object[] allowedValues = dbrProp.GetAllowedValues();
                                no_of_notches = dynamicParameterValues[dbrProp.PropertyName];
                                dbrProp.Value = allowedValues[no_of_notches].ToString();
                            }
                        }
                    }

                    trans.Commit();
                    trans.Dispose();
                }
            }
            catch (Exception ex)
            {
                retBlockID = ObjectId.Null;
            }

            return retBlockID;
        }

        public ObjectId InsertXref(string fileName, string blkName, Point3d insPoint, string strRelativePath)
        {
            ObjectId xrefId = CadApplication.CurrentDatabase.AttachXref(fileName, blkName);
            BlockReference xrefEntity = new BlockReference(insPoint, xrefId);
            xrefEntity.ScaleFactors = new Scale3d(1, 1, 1);
            xrefEntity.Layer = "0";

            ObjectId retObjectId = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                //change the path of the XREF block
                BlockTableRecord blkObj = (BlockTableRecord)transaction.GetObject(xrefId, OpenMode.ForWrite);
                blkObj.PathName = strRelativePath;

                //insert the XREF here
                BlockTable table = (BlockTable)transactionManager.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead, false);
                retObjectId = ((BlockTableRecord)transactionManager.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false)).AppendEntity(xrefEntity);
                transactionManager.AddNewlyCreatedDBObject(xrefEntity, true);
                transaction.Commit();
            }

            return retObjectId;
        }

        /// <summary>
        /// check the defination of given block defination exists or not
        /// </summary>
        /// <param name="strBlockName"></param>
        /// <returns></returns>
        public bool IsBlockDefinationExists(string strBlockName)
        {
            bool ret = false;

            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)(trans.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead));
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                ret = bt.Has(strBlockName);

                if (ret == true)
                {
                    //if defination is erased then return false
                    ObjectId blkDefId = bt[strBlockName];

                    if (blkDefId.IsErased == true)
                    {
                        ret = false;
                    }
                }
            }

            return ret;
        }

        public bool IsLayerExists(string layerName, out ObjectId layerId)
        {
            bool retResult = false;
            layerId = ObjectId.Null;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.LayerTableId, OpenMode.ForRead, false);
                    retResult = table.Has(layerName);
                    layerId = table[layerName];
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Is Paper Space active in the given database?
        /// </summary>
        /// <param name="m_dwgDatabase">Specific database to use</param>
        /// <returns></returns>
        public bool IsPaperSpace()
        {
            if (this.DwgDatabase.TileMode)
                return false;

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            if (this.DwgDatabase.PaperSpaceVportId == ed.CurrentViewportObjectId)
                return true;

            return false;
        }

        /// <summary>
        /// Check the view with given name exists or not
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public bool IsViewExists(string viewName, out ObjectId viewId)
        {
            bool retResult = true;
            viewId = ObjectId.Null;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.ViewTableId, OpenMode.ForRead, false);
                    retResult = table.Has(viewName);
                    viewId = table[viewName];
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        public bool IsVisible(ObjectId id)
        {
            bool retResult = false;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForRead, true);
                retResult = entity.Visible;
                transaction.Commit();
            }

            return retResult;
        }

        public bool LoadEntityData<T>(ObjectId entityID, string strApplicationDictKey, out T retObject) where T : new()
        {
            retObject = new T();

            string arrData = this.LoadEntityCustomData(entityID, strApplicationDictKey);

            if (arrData == null || arrData.Length == 0)
            {
                return false;
            }

            try
            {
                retObject = Serialization.BinaryDeSerialize<T>(arrData);

                if (retObject is IObjectInformation)
                {
                    (retObject as IObjectInformation).ObjectHandle = entityID.Handle.Value;
                }
            }
            catch
            {

                return false;
            }

            return true;
        }

        public bool LoadEntityData<T>(ObjectId entityID, string strApplicationDictKey, List<string> allowedObjects, out T retObject) where T : new()
        {
            retObject = new T();

            string arrData = this.LoadEntityCustomData(entityID, strApplicationDictKey);

            if (arrData == null || arrData.Length == 0)
            {
                return false;
            }

            try
            {
                retObject = Serialization.BinaryDeSerialize<T>(arrData);

                if (retObject is IObjectInformation)
                {
                    (retObject as IObjectInformation).ObjectHandle = entityID.Handle.Value;
                }
            }
            catch
            {

                return false;
            }

            return true;
        }

        public bool LoadEntityData(ObjectId entityID, string strApplicationDictKey, out object attClass)
        {
            attClass = new object();

            string arrData = this.LoadEntityCustomData(entityID, strApplicationDictKey);

            if (arrData == null || arrData.Length == 0)
            {
                //CADLogger.log_info(entityID.Handle.ToString() + " : No data attached");
                return false;
            }

            try
            {
                attClass = Serialization.BinaryDeSerialize(arrData);

                if (attClass is IObjectInformation)
                {
                    (attClass as IObjectInformation).ObjectHandle = entityID.Handle.Value;
                }
            }
            catch
            {

                return false;
            }

            return true;
        }

        public ObjectId Mirror(Entity ent, Point3d mirrorPoint1, Point3d mirrorPoint2, bool eraseSourceObject)
        {
            return this.Mirror(ent.ObjectId, mirrorPoint1, mirrorPoint2, eraseSourceObject);
        }

        public ObjectId Mirror(ObjectId idSource, Point3d mirrorPoint1, Point3d mirrorPoint2, bool eraseSourceObject)
        {
            Line3d line = new Line3d(mirrorPoint1, mirrorPoint2);
            Matrix3d transform = Matrix3d.Mirroring(line);
            ObjectId id = this.Copy(idSource);
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                ((Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true)).TransformBy(transform);
                transaction.Commit();
            }
            if (eraseSourceObject)
            {
                this.Erase(idSource);
            }
            return id;
        }

        public void Move(Entity ent, Point3d fromPoint, Point3d toPoint)
        {
            this.Move(ent.ObjectId, fromPoint, toPoint);
        }

        public void Move(ObjectId id, Point3d fromPoint, Point3d toPoint)
        {
            Matrix3d transform = Matrix3d.Displacement(toPoint.GetVectorTo(fromPoint));
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                ((Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true)).TransformBy(transform);
                transaction.Commit();
            }
        }

        public int PlotDrawing(string devname, string medname, List<string> layoutNameList, string strOutputFileName)
        {
            int retResult = 0;

            Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);
                PlotInfo pi = new PlotInfo();
                PlotInfoValidator piv = new PlotInfoValidator();

                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;

                // A PlotEngine does the actual plotting
                // (can also create one for Preview)

                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe = PlotFactory.CreatePublishEngine();

                    using (pe)
                    {
                        // Collect all the paperspace layouts
                        // for plotting

                        ObjectIdCollection layoutsToPlot = new ObjectIdCollection();
                        foreach (ObjectId btrId in bt)
                        {
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                            if (btr.IsLayout && btr.Name.ToUpper() != BlockTableRecord.ModelSpace.ToUpper())
                            {
                                layoutsToPlot.Add(btrId);
                            }
                        }

                        // Create a Progress Dialog to provide info
                        // and allow thej user to cancel

                        PlotProgressDialog ppd = new PlotProgressDialog(false, layoutsToPlot.Count, true);

                        using (ppd)
                        {
                            int numSheet = 1;
                            foreach (ObjectId btrId in layoutsToPlot)
                            {
                                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                                Layout lo = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);

                                //Plot only selected layout only
                                if (!layoutNameList.Contains(lo.LayoutName))
                                {
                                    continue;
                                }

                                // We need a PlotSettings object
                                // based on the layout settings
                                // which we then customize

                                PlotSettings ps = new PlotSettings(lo.ModelType);
                                ps.CopyFrom(lo);

                                // The PlotSettingsValidator helps
                                // create a valid PlotSettings object

                                PlotSettingsValidator psv = PlotSettingsValidator.Current;

                                // We'll plot the extents, centered and 
                                // scaled to fit

                                psv.SetPlotType(ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);
                                psv.SetUseStandardScale(ps, true);
                                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                                psv.SetPlotCentered(ps, true);

                                // We'll use the standard DWFx PC3, as
                                // this supports multiple sheets
                                psv.SetPlotConfigurationName(ps, devname, medname);

                                // We need a PlotInfo object
                                // linked to the layout
                                pi.Layout = btr.LayoutId;

                                // Make the layout we're plotting current
                                LayoutManager.Current.CurrentLayout = lo.LayoutName;

                                // We need to link the PlotInfo to the
                                // PlotSettings and then validate it
                                pi.OverrideSettings = ps;
                                piv.Validate(pi);

                                if (numSheet == 1)
                                {
                                    ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                                    ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                    ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                    ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                    ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                                    ppd.LowerPlotProgressRange = 0;
                                    ppd.UpperPlotProgressRange = 100;
                                    ppd.PlotProgressPos = 0;

                                    // Let's start the plot, at last

                                    ppd.OnBeginPlot();
                                    ppd.IsVisible = true;
                                    pe.BeginPlot(ppd, null);

                                    // We'll be plotting a single document
                                    if (devname.Contains("PDF") || devname.Contains("DWF") || devname.Contains("JPG") || devname.Contains("PNG"))
                                    {
                                        pe.BeginDocument(pi, this.DwgFileName, null, 1, true, strOutputFileName);
                                    }
                                    else
                                    {
                                        pe.BeginDocument(pi, this.DwgFileName, null, 1, false, strOutputFileName);
                                    }
                                }

                                // Which may contains multiple sheets
                                ppd.set_PlotMsgString(PlotMessageIndex.SheetName,
                                    this.DwgFileName.Substring(this.DwgFileName.LastIndexOf("\\") + 1) +
                                    " - sheet " + numSheet.ToString() + " of " + layoutNameList.Count.ToString());

                                ppd.OnBeginSheet();
                                ppd.LowerSheetProgressRange = 0;
                                ppd.UpperSheetProgressRange = 100;
                                ppd.SheetProgressPos = 0;

                                PlotPageInfo ppi = new PlotPageInfo();

                                pe.BeginPage(ppi, pi, (numSheet == layoutsToPlot.Count), null);
                                pe.BeginGenerateGraphics(null);
                                ppd.SheetProgressPos = 50;
                                pe.EndGenerateGraphics(null);

                                // Finish the sheet
                                pe.EndPage(null);
                                ppd.SheetProgressPos = 100;
                                ppd.OnEndSheet();
                                numSheet++;
                                ppd.PlotProgressPos += (100 / layoutsToPlot.Count);
                            }

                            // Finish the document
                            pe.EndDocument(null);

                            // And finish the plot
                            ppd.PlotProgressPos = 100;
                            ppd.OnEndPlot();
                            pe.EndPlot(null);
                        }
                    }
                }
                else
                {
                    retResult = 1;
                }
            }

            return retResult;
        }

        /// <summary>
        /// Purge block
        /// </summary>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public bool PurgeBlock(string blockName)
        {
            ObjectId blkSymbolId = this.GetSymbolRecordId(blockName, this.DwgDatabase.BlockTableId);

            ObjectIdCollection ids = new ObjectIdCollection();
            ids.Add(blkSymbolId);
            if (ObjectId.Null != blkSymbolId)
            {
                this.DwgDatabase.Purge(ids);

                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    foreach (ObjectId id in ids)
                    {
                        DBObject obj = transaction.GetObject(id, OpenMode.ForWrite);
                        obj.Erase();
                    }

                    transaction.Commit();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Purge layers
        /// </summary>
        /// <param name="layerList"></param>
        public void PurgeLayers(List<string> layerList)
        {
            ObjectIdCollection layerIdList = new ObjectIdCollection();

            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                //extract layer object ids here
                LayerTable laTable = transactionManager.GetObject(this.DwgDatabase.LayerTableId, OpenMode.ForRead, false) as LayerTable;

                //set the layer ZERO("0") to be current
                this.DwgDatabase.Clayer = laTable["0"];

                foreach (string laName in layerList)
                {
                    if (false == laTable.Has(laName)) continue;

                    layerIdList.Add(laTable[laName]);
                }

                //delete all the objects on these layers
                BlockTable btr = transactionManager.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                using (btr)
                {
                    BlockTableRecord btrMSpace = transactionManager.GetObject(btr[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    using (btrMSpace)
                    {
                        foreach (ObjectId objId in btrMSpace)
                        {
                            Entity acEntity = transactionManager.GetObject(objId, OpenMode.ForWrite, true) as Entity;
                            if (null == acEntity) continue;
                            if (false == layerIdList.Contains(acEntity.LayerId)) continue;

                            //erase the entity here
                            acEntity.Erase();
                        }
                    }
                }

                //now purge the layers here
                this.DwgDatabase.Purge(layerIdList);

                //now we need to delete these objects
                foreach (ObjectId layId in layerIdList)
                {
                    try
                    {
                        DBObject acObj = transactionManager.GetObject(layId, OpenMode.ForWrite, true);
                        acObj.Erase();
                    }
                    catch
                    {
                    }
                }

                //comit the changes
                transaction.Commit();
            }
        }

        public void PutColor(Entity ent, Color color)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.Color = color;
            }
            else
            {
                this.PutColor(ent.ObjectId, color);
            }
        }

        public void PutColor(ObjectId id, Color color)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.Color = color;
                transaction.Commit();
            }
        }

        public void PutColorIndex(Entity ent, int colorIndex)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex);
            }
            else
            {
                this.PutColorIndex(ent.ObjectId, colorIndex);
            }
        }

        public void PutColorIndex(ObjectId id, int colorIndex)
        {
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, false);
                    entity.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex);
                    transaction.Commit();
                }
            }
            catch
            {
            }
        }

        public void PutLayer(Entity ent, string layer)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.Layer = layer;
            }
            else
            {
                this.PutLayer(ent.ObjectId, layer);
            }
        }

        public void PutLayer(ObjectId id, string layer)
        {
            ObjectId layerId = ObjectId.Null;
            if (false == this.IsLayerExists(layer, out layerId))
            {
                //layer does not exits, so default to zero
                layer = "0";
            }

            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.Layer = layer;
                transaction.Commit();
            }
        }

        public void PutLineWeight(Entity ent, LineWeight lineWeight)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.LineWeight = lineWeight;
            }
            else
            {
                this.PutLineWeight(ent.ObjectId, lineWeight);
            }
        }

        public void PutLineWeight(ObjectId id, LineWeight lineWeight)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.LineWeight = lineWeight;
                transaction.Commit();
            }
        }

        public void PutLinetype(Entity ent, string linetype)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.Linetype = linetype;
            }
            else
            {
                this.PutLinetype(ent.ObjectId, linetype);
            }
        }

        public void PutLinetype(ObjectId id, string linetype)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.Linetype = linetype;
                transaction.Commit();
            }
        }

        public void PutLinetypeScale(Entity ent, double linetypeScale)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.LinetypeScale = linetypeScale;
            }
            else
            {
                this.PutLinetypeScale(ent.ObjectId, linetypeScale);
            }
        }

        public void PutLinetypeScale(ObjectId id, double linetypeScale)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.LinetypeScale = linetypeScale;
                transaction.Commit();
            }
        }

        public void PutPlotStyleName(Entity ent, string plotStyleName)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.PlotStyleName = plotStyleName;
            }
            else
            {
                this.PutPlotStyleName(ent.ObjectId, plotStyleName);
            }
        }

        public void PutPlotStyleName(ObjectId id, string plotStyleName)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.PlotStyleName = plotStyleName;
                transaction.Commit();
            }
        }

        public void PutVisible(Entity ent, bool visible)
        {
            if (ent.ObjectId == ObjectId.Null)
            {
                ent.Visible = visible;
            }
            else
            {
                this.PutVisible(ent.ObjectId, visible);
            }
        }

        public void PutVisible(ObjectId id, bool visible)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                entity.Visible = visible;
                transaction.Commit();
            }
        }

        public void PutVisibleGroupByEntityId(ObjectId entityId, bool state)
        {
            //if entity is already visible then return
            if (this.IsVisible(entityId) == true && state == true) return;

            //if entity is already invisible then return
            if (this.IsVisible(entityId) == false && state == false) return;

            List<Group> grpList = this.GetGroupsFromEntity(entityId);
            if (grpList.Count == 0)
            {
                this.PutVisible(entityId, state);
                return;
            }

            foreach (Group grp in grpList)
            {
                ObjectId[] ids = grp.GetAllEntityIds();
                foreach (ObjectId entId in ids)
                {
                    this.PutVisible(entId, state);
                }
            }
        }

        public void PutWidth(ObjectId id, double width)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Polyline plineEntity = new Polyline();
                Entity entity = (Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true);
                if (!(entity is Polyline)) return;
                plineEntity = entity as Polyline;
                plineEntity.ConstantWidth = width;
                transaction.Commit();
            }
        }

        public void RemoveHyperLink(ObjectId objId)
        {
            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = (Entity)trans.GetObject(objId, OpenMode.ForWrite);

                        if (ent.Hyperlinks != null && ent.Hyperlinks.Count > 0)
                        {
                            ent.Hyperlinks.RemoveAt(0);
                        }

                        trans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {

                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// rename layout here
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameDrawingLayout(string oldName, string newName)
        {
            bool retResult = true;
            try
            {
                HostApplicationServices.WorkingDatabase = this.DwgDatabase;
                LayoutManager.Current.RenameLayout(oldName, newName);
            }
            catch
            {
                retResult = false;
            }
            finally
            {
                HostApplicationServices.WorkingDatabase = CadApplication.CurrentDocument.Database;
            }

            return retResult;
        }

        /// <summary>
        /// Rename view name
        /// </summary>
        /// <param name="viewOldName"></param>
        /// <param name="viewNewName"></param>
        /// <returns></returns>
        public bool RenameView(string viewOldName, string viewNewName)
        {
            bool retResult = false;
            try
            {
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.ViewTableId, OpenMode.ForWrite, false);
                    if (table.Has(viewOldName) == true)
                    {
                        //erase record here
                        ViewTableRecord vtr = transactionManager.GetObject(table[viewOldName], OpenMode.ForWrite, true) as ViewTableRecord;
                        vtr.Name = viewNewName;
                        retResult = true;
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception e)
            {
                retResult = false;
            }

            return retResult;
        }

        public bool ReveresPolyline(ObjectId polylineIDToReveres)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                try
                {
                    Entity entity = (Entity)transactionManager.GetObject(polylineIDToReveres, OpenMode.ForWrite, true);

                    //if entity is not a polyline return false
                    if (!(entity is Polyline)) return false;

                    //Assign polyline class to entity
                    Polyline selectedPolylineObj = entity as Polyline;

                    //create new polyline entity here and add reveres vertex points to it
                    Polyline newPolyline = new Polyline();

                    //reveres vertex point and asign to newly created polyline
                    int j = 0;
                    for (int k = selectedPolylineObj.NumberOfVertices; k > 0; --k)
                    {
                        int minK = k - 2;
                        if (k < 3)
                        {
                            minK = 0;
                        }
                        double blg = 0;

                        blg = selectedPolylineObj.GetBulgeAt(minK);
                        if (blg < 0)
                        {
                            blg = Math.Abs(blg);
                        }
                        else
                        {
                            blg = System.Convert.ToDouble("-" + blg);
                        }
                        newPolyline.AddVertexAt(j, selectedPolylineObj.GetPoint2dAt(k - 1),
                            blg, selectedPolylineObj.GetStartWidthAt(k - 1),
                            selectedPolylineObj.GetEndWidthAt(k - 1));

                        ++j;
                    }

                    //asign newpolylin vertex point (reveres points) to old selected polyline
                    for (int k = 0; k < newPolyline.NumberOfVertices; ++k)
                    {
                        selectedPolylineObj.SetBulgeAt(k, newPolyline.GetBulgeAt(k));
                        selectedPolylineObj.SetPointAt(k, newPolyline.GetPoint2dAt(k));
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    transaction.Commit();
                }
            }

            return true;
        }

        public void Rotate(Entity ent, Point3d basePoint, double rotationAngle)
        {
            this.Rotate(ent.ObjectId, basePoint, rotationAngle);
        }

        public void Rotate(ObjectId id, Point3d basePoint, double rotationAngle)
        {
            Matrix3d transform = Matrix3d.Rotation((rotationAngle * 3.1415926535897931) / 180, new Vector3d(0, 0, 1), basePoint);
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                ((Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true)).TransformBy(transform);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Attach data to drawing database
        /// </summary>
        /// <param name="binData"></param>
        /// <param name="strApplicationDictKey"></param>
        /// <param name="strNodeKey"></param>
        /// <param name="strSubNodeKey"></param>
        /// <returns></returns>
        public bool SaveDrawingData(string binData, string strApplicationDictKey, string strNodeKey, string strSubNodeKey)
        {
            bool bRet = true;

            Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction();

            try
            {
                //First, get the NOD...
                DBDictionary NOD = (DBDictionary)trans.GetObject(this.DwgDatabase.NamedObjectsDictionaryId, OpenMode.ForWrite);
                //Define a model application level dictionary
                DBDictionary modelAppDict;
                try
                {
                    //Just throw if it doesn't exist...do nothing else
                    modelAppDict = (DBDictionary)trans.GetObject(NOD.GetAt(strApplicationDictKey), OpenMode.ForRead);
                }
                catch
                {
                    //Doesn't exist, so create one, and set it in the NOD…
                    modelAppDict = new DBDictionary();
                    NOD.SetAt(strApplicationDictKey, modelAppDict);
                    trans.AddNewlyCreatedDBObject(modelAppDict, true);
                }

                //Now get the application node we want from acmeDict
                DBDictionary modelNodeDict;
                try
                {
                    modelNodeDict = (DBDictionary)trans.GetObject(modelAppDict.GetAt(strNodeKey), OpenMode.ForWrite);
                }
                catch
                {
                    modelNodeDict = new DBDictionary();
                    //Division doesn't exist, create one
                    modelAppDict.UpgradeOpen();
                    modelAppDict.SetAt(strNodeKey, modelNodeDict);
                    trans.AddNewlyCreatedDBObject(modelNodeDict, true);
                }

                //Now save the binary data here
                //We'll do this with another XRecord.
                Xrecord binDataXRec = null;
                try
                {
                    modelNodeDict.Remove(strSubNodeKey);
                }
                catch
                {
                }

                binDataXRec = new Xrecord();
                binDataXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, binData));
                modelNodeDict.SetAt(strSubNodeKey, binDataXRec);
                trans.AddNewlyCreatedDBObject(binDataXRec, true);

                trans.Commit();
            }
            finally
            {
                trans.Dispose();
            }

            return bRet;
        }

        public bool SaveEntityData<T>(ObjectId entityID, T infoClass, string strApplicationDictKey)
        {
            string arrData = Serialization.BinarySerialize<T>(infoClass);

            this.SaveEntityCustomData(entityID, arrData, strApplicationDictKey);

            return true;
        }

        public void Scale(Entity ent, Point3d basePoint, double scaleFactor)
        {
            this.Scale(ent.ObjectId, basePoint, scaleFactor);
        }

        public void Scale(ObjectId id, Point3d basePoint, double scaleFactor)
        {
            Matrix3d transform = Matrix3d.Scaling(scaleFactor, basePoint);
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                ((Entity)transactionManager.GetObject(id, OpenMode.ForWrite, true)).TransformBy(transform);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Scale detail cut block to match with target eleavtion ltscale. This routine is used while inserting smart block
        /// </summary>
        /// <param name="strBlockName"></param>
        /// <param name="blkScale"></param>
        public void ScaleDetailCutBlocks(string strBlockName, double blkScale)
        {
            try
            {
                Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt = (BlockTable)tr.GetObject(this.DwgDatabase.BlockTableId, OpenMode.ForRead);

                    using (bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                        using (btr)
                        {
                            foreach (ObjectId blkInstanceId in btr)
                            {
                                Entity ent = (Entity)tr.GetObject(blkInstanceId, OpenMode.ForRead);
                                if (ent is BlockReference)
                                {
                                    BlockReference blkRef = ent as BlockReference;

                                    BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForRead);

                                    if (blkObj.Name.ToUpper() == strBlockName.ToUpper().Replace(".DWG", ""))
                                    {
                                        //now find the intersection block and scale the block accordingly
                                        Point3d basePoint = blkRef.Position;

                                        //get group from the id
                                        List<Group> lstGroups = this.GetGroupsFromEntity(ent.ObjectId);
                                        if (lstGroups.Count != 1)
                                        {
                                            continue;
                                        }

                                        //get current scale here
                                        double currentScale = blkRef.ScaleFactors.X;

                                        Group entGroup = lstGroups[0];
                                        ObjectId[] entIds = entGroup.GetAllEntityIds();
                                        foreach (ObjectId entId in entIds)
                                        {
                                            Entity objEnt = this.GetEntity(entId);
                                            if (objEnt is Polyline)
                                            {
                                                //find out intersections of this entity with other entity and return a list of the same.
                                                //most of the time there will be one intersection only.

                                                ObjectIdCollection lstIds = this.GetAllEntityIds();
                                                foreach (ObjectId entId1 in lstIds)
                                                {
                                                    if (entId == entId1) continue;

                                                    Entity objEnt1 = this.GetEntity(entId1);

                                                    if (objEnt1 is BlockReference || objEnt1 is MText || objEnt1 is DBText) continue;

                                                    Point3dCollection pts = new Point3dCollection();
                                                    objEnt.IntersectWith(objEnt1, Intersect.OnBothOperands, pts, new IntPtr(1), new IntPtr(1));

                                                    if (pts != null && pts.Count > 0)
                                                    {
                                                        basePoint = pts[0];
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        //scale block reference and polyline here
                                        foreach (ObjectId entId in entIds)
                                        {
                                            double targetScale = blkScale / currentScale;
                                            this.Scale(entId, basePoint, targetScale);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //commit trasaction here
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// set current dimension style
        /// </summary>
        /// <param name="strStyleName"></param>
        /// <returns></returns>
        public bool SetCurrentDimensionStyle(string strStyleName)
        {
            ObjectId dimStyleId = this.GetSymbolRecordId(strStyleName, this.DwgDatabase.DimStyleTableId);

            if (ObjectId.Null != dimStyleId)
            {
                this.DwgDatabase.Dimstyle = dimStyleId;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set current layer
        /// </summary>
        /// <param name="layer"></param>
        public void SetCurrentLayer(string layer)
        {
            try
            {
                ObjectId layerId = this.GetSymbolRecordId(layer, this.DwgDatabase.LayerTableId);
                if (layerId != ObjectId.Null)
                {
                    this.DwgDatabase.Clayer = layerId;
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// set dim scale of the given style
        /// </summary>
        /// <param name="styleName"></param>
        /// <param name="scl"></param>
        /// <returns></returns>
        public bool SetDimScale(string styleName, double scl)
        {
            bool retResult = false;

            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)transactionManager.GetObject(this.DwgDatabase.DimStyleTableId, OpenMode.ForRead, false);
                if (table.Has(styleName))
                {
                    ObjectId styleId = table[styleName];

                    DimStyleTableRecord dimStyRec = (DimStyleTableRecord)transactionManager.GetObject(styleId, OpenMode.ForWrite, false);

                    dimStyRec.Dimscale = scl;

                    transaction.Commit();
                    retResult = true;
                }
            }

            return retResult;
        }

        public void SetDrawingName(string strFileName)
        {
            this.DwgFileName = strFileName;
        }

        public bool SetDrawingProperty(string dictName, string strPropertyName, string strPropertyValue)
        {
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                //Define an application level dictionary
                DBDictionary pdmsDict = null;
                //First, get the NOD...
                DBDictionary NOD = (DBDictionary)trans.GetObject(this.DwgDatabase.NamedObjectsDictionaryId, OpenMode.ForWrite, false);
                try
                {
                    //if it	exists,	just get it
                    pdmsDict = (DBDictionary)trans.GetObject(NOD.GetAt(dictName), OpenMode.ForWrite);
                }
                catch
                {
                    //Doesn//t exist, so create	one
                    NOD.UpgradeOpen();
                    pdmsDict = new DBDictionary();
                    NOD.SetAt(dictName, pdmsDict);
                    trans.AddNewlyCreatedDBObject(pdmsDict, true);
                }

                //store the property value in x record
                Xrecord propXRec;
                try
                {
                    propXRec = (Xrecord)trans.GetObject(pdmsDict.GetAt(strPropertyName), OpenMode.ForWrite);
                    //The first field is value of the property, but we can have more values like, description, default value etc
                    propXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, strPropertyValue));
                }
                catch
                {
                    propXRec = new Xrecord();
                    propXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, strPropertyValue));
                    pdmsDict.SetAt(strPropertyName, propXRec);
                    trans.AddNewlyCreatedDBObject(propXRec, true);
                }

                trans.Commit();
            }

            return true;
        }

        public bool SetEntityProperty(ObjectId id, string strPropertyName, string strPropertyValue)
        {
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                DBObject ent = (DBObject)trans.GetObject(id, OpenMode.ForWrite, true);
                DBDictionary entExtDict = null;
                try
                {
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite, false);
                }
                catch
                {
                    ent.CreateExtensionDictionary();
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite, false);
                }

                //store the property value in x record
                Xrecord propXRec;
                try
                {
                    propXRec = (Xrecord)trans.GetObject(entExtDict.GetAt(strPropertyName), OpenMode.ForWrite);
                    //The first field is value of the property, but we can have more values like, description, default value etc
                    propXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, strPropertyValue));
                }
                catch
                {
                    propXRec = new Xrecord();
                    //The first field is value of the property, but we can have more values like, description, default value etc
                    propXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, strPropertyValue));
                    entExtDict.SetAt(strPropertyName, propXRec);
                    trans.AddNewlyCreatedDBObject(propXRec, true);
                }

                trans.Commit();

                return true;
            }
        }

        /// <summary>
        /// Set LTSCALE from other drawing file
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public bool SetLTScaleFromDrawingFile(string strFileName)
        {
            bool retResult = true;
            try
            {
                Database db = new Database(false, true);
                db.ReadDwgFile(strFileName, System.IO.FileShare.ReadWrite, true, "");

                this.DwgDatabase.Ltscale = db.Ltscale;

                db.CloseInput(true);
                db.Dispose();
                db = null;
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        public void SetLtscale(double ltscaleValue)
        {
            this.DwgDatabase.Ltscale = ltscaleValue;
        }

        public void SetPaperSpaceMode()
        {
            if (this.DwgDatabase.TileMode)
            {
                this.DwgDatabase.TileMode = false;
                return;
            }
        }

        public bool SetVarialbleValues(string variable, double dwgValue)
        {
            bool retResult = false;
            string inputVariable = variable.ToUpper();
            try
            {
                switch (inputVariable)
                {
                    case "LTSCALE":
                        this.DwgDatabase.Ltscale = dwgValue;
                        break;
                    case "DIMSCALE":
                        this.DwgDatabase.Dimscale = dwgValue;
                        break;
                    case "CELTSCALE":
                        this.DwgDatabase.Celtscale = dwgValue;
                        break;
                    default:
                        this.DwgDatabase.Ltscale = dwgValue;
                        break;
                }
                retResult = true;
            }
            catch (Exception ex)
            {
            }
            return retResult;
        }

        /// <summary>
        /// set visual style name
        /// </summary>
        /// <param name="strVisualStyleName"></param>
        /// <returns></returns>
        public bool SetVisualStyle(string strVisualStyleName)
        {
            bool retResult = true;

            try
            {
                ViewTableRecord vtr = CadApplication.CurrentEditor.GetCurrentView();
                ObjectId visualStyleId = this.GetVisualStyleId(strVisualStyleName);
                if (visualStyleId != ObjectId.Null)
                {
                    vtr.VisualStyleId = visualStyleId;
                    CadApplication.CurrentEditor.SetCurrentView(vtr);
                }
                else
                {
                    retResult = false;
                }
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        public bool SetXData(ObjectId objId, ResultBuffer rb)
        {
            bool retResult = true;
            try
            {
                using (Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    DBObject obj = tr.GetObject(objId, OpenMode.ForWrite, false);
                    obj.XData = rb;
                    tr.Commit();
                }
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Transform an Entity from UCS to WCS
        /// </summary>
        /// <param name="ent">Entity to transform</param>
        /// <param name="m_dwgDatabase">Database the entity belongs to (or will belong to)</param>

        public void TransformToWcs(Entity ent)
        {
            Debug.Assert(ent != null, "Entity can not be null");
            Debug.Assert(this.DwgDatabase != null, "AutoCAD Database can not be null");
            Debug.Assert(ent.IsWriteEnabled, "Entity is readonly");

            Matrix3d m = this.GetUcsMatrix();
            ent.TransformBy(m);
        }

        /// <summary>
        /// Transform a collection of Entities from UCS to WCS
        /// </summary>
        /// <param name="ents">Entities to transform</param>
        /// <param name="m_dwgDatabase">Database the entities belong to (or will belong to)</param>

        public void TransformToWcs(DBObjectCollection ents)
        {
            Debug.Assert(ents != null, "Entity can not be null");
            Debug.Assert(this.DwgDatabase != null, "AutoCAD Current database can not be null");

            Matrix3d m = this.GetUcsMatrix();

            foreach (Entity tmpEnt in ents)
            {
                Debug.Assert(tmpEnt.IsWriteEnabled, "Entity is readonly");
                tmpEnt.TransformBy(m);
            }
        }

        /// <summary>
        /// Get a transformed copy of a point from UCS to WCS
        /// </summary>
        /// <param name="pt">Point to transform</param>
        /// <returns>Transformed copy of point</returns>

        public Point3d UcsToWcs(Point3d pt)
        {
            Matrix3d m = this.GetUcsMatrix();

            return pt.TransformBy(m);
        }

        /// <summary>
        /// Get a transformed copy of a vector from UCS to WCS
        /// </summary>
        /// <param name="vec">Vector to transform</param>
        /// <returns>Transformed copy of vector</returns>

        public Vector3d UcsToWcs(Vector3d vec)
        {
            Matrix3d m = this.GetUcsMatrix();

            return vec.TransformBy(m);
        }

        public bool UpdateBlockAttributeValues(ObjectId blkId, Dictionary<string, string> attValueDictionary)
        {
            bool retResult = false;
            try
            {
                using (Transaction tr = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    using (BlockReference blkRef = tr.GetObject(blkId, OpenMode.ForWrite) as BlockReference)
                    {
                        using (BlockTableRecord blkObj = (BlockTableRecord)tr.GetObject(blkRef.BlockTableRecord, OpenMode.ForWrite))
                            if (blkObj.HasAttributeDefinitions)
                            {
                                foreach (ObjectId attId in blkRef.AttributeCollection)
                                {
                                    AttributeReference attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForWrite);
                                    if (attValueDictionary.ContainsKey(attRef.Tag.ToUpper()))
                                    {
                                        attRef.UpgradeOpen();
                                        attRef.TextString = attValueDictionary[attRef.Tag.ToUpper()];
                                        // Begin alignment code
                                        Database wdb = HostApplicationServices.WorkingDatabase;
                                        HostApplicationServices.WorkingDatabase = this.DwgDatabase;
                                        attRef.AdjustAlignment(this.DwgDatabase);
                                        HostApplicationServices.WorkingDatabase = wdb;
                                        // End alignment code
                                        attRef.DowngradeOpen();
                                    }
                                }
                            }
                    }
                    tr.Commit();
                }

                retResult = true;
            }
            catch (Exception ex)
            {
                retResult = false;
            }

            return retResult;
        }

        public void UpdateHyperLink(ObjectId objId, string hyperLinkText)
        {
            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = (Entity)trans.GetObject(objId, OpenMode.ForWrite);

                        HyperLink hpLink = new HyperLink();
                        hpLink.Name = "PDMSHPLink";
                        hpLink.Description = hyperLinkText;
                        if (ent.Hyperlinks.Contains(hpLink))
                        {
                            ent.Hyperlinks.RemoveAt(0);
                        }
                        ent.Hyperlinks.Insert(0, hpLink);

                        trans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public void UpdateHyperLink(ObjectId objId, string hyperLinkText, string hyperLinkName)
        {
            try
            {
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity ent = (Entity)trans.GetObject(objId, OpenMode.ForWrite);

                        HyperLink hpLink = new HyperLink();
                        hpLink.Name = hyperLinkName;
                        hpLink.Description = hyperLinkText;
                        if (ent.Hyperlinks.Contains(hpLink))
                        {
                            ent.Hyperlinks.RemoveAt(0);
                        }
                        ent.Hyperlinks.Insert(0, hpLink);

                        trans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {

                    }
                }
            }
            catch
            {
            }
        }

        public void UpdateLinePoint(ObjectId entId, Point3d intPt, int starOrEndPoint)
        {
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                Line entity = transactionManager.GetObject(entId, OpenMode.ForWrite, true) as Line;
                if (starOrEndPoint == 0)
                    entity.StartPoint = intPt;
                else
                    entity.EndPoint = intPt;
                transaction.Commit();
            }
        }

        /// <summary>
        /// Get a transformed copy of a point from WCS to UCS
        /// </summary>
        /// <param name="pt">Point to transform</param>
        /// <returns>Transformed copy of point</returns>

        public Point3d WcsToUcs(Point3d pt)
        {
            Matrix3d m = this.GetUcsMatrix();

            return pt.TransformBy(m.Inverse());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add object to dictionary like groups etc
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="newValue"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        private ObjectId AddDictionaryObject(string searchKey, DBObject newValue, ObjectId ownerId)
        {
            ObjectId retID = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager manager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = manager.StartTransaction())
            {
                DBDictionary dictionary = (DBDictionary)manager.GetObject(ownerId, OpenMode.ForWrite, false);
                if (!dictionary.Contains(searchKey))
                {
                    dictionary.SetAt(searchKey, newValue);
                    manager.AddNewlyCreatedDBObject(newValue, true);
                    retID = newValue.ObjectId;
                }
                else
                {
                    retID = dictionary.GetAt(searchKey);
                }
                transaction.Commit();
            }
            return retID;
        }

        /// <summary>
        /// Get Dictionary object like group
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        private ObjectId GetDictionaryObject(string searchKey, ObjectId ownerId)
        {
            ObjectId retID = ObjectId.Null;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager manager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = manager.StartTransaction())
            {
                DBDictionary dictionary = (DBDictionary)manager.GetObject(ownerId, OpenMode.ForWrite, false);
                if (dictionary.Contains(searchKey))
                {
                    retID = dictionary.GetAt(searchKey);
                }

                transaction.Commit();
            }
            return retID;
        }

        private List<string> GetSymbolNameList(ObjectId symbolTableId)
        {
            List<string> lstStyles = new List<string>();
            Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = this.DwgDatabase.TransactionManager;
            using (Transaction transaction = transactionManager.StartTransaction())
            {
                SymbolTable table = (SymbolTable)transactionManager.GetObject(symbolTableId, OpenMode.ForRead, false);
                foreach (ObjectId symId in table)
                {
                    SymbolTableRecord symRec = (SymbolTableRecord)transactionManager.GetObject(symId, OpenMode.ForRead, false);
                    lstStyles.Add(symRec.Name);
                }
            }

            return lstStyles;
        }

        /// <summary>
        /// Get visual style by specifying style name
        /// </summary>
        /// <param name="strVisualStyleName"></param>
        /// <returns></returns>
        private ObjectId GetVisualStyleId(string strVisualStyleName)
        {
            ObjectId visualStyleId = ObjectId.Null;
            try
            {
                DBDictionary dict = this.GetObject(this.DwgDatabase.VisualStyleDictionaryId) as DBDictionary;
                visualStyleId = dict.GetAt(strVisualStyleName);
            }
            catch (Exception ex)
            {
            }

            return visualStyleId;
        }

        /// <summary>
        /// Check ObjectId class for ObjectClass property
        /// </summary>
        /// <returns></returns>
        private bool HasObjectClassProperty()
        {
            bool retResult = false;

            PropertyInfo[] props = typeof(Autodesk.AutoCAD.DatabaseServices.ObjectId).GetProperties();
            foreach (PropertyInfo pi in props)
            {
                if (pi.Name == "ObjectClass")
                {
                    retResult = true;
                    break;
                }
            }

            return retResult;
        }

        private bool IsValidObjectClass(ObjectId entityID, List<string> objectClassList)
        {
            bool retResult = true;

            PropertyInfo[] props = entityID.GetType().GetProperties();
            foreach (PropertyInfo pi in props)
            {
                if (pi.Name == "ObjectClass")
                {
                    object ocObj = pi.GetValue(entityID, null);
                    PropertyInfo[] rXprops = ocObj.GetType().GetProperties();

                    foreach (PropertyInfo rXpi in rXprops)
                    {
                        if (rXpi.Name == "DxfName")
                        {
                            //string entClass = entityID.ObjectClass.DxfName;
                            string entClass = rXpi.GetValue(ocObj, null).ToString();

                            retResult = objectClassList.Contains(entClass);
                            break;
                        }
                    }

                    break;
                }
            }

            return retResult;
        }

        private string LoadEntityCustomData(ObjectId entityID, string strApplicationDictKey)
        {
            string retArr = null;
            try
            {
                //Checking object type here will be faster, instead of opening the entity and checking the same
                if (this.HasObjectClass &&
                    !this.IsValidObjectClass(entityID, new List<string>() { "POLYLINE", "BLOCKREFERENCE", "SOLID", "REGION", "LWPOLYLINE", "INSERT", "LINE" }))
                {
                    return retArr;
                }

                DBDictionary entExtDict;
                using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
                {
                    DBObject ent = (DBObject)trans.GetObject(entityID, OpenMode.ForRead, true);

                    if (!(ent is Polyline || ent is BlockReference || ent is Solid || ent is Region || ent is Line))
                    {
                        //CADLogger.log_info(entityID.Handle.ToString() + " : Entity should be Polyline, BlockReference, Solid, Region or Line");

                        return null;
                    }

                    if (ent.ExtensionDictionary == ObjectId.Null)
                    {
                        //CADLogger.log_info(entityID.Handle.ToString() + " : Entity does not have extension dictionary");

                        return null;
                    }

                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    Xrecord propXRec = (Xrecord)trans.GetObject(entExtDict.GetAt(strApplicationDictKey), OpenMode.ForRead);

                    TypedValue resBuf = propXRec.Data.AsArray()[0];

                    retArr = resBuf.Value.ToString();

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                retArr = null;
            }

            return retArr;
        }

        /// <summary>
        /// Attach data to drawing database
        /// </summary>
        /// <param name="binData"></param>
        /// <param name="strApplicationDictKey"></param>
        /// <param name="strNodeKey"></param>
        /// <param name="strSubNodeKey"></param>
        /// <returns></returns>
        private bool SaveEntityCustomData(ObjectId entityID, string binData, string strApplicationDictKey)
        {
            using (Transaction trans = this.DwgDatabase.TransactionManager.StartTransaction())
            {
                DBObject ent = trans.GetObject(entityID, OpenMode.ForWrite, true);
                DBDictionary entExtDict = null;
                try
                {
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite, false);
                }
                catch
                {
                    ent.CreateExtensionDictionary();
                    entExtDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite, false);
                }

                //Now save the binary data here
                //We'll do this with another XRecord.
                Xrecord binDataXRec = null;
                try
                {
                    entExtDict.Remove(strApplicationDictKey);
                }
                catch
                {
                }

                binDataXRec = new Xrecord();
                binDataXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, binData));

                entExtDict.SetAt(strApplicationDictKey, binDataXRec);
                trans.AddNewlyCreatedDBObject(binDataXRec, true);

                trans.Commit();
            }
            return true;
        }

        #endregion
    }
}