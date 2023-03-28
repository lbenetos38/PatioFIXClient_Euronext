using System;
using System.Data.Common;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    internal class PtOrder
    {
        public Int32 OrderID { get; }
        public Int32 LastClOrdID { get; }
        public string ExchangeOrderID { get; }
        public string SecurityID { get; }
        public Decimal Volume { get; }
        public Decimal Price { get; }
        public Char Side { get; }
        public OrderProcessCodeEnum OrderProcessCode { get; }
        public OrderStatusCodeEnum OrderStatusCode { get; }
        public string OrderComment { get; }
        public string UserAseCode { get; }
        public string VenueId { get; }
        public char SecurityIDSource { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="PtException"></exception>
        internal PtOrder(DbDataReader reader)
        {
            this.OrderID = reader.GetInt32(0);
            this.LastClOrdID = reader.GetInt32(1);
            if (!reader.IsDBNull(2)) this.ExchangeOrderID = reader.GetString(2).Trim();
            this.SecurityID = reader.GetString(3).Trim();
            this.Volume = reader.GetDecimal(4);
            this.Price = reader.GetDecimal(5);
            this.Side = reader.GetString(6)[0];
            this.OrderProcessCode = (OrderProcessCodeEnum)reader.GetInt32(7);

            var _OrderStatusCode = reader.GetString(8).Trim()[0];
            if (_OrderStatusCode == '0')
                this.OrderStatusCode = OrderStatusCodeEnum.New;
            else if (_OrderStatusCode == '1')
                this.OrderStatusCode = OrderStatusCodeEnum.PartiallyFilled;
            else if (_OrderStatusCode == '2')
                this.OrderStatusCode = OrderStatusCodeEnum.Filled;
            else if (_OrderStatusCode == '3')
                this.OrderStatusCode = OrderStatusCodeEnum.DoneForDay;
            else if (_OrderStatusCode == '4')
                this.OrderStatusCode = OrderStatusCodeEnum.Canceled;
            else if (_OrderStatusCode == '5')
                this.OrderStatusCode = OrderStatusCodeEnum.Replaced;
            else if (_OrderStatusCode == '6')
                this.OrderStatusCode = OrderStatusCodeEnum.PendingCancel;
            else if (_OrderStatusCode == '7')
                this.OrderStatusCode = OrderStatusCodeEnum.Stopped;
            else if (_OrderStatusCode == '8')
                this.OrderStatusCode = OrderStatusCodeEnum.Rejected;
            else if (_OrderStatusCode == '9')
                this.OrderStatusCode = OrderStatusCodeEnum.Suspended;
            else if (_OrderStatusCode == 'A')
                this.OrderStatusCode = OrderStatusCodeEnum.PendingNew;
            else if (_OrderStatusCode == 'B')
                this.OrderStatusCode = OrderStatusCodeEnum.Calculated;
            else if (_OrderStatusCode == 'C')
                this.OrderStatusCode = OrderStatusCodeEnum.Expired;
            else if (_OrderStatusCode == 'D')
                this.OrderStatusCode = OrderStatusCodeEnum.AcceptedForBidding;
            else if (_OrderStatusCode == 'E')
                this.OrderStatusCode = OrderStatusCodeEnum.PendingReplace;
            else
            {
                throw new PtException(string.Format("Order {0} has unknown OrderStatusCode '{1}'", this.OrderID, _OrderStatusCode));
            }


            if (!reader.IsDBNull(9)) this.OrderComment = reader.GetString(9).Trim();
            if (!reader.IsDBNull(10)) this.UserAseCode = reader.GetString(10).Trim();
            this.VenueId = reader.GetString(11).Trim();
            this.SecurityIDSource = reader.GetString(12)[0];
        }
    }
}
