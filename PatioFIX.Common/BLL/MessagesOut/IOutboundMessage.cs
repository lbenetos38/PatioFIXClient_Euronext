using PatioFIX.Common.FixSupport;
using System;

namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOutboundMessage : IODLMessage
    {
        /// <summary>
        /// Εδω εχουμε με ενα κοινο ονομα το id της εντιστοιχης εγγραφης (Order, Change, Cancel)
        /// </summary>
        Int32 RowID { get; }
        /// <summary>
        /// Ολα τα OutboundMessages(OrderEntryMessage, OrderChangeMessage και OrderEditMessage) αφορουν
        /// καποιο order μέσα στο συστημά μας. Εδω έχουμε αυτο το OrderId.
        /// <para>Υπαρχει μια σπανια περιπτωση ένα 3ο application να εχει δημιουργήσει εγγραφες στους πίνακες 
        /// Cancels και Changes, και αρα να εχουμε εγγραφες που δεν αντιστοιχουν σε κάποιο Order μεσα 
        /// απο τον πίνακα Orders. Σε αυτη την περιπτωση το OrderID εχει την τιμη -1</para>
        /// </summary>
        Int32 OrderID { get; }

        /// <summary>
        /// Ημερα και ωρα δημιουργίας αυτης της εγγραφης στο συστημά μας
        /// </summary>
        DateTime WorkingDate { get; }
        /// <summary>
        /// 
        /// </summary>
        String TargetConnection { get; }


        void FormatMessage(FIXMessageWriter writer);


        #region UnConfirmedPool support
        long _UnConfirmedPool_ticks { get; set; }
        bool _UnConfirmedPool_abandoned { get; set; }
        #endregion
    }
}
