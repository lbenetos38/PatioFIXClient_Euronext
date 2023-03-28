namespace PatioFIX.Common
{
    /// <summary>
    /// Οι τιμές προέρχονται απο τον πίνακα OrderProcessCodes
    /// </summary>
    public enum OrderProcessCodeEnum : int
    {
        /// <summary>
        /// -2	Αναμένει επιβεβαίωση
        /// </summary>
        Anamenei_epibebaiwsh = -2,
        /// <summary>
        /// - 1	Εντολή έτοιμη για αποστολή
        /// </summary>
        Entolh_etoimh_gia_apostolh = -1,
        /// <summary>
        /// 1	Σε κατάσταση αποστολής
        /// </summary>
        Se_katastash_apostolhs = 1,
        /// <summary>
        /// 2	Επιτυχής αποστολή
        /// </summary>
        Epityxhs_apostolh = 2,
        /// <summary>
        /// 3	Ανεπιτυχής αποστολή
        /// </summary>
        Anepityxhs_apostolh = 3,
        /// <summary>
        /// 4	Οριστικά ανεπιτυχής αποστολή
        /// </summary>
        Oristika_anepityxhs_apostolh = 4,
        /// <summary>
        /// 5	Έχει δοθεί εντολή ακύρωσης
        /// </summary>
        Exei_dothei_entolh_akyrwshs = 5,
        /// <summary>
        /// 6	Ακύρωση πριν την αποστολή
        /// </summary>
        Akyrwsh_prin_thn_apostolh = 6,
        /// <summary>
        /// 7	Έχει δοθεί εντολή αλλαγής
        /// </summary>
        Exei_dothei_entolh_allaghs = 7,
        /// <summary>
        /// 8	Η αλλαγή ταξιδεύει
        /// </summary>
        H_allagh_taksideyei = 8,
        /// <summary>
        /// 9	Η αλλαγή απέτυχε
        /// </summary>
        H_allagh_apetyxe = 9,
        /// <summary>
        /// 10	Επιτυχημένη αλλαγή
        /// </summary>
        Epityxhmenh_allagh = 10,
        /// <summary>
        /// 11	Η εντολή ακύρωσης πέτυχε
        /// </summary>
        H_entolh_akyrwshs_petyxe = 11,
        /// <summary>
        /// 12	Η εντολή ακύρωσης απέτυχε
        /// </summary>
        H_entolh_akyrwshs_apetyxe = 12,
        /// <summary>
        /// 14	Εντολή Ανενεργή
        /// </summary>
        Entolh_Anenergh = 14,
        /// <summary>
        /// 15	Απέτυχε η ανενεργοποίηση
        /// </summary>
        Apetyxe_h_anenergopoihsh = 15,
        /// <summary>
        /// 16	Εντολή ενεργοποιήθηκε
        /// </summary>
        Entolh_energopoihthhke = 16,
        /// <summary>
        /// 17	Απέτυχε η ενεργοποίηση
        /// </summary>
        Apetyxe_h_energopoihsh = 17


    }
}
