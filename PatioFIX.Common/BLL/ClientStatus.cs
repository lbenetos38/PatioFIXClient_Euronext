using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public struct ClientStatus
    {
        /*
         * Το αποθηκευμενο (στην βαση μας, στον πινακα PatioFIXClients_State) τελευταιο ApplicationMessageId (tag198)
         * που επεξεργαστηκαμε επιτυχημένα.
         */
        public int ETS_LastAppMsgId;
        /*
         * Το αποθηκευμενο (στην βαση μας, στον πινακα PatioFIXClients_State) τελευταιο Message Sequence Number (tag34).
         * που επεξεργαστηκαμε επιτυχημένα.
         */
        public int ETS_LastMsgSeqNum;

        public int ORA_LastAppMsgId;
        public int ORA_LastMsgSeqNum;
        public Guid AppID;
        public int DayOfYear;
        public DateTime CreateDT;
        public string ATHEXSessionID;

        /*
         * Μεταφερει την συγκεκριμενη ρυθμιση στον FixClient ετσι
         * οπως της εχουμε στο appsettings.json
         * Χρησιμευει μονο στο development
         */
        public bool DisableDataLayer;
    }
}
