namespace Steel_Bracing_2d.AcFramework
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;

    public class LayerManager
    {
        #region Public Methods and Operators

        public static void CreateLayers()
        {
            using( CadApplication.CurrentDocument.LockDocument() )
            {
                try
                {
                    LoadLinetypes();
                }
                catch( Exception )
                {
                }

                DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
                currDwg.AddLayer("objects", 1, "continuous");
                currDwg.AddLayer("center", 2, "center");
                currDwg.AddLayer("hidden", 3, "hidden");
                currDwg.AddLayer("dimension", 4, "continuous");
                currDwg.AddLayer("text", 7, "continuous");
            }
        }

        public static void CreateLayers_BB()
        {
            using (DocumentLock dockLock = CadApplication.CurrentDocument.LockDocument())
            {
                try
                {
                    LoadLinetypes();
                }
                catch
                {
                }

                DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
                currDwg.AddLayer("objects", 2, "continuous");
                currDwg.AddLayer("center", 1, "center");
                currDwg.AddLayer("hidden", 6, "hidden");
                currDwg.AddLayer("dimension", 4, "continuous");
                currDwg.AddLayer("text", 7, "continuous");
            }
        }

        public static void CreateTextStyles(double dwgScale)
        {
            DrawingDatabase currDwg = new DrawingDatabase(CadApplication.CurrentDocument);
            using (DocumentLock dockLock = CadApplication.CurrentDocument.LockDocument())
            {
                TextStyleTableRecord txtSty = new TextStyleTableRecord();
                txtSty.Name = "brc25";
                txtSty.FileName = "RomanS.shx";
                txtSty.TextSize = 2.5 * dwgScale;
                txtSty.XScale = 0.9;
                ObjectId styId = currDwg.AddSymbolTableRecord(txtSty, CadApplication.CurrentDatabase.TextStyleTableId);

                ////txtSty = new TextStyleTableRecord();
                ////txtSty.Name = "s30";
                ////txtSty.FileName = "RomanD.shx";
                ////txtSty.TextSize = 3 * dwgScale;
                ////txtSty.XScale = 1.0;
                ////currDwg.AddSymbolTableRecord(txtSty, CADApplication.CurrentDatabase.TextStyleTableId);

                ////txtSty = new TextStyleTableRecord();
                ////txtSty.Name = "s35";
                ////txtSty.FileName = "RomanD.shx";
                ////txtSty.TextSize = 3.5 * dwgScale;
                ////txtSty.XScale = 1.0;
                ////currDwg.AddSymbolTableRecord(txtSty, CADApplication.CurrentDatabase.TextStyleTableId);

                ////txtSty = new TextStyleTableRecord();
                ////txtSty.Name = "s40";
                ////txtSty.FileName = "RomanD.shx";
                ////txtSty.TextSize = 4 * dwgScale;
                ////txtSty.XScale = 1.0;
                ////currDwg.AddSymbolTableRecord(txtSty, CADApplication.CurrentDatabase.TextStyleTableId);
            }
        }

        #endregion

        #region Methods

        private static void LoadLinetypes()
        {
            const string filename = "acad.lin";
            try
            {
                string path =
                  HostApplicationServices.Current.FindFile(
                    filename, CadApplication.CurrentDatabase, FindFileHint.Default
                  );
                CadApplication.CurrentDatabase.LoadLineTypeFile("*", path);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
            }
        }

        #endregion
    }
}
