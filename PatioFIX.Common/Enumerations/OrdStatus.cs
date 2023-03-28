namespace PatioFIX.Common
{
    /*
     * OrdStatus (Tag = 39, Type: char)
     * Identifies current status of order.
     */
    public static class OrdStatus
    {
        /*
         * 
         */
        public const char Undefined = default(char);

        /*
         * FIX 4.4
         */
        public const char New = '0';
        public const char PartiallyFilled = '1';
        public const char Filled = '2';
        public const char DoneForDay = '3';
        public const char Canceled = '4';
        public const char Pending_Cancel = '6';
        public const char Stopped = '7';
        public const char Rejected = '8';
        public const char Suspended = '9';
        public const char PendingNew = 'A';
        public const char Calculated = 'B';
        public const char Expired = 'C';
        public const char AcceptedForBidding = 'D';
        public const char Pending_Replace = 'E';
        public const char ReplacedNoLongerUsed = '5';

        /*
         * User Definid
         */
        public const char Inactive = 'I';
        public const char Not_Released = 'N';
    }
}
