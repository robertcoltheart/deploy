using System;
using System.Runtime.Serialization;

namespace Deploy
{
    /// <summary>
    /// The exception that is thrown when an installation package fails to build.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected PackageException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}