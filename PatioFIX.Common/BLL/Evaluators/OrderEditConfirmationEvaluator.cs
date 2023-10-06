using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class OrderEditConfirmationEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public OrderEditConfirmationEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("OrderEditConfirmationEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Order_Edit_Confirmation) throw new PtBusinessException($"InvalidMessageType. Need Order_Edit_Confirmation but received {fixInMessage.ODLMessageType}");
            #endregion

            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var confirmation = (OrderEditConfirmationMessage)odlMessage;


                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    //Βγαζουμε απο το UnConfirmedPool το pending message
                    UnConfirmedPool.Instance.Remove(confirmation.ClOrdID, fixInMessage.ODLMessageType);
                }


                /*
				* EditType: A single character alpha field indicating the action to perform on the order
				*      “C” Cancel order
				*      “S” Suspend (deactivate) order
				*      “U” Unsuspend (activate) order
				*/
                char editType = (char)confirmation.EditType;

                if (fixInMessage.Source == ODLMesssageSource.Administrator)
                {
                    if (editType == 'C')
                    {
                        /*
						 * C = Cancel order
						 * 
						 * Εισαγει και το ConfirmCancel και κανει update και το αντιστοιχο Order (εαν υπαρχει)
						 */
                        OdlDal.InsertConfirmCancel(fixInMessage, confirmation);
                    }
                    else
                    {
                        /*
						 * S = Suspend, U = Unsuspend
						 * 
						 * Εισαγει και το ConfirmOrderEdit και κανει update και το αντιστοιχο Order (εαν υπαρχει)
						 */
                        OdlDal.InsertConfirmOrderEdit(fixInMessage, confirmation);
                    }
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
