using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;
using System.Globalization;

namespace PatioFIX.Common.BLL.Evaluators
{
    /// <summary>
    /// 
    /// </summary>
    class RejectionEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public RejectionEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("RejectionEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Rejection) throw new PtBusinessException($"InvalidMessageType. Need Rejection but received {fixInMessage.ODLMessageType}");
            #endregion

            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var reject = (RejectMessage)odlMessage;


                var processCode = OrderProcessCodeEnum.Anepityxhs_apostolh;
                var originalType = ODLMessageTypeEnum.Unknown;
                var originalTypeStr = string.Empty;


                PtOrder existingOrder = null;
                PtChange originalChange = null;
                PtCancel originalCancel = null;
                /*
                 * πρωτο πραγμα που κανουμε ειναι να βρουμε την αρχικη εντολη αγορας/πωλησης που αφορα αυτο το rejection.
                 * Ομως μπορει να μην βρουμε δικια μας εντολη. 
                 * Τοτε το rejection εχει προελθει απο αλλη εφαρμογη εντολοδοσίας...
                 */
                if (string.IsNullOrWhiteSpace(reject.ExchangeOrderID) || reject.ExchangeOrderID == "NONE")
                {
                    // Ειχαμε στείλει μια νέα εντολή, η οποια απορριφτηκε....
                    originalTypeStr = "MB";
                    originalType = ODLMessageTypeEnum.Order_Entry;

                    if (Int32.TryParse(reject.ClOrdID, out int _orderid))
                    {
                        existingOrder = OdlDal.GetOrderById(Int32.Parse(reject.ClOrdID, CultureInfo.InvariantCulture));
                        if (existingOrder != null)
                        {
                            reject.SecurityExchange = existingOrder.VenueId;
                            reject.SecurityID = existingOrder.SecurityID;
                            reject.SecurityIDSource = existingOrder.SecurityIDSource;
                        }
                    }

                    processCode = OrderProcessCodeEnum.Anepityxhs_apostolh;
                }
                else
                {
                    existingOrder = OdlDal.GetOrderByExchangeId(reject.ExchangeOrderID);
                    if (existingOrder != null)
                    {
                        /*
                         * Εχουμε rejection είτε σε καποιο change είτε σε κάποιο cancel ΔΙΚΟ ΜΑΣ
                         */
                        Int32 _id = Int32.Parse(reject.ClOrdID, CultureInfo.InvariantCulture);

                        /*
                         * Ψαχνουμε να το βρουμε στα changes πρωτα....
                         */
                        originalChange = OdlDal.GetChangeById(_id);
                        if (originalChange != null)
                        {
                            //ακυρωση σε δικια μας αλλαγη
                            originalTypeStr = "MD";
                            originalType = ODLMessageTypeEnum.Order_Change;

                            reject.SecurityExchange = originalChange.VenueId;
                            reject.SecurityID = originalChange.SecuritySymbol;
                            reject.SecurityIDSource = originalChange.SecurityIDSource;

                            processCode = OrderProcessCodeEnum.H_allagh_apetyxe;
                        }
                        else
                        {
                            /*
                            * ψαχνουμε να το βρουμε στα cancels μας....
                            */
                            originalCancel = OdlDal.GetCancelById(_id);
                            if (originalCancel != null)
                            {
                                //ακυρωση σε δικο μας Cancelation
                                originalTypeStr = "MC";
                                originalType = ODLMessageTypeEnum.Order_Edit;

                                reject.SecurityExchange = originalCancel.VenueId;
                                reject.SecurityID = originalCancel.SecurityID;

                                processCode = OrderProcessCodeEnum.H_entolh_akyrwshs_apetyxe;
                            }
                        }
                    }
                }


                if (existingOrder != null)
                {
                    if (originalChange != null)
                    {
                        theLogger.Info($"Order FOUND '{existingOrder.OrderID}', Change FOUND '{originalChange.ChngID}'");
                    }
                    else if (originalCancel != null)
                    {
                        theLogger.Info($"Order FOUND '{existingOrder.OrderID}', Cancel FOUND '{originalCancel.CancelID}'");
                    }
                    else
                    {
                        theLogger.Info($"Order FOUND '{existingOrder.OrderID}', NO CHANGE, NO CANCEL");
                    }
                }
                else
                {
                    if (originalChange != null)
                    {
                        theLogger.Info($"Order NOT FOUND, Change FOUND '{originalChange.ChngID}'");
                    }
                    else if (originalCancel != null)
                    {
                        theLogger.Info($"Order NOT FOUND, Cancel FOUND '{originalCancel.CancelID}'");
                    }
                    else
                    {
                        theLogger.Info("Order NOT FOUND, NO CHANGE, NO CANCEL");
                    }
                }



                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    //Βγαζουμε απο το UnConfirmedPool το pending message
                    UnConfirmedPool.Instance.Remove(reject.ClOrdID, fixInMessage.ODLMessageType);
                }


