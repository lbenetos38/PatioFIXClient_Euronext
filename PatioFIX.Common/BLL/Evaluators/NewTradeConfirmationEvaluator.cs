using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class NewTradeConfirmationEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public NewTradeConfirmationEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("NewTradeConfirmationEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.New_Trade_Confirmation) throw new PtBusinessException($"InvalidMessageType. Need New_Trade_Confirmation but received {fixInMessage.ODLMessageType}");
            #endregion


            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var trade = (NewTradeConfirmationMessage)odlMessage;


                if (trade.TradeStatus == "L ")
                {
                    /*
                     * TradeStatus: A 2single character alphanumeric field that indicates the status of a Trade
                     *      “ ” Normal Completed Trade
                     *      “L ” Alleged
                     *      “A ” Accepted
                     *      “D ” Declined
                     *      “E ” Expired
                     *      “X ” Cancelled trade
                     *      “C ” Cancelled incomplete trade report
                     *      “U ” Changed trade (XNet)
                     *      
                     *  Αν το TF μήνυμα έχει tradeStatus="L" σημαίνει ότι είναι Alleged και δεν το γράφουμε στον πίνακα με τα Trades.
                     */
                    return EvaluationResult.Success;
                }

                if (fixInMessage.Source == ODLMesssageSource.Administrator)
                {
                    OdlDal.InsertTrades(fixInMessage, trade);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == -2/* Timeout expired*/)
                {
                    //Timeout expired. The timeout period elapsed prior to completion of the operation......
                    theLogger.Error($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                    return EvaluationResult.RetryableSqlException;
                }
                else if (ex.Number == 1205/*deadlock*/)
                {
                    //ransaction (Process ID %d) was deadlocked on %.*ls resources with ....
                    theLogger.Error($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                    return EvaluationResult.RetryableSqlException;
                }
                else
                {
                    theLogger.Error($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                }

                return EvaluationResult.SqlException;
            }
            catch (Exception ex)
            {
                theLogger.Error($"({ex.Message}) {fixInMessage.Message}");
                return EvaluationResult.Exception;
            }

            return EvaluationResult.Success;
        }

    }
}
