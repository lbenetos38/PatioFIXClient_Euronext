using System;
using System.Data.Common;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PtChange
    {
        #region Class Properties
        /// <summary>
        /// 
        /// </summary>
        public Int32 ChngID { get; }

        /// <summary>
        /// 
        /// </summary>
        public String OrderNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String OrderDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ChangedPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ChangedVolume { get; set; }



        /// <summary>
        /// 
        /// </summary>
        public String CSDAccountID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String OriginalPriceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ChangedLife { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ChangedExpirationDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String MemberOrderNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ChngProcessCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? WorkingDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String VenueId { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public String SecuritySymbol { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public Char SecurityIDSource { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public String MemberID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String TraderID { get; set; }




        #endregion

        #region class constructors
        /// <summary>
        /// 
        /// </summary>
        internal PtChange()
        {
            this.WorkingDate = DateTime.Now;
            this.VenueId = string.Empty;
            this.SecuritySymbol = string.Empty;
            this.SecurityIDSource = '8';
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        internal PtChange(DbDataReader reader)
        {
            this.ChngID = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) this.OrderNumber = reader.GetString(1).Trim();
            if (!reader.IsDBNull(2)) this.OrderDate = reader.GetString(2).Trim();
            if (!reader.IsDBNull(3)) this.ChangedPrice = reader.GetDecimal(3);
            if (!reader.IsDBNull(4)) this.ChangedVolume = reader.GetDecimal(4);
            if (!reader.IsDBNull(5)) this.CSDAccountID = reader.GetString(5).Trim();
            if (!reader.IsDBNull(6)) this.OriginalPriceType = reader.GetString(6).Trim();
            if (!reader.IsDBNull(7)) this.ChangedLife = reader.GetString(7).Trim();
            if (!reader.IsDBNull(8)) this.ChangedExpirationDate = reader.GetString(8).Trim();
            if (!reader.IsDBNull(9)) this.MemberOrderNumber = reader.GetString(9).Trim();
            if (!reader.IsDBNull(10)) this.ChngProcessCode = reader.GetDecimal(10);
            if (!reader.IsDBNull(11)) this.WorkingDate = reader.GetDateTime(11);
            if (!reader.IsDBNull(12)) this.VenueId = reader.GetString(12).Trim();
            this.SecuritySymbol = reader.GetString(13);
            this.SecurityIDSource = reader.GetString(14)[0];
            if (!reader.IsDBNull(15)) this.MemberID = reader.GetString(15).Trim();
            if (!reader.IsDBNull(16)) this.TraderID = reader.GetString(16).Trim();
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ChngID.ToString();
        }




    }
}