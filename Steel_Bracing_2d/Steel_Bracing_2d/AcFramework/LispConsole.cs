namespace Steel_Bracing_2d.AcFramework
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Class for executing lisp commands
    /// </summary>
    public class LispConsole
    {
        #region Fields

        protected Document m_currDocument = null;

        #endregion

        #region Constructors and Destructors

        public LispConsole(Document currDocument)
        {
            if (currDocument == null) throw new Exception("AutoCAD document is null");

            this.m_currDocument = currDocument;
        }

        #endregion

        #region Public Methods and Operators

        public void ExecuteCommand(string cmdName)
        {
            if (this.m_currDocument == null) return;

            ExecuteInApplicationContextCallback cb = new ExecuteInApplicationContextCallback(this.ExecuteCommandCB);
            Application.DocumentManager.ExecuteInApplicationContext(cb, cmdName);
        }

        public void ZoomWindow(Extents3d ext)
        {
            if (this.m_currDocument == null) return;

            ExecuteInApplicationContextCallback cb = new ExecuteInApplicationContextCallback(this.ZoomWindowCB);
            Application.DocumentManager.ExecuteInApplicationContext(cb, ext);
        }

        #endregion

        #region Methods

        private void ExecuteCommandCB(object str)
        {
            try
            {
                this.m_currDocument.SendStringToExecute(str + "\n", false, false, false);
            }
            catch
            {

            }
        }

        private void ZoomWindowCB(object exts)
        {
            try
            {
                string str = "_.zoom w ";
                Extents3d ext = (Extents3d)exts;
                str += ext.MinPoint.X.ToString() + "," + ext.MinPoint.Y.ToString() + " ";
                str += ext.MaxPoint.X.ToString() + "," + ext.MaxPoint.Y.ToString() + "\n";

                //Zoom Extents
                this.m_currDocument.SendStringToExecute("_.zoom e ", false, false, false);

                //Zoom to exts
                this.m_currDocument.SendStringToExecute(str, false, false, false);
            }
            catch
            {

            }
        }

        #endregion
    }
}