using System;
using System.Runtime.Serialization;
using System.Security;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PtFixNotReceivedLogon : PtFixException
    {
        /// <summary>
        /// Initializes a new instance of the PatioFIX.Common.FixSupport.PtFixNotReceivedLogon class.
        /// <para>This member is reserved for internal use and is not intended to be used directly by user code.</para>
        /// </summary>
        public PtFixNotReceivedLogon()
            : base("NotReceivedLogon")
        {
        }

        /// <summary>
        /// Initializes a new instance of the  PatioFIX.Common.FixSupport.PtFixNotReceivedLogon class with a specified error message.
        /// <para>This member is reserved for internal use and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="message"> A message that describes the error.</param>
        public PtFixNotReceivedLogon(string message)
            : base(message)
        {
        }



        /// <summary>
        /// Initializes a new instance of the PatioFIX.Common.FixSupport.PtFixNotReceivedLogon class with serialized data.
        /// <para>This protected constructor is used for deserialization and is not intended to be used directly by user code.</para>
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context"> The contextual information about the source or destination.</param>
        [SecuritySafeCritical]
        protected PtFixNotReceivedLogon(SerializationInfo info, StreamingContext context)
            : base(info, context)
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
