namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum MetricKeysEnumeration : byte
    {
        Undefined = 0,
        Hello = 1,

        TCPIncomingMessage = 10,
        TCPOutcomingMessage = 11,
        TCPWarning = 12,
        TCPError = 13,
        TCPConnect = 14,
        TCPDisconnect = 15,

        FIXClientNewInstance = 20,
        FIXClientStarted = 21,
        FIXClientInboundSeqNumTooHigh = 22,
        FIXClientThrowAwayMessage = 23,
        FIXClientIgnoredMessage = 24,
        FIXClientSendResendRequest = 25,
        FIXClientSendRejection = 26,
        FIXClientState1 = 27,
        FIXClientState2 = 28,
        FIXClientSessionTimerHeartBeat = 29,
        FIXClientFailedLogin = 30,
        FIXClientWarning = 31,
        FIXClientError = 32,
        FIXClientReject = 33,

        ParsingWarning = 40,
        ParsingError = 41,

        EvaluationIime = 50,
        Receive = 51,
        Send = 52,

        Error = 60,
        Warning = 61,

        ControllerHeartBeat = 70,
        EventsListenerHeartBeat = 71,
        UnConfirmedPoolMessages = 72,

        MarketStatus = 80,
        SecurityStatus = 81,
    }
}
