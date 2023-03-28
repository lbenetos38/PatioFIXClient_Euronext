using PatioFIX.Common.DAL;

namespace PatioFIX.Common.BLL
{
    /// <summary>
    /// Αποτελει την προγονικη class για ολους τους Evaluators του συστήματος μας.
    /// Υπαρχει ενα Evaluator για καθε ξεχωριστο τυπο ODL μηνυματος που υποστηρίζουμε.Ο σκοπος ενος evaluator
    /// ειναι να παρει το ODL μήνυμα και να το αποθηκευσει στο συστημά μας.
    /// </summary>
    abstract class BaseEvaluator
    {
        protected IOdlDataLayer OdlDal { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public BaseEvaluator(IOdlDataLayer datalayer)
        {
            this.OdlDal = datalayer;
        }


        public bool IfHasErrorCanRecover { get; protected set; }
        public bool HasError { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixInMessage">Το αρχικο fixMessage το οποιο μετατραπηκε στο αντιστοιχο ODLMessage</param>
        /// <param name="odlMessage">Το τελικό ODL message το οποιο προοριζεται για αποθηκευση στο συστημα μας</param>
        /// <returns></returns>
        public abstract EvaluationResult Evaluate(FIXInMessage fixInMessage, IODLMessage odlMessage);
    }
}
