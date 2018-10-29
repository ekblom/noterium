using System;
using System.Reflection;

namespace Noterium.Core.Helpers
{
    public static class Reflection
    {
        /// <summary>
        ///     Extension for 'Object' that copies the properties to a destination object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            // Iterate the Properties of the source instance and
            // populate them from their desination counterparts
            var srcProps = typeSrc.GetProperties();
            foreach (var srcProp in srcProps)
            {
                if (!srcProp.CanRead) continue;
                var targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null) continue;
                if (!targetProperty.CanWrite) continue;

                var setMethod = targetProperty.GetSetMethod(true);
                if (setMethod != null && setMethod.IsPrivate) continue;

                setMethod = targetProperty.GetSetMethod();
                if (setMethod == null || (setMethod.Attributes & MethodAttributes.Static) != 0) continue;
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)) continue;
                // Passed all tests, lets set the value
                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }
    }
}