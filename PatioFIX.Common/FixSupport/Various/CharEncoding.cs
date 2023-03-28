namespace PatioFIX.Common
{
    public static class CharEncoding
    {

        /*
         * To FixProtocol ειναι ASCII αλλα το ΑΤΗΕΧ μας επιστρεφει και Ελληνικους χαρακτηρες 
         * συμφωνα με το ISO-8859-7. Εδω λοιπον εχουμε το Default Encoding για ολο το συστημα. 
         * Εαν δεν βρεθει το ISO-8859-7  τοτε γυρναμε σε iso-8859-1
         */
        public static System.Text.Encoding DefaultEncoding;

    }
}
