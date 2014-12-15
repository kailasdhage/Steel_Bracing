namespace Steel_Bracing_2d
{
    using System;
    using System.Reflection;

    using Autodesk.AutoCAD.DatabaseServices;

    public static class VersionCompatibleExtensionMethods
    {
        #region Static Fields

        private static readonly PropertyInfo mtextStyleProperty = null;

        private static readonly PropertyInfo textStyleProperty = null;

        #endregion

        #region Constructors and Destructors

        static VersionCompatibleExtensionMethods()
        {
            textStyleProperty = typeof(DBText).GetProperty("TextStyle");
            if (textStyleProperty == null)
                textStyleProperty = typeof(DBText).GetProperty("TextStyleId");

            mtextStyleProperty = typeof(DBText).GetProperty("TextStyle");

            if (mtextStyleProperty == null)
                mtextStyleProperty = typeof(DBText).GetProperty("TextStyleId");
        }

        #endregion

        #region Public Methods and Operators

        public static ObjectId GetTextStyleId(this DBText instance)
        {
            if (textStyleProperty != null)
                return (ObjectId)textStyleProperty.GetValue(instance, null);
            else
                throw new MissingMemberException();
        }

        public static ObjectId GetTextStyleId(this MText instance)
        {
            if (mtextStyleProperty != null)
                return (ObjectId)mtextStyleProperty.GetValue(instance, null);
            else
                throw new MissingMemberException();
        }

        public static void SetTextStyleId(this DBText instance, ObjectId value)
        {
            if (textStyleProperty != null)
                textStyleProperty.SetValue(instance, value, null);
            else
                throw new MissingMemberException();
        }

        public static void SetTextStyleId(this MText instance, ObjectId value)
        {
            if (mtextStyleProperty != null)
                mtextStyleProperty.SetValue(instance, value, null);
            else
                throw new MissingMemberException();
        }

        #endregion
    }
}
