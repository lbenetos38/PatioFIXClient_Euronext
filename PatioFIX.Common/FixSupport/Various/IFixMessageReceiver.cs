namespace PatioFIX.Common.FixSupport
{
    internal interface IFixMessageReceiver
    {
        void OnNewMessage(FIXMessage message);

        void OnInvalidMessage(FIXMessage message);

        void OnDisconnect();
    }
}
