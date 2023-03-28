using PatioFIX.Common.BLL.Messages;
using System;
using System.Data;
using static PatioFIX.Common.ODLClientAPIUtilities;



namespace PatioFIX.Common.DAL
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OdlDataLayer : BaseDataLayer, IOdlDataLayer
    {
        readonly Logger theLogger = null;

        public OdlDataLayer() : base(Globals.PatioOMS.ODLConnStr)
        {
            theLogger = new Logger("OdlDataLayer");
        }


        #region called by Evaluators
        public void InsertIgnored(FIXInMessage message, IgnoredMessage ignoredMessage)
        {
            if (ignoredMessage == null) throw new ArgumentNullException(nameof(ignoredMessage));

            var command = CreateCommandForProc("dbo.fxodl_ignored_Create");
            AddParameter(command, "@messageType", ignoredMessage.ODLMessageType, SqlDbType.Char, ParameterDirection.Input, 2);

            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);

            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertConfirmCancel(FIXInMessage message, OrderEditConfirmationMessage confirmation)
        {
            var command = CreateCommandForProc("dbo.fxodl_confirmcancel_Create");
            AddParameter(command, "@cfcMemberID", _trim(confirmation.MemberID), SqlDbType.Char);
            AddParameter(command, "@cfcTraderID", _trim(confirmation.TraderID), SqlDbType.Char);
            AddParameter(command, "@cfcBoardID", confirmation.BoardID, SqlDbType.Char);
            AddParameter(command, "@clOrdID", _RemoveSpecialCharacters(confirmation.ClOrdID), SqlDbType.Char);
            AddParameter(command, "@cfcCSDAccountID", _ReturnPureField(confirmation.CSDAccountID), SqlDbType.Char);
            AddParameter(command, "@cfcOrderNumber", confirmation.OrderNumber, SqlDbType.Char);
            AddParameter(command, "@cfcEntryDate", confirmation.OrderDate, SqlDbType.Char);
            AddParameter(command, "@cfcSource", confirmation.CancelSource, SqlDbType.Char);
            AddParameter(command, "@cfcReasonCode", confirmation.CancelReasonCode, SqlDbType.Char);
            AddParameter(command, "@cfcTime", confirmation.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@cfcVenueId", confirmation.SecurityExchange, SqlDbType.Char);
            AddParameter(command, "@cfcLeavesQuantity", confirmation.LeavesQuantity, SqlDbType.Decimal);
            AddParameter(command, "@cfcAveragePrice", confirmation.AveragePrice, SqlDbType.Decimal);
            AddParameter(command, "@cfcEditType", confirmation.EditType, SqlDbType.Char);
            AddParameter(command, "@cfcCurrentCreditValue", confirmation.CurrentCreditValue, SqlDbType.Decimal);
            AddParameter(command, "@origClOrdID", _RemoveSpecialCharacters(confirmation.OrigClOrdID), SqlDbType.VarChar);
            AddParameter(command, "@cfcSecurityID", confirmation.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@cfcSecurityIDSource", confirmation.SecurityIDSource, SqlDbType.Char);
            AddParameter(command, "@cfcCurrency", confirmation.Currency, SqlDbType.Char);
            AddParameter(command, "@cfcExpirationDate", confirmation.ExpirationDate, SqlDbType.Char);
            AddParameter(command, "@ODLOrderStatus", confirmation.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", confirmation.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@DisclosedVolume", confirmation.DisclosedVolume, SqlDbType.Decimal);
            AddParameter(command, "@cfcOrderNote", _RemoveSpecialCharacters(confirmation.OrderNote), SqlDbType.VarChar);
            AddParameter(command, "@cfcListID", confirmation.ListID, SqlDbType.Char);
            AddParameter(command, "@cfcOriCSDAccountID", confirmation.CSDAccountID, SqlDbType.Char);
            AddParameter(command, "@cfcTimeStamp", confirmation.Timestamp, SqlDbType.Char);
            AddParameter(command, "@exchangeOrderID", confirmation.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertConfirmChange(FIXInMessage message, OrderChangeConfirmationMessage confirmation, string strOrderNotes)
        {
            var command = CreateCommandForProc("dbo.fxodl_confirmchange_Create");
            AddParameter(command, "@p03", confirmation.MemberID, SqlDbType.Char);//cfhMemberID
            AddParameter(command, "@p04", confirmation.TraderID, SqlDbType.Char);
            AddParameter(command, "@p06", confirmation.BoardID, SqlDbType.Char);
            AddParameter(command, "@cfhOrderNumber", confirmation.OrderNumber, SqlDbType.Char);
            AddParameter(command, "@p08", confirmation.OrderDate, SqlDbType.Char);//cfhOrderEntryDate
            AddDecimalParameter(command, "@p09", confirmation.ChangedPrice, ParameterDirection.Input, 18, 6);
            AddDecimalParameter(command, "@p10", confirmation.ChangedVolume, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@p11", confirmation.ChangedDisclosedVolume, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@p12", confirmation.ChangedAutoDisclosedVolume, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p13", _ReturnPureField(confirmation.ChangedCSDAccountID), SqlDbType.Char);
            AddParameter(command, "@p14", confirmation.ChangedOriginalPriceType, SqlDbType.Char);
            AddParameter(command, "@p15", confirmation.ChangedLife, SqlDbType.Char);
            AddParameter(command, "@p16", confirmation.ChangedExpirationDate, SqlDbType.Char);
            AddParameter(command, "@origClOrdID", confirmation.OrigClOrdID, SqlDbType.Char);
            AddParameter(command, "@p18", _RemoveSpecialCharacters(strOrderNotes), SqlDbType.Char);//cfhChangedOrderNote
            AddParameter(command, "@p19", confirmation.ChangedClearingMemberID, SqlDbType.Char);
            AddParameter(command, "@origSource", confirmation.OrigSource, SqlDbType.Char);
            AddParameter(command, "@ODLOrderStatus", confirmation.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", confirmation.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@p22", confirmation.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@cfhListId", confirmation.ListID, SqlDbType.Char);
            AddParameter(command, "@cfhVenueId", confirmation.VenueID, SqlDbType.Char);
            AddDecimalParameter(command, "@p25", confirmation.LeavesQuantity, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@p26", confirmation.AveragePrice, ParameterDirection.Input, 18, 6);
            AddDecimalParameter(command, "@p27", confirmation.CurrentCreditValue, ParameterDirection.Input, 18, 2);
            AddParameter(command, "@cfhSecurityID", confirmation.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@p29", confirmation.SecurityIDSource, SqlDbType.Char);//cfhSecurityIDSource
            AddParameter(command, "@cfhCurrency", confirmation.Currency, SqlDbType.Char);
            AddParameter(command, "@p31", confirmation.ChangedShortSellFlag, SqlDbType.Char);
            AddParameter(command, "@p32", confirmation.ChangedGOIFlag, SqlDbType.Char);
            AddParameter(command, "@clOrdID", confirmation.ClOrdID, SqlDbType.VarChar);
            AddParameter(command, "@p34", confirmation.ChangedPositionEffect, SqlDbType.Char);
            AddParameter(command, "@p35", confirmation.ChangedSettlType, SqlDbType.Char);
            AddParameter(command, "@p36", confirmation.SpecialConditions, SqlDbType.Char);
            AddParameter(command, "@p37", confirmation.ChangedCSDAccountID, SqlDbType.Char);
            AddParameter(command, "@p38", confirmation.ChangedDirectElectronicAccess, SqlDbType.Char);
            AddDecimalParameter(command, "@cfhChangedClientID", confirmation.ClientID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p40", confirmation.ClientIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p41", confirmation.InvestmentDecisionID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p42", confirmation.InvestmentDecisionIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p43", confirmation.ExecutionWithinFirmID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p44", confirmation.ExecutionWithinFirmIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p45", confirmation.NonExecutingBrokerID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@cfhTimeStamp", confirmation.Timestamp, SqlDbType.Char);
            AddParameter(command, "@p47", confirmation.ChangedSpecialInstructions, SqlDbType.NVarChar);
            AddParameter(command, "@p48", confirmation.ChangedCommodityHedgingFlag, SqlDbType.Char);
            AddParameter(command, "@exchangeOrderID", confirmation.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertConfirmOrder(FIXInMessage message, OrderEntryConfirmationMessage confirmOrder, string orderNotes)
        {
            if (confirmOrder == null) throw new ArgumentNullException(nameof(confirmOrder));

            var command = CreateCommandForProc("dbo.fxodl_confirmorder_Create");
            AddParameter(command, "@MemberID", _trim(confirmOrder.MemberID), SqlDbType.Char);
            AddParameter(command, "@TraderID", _trim(confirmOrder.TraderID), SqlDbType.Char);
            AddParameter(command, "@OrderType", confirmOrder.OrderType, SqlDbType.Char);
            AddParameter(command, "@BoardID", confirmOrder.BoardID, SqlDbType.Char);
            AddParameter(command, "@Side", confirmOrder.Side, SqlDbType.Char);
            AddParameter(command, "@CSDAccountID", _ReturnPureField(confirmOrder.CSDAccountID), SqlDbType.Char);
            AddParameter(command, "@ClientFlag", confirmOrder.OrderSource, SqlDbType.Char);
            AddParameter(command, "@Price", confirmOrder.Price, SqlDbType.Decimal);
            AddParameter(command, "@Volume", confirmOrder.Volume, SqlDbType.Decimal);
            AddParameter(command, "@p13", confirmOrder.DisclosedVolume, SqlDbType.Decimal);
            AddParameter(command, "@p14", confirmOrder.AutoDisclosedVolume, SqlDbType.Decimal);
            AddParameter(command, "@p15", confirmOrder.ConditionVolume, SqlDbType.Decimal);
            AddParameter(command, "@p16", confirmOrder.OrderLifetime, SqlDbType.Char);
            AddParameter(command, "@p17", confirmOrder.SpecialConditions, SqlDbType.Char);
            AddParameter(command, "@p18", confirmOrder.OriginalPriceType, SqlDbType.Char);
            AddParameter(command, "@p19", confirmOrder.ExpirationDate, SqlDbType.Char);
            AddParameter(command, "@clOrdID", _RemoveSpecialCharacters(confirmOrder.ClOrdID), SqlDbType.Char);
            AddParameter(command, "@Note", _RemoveSpecialCharacters(orderNotes), SqlDbType.Char);
            AddParameter(command, "@p22", confirmOrder.ClearingMemberID, SqlDbType.Char);
            AddParameter(command, "@p23", confirmOrder.OrderSource, SqlDbType.Char);
            AddParameter(command, "@p24", confirmOrder.OrderNumber, SqlDbType.Char);//cfoNewOrderNumber
            AddParameter(command, "@p25", confirmOrder.OrderDate, SqlDbType.Char);//cfoNewOrderDate
            AddParameter(command, "@ODLOrderStatus", confirmOrder.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", confirmOrder.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@Time", confirmOrder.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@VenueId", confirmOrder.VenueID, SqlDbType.Char);
            AddParameter(command, "@LeavesQuantity", confirmOrder.LeavesQuantity, SqlDbType.Decimal);
            AddParameter(command, "@AveragePrice", confirmOrder.AveragePrice, SqlDbType.Decimal);
            AddParameter(command, "@p31", confirmOrder.CurrentCreditValue, SqlDbType.Decimal);
            AddParameter(command, "@ListId", confirmOrder.ListID, SqlDbType.Char);
            AddParameter(command, "@GOIFlag", confirmOrder.GOIFlag, SqlDbType.Char);
            AddParameter(command, "@ShortSellFlag", confirmOrder.ShortSellFlag, SqlDbType.Char);
            AddParameter(command, "@SecurityID", confirmOrder.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@p36", confirmOrder.SecurityIDSource, SqlDbType.Char);
            AddParameter(command, "@Currency", confirmOrder.Currency, SqlDbType.Char);
            AddParameter(command, "@p38", confirmOrder.StopSecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@StopPrice", confirmOrder.StopPrice, SqlDbType.Decimal);
            AddParameter(command, "@PositionEffect", confirmOrder.PositionEffect, SqlDbType.Char);
            AddParameter(command, "@SettlType", confirmOrder.SettlType, SqlDbType.Char);
            AddParameter(command, "@p42", confirmOrder.CSDAccountID, SqlDbType.Char);
            AddParameter(command, "@p43", confirmOrder.DirectElectronicAccess, SqlDbType.Char);
            AddDecimalParameter(command, "@ClientID", confirmOrder.ClientID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p45", confirmOrder.ClientIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p46", confirmOrder.InvestmentDecisionID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p47", confirmOrder.InvestmentDecisionIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p48", confirmOrder.ExecutionWithinFirmID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p49", confirmOrder.ExecutionWithinFirmIDQualifier, SqlDbType.Char);
            AddDecimalParameter(command, "@p50", confirmOrder.NonExecutingBrokerID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p51", confirmOrder.TradingCapacity, SqlDbType.Char);
            AddParameter(command, "@p52", confirmOrder.LiquidityProvision, SqlDbType.Char);
            AddParameter(command, "@TimeStamp", confirmOrder.Timestamp, SqlDbType.Char);
            AddParameter(command, "@p54", _RemoveSpecialCharacters(confirmOrder.SpecialInstructions), SqlDbType.NVarChar);
            AddParameter(command, "@AlgoFlag", confirmOrder.AlgoFlag, SqlDbType.Char);
            AddParameter(command, "@p56", confirmOrder.CommodityHedgingFlag, SqlDbType.Char);
            AddParameter(command, "@exchangeOrderID", confirmOrder.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }

        }
        public void InsertConfirmOrderEdit(FIXInMessage message, OrderEditConfirmationMessage confirmation)
        {
            var command = CreateCommandForProc("dbo.fxodl_confirmorderedit_Create");
            AddParameter(command, "@cfcMemberID", _trim(confirmation.MemberID), SqlDbType.Char);
            AddParameter(command, "@cfcTraderID", _trim(confirmation.TraderID), SqlDbType.Char);
            AddParameter(command, "@cfcBoardID", confirmation.BoardID, SqlDbType.Char);
            AddParameter(command, "@clOrdID", _RemoveSpecialCharacters(confirmation.ClOrdID), SqlDbType.Char);
            AddParameter(command, "@cfcCSDAccountID", _ReturnPureField(confirmation.CSDAccountID), SqlDbType.Char);
            AddParameter(command, "@cfcOrderNumber", confirmation.OrderNumber, SqlDbType.Char);
            AddParameter(command, "@cfcEntryDate", confirmation.OrderDate, SqlDbType.Char);
            AddParameter(command, "@cfcSource", confirmation.CancelSource, SqlDbType.Char);
            AddParameter(command, "@cfcReasonCode", confirmation.CancelReasonCode, SqlDbType.Char);
            AddParameter(command, "@cfcTime", confirmation.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@cfcVenueId", confirmation.SecurityExchange, SqlDbType.Char);
            AddParameter(command, "@cfcLeavesQuantity", confirmation.LeavesQuantity, SqlDbType.Decimal);
            AddParameter(command, "@cfcAveragePrice", confirmation.AveragePrice, SqlDbType.Decimal);
            AddParameter(command, "@cfcEditType", confirmation.EditType, SqlDbType.Char);
            AddParameter(command, "@cfcCurrentCreditValue", confirmation.CurrentCreditValue, SqlDbType.Decimal);
            AddParameter(command, "@origClOrdID", _RemoveSpecialCharacters(confirmation.OrigClOrdID), SqlDbType.VarChar);
            AddParameter(command, "@cfcSecurityID", confirmation.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@cfcSecurityIDSource", confirmation.SecurityIDSource, SqlDbType.Char);
            AddParameter(command, "@cfcCurrency", confirmation.Currency, SqlDbType.Char);
            AddParameter(command, "@cfcExpirationDate", confirmation.ExpirationDate, SqlDbType.Char);
            AddParameter(command, "@ODLOrderStatus", confirmation.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", confirmation.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@DisclosedVolume", confirmation.DisclosedVolume, SqlDbType.Decimal);
            AddParameter(command, "@cfcOrderNote", _RemoveSpecialCharacters(confirmation.OrderNote), SqlDbType.VarChar);
            AddParameter(command, "@cfcListID", confirmation.ListID, SqlDbType.Char);
            AddParameter(command, "@cfcOriCSDAccountID", confirmation.CSDAccountID, SqlDbType.Char);
            AddParameter(command, "@cfcTimeStamp", confirmation.Timestamp, SqlDbType.Char);
            AddParameter(command, "@exchangeOrderID", confirmation.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertCreditLimitInformation(FIXInMessage message, CreditLimitInfoMessage creditLimitInfo)
        {
            if (creditLimitInfo == null) throw new ArgumentNullException(nameof(creditLimitInfo));

            var command = CreateCommandForProc("dbo.fxodl_creditlimitinformation_Create");
            AddParameter(command, "@memberId", creditLimitInfo.MemberID, SqlDbType.Char, ParameterDirection.Input, 4);
            AddDecimalParameter(command, "@creditLimit", creditLimitInfo.CreditLimit, ParameterDirection.Input, 14, 2);
            AddParameter(command, "@clearingSpace", _trim(creditLimitInfo.ClearingSpace), SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@clearingSubAccountID", _trim(creditLimitInfo.ClearingSubAccountID), SqlDbType.Char, ParameterDirection.Input, 4);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertExchangeNotes(FIXInMessage message, ExchangeNotesMessage exchangeNotes)
        {
            if (exchangeNotes == null) throw new ArgumentNullException(nameof(exchangeNotes));

            var command = CreateCommandForProc("dbo.fxodl_exchangenotes_Create");

            AddParameter(command, "@memberID", exchangeNotes.MemberID, SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@traderID", exchangeNotes.TraderID, SqlDbType.Char, ParameterDirection.Input, 5);
            AddParameter(command, "@noteType", exchangeNotes.NoteType, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@messageNote", _trim(exchangeNotes.MessageNote), SqlDbType.NVarChar, ParameterDirection.Input, 50);
            AddParameter(command, "@transPerSecond", exchangeNotes.TransPerSecond.ToString(), SqlDbType.Char, ParameterDirection.Input, 5);
            AddParameter(command, "@outstandingMsgs", exchangeNotes.OutstandingMsgs.ToString(), SqlDbType.Char, ParameterDirection.Input, 5);
            AddParameter(command, "@exchangeId", (char)exchangeNotes.ExchangeId, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@timestamp", exchangeNotes.Timestamp, SqlDbType.Char, ParameterDirection.Input, 20);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        public void InsertOrderMarketStatus(FIXInMessage message, MarketStatusMessage marketStatus)
        {
            var command = CreateCommandForProc("dbo.fxodl_ordermarketstatus_Create");
            AddParameter(command, "@marStatMarketID", marketStatus.MarketID, SqlDbType.Char);
            AddParameter(command, "@marStatBoardID", marketStatus.BoardID, SqlDbType.Char);
            AddParameter(command, "@marStatStatus", marketStatus.TradingSessionID, SqlDbType.Char);
            AddParameter(command, "@marVenueId", marketStatus.SecurityExchange, SqlDbType.Char);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        public void InsertReject(FIXInMessage message, string memberOrderID, RejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator)
        {
            var command = CreateCommandForProc("dbo.fxodl_reject_Create");
            AddParameter(command, "@rejMemberID", rejection.MemberID, SqlDbType.Char);
            AddParameter(command, "@rejTraderID", rejection.TraderID, SqlDbType.Char);
            AddParameter(command, "@rejRejectReasonCode", rejection.RejectReasonCode, SqlDbType.Char);
            AddParameter(command, "@rejTime", rejection.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@rejMemberOrderNumber", memberOrderID, SqlDbType.Char);
            AddParameter(command, "@rejVenueID", _trim(rejection.SecurityExchange), SqlDbType.Char);
            AddParameter(command, "@ODLOrderStatus", rejection.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", rejection.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@rejSourceMsgType", sourceMsgType, SqlDbType.Char);
            AddParameter(command, "@rejTimeStamp", _trim(rejection.Timestamp), SqlDbType.Char);
            //
            AddParameter(command, "@securityID", rejection.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@orderNote", _RemoveSpecialCharacters(orderNote), SqlDbType.Char);
            AddParameter(command, "@csdAccountID", rejection.CSDAccountID, SqlDbType.Char);
            AddParameter(command, "@generator", (short)generator, SqlDbType.SmallInt);

            AddParameter(command, "@rejReason", rejection.RejectReason, SqlDbType.VarChar);
            AddParameter(command, "@clOrdID", rejection.ClOrdID, SqlDbType.VarChar);
            AddParameter(command, "@origClOrdID", rejection.OrigClOrdID, SqlDbType.VarChar);
            AddParameter(command, "@exchangeOrderID", rejection.ExchangeOrderID, SqlDbType.VarChar);

            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertReject(FIXInMessage message, OrderCancelRejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator)
        {
            var command = CreateCommandForProc("dbo.fxodl_reject_Create");
            AddParameter(command, "@rejMemberID", rejection.MemberID, SqlDbType.Char);
            AddParameter(command, "@rejTraderID", rejection.TraderID, SqlDbType.Char);
            AddParameter(command, "@rejRejectReasonCode", rejection.RejectReasonCode, SqlDbType.Char);
            AddParameter(command, "@rejTime", rejection.Timestamp.Substring(8, 8), SqlDbType.Char);
            AddParameter(command, "@rejMemberOrderNumber", _RemoveSpecialCharacters(rejection.MemberOrderNumber), SqlDbType.Char);
            AddParameter(command, "@rejVenueID", _trim(rejection.SecurityExchange), SqlDbType.Char);
            AddParameter(command, "@ODLOrderStatus", rejection.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", rejection.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@rejSourceMsgType", sourceMsgType, SqlDbType.Char);
            AddParameter(command, "@rejTimeStamp", _trim(rejection.Timestamp), SqlDbType.Char);
            //
            AddParameter(command, "@securityID", rejection.SecurityID, SqlDbType.NVarChar);
            AddParameter(command, "@orderNote", _RemoveSpecialCharacters(orderNote), SqlDbType.Char);
            AddParameter(command, "@csdAccountID", rejection.CSDAccountID, SqlDbType.Char);
            AddParameter(command, "@generator", (short)generator, SqlDbType.SmallInt);

            AddParameter(command, "@rejReason", rejection.RejectReason, SqlDbType.VarChar);
            AddParameter(command, "@clOrdID", rejection.ClOrdID, SqlDbType.VarChar);
            AddParameter(command, "@origClOrdID", rejection.OrigClOrdID, SqlDbType.VarChar);
            AddParameter(command, "@exchangeOrderID", rejection.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }


        public void InsertSecurityPrice(FIXInMessage message, SecurityPricesMessage securityPrice)
        {
            if (securityPrice == null) throw new ArgumentNullException(nameof(securityPrice));

            var command = CreateCommandForProc("dbo.fxodl_securityprices_Create");
            AddDecimalParameter(command, "@secPriceStartOfDayPrice", securityPrice.StartOfDayPrice, ParameterDirection.Input, 18, 6);
            AddDecimalParameter(command, "@secPriceFloorPrice", securityPrice.FloorPrice, ParameterDirection.Input, 18, 6);
            AddDecimalParameter(command, "@secPriceCeillingPrice", securityPrice.CeilingPrice, ParameterDirection.Input, 18, 6);
            AddParameter(command, "@secPriceVenueid", securityPrice.SecurityExchange, SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@secSecuritySymbol", securityPrice.SecurityID, SqlDbType.NVarChar, ParameterDirection.Input, 20);
            AddParameter(command, "@secSecurityCode", _trim(securityPrice.SecurityCode), SqlDbType.NVarChar, ParameterDirection.Input, 20);
            AddDecimalParameter(command, "@secAccruedInterest", securityPrice.AccruedInterest, ParameterDirection.Input, 18, 6);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertSecurityStatus(FIXInMessage message, SecurityStatusMessage securityStatus)
        {
            if (securityStatus == null) throw new ArgumentNullException(nameof(securityStatus));

            var command = CreateCommandForProc("dbo.fxodl_securitystatus_Create");
            AddParameter(command, "@secStatMarketID", securityStatus.MarketID, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@secStatSecurityStatus", securityStatus.SecurityStatus, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@secStatPhaseID", securityStatus.PhaseID, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@secStatSecurityPrice", securityStatus.SecurityPrice, ParameterDirection.Input, 18, 6);
            AddParameter(command, "@secStatHaltReasonCode", _trim(securityStatus.HaltReasonCode), SqlDbType.Char, ParameterDirection.Input, 2);
            AddParameter(command, "@secStatHaltStartTime", _trim(securityStatus.HaltStartTime), SqlDbType.Char, ParameterDirection.Input, 12);
            AddParameter(command, "@secStatVenueId", _trim(securityStatus.SecurityExchange), SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@secStatSecuritySymbol", _trim(securityStatus.SecurityID), SqlDbType.NVarChar, ParameterDirection.Input, 20);
            AddParameter(command, "@secStatSecurityCode", _trim(securityStatus.SecurityCode), SqlDbType.NVarChar, ParameterDirection.Input, 20);
            AddParameter(command, "@secStatTimeStamp", securityStatus.Timestamp, SqlDbType.Char, ParameterDirection.Input, 20);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
        public void InsertTrades(FIXInMessage message, NewTradeConfirmationMessage newTradeConfirmation)
        {
            if (newTradeConfirmation == null) throw new ArgumentNullException(nameof(newTradeConfirmation));

            var command = CreateCommandForProc("dbo.fxodl_trades_Create");
            AddParameter(command, "@p02", "", SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@p04", _trim(newTradeConfirmation.MemberID), SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@p05", _trim(newTradeConfirmation.TraderID), SqlDbType.Char, ParameterDirection.Input, 5);
            AddParameter(command, "@p07", newTradeConfirmation.BoardID, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p08", _trim(_RemoveSpecialCharacters(newTradeConfirmation.ClientOrderID)), SqlDbType.Char, ParameterDirection.Input, 16);
            AddParameter(command, "@p09", _ReturnPureField(newTradeConfirmation.CSDAccountID), SqlDbType.Char, ParameterDirection.Input, 12);
            AddParameter(command, "@p10", _trim(newTradeConfirmation.ClearingMemberID), SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@OrderSource", newTradeConfirmation.OrderSource, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@ODLOrderStatus", newTradeConfirmation.ODLOrderStatus, SqlDbType.Char);
            AddParameter(command, "@FIXOrderStatus", newTradeConfirmation.FixOrderStatus, SqlDbType.Char);
            AddParameter(command, "@p12", _trim(newTradeConfirmation.OrderNumber), SqlDbType.Char, ParameterDirection.Input, 8);
            AddParameter(command, "@p13", _trim(newTradeConfirmation.OrderDate), SqlDbType.Char, ParameterDirection.Input, 8);
            AddParameter(command, "@Side", newTradeConfirmation.Side, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@p15", newTradeConfirmation.Volume, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@p16", newTradeConfirmation.Price, ParameterDirection.Input, 18, 6);
            AddParameter(command, "@p17", _trim(newTradeConfirmation.ContraMemberID), SqlDbType.Char, ParameterDirection.Input, 4);
            AddParameter(command, "@p18", _trim(newTradeConfirmation.TradeNumber), SqlDbType.Char, ParameterDirection.Input, 6);
            AddParameter(command, "@p19", _trim(_substring(newTradeConfirmation.Timestamp, 8, 8)), SqlDbType.Char, ParameterDirection.Input, 8);
            AddParameter(command, "@p20", newTradeConfirmation.TradeSource, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@PhaseID", newTradeConfirmation.PhaseID, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p22", newTradeConfirmation.SecurityStatus, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p23", _trim(newTradeConfirmation.TradeType), SqlDbType.Char, ParameterDirection.Input, 2);
            AddParameter(command, "@p24", _trim(newTradeConfirmation.TradeStatus), SqlDbType.Char, ParameterDirection.Input, 2);
            AddParameter(command, "@VenueId", _trim(newTradeConfirmation.VenueID), SqlDbType.Char, ParameterDirection.Input, 4);
            AddDecimalParameter(command, "@p26", newTradeConfirmation.LeavesQuantity, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@p27", newTradeConfirmation.AveragePrice, ParameterDirection.Input, 18, 6);
            AddDecimalParameter(command, "@p28", newTradeConfirmation.CurrentCreditValue, ParameterDirection.Input, 18, 2);
            AddParameter(command, "@p29", _trim(newTradeConfirmation.ListID), SqlDbType.Char, ParameterDirection.Input, 6);
            AddParameter(command, "@OrderRelFlag", newTradeConfirmation.OrderRelFlag, SqlDbType.Char, ParameterDirection.Input, 1);


            AddParameter(command, "@GOIFlag", newTradeConfirmation.GOIFlag, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p32", newTradeConfirmation.ShortSellFlag, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p33", newTradeConfirmation.PositionEffect, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p34", newTradeConfirmation.SettlType, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p35", _trim(newTradeConfirmation.ExpirationDate), SqlDbType.Char, ParameterDirection.Input, 8);
            AddParameter(command, "@p36", _trim(newTradeConfirmation.TradeStatus), SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@SecurityID", _trim(newTradeConfirmation.SecurityID), SqlDbType.NVarChar, ParameterDirection.Input, 20);
            AddParameter(command, "@p38", newTradeConfirmation.SecurityIDSource, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p39", _trim(newTradeConfirmation.Currency), SqlDbType.Char, ParameterDirection.Input, 3);
            AddParameter(command, "@p40", _trim(newTradeConfirmation.ContraTraderID), SqlDbType.Char, ParameterDirection.Input, 5);

            AddParameter(command, "@OrderRefID", newTradeConfirmation.OrderRefID, SqlDbType.Char, ParameterDirection.Input, 16);

            AddParameter(command, "@p42", newTradeConfirmation.LastLiquidityIndicator, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p43", _trim(_RemoveSpecialCharacters(newTradeConfirmation.CSDAccountID)), SqlDbType.Char, ParameterDirection.Input, 12);
            AddDecimalParameter(command, "@p44", newTradeConfirmation.NotionalAmmount, ParameterDirection.Input, 18, 2);
            AddParameter(command, "@p45", newTradeConfirmation.DirectElectronicAccess, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@ClientID", newTradeConfirmation.ClientID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p47", newTradeConfirmation.ClientIDQualifier, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@p48", newTradeConfirmation.InvestmentDecisionID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p49", newTradeConfirmation.InvestmentDecisionIDQualifier, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@p50", newTradeConfirmation.ExecutionWithinFirmID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p51", newTradeConfirmation.ExecutionWithinFirmIDQualifier, SqlDbType.Char, ParameterDirection.Input, 1);
            AddDecimalParameter(command, "@p52", newTradeConfirmation.NonExecutingBrokerID, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p53", newTradeConfirmation.TradingCapacity, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p54", newTradeConfirmation.LiquidityProvision, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p55", _trim(newTradeConfirmation.WaiverIndicator), SqlDbType.Int);
            AddDecimalParameter(command, "@p56", newTradeConfirmation.BestBidPrice, ParameterDirection.Input, 18, 4);
            AddDecimalParameter(command, "@p57", newTradeConfirmation.BestBidQuantity, ParameterDirection.Input, 18, 0);
            AddDecimalParameter(command, "@BestOfferPrice", newTradeConfirmation.BestOfferPrice, ParameterDirection.Input, 18, 4);
            AddDecimalParameter(command, "@BestOfferQuantity", newTradeConfirmation.BestOfferQuantity, ParameterDirection.Input, 18, 0);
            AddParameter(command, "@p60", newTradeConfirmation.Timestamp, SqlDbType.Char, ParameterDirection.Input, 20);
            AddParameter(command, "@AlgoFlag", newTradeConfirmation.AlgoFlag, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@p62", newTradeConfirmation.CommodityHedgingFlag, SqlDbType.Char, ParameterDirection.Input, 1);
            AddParameter(command, "@exchangeOrderID", newTradeConfirmation.ExchangeOrderID, SqlDbType.VarChar);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);

            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }


        public void UpdateOrderProcessAndStatusCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode, char ordStatus, string rejReasCode = default)
        {
            var command = CreateCommandForProc("dbo.fxodl_Orders_UpdateStatuses");
            AddParameter(command, "@orderID", orderId, SqlDbType.Int);
            AddParameter(command, "@processCode", (int)processCode, SqlDbType.Int);

            AddParameter(command, "@statusCode", ordStatus, SqlDbType.Char);


            AddParameter(command, "@rejectReasonCode", rejReasCode, SqlDbType.Char);
            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        public void UpdateOrderProcessCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode)
        {
            var command = CreateCommandForProc("dbo.fxodl_Orders_UpdateProcessCode");
            AddParameter(command, "@orderID", orderId, SqlDbType.Int);
            AddParameter(command, "@processCode", (int)processCode, SqlDbType.Int);

            /*Common Parameters:*/
            AddParameter(command, "@appMsgId", message.AppMsgID, SqlDbType.Int);
            AddParameter(command, "@msgSeqNum", message.MsgSeqNum, SqlDbType.Int);
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@msgSource", (byte)message.Source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", Globals.DayOfYear, SqlDbType.Int);
            AddParameter(command, "@ATHEXServer", (byte)message.ATHEXServer, SqlDbType.TinyInt);


            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }


        public PtCancel GetCancelById(int cancelID)
        {
            var cmd = CreateCommandForSql("select CancelID,CancelMemberOrderNumber,CancelOrderNumber,CancelOrderDate,CancelProcessCode,ord.UserAseCode,CancelVenueId,CancelSecuritySymbol,CancelMemberID,CancelTraderID from [dbo].[Cancels] left join [dbo].[Orders] as ord on ord.OrderID = CancelMemberOrderNumber where CancelID = @cancelID");
            AddParameter(cmd, "@cancelID", cancelID, SqlDbType.Int);

            PtCancel _cancel = null;
            try
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == true)
                    {
                        reader.Read();

                        _cancel = new PtCancel(reader);
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            return _cancel;
        }
        public PtChange GetChangeById(int changeID)
        {
            var cmd = CreateCommandForSql("select [ChngID],[ChngOrderNumber],[ChngOrderEntryDate],[ChngChangedPrice],[ChngChangedVolume],[ChngChangedCSDAccountID],[ChngChangedOriginalPriceType],[ChngChangedLife],[ChngChangedExpirationDate],[ChngChangedMemberOrderNumber],[ChngProcessCode],[WorkingDate],[ChngVenueId],[ChngSecuritySymbol],[ChngSecurityIDSource],[ChngMemberID],[ChngTraderID] from changes where ChngID = @chngID");
            AddParameter(cmd, "@chngID", changeID, SqlDbType.Int);

            PtChange _change = null;
            try
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == true)
                    {
                        reader.Read();

                        _change = new PtChange(reader);
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            return _change;
        }

        public PtOrder GetOrderById(int orderID)
        {
            var cmd = CreateCommandForSql("select [OrderID],[LastClOrdID],[ExchangeOrderID],[OrderSecuritySymbol],[OrderQuantity],[OrderPrice],[OrderSide],[OrderProcessCode],[OrderStatusCode],[OrderComment],[UserAseCode],[VenueId], [OrderSecurityIDSource] from Orders where OrderID = @orderID");
            AddParameter(cmd, "@orderID", orderID, SqlDbType.Int);

            PtOrder _order = null;
            try
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == true)
                    {
                        reader.Read();

                        _order = new PtOrder(reader);
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            return _order;
        }

        public PtOrder GetOrderByExchangeId(string exchangeID)
        {
            var cmd = CreateCommandForSql("select [OrderID],[LastClOrdID],[ExchangeOrderID],[OrderSecuritySymbol],[OrderQuantity],[OrderPrice],[OrderSide],[OrderProcessCode],[OrderStatusCode],[OrderComment],[UserAseCode],[VenueId], [OrderSecurityIDSource] from Orders where ExchangeOrderID = @exchangeID");
            AddParameter(cmd, "@exchangeID", exchangeID, SqlDbType.NVarChar);

            PtOrder _order = null;
            try
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == true)
                    {
                        reader.Read();

                        _order = new PtOrder(reader);
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            return _order;
        }
        #endregion



        public int GetOutboundMessages(OutboundMessages vehicle, short venue_switch, int maxRows)
        {
            vehicle.Clear();

            var cmd = CreateCommandForProc("dbo.fxodl_broker_GetOutboundMessages");
            AddParameter(cmd, "@venue_switch", venue_switch, SqlDbType.SmallInt);
            AddParameter(cmd, "@top", maxRows, SqlDbType.Int);

            try
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    /*Στο 1o result set έχουμε ORDERS*/
                    while (reader.Read())
                    {
                        try
                        {
                            var _orderEntry = new OrderEntryOutMessage(reader);
                            vehicle.Messages.Add(_orderEntry);
                        }
                        catch (Exception ex)
                        {
                            theLogger.Error($"GetOutboundMessages(::Exception while Reading Orders/OrderEntryOutMessages, OrderID={reader.GetValue(0)}, message={ex.Message})");
                            MetricsProxy.Instance.OnError("GetOutboundMessages");
                        }
                    }
                    /*Στο 2ο result set έχουμε Changes*/
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var _orderChange = new OrderChangeOutMessage(reader);
                                vehicle.Messages.Add(_orderChange);
                            }
                            catch (Exception ex)
                            {
                                theLogger.Error($"GetOutboundMessages(::Exception while Reading Changes/OrderChangeOutMessage, ChngID={reader.GetValue(0)}, message={ex.Message})");
                                MetricsProxy.Instance.OnError("GetOutboundMessages");
                            }
                        }
                    }
                    /*Στο 3ο result set έχουμε Cancels*/
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var _orderEdit = new OrderEditOutMessage(reader);
                                vehicle.Messages.Add(_orderEdit);
                            }
                            catch (Exception ex)
                            {
                                theLogger.Error($"GetOutboundMessages(::Exception while Reading Cancels/OrderEditOutMessage, Cancelid={reader.GetValue(0)}, message={ex.Message})");
                                MetricsProxy.Instance.OnError("GetOutboundMessages");
                            }
                        }
                    }
                }

                return vehicle.Messages.Count;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        #region MarkOutBoundMessageAsSent
        public void SetOrderAsSent(decimal orderId, int processCode)
        {
            var cmd = CreateCommandForProc("dbo.fxodl_broker_SetOrderAsSent");
            AddParameter(cmd, "@OrderID", orderId, SqlDbType.Decimal);
            AddParameter(cmd, "@OrderProcessCode", processCode, SqlDbType.Int);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public void SetChangeAsSent(decimal orderId, int orderprocesscode, decimal chngID, int chngProcessCode)
        {
            var cmd = CreateCommandForProc("dbo.fxodl_broker_SetChangeAsSent");
            AddParameter(cmd, "@OrderID", orderId, SqlDbType.Decimal);
            AddParameter(cmd, "@OrderProcessCode", orderprocesscode, SqlDbType.Int);
            AddParameter(cmd, "@ChngID", chngID, SqlDbType.Decimal);
            AddParameter(cmd, "@ChngProcessCode", chngProcessCode, SqlDbType.Decimal);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public void SetCancelAsSent(decimal cancelID, int processCode)
        {
            var cmd = CreateCommandForProc("dbo.fxodl_broker_SetCancelAsSent");
            AddParameter(cmd, "@CancelID", cancelID, SqlDbType.Decimal);
            AddParameter(cmd, "@CancelProcessCode", processCode, SqlDbType.Int);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        #endregion

        #region Undo_MarkOutBoundMessageAsSent
        /// <summary>
        /// Θετει την συγκεκριμενη Orders απο OrderProcessCodeEnum.Se_katastash_apostolhs (1) σε OrderProcessCodeEnum.Entolh_etoimh_gia_apostolh (-1)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool UnSetOrderAsSent(decimal orderId)
        {
            var cmd = CreateCommandForSql("UPDATE dbo.ORDERS SET Orderprocesscode= -1 , OrderdateSent = null WHERE Orderid = @OrderID and OrderProcessCode = 1");
            AddParameter(cmd, "@OrderID", orderId, SqlDbType.Decimal);

            try
            {
                cmd.Connection.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected != 0;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public bool UnSetChangeAsSent(decimal orderId, decimal chngID)
        {
            var cmd = CreateCommandForSql("UPDATE dbo.[Changes] SET ChngProcessCode= -1  WHERE ChngID = @ChngID and ChngProcessCode = -5");
            AddParameter(cmd, "@ChngID", chngID, SqlDbType.Decimal);

            try
            {
                cmd.Connection.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected != 0;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public bool UnSetCancelAsSent(decimal cancelID)
        {
            var cmd = CreateCommandForSql("UPDATE dbo.Cancels SET CancelProcessCode= -1 WHERE CancelID = @CancelID and CancelProcessCode = -5");
            AddParameter(cmd, "@CancelID", cancelID, SqlDbType.Decimal);

            try
            {
                cmd.Connection.Open();
                var rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected != 0;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        #endregion


        public ClientStatus Clients_GetStatus(Guid appId, ODLMesssageSource source, int dayOfYear)
        {
            var status = new ClientStatus();

            var command = CreateCommandForProc("dbo.fxodl_FIXClients_GetStatus");
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@odlMesssageSource", (byte)source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", dayOfYear, SqlDbType.SmallInt);


            command.Connection.Open();
            using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleRow))
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    status.ETS_LastAppMsgId = reader.GetInt32(0);
                    status.ETS_LastMsgSeqNum = reader.GetInt32(1);
                    status.ORA_LastAppMsgId = reader.GetInt32(2);
                    status.ORA_LastMsgSeqNum = reader.GetInt32(3);
                    status.AppID = reader.GetGuid(4);
                    status.DayOfYear = reader.GetInt16(5);
                    if (!reader.IsDBNull(6)) status.ATHEXSessionID = reader.GetString(6);
                    status.CreateDT = reader.GetDateTime(7);
                }
            }
            return status;
        }
        public void Clients_Housekeeping(Guid appId, ODLMesssageSource source, int dayOfYear)
        {
            var command = CreateCommandForProc("dbo.fxodl_FIXClients_Housekeeping");
            AddParameter(command, "@AppID", Globals.AppID, SqlDbType.UniqueIdentifier);
            AddParameter(command, "@odlMesssageSource", (byte)source, SqlDbType.TinyInt);
            AddParameter(command, "@DayOfYear", dayOfYear, SqlDbType.SmallInt);

            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}
