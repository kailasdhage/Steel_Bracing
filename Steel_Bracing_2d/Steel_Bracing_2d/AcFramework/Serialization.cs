namespace Steel_Bracing_2d.AcFramework
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    class Serialization
    {
        #region Public Methods and Operators

        /// <summary>
        /// Deserilize generic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static T BinaryDeSerialize<T>(string hexString) where T : new()
        {
            T obj = new T();
            try
            {
                byte[] byteArr = HexEncoding.toBinArray(hexString);
                MemoryStream ms = new MemoryStream();
                ms.Write(byteArr, 0, byteArr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new CustomBinder();
                obj = (T)bf.Deserialize(ms);

                ms.Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// deserialize from the string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static object BinaryDeSerialize(string hexString)
        {
            object obj = null;
            try
            {
                byte[] byteArr = HexEncoding.toBinArray(hexString);
                MemoryStream ms = new MemoryStream();
                ms.Write(byteArr, 0, byteArr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new CustomBinder();
                obj = bf.Deserialize(ms);

                ms.Close();
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        /// <summary>
        /// Deserilize generic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static T BinaryDeSerialize<T>(byte[] byteArr) where T : new()
        {
            T obj = new T();
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(byteArr, 0, byteArr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new CustomBinder();
                obj = (T)bf.Deserialize(ms);

                ms.Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return obj;
        }

        /// <summary>
        /// deserialize from the string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static object BinaryDeSerialize(byte[] byteArr)
        {
            object obj = null;
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(byteArr, 0, byteArr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new CustomBinder();
                obj = bf.Deserialize(ms);

                ms.Close();
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        /// <summary>
        /// Serilize generic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string BinarySerialize<T>(T obj)
        {
            string retResult = "";

            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                byte[] retArr = ms.ToArray();
                ms.Close();

                retResult = HexEncoding.toHexString(retArr);
            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        /// <summary>
        /// Serilize generic object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] BinarySerialize2<T>(T obj)
        {
            byte[] retResult = null;

            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                byte[] retArr = ms.ToArray();
                ms.Close();

                retResult = retArr;
            }
            catch (Exception ex)
            {
            }

            return retResult;
        }

        #endregion
    }
}
