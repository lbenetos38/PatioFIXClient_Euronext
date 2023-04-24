using PatioFIX.Common.BLL.Messages;
using PatioFIX.Common.DAL;
using System;
using System.Data.SqlClient;

namespace PatioFIX.Common.BLL.Evaluators
{
    class TradeCaptureReportEvaluator : BaseEvaluator
    {
        readonly Logger theLogger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datalayer"></param>
        public TradeCaptureReportEvaluator(IOdlDataLayer datalayer) : base(datalayer)
        {
            theLogger = new Logger("AE_Evaluator");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixInMessage"></param>
        /// <param name="odlMessage"></param>
        /// <returns></returns>
        /// <exception cref="PtBusinessException"></exception>
        public override EvaluationResult Evaluate(FIXInMessage fixInMessage, IODLMessage odlMessage)
        {
            #region input validation
            if (fixInMessage.ODLMessageType != ODLMessageTypeEnum.Trade_Capture_Report) throw new PtBusinessException($"InvalidMessageType. Need Trade_Capture_Report but received {fixInMessage.ODLMessageType}");
            #endregion

            try
            {
                //Κανουμε cast το comObject στον σωστο συγκεκριμένο τύπο
                var trade = (TradeCaptureReportMessage)odlMessage;


                if (fixInMessage.Source == ODLMesssageSource.Administrator)
                {
                    theLogger.Info(trade.ToString());

                    if (trade.MatchStatus != MatchStatusEnum.Matched)
                    {
                        theLogger.Warning($"TradeCaptureReport SKIPPED (MatchStatus != Matched), TradeReportID={trade.TradeReportID}, AppMsgID={fixInMessage.AppMsgID} ");
                        return EvaluationResult.Success;
                    }
                    if (trade.TradeReportType != TradeReportTypeEnum.Submit)
                    {
                        theLogger.Warning($"TradeCaptureReport SKIPPED (TradeReportType != Submit), TradeReportID={trade.TradeReportID}, AppMsgID={fixInMessage.AppMsgID} ");
                        return EvaluationResult.Success;
                    }
                    if (trade.TradeReportTransType != TradeReportTransTypeEnum.Replace)
                    {
                        theLogger.Warning($"TradeCaptureReport SKIPPED (TradeReportTransType != Replace), TradeReportID={trade.TradeReportID}, AppMsgID={fixInMessage.AppMsgID} ");
                        return EvaluationResult.Success;
                    }

                    OdlDal.InsertTradeCaptureReport(fixInMessage, trade);
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
