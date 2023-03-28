using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common
{
    /// <summary>
    /// Η παρακατω δομη αποθηκευει ενα μερικο state του FIX Client μας,
    /// ετσι ωστε εαν χαθει η συνδεση με τον Aggregator, οταν συνδεθουμε ξανα μαζι του
    /// να μπορεσουμε να του δωσουμε καποιο state
    /// </summary>
    internal struct LocalStateDTO
    {
        public bool SentLogon;
        public bool ReceivedLogon;
        public bool SentLogout;
        public bool ReceivedLogout;
        public bool Synchronizing;
        public bool Server_Synchronizing;
        public bool TestRequestPending;
        public bool ForceStopInProcess;

        public int NextOutboundSeqNum;
        public int NextInboundSeqNum;

        public bool IsTCPConnected;
        public bool IsFixClientStarted;

        public void SetState(SessionState state)
        {
            this.SentLogon = state.SentLogon;
            this.ReceivedLogon = state.ReceivedLogon;
            this.SentLogout = state.SentLogout;
            this.ReceivedLogout = state.ReceivedLogout;
            this.Synchronizing = state.Synchronizing;
            this.Server_Synchronizing = state.Server_Synchronizing;
            this.TestRequestPending = state.TestRequestPending;
            this.ForceStopInProcess = state.ForceStopInProcess;
        }

        public void SetSeqNums(int nextOutboundSeqNum, int nextInboundSeqNum)
        {
            this.NextOutboundSeqNum = nextOutboundSeqNum;
            this.NextInboundSeqNum = nextInboundSeqNum;
        }

        public void OnTCPConnect()
        {
            IsTCPConnected = true;
        }
        public void OnTCPDisconnect()
        {
            IsTCPConnected = false;
        }

        public void OnFIXClientStarted(bool value)
        {
            IsFixClientStarted = value;
        }
    }
}
