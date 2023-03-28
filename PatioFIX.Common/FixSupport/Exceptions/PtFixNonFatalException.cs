using System;
using System.Runtime.Serialization;
using System.Security;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class PtFixNonFatalException : PtFixException
    {
        /// <summary>
        /// Initializes a new instance of the PatioFIX.Common.FixSupport.PtFixNonFatalException class.
        /// <para>This member is reserved for internal use and is not intended to be used directly by user code.</para>
        /// </summary>
        public PtFixNonFatalException()
            : base("FixFatalException")
        {
        }

        /// <summary>
        /// Initializes a new instance of the  PatioFIX.Common.FixSupport.PtFixNonFatalException class with a specified error message.
        /// <para>This member is reserved for internal use and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="message"> A message that describes the error.</param>
        public PtFixNonFatalException(string message)
            : base(message)
        {
        }




        /// <summary>
        /// Initializes a new instance of the PatioFIX.Common.FixSupport.PtFixNonFatalException class with serialized data.
        /// <para>This protected constructor is used for deserialization and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context"> The contextual information about the source or destination.</param>
        [SecuritySafeCritical]
        protected PtFixNonFatalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PatioFIX.Common.FixSupport.PtFixNonFatalException class with
        /// a specified error message and a reference to the inner exception that is
        /// the cause of this exception.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public PtFixNonFatalException(string message, Exception innerException)
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
