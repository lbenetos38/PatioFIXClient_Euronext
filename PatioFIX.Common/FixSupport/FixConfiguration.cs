using System.Net;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// Σε αυτην class εχουμε μονο τα settings που αφορουν την λειτοργία του Fix Client
    /// (Ειναι υποσυνολο απο αυτα που βρισκονται στην PatioFIXClientConfiguration)
    /// </summary>
    public class FixConfiguration
    {
        public ODLMesssageSource ClientRole { get; internal set; }

        #region FixServer
        public IPAddress ServerIP { get; internal set; }
        public int Port1 { get; internal set; }
        public int Port2 { get; internal set; }
        public bool SSLEnable { get; internal set; }
        /// <summary>
        /// θα κανουμε validation του remote certificate 
        /// </summary>
        public bool VerifyCertificate { get; internal set; }
        /// <summary>
        /// The name of the server the client is trying to connect to. 
        /// That name is used for server certificate validation. 
        /// </summary>
        public string SSLServerName { get; internal set; }
        #endregion



        #region Session
        /// <summary>
        /// 
        /// </summary>
        public string Version { get; internal set; }
        /// <summary>
        /// Heartbeat interval (seconds)
        /// The HeartBtInt (108) field is used to declare the timeout interval for generating heartbeats 
        /// (same value used by both sides).
        /// </summary>
        public int HeartbeatInterval { get; internal set; }

        /// <summary>
        /// SenderCompID (Tag = 49, Type: String)
        /// Assigned value used to identify firm sending message.
        /// </summary>
        public string SenderCompID { get; set; }
        /// <summary>
        /// SenderSubID (Tag = 50, Type: String)
        /// Assigned value used to identify specific message originator (desk, trader, etc.)
        /// </summary>
        public string SenderSubID { get; set; }
        /// <summary>
        /// TargetCompID (Tag = 56, Type: String)
        /// Assigned value used to identify receiving firm.
        /// </summary>
        public string TargetCompID { get; set; }
        /// <summary>
        /// TargetSubID (Tag = 57, Type: String)
        /// Assigned value used to identify specific individual or unit intended to receive message
        /// </summary>
        public string TargetSubID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }
        #endregion


        #region TCP Protocol Related
        /// <summary>
        /// Improve latency at the expense of throughput (disable/enable Nagle's algorithm).
        /// </summary>
        public bool TcpNoDelay { get; internal set; }
        /// <summary>
        /// TCP socket receive buffer size allocated to FIX connection, in bytes.
        /// </summary>
        public int? ReceiveBufferSize { get; internal set; }
        public int? ReceiveTimeout { get; internal set; }
        /// <summary>
        /// TCP socket send buffer size allocated to FIX connection, in bytes.
        /// </summary>
        public int? SendBufferSize { get; internal set; }
        public int? SendTimeout { get; internal set; }

        /// <summary>
        /// Το μεγιστο μεγεθος του receive buffer, του TCPConnector
        /// Εκει βαζει το συστημα τα bytes των δεδομενων που λαμβανει απο το m_socket.Receive()
        /// </summary>
        public int MaxRcvBuffer { get; internal set; }
        #endregion





        /// <summary>
        /// SessionTimerInterval (milliseconds)
        /// </summary>
        public int SessionTimerInterval { get; internal set; }
        /// <summary>
        /// To χρονικο διαστημα που μεσολαβει μεταξυ διαδοχικων επανασυνδεσεων (milliseconds)
        /// σε επιπεδο TCP (TCPConnection.Connect())
        /// </summary>
        public int TCPReconnectInterval { get; internal set; }
        /// <summary>
        /// Log incoming messages.
        /// </summary>
        public bool LogInboundMessages { get; internal set; }
        /// <summary>
        /// Log outgoing messages. Setting to false forces fixclient 
        /// to always send GapFills instead of resending messages
        /// </summary>
        public bool LogOutboundMessages { get; internal set; }
        /// <summary>
        /// Ελεγχει το πως απαντα ο FIX Client σε Resend Requests (35=2) απο τον ATHEX Fix Server
        /// Εαν ειναι true, τοτε ο FIX Client ξαναδτελνει τα τα αποθηκευμενα μηνυματα απο το MessageStore.
        /// Για τα στειλουμε αποθηκευμενα μηνυματα θα πρεπει και το LogInboundMessages να ειναι true.
        /// Εαν εχει την τιμη false, τοτε o Fix Client στελνει μονο Sequence Reset-GapFill<4>
        /// </summary>
        public bool UseAlwaysResetGapFilling { get; internal set; }



        #region Fix Message Validation
        /// <summary>
        /// Validate the CheckSum (tag 10) field value.
        /// </summary>
        public bool ValidateCheckSum { get; internal set; }
        /// <summary>
        /// Validate the BodyLength  (tag 9) field value.
        /// </summary>
        public bool ValidateBodyLength { get; internal set; }
        /// <summary>
        /// Validate the presence of fields that appear more than once.
        /// </summary>
        public bool ValidateDuplicatedFields { get; internal set; }
        /// <summary>
        /// Validate empty message field values.
        /// </summary>
        public bool ValidateEmptyFieldValues { get; internal set; }
        /// <summary>
        /// Option to validate field values of FIX messages in accordance with the FIX protocol or its FIX Dialect.
        /// </summary>
        public bool ValidateFieldValues { get; internal set; }
        /// <summary>
        /// Validate that the declared number of repeating group instances is equal to the actual one.
        /// </summary>
        public bool ValidateRepeatingGroupEntryCount { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ValidateRepeatingGroupLeadingField { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidateDuplicatePartyRole { get; internal set; }

        /// <summary>
        /// Validate the presence of required message fields.
        /// </summary>
        public bool ValidateRequiredFields { get; internal set; }
        /// <summary>
        /// Validate that there are no unknown message fields.
        /// </summary>
        public bool ValidateUnknownFields { get; internal set; }
        /// <summary>
        /// Validate that there are no unknown messages received.
        /// </summary>
        public bool ValidateUnknownMessages { get; internal set; }
        #endregion


        /// <summary>
        /// To μεγιστο μεγεθος σε bytes του εσωτερικου buffer ενος FIXMessage
        /// </summary>
        public int MaxMessageLength { get; internal set; }
        /// <summary>
        /// Το μεγιστο πληθος των Fields που μπορει να περιεχει ενα FIXMessage
        /// </summary>
        public int MaxMessageFields { get; internal set; }




    }
}
