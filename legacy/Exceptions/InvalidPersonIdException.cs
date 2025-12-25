namespace Exceptions
{
    /// <summary>
    /// Microsoft's Custom Exception best practices recommend creating the 3 standard constructors for custom exceptions.
    public class InvalidPersonIdException : ArgumentException
    {
        public InvalidPersonIdException() : base() { }

        public InvalidPersonIdException(string message) : base(message) { }

        public InvalidPersonIdException(string message, Exception innerException) : base(message, innerException) { }

        //Optional: Additional constructor for ArgumentException
        public InvalidPersonIdException(string message, string paramName) : base(message, paramName) { }

        public InvalidPersonIdException(string message, string paramName, Exception innerException) : base(message, paramName, innerException) { }

    }
}
