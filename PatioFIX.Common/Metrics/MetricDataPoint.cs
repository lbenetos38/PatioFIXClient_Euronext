namespace PatioFIX.Common
{
    public struct MetricDataPoint
    {
        public bool WatchDog_Connected;
        public int WatchDog_Clients;

        #region TCPConnection
        public long TCP_ReceivedBytes;
        public long TCP_ReceivedMessages;
        public long TCP_SendBytes;
        public long TCP_SendMessages;
        public int TCP_IsConnected;
        public int TCP_Errors;
        public int TCP_Warnings;
        #endregion


        #region FixClient
        public int FIXCLIENT_InstanceCounter;
        public int FIXCLIENT_Started;
        public int FIXCLIENT_InboundSeqNumTooHigh;
        public int FIXCLIENT_ThrowAwayMessages;
        public int FIXCLIENT_IgnoredMessages;
        public int FIXCLIENT_ResendRequests;
        public int FIXCLIENT_Rejections;
        public int FIXCLIENT_SessionTimerHeartbeats;
        public int FIXCLIENT_Errors;
        public int FIXCLIENT_Warnings;
        public int FIXCLIENT_Rejects;
        public int FIXCLIENT_FailedLogins;
        #endregion


        #region SessionState
        public int SESSIONSTATE_SentLogon;
        public int SESSIONSTATE_ReceivedLogon;
        public int SESSIONSTATE_SentLogout;
        public int SESSIONSTATE_ReceivedLogout;
        public int SESSIONSTATE_Synchronizing;
        public int SESSIONSTATE_ServerSynchronizing;
        public int SESSIONSTATE_TestRequestPending;
        public int SESSIONSTATE_ForceStopInProcess;
        public int SESSIONSTATE_NextInboundSeqNum;
        public int SESSIONSTATE_NextOutboundSeqNum;
        #endregion


        public int Parsing_Errors;
        public int Parsing_Warnings;



        #region ODL Messages Counters
        public int Unknown;
        public int Credit_Limit_Information;
        public int Exchange_Notes;
        public int Market_Status;
        public int New_Trade_Confirmation;
        public int Order_Change_Confirmation;
        public int Order_Edit_Confirmation;
        public int Order_Entry_Confirmation;
        public int Rejection;
        public int OrderCancelRejection;
        public int Security_Price;
        public int Security_Status;
        public int System_Status;
        #endregion


        #region ODL Summaries
        public int Credit_Limit_Information_count;
        public long Credit_Limit_Information_sum;
        public int Exchange_Notes_count;
        public long Exchange_Notes_sum;
        public int Market_Status_count;
        public long Market_Status_sum;
        public int New_Trade_Confirmation_count;
        public long New_Trade_Confirmation_sum;
        public int Order_Change_Confirmation_count;
        public long Order_Change_Confirmation_sum;
        public int Order_Edit_Confirmation_count;
        public long Order_Edit_Confirmation_sum;
        public int Order_Entry_Confirmation_count;
        public long Order_Entry_Confirmation_sum;
        public int Rejection_count;
        public long Rejection_sum;
        public int OrderCancelReject_count;
        public long OrderCancelReject_sum;
        public int Security_Price_count;
        public long Security_Price_sum;
        public int Security_Status_count;
        public long Security_Status_sum;
        public int System_Status_count;
        public long System_Status_sum;
        public int Ignored_count;
        public long Ignored_sum;
        #endregion


        public int Order_Entry;
        public int Order_Change;
        public int Order_Edit;

        public int Received;
        public int Send;

        public int TheControllerHeartBeats;
        public int EventsListenerHeartBeats;

        public int Application_Errors;
        public int Application_Warnings;

        public int Errors_EventsListener;
        public int Errors_TheController;
        public int Errors_OrdersDispatcher;
        public int Errors_GetOutboundMessages;

        public int Warnings_EventsListener;
        public int Warnings_OrdersDispatcher;
        public int Warnings_TheController;
        public int Warnings_SendMessage_Guard;
        public int Warnings_SendMessage_Failled;
        public int Warnings_SendMessage_Retries;
        public int Warnings_SendMessage_Expired;
        public int Warnings_SetOutBound_Timeout;
        public int Warnings_SetOutBound_Deadlock;
        public int Warnings_Restore_failed;
        public int Warnings_UnSetOutBound_Timeout;
        public int Warnings_UnSetOutBound_Deadlock;

        public int Num_of_Connections;

        public float CPU;
        public float Memory;


        public int UnConfirmedPool_NumOfMessages;

        public void Reset()
        {
            this.WatchDog_Connected = false;
            this.WatchDog_Clients = 0;

            this.TCP_ReceivedBytes = 0;
            this.TCP_ReceivedMessages = 0;
            this.TCP_SendBytes = 0;
            this.TCP_SendMessages = 0;
            this.TCP_IsConnected = 0;
            this.TCP_Errors = 0;
            this.TCP_Warnings = 0;

            this.FIXCLIENT_InstanceCounter = default;
            this.FIXCLIENT_FailedLogins = default;
            this.FIXCLIENT_Warnings = default;
            this.FIXCLIENT_Errors = default;
            this.FIXCLIENT_Rejects = default;
            this.FIXCLIENT_Started = default;
            this.FIXCLIENT_InboundSeqNumTooHigh = default;
            this.FIXCLIENT_ThrowAwayMessages = default;
            this.FIXCLIENT_IgnoredMessages = default;
            this.FIXCLIENT_ResendRequests = default;
            this.FIXCLIENT_Rejections = default;
            this.FIXCLIENT_SessionTimerHeartbeats = default;

            this.SESSIONSTATE_SentLogon = default;
            this.SESSIONSTATE_ReceivedLogon = default;
            this.SESSIONSTATE_SentLogout = default;
            this.SESSIONSTATE_ReceivedLogout = default;
            this.SESSIONSTATE_Synchronizing = default;
            this.SESSIONSTATE_ServerSynchronizing = default;
            this.SESSIONSTATE_TestRequestPending = default;
            this.SESSIONSTATE_ForceStopInProcess = default;
            this.SESSIONSTATE_NextInboundSeqNum = default;
            this.SESSIONSTATE_NextOutboundSeqNum = default;

            this.Parsing_Errors = default;
            this.Parsing_Warnings = default;


            this.Unknown = 0;
            this.Credit_Limit_Information = 0;
            this.Exchange_Notes = 0;
            this.Market_Status = 0;
            this.New_Trade_Confirmation = 0;
            this.Order_Change_Confirmation = 0;
            this.Order_Edit_Confirmation = 0;
            this.Order_Entry_Confirmation = 0;
            this.Rejection = 0;
            this.OrderCancelRejection = 0;
            this.Security_Price = 0;
            this.Security_Status = 0;
            this.System_Status = 0;

            this.Order_Entry = 0;
            this.Order_Change = 0;
            this.Order_Edit = 0;

            this.Received = 0;
            this.Send = 0;
            this.TheControllerHeartBeats = 0;
            this.EventsListenerHeartBeats = 0;
            this.Application_Errors = 0;
            this.Application_Warnings = 0;


            this.Errors_EventsListener = 0;
            this.Errors_TheController = 0;
            this.Errors_OrdersDispatcher = 0;
            this.Errors_GetOutboundMessages = 0;

            this.Warnings_EventsListener = 0;
            this.Warnings_OrdersDispatcher = 0;
            this.Warnings_TheController = 0;
            this.Warnings_SendMessage_Guard = 0;
            this.Warnings_SendMessage_Failled = 0;
            this.Warnings_SendMessage_Retries = 0;
            this.Warnings_SendMessage_Expired = 0;
            this.Warnings_SetOutBound_Timeout = 0;
            this.Warnings_SetOutBound_Deadlock = 0;
            this.Warnings_Restore_failed = 0;
            this.Warnings_UnSetOutBound_Timeout = 0;
            this.Warnings_UnSetOutBound_Deadlock = 0;

            this.Num_of_Connections = 0;

            this.CPU = 0;
            this.Memory = 0;

            this.UnConfirmedPool_NumOfMessages = 0;
        }
        public void ResetSummaries()
        {
            this.Credit_Limit_Information_count = 0;
            this.Credit_Limit_Information_sum = 0;
            this.Exchange_Notes_count = 0;
            this.Exchange_Notes_sum = 0;
            this.Market_Status_count = 0;
            this.Market_Status_sum = 0;
            this.New_Trade_Confirmation_count = 0;
            this.New_Trade_Confirmation_sum = 0;
            this.Order_Change_Confirmation_count = 0;
            this.Order_Change_Confirmation_sum = 0;
            this.Order_Edit_Confirmation_count = 0;
            this.Order_Edit_Confirmation_sum = 0;
            this.Order_Entry_Confirmation_count = 0;
            this.Order_Entry_Confirmation_sum = 0;
            this.Rejection_count = 0;
            this.Rejection_sum = 0;
            this.OrderCancelReject_count = 0;
            this.OrderCancelReject_sum = 0;
            this.Security_Price_count = 0;
            this.Security_Price_sum = 0;
            this.Security_Status_count = 0;
            this.Security_Status_sum = 0;
            this.System_Status_count = 0;
            this.System_Status_sum = 0;
            this.Ignored_count = 0;
            this.Ignored_sum = 0;
        }
    }

}
