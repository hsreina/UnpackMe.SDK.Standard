namespace UnpackMe.SDK.Core.Exceptions
{
    public class InvalidLoginException : UnpackMeException
    {
        public InvalidLoginException() : base() { }

        public InvalidLoginException(string message) : base(message) { }
    }
}
