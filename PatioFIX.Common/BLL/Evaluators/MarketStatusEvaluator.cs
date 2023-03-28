using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class MarketStatusEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public MarketStatusEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("MarketStatusEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Market_Status) throw new PtBusinessException($"InvalidMessageType. Need Market_Status but received {fixInMessage.ODLMessageType}");
            #endregion


            //CCENAXFMT
            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var marketStatus = (MarketStatusMessage)odlMessage;

                /*
                 * BoardID: (OASIS): A single character alpha field that identifies the trading board
                 *              Possible Values:
                 *              “M” Main board
                 *              “S” Special conditions board
                 *              “B” Report Only board
                 *              “F” Forced sales board
                 *              
                 * MarketID: A single character alphanumeric field indicating the trading market
                 * 
                 * Status: A single character alphanumeric field indicating the market/board status. 
                 *          Possible values :
                 *          For the main board:
                 *              ”P” Pre-Call
                 *              “J” Calculated projected opening price
                 *              “T” Continuous/Auction event
                 *              “C” Closing price trading
                 *              “R” Run-off
                 *              “E” End of trading
                 *              “H” Halt
                 *              “S” Stop (Used only in Auction Market)
                 *              “N” No Orders accepted until the next Status change (Used only for XNET interface)
                 *          For the other boards:
                 *              “O” Open
                 *              “E” End
                 *              
                 *  Τα πεδια MarketID & BoardID προσδιορίζουν μοναδικά ενα IMarketStatus
                 */

                if (Globals.ClientRole == ODLMesssageSource.Administrator && Globals.Configuration.MonitorSecurityStatus)
                {
                    /*
                     * Μονο απο τον Administrator στελνουμε MarketStatusMessages
                     */
                    MetricsProxy.Instance.OnMarketStatus(marketStatus);
                }
                OdlDal.InsertOrderMarketStatus(fixInMessage, marketStatus);
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
