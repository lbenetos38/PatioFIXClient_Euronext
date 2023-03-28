using PatioFIX.Common.BLL.Messages;
using System;

namespace PatioFIX.Common.DAL
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IOdlDataLayer
    {



        void InsertIgnored(FIXInMessage message, IgnoredMessage ignoredMessage);
        void InsertConfirmCancel(FIXInMessage message, OrderEditConfirmationMessage confirmation);
        void InsertConfirmChange(FIXInMessage message, OrderChangeConfirmationMessage confirmation, string strOrderNotes);
        void InsertConfirmOrder(FIXInMessage message, OrderEntryConfirmationMessage confirmOrder, string orderNotes);
        void InsertConfirmOrderEdit(FIXInMessage message, OrderEditConfirmationMessage confirmation);
        void InsertCreditLimitInformation(FIXInMessage message, CreditLimitInfoMessage creditLimitInfo);
        void InsertExchangeNotes(FIXInMessage message, ExchangeNotesMessage exchangeNotes);

        void InsertOrderMarketStatus(FIXInMessage message, MarketStatusMessage marketStatus);

        void InsertReject(FIXInMessage message, string memberOrderID, RejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator);
        void InsertReject(FIXInMessage message, OrderCancelRejectMessage rejection, string sourceMsgType, string orderNote, PtOrderGenerator generator);


        void InsertSecurityPrice(FIXInMessage message, SecurityPricesMessage securityPrice);
        void InsertSecurityStatus(FIXInMessage message, SecurityStatusMessage securityStatus);
        void InsertTrades(FIXInMessage message, NewTradeConfirmationMessage newTradeConfirmation);


        void UpdateOrderProcessAndStatusCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode, char ordStatus, string rejReasCode);
        void UpdateOrderProcessCode(FIXInMessage message, int orderId, OrderProcessCodeEnum processCode);


        PtCancel GetCancelById(int cancelID);
        PtChange GetChangeById(int changeID);
        PtOrder GetOrderById(int orderID);
        PtOrder GetOrderByExchangeId(string exchangeID);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="venue_switch">0=ETS,	1=ORA,	2=*</param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        int GetOutboundMessages(OutboundMessages vehicle, short venue_switch, int maxRows);
        void SetOrderAsSent(decimal orderId, int processCode);
        void SetChangeAsSent(decimal orderId, int orderprocesscode, decimal chngID, int chngProcessCode);
        void SetCancelAsSent(decimal cancelID, int processCode);

        /// <summary>
        /// Θετει την συγκεκριμενη Orders σε OrderProcessCodeEnum.Entolh_etoimh_gia_apostolh (-1)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool UnSetOrderAsSent(decimal orderId);
        bool UnSetChangeAsSent(decimal orderId, decimal chngID);
        bool UnSetCancelAsSent(decimal cancelID);


        ClientStatus Clients_GetStatus(Guid appId, ODLMesssageSource source, int dayOfYear);
        void Clients_Housekeeping(Guid appId, ODLMesssageSource source, int dayOfYear);
    }
}