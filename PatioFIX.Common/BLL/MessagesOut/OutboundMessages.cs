using System.Collections.Generic;


namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class OutboundMessages
    {
        public IList<IOutboundMessage> Messages { get; }


        public OutboundMessages()
        {

            this.Messages = new List<IOutboundMessage>();
        }

        public void Clear()
        {
            this.Messages.Clear();
        }



    }
}
