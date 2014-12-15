namespace Steel_Bracing_2d.AcFramework
{
    using System.Collections.Generic;

    public interface IObjectBrowser
    {
        #region Public Methods and Operators

        List<long> GetEntityHandles();

        #endregion
    }
}
