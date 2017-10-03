using System;

namespace UnpackMe.SDK.Core.Exceptions
{
    public class UnpackMeException : Exception
    {
        public UnpackMeException() : base() { }

        public UnpackMeException(string message) : base(message) { }
    }
}
