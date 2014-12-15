namespace Steel_Bracing_2d.AcFramework
{
    using System.Text;

    /// <summary>
    /// class for encoding string
    /// </summary>
    public class HexEncoding
    {
        #region Static Fields

        static readonly char[] hexChar = new char[]
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8',
                '9', 'A', 'B', 'C', 'D', 'E', 'F'
            };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// convert string to byte array
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static byte[] toBinArray(string hexStr)
        {
            byte[] retArr = new byte[hexStr.Length / 2];

            for (int i = 0; i < retArr.Length; i++)
            {
                retArr[i] = byte.Parse(hexStr.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            return retArr;
        }

        /// <summary>
        /// convert byte array to string
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string toHexString(byte[] b)
        {
            StringBuilder sb = new StringBuilder(b.Length * 2);
            for (int i = 0; i < b.Length; i++)
            {
                //look up high nibble char
                sb.Append(hexChar[(b[i] & 0xF0) >> 4]);

                //look up low nibble char
                sb.Append(hexChar[b[i] & 0x0F]);
            }

            return sb.ToString();
        }

        #endregion
    }
}
