using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// Εδω εχω όλες εκεινες τις μεθόδους που με βοηθάνε να μορφοποιω (formatting) ή 
    /// να απομορφοποιω (decomposition) τα πεδια που πανε και ερχονται απο το 
    /// ODL Client API (COM Object)
    /// </summary>
    public static class ODLClientAPIUtilities
    {

        public static string _trim(string value)
        {
            if (value == null)
                return string.Empty;
            return value.Trim();
        }

        public static string _substring(string value, int start, int length)
        {
            if (value == null || start > value.Length - 1)
                return string.Empty;

            if (start + length > value.Length)
            {
                length = value.Length - start;
            }

            return value.Substring(start, length);
        }

        //public static string _substring(string value, int start, int length)
        //{
        //    if (value == null || start > value.Length - 1)
        //        return String.Empty;

        //    if (start + length > value.Length)
        //    {
        //        length = value.Length - start;
        //    }

        //    char[] buffer = new char[length];
        //    for (int i = 0; i < length; i++)
        //    {
        //        buffer[i] = value[start + i];
        //    }

        //    return new string(buffer);
        //}

        public static string _RemoveSpecialCharacters(string input)
        {
            if (input == null)
                return input;
            input = input.Trim();

            var buffer = new char[input.Length];
            int bidx = 0, idx = 0;
            for (; idx < input.Length; idx++)
            {
                char c = input[idx];
                if (c == ',' || c == '"' || c == '\'')
                    continue;
                buffer[bidx++] = c;
            }

            if (bidx == idx)
                return input;

            return new string(buffer, 0, bidx);
        }
        public static string _ReturnPureField(string input)
        {
            if (input == null)
                return "0";

            var buffer = new char[input.Length];
            int bidx = 0, idx = 0;
            for (; idx < input.Length; idx++)
            {
                char c = input[idx];
                if (Char.IsDigit(c))
                    buffer[bidx++] = c;
            }

            if (bidx == 0)
                return "0";
            if (bidx == idx)
                return input;

            return new string(buffer, 0, bidx);
        }


        #region format functions for message field types

        /// <summary>
        /// The alpha fields have to be aligned to the left and padded with spaces. For example, 
        /// in the message used for the entry of a new order, the field Member Order Number is defined as 
        /// alpha having a total width of 12 bytes. If the desired value of this field is XYZ, then we fill the 
        /// field (left to right) with XYZ and then continue appending nine (9) spaces.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string _FormatAlphaField(string input, int size)
        {
            var buffer = new char[size];

            //fill buffer with zeroes...
            for (int idx = 0; idx < buffer.Length; idx++)
                buffer[idx] = ' ';


            if (String.IsNullOrWhiteSpace(input))
                return new string(buffer);

            input = input.Trim();
            for (int idx = 0; idx < buffer.Length; idx++)
            {
                if (idx > (input.Length - 1))
                    break;

                buffer[idx] = input[idx];
            }

            return new string(buffer);
        }

        #endregion
    }
}
