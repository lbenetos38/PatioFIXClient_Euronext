using PatioFIX.Common.FixSupport;
using System;
using System.Data.SqlClient;
using static PatioFIX.Common.ODLClientAPIUtilities;


namespace PatioFIX.Common
{
    /// <summary>
    /// Αυτο το Message (MD) το δημιουργούμε εμείς για να το στείλουμε στο ATHEX Gateway 
    /// (Υλοποιεί τα πεδία απο το ETS_BROKERLib.IOrderChange)
    /// </summary>
    public class OrderChangeOutMessage : IOutboundMessage
    {
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 RowID => this.ChngID;
        /// <summary>
        /// Για ποιο OrderId είναι (με το OrderID της βασης μας) αυτη η αλλαγη
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
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Change;




        #region Class Properties
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 ChngID { get; }

        #region Τα επομενα πεδία είναι απο το ETS_BROKERLib.IOrderChange

        /// <summary>
        /// 
        /// </summary>
        public String MemberID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String TraderID { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public String VenueID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char BoardID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public string SecurityID { get; internal set; }
        /// <summary>
        /// '8': Exchange Symbol
        /// 'A': Bloomberg Symbol.
        /// </summary>
        public Char SecurityIDSource { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String Currency { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrderNumber { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrderDate { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedPrice { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedVolume { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedDisclosedVolume { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedAutoDisclosedVolume { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ChangedCSDAccountID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedGOIFlag { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedShortSellFlag { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedOriginalPriceType { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedLife { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ChangedExpirationDate { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrigClientOrderID { get; internal set; }
        /// <summary>
        /// This ClientOrderID attribute should be completed using 16 characters and it is intended for 
        /// internal use by the Member
        /// <para>ChangedMemberOrderNumber</para>
        /// </summary>
        public String ClientOrderID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ChangedOrderNote { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ListID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ChangedClearingMemberID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedPositionEffect { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedSettlType { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedDirectElectronicAccess { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedClientID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedClientIDQualifier { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedInvestmentDecisionID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedInvestmentDecisionIDQualifier { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedExecutionWithinFirmID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedExecutionWithinFirmIDQualifier { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ChangedNonExecutingBrokerID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ChangedCommodityHedgingFlag { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String ChangedSpecialInstructions { get; internal set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public String SecuritySymbol { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String SecurityCode { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public String OrderStatusNote { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String PEL_PROF { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime WorkingDate { get; internal set; }


        public Char Side { get; private set; }
        /// <summary>
        /// OrigClOrdID (Tag = 41, Type: String)
        /// </summary>
        public Int32 OrigClOrdID { get; }
        /// <summary>
        /// OrderID (Tag = 37, Type: String)
        /// </summary>
        public string ExchangeOrderID { get; }

        /// <summary>
        /// ExecInst (Tag = 18, Type: MultipleCharValue)
        /// Instructions for order handling on exchange trading floor.
        /// </summary>
        public ExecInstEnum ExecInst { get; } = ExecInstEnum.Default;
        #endregion




        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        internal OrderChangeOutMessage(SqlDataReader reader)
        {
            #region SqlDataReader
            this.ChngID = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) this.MemberID = reader.GetString(1).Trim();
            if (!reader.IsDBNull(2)) this.TraderID = reader.GetString(2).Trim();
            if (!reader.IsDBNull(3)) this.VenueID = reader.GetString(3).Trim();
            if (!reader.IsDBNull(4)) this.BoardID = reader.GetString(4)[0];
            this.SecurityIDSource = reader.GetString(5)[0];
            this.SecuritySymbol = reader.GetString(6);
            this.SecurityCode = reader.GetString(7);
            this.Currency = reader.GetString(8).Trim();
            if (!reader.IsDBNull(9)) this.OrderNumber = reader.GetString(9).Trim();
            if (!reader.IsDBNull(10)) this.OrderDate = reader.GetString(10).Trim();
            if (!reader.IsDBNull(11)) this.ChangedPrice = reader.GetDecimal(11);
            if (!reader.IsDBNull(12)) this.ChangedVolume = reader.GetDecimal(12);
            if (!reader.IsDBNull(13)) this.ChangedDisclosedVolume = reader.GetDecimal(13);
            if (!reader.IsDBNull(14)) this.ChangedAutoDisclosedVolume = reader.GetDecimal(14);
            if (!reader.IsDBNull(15)) this.ChangedCSDAccountID = reader.GetString(15).Trim();
            this.ChangedGOIFlag = reader.GetString(16)[0];
            this.ChangedShortSellFlag = reader.GetString(17)[0];
            if (!reader.IsDBNull(18)) this.ChangedOriginalPriceType = reader.GetString(18)[0];
            if (!reader.IsDBNull(19)) this.ChangedLife = reader.GetString(19)[0];
            if (!reader.IsDBNull(20)) this.ChangedExpirationDate = reader.GetString(20).Trim();
            if (!reader.IsDBNull(21)) this.OrigClientOrderID = reader.GetString(21).Trim();
            if (!reader.IsDBNull(22)) this.ClientOrderID = reader.GetString(22).Trim();             //ChangedMemberOrderNumber

            if (!reader.IsDBNull(23)) ChangedOrderNote = reader.GetString(23).Trim();

            if (!reader.IsDBNull(24)) this.ListID = reader.GetString(24).Trim();
            if (!reader.IsDBNull(25)) this.ChangedClearingMemberID = reader.GetString(25).Trim();
            this.ChangedPositionEffect = reader.GetString(26)[0];
            this.ChangedSettlType = reader.GetString(27)[0];
            if (!reader.IsDBNull(28)) this.ChangedDirectElectronicAccess = reader.GetString(28)[0];
            if (!reader.IsDBNull(29)) this.ChangedClientID = reader.GetDecimal(29);
            if (!reader.IsDBNull(30)) this.ChangedClientIDQualifier = reader.GetString(30)[0];
            if (!reader.IsDBNull(31)) this.ChangedInvestmentDecisionID = reader.GetDecimal(31);
            if (!reader.IsDBNull(32)) this.ChangedInvestmentDecisionIDQualifier = reader.GetString(32)[0];
            if (!reader.IsDBNull(33)) this.ChangedExecutionWithinFirmID = reader.GetDecimal(33);
            if (!reader.IsDBNull(34)) this.ChangedExecutionWithinFirmIDQualifier = reader.GetString(34)[0];
            if (!reader.IsDBNull(35)) this.ChangedNonExecutingBrokerID = reader.GetDecimal(35);
            if (!reader.IsDBNull(36)) this.ChangedSpecialInstructions = reader.GetString(36);

            if (!reader.IsDBNull(37)) this.OrderStatusNote = reader.GetString(37).Trim().ToUpperInvariant();

            if (!reader.IsDBNull(38)) this.PEL_PROF = reader.GetString(38);
            if (!reader.IsDBNull(39)) this.ChangedCommodityHedgingFlag = reader.GetString(39)[0];
            this.WorkingDate = reader.GetDateTime(40);

            this.Side = reader.GetString(41)[0];//char(1)
            if (!reader.IsDBNull(42)) this.OrigClOrdID = reader.GetInt32(42);
            if (!reader.IsDBNull(43)) this.ExchangeOrderID = reader.GetString(43);
            if (!reader.IsDBNull(44))
            {
                var execInst = reader.GetString(44);
                if (execInst == "S")
                    this.ExecInst = ExecInstEnum.Suspend;
                else if(execInst == "q")
                    this.ExecInst = ExecInstEnum.Unsuspend;
                //ειδαλλως μενει με την Default τιμη
            }

            if (Int32.TryParse(this.ClientOrderID, out Int32 result))
            {
                this.OrderID = result;
            }
            #endregion



            if (this.VenueID == "XIPO")
			{
				/* 
				 * Electroning Book Building HBIP
				 */
				this.TargetConnection = "ORA";
                this.ChangedSpecialInstructions = _FormatAlphaField(this.ChangedSpecialInstructions, 120);
            }
            else
            {
                this.TargetConnection = "ETS";
                this.ChangedSpecialInstructions = string.Empty;
            }



            if (this.SecurityIDSource == SECURITYIDSource.USE_EXCHAGE_SYMBOL)
            {
                //Exchange Symbol
                this.SecurityID = this.SecuritySymbol;
            }
            else
            {
                //Bloomberg Symbol
                this.SecurityID = this.SecurityCode;
            }

			//OrderStatusNote
			if (this.OrderStatusNote == null)
			{
				this.OrderStatusNote = "XX";/* Return an empty ACC_Description */
			}
			//OrderComment (το συμπιεζουμε λιγο...)
			var _originalComment = this.ChangedOrderNote;
            if (_originalComment != null)
            {
                if (_originalComment.StartsWith(@"GALATIA\"))
                    this.ChangedOrderNote = _originalComment.Replace(@"GALATIA\", @"GL\");
                else if (_originalComment.StartsWith(@"EXTRANET\"))
                    this.ChangedOrderNote = _originalComment.Replace(@"EXTRANET\", @"EXT\");
            }


			/*
             * Τωρα θα φτιαξουμε το ChangedOrderNote/tag58 για την αλλαγη μας
             * Προσοχη, ακομα εχουμε το limit των 25 χαρακτηρων (εξαιτιας του ODL)!
             */
			var _orderNote = _FormatAlphaField(this.OrderStatusNote, 5);
			if (this.VenueID == "XIPO")
			{
				_orderNote = _orderNote + _FormatAlphaField(this.ChangedOrderNote, 20);
			}
            else
            {
			    if (_originalComment == @"GALATIA\SALESTRADER")
                {
				    _orderNote = _orderNote + _FormatAlphaField(this.PEL_PROF == "ΡΩΩΩ" ? "~1~" : "~6~", 3);
                    _orderNote = _orderNote + _FormatAlphaField(this.ChangedOrderNote, 17);
                }
                else
                {
                    _orderNote = _orderNote + _FormatAlphaField(this.ChangedOrderNote, 20);
                }
            }
            this.ChangedOrderNote = _orderNote;


        }



        public void FormatMessage(FIXMessageWriter writer)
        {
            writer.Clear();

            writer.Set(Tags.OrderID, this.ExchangeOrderID);     //το OrderID απο το Exchange
            writer.Set(Tags.OrigClOrdID, this.OrigClOrdID);     //To τελευταιο id που στειλαμε για αυτο το order
            writer.Set(Tags.ClOrdID, this.ChngID);              //το νεο μας orderID


            SetPartyIDs(writer);

            writer.Set(Tags.Account, this.ChangedCSDAccountID);


            writer.Set(Tags.SecurityID, this.SecurityID);
            writer.Set(Tags.SecurityIDSource, this.SecurityIDSource);
            writer.Set(Tags.SecurityExchange, this.VenueID);


            #region OrdType + TimeInForce + ExpireDate
            if (this.ChangedOriginalPriceType == /*OrderPriceTypeEnum.Limit*/'L')
            {
                writer.Set(Tags.OrdType, /*Limit or better (Deprecated)*/'7');
                writer.Set(Tags.Price, this.ChangedPrice, decimals: 4);
            }
            else if (this.ChangedOriginalPriceType == /*OrderPriceTypeEnum.Market*/'M')
            {
                writer.Set(Tags.OrdType, /*Market*/'1');
            }
            else if (this.ChangedOriginalPriceType == /*OrderPriceTypeEnum.AtTheClose*/'C')
            {
                //writer.Set(Tags.OrdType, /*Limit or better (Deprecated)*/'7');
                //writer.Set(Tags.TimeInForce, /*At the Close*/'7');
                writer.Set(Tags.OrdType, /*On Close*/'A');

                writer.Set(Tags.Price, this.ChangedPrice, decimals: 4);
            }
            else if (this.ChangedOriginalPriceType == /*OrderPriceTypeEnum.AtTheOpen*/'O')
            {
                writer.Set(Tags.OrdType, /*Limit or better (Deprecated)*/'7');
                writer.Set(Tags.TimeInForce, /*At the Opening (OPG)*/'2');
                writer.Set(Tags.Price, this.ChangedPrice, decimals: 4);
            }
            else
            {
                throw new Exception($"OriginalPriceType unsupported '{this.ChangedOriginalPriceType}'");
            }

            if (this.ChangedOriginalPriceType == 'L' || this.ChangedOriginalPriceType == 'M')
            {
                if (this.ChangedLife == 'D')
                {
                    writer.Set(Tags.TimeInForce, /*Day (or session)*/'0');
                }
                else if (this.ChangedLife == 'C')
                {
                    writer.Set(Tags.TimeInForce, /*Good Till Cancel (GTC)*/'1');
                }
                else if (this.ChangedLife == 'E')
                {
                    writer.Set(Tags.TimeInForce, /*Good Till Date (GTD)*/'6');
                    writer.Set(Tags.ExpireDate, this.ChangedExpirationDate);
                }
                else
                {
                    throw new Exception($"OrderLifeTime unsupported '{this.ChangedLife}'");
                }
            }
            #endregion


            writer.Set(Tags.Text, this.ChangedOrderNote.Trim());

            if (this.ChangedPositionEffect == ' ' || this.ChangedPositionEffect == 'O')
            {
                writer.Set(Tags.PositionEffect, 'O');
            }
            else if (this.ChangedPositionEffect == 'C')
            {
                writer.Set(Tags.PositionEffect, 'C');
            }
            else
            {
                throw new Exception($"Unknown PositionEffect '{this.ChangedPositionEffect}'");
            }

            if (this.ChangedSettlType == 'C' || this.ChangedSettlType == ' ' || this.ChangedSettlType == '0')
            {
                writer.Set(Tags.SettlType, /*Regular*/'0');
            }
            else if (this.ChangedSettlType == '1')
            {
                writer.Set(Tags.SettlType, /*Immediate*/'1');
            }
            else
            {
                throw new Exception($"Unknown SettlType '{this.ChangedSettlType}'");
            }



            #region OrderQty, MaxShow, ExecInst
            if (this.ExecInst == ExecInstEnum.Suspend)
            {
                /*
                 * Εχουμε απενεργοποιηση εντολης
                 */
                writer.Set(Tags.OrderQty, this.ChangedVolume);
                writer.Set(Tags.MaxShow, this.ChangedDisclosedVolume);
                writer.Set(Tags.ExecInst, "S");
            }
            else if(this.ExecInst == ExecInstEnum.Unsuspend)
            {
                /*
                 * Εχουμε ενεργοποιηση εντολης
                 */
                writer.Set(Tags.OrderQty, this.ChangedVolume);
                writer.Set(Tags.MaxShow, this.ChangedDisclosedVolume);
                writer.Set(Tags.ExecInst, "q");
            }
            else
            {
                /*
                 * Εχουμε αλλαγη σε ιδιοτητες της εντολης
                 */
                writer.Set(Tags.OrderQty, this.ChangedVolume);
                writer.Set(Tags.MaxShow, this.ChangedDisclosedVolume);
            }
            #endregion


            if (this.Side == 'S')
            {
                if (this.ChangedShortSellFlag == 'Y')
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
            writer.Set(Tags.NoPartyIDs, 7);



            /*Executing Firm*/
            writer.Set(Tags.PartyID, this.MemberID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, 1);


            /*Entering trader*/
            writer.Set(Tags.PartyID, this.TraderID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, /*Entering trader*/36);

            /*Clearing Firm*/
            writer.Set(Tags.PartyID, this.ChangedClearingMemberID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, 4);


            /*Client ID*/
            writer.Set(Tags.PartyID, (int)this.ChangedClientID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, 3);
            if (this.ChangedClientIDQualifier == 'L')
                writer.Set(CustomTags.PartyRoleQualifier, /*Firm or Legal Entity*/23);
            else
                writer.Set(CustomTags.PartyRoleQualifier, /*Natural Person*/24);





            /*Investment Decision Maker - (MIFID II: Investment decision within firm)*/
            writer.Set(Tags.PartyID, (int)this.ChangedInvestmentDecisionID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, /*Investment Decision Maker - (MIFID II: Investment decision within firm)*/122);
            if (this.ChangedInvestmentDecisionID != 0)
            {
                if (this.ChangedInvestmentDecisionIDQualifier == 'A')
                    writer.Set(CustomTags.PartyRoleQualifier, 22);
                else
                    writer.Set(CustomTags.PartyRoleQualifier, 24);
            }


            /*Executing Trader - (MIFID II: Execution within firm)*/
            writer.Set(Tags.PartyID, (int)this.ChangedExecutionWithinFirmID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, /*Executing Trader - (MIFID II: Execution within firm)*/12);
            //if (this.ChangedExecutionWithinFirmIDQualifier == 'A')
            //    writer.Set(CustomTags.PartyRoleQualifier, 22);
            //else
            //    writer.Set(CustomTags.PartyRoleQualifier, 24);



            /*Correspondent Broker - (MIFID II: Non-executing broker)*/
            writer.Set(Tags.PartyID, (int)this.ChangedNonExecutingBrokerID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, 26);




            #endregion
        }


        #region UnConfirmedPool support
        public long _UnConfirmedPool_ticks { get; set; }
        public bool _UnConfirmedPool_abandoned { get; set; }
        #endregion
    }
}
