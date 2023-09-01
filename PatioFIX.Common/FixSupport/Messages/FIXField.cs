using System;

namespace PatioFIX.Common.FixSupport
{
    public struct FIXField
    {
        internal byte[] _message;
        internal Segment _value;

        /// <summary> The tag of this field </summary>
        public int Tag { get; internal set; }
        /// <summary> The length of this field </summary>
        public int Length { get; internal set; }
        /// <summary> The checksum of this field </summary>
        public int Checksum { get; internal set; }


        /// <summary> Returns the field value as a 32 bit number </summary>
        public int AsInt => _message.ReadInt(_value.Offset, _value.Length, Tag);
        /// <summary> Returns the field value as a 64 bit number </summary>
        public long AsLong => _message.ReadLong(_value.Offset, _value.Length, Tag);
        /// <summary> Returns the field value as a decimal number </summary>
        public double AsFloat => _message.ReadFloat(_value.Offset, _value.Length, Tag);
        /// <summary> Returns the field value as a string </summary>
        public string AsString => _message.ReadStringAsGreek(_value.Offset, _value.Length, Tag);
        /// <summary>
        /// 
        /// </summary>
        public char AsChar => _message.ReadChar(_value.Offset, _value.Length, Tag);
        /// <summary>
        /// 
        /// </summary>
        public bool AsBoolean => _message.ReadBoolean(_value.Offset, _value.Length, Tag);
        /// <summary> Returns the field value as a datetime </summary>
        public DateTime AsDateTime => _message.ReadDateTime(_value.Offset, _value.Length, Tag);

        /// <summary>
        /// Παιρνει το FIX timestamp και επιστρεφει πισω ενα μήκους 20 χαρακτηρων ODL Timestamp
        /// </summary>
        public string AsODLTimestamp => _message.ReadODLTimestamp(_value.Offset, _value.Length, Tag);


		/// <summary>
		/// Παιρνει το FIX timestamp και επιστρεφει πισω ενα μήκους 20 χαρακτηρων ODL Timestamp
		/// </summary>
		public string AsODLTimestampLocal => _message.ReadODLTimestampLocal(_value.Offset, _value.Length, Tag, Globals.UTCOffset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <param name="checksum"></param>
        public FIXField(byte[] message, int tag, Segment value, int length, int checksum)
        {
            _message = message;
            _value = value;

            Tag = tag;
            Length = length;
            Checksum = checksum;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <param name="checksum"></param>
        internal void SetExplicitValues(byte[] message, int tag, Segment value, int length, int checksum)
        {
            _message = message;
            _value = value;

            Tag = tag;
            Length = length;
            Checksum = checksum;
        }



        /// <summary>
        /// Returns true if the field value is equal to the specified int, false if the value
        /// is not a valid int or it isn't equal to the specified value.
        /// <remarks>
        /// This operation is garbage free.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public bool Is(int value)
        {
            // TODO: Implement without using exceptions

            try
            {
                return this.AsInt == value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the field value is equal to the specified long, false if the value
        /// is not a valid long or it isn't equal to the specified value.
        /// <remarks>
        /// This operation is garbage free.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public bool Is(long value)
        {
            // TODO: Implement without using exceptions

            try
            {
                return this.AsLong == value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the field value is equal to the specified double, false if the value
        /// is not a valid double or it isn't equal to the specified value.
        /// <remarks>
        /// This operation is garbage free.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public bool Is(double value)
        {
            // TODO: Implement without using exceptions

            try
            {
                return this.AsFloat == value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the field value is equal to the specified string, false if the value
        /// is not a valid string or it isn't equal to the specified value.
        /// <remarks>
        /// This operation is garbage free.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public bool Is(string value)
        {
            if (_value.Length != value.Length) return false;

            for (var i = 0; i < _value.Length; i++)
            {
                if (_message[_value.Offset + i] != value[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the field value is equal to the specified datetime, false if the value
        /// is not a valid datetime or it isn't equal to the specified value.
        /// <remarks>
        /// This operation is garbage free.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to compare to.</param>
        /// <returns>The result of the comparison.</returns>
        public bool Is(DateTime value)
        {
            // TODO: Implement without using exceptions

            try
            {
                return this.AsDateTime == value;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Tag}={AsString}";
        }
    }
}