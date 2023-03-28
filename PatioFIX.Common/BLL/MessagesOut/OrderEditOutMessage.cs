using PatioFIX.Common.FixSupport;
using System;
using System.Data.SqlClient;


namespace PatioFIX.Common
{
    /// <summary>
    /// Αυτο το Message (MC) το δημιουργούμε εμείς για να το στείλουμε στο ATHEX Gateway 
    /// (Υλοποιεί τα πεδία απο το ETS_BROKERLib.IOrderEdit)
    /// </summary>
    public class OrderEditOutMessage : IOutboundMessage
    {
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 RowID => this.Cancelid;
        /// <summary>
        /// Για ποιο OrderId είναι (με το OrderID της βασης μας) αυτη η ακυρωση
        /// <para>Σε περιπτωση που είναι ορφανη εγγραφη (απο 3ο συστημα) τοτε εχει την τιμη -1</para>
        /// </summary>
        public Int32 OrderID { get; } = -1;
        /// <summary>
        /// 
        /// </summary>
        public String TargetConnection { get; }
        /// <summary>
        /// 
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Edit;




        #region Class Properties
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 Cancelid { get; }


        #region Τα επομενα πεδία είναι απο το ETS_BROKERLib.IOrderEdit
        /// <summary>
        /// All messages sent through the CTCI – ATHEX Gateway should 
        /// have the value 'C' in the MessageSource field.
        /// Filled by ODL
        /// </summary>
        public Char MessageSource { get; } = 'C';
        /// <summary>
        /// 
        /// </summary>
        public String MemberID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string TraderID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string MemberSequenceNumber { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String VenueID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char BoardID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string SecurityID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char SecurityIDSource { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String Currency { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrderNumber { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrderDate { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrigClientOrderID { get; private set; }
        /// <summary>
        /// This ClientOrderID attribute should be completed using 16 characters and it is intended for 
        /// internal use by the Member
        /// <para>MemberOrderNumber</para>
        /// </summary>
        public String ClientOrderID { get; private set; }
        /// <summary>
        /// 'C'	-> Cancel Order
        /// 'S'	-> Suspend (deactivate) order
        /// 'U'	-> Unsuspend (activate) order
        /// </summary>
        public Char EditType { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        public String ListID { get; private set; }
        /// <summary>
        /// Char(1)
        /// This property may be 'B' or 'S' for a buy order or a sell order respectively.
        /// </summary>
        public Char Side { get; private set; }
        /// <summary>
        /// This attribute indicates whether the order is a short sell order or a buy to cover order or none of the above.
        /// It may take the following values:
        ///		• 'N': Normal
        ///		• 'Y': Short Sell / Buy To Cover
        /// </summary>
        public Char ShortSellFlag { get; private set; } = 'N';


        public string ExchangeOrderID { get; }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        String SecuritySymbol { get; }
        /// <summary>
        /// 
        /// </summary>
        String SecurityCode { get; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime WorkingDate { get; }


        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// </summary>
        public string OrigClOrdID { get; }
        #endregion




        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        internal OrderEditOutMessage(SqlDataReader reader)
        {
            #region SqlDataReader
            this.Cancelid = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) this.VenueID = reader.GetString(1).Trim();
            if (!reader.IsDBNull(2)) this.BoardID = reader.GetString(2)[0];
            this.SecurityIDSource = reader.GetString(3)[0];
            this.SecuritySymbol = reader.GetString(4);
            this.SecurityCode = reader.GetString(5);
            this.Currency = reader.GetString(6).Trim();
            if (!reader.IsDBNull(7)) this.OrderNumber = reader.GetString(7).Trim();
            if (!reader.IsDBNull(8)) this.OrderDate = reader.GetString(8).Trim();
            if (!reader.IsDBNull(9)) this.ClientOrderID = reader.GetString(9).Trim();       //MemberOrderNumber
            this.OrigClientOrderID = this.ClientOrderID;
            if (!reader.IsDBNull(10)) this.EditType = reader.GetString(10)[0];
            if (!reader.IsDBNull(11)) this.ListID = reader.GetString(11).Trim();
            if (!reader.IsDBNull(12)) this.MemberID = reader.GetString(12).Trim();
            this.WorkingDate = reader.GetDateTime(13);
            if (!reader.IsDBNull(14)) this.OrigClOrdID = reader.GetString(14);
            this.Side = reader.GetString(15)[0];//char(1)
            if (!reader.IsDBNull(16)) this.ShortSellFlag = reader.GetString(16)[0];//char(1)
            if (!reader.IsDBNull(17)) this.ExchangeOrderID = reader.GetString(17);

            if (Int32.TryParse(this.ClientOrderID, out Int32 result))
            {
                this.OrderID = result;
            }
            #endregion


            this.TargetConnection = "ETS";

            this.TraderID = Globals.ETS_SiteTraderId;


            if (this.SecurityIDSource == (char)SECURITYIDSource.USE_EXCHAGE_SYMBOL)
            {
                //Exchange Symbol
                this.SecurityID = this.SecuritySymbol;
            }
            else
            {
                //Bloomberg Symbol
                this.SecurityID = this.SecurityCode;
            }


            if (this.VenueID == "XIPO")
            {
                this.TargetConnection = "ORA";          /* ORA=XNET Target system */

                /*
				 * οι ακυρώσεις που ερχονται απο το OrderBank δεν εχουν σεταρισμένο το Currency
				 */
                if (string.IsNullOrWhiteSpace(this.Currency))
                {
                    this.Currency = "EUR";
                }
            }

        }


        public void FormatMessage(FIXMessageWriter writer)
        {
            writer.Clear();


            writer.Set(Tags.OrderID, this.ExchangeOrderID);     //το OrderID απο το Exchange
            writer.Set(Tags.OrigClOrdID, this.OrigClOrdID);     //To τελευταιο id που στειλαμε για αυτο το order
            writer.Set(Tags.ClOrdID, this.Cancelid);              //το νεο μας orderID

            SetPartyIDs(writer);


            writer.Set(Tags.SecurityID, this.SecurityID);
            writer.Set(Tags.SecurityIDSource, this.SecurityIDSource);
            writer.Set(Tags.SecurityExchange, this.VenueID);



            if (this.Side == 'S')
            {
                if (this.ShortSellFlag == 'Y')
                {
                    writer.Set(Tags.Side, '5');
                }
                else
                {
                    writer.Set(Tags.Side, '2');
                }
            }
            else if (this.Side == 'B')
            {
                writer.Set(Tags.Side, '1');
            }
            else
            {
                throw new Exception($"Unknown OrderSide '{this.Side}'");
            }


            writer.Set(Tags.TransactTime, DateTime.UtcNow);
        }



        void SetPartyIDs(FIXMessageWriter writer)
        {
            #region PartyIDs
            writer.Set(Tags.NoPartyIDs, 2);



            /*Executing Firm*/
            writer.Set(Tags.PartyID, this.MemberID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, 1);


            /*Entering trader*/
            writer.Set(Tags.PartyID, this.TraderID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, /*Entering trader*/36);



            #endregion
        }



        #region UnConfirmedPool support
        public long _UnConfirmedPool_ticks { get; set; }
        public bool _UnConfirmedPool_abandoned { get; set; }
        #endregion
    }
}
