using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace PatioFIX.Common.FixSupport
{
    public class FIXMessage
    {
        internal byte[] m_rawBytes { get; }
        internal int Length { get; private set; }

        internal FIXField[] m_fields { get; }
        public int FieldCount { get; private set; }

        bool m_validateChecksum;
        bool m_validateBodyLength;

        /// <summary> Whether this message contains a valid set of fields </summary>
        public bool Valid { get; private set; }
        /// <summary>
        /// Προκειται για ενα αντιγραφο ενος FIXMessage, στο οποιο δεν εχουμε την δυνατοτητα να
        /// το αλλαξουμε με καποιο τροπο
        /// </summary>
        public bool IsLocked { get; }

        #region field lookup optimizations
        /// <summary>
        /// MsgSeqNum (Tag = 34, Type: SeqNum)
        /// Integer message sequence number.
        /// </summary>
        public int MsgSeqNum { get; private set; }
        /// <summary>
        /// MsgType (Tag = 35, Type: String)
        /// Defines message type. ALWAYS THIRD FIELD IN MESSAGE. (Always unencrypted)
        /// </summary>
        public string MsgType { get; private set; }
        /// <summary>
        /// PossResend (Tag = 97, Type: Boolean)
        /// Indicates that message may contain information that has been sent under another sequence number.
        /// </summary>
        public bool PossResend { get; private set; }
        /// <summary>
        /// PossDupFlag (Tag = 43, Type: Boolean)
        /// Indicates possible retransmission of message with this sequence number
        /// </summary>
        public bool PossDupFlag { get; private set; }
        /// <summary>
        /// ATHEXSessionID (Tag = 5604, Type: String)
        /// User-defined field will be included specifying the unique identity of the session
        /// </summary>
        public string ATHEXSessionID { get; private set; }
        /// <summary>
        /// SecondaryOrderID (Tag = 198, Type: String)
        /// ATHEX CUSTOM: "Unique application message id. Used in recovery mechanism."
        /// </summary>
        public int SecondaryOrderID { get; private set; }

        /// <summary>
        /// SendingTime (Tag = 52, Type: UTCTimestamp)
        /// </summary>
        public bool ContainsTag52 { get; private set; }
        /// <summary>
        /// SenderCompID (Tag = 49, Type: String)
        /// </summary>
        public bool ContainsTag49 { get; private set; }
        /// <summary>
        /// TargetCompID (Tag = 56, Type: String)
        /// </summary>
        public bool ContainsTag56 { get; private set; }
        /*
         * Εαν στο message υπαρχει το 
         *      NoPartyIDs (Tag = 453, Type: NumInGroup)
         * τοτε στο IndexOfTag453 εχουμε την θεση του στο array m_fields (FIXField[]).
         * εαν δεν υπαρχει το tag αυτο, τοτε φερει την τιμη -1
         */
        public int IndexOfTag453 { get; private set; }
        #endregion


        #region special (optimized) fields if message is a MessageNote/News one
        /*
         * Εδω εχουμε μια συντομευση που μας λεει οτι το message που μολις παραλαβαμε
         * ειναι ενα 35=B^148=M (MessageNote)
         */
        public bool IsMessageNote { get; private set; }
        /*
         * Εδω εχουμε μια συντομευση που μας λεει οτι το message που μολις παραλαβαμε
         * ειναι ενα 35=B^148=M^5577=2 (MessageNote/Throttling Parameters)
         */
        public bool IsThrottlingParameters { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int TransPerSecond { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int OutstandingMsgs { get; private set; }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxLength">To μεγιστο μεγεθος σε bytes του εσωτερικου buffer</param>
        /// <param name="maxFields">Το μεγιστο πληθος των Fields που μπορει να περιεχει ενα FIXMessage</param>
        /// <param name="validateCheckSum"></param>
        /// <param name="validateBodyLength"></param>
        public FIXMessage(int maxLength = 2048, int maxFields = 128, bool validateCheckSum = true, bool validateBodyLength = true)
        {
            m_rawBytes = new byte[maxLength];
            m_fields = new FIXField[maxFields];
            m_validateChecksum = validateCheckSum;
            m_validateBodyLength = validateBodyLength;
        }

        /// <summary>
        /// Δημιουργει ένα νεο FIXMessage αντιγραφο του source
        /// </summary>
        /// <param name="source"></param>
        public FIXMessage(FIXMessage source)
        {
            this.IsLocked = true;
            this.Length = source.Length;
            this.FieldCount = source.FieldCount;
            this.m_validateChecksum = source.m_validateChecksum;
            this.m_validateBodyLength = source.m_validateBodyLength;

            #region field lookup optimizations
            this.MsgSeqNum = source.MsgSeqNum;
            this.MsgType = source.MsgType;
            this.PossResend = source.PossResend;
            this.PossDupFlag = source.PossDupFlag;
            this.ATHEXSessionID = source.ATHEXSessionID;
            this.SecondaryOrderID = source.SecondaryOrderID;
            this.ContainsTag52 = source.ContainsTag52;
            this.ContainsTag49 = source.ContainsTag49;
            this.ContainsTag56 = source.ContainsTag56;

            this.IsMessageNote = source.IsMessageNote;
            this.IsThrottlingParameters = source.IsThrottlingParameters;
            this.TransPerSecond = source.TransPerSecond;
            this.OutstandingMsgs = source.OutstandingMsgs;
            this.IndexOfTag453 = source.IndexOfTag453;
            #endregion

            this.m_rawBytes = new byte[source.Length];
            this.m_fields = new FIXField[source.FieldCount];

            this.Valid = source.Valid;

            //Αντιγραφουμε το raw bytes
            Buffer.BlockCopy(source.m_rawBytes, 0, this.m_rawBytes, 0, source.Length);
            //αντιγραφουμε τα πεδία μας:
            for (int idx = 0; idx < source.FieldCount; idx++)
            {
                this.m_fields[idx].SetExplicitValues(this.m_rawBytes, source.m_fields[idx].Tag, source.m_fields[idx]._value, source.m_fields[idx].Length, source.m_fields[idx].Checksum);
            }
        }



        /// <summary>
        /// Parses a message from a byte array, expecting an entire message to be available at the specified offset and count.
        /// Any parsing errors will result in the message being invalid where the Valid property will return false and no fields
        /// will be accessible.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="offset"></param>
        /// <param name="mlength"></param>
        /// <param name="logger"></param>
        /// <returns>A reference to this instance, built from the parsed message.</returns>
        public FIXMessage Parse(byte[] message, int offset, int mlength, Logger logger)
        {
            if (this.IsLocked)
            {
                throw new InvalidOperationException("FixMessage is Locked");
            }

            bool _messageParsingFinished = false;
            try
            {
                if (mlength < 60)
                {
                    throw new Exception("MESSAGE_LENGTH_TOO_SMALL  (< 60 bytes)");
                }
                if (mlength > this.m_rawBytes.Length)
                {
                    throw new Exception($"MESSAGE_LENGTH_TOO_BIG  (> {this.m_rawBytes.Length} bytes)");
                }

                Buffer.BlockCopy(message, offset, m_rawBytes, 0, mlength);
                this.Length = mlength;
                this.FieldCount = 0;
                this.Valid = false;
                this.MsgSeqNum = -1;                //REQUIRED FIELD, default value of -1 in purpose! Μην αλλαχτει ποτε.
                                                    //Εαν δεν παρει τιμη απο το incoming mesage, το -1 θα προκαλεσει PtFixFatalException στον FixClient που ειναι το επιθυμητο
                this.MsgType = "@";                 //REQUIRED FIELD, default value τετοιο ωστε να ειναι unknown MessageType.
                this.PossResend = false;            //NON-REQUIRED FIELD, NEEDS DEFAULT VALUE
                this.PossDupFlag = false;           //NON-REQUIRED FIELD, NEEDS DEFAULT VALUE
                this.ATHEXSessionID = null;         //NON-REQUIRED FIELD, NEEDS DEFAULT VALUE
                this.SecondaryOrderID = 0;          //NON-REQUIRED FIELD, NEEDS DEFAULT VALUE
                this.ContainsTag52 = false;
                this.ContainsTag49 = false;
                this.ContainsTag56 = false;
                this.IndexOfTag453 = -1;
                this.IsMessageNote = false;
                this.IsThrottlingParameters = false;
                this.TransPerSecond = -1;
                this.OutstandingMsgs = -1;


                var length = 0;
                var checksum = 0;
                for (var position = 0; position < Length; position++)
                {
                    try
                    {
                        var field = ParseField(this.m_rawBytes, ref position);

                        m_fields[FieldCount++] = field;

                        length += field.Length;
                        checksum += field.Checksum;

                        #region field lookup optimizations
                        if (field.Tag == Tags.MsgSeqNum)
                        {
                            this.MsgSeqNum = field.AsInt;
                        }
                        else if (field.Tag == Tags.MsgType)
                        {
                            this.MsgType = field.AsString;
                        }
                        else if (field.Tag == Tags.PossDupFlag)
                        {
                            this.PossDupFlag = field.AsBoolean;
                        }
                        else if (field.Tag == Tags.PossResend)
                        {
                            this.PossResend = field.AsBoolean;
                        }
                        else if (field.Tag == Tags.SecondaryOrderID)
                        {
                            this.SecondaryOrderID = field.AsInt;
                        }
                        else if (field.Tag == CustomTags.ATHEXSessionID)
                        {
                            this.ATHEXSessionID = field.AsString;
                        }
                        else if (field.Tag == Tags.SendingTime)
                        {
                            this.ContainsTag52 = true;
                        }
                        else if (field.Tag == Tags.SenderCompID)
                        {
                            this.ContainsTag49 = true;
                        }
                        else if (field.Tag == Tags.TargetCompID)
                        {
                            this.ContainsTag56 = true;
                        }
                        else if (field.Tag == Tags.NoPartyIDs)
                        {
                            this.IndexOfTag453 = (FieldCount - 1);
                        }
                        else if (field.Tag == Tags.Headline)
                        {
                            /*
                             * To tag148 (HeadLine) εμφανιζεται μονο σε messages με MsgType 'B',δηλαδη News.
                             */
                            if (field.AsChar == 'M')
                            {
                                this.IsMessageNote = true;
                            }
                        }
                        else if (field.Tag == CustomTags.NoteType)
                        {
                            /*
                             * To custom tag5577 (NoteType) εμφανιζεται μονο σε messages με MsgType 'B',δηλαδη News.
                             */
                            if (field.AsChar == '2')
                            {
                                this.IsThrottlingParameters = true;
                            }
                        }
                        else if (field.Tag == CustomTags.TransPerSecond)
                        {
                            this.TransPerSecond = field.AsInt;
                        }
                        else if (field.Tag == CustomTags.OutstandingMsgs)
                        {
                            this.OutstandingMsgs = field.AsInt;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error at position {position} -> {ex.Message}");
                    }

                    if (FieldCount > this.m_fields.Length)
                    {
                        throw new Exception($"TOO_MANY_FIELDS  (> {this.m_fields.Length})");
                    }
                }

                _messageParsingFinished = true;

                // Validate message (any errors at this point result in an invalid/garbled message) 
                if (m_fields[0].Tag != 8) throw new Exception("BeginString field not found at expected position");
                if (m_fields[1].Tag != 9) throw new Exception("BodyLength field not found at expected position");
                if (m_fields[2].Tag != 35) throw new Exception("MsgType field not found at expected position");
                if (m_fields[FieldCount - 1].Tag != 10) throw new Exception("CheckSum field not found at expected position");

                if (this.m_validateBodyLength)
                {
                    length = length - m_fields[0].Length - m_fields[1].Length - m_fields[FieldCount - 1].Length;
                    if (m_fields[1].AsInt != length) throw new Exception($"BodyLength of the message does not match (expected {length})");
                }

                if (this.m_validateChecksum)
                {
                    checksum = (checksum - m_fields[FieldCount - 1].Checksum) % 256;
                    if (m_fields[FieldCount - 1].AsInt != checksum) throw new Exception($"CheckSum of the message does not match (expected {checksum})");
                }

                Valid = true;
            }
            catch (Exception e)
            {
                if (_messageParsingFinished)
                {
                    logger?.Warning($"Parsing failed: '{e.Message}' [{this.ToString()}]");
                }
                else
                {
                    var sb = new StringBuilder($"Parsing failed: '{e.Message}' [");
                    for (int idx = offset; idx < mlength; idx++)
                    {
                        if (idx > offset)
                            sb.Append(",");
                        sb.AppendFormat("0x{0:X}", message[idx]);
                    }
                    sb.Append("]");

                    logger?.Warning(sb.ToString());
                }

                Valid = false;
                FieldCount = 0;
            }

            return this;
        }

        /// <summary>
        /// Parses a fix field from the message from the provided position. Parsing will fail with an error
        /// if an illegal character is seen whilst parsing the tag or value.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="position">The position to parse from.</param>
        /// <returns>The parsed field.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FIXField ParseField(byte[] message, ref int position)
        {
            var length = 0;
            var checksum = 0;

            // Parse next tag and value from the message
            var tag = ParseTag(message, ref position, ref length, ref checksum);
            var value = ParseValue(tag, message, ref position, ref length, ref checksum);

            // Create the relevant field
            return new FIXField(message, tag, value, length, checksum);
        }

        /// <summary>
        /// Parses a fix tag from the message from the provided position. Parsing will fail with an error
        /// if an illegal character is seen before the tag is terminated.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="position">The position to parse from.</param>
        /// <param name="length">The current length of the whole field</param>
        /// <param name="checksum">The current checksum of the whole field</param>
        /// <returns>The tag number as an integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int ParseTag(byte[] message, ref int position, ref int length, ref int checksum)
        {
            var tag = 0;

            for (; position < Length; position++)
            {
                var b = message[position];

                length++;
                checksum += b;

                if (b == '=') break;
                if (b < '0' || b > '9')
                {
                    throw new Exception($"INVALID_CHARACTER_INSIDE_TAG '{(char)b}'");
                }

                tag *= 10;
                tag += b - '0';
            }

            position += 1;

            if (tag == 0)
            {
                throw new Exception("INVALID_TAG");
            }
            return tag;
        }

        /// <summary>
        /// Parses a fix value from the message from the provided position. Parsing will fail with an error
        /// if an illegal character is seen before the value is terminated.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message">The message to parse.</param>
        /// <param name="position">The position to parse from.</param>
        /// <param name="length">The current length of the whole field</param>
        /// <param name="checksum">The current checksum of the whole field</param>
        /// <returns>The value as a segment of the original message.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Segment ParseValue(int tag, byte[] message, ref int position, ref int length, ref int checksum)
        {
            var offset = position;

            for (; position < Length; position++)
            {
                var b = message[position];

                checksum += b;

                if (b == '\u0001') break;
                //if (b == '=') throw new Exception("Not a valid value"); // TODO: Check whether this is legit?
            }

            var valueLength = position - offset;
            if (valueLength == 0)
            {
                throw new Exception($"INVALID_VALUE for tag{tag}");
            }

            length += valueLength + 1;

            return new Segment(offset, valueLength);
        }

        /// <summary>
        /// Retrieves the field with the specified tag, optionally retrieving the specified instance (if there are groups).
        /// </summary>
        /// <param name="tag">The tag of the field to retrieve.</param>
        /// <param name="instance">The instance of the field (if there are multiple).</param>
        /// <returns>The field.</returns>
        public FIXField this[int tag, int instance = 0]
        {
            get
            {
                // NOTE: Optimized for retrieving expected header/trailer fields

                if (!Valid) throw new Exception("Field not found");

                switch (tag)
                {
                    case 8: return m_fields[0];
                    case 9: return m_fields[1];
                    case 35: return m_fields[2];
                    case 10: return m_fields[FieldCount - 1];
                    default:
                        // Linear search for relevant field (don't worry it's pretty fast)
                        for (var i = 3; i < FieldCount - 1; i++)
                        {
                            if (m_fields[i].Tag == tag && --instance < 0)
                            {
                                return m_fields[i];
                            }
                        }
                        throw new Exception($"Field with Tag={tag}, not found");
                }
            }
        }

        /// <summary>
        /// Returns true if a field with the specified tag, as the specified instance, exists.
        /// </summary>
        /// <param name="tag">The tag of the field to check.</param>
        /// <param name="instance">The instance of the field (if there are multiple).</param>
        public bool Contains(int tag, int instance = 0)
        {
            if (!Valid) return false;

            switch (tag)
            {
                case 8:
                case 9:
                case 35:
                case 10:
                    return true;
                case 49:
                    return this.ContainsTag49;
                case 52:
                    return this.ContainsTag52;
                case 56:
                    return this.ContainsTag56;
                default:
                    // Linear search for relevant field (don't worry it's pretty fast) 
                    for (var i = 3; i < FieldCount - 1; i++)
                        if (m_fields[i].Tag == tag && --instance < 0)
                            return true;
                    return false;
            }
        }

        /// <summary>
        /// Clears the message for reuse
        /// </summary>
        public void Clear()
        {
            if (this.IsLocked)
            {
                throw new InvalidOperationException("FixMessage is Locked");
            }

            FieldCount = 0;
            Length = 0;
            Valid = false;
        }

        public override string ToString()
        {
            return CharEncoding.DefaultEncoding.GetString(m_rawBytes, 0, Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        public void WriteTo(byte[] target, int offset)
        {
            Buffer.BlockCopy(m_rawBytes, 0, target, offset, Length);
        }
    }
}