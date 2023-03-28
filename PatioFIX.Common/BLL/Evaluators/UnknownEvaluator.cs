using PatioFIX.Common.DAL;
using System;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class UnknownEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public UnknownEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("UnknownEvaluator");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixInMessage">Το αρχικο fixMessage το οποιο μετατραπηκε στο αντιστοιχο ODLMessage</param>
        /// <param name="odlMessage">Το τελικό ODL message το οποιο προοριζεται για αποθηκευση στο συστημα μας</param>
        /// <returns></returns>
        /// <exception cref="PtBusinessException"></exception>
        public override EvaluationResult Evaluate(FIXInMessage fixInMessage, IODLMessage odlMessage)
        {
            #region input validation
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Unknown) throw new PtBusinessException($"InvalidMessageType. Need Unknown but received {fixInMessage.ODLMessageType}");
            #endregion

            throw new NotImplementedException();
        }

    }
}
