namespace PatioFIX.Common
{
    /// <summary>
    /// Οπως φαίνεται και στον κώδικα το enum ODLMessageType περιλαμβάνει τιμές απο το μηδέν μέχρι και το 40, συνεχομενες.
    /// Για καθε ODLMessageType υπάρχει ένα αντίστοιχο message (IMessage) και ένας αντίστοιχος evaluator (IEvaluator)
    /// Αυτό το χρησιμοποιούμε για να δημιουργήσουμε ένα πίνακα απο parsers (MessageParser.m_messages) και ενα πίνακα
    /// απο evaluators (Evaluator.m_evaluators), και για να βρίσκουμε αμεσα τον pasrer/messsage και τον evaluator που
    /// χρειαζόμαστε στο runtime.
    /// Οποιαδήποτε αλλαγη, λοιπον πολυ προσεκτικά, εχοντας κατανοήσει πρωτα την σχεση που περγραφεται προηγουμενως
    /// </summary>
    public enum ODLMessageTypeEnum : int
    {
        Error = -1,
        /// <summary>
        /// 0. Unknown Type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 1. Order Entry ("MB")
        /// </summary>
        Order_Entry = 1,
        /// <summary>
        /// 2. Order Edit ("MC")
        /// <para>Cancel, Suspend, Unsuspend</para>
        /// </summary>
        Order_Edit = 2,
        /// <summary>
        /// 3. Order Change ("MD")
        /// </summary>
        Order_Change = 3,
        /// <summary>
        /// 4. Trade Report Entry(Preagreed Price Trade) ("MI")
        /// </summary>
        Trade_Report_Entry = 4,
        /// <summary>
        /// 5. Order Mass Cancel ("MX")
        /// </summary>
        Order_Mass_Cancel = 5,
        /// <summary>
        /// 6. Hit & Take Order Entry ("MF")
        /// </summary>
        Hit_n_Take_Order_Entry = 6,
        /// <summary>
        /// 7. Order Mass Cancel Confirmation ("TX")
        /// </summary>
        Order_Mass_Cancel_Confirmation = 7,
        /// <summary>
        /// 8. Order Entry Confirmation ("TB")
        /// </summary>
        Order_Entry_Confirmation = 8,
        /// <summary>
        /// 9.Order Edit Confirmation ("TC")
        /// </summary>
        Order_Edit_Confirmation = 9,
        /// <summary>
        /// 10. Order Change Confirmation ("TD")
        /// </summary>
        Order_Change_Confirmation = 10,
        /// <summary>
        /// 11. New Trade Confirmation ("TF")
        /// </summary>
        New_Trade_Confirmation = 11,
        /// <summary>
        /// 12. Quote Mass Cancel ("MY")
        /// </summary>
        Quote_Mass_Cancel = 12,
        /// <summary>
        /// 13. Reserved,
        /// </summary>
        Reserved1 = 13,
        /// <summary>
        /// 14. Rejection ("TR")
        /// </summary>
        Rejection = 14,
        /// <summary>
        /// 14. MsgType == Order Cancel Reject (9) ("TE")
        /// </summary>
        OrderCancelReject = 15,
        /// <summary>
        /// 16. Credit Limit Information ("TL")
        /// </summary>
        Credit_Limit_Information = 16,
        /// <summary>
        /// 17. Security Status ("CA")
        /// </summary>
        Security_Status = 17,
        /// <summary>
        /// 18. Security Price ("CD")
        /// </summary>
        Security_Price = 18,
        /// <summary>
        /// 19. Market Status ("CC")
        /// </summary>
        Market_Status = 19,
        /// <summary>
        /// 20. System Status ("CB")
        /// </summary>
        System_Status = 20,
        /// <summary>
        /// 21. Hit & Take Order Information ("CH")
        /// </summary>
        Hit_n_Take_Order_Information = 21,
        /// <summary>
        /// 22. Reserved,
        /// </summary>
        Reserved2 = 22,
        /// <summary>
        /// 23. Reserved,
        /// </summary>
        Reserved3 = 23,
        /// <summary>
        /// 24. DSS Entry ("MS")
        /// </summary>
        DSS_Entry = 24,
        /// <summary>
        /// 25. DSS Entry Confirmation ("TS")
        /// </summary>
        DSS_Entry_Confirmation = 25,
        /// <summary>
        /// 26. DSS Trade ("TU")
        /// </summary>
        DSS_Trade = 26,
        /// <summary>
        /// 27. DSS Broadcast ("CS")
        /// </summary>
        DSS_Broadcast = 27,
        /// <summary>
        /// 28. Quote Entry / Change ("MA")
        /// </summary>
        Quote_Entry_Change = 28,
        /// <summary>
        /// 29. Quote Cancel ("ME")
        /// </summary>
        Quote_Cancel = 29,
        /// <summary>
        /// 30. Quote Status Report ("TA")
        /// </summary>
        Quote_Status_Report = 30,
        /// <summary>
        /// 31. Quote Request "MJ"
        /// </summary>
        Quote_Request = 31,
        /// <summary>
        /// 32. Quote Request Confirmation ("TJ")
        /// </summary>
        Quote_Request_Confirmation = 32,
        /// <summary>
        /// 33. Quote Request Execution ("TP")
        /// </summary>
        Quote_Request_Execution = 33,
        /// <summary>
        /// 34. Quote Request Info ("TK")
        /// </summary>
        Quote_Request_Info = 34,
        /// <summary>
        /// 35. Quote Responsibility Suspend / Resume ("TN")
        /// </summary>
        Quote_Responsibility_Suspend_Resume = 35,
        /// <summary>
        /// 36. Quote Alarm ("TM")
        /// </summary>
        Quote_Alarm = 36,
        /// <summary>
        /// 37. Exchange Notes ("TO")
        /// </summary>
        Exchange_Notes = 37,
        /// <summary>
        /// 38. Quote Mass Cancel Confirmation ("TY")
        /// </summary>
        Quote_Mass_Cancel_Confirmation = 38,
        /// <summary>
        /// 
        /// </summary>
        Business_Message_Reject = 39,
        /// <summary>
        /// 
        /// </summary>
        Ignored_Message = 40,

    }
}
