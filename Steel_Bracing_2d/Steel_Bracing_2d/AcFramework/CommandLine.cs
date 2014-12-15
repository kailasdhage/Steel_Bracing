namespace Steel_Bracing_2d.AcFramework
{
    using System;

    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Class for handling interaction with command line like select object and get inputs
    /// </summary>
    public class CommandLine
    {
        #region Public Methods and Operators

        public static PromptStatus PromptYesNo(string prmpt, bool def, out bool answer)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            answer = false;

            string defStr = (def) ? "Yes" : "No";
            PromptKeywordOptions prOpts = new PromptKeywordOptions(prmpt);
            prOpts.Keywords.Add("Yes");
            prOpts.Keywords.Add("No");
            prOpts.Keywords.Default = defStr;

            PromptResult prRes = ed.GetKeywords(prOpts);
            if (prRes.Status == PromptStatus.OK)
            {
                if (prRes.StringResult == "Yes")
                    answer = true;
            }

            return prRes.Status;
        }

        /// <summary>
        /// Select all object using given filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ObjectIdCollection SelectAll(TypedValue[] filter)
        {
            SelectionFilter ssf = new SelectionFilter(filter);
            ObjectIdCollection ids = new ObjectIdCollection();
            PromptSelectionResult ssresult = CadApplication.CurrentEditor.SelectAll(ssf);

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        /// <summary>
        /// select by crossing polygon
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static ObjectIdCollection SelectCrossingPolygon(TypedValue[] filter, Point3dCollection polygon)
        {
            SelectionFilter ssf = new SelectionFilter(filter);
            ObjectIdCollection ids = new ObjectIdCollection();

            PromptSelectionResult ssresult = CadApplication.CurrentEditor.SelectCrossingPolygon(polygon, ssf);

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        public static ObjectIdCollection SelectCrossingWindow(TypedValue[] filter, Point3d pt1, Point3d pt2)
        {
            ObjectIdCollection ids = new ObjectIdCollection();

            PromptSelectionResult ssresult;
            if (filter != null)
            {
                ssresult = CadApplication.CurrentEditor.SelectCrossingWindow(pt1, pt2, new SelectionFilter(filter));
            }
            else
            {
                ssresult = CadApplication.CurrentEditor.SelectCrossingWindow(pt1, pt2);
            }

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        /// <summary>
        /// Select single entity till user cancel the action or till user selects the valid entity
        /// TODO - We also need a function that allows us to select only entity with given criterion
        /// Kailas Dhage
        /// </summary>
        /// <param name="allowedType"></param>
        /// <param name="id"></param>
        /// <param name="strPrompt"></param>
        /// <returns></returns>
        public static bool SelectEntity(string strPrompt, Type allowedType, out ObjectId id)
        {
            id = ObjectId.Null;
            PromptEntityResult perResult;
            PromptEntityOptions peo = new PromptEntityOptions(strPrompt);
            if (allowedType != null)
            {
                peo.SetRejectMessage("Not valid entity type:");
                peo.AddAllowedClass(allowedType, true);
            }

            while (true)
            {
                perResult = CadApplication.CurrentEditor.GetEntity(peo);
                if (perResult.Status == PromptStatus.Cancel)
                {
                    return false;
                }

                if (perResult.Status == PromptStatus.OK)
                {

                    id = perResult.ObjectId;

                    return true;
                }
            }
        }

        public static ObjectIdCollection SelectFence(TypedValue[] filter, Point3dCollection fence)
        {
            SelectionFilter ssf = new SelectionFilter(filter);
            ObjectIdCollection ids = new ObjectIdCollection();
            PromptSelectionResult ssresult = CadApplication.CurrentEditor.SelectFence(fence, ssf);

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        public static ObjectIdCollection SelectWindow(TypedValue[] filter, Point3d pt1, Point3d pt2)
        {
            ObjectIdCollection ids = new ObjectIdCollection();

            PromptSelectionResult ssresult;
            if (filter != null)
            {
                ssresult = CadApplication.CurrentEditor.SelectWindow(pt1, pt2, new SelectionFilter(filter));
            }
            else
            {
                ssresult = CadApplication.CurrentEditor.SelectWindow(pt1, pt2);
            }

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        public static ObjectIdCollection SelectWindowPolygon(TypedValue[] filter, Point3dCollection polygon)
        {
            SelectionFilter ssf = new SelectionFilter(filter);
            ObjectIdCollection ids = new ObjectIdCollection();
            PromptSelectionResult ssresult = CadApplication.CurrentEditor.SelectWindowPolygon(polygon, ssf);

            if (ssresult.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(ssresult.Value.GetObjectIds());
            }

            return ids;
        }

        #endregion
    }
}

