using System;
using System.Runtime.Serialization;

namespace PatioFIX
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PtDeadlockException : PtDataException
    {
        /// <summary>
        /// Initializes a new instance of the PtDeadlockException class. This is the default constructor.
        /// </summary>
        public PtDeadlockException()
            : base("Transaction was deadlocked")
        {
        }

        /// <summary>
        /// Initializes a new instance of the PtDeadlockException class with the specified string.
        /// </summary>
        /// <param name="message">The string to display when the exception is thrown.</param>
        public PtDeadlockException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PtDeadlockException class with the specified serialization information and context.
        /// <para>This protected constructor is used for deserialization and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="info">The data necessary to serialize or deserialize an object.</param>
        /// <param name="context">Description of the source and destination of the specified serialized stream.</param>
        protected PtDeadlockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PtDeadlockException class with the specified string and inner exception.
        /// </summary>
        /// <param name="message">The string to display when the exception is thrown.</param>
        /// <param name="innerException">A reference to an inner exception.</param>
        public PtDeadlockException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// GetObjectData performs a custom serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
