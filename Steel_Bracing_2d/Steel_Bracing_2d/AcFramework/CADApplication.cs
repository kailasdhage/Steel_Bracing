namespace Steel_Bracing_2d.AcFramework
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Class for referring current top level object of the AutoCAD Application
    /// like Current Database, Current Editor, Current Document etc
    /// </summary>
    public class CadApplication
    {
        #region Public Properties

        /// <summary>
        /// Returns the instance of Current Database from AutoCAD
        /// </summary>
        public static Database CurrentDatabase
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
            }
        }

        /// <summary>
        /// Return Current document Document instance in AutoCAD
        /// </summary>
        public static Document CurrentDocument
        {
            get
            {
                if (Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.Count == 0) return null;

                return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            }
            set
            {
                if (value != null && Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument != value)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument = value;
                }
            }
        }

        /// <summary>
        /// Returns the instance of the Current AutoCAD Editor
        /// </summary>
        public static Editor CurrentEditor
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        /// <summary>
        /// Returns the no of open documents
        /// </summary>
        public static int DocumentCount
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Add blank drawing into AutoCAD
        /// .NET API do need drawing open most of the time.
        /// We are opening and closing blank drawing when required
        /// </summary>
        public static void AddBlankDocument()
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.Add("acad.dwt");
        }

        /// <summary>
        /// Returns the currently running AutoCAD Version
        /// </summary>
        /// <returns></returns>
        public static string GetAutoCadVersion()
        {
            string acadVer;
            if (Application.Version.Major == 17 && Application.Version.Minor == 2)
            {
                acadVer = "AutoCAD 2009";
            }
            else if (Application.Version.Major == 17 && Application.Version.Minor == 1)
            {
                acadVer = "AutoCAD 2008";
            }
            else if (Application.Version.Major == 17 && Application.Version.Minor == 0)
            {
                acadVer = "AutoCAD 2007";
            }
            else
            {
                acadVer = "AutoCAD 2006";
            }

            return acadVer;
        }

        /// <summary>
        /// Returns the state of AutoCAD 
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoCADReady()
        {
            return Application.IsQuiescent;
        }

        /// <summary>
        /// Checks given drawing is open inside AutoCAD editor or not
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsDrawingOpen(string fileName)
        {
            bool retResult = false;

            fileName = fileName.ToUpper();

            string[] strFileName = fileName.Split(new char[] { '\\' }, 2);

            foreach (Document doc in Application.DocumentManager)
            {
                string[] strDocName = doc.Name.Split(new char[] { '\\' }, 3, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    if (doc.Name.ToUpper() == fileName.ToUpper())
                    {
                        retResult = true;
                        break;
                    }
                    else if (strDocName[2].ToUpper() == strFileName[1].ToUpper())
                    {
                        retResult = true;
                        break;
                    }
                }
                catch
                {
                }
            }

            return retResult;
        }

        /// <summary>
        /// Check the drawing file is opened in read-only mode or not
        /// </summary>
        /// <param name="dwgFileName"></param>
        /// <returns></returns>
        public static bool OpenDrawingInReadOnlyMode(string dwgFileName)
        {
            bool retResult = true;
            try
            {
                dwgFileName = dwgFileName.ToUpper();

                //check drawing is open in AutoCAD or not
                foreach (Document doc in Application.DocumentManager)
                {
                    if (doc.Name.ToUpper() == dwgFileName)
                    {
                        if (Application.DocumentManager.MdiActiveDocument != doc)
                        {
                            Application.DocumentManager.MdiActiveDocument = doc;
                        }

                        return retResult;
                    }
                }

                Application.DocumentManager.Open(dwgFileName.ToUpper(), true);
            }
            catch
            {
                retResult = false;
            }

            return retResult;
        }

        /// <summary>
        /// Regen all drawings
        /// </summary>
        public static void RegenAll()
        {
            //regen all drawings here
            Document currDoc = CadApplication.CurrentDocument;

            if (currDoc != null)
            {
                CadApplication.CurrentEditor.Regen();
            }

            foreach (Document doc in Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager)
            {
                if (CadApplication.CurrentDocument == doc) continue;

                CadApplication.CurrentDocument = doc;
                CadApplication.CurrentEditor.Regen();
            }

            //restore current document
            if (CadApplication.CurrentDocument != currDoc)
            {
                CadApplication.CurrentDocument = currDoc;
            }
        }

        /// <summary>
        /// Com based zoom all method
        /// </summary>
        public static void ZoomAll()
        {
            if (DocumentCount == 0) return;

            //use com method to zoom extents drawing
            object acadApp = Application.AcadApplication;
            acadApp.GetType().InvokeMember("ZoomAll", System.Reflection.BindingFlags.InvokeMethod, null, acadApp, null);
        }

        /// <summary>
        /// Com based zoom extents method
        /// </summary>
        public static void ZoomExtents()
        {
            if (DocumentCount == 0) return;

            //use com method to zoom extents drawing
            object acadApp = Application.AcadApplication;
            acadApp.GetType().InvokeMember("ZoomExtents", System.Reflection.BindingFlags.InvokeMethod, null, acadApp, null);
        }

        /// <summary>
        /// Com based zoom previous method
        /// </summary>
        public static void ZoomPrevious()
        {
            if (DocumentCount == 0) return;

            //use com method to zoom extents drawing
            object acadApp = Application.AcadApplication;
            acadApp.GetType().InvokeMember("ZoomPrevious", System.Reflection.BindingFlags.InvokeMethod, null, acadApp, null);
        }

        /// <summary>
        /// Com based zoom relative method
        /// </summary>
        public static void ZoomRelativeScaled(double scaleFactor)
        {
            if (DocumentCount == 0) return;

            //use com method to zoom extents drawing
            object acadApp = Application.AcadApplication;
            acadApp.GetType().InvokeMember("ZoomScaled", System.Reflection.BindingFlags.InvokeMethod, null,
                acadApp, new object[] { scaleFactor, 1 });
        }

        /// <summary>
        /// Com based zoom window method
        /// </summary>
        public static void ZoomWindow(Point3d p1, Point3d p2)
        {
            if (DocumentCount == 0) return;

            double[] pt1 = new double[3];
            double[] pt2 = new double[3];

            pt1[0] = p1.X;
            pt1[1] = p1.Y;
            pt1[2] = p1.Z;

            pt2[0] = p2.X;
            pt2[1] = p2.Y;
            pt2[2] = p2.Z;

            //use com method to zoom extents drawing
            object acadApp = Application.AcadApplication;
            acadApp.GetType().InvokeMember("ZoomWindow", System.Reflection.BindingFlags.InvokeMethod, null, acadApp, new object[] { pt1, pt2 });
        }

        #endregion
    }
}
