using System;
using System.Runtime.CompilerServices;

namespace PatioFIX.Common.FixSupport
{
    public static class FIXEncoding
    {
        /*
         * Αυτος ειναι ο πινακας αντιστοιχισης της codepage iso-8859-7 ετσι οπως ειναι στο .ΝΕΤ 6
         * Μπορουμε να δουμε τον πινακ αυτον με τον εξης κωδικα:
         * 
         *              Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
         *              var enc = Encoding.GetEncoding("iso-8859-7");
         * 
         *              var values = new byte[256];
         *              for(int i=0; i<=255; i++)
         *              {
         *                  values[i] = (byte)i;
         *              }
         *              
         *              var decoder = enc.GetDecoder();
         *              var letters = new char[256];
         *              decoder.GetChars(values, 0, 256, letters, 0);
         *              
         *  O πίνακας είναι μεσα στο letters!!
         *  O πινακς κανει mapping ASCII χαρακτηρες σε Unicode (UTF-16) χαρατηρες
         */
        static char[] _iso_8859_7 = {
                '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000f', '\u0010', '\u0011',
                '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', '\u0020', '\u0021', '\u0022', '\u0023',
                '\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', '\u002a', '\u002b', '\u002c', '\u002d', '\u002e', '\u002f', '\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035',
                '\u0036', '\u0037', '\u0038', '\u0039', '\u003a', '\u003b', '\u003c', '\u003d', '\u003e', '\u003f', '\u0040', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047',
                '\u0048', '\u0049', '\u004a', '\u004b', '\u004c', '\u004d', '\u004e', '\u004f', '\u0050', '\u0051', '\u0052', '\u0053', '\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059',
                '\u005a', '\u005b', '\u005c', '\u005d', '\u005e', '\u005f', '\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', '\u006a', '\u006b',
                '\u006c', '\u006d', '\u006e', '\u006f', '\u0070', '\u0071', '\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007a', '\u007b', '\u007c', '\u007d',
                '\u007e', '\u007f', '\u0080', '\u0081', '\u0082', '\u0083', '\u0084', '\u0085', '\u0086', '\u0087', '\u0088', '\u0089', '\u008a', '\u008b', '\u008c', '\u008d', '\u008e', '\u008f',
                '\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', '\u0096', '\u0097', '\u0098', '\u0099', '\u009a', '\u009b', '\u009c', '\u009d', '\u009e', '\u009f', '\u00a0', '\u02bd',
                '\u02bc', '\u00a3', '\uf7c2', '\uf7c3', '\u00a6', '\u00a7', '\u00a8', '\u00a9', '\uf7c4', '\u00ab', '\u00ac', '\u00ad', '\uf7c5', '\u2015', '\u00b0', '\u00b1', '\u00b2', '\u00b3',
                '\u0384', '\u0385', '\u0386', '\u00b7', '\u0388', '\u0389', '\u038a', '\u00bb', '\u038c', '\u00bd', '\u038e', '\u038f', '\u0390', '\u0391', '\u0392', '\u0393', '\u0394', '\u0395',
                '\u0396', '\u0397', '\u0398', '\u0399', '\u039a', '\u039b', '\u039c', '\u039d', '\u039e', '\u039f', '\u03a0', '\u03a1', '\uf7c6', '\u03a3', '\u03a4', '\u03a5', '\u03a6', '\u03a7',
                '\u03a8', '\u03a9', '\u03aa', '\u03ab', '\u03ac', '\u03ad', '\u03ae', '\u03af', '\u03b0', '\u03b1', '\u03b2', '\u03b3', '\u03b4', '\u03b5', '\u03b6', '\u03b7', '\u03b8', '\u03b9',
                '\u03ba', '\u03bb', '\u03bc', '\u03bd', '\u03be', '\u03bf', '\u03c0', '\u03c1', '\u03c2', '\u03c3', '\u03c4', '\u03c5', '\u03c6', '\u03c7', '\u03c8', '\u03c9', '\u03ca', '\u03cb',
                '\u03cc', '\u03cd', '\u03ce', '\uf7c7'
            };

        public static long[] Powers =
        {
            1,
            10,
            100,
            1000,
            10000,
            100000,
            1000000,
            10000000,
            100000000,
            1000000000,
            10000000000,
            100000000000,
            1000000000000
        };


        /// <summary>
        /// Parses an integer (Int32) from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance, garbage free implementation (5x faster than bcl).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this byte[] source, int offset, int count, int forTag)
        {
            var value = 0;
            var sign = source[offset] == '-' ? -1 : +1;

            var skip = sign < 0 ? 1 : 0;

            for (var i = 0; i < count - skip; i++)
            {
                var b = source[offset + skip + i];

                if (b < '0' || b > '9') throw new Exception($"NOT_VALID_INTEGER_NUMBER (Tag{forTag})");

                value *= 10;
                value += b - '0';
            }

            return value * sign;
        }


