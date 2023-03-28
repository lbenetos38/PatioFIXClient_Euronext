using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class OrderEntryConfirmationEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public OrderEntryConfirmationEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("OrderEntryConfirmationEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Order_Entry_Confirmation) throw new PtBusinessException($"InvalidMessageType. Need Order_Entry_Confirmation but received {fixInMessage.ODLMessageType}");
            #endregion

            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var confirmOrder = (OrderEntryConfirmationMessage)odlMessage;


                string strOrderNotes = confirmOrder.OrderNote;
                if (string.IsNullOrWhiteSpace(strOrderNotes) == false && strOrderNotes.Length > 14)
                {
                    if (strOrderNotes[0] == 'X' && strOrderNotes[1] == 'X')    // if Acc_Descriptn has passed ('XX1  ~6~GL\SALESTRADER   ')
                    {
                        strOrderNotes = strOrderNotes.Substring(5);
                        if (strOrderNotes[0] == '~' && strOrderNotes[2] == '~')
                        {
                            strOrderNotes = strOrderNotes.Substring(3);
                        }
                        strOrderNotes = strOrderNotes.Replace("GL\\", "GALATIA\\");
                        strOrderNotes = strOrderNotes.Replace("EXT\\", "EXTRANET\\");
                    }
                }


                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    //Βγαζουμε απο το UnConfirmedPool το pending message
                    UnConfirmedPool.Instance.Remove(confirmOrder.ClOrdID, fixInMessage.ODLMessageType);
                }


                if (fixInMessage.Source == ODLMesssageSource.Administrator)
                {
                    /*
                     * Εισαγει και το ConfirmOrder και κανει update και το αντιστοιχο Order (εαν υπαρχει)
                     */
                    OdlDal.InsertConfirmOrder(fixInMessage, confirmOrder, strOrderNotes);
                }


                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    //Nothing todo....
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
