using System;
using Noterium.Core.DataCarriers;

namespace Noterium.Core.Exceptions
{
    public class SaveException : Exception
    {
        public SaveException(Object unsavedObject)
        {
            UnsavedObject = unsavedObject;
        }

        public Object UnsavedObject { get; private set; }
    }
}