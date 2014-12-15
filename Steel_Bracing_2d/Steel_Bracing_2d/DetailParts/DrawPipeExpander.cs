namespace Steel_Bracing_2d.DetailParts
{
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    using Steel_Bracing_2d.AcFramework;

    public class DrawPipeExpander
    {
        private double _offset;
        private double _expanderLength;
        private double _smallDuctDia;
        private double _largeDuctDia;
        private double _branchDuctDia;
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

        public DrawPipeExpander()
        {
        }

        public DrawPipeExpander(double off, double expLength, double smallDuct, double largeDuct, double branchDuct)
        {
            this._offset = off;
            this._expanderLength = expLength;
            this._smallDuctDia = smallDuct;
            this._largeDuctDia = largeDuct;
            this._branchDuctDia = branchDuct;
        }

        public void ExecuteCommand()
        {
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
