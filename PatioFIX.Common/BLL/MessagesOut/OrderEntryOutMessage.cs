using PatioFIX.Common.FixSupport;
using System;
using System.Data.SqlClient;
using static PatioFIX.Common.ODLClientAPIUtilities;

namespace PatioFIX.Common
{
    /// <summary>
    /// Αυτο το Message (MB) το δημιουργούμε εμείς για να το στείλουμε στο ATHEX Gateway 
    /// (Υλοποιεί τα πεδία απο το ETS_BROKERLib.IOrderEntry)
    /// </summary>
    public class OrderEntryOutMessage : IOutboundMessage
    {
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 RowID => this.OrderID;
        /// <summary>
        /// 
        /// </summary>
        public String TargetConnection { get; }
        /// <summary>
        /// 
        /// </summary>
        public ODLMessageTypeEnum ODLMessageType => ODLMessageTypeEnum.Order_Entry;




        #region Class Properties
        /// <summary>
        /// To id της εγγραφης μας, (στην βαση μας)
        /// </summary>
        public Int32 OrderID { get; }


        #region Τα επομενα πεδία είναι απο το ETS_BROKERLib.IOrderEntry
        /// <summary>
        /// 
        /// </summary>
        public String MemberID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String TraderID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String VenueID { get; private set; }
        /// <summary>
        /// Char(1)
        /// </summary>
        public Char BoardID { get; private set; }
        /// <summary>
        /// Char(1)
        /// This property may be “N” or “I” for automatic insertion of a new active order or an automatic insertion of a new inactive order respectively.For orders directed to Xorder Server two more options exist: ‘M’ for manual insertion of a new active order, and ‘X’ for manual insertion of a new inactive order.
        /// </summary>
        public Char OrderType { get; private set; } = 'N';
        /// <summary>
        /// Char(1)
        /// This property may be 'B' or 'S' for a buy order or a sell order respectively.
        /// </summary>
        public Char Side { get; private set; }
        /// <summary>
        /// UserAseCode
        /// </summary>
        public String CSDAccountID { get; private set; }
        /// <summary>
        /// Char(1)
        /// This field is not used anymore. The Group Of Inverstor functionality is removed.
        /// </summary>
        public Char GOIFlag { get; private set; } = 'N';
        /// <summary>
        /// This attribute indicates whether the order is a short sell order or a buy to cover order or none of the above.
        /// It may take the following values:
        ///		• 'N': Normal
        ///		• 'Y': Short Sell / Buy To Cover
        /// </summary>
        public Char ShortSellFlag { get; private set; } = 'N';
        /// <summary>
        /// 
        /// </summary>
        public string SecurityID { get; private set; }
        /// <summary>
        /// '8': Exchange Symbol
        /// 'A': Bloomberg Symbol.
        /// </summary>
        public Char SecurityIDSource { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String Currency { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal Price { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal Volume { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal DisclosedVolume { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char OrderLifeTime { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char SpecialConditions { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char OriginalPriceType { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String StopSecurityID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal StopPrice { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String ExpirationDate { get; private set; }

        /// <summary>
        /// This attribute should be completed using 25 characters and it is intended for 
        /// internal use by the member.
        /// </summary>
        public string OrderNote { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String ListID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public String ClearingMemberID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char PositionEffect { get; private set; } = ' ';//προσοχη είναι space
        /// <summary>
        /// 
        /// </summary>
        public Char SettlType { get; private set; } = 'C';
        /// <summary>
        /// 
        /// </summary>
        public Char DirectElectronicAccess { get; private set; } = '0';
        /// <summary>
        /// 
        /// </summary>
        public Decimal ClientID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ClientIDQualifier { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal InvestmentDecisionID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char InvestmentDecisionIDQualifier { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal ExecutionWithinFirmID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char ExecutionWithinFirmIDQualifier { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Decimal NonExecutingBrokerID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char TradingCapacity { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char LiquidityProvision { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Char AlgoFlag { get; private set; } = 'N';
        /// <summary>
        /// 
        /// </summary>
        public Char CommodityHedgingFlag { get; private set; } = 'N';
        /// <summary>
        /// Only used for XNET.
        /// This field is omitted altogether for messages directed to the ETS interface
        /// </summary>
        public String SpecialInstructions { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string KemRequesterAseCode { get; private set; }
		#endregion


		/// <summary>
		/// Εδω εχουμε το PELA_XR.PEL_ACC_Description
		/// (Εχει την μορφη 'ΧΧ1' ή 'ΧΧ12' ή 'ΧΧ123')
		/// </summary>
		String OrderStatusNote { get; set; }

        /// <summary>
        /// 
        /// </summary>
        String OrderComment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        String SecuritySymbol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        String SecurityCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        String PEL_PROF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime WorkingDate { get; }

        /// <summary>
        /// 
        /// </summary>
        public int ClientSalesTraderNoDiscount { get; }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public OrderEntryOutMessage(SqlDataReader reader)
        {
            #region SqlDataReader
            this.OrderID = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) this.BoardID = reader.GetString(1)[0]; //char(1)
            this.VenueID = reader.GetString(2).Trim();
            if (!reader.IsDBNull(3)) this.OrderType = reader.GetString(3)[0];//char(1)
            this.Side = reader.GetString(4)[0];//char(1)
            if (!reader.IsDBNull(5)) this.CSDAccountID = reader.GetString(5).Trim();        //UserAseCode
            if (!reader.IsDBNull(6)) this.GOIFlag = reader.GetString(6)[0];//char(1)
            if (!reader.IsDBNull(7)) this.ShortSellFlag = reader.GetString(7)[0];//char(1)
            this.SecurityIDSource = reader.GetString(8)[0];
            this.Currency = reader.GetString(9).Trim();
            this.SecuritySymbol = reader.GetString(10);
            this.SecurityCode = reader.GetString(11);
            if (!reader.IsDBNull(12)) this.Price = reader.GetDecimal(12);
            if (!reader.IsDBNull(13)) this.Volume = reader.GetDecimal(13);
            if (!reader.IsDBNull(14)) this.DisclosedVolume = reader.GetDecimal(14);
            if (!reader.IsDBNull(15)) this.OrderLifeTime = reader.GetString(15)[0];
            this.SpecialConditions = reader.GetString(16)[0];//char(1)
            this.OriginalPriceType = reader.GetString(17)[0];//char(1)
            this.StopSecurityID = reader.GetString(18);
            this.StopPrice = reader.GetDecimal(19);
            this.ExpirationDate = reader.GetString(20);

            if (!reader.IsDBNull(21)) this.OrderComment = reader.GetString(21).Trim();

            this.ClearingMemberID = reader.GetString(22);

            if (!reader.IsDBNull(23)) this.ListID = reader.GetString(23).Trim();
            if (!reader.IsDBNull(24)) this.PositionEffect = reader.GetString(24)[0];//char(1)
            if (!reader.IsDBNull(25)) this.SettlType = reader.GetString(25)[0];//char(1)

            if (!reader.IsDBNull(26)) this.DirectElectronicAccess = reader.GetString(26)[0];
            if (!reader.IsDBNull(27)) this.ClientID = reader.GetDecimal(27);
            if (!reader.IsDBNull(28)) this.ClientIDQualifier = reader.GetString(28)[0];
            if (!reader.IsDBNull(29)) this.InvestmentDecisionID = reader.GetDecimal(29);
            if (!reader.IsDBNull(30)) this.InvestmentDecisionIDQualifier = reader.GetString(30)[0];
            if (!reader.IsDBNull(31)) this.ExecutionWithinFirmID = reader.GetDecimal(31);
            if (!reader.IsDBNull(32)) this.ExecutionWithinFirmIDQualifier = reader.GetString(32)[0];
            if (!reader.IsDBNull(33)) this.NonExecutingBrokerID = reader.GetDecimal(33);
            if (!reader.IsDBNull(34)) this.TradingCapacity = reader.GetString(34)[0];
            if (!reader.IsDBNull(35)) this.LiquidityProvision = reader.GetString(35)[0];
            if (!reader.IsDBNull(36)) this.SpecialInstructions = reader.GetString(36);
            if (!reader.IsDBNull(37)) this.OrderStatusNote = reader.GetString(37);
            if (!reader.IsDBNull(38)) this.AlgoFlag = reader.GetString(38)[0];
            if (!reader.IsDBNull(39)) this.CommodityHedgingFlag = reader.GetString(39)[0];
            if (!reader.IsDBNull(40)) this.PEL_PROF = reader.GetString(40).Trim();
            this.WorkingDate = reader.GetDateTime(41);
            if (!reader.IsDBNull(42)) this.KemRequesterAseCode = reader.GetString(42).Trim();
            if (!reader.IsDBNull(43)) this.ClientSalesTraderNoDiscount = reader.GetInt32(43);
            #endregion



            if (this.VenueID == "XIPO")
			{
                /* 
				 * Electroning Book Building HBIP
				 */
                this.TargetConnection = "ORA";
				this.MemberID = Globals.Configuration.PatioOMS.ORA_MemberId;
				this.TraderID = Globals.Configuration.PatioOMS.ORA_TraderId;
                this.PositionEffect = 'O';
                this.SettlType = '0';
                this.SpecialInstructions = _FormatAlphaField(this.SpecialInstructions, 120);
            }
            else
            {
                this.TargetConnection = "ETS";
                this.MemberID = Globals.Configuration.PatioOMS.ETS_MemberId;
                this.TraderID = Globals.Configuration.PatioOMS.ETS_TraderId;
				this.SpecialInstructions = string.Empty;
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

            if (this.OrderType != 'N')
            {
                // New Order
                this.DisclosedVolume = 0;
			}




			//OrderStatusNote
            if(this.OrderStatusNote == null)
            {
				this.OrderStatusNote = "XX";/* Return an empty ACC_Description */
			}
			//OrderComment (το συμπιεζουμε λιγο...)
            var _originalComment = this.OrderComment;
			if (_originalComment != null)
            {
                if (_originalComment.StartsWith(@"GALATIA\"))
                    this.OrderComment = _originalComment.Replace(@"GALATIA\", @"GL\");
                else if (_originalComment.StartsWith(@"EXTRANET\"))
                    this.OrderComment = _originalComment.Replace(@"EXTRANET\", @"EXT\");
            }


			/*
             * Τωρα θα φτιαξουμε το OrderNote/tag58 για την νεα εντολη μας
             * Προσοχη, ακομα εχουμε το limit των 25 χαρακτηρων (εξαιτιας του ODL)!
             */
			var _orderNote = _FormatAlphaField(this.OrderStatusNote, 5);
			if (this.VenueID == "XIPO")
            {
				_orderNote = _orderNote + _FormatAlphaField(this.OrderComment, 20);
			}
            else
            {
				if (_originalComment == @"GALATIA\SALESTRADER")
                {
                    _orderNote = _orderNote + _FormatAlphaField(this.PEL_PROF == "ΡΩΩΩ" || this.ClientSalesTraderNoDiscount == 1 ? "~1~" : "~6~", 3);
                    _orderNote = _orderNote + _FormatAlphaField(this.OrderComment, 17);
                }
                else
                {
                    _orderNote = _orderNote + _FormatAlphaField(this.OrderComment, 20);
                }
            }
            this.OrderNote = _orderNote;
        
        }




        public void FormatMessage(FIXMessageWriter writer)
        {
            writer.Clear();

            writer.Set(Tags.ClOrdID, (int)this.OrderID);

            SetPartyIDs(writer);


            writer.Set(Tags.Account, this.CSDAccountID);

            if (this.TradingCapacity == /*Any other capacity*/'2')
            {
                writer.Set(Tags.OrderCapacity, /*Agency (AOTC)*/'A');
            }
            else if (this.TradingCapacity == /*Matched principal*/'1')
            {
                writer.Set(Tags.OrderCapacity, /*Riskless principal (MTCH)*/'R');
            }
            else if (this.TradingCapacity == /*Deal on own account*/'0')
            {
                writer.Set(Tags.OrderCapacity, /*Principal (DEAL)*/'P');
            }
            else
            {
                throw new Exception($"TradingCapacity unsupported '{this.TradingCapacity}'");
            }


            writer.Set(Tags.SecurityID, this.SecurityID);
            writer.Set(Tags.SecurityIDSource, this.SecurityIDSource);
            writer.Set(Tags.SecurityExchange, this.VenueID);

            #region OrdType + TimeInForce + ExpireDate
            if (this.OriginalPriceType == /*OrderPriceTypeEnum.Limit*/'L')
            {
                writer.Set(Tags.OrdType, /*Limit or better (Deprecated)*/'7');
                writer.Set(Tags.Price, this.Price, decimals: 4);
            }
            else if (this.OriginalPriceType == /*OrderPriceTypeEnum.Market*/'M')
            {
                /*
                 * MKT order, no conditions, valid only for current day
                 */
                writer.Set(Tags.OrdType, /*Market*/'1');
            }
            else if (this.OriginalPriceType == /*OrderPriceTypeEnum.AtTheClose*/'C')
            {
                /*
                 * ATC order, no conditions, valid only for current day
                 */
                writer.Set(Tags.OrdType, /*On Close*/'A');
                writer.Set(Tags.TimeInForce, /*Day*/'0');
                writer.Set(Tags.Price, this.Price, decimals: 4);
            }
            else if (this.OriginalPriceType == /*OrderPriceTypeEnum.AtTheOpen*/'O')
            {
                /*
                 * ATO order, no conditions, valid only for current day
                 */
                writer.Set(Tags.OrdType, /*Market*/'1');
                writer.Set(Tags.TimeInForce, /*At the Opening (OPG)*/'2');
                writer.Set(Tags.Price, this.Price, decimals: 4);
            }
            else
            {
                throw new Exception($"OriginalPriceType unsupported '{this.OriginalPriceType}'");
            }

            if (this.OriginalPriceType == 'L' || this.OriginalPriceType == 'M')
            {
                if (this.OrderLifeTime == 'D')
                {
                    writer.Set(Tags.TimeInForce, /*Day (or session)*/'0');
                }
                else if (this.OrderLifeTime == 'C')
                {
                    writer.Set(Tags.TimeInForce, /*Good Till Cancel (GTC)*/'1');
                }
                else if (this.OrderLifeTime == 'E')
                {
                    writer.Set(Tags.TimeInForce, /*Good Till Date (GTD)*/'6');
                    writer.Set(Tags.ExpireDate, this.ExpirationDate);
                }
                else
                {
                    throw new Exception($"OrderLifeTime unsupported '{this.OrderLifeTime}'");
                }
            }
            #endregion

            writer.Set(Tags.Text, this.OrderNote.Trim());

            //StopPx (Tag = 99, Type: Price)
            //CustomTags.StopSymbol
            //CustomTags.StopSymbolType


            writer.Set(Tags.OrderQty, this.Volume);
            if (this.PositionEffect == ' ' || this.PositionEffect == 'O')
            {
                writer.Set(Tags.PositionEffect, 'O');
            }
            else if (this.PositionEffect == 'C')
            {
                writer.Set(Tags.PositionEffect, 'C');
            }
            else
            {
                throw new Exception($"Unknown PositionEffect '{this.PositionEffect}'");
            }

            if (this.SettlType == 'C' || this.SettlType == ' ' || this.SettlType == '0')
            {
                writer.Set(Tags.SettlType, /*Regular*/'0');
            }
            else if (this.SettlType == '1')
            {
                writer.Set(Tags.SettlType, /*Immediate*/'1');
            }
            else
            {
                throw new Exception($"Unknown SettlType '{this.SettlType}'");
            }

            writer.Set(Tags.MaxShow, this.DisclosedVolume);

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

            writer.Set(CustomTags.BoardID, this.BoardID);
            if (this.DirectElectronicAccess == '0')
            {
                writer.Set(CustomTags.OrderOrigination, '0');
            }
            else if (this.DirectElectronicAccess == '1')
            {
                writer.Set(CustomTags.OrderOrigination, '5');
            }
            else
            {
                throw new Exception($"Unknown DirectElectronicAccess '{this.Side}'");
            }

            #region OrderAttributes
            writer.Set(CustomTags.NoOrderAttributes, 2);

            writer.Set(CustomTags.OrderAttributeType, 4);//Algorithmic order
            if (this.AlgoFlag == 'Y')
            {
                writer.Set(CustomTags.OrderAttributeValue, 'Y');
            }
            else if (this.AlgoFlag == 'N')
            {
                writer.Set(CustomTags.OrderAttributeValue, 'N');
            }
            else
            {
                throw new Exception($"Unknown AlgoFlag '{this.AlgoFlag}'");
            }


            writer.Set(CustomTags.OrderAttributeType, 2);//Liquidity provision activity order
            if (this.LiquidityProvision == '0')
            {
                writer.Set(CustomTags.OrderAttributeValue, 'N');
            }
            else if (this.LiquidityProvision == '1')
            {
                writer.Set(CustomTags.OrderAttributeValue, 'Y');
            }
            else
            {
                throw new Exception($"Unknown LiquidityProvision '{this.LiquidityProvision}'");
            }
            #endregion

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
            writer.Set(Tags.PartyID, this.ClearingMemberID);
            writer.Set(Tags.PartyIDSource, 'D');
            writer.Set(Tags.PartyRole, 4);


            /*Client ID - (MIFID II: Client identification code)*/
            if (string.IsNullOrEmpty(this.KemRequesterAseCode) != true)
            {
                /*
                 * H εντολη προηλθε απο καποιον εντολέα για καποιο KEM Account.
                 * Σαν ClientID στελνουμε τον κωδικο του εντολεα:
                 */
                writer.Set(Tags.PartyID, this.KemRequesterAseCode);
                writer.Set(Tags.PartyIDSource, 'P');
                writer.Set(Tags.PartyRole, 3);
                writer.Set(CustomTags.PartyRoleQualifier, /*Natural Person*/24);
            }
            else
            {
                /*
                 * Στελνουμε το ClientID μας...
                 */
                writer.Set(Tags.PartyID, (int)this.ClientID);
                writer.Set(Tags.PartyIDSource, 'P');
                writer.Set(Tags.PartyRole, 3);
                if (this.ClientIDQualifier == 'L')
                    writer.Set(CustomTags.PartyRoleQualifier, /*Firm or Legal Entity*/23);
                else
                    writer.Set(CustomTags.PartyRoleQualifier, /*Natural Person*/24);
            }



            /*Investment Decision Maker - (MIFID II: Investment decision within firm)*/
            writer.Set(Tags.PartyID, (int)this.InvestmentDecisionID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, /*Investment Decision Maker - (MIFID II: Investment decision within firm)*/122);
            if (this.InvestmentDecisionID != 0)
            {
                if (this.InvestmentDecisionIDQualifier == 'A')
                    writer.Set(CustomTags.PartyRoleQualifier, 22);
                else
                    writer.Set(CustomTags.PartyRoleQualifier, 24);
            }



            /*Executing Trader - (MIFID II: Execution within firm)*/
            writer.Set(Tags.PartyID, (int)this.ExecutionWithinFirmID);
            writer.Set(Tags.PartyIDSource, 'P');
            writer.Set(Tags.PartyRole, /*Executing Trader - (MIFID II: Execution within firm)*/12);
            //if (this.ExecutionWithinFirmIDQualifier == 'A')
            //    writer.Set(CustomTags.PartyRoleQualifier, 22);
            //else
            //    writer.Set(CustomTags.PartyRoleQualifier, 24);



            /*Correspondent Broker - (MIFID II: Non-executing broker)*/
            writer.Set(Tags.PartyID, (int)this.NonExecutingBrokerID);
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