        /// <summary>
        /// Parses a long (Int64) from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance, garbage free implementation (5x faster than bcl).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadLong(this byte[] source, int offset, int count, int forTag)
        {
            var value = 0L;
            var sign = source[offset] == '-' ? -1 : +1;

            var skip = sign < 0 ? 1 : 0;

            for (var i = 0; i < count - skip; i++)
            {
                var b = source[offset + skip + i];

                if (b < '0' || b > '9') throw new Exception($"NOT_VALID_LONG_NUMBER (Tag{forTag})");

                value *= 10;
                value += b - '0';
            }

            return value * sign;
        }



        /// <summary>
        /// Parses an float (double) from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance, garbage free implementation (3x faster than bcl).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed double.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadFloat(this byte[] source, int offset, int count, int forTag)
        {
            var value = 0L;
            var exponent = 0d;
            var sign = source[offset] == '-' ? -1 : +1;

            var skip = sign < 0 ? 1 : 0;

            for (var i = 0; i < count - skip; i++)
            {
                var b = source[offset + skip + i];

                if (b < '0' || b > '9')
                {
                    if (b != '.') throw new Exception($"NOT_VALID_FLOAT_NUMBER (Tag{forTag})");

                    exponent = 1d;
                    continue;
                }

                value *= 10;
                value += b - '0';
                exponent *= 10;
            }

            if (exponent == 0d) exponent = 1d;

            return sign * (value / exponent);
        }



        /// <summary>
        /// Parses a string from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance implementation (allocates a new string).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string ReadString(this byte[] source, int offset, int count, int forTag)
        {
            char* chars = stackalloc char[count + 1];

            for (var i = 0; i < count; i++)
            {
                *(chars + i) = (char)source[offset + i];
            }

            chars[count] = '\0';

            var s = new string(chars);

            return s;
        }


        /// <summary>
        /// Parses a string from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance implementation (allocates a new string).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string ReadStringAsGreek(this byte[] source, int offset, int count, int forTag)
        {
            char* chars = stackalloc char[count + 1];

            for (var i = 0; i < count; i++)
            {
                *(chars + i) = _iso_8859_7[source[offset + i]];
            }

            chars[count] = '\0';

            var s = new string(chars);

            return s;
        }

        /// <summary>
        /// Single character value, can include any alphanumeric character or punctuation except the delimiter. 
        /// All char fields are case sensitive (i.e. m != M). 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="forTag">The associated tag</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadChar(this byte[] source, int offset, int count, int forTag)
        {
            if (count != 1)
            {
                throw new Exception($"NOT_VALID_SINGLE_CHARACTER (Tag{forTag}, value more than 1 characters)");
            }
            return (char)source[offset];
        }
        /// <summary>
        /// Char field containing one of two values: 'Y' = True/Yes, 'N' = False/No.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="forTag">The associated tag</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBoolean(this byte[] source, int offset, int count, int forTag)
        {
            if (count != 1)
            {
                throw new Exception($"NOT_VALID_BOOLEAN (Tag{forTag}, value more than 1 characters)");
            }

            if (source[offset] == 'Y')
                return true;
            if (source[offset] == 'N')
                return false;

            throw new Exception($"NOT_VALID_BOOLEAN (Tag{forTag}, not expected value '{source[offset]}')");
        }



        /// <summary>
        /// Parses a datetime from the provided byte array.
        /// </summary>
        /// <remarks>
        /// High performance, garbage free implementation (10x faster than bcl).
        /// </remarks>
        /// <param name="source">The source string to parse.</param>
        /// <param name="offset">The offset in the string to start parsing from.</param>
        /// <param name="count">The number of characters to parse.</param>
        /// <param name="forTag">The associated tag</param>
        /// <returns>The parsed datetime.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ReadDateTime(this byte[] source, int offset, int count, int forTag)
        {
            if (count != 17 && count != 21 && count != 24) throw new Exception($"NOT_VALID_DATETIME (Tag{forTag})");

            //if (source[ 8] != '-') throw new Exception("Not a valid datetime");
            //if (source[11] != ':') throw new Exception("Not a valid datetime");
            //if (source[14] != ':') throw new Exception("Not a valid datetime");
            //if (source[17] != '.') throw new Exception("Not a valid datetime");

            var year = source.ReadInt(offset + 00, 4, forTag);
            var month = source.ReadInt(offset + 04, 2, forTag);
            var day = source.ReadInt(offset + 06, 2, forTag);
            var hour = source.ReadInt(offset + 09, 2, forTag);
            var minute = source.ReadInt(offset + 12, 2, forTag);
            var second = source.ReadInt(offset + 15, 2, forTag);
            var millis = count == 21 ? source.ReadInt(offset + 18, 3, forTag) : 0;

            return new DateTime(year, month, day, hour, minute, second, millis);
        }

