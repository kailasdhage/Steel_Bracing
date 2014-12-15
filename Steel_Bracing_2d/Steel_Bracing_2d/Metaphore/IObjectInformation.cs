namespace Steel_Bracing_2d.Metaphore
{
    public interface IObjectInformation
    {
        #region Public Properties

        long ObjectHandle { get; set; }

        #endregion

        #region Public Methods and Operators

        string GetToolTip();

        #endregion
    }
}
