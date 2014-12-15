namespace Steel_Bracing_2d
{
    using System;
    using System.Reflection;

    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// 
    /// Platform compatibility extension methods for 
    /// Autodesk.AutoCAD.DatabaseServices.Entity
    /// 
    /// These methods make it easier to deploy a single,
    /// platform-neutral managed assembly that can run 
    /// on both 32 and 64 bit AutoCAD.
    /// 
    /// </summary>

    public static class PlatformCompatibilityExtensionMethods
    {
        #region Static Fields

        static readonly MethodInfo IntersectWithMethod1;
        static readonly MethodInfo IntersectWithMethod2;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Extension methods that act as platform-independent
        /// surrogates for the Entity.IntersectWith() method, 
        /// allowing a single managed assembly that uses them
        /// to run on both 32 or 64 bit AutoCAD.
        /// 
        /// The following extension method overloads are supported:
        /// 
        ///   IntersectWith( Entity, Intersect, Point3dCollection );
        ///   IntersectWith( Entity, Intersect, Point3dCollection, IntPtr, IntPtr );
        ///   IntersectWith( Entity, Intersect, Plane, Point3dCollection );
        ///   IntersectWith( Entity, Intersect, Plane, Point3dCollection, IntPtr, IntPtr );
        ///    
        /// The versions which do not require IntPtr as the last two arguments
        /// pass the default of 0 for the GsMarker parameters of the Entity's
        /// IntersectWith() method.
        /// 
        /// The versions that require two IntPtr arguments as their last two
        /// parameters convert the passed IntPtr to the required type (Int32
        /// or Int64, depending on the platform the code is running on), and
        /// pass those values for the GsMarker parameters.
        /// 
        /// All other parameters are equivalent to the corresponding
        /// parameters of the Entity's IntersectWith() method.
        /// 
        /// All overloads return the number of intersections found.
        /// 
        /// Use these methods in lieu of Entity.IntersectWith() to 
        /// enable your code to run on both 32 and 64 bit systems.
        /// 
        /// Performance Issues:
        /// 
        /// Because these extension methods use reflection to invoke the 
        /// underlying methods they act as surrogates for, they will not 
        /// perform as well as direct calls to those underlying methods. 
        /// This can be an issue in performance intensive applications, 
        /// and in such cases a hand-coded solution that avoids the use 
        /// of reflection may be a preferable alternative.
        /// 
        /// </summary>


        static PlatformCompatibilityExtensionMethods()
        {
            Object test32 = 0;
            Object test64 = (Int64)0;

            Console.Write(test32);
            Console.Write(test64);

            Type[] types1;
            Type[] types2;
            if (IntPtr.Size > 4)
            {
                types1 = new[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(Int64), typeof(Int64) };
                types2 = new[] { typeof(Entity), typeof(Intersect), typeof(Plane), typeof(Point3dCollection), typeof(Int64), typeof(Int64) };
            }
            else
            {
                types1 = new[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(Int32), typeof(Int32) };
                types2 = new[] { typeof(Entity), typeof(Intersect), typeof(Plane), typeof(Point3dCollection), typeof(Int32), typeof(Int32) };
            }

            IntersectWithMethod1 = typeof(Entity).GetMethod("IntersectWith", BindingFlags.Public | BindingFlags.Instance, null, types1, null);
            IntersectWithMethod2 = typeof(Entity).GetMethod("IntersectWith", BindingFlags.Public | BindingFlags.Instance, null, types2, null);
        }

        #endregion

        #region Public Methods and Operators

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Point3dCollection points)
        {
            int start = points.Count;
            object[] args;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, points, (Int64)0, (Int64)0 };
            else
                args = new object[] { other, intersectType, points, 0, 0 };
            IntersectWithMethod1.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Point3dCollection points, IntPtr thisGsMarker, IntPtr otherGsMarker)
        {
            int start = points.Count;
            object[] args;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, points, thisGsMarker.ToInt64(), otherGsMarker.ToInt64() };
            else
                args = new object[] { other, intersectType, points, thisGsMarker.ToInt32(), otherGsMarker.ToInt32() };
            IntersectWithMethod1.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Plane plane, Point3dCollection points)
        {
            int start = points.Count;
            object[] args;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, plane, points, (Int64)0, (Int64)0 };
            else
                args = new object[] { other, intersectType, plane, points, 0, 0 };
            IntersectWithMethod2.Invoke(entity, args);
            return points.Count - start;
        }

        public static int IntersectWith(this Entity entity, Entity other, Intersect intersectType, Plane plane, Point3dCollection points, IntPtr thisGsMarker, IntPtr otherGsMarker)
        {
            int start = points.Count;
            object[] args;
            if (IntPtr.Size > 4)
                args = new object[] { other, intersectType, plane, points, thisGsMarker.ToInt64(), otherGsMarker.ToInt64() };
            else
                args = new object[] { other, intersectType, plane, points, thisGsMarker.ToInt32(), otherGsMarker.ToInt32() };
            IntersectWithMethod2.Invoke(entity, args);
            return points.Count - start;
        }

        #endregion
    }
}