        /// <summary>
        /// Παιρνει το FIX timestamp και επιστρεφει πισω ενα μήκους 20 χαρακτηρων ODL Timestamp
        /// </summary>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="forTag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string ReadODLTimestamp(this byte[] source, int offset, int count, int forTag)
        {
            char* chars = stackalloc char[20 + 1];

            int idx = 0;
            for (var i = 0; i < count; i++)
            {
                char _b = (char)source[offset + i];

                if (_b != '-' && _b != ':' && _b != '.')
                {
                    *(chars + idx) = _b;
                    idx++;

                    if (idx >= 20)
                        break;
                }
            }

            chars[idx] = '\0';

            var s = new string(chars);

            return s;
        }

        /*
         * Notes:
         * 
         * There are a few ways of converting numbers to bytes, including:
         * 
         * Option 1:
         * 
         * Start from the left, printing the digits from the most to the least significant digit
         * 
         * In order to figure out what to print starting from the most significant either:
         * 
         *   a) Loop until you find the biggest divisor
         *   
         *   b) Calculate it by doing Floor(Log10(Abs(number)))
         * 
         * Option 2 (implemented):
         * 
         * Start from the right, printing the digits in reverse order, then reversing the bytes
         * 
         * Option 3:
         * 
         * Use the ToString() method, however this generates garbage
         * 
         * 
         * Option 1a seems to be faster than option 3 for integers, with better performance the smaller the number
         * Option 2 seems to be faster than options 1a and 3 for integers
         * 
         * Option 1a seems to be slower than option 3 for longs
         * Option 2 seems to be faster than options 1a and 3 for longs
         */

        /// <summary>
        /// Writes an integer to the given byte array.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position to start writing.</param>
        /// <param name="value">The number to write.</param>
        /// <returns>The number of characters written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteInt(this byte[] buffer, int position, int value)
        {
            if (value == 0)
            {
                buffer[position] = (byte)'0';
                return 1;
            }

            var initial = position;
            var negative = false;

            if (value < 0)
            {
                negative = true;
                value = -value; // Note: Code won't work for int.MinValue
            }

            while (value > 0)
            {
                var n = '0' + (char)(value % 10);
                buffer[position++] = (byte)n;
                value /= 10;
            }

            if (negative) buffer[position++] = (byte)'-';

            for (var i = 0; i < (position - initial) / 2; i++)
            {
                var temp = buffer[initial + i];
                buffer[initial + i] = buffer[position - i - 1];
                buffer[position - i - 1] = temp;
            }

            return position - initial;
        }

        /// <summary>
        /// Writes an integer to the given byte array. 
        /// The number is written before the given position, so that the last digit is at that position.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position that marks the end of the number.</param>
        /// <param name="value">The number to write.</param>
        /// <returns>The number of characters written</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteIntBackwards(this byte[] buffer, int position, int value)
        {
            if (value == 0)
            {
                buffer[position] = (byte)'0';
                return 1;
            }

            var initial = position;
            var negative = false;

            if (value < 0)
            {
                negative = true;
                value = -value; // Note: Code won't work for int.MinValue
            }

            while (value > 0)
            {
                var n = '0' + (char)(value % 10);
                buffer[position--] = (byte)n;
                value /= 10;
            }

            if (negative) buffer[position--] = (byte)'-';

            return initial - position;
        }

        /// <summary>
        /// Writes a long to the given byte array.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position to start writing.</param>
        /// <param name="value">The number to write.</param>
        /// <param name="negative"></param>
        /// <returns>The number of characters written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteLong(this byte[] buffer, int position, long value, bool negative)
        {
            if (value == 0)
            {
                if (negative)
                {
                    buffer[position] = (byte)'-';
                    buffer[position + 1] = (byte)'0';
                    return 2;
                }
                else
                {
                    buffer[position] = (byte)'0';
                    return 1;
                }
            }

            var initial = position;

            if (negative == true)
            {
                value = -value; // Note: Code won't work for long.MinValue
            }

            while (value > 0)
            {
                var n = '0' + (char)(value % 10);
                buffer[position++] = (byte)n;
                value /= 10;
            }

            if (negative) buffer[position++] = (byte)'-';

            for (var i = 0; i < (position - initial) / 2; i++)
            {
                var temp = buffer[initial + i];
                buffer[initial + i] = buffer[position - i - 1];
                buffer[position - i - 1] = temp;
            }

            return position - initial;
        }