                if (fixInMessage.Source == ODLMesssageSource.Administrator)
                {
                    var orderNote = string.Empty;
                    var generator = PtOrderGenerator.Unknown;

                    if (existingOrder != null)
                    {
                        orderNote = existingOrder.OrderComment;
                        generator = PtOrderGenerator.Patio;
                        if (existingOrder.OrderComment.Contains("\\SALESTRADER"))
                        {
                            generator = PtOrderGenerator.Eurotrader;
                        }
                        else if (existingOrder.OrderComment.Contains("\\EUROBANKTRADER"))
                        {
                            generator = PtOrderGenerator.Eurotrader;
                        }
                        else if (existingOrder.OrderComment.Contains("\\EUROBANK"))
                        {
                            generator = PtOrderGenerator.BankOrders;
                        }
                        else if (existingOrder.OrderComment.StartsWith("EXTRANET\\"))
                        {
                            generator = PtOrderGenerator.Extranet;
                        }

                        OdlDal.InsertReject(fixInMessage, existingOrder.OrderID.ToString(), reject, originalTypeStr, orderNote, generator);
                    }
                    else
                    {
                        /*
                         * με βαση καποιους εμπειρικους κανονες (οι οποιο μπορει να αλλαξουν) βρισκουμε την προελευση του αρχικου μηνυματος
                         */
                        var today = DateTime.Now.ToString("yyyyMMdd");
                        var orderId = reject.ClOrdID;
                        if (orderId.Length == 16 && (orderId[0] == today[0] && orderId[1] == today[1] && orderId[2] == today[2] && orderId[3] == today[3] && orderId[4] == today[4] && orderId[5] == today[5] && orderId[6] == today[6] && orderId[7] == today[7]))
                        {
                            generator = PtOrderGenerator.Catalys;
                        }
                        else if (orderId.Length == 16 && orderId.StartsWith("00000000"))
                        {
                            generator = PtOrderGenerator.Skouras;
                        }
                        else if (orderId.Contains("/") || orderId.Contains("(") || orderId.Contains(")") || orderId.Contains("_"))
                        {
                            generator = PtOrderGenerator.Horizon;
                        }
                        OdlDal.InsertReject(fixInMessage, reject.ClOrdID, reject, originalTypeStr, orderNote, generator);
                    }
                }



                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    if (existingOrder != null)
                    {
                        if (originalType == ODLMessageTypeEnum.Order_Entry)
                        {
                            /*εδω ερχομαστε μονο ενα το Rejection αφορούσε δημιουργία νεας εντολης*/
                            OdlDal.UpdateOrderProcessAndStatusCode(fixInMessage, existingOrder.OrderID, processCode, /*reject.OrdStatus*/'8', reject.RejectReasonCode);
                        }
                        else
                        {
                            OdlDal.UpdateOrderProcessCode(fixInMessage, existingOrder.OrderID, processCode);
                        }
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
