using PatioFIX.Common;
using PatioFIX.WatchDog.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace PatioFIX.WatchDog.Actors
{
    /// <summary>
    /// https://localhost:44390/metrics
    /// https://localhost:44390/v1.0/
    /// https://localhost:44390/v1.0/measurements
    /// https://localhost:44390/v1.0/measurements?prvTimeTicks=637546451650000000
    /// </summary>
    internal class AdminMonitoringServer : MonitoringServer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="forceCancelEvent"></param>
        /// <param name="settings"></param>
        public AdminMonitoringServer(ManualResetEvent forceCancelEvent, string uri, bool logRequests = false) : base("AdminWebServer", forceCancelEvent, uri, logRequests)
        {

        }

        /// <summary>
        /// https://localhost:44390/v1.0/measurements
        /// https://localhost:44390/v1.0/measurements?prvTimeTicks=0
        /// https://localhost:44390/v1.0/measurements?prvTimeTicks=6666646
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            //theLogger.Verbose($"Url={request.Url}");


            if (request.Url.Segments.Length == 2 && request.Url.Segments[1] == "metrics")
            {
                Prometheus(context);
            }
            else if (request.Url.Segments.Length >= 3 && request.Url.Segments[1] == "v1.0/")
            {
                if (request.Url.Segments[2] == "info")
                {
                    Info(context);
                }
                else if (request.Url.Segments[2] == "version")
                {
                    Version(context);
                }
                else if (request.Url.Segments[2] == "measurements")
                {
                    Measurements(context);
                }
                else
                {
                    SayHi(context);
                }
            }
        }

        void SayHi(HttpListenerContext context)
        {
            var s = string.Format("<html><head><title>TextConsole</title></head><body><h2>Hello AdminMonitoringServer!</h2><p>ticks = {0}</p></body></html>", DateTime.Now.Ticks);
            var msg = Encoding.UTF8.GetBytes(s);
            context.Response.ContentLength64 = msg.Length;
            using (Stream str = context.Response.OutputStream)
                str.Write(msg, 0, msg.Length);
        }

        void Version(HttpListenerContext context)
        {
            var s = string.Format("<html><head><title>TextConsole</title></head><body><h2>Version AdminMonitoringServer!</h2><p>ticks = {0}</p></body></html>", DateTime.Now.Ticks);
            var msg = Encoding.UTF8.GetBytes(s);
            context.Response.ContentLength64 = msg.Length;
            using (Stream str = context.Response.OutputStream)
                str.Write(msg, 0, msg.Length);
        }
        void Info(HttpListenerContext context)
        {
            var s = string.Format("<html><head><title>TextConsole</title></head><body><h2>Info AdminMonitoringServer!</h2><p>ticks = {0}</p></body></html>", DateTime.Now.Ticks);
            var msg = Encoding.UTF8.GetBytes(s);
            context.Response.ContentLength64 = msg.Length;
            using (Stream str = context.Response.OutputStream)
                str.Write(msg, 0, msg.Length);
        }


        void Measurements(HttpListenerContext context)
        {

        }
        /// <summary>
        /// https://github.com/prometheus/docs/blob/master/content/docs/instrumenting/exposition_formats.md
        /// </summary>
        /// <param name="context"></param>
        void Prometheus(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var metrics = AdminMetrics.Instance.ReadAccumulator();
            StringBuilder sb = new StringBuilder();


            #region TCPConnection
            sb.Append("# TYPE tcp_in_bytes counter\n");
            sb.AppendFormat("tcp_in_bytes {0}\n", metrics.TCP_ReceivedBytes);
            sb.Append("\n");

            sb.Append("# TYPE tcp_in_msgs counter\n");
            sb.AppendFormat("tcp_in_msgs {0}\n", metrics.TCP_ReceivedMessages);
            sb.Append("\n");

            sb.Append("# TYPE tcp_out_bytes counter\n");
            sb.AppendFormat("tcp_out_bytes {0}\n", metrics.TCP_SendBytes);
            sb.Append("\n");

            sb.Append("# TYPE tcp_out_msgs counter\n");
            sb.AppendFormat("tcp_out_msgs {0}\n", metrics.TCP_SendMessages);
            sb.Append("\n");

            sb.Append("# TYPE tcp_errors counter\n");
            sb.AppendFormat("tcp_errors {0}\n", metrics.TCP_Errors);
            sb.Append("\n");

            sb.Append("# TYPE tcp_warnings counter\n");
            sb.AppendFormat("tcp_warnings {0}\n", metrics.TCP_Warnings);
            sb.Append("\n");

            sb.Append("# TYPE tcp_connected gauge\n");
            sb.AppendFormat("tcp_connected {0}\n", metrics.TCP_IsConnected);
            sb.Append("\n");
            #endregion


            #region FixClient
            sb.Append("# TYPE fcl_instances counter\n");
            sb.AppendFormat("fcl_instances {0}\n", metrics.FIXCLIENT_InstanceCounter);
            sb.Append("\n");

            sb.Append("# TYPE fcl_started gauge\n");
            sb.AppendFormat("fcl_started {0}\n", metrics.FIXCLIENT_Started);
            sb.Append("\n");


            sb.Append("# TYPE fcl_in_seqnum_high counter\n");
            sb.AppendFormat("fcl_in_seqnum_high {0}\n", metrics.FIXCLIENT_InboundSeqNumTooHigh);
            sb.Append("\n");

            sb.Append("# TYPE fcl_throwaway counter\n");
            sb.AppendFormat("fcl_throwaway {0}\n", metrics.FIXCLIENT_ThrowAwayMessages);
            sb.Append("\n");

            sb.Append("# TYPE fcl_ignored counter\n");
            sb.AppendFormat("fcl_ignored {0}\n", metrics.FIXCLIENT_IgnoredMessages);
            sb.Append("\n");

            sb.Append("# TYPE fcl_resendrequests counter\n");
            sb.AppendFormat("fcl_resendrequests {0}\n", metrics.FIXCLIENT_ResendRequests);
            sb.Append("\n");

            sb.Append("# TYPE fcl_rejections counter\n");
            sb.AppendFormat("fcl_rejections {0}\n", metrics.FIXCLIENT_Rejections);
            sb.Append("\n");

            sb.Append("# TYPE fcl_sessionheartbeats counter\n");
            sb.AppendFormat("fcl_sessionheartbeats {0}\n", metrics.FIXCLIENT_SessionTimerHeartbeats);
            sb.Append("\n");

            sb.Append("# TYPE fcl_errors counter\n");
            sb.AppendFormat("fcl_errors {0}\n", metrics.FIXCLIENT_Errors);
            sb.Append("\n");

            sb.Append("# TYPE fcl_warnings counter\n");
            sb.AppendFormat("fcl_warnings {0}\n", metrics.FIXCLIENT_Warnings);
            sb.Append("\n");

            sb.Append("# TYPE fcl_rejects counter\n");
            sb.AppendFormat("fcl_rejects {0}\n", metrics.FIXCLIENT_Rejects);
            sb.Append("\n");

            sb.Append("# TYPE fcl_failedlogins counter\n");
            sb.AppendFormat("fcl_failedlogins {0}\n", metrics.FIXCLIENT_FailedLogins);
            sb.Append("\n");
            #endregion


            #region SessionState
            sb.Append("# TYPE ses_sentlogon gauge\n");
            sb.AppendFormat("ses_sentlogon {0}\n", metrics.SESSIONSTATE_SentLogon);
            sb.Append("\n");
            sb.Append("# TYPE ses_receivedlogon gauge\n");
            sb.AppendFormat("ses_receivedlogon {0}\n", metrics.SESSIONSTATE_ReceivedLogon);
            sb.Append("\n");
            sb.Append("# TYPE ses_sentlogout gauge\n");
            sb.AppendFormat("ses_sentlogout {0}\n", metrics.SESSIONSTATE_SentLogout);
            sb.Append("\n");
            sb.Append("# TYPE ses_receivedlogout gauge\n");
            sb.AppendFormat("ses_receivedlogout {0}\n", metrics.SESSIONSTATE_ReceivedLogout);
            sb.Append("\n");
            sb.Append("# TYPE ses_synchronizing gauge\n");
            sb.AppendFormat("ses_synchronizing {0}\n", metrics.SESSIONSTATE_Synchronizing);
            sb.Append("\n");
            sb.Append("# TYPE ses_serversynchronizing gauge\n");
            sb.AppendFormat("ses_serversynchronizing {0}\n", metrics.SESSIONSTATE_ServerSynchronizing);
            sb.Append("\n");
            sb.Append("# TYPE ses_testrequestpending gauge\n");
            sb.AppendFormat("ses_testrequestpending {0}\n", metrics.SESSIONSTATE_TestRequestPending);
            sb.Append("\n");
            sb.Append("# TYPE ses_forcestopinprocess gauge\n");
            sb.AppendFormat("ses_forcestopinprocess {0}\n", metrics.SESSIONSTATE_ForceStopInProcess);
            sb.Append("\n");

            sb.Append("# TYPE ses_nextinboundseqnum counter\n");
            sb.AppendFormat("ses_nextinboundseqnum {0}\n", metrics.SESSIONSTATE_NextInboundSeqNum);
            sb.Append("\n");
            sb.Append("# TYPE ses_nextoutboundseqnum counter\n");
            sb.AppendFormat("ses_nextoutboundseqnum {0}\n", metrics.SESSIONSTATE_NextOutboundSeqNum);
            sb.Append("\n");
            #endregion



            sb.Append("# TYPE parsing_errors counter\n");
            sb.AppendFormat("parsing_errors {0}\n", metrics.Parsing_Errors);
            sb.Append("\n");

            sb.Append("# TYPE parsing_warnings counter\n");
            sb.AppendFormat("parsing_warnings {0}\n", metrics.Parsing_Warnings);
            sb.Append("\n");

            sb.Append("# TYPE rejection_warnings counter\n");
            sb.AppendFormat("rejection_warnings {0}\n", metrics.Rejection_Warnings);
            sb.Append("\n");




            sb.Append("# TYPE odl_in_total_sum counter\n");
            sb.AppendFormat("odl_in_total_sum {0}\n", metrics.Received);
            sb.Append("\n");

            sb.Append("# TYPE odl_in_total counter\n");
            sb.AppendFormat("odl_in_total{{type=\"UN\"}} {0}\n", metrics.Unknown);
            sb.AppendFormat("odl_in_total{{type=\"TL\"}} {0}\n", metrics.Credit_Limit_Information);
            sb.AppendFormat("odl_in_total{{type=\"TO\"}} {0}\n", metrics.Exchange_Notes);
            sb.AppendFormat("odl_in_total{{type=\"CC\"}} {0}\n", metrics.Market_Status);
            sb.AppendFormat("odl_in_total{{type=\"TF\"}} {0}\n", metrics.New_Trade_Confirmation);
            sb.AppendFormat("odl_in_total{{type=\"TD\"}} {0}\n", metrics.Order_Change_Confirmation);
            sb.AppendFormat("odl_in_total{{type=\"TC\"}} {0}\n", metrics.Order_Edit_Confirmation);
            sb.AppendFormat("odl_in_total{{type=\"TB\"}} {0}\n", metrics.Order_Entry_Confirmation);
            sb.AppendFormat("odl_in_total{{type=\"TR\"}} {0}\n", metrics.Rejection);
            sb.AppendFormat("odl_in_total{{type=\"TE\"}} {0}\n", metrics.OrderCancelRejection);
            sb.AppendFormat("odl_in_total{{type=\"CD\"}} {0}\n", metrics.Security_Price);
            sb.AppendFormat("odl_in_total{{type=\"CA\"}} {0}\n", metrics.Security_Status);
            sb.AppendFormat("odl_in_total{{type=\"CB\"}} {0}\n", metrics.System_Status);
            sb.Append("\n");


            sb.Append("# TYPE odl_out_total_sum counter\n");
            sb.AppendFormat("odl_out_total_sum {0}\n", metrics.Send);
            sb.Append("\n");
            sb.Append("# TYPE odl_out_total counter\n");
            sb.AppendFormat("odl_out_total{{type=\"MB\"}} {0}\n", metrics.Order_Entry);
            sb.AppendFormat("odl_out_total{{type=\"MD\"}} {0}\n", metrics.Order_Change);
            sb.AppendFormat("odl_out_total{{type=\"MC\"}} {0}\n", metrics.Order_Edit);
            sb.Append("\n");


            sb.Append("# TYPE odl_heartBeats counter\n");
            sb.AppendFormat("odl_heartBeats{{type=\"controller\"}} {0}\n", metrics.TheControllerHeartBeats);
            sb.AppendFormat("odl_heartBeats{{type=\"eventslistener\"}} {0}\n", metrics.EventsListenerHeartBeats);
            sb.Append("\n");


            sb.Append("# TYPE odl_errors_total counter\n");
            sb.AppendFormat("odl_errors_total {0}\n", metrics.Application_Errors);
            sb.Append("\n");

            sb.Append("# TYPE odl_errors counter\n");
            sb.AppendFormat("odl_errors{{source=\"EventsListener\"}} {0}\n", metrics.Errors_EventsListener);
            sb.AppendFormat("odl_errors{{source=\"Controller\"}} {0}\n", metrics.Errors_TheController);
            //sb.AppendFormat("odl_errors{{source=\"Dispatcher\"}} {0}\n", metrics.Errors_OrdersDispatcher);
            //sb.AppendFormat("odl_errors{{source=\"GetOutboundMessages\"}} {0}\n", metrics.Errors_GetOutboundMessages);
            sb.Append("\n");


            sb.Append("# TYPE odl_warnings_total counter\n");
            sb.AppendFormat("odl_warnings_total {0}\n", metrics.Application_Warnings);
            sb.Append("\n");


            //sb.Append("# TYPE odl_warnings counter\n");
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SendMessage_GUARD\"}} {0}\n", metrics.Warnings_SendMessage_Guard);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SendMessage_FAILLED\"}} {0}\n", metrics.Warnings_SendMessage_Failled);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SendMessage_RETRY\"}} {0}\n", metrics.Warnings_SendMessage_Retries);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SendMessage_EXPIRED\"}} {0}\n", metrics.Warnings_SendMessage_Expired);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SetOutBoundMsg_Timeout\"}} {0}\n", metrics.Warnings_SetOutBound_Timeout);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"SetOutBoundMsg_Deadlock\"}} {0}\n", metrics.Warnings_SetOutBound_Deadlock);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"Restore_failed\"}} {0}\n", metrics.Warnings_Restore_failed);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"UnSetOutBoundMsg_Timeout\"}} {0}\n", metrics.Warnings_UnSetOutBound_Timeout);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"UnSetOutBoundMsg_Deadlock\"}} {0}\n", metrics.Warnings_UnSetOutBound_Deadlock);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"Dispatcher\"}} {0}\n", metrics.Warnings_OrdersDispatcher);
            //sb.AppendFormat("odl_warnings{{source=\"OrdersDisp\", type=\"EventsListener\"}} {0}\n", metrics.Warnings_EventsListener);
            //sb.Append("\n");


            sb.Append("# TYPE num_of_connections counter\n");
            sb.AppendFormat("num_of_connections {0}\n", metrics.Num_of_Connections);
            sb.Append("\n");



            sb.Append("# TYPE odl_machine gauge\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "odl_machine{{type=\"cpu\"}} {0}\n", metrics.CPU);
            sb.AppendFormat(CultureInfo.InvariantCulture, "odl_machine{{type=\"memory\"}} {0}\n", metrics.Memory);
            sb.Append("\n");


            //summaries....
            sb.Append("# TYPE odl_evaltime_TL summary\n");
            sb.AppendFormat("odl_evaltime_TL_count {0}\n", metrics.Credit_Limit_Information_count);
            sb.AppendFormat("odl_evaltime_TL_sum {0}\n", metrics.Credit_Limit_Information_sum);
            sb.Append("# TYPE odl_evaltime_TO summary\n");
            sb.AppendFormat("odl_evaltime_TO_count {0}\n", metrics.Exchange_Notes_count);
            sb.AppendFormat("odl_evaltime_TO_sum {0}\n", metrics.Exchange_Notes_sum);
            sb.Append("# TYPE odl_evaltime_CC summary\n");
            sb.AppendFormat("odl_evaltime_CC_count {0}\n", metrics.Market_Status_count);
            sb.AppendFormat("odl_evaltime_CC_sum {0}\n", metrics.Market_Status_sum);
            sb.Append("# TYPE odl_evaltime_TF summary\n");
            sb.AppendFormat("odl_evaltime_TF_count {0}\n", metrics.New_Trade_Confirmation_count);
            sb.AppendFormat("odl_evaltime_TF_sum {0}\n", metrics.New_Trade_Confirmation_sum);
            sb.Append("# TYPE odl_evaltime_TD summary\n");
            sb.AppendFormat("odl_evaltime_TD_count {0}\n", metrics.Order_Change_Confirmation_count);
            sb.AppendFormat("odl_evaltime_TD_sum {0}\n", metrics.Order_Change_Confirmation_sum);
            sb.Append("# TYPE odl_evaltime_TC summary\n");
            sb.AppendFormat("odl_evaltime_TC_count {0}\n", metrics.Order_Edit_Confirmation_count);
            sb.AppendFormat("odl_evaltime_TC_sum {0}\n", metrics.Order_Edit_Confirmation_sum);
            sb.Append("# TYPE odl_evaltime_TB summary\n");
            sb.AppendFormat("odl_evaltime_TB_count {0}\n", metrics.Order_Entry_Confirmation_count);
            sb.AppendFormat("odl_evaltime_TB_sum {0}\n", metrics.Order_Entry_Confirmation_sum);
            sb.Append("# TYPE odl_evaltime_TR summary\n");
            sb.AppendFormat("odl_evaltime_TR_count {0}\n", metrics.Rejection_count);
            sb.AppendFormat("odl_evaltime_TR_sum {0}\n", metrics.Rejection_sum);
            sb.Append("# TYPE odl_evaltime_TE summary\n");
            sb.AppendFormat("odl_evaltime_TE_count {0}\n", metrics.OrderCancelReject_count);
            sb.AppendFormat("odl_evaltime_TE_sum {0}\n", metrics.OrderCancelReject_sum);
            sb.Append("# TYPE odl_evaltime_CD summary\n");
            sb.AppendFormat("odl_evaltime_CD_count {0}\n", metrics.Security_Price_count);
            sb.AppendFormat("odl_evaltime_CD_sum {0}\n", metrics.Security_Price_sum);
            sb.Append("# TYPE odl_evaltime_CA summary\n");
            sb.AppendFormat("odl_evaltime_CA_count {0}\n", metrics.Security_Status_count);
            sb.AppendFormat("odl_evaltime_CA_sum {0}\n", metrics.Security_Status_sum);
            sb.Append("# TYPE odl_evaltime_CB summary\n");
            sb.AppendFormat("odl_evaltime_CB_count {0}\n", metrics.System_Status_count);
            sb.AppendFormat("odl_evaltime_CB_sum {0}\n", metrics.System_Status_sum);
            sb.Append("\n");

            var totalSecurities = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.TotalSecurities, 0, 0);
            var totalEnabled = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_Enabled, 0, 0);
            var totalDisabled = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_Disabled, 0, 0);
            var totalActive = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SActive, 0, 0);
            var totalResume = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SResume, 0, 0);
            var totalSuspended = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SSuspended, 0, 0);
            var totalHalted = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SHalted, 0, 0);
            var totalNotActive = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SNotActive, 0, 0);

            var total_SOD = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_SOD, 0, 0);
            var total_P = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_P, 0, 0);
            var total_O = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_O, 0, 0);
            var total_T = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_T, 0, 0);
            var total_A = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_A, 0, 0);
            var total_C = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_C, 0, 0);
            var total_EOD = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_EOD, 0, 0);
            var total_S = Interlocked.CompareExchange(ref SecurityStatusDB.Instance.Total_S, 0, 0);

            sb.Append("# TYPE sec_totals gauge\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"all\"}} {0}\n", totalSecurities);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"on\"}} {0}\n", totalEnabled);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"off\"}} {0}\n", totalDisabled);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"a\"}} {0}\n", totalActive);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"r\"}} {0}\n", totalResume);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"s\"}} {0}\n", totalSuspended);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"h\"}} {0}\n", totalHalted);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_totals{{type=\"n\"}} {0}\n", totalNotActive);

            sb.Append("# TYPE sec_phase gauge\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"SOD\"}} {0}\n", total_SOD);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"P\"}} {0}\n", total_P);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"O\"}} {0}\n", total_O);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"T\"}} {0}\n", total_T);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"A\"}} {0}\n", total_A);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"C\"}} {0}\n", total_C);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"EOD\"}} {0}\n", total_EOD);
            sb.AppendFormat(CultureInfo.InvariantCulture, "sec_phase{{type=\"S\"}} {0}\n", total_S);
            sb.Append("\n");


            var msg = Encoding.UTF8.GetBytes(sb.ToString());
            response.ContentLength64 = msg.Length;
            response.Headers.Add("Content-type", "text/plain");
            using (Stream str = context.Response.OutputStream)
            {
                str.Write(msg, 0, msg.Length);
            }
        }


    }
}