        /// <summary>
        /// Writes a long to the given byte array. 
        /// The number is written before the given position, so that the last digit is at that position.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position that marks the end of the number.</param>
        /// <param name="value">The number to write.</param>
        /// <returns>The number of characters written</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteLongBackwards(this byte[] buffer, int position, long value)
        {
            if (value == 0)
            {
                buffer[position] = (byte)'0';
                return 1;
            }

            var initial = position;
            var negative = false;

            if (value < 0)
            {
                negative = true;
                value = -value; // Note: Code won't work for long.MinValue
            }

            while (value > 0)
            {
                var n = '0' + (char)(value % 10);
                buffer[position--] = (byte)n;
                value /= 10;
            }

            if (negative) buffer[position--] = (byte)'-';

            return initial - position;
        }

        /// <summary>
        /// Writes a double to the given byte array. 
        /// The number is rounded to the specified number of decimal places.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position to start writing.</param>
        /// <param name="value">The number to write.</param>
        /// <param name="decimals">The number of decimal places to round to.</param>
        /// <returns>The number of characters written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteFloat(this byte[] buffer, int position, double value, int decimals)
        {
            var start = position;
            var power = Powers[decimals];
            var integer = (long)value;
            var fraction = (long)Math.Round(Math.Abs(value) * power) % power;

            /*
             * εαν εχουμε ενα value οπως -0.001, το integer ειναι μηδεν (0) και εχει χασει το minus sign
             * για αυτο κοιταμε παντα το αρχικο value εαν ειναι αρνητικο...
             */
            position += buffer.WriteLong(position, integer, negative: value >= 0 ? false : true);

            if (decimals == 0) return position - start;

            buffer[position++] = (byte)'.';
            var written = buffer.WriteLongBackwards(position + decimals - 1, fraction);

            for (var i = position; i < position + decimals - written; i++)
            {
                buffer[i] = (byte)'0';
            }

            return position - start + decimals;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteDecimal(this byte[] buffer, int position, decimal value, int decimals)
        {
            var start = position;
            var power = Powers[decimals];
            var integer = (long)value;
            var fraction = (long)Math.Round(Math.Abs(value) * power) % power;

            /*
             * εαν εχουμε ενα value οπως -0.001, το integer ειναι μηδεν (0) και εχει χασει το minus sign
             * για αυτο κοιταμε παντα το αρχικο value εαν ειναι αρνητικο...
             */
            position += buffer.WriteLong(position, integer, negative: value >= 0 ? false : true);

            if (decimals == 0) return position - start;

            buffer[position++] = (byte)'.';
            var written = buffer.WriteLongBackwards(position + decimals - 1, fraction);

            for (var i = position; i < position + decimals - written; i++)
            {
                buffer[i] = (byte)'0';
            }

            return position - start + decimals;
        }

        /// <summary>
        /// Writes a string to the given byte array.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position to start writing.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The number of characters written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteString(this byte[] buffer, int position, string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                buffer[position + i] = (byte)value[i];
            }

            return value.Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteChar(this byte[] buffer, int position, char value)
        {
            buffer[position] = (byte)value;

            return 1;
        }

        /// <summary>
        /// Writes a date and time to the given byte array.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="position">The position to start writing.</param>
        /// <param name="value">The date and time to write.</param>
        /// <returns>The number of characters written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteDateTime(this byte[] buffer, int position, DateTime value)
        {
            var year = value.Year;
            buffer[position] = (byte)('0' + year / 1000);
            year %= 1000;
            buffer[position + 1] = (byte)('0' + year / 100);
            year %= 100;
            buffer[position + 2] = (byte)('0' + year / 10);
            year %= 10;
            buffer[position + 3] = (byte)('0' + year);

            var month = value.Month;
            buffer[position + 4] = (byte)('0' + month / 10);
            month %= 10;
            buffer[position + 5] = (byte)('0' + month);

            var day = value.Day;
            buffer[position + 6] = (byte)('0' + day / 10);
            day %= 10;
            buffer[position + 7] = (byte)('0' + day);

            buffer[position + 8] = (byte)'-';

            var hour = value.Hour;
            buffer[position + 9] = (byte)('0' + hour / 10);
            hour %= 10;
            buffer[position + 10] = (byte)('0' + hour);

            buffer[position + 11] = (byte)':';

            var minute = value.Minute;
            buffer[position + 12] = (byte)('0' + minute / 10);
            minute %= 10;
            buffer[position + 13] = (byte)('0' + minute);

            buffer[position + 14] = (byte)':';

            var second = value.Second;
            buffer[position + 15] = (byte)('0' + second / 10);
            second %= 10;
            buffer[position + 16] = (byte)('0' + second);

            buffer[position + 17] = (byte)'.';

            var millis = value.Millisecond;
            buffer[position + 18] = (byte)('0' + millis / 100);
            millis %= 100;
            buffer[position + 19] = (byte)('0' + millis / 10);
            millis %= 10;
            buffer[position + 20] = (byte)('0' + millis);

            return 21;
        }
    }
}
