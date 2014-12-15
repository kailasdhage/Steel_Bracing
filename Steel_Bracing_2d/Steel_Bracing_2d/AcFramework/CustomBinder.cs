namespace Steel_Bracing_2d.AcFramework
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class used in serialization to resolve class names
    /// </summary>
    class CustomBinder : SerializationBinder
    {
        #region Public Methods and Operators

        /// <summary>
        /// Bind method to resolve the class names, used in serialization
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            string[] typeInfo = typeName.Split('.');
            bool isSystem = (typeInfo[0] == "System");

            Type retType = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

            if( !isSystem && retType == null )
            {
                System.Reflection.Assembly currAssy = System.Reflection.Assembly.GetExecutingAssembly();
                Type newType = currAssy.GetType( typeName );
                if( newType == null )
                {

                }
                else
                {
                    retType = newType;
                }
            }
            return retType;
        }

        #endregion
    }
}
