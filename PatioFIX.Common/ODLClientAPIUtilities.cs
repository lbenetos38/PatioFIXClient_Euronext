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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string _trim(string value)
        {
            if (value == null)
                return string.Empty;
            return value.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static unsafe string _RemoveSpecialCharacters(string value)
        {
            if (value == null)
                return value;
            value = value.Trim();

            var buffer = stackalloc char[value.Length];
            int bidx = 0, idx = 0;
            for (; idx < value.Length; idx++)
            {
                char c = value[idx];
                if (c == ',' || c == '"' || c == '\'')
                    continue;
                buffer[bidx++] = c;
            }

            if (bidx == idx)
                return value;

            return new string(buffer, 0, bidx);
        }

		/// <summary>
		/// Αφαιρει απο το value ολους τους χαρακτηρες που δεν ειναι νουμερα.
		/// Εαν δεν μεινει καποιος χαρακτηρας στο value τοτε επιστρεφει "0"
		/// <para>Χρησιμοποιειται για να βεβαιωθουμε οτι το CSDAccountID περιεχει μονο αριθμους οταν το αποθηκευσουμε στην βαση (ODL).
        /// Εδω ειναι το σημειο που accounts οπως 'MMCL', 'FTSPOT','PPCMMCL','ERR_EB1_EB1' κ.α. μετατρεπονται σε "0" </para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static unsafe string _FilterOutNonNumericCharacters(string value)
        {
            if (value == null)
                return "0";

            var buffer = stackalloc char[value.Length];
            int bidx = 0, idx = 0;
            for (; idx < value.Length; idx++)
            {
                char c = value[idx];
				if (Char.IsBetween(c, '0', '9'))
					buffer[bidx++] = c;
			}

            if (bidx == 0)
                return "0";
            if (bidx == idx)
                return value;

            return new string(buffer, 0, bidx);
        }



        /// <summary>
        /// The alpha fields have to be aligned to the left and padded with spaces. For example, 
        /// in the message used for the entry of a new order, the field Member Order Number is defined as 
        /// alpha having a total width of 12 bytes. If the desired value of this field is XYZ, then we fill the 
        /// field (left to right) with XYZ and then continue appending nine (9) spaces.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static unsafe string _FormatAlphaField(string value, int size)
        {
            //Δεσμευουμε απο το stack χωρο για να φτιαξουμε το νεο string
			var buffer = stackalloc char[size + 1];

			//fill buffer with spaces...
			for (int i = 0; i < size; i++)
				buffer[i] = ' ';
			buffer[size] = '\0';

			if (value is null || value.Length == 0)
				return new string(buffer);

			//Προσπερναμε στο value (τυχων) αρχικα spaces (trimming)...
			int idx = 0;
			while (idx < value.Length && value[idx] == ' ') idx++;
			//Αντιγραφουμε απο το value (γραμμα γραμμα) στο buffer
			for (int _bfi = 0; _bfi < size; idx++, _bfi++)
			{
				if (idx >= value.Length)
					break;

				buffer[_bfi] = value[idx];
			}

			return new string(buffer);
		}

    }
}
