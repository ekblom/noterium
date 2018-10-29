using System;

namespace Noterium.Core.Exceptions
{
    public class SaveException : Exception
    {
        public SaveException(object unsavedObject)
        {
            UnsavedObject = unsavedObject;
        }

        public object UnsavedObject { get; }
    }
}