namespace PatioFIX.Common.BLL.Messages
{
    /// <summary>
    /// The OrderAttributeGrp component provides additional attributes about the order.
    /// </summary>
    public class OrderAttributeComponent
    {
        /// <summary>
        /// OrderAttributeType (Tag = 2594, Type: int)
        /// The type of order attribute.
        /// </summary>
        public int OrderAttributeType;
        /// <summary>
        /// OrderAttributeValue (Tag = 2595, Type: String)
        /// The value associated with the order attribute type specified in OrderAttributeType(2594).
        /// </summary>
        public char OrderAttributeValue;
    }
}
