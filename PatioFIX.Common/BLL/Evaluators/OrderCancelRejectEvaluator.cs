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
    class OrderCancelRejectEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public OrderCancelRejectEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("OrderCancelRejectEvaluator");
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
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.OrderCancelReject) throw new PtBusinessException($"InvalidMessageType. Need OrderCancelReject but received {fixInMessage.ODLMessageType}");
            #endregion


            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var reject = (OrderCancelRejectMessage)odlMessage;
                var originalTypeStr = string.Empty;


                PtOrder existingOrder = null;
                PtChange originalChange = null;
                PtCancel originalCancel = null;
                /*
                 * Εχουμε ενα OrderCancelReject το οποιο το λαμβανουμε στην περιπτωση 
                 *  1) rejection ενος OrderCancelRequest(F) (ακυρωσης) ή 
                 *  2) rejection of an order suspension/resumption through OrderCancelReplaceRequest(G) (αλλαγης)
                 */
                if (Int32.TryParse(reject.ClOrdID, out int _clOrdId))
                {

                    if (reject.CxlRejResponseTo == '1')
                    {
                        //'1' 	Order Cancel Request (F)
                        originalCancel = OdlDal.GetCancelById(_clOrdId);
                        if (originalCancel != null)
                        {
                            originalTypeStr = "MC";

                            reject.MemberOrderNumber = originalCancel.CancelMemberOrderNumber;
                            reject.CSDAccountID = originalCancel.CSDAccountID;
                            reject.SecurityExchange = originalCancel.VenueId;
                            reject.MemberID = originalCancel.MemberID;
                            reject.TraderID = originalCancel.TraderID;
                            reject.SecurityID = originalCancel.SecurityID;
                        }
                    }
                    else if (reject.CxlRejResponseTo == '2')
                    {
                        //'2' 	Order Cancel/Replace Request (G)
                        originalChange = OdlDal.GetChangeById(_clOrdId);
                        if (originalChange != null)
                        {
                            originalTypeStr = "MD";

                            reject.MemberOrderNumber = originalChange.MemberOrderNumber;
                            reject.CSDAccountID = originalChange.CSDAccountID;
                            reject.SecurityExchange = originalChange.VenueId;
                            reject.MemberID = originalChange.MemberID;
                            reject.TraderID = originalChange.TraderID;
                            reject.SecurityID = originalChange.SecuritySymbol;
                        }
                    }

                }



                /*
                 * Θελω τωρα να βρω την αρχικη εντολη απο τον πίνακα Orders:
                 */
                if (!string.IsNullOrEmpty(reject.MemberOrderNumber))
                {
                    if (Int32.TryParse(reject.MemberOrderNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out int _orderid))
                    {
                        existingOrder = OdlDal.GetOrderById(_orderid);
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

                        OdlDal.InsertReject(fixInMessage, reject, originalTypeStr, orderNote, generator);
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

                        OdlDal.InsertReject(fixInMessage, reject, originalTypeStr, orderNote, generator);
                    }
                }


                if (fixInMessage.Source == ODLMesssageSource.Broker)
                {
                    if (existingOrder != null)
                    {
                        if (reject.CxlRejResponseTo == '1')
                        {
                            //'1' 	Order Cancel Request (F)
                            OdlDal.UpdateOrderProcessCode(fixInMessage, existingOrder.OrderID, OrderProcessCodeEnum.H_entolh_akyrwshs_apetyxe);
                        }
                        else if (reject.CxlRejResponseTo == '2')
                        {
                            //'2' 	Order Cancel/Replace Request (G)
                            OdlDal.UpdateOrderProcessCode(fixInMessage, existingOrder.OrderID, OrderProcessCodeEnum.H_allagh_apetyxe);
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
