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
            theLogger = new Logger("TB_Evaluator");
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


                string _οrderNote = confirmOrder.OrderNote;
				if (_οrderNote.Length >= 8)
				{
					/*
                     * Αφαιρουμε τα ACC_Description, το  ~6~/~1~ που βαλαμε στο OrderEntryOutMessage
                     */
					if (_οrderNote[0] == 'X' && _οrderNote[1] == 'X')
					{
						if (_οrderNote[5] == '~' && _οrderNote[7] == '~')
						{
							_οrderNote = _οrderNote.Substring(8);
						}
						else
						{
							_οrderNote = _οrderNote.Substring(5);
						}
					}
				    /*
                     * Αποσυμπιεζουμε τα 'GL\' και 'EXT\'
                     */
				    _οrderNote = _οrderNote.Replace("GL\\", "GALATIA\\");
                    _οrderNote = _οrderNote.Replace("EXT\\", "EXTRANET\\");
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
                    OdlDal.InsertConfirmOrder(fixInMessage, confirmOrder, _οrderNote);
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
