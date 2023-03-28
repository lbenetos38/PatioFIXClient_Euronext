using PatioFIX.Common.BLL.Messages;
using System;



namespace PatioFIX.Common.DAL
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OdlDummyDatalayer : BaseDataLayer, IOdlDataLayer
    {
        readonly Logger theLogger = null;

        public OdlDummyDatalayer() : base(Globals.PatioOMS.ODLConnStr)
        {
            theLogger = new Logger("OdlDataLayer");
        }



        public void InsertIgnored(FIXInMessage message, IgnoredMessage ignoredMessage)
        {
        }
        public void InsertConfirmCancel(FIXInMessage message, OrderEditConfirmationMessage confirmation)
        {

        }
        public void InsertConfirmChange(FIXInMessage message, OrderChangeConfirmationMessage confirmation, string strOrderNotes)
        {

        }
        public void InsertConfirmOrder(FIXInMessage message, OrderEntryConfirmationMessage confirmOrder, string orderNotes)
        {

        }
        public void InsertConfirmOrderEdit(FIXInMessage message, OrderEditConfirmationMessage confirmation)
        {

        }
        public void InsertCreditLimitInformation(FIXInMessage message, CreditLimitInfoMessage creditLimitInfo)
        {

        }
        public void InsertExchangeNotes(FIXInMessage message, ExchangeNotesMessage exchangeNotes)
        {

        }

        public void InsertOrderMarketStatus(FIXInMessage message, MarketStatusMessage marketStatus)
        {

        }

        public void InsertReject(FIXInMessage message, string memberOrderID, RejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator)
        {

        }
        public void InsertReject(FIXInMessage message, OrderCancelRejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator)
        {

        }


        public void InsertSecurityPrice(FIXInMessage message, SecurityPricesMessage securityPrice)
        {

        }
        public void InsertSecurityStatus(FIXInMessage message, SecurityStatusMessage securityStatus)
        {

        }
        public void InsertTrades(FIXInMessage message, NewTradeConfirmationMessage newTradeConfirmation)
        {

        }



        public void UpdateOrderProcessAndStatusCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode, char ordStatus, string rejReasCode = default)
        {

        }

        public void UpdateOrderProcessCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode)
        {

        }


        public PtCancel GetCancelById(int cancelID)
        {
            return null;
        }
        public PtChange GetChangeById(int changeID)
        {
            return null;
        }

        public PtOrder GetOrderById(int orderID)
        {
            return null;
        }

        public PtOrder GetOrderByExchangeId(string exchangeID)
        {
            return null;
        }



        public int GetOutboundMessages(OutboundMessages vehicle, short venue_switch, int maxRows)
        {
            return 0;
        }


        public void SetOrderAsSent(decimal orderId, int processCode)
        {

        }
        public void SetChangeAsSent(decimal orderId, int orderprocesscode, decimal chngID, int chngProcessCode)
        {

        }
        public void SetCancelAsSent(decimal cancelID, int processCode)
        {

        }


        /// <summary>
        /// Θετει την συγκεκριμενη Orders απο OrderProcessCodeEnum.Se_katastash_apostolhs (1) σε OrderProcessCodeEnum.Entolh_etoimh_gia_apostolh (-1)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool UnSetOrderAsSent(decimal orderId)
        {
            return true;
        }
        public bool UnSetChangeAsSent(decimal orderId, decimal chngID)
        {
            return true;
        }
        public bool UnSetCancelAsSent(decimal cancelID)
        {
            return true;
        }



        public ClientStatus Clients_GetStatus(Guid appId, ODLMesssageSource source, int dayOfYear)
        {
            var status = new ClientStatus();

            return status;
        }
        public void Clients_Housekeeping(Guid appId, ODLMesssageSource source, int dayOfYear)
        {

        }
    }
}
