using System;
using System.Data.Common;

namespace PatioFIX.Common
{
    public sealed class PtCancel
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 CancelID { get; }
        /// <summary>
        /// 
        /// </summary>
        public String CancelMemberOrderNumber { get; }
        /// <summary>
        /// 
        /// </summary>
        public String CancelOrderNumber { get; }
        /// <summary>
        /// 
        /// </summary>
        public String CancelOrderDate { get; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal? CancelProcessCode { get; }



        public String CSDAccountID { get; set; }
        public String VenueId { get; set; }
        public String SecurityID { get; set; }
        public String MemberID { get; set; }
        public String TraderID { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        internal PtCancel(DbDataReader reader)
        {
            this.CancelID = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) this.CancelMemberOrderNumber = reader.GetString(1).Trim();
            if (!reader.IsDBNull(2)) this.CancelOrderNumber = reader.GetString(2).Trim();
            if (!reader.IsDBNull(3)) this.CancelOrderDate = reader.GetString(3).Trim();
            if (!reader.IsDBNull(4)) this.CancelProcessCode = reader.GetDecimal(4);

            if (!reader.IsDBNull(5)) this.CSDAccountID = reader.GetString(5).Trim();
            if (!reader.IsDBNull(6)) this.VenueId = reader.GetString(6).Trim();
            if (!reader.IsDBNull(7)) this.SecurityID = reader.GetString(7).Trim();
            if (!reader.IsDBNull(8)) this.MemberID = reader.GetString(8).Trim();
            if (!reader.IsDBNull(9)) this.TraderID = reader.GetString(9).Trim();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.CancelID.ToString();
        }
    }
}
