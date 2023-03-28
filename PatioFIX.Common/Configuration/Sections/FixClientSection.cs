using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class FixClientSection
    {
        const int SessionTimerInterval_MINVALUE = 2000;
        const int ReconnectInterval_MINVALUE = 8000;
        const int MaxMessageLength_MINVALUE = 2048;
        const int MaxMessageFields_MINVALUE = 128;


        /// <summary>
        /// 
        /// </summary>
        public string SectionName { get; } = "FixClient";


        /// <summary>
        /// SessionTimerInterval (milliseconds)
        /// </summary>
        public int SessionTimerInterval { get; } = SessionTimerInterval_MINVALUE;

        /// <summary>
        /// To χρονικο διαστημα που μεσολαβει μεταξυ διαδοχικων επανασυνδεσεων (milliseconds)
        /// σε επιπεδο TCP (TCPConnection.Connect())
        /// </summary>
        public int TCPReconnectInterval { get; } = ReconnectInterval_MINVALUE;


        /// <summary>
        /// Log incoming messages.
        /// </summary>
        public bool LogInboundMessages { get; } = false;
        /// <summary>
        /// Log outgoing messages. Setting to false forces fixclient 
        /// to always send GapFills instead of resending messages
        /// </summary>
        public bool LogOutboundMessages { get; } = true;
        /// <summary>
        /// Ελεγχει το πως απαντα ο FIX Client σε Resend Requests (35=2) απο τον ATHEX Fix Server
        /// Εαν ειναι true, τοτε ο FIX Client ξαναδτελνει τα τα αποθηκευμενα μηνυματα απο το MessageStore.
        /// Για τα στειλουμε αποθηκευμενα μηνυματα θα πρεπει και το LogInboundMessages να ειναι true.
        /// Εαν εχει την τιμη false, τοτε o Fix Client στελνει μονο Sequence Reset-GapFill<4>
        /// </summary>
        public bool UseAlwaysResetGapFilling { get; } = true;




        #region Fix Message Validation
        /// <summary>
        /// Validate the CheckSum (tag 10) field value.
        /// </summary>
        public bool ValidateCheckSum { get; } = true;
        /// <summary>
        /// Validate the BodyLength  (tag 9) field value.
        /// </summary>
        public bool ValidateBodyLength { get; } = true;
        /// <summary>
        /// Validate the presence of fields that appear more than once.
        /// </summary>
        public bool ValidateDuplicatedFields { get; } = false;
        /// <summary>
        /// Validate empty message field values.
        /// </summary>
        public bool ValidateEmptyFieldValues { get; } = false;
        /// <summary>
        /// Option to validate field values of FIX messages in accordance with the FIX protocol or its FIX Dialect.
        /// </summary>
        public bool ValidateFieldValues { get; } = false;
        /// <summary>
        /// Validate that the declared number of repeating group instances is equal to the actual one.
        /// </summary>
        public bool ValidateRepeatingGroupEntryCount { get; } = true;
        /// <summary>
        /// 
        /// </summary>
        public bool ValidateRepeatingGroupLeadingField { get; } = false;



        /// <summary>
        /// ελεγχει την περιπτωση να εχουμε ενα Party με επαναλαμβανομενη τιμη στο PartyRole(Tag=452)
        /// <para>Στο UAT to ΑΤΗΕΧ μας εστελνε συνεχεια Parties με αυτο το bug και γεμιζαν τα log αρχεια μας
        /// </para>
        /// </summary>
        public bool ValidateDuplicatePartyRole { get; } = true;


        /// <summary>
        /// Validate the presence of required message fields.
        /// </summary>
        public bool ValidateRequiredFields { get; } = false;
        /// <summary>
        /// Validate that there are no unknown message fields.
        /// </summary>
        public bool ValidateUnknownFields { get; } = false;
        /// <summary>
        /// Validate that there are no unknown messages received.
        /// </summary>
        public bool ValidateUnknownMessages { get; } = false;
        #endregion



        /// <summary>
        /// 
        /// </summary>
        public int MaxMessageLength { get; } = MaxMessageLength_MINVALUE;
        /// <summary>
        /// 
        /// </summary>
        public int MaxMessageFields { get; } = MaxMessageFields_MINVALUE;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal FixClientSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {

                var value = section["SessionTimerInterval"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.SessionTimerInterval = Int32.Parse(value);
                }
                if (this.SessionTimerInterval < SessionTimerInterval_MINVALUE)
                {
                    throw new ArgumentOutOfRangeException($"Invalid value for {section.Path}:SessionTimerInterval. Cannot be smaller than {SessionTimerInterval_MINVALUE} ms");
                }


                value = section["TCPReconnectInterval"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.TCPReconnectInterval = Int32.Parse(value);
                }
                if (this.TCPReconnectInterval < ReconnectInterval_MINVALUE)
                {
                    throw new ArgumentOutOfRangeException($"Invalid value for {section.Path}:ReconnectInterval. Cannot be smaller than {ReconnectInterval_MINVALUE} ms");
                }

                value = section["LogInboundMessages"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.LogInboundMessages = bool.Parse(value);
                }

                value = section["LogOutboundMessages"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.LogOutboundMessages = bool.Parse(value);
                }

                value = section["UseAlwaysResetGapFilling"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.UseAlwaysResetGapFilling = bool.Parse(value);
                }


                #region Fix Message Validation
                value = section["ValidateCheckSum"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateCheckSum = bool.Parse(value);
                }
                value = section["ValidateBodyLength"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateBodyLength = bool.Parse(value);
                }
                value = section["ValidateDuplicatedFields"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateDuplicatedFields = bool.Parse(value);
                }
                value = section["ValidateEmptyFieldValues"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateEmptyFieldValues = bool.Parse(value);
                }
                value = section["ValidateFieldValues"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateFieldValues = bool.Parse(value);
                }
                value = section["ValidateRepeatingGroupEntryCount"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateRepeatingGroupEntryCount = bool.Parse(value);
                }
                value = section["ValidateRepeatingGroupLeadingField"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateRepeatingGroupLeadingField = bool.Parse(value);
                }

                value = section["ValidateDuplicatePartyRole"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateDuplicatePartyRole = bool.Parse(value);
                }

                value = section["ValidateRequiredFields"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateRequiredFields = bool.Parse(value);
                }
                value = section["ValidateUnknownFields"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateUnknownFields = bool.Parse(value);
                }
                value = section["ValidateUnknownMessages"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.ValidateUnknownMessages = bool.Parse(value);
                }
                #endregion


                value = section["MaxMessageLength"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.MaxMessageLength = Int32.Parse(value);
                }
                if (this.MaxMessageLength < MaxMessageLength_MINVALUE)
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:MaxMessageLength. Cannot be smaller than {MaxMessageLength_MINVALUE} bytes");
                }


                value = section["MaxMessageFields"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.MaxMessageFields = Int32.Parse(value);
                }
                if (this.MaxMessageFields < MaxMessageFields_MINVALUE)
                {
                    throw new ArgumentNullException($"Invalid value for {section.Path}:MaxMessageFields. Cannot be smaller than {MaxMessageFields_MINVALUE} fields");
                }
            }
            else
            {
                if (required)
                {
                    throw new ArgumentException($"There is no {section.Path} section but is a required one");
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="theLogger"></param>
        internal void DumpSettings(Logger theLogger)
        {

            theLogger.Info($"FixClient::SessionTimerInterval = {this.SessionTimerInterval} milliseconds");
            theLogger.Info($"FixClient::ReconnectInterval = {this.TCPReconnectInterval} milliseconds");
            theLogger.Info($"FixClient::LogInboundMessages = {this.LogInboundMessages}");
            theLogger.Info($"FixClient::LogOutboundMessages = {this.LogOutboundMessages}");
            theLogger.Info($"FixClient::UseAlwaysResetGapFilling = {this.UseAlwaysResetGapFilling}");


            theLogger.Info($"FixClient::ValidateCheckSum = {this.ValidateCheckSum}");
            theLogger.Info($"FixClient::ValidateBodyLength = {this.ValidateBodyLength}");
            //theLogger.Info($"FixClient::ValidateDuplicatedFields = {this.ValidateDuplicatedFields}");
            //theLogger.Info($"FixClient::ValidateEmptyFieldValues = {this.ValidateEmptyFieldValues}");
            //theLogger.Info($"FixClient::ValidateFieldValues = {this.ValidateFieldValues}");
            theLogger.Info($"FixClient::ValidateRepeatingGroupEntryCount = {this.ValidateRepeatingGroupEntryCount}");
            //theLogger.Info($"FixClient::ValidateRepeatingGroupLeadingField = {this.ValidateRepeatingGroupLeadingField}");
            theLogger.Info($"FixClient::ValidateDuplicatePartyRole = {this.ValidateDuplicatePartyRole}");
            //theLogger.Info($"FixClient::ValidateRequiredFields = {this.ValidateRequiredFields}");
            //theLogger.Info($"FixClient::ValidateUnknownFields = {this.ValidateUnknownFields}");
            //theLogger.Info($"FixClient::ValidateUnknownMessages = {this.ValidateUnknownMessages}");

            theLogger.Info($"FixClient::MaxMessageLength = {this.MaxMessageLength}");
            theLogger.Info($"FixClient::MaxMessageFields = {this.MaxMessageFields}");
        }
    }
}
