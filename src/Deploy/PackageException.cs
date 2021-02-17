using System;

namespace Deploy
{
    /// <summary>
    /// The exception that is thrown when an installation package fails to build.
    /// </summary>
    public class PackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class.
        /// </summary>
        public PackageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class with the specifed error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PackageException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class with the specifed error message
        /// and a reference to the inner exception that is the cause of the exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The inner exception that is the cause of the exception.</param>
        public PackageException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
