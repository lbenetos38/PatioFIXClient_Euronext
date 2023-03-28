using PatioFIX.Common;
using PatioFIX.Common.FixSupport;
using System;
using System.Diagnostics;
using System.Threading;

namespace PatioFIX.Admin
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class EventsListener : ActiveObject
    {
        #region local variables
        IFixClient theFixClient = null;
        readonly ManualResetEvent _forceCancelEvent;
        readonly MessageQueue<FIXInMessage> _queue;
        readonly Evaluator m_evaluator;
        int _numOfMessages = 0, _numOfIgnoredMesages = 0;
        DateTime m_lastRcvMessageDT = DateTime.MinValue;
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public EventsListener(ManualResetEvent forceCancelEvent) : base("EventsListener", Globals.Configuration.EventsListenerTimer)
        {
            _forceCancelEvent = forceCancelEvent;

            _queue = new MessageQueue<FIXInMessage>(this.QueueEvent, 24);

            m_evaluator = new Evaluator();

            this.EnableNightTimeSlow = true;
            this.EnableWeekEndSlow = true;
        }



        /// <summary>
        /// Ποτε λαβαμε τελευταια φορα μηνυμα
        /// </summary>
        public DateTime LastRcvMessageDT => m_lastRcvMessageDT;


        #region Οι παρακάτω μέθοδοι καλούνται απο το νήμα του caller
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixClient"></param>
        public void SetFixClient(IFixClient fixClientInstance)
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"SetFixClient() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"SetFixClient() called by '{Thread.CurrentThread.Name}'");

            if (theFixClient != null)
            {
                UnSetFixClient();
            }

            if (fixClientInstance != null)
            {
                theFixClient = fixClientInstance;
                theFixClient.NewMessageEvent += OnNewMessage;
                theFixClient.DisconnectEvent += OnFixClientDisconnect;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void UnSetFixClient()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"UnSetFixClient() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"UnSetFixClient() called by '{Thread.CurrentThread.Name}'");


            if (theFixClient != null)
            {
                theFixClient.DisconnectEvent -= OnFixClientDisconnect;
                theFixClient.NewMessageEvent -= OnNewMessage;
                theFixClient = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartOfDayTasks()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"StartOfDayTasks() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"StartOfDayTasks() called by '{Thread.CurrentThread.Name}'");


            _numOfMessages = 0;
            _numOfIgnoredMesages = 0;
        }


        /// <summary>
        /// Επιστρεφει το πληθος των μηνυματων στο Queue
        /// </summary>
        public int PendingNewMessages => _queue.Count;
        #endregion



        protected override void OnStartEvent()
        {
            theLogger.Info("OnStartEvent() entering...");
            try
            {
                var _qcount = _queue.Count;
                if (_qcount > 0)
                {
                    theLogger.Warning($"The queue has {_qcount} messages");
                }

                #region Ξεκιναμε τον timer
                this.StartTimerAuthoritative(true);
                #endregion
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                throw;
            }
            finally
            {
                theLogger.Verbose("OnStartEvent() leaving...");
            }
        }

        protected override bool OnStopEvent()
        {
            _Stop();
            return false;
        }

        protected override bool OnQuitEvent()
        {
            _Stop(true);
            return true;
        }
        void _Stop(bool quit = false)
        {
            try
            {
                _queue.QueueEvent.Reset();

                //Σταματαμε τον timer....
                this.StopTimerAuthoritative();


                var _qcount = _queue.Count;
                if (_qcount > 0)
                {
                    theLogger.Warning($"The queue has {_qcount} messages. All messages will be cleared out");
                }
                _queue.Clear();
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                throw;
            }
            finally
            {
                theLogger.Verbose("_Stop() leaving...");
            }
        }

        protected override void OnExitingThread()
        {
            theLogger.Verbose("ActiveObject's thread is terminationg...");
        }

        /// <summary>
        /// Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop
        /// 1). Πρεπει να επιστρεψουμε αμεσα, οσο ειμαστε εδω δεν λαμβανουμε νεα incoming FIX Messages
        /// 2). To inbound FIXMessage, ειναι αντιγραφο προς δικη μας χρηση
        /// </summary>
        /// <param name="inbound"></param>
        /// <param name="source"></param>
        void OnNewMessage(FIXMessage inbound, ODLMesssageSource source)
        {
            ODLMessageTypeEnum messageType = ODLMessageTypeEnum.Unknown;

            try
            {
                messageType = MessageParser.GetODLMessageType(inbound);
            }
            catch (Exception ex)
            {
                theLogger.Error($"MessageParser.GetODLMessageType() exception: {ex.Message}");
            }


            //PossDupFlag(43)       ->
            //OrigSendingTime(122)  ->
            //PossResend(97)        ->

            if (Evaluator.DoWeIgnoreMessageType(ODLMesssageSource.Administrator, messageType) == true)
            {
                if (++_numOfIgnoredMesages % 50 == 0)
                    theLogger.Info($"OnNewMessage (_numOfIgnoredMesages = {_numOfIgnoredMesages})....");
                MetricsProxy.Instance.OnReceive(messageType);
            }
            else
            {
                var message = new FIXInMessage(inbound, source, messageType);
                _queue.Enqueue(message);
            }

            m_lastRcvMessageDT = DateTime.Now;
        }
        /// <summary>
        /// Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop
        /// </summary>
        void OnFixClientDisconnect()
        {
            theLogger.Verbose("OnFixClientDisconnect() called");
        }




        protected override void OnHeartBeatEvent()
        {
            MetricsProxy.Instance.OnEventsListenerHeartBeat();
            try
            {
                if (totalHeartBeats++ % 20 == 0)
                    theLogger.Verbose($"HeartBeat() called by threadid={Thread.CurrentThread.ManagedThreadId}, totalHeartBeats={totalHeartBeats}");






            }
            catch (Exception ex)
            {
                theLogger.Error(ex.Message);
            }
            finally
            {
                this.StartTimer();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnQueueEvent()
        {
            FIXInMessage p_message = null, d_message = null;


            try
            {
                this.StopTimer();


                while (_queue.TryPeek(out p_message))
                {
                    if (_forceCancelEvent.WaitOne(0))
                    {
                        theLogger.Info("_forceCancelEvent.IsSet!");
                        break;
                    }
                    if (p_message == null)
                    {
                        continue;
                    }

                    if (++_numOfMessages % 40 == 0)
                        theLogger.Info($"OnQueueEvent working (_numOfMessages={_numOfMessages})....");

                    theLogger.Info($"AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum} - {p_message.ODLMessageType}");


                    try
                    {
                        var evaluationResult = m_evaluator.EvaluateMessage(p_message);
                        if (evaluationResult == EvaluationResult.Success || evaluationResult == EvaluationResult.MessageIgnored)
                        {
                            _queue.Dequeue(out d_message);
                        }
                        else if (evaluationResult == EvaluationResult.RetryableSqlException)
                        {
                            /*
							 * Αυτη την περιπτωση, ένα SQLException, μπορούμε να την χειριστούμε διαφορετικα απο τις υπολοιπες.
							 * Μπορούμε να δοκιμάσουμε να ξανακάνουμε evaluation το ETSDataMessage με την ελπιδα ότι οποιο προβλημα
							 * με τον SQL Server θα αποκατασταθεί συντόμως (μπορει να ηταν deadlock, timeout η να επεσε το connection)
							 * Για να κανουμε evaluation ξανα το ETSDataMessage απλα δεν το κανουμε Dequeue (παραμενει στο _queue, και θα 
							 * το ξαναδιαβασουμε με την TryPeek()
							 */
                            if (Globals.RetryRetryableException == false)
                            {
                                /*
                                 * Δεν ξαναπροσπαθουμε
                                 */
                                _queue.Dequeue(out d_message);

                                theLogger.Fatal($"EVALUATION_FAILED_RetryableSqlException (FALSE) for (AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum}) {p_message.ODLMessageType}");
                                MetricsProxy.Instance.OnError("EventsListener");

                                #region Kill process 
                                theLogger.Fatal($"Process.GetCurrentProcess().Kill();");
                                Thread.Sleep(120);//για να γραφτει το log
                                Process.GetCurrentProcess().Kill();
                                #endregion
                            }
                            else
                            {
                                /*
                                 * Ξαναπροσπαθουμε
                                 */
                                theLogger.Warning($"EVALUATION_FAILED_RetryableSqlException (TRUE) for (AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum}) {p_message.ODLMessageType}");
                                MetricsProxy.Instance.OnWarning("EventsListener");
                            }
                        }
                        else if (evaluationResult == EvaluationResult.SqlException)
                        {
                            _queue.Dequeue(out d_message);

                            theLogger.Fatal($"EVALUATION_FAILED_SqlException for (AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum}) {p_message.ODLMessageType}");
                            MetricsProxy.Instance.OnError("EventsListener");

                            #region Kill process 
                            theLogger.Fatal($"Process.GetCurrentProcess().Kill();");
                            Thread.Sleep(120);//για να γραφτει το log
                            Process.GetCurrentProcess().Kill();
                            #endregion
                        }
                        else if (evaluationResult == EvaluationResult.Exception)
                        {
                            _queue.Dequeue(out d_message);

                            theLogger.Error($"EVALUATION_FAILED_Exception for (AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum}) {p_message.ODLMessageType}");
                            MetricsProxy.Instance.OnError("EventsListener");
                        }
                        else if (evaluationResult == EvaluationResult.FailedOther)
                        {
                            _queue.Dequeue(out d_message);

                            theLogger.Error($"EVALUATION_FAILED_FailedOther for (AppMsgID:{p_message.AppMsgID} - MsgSeqNum:{p_message.MsgSeqNum}) {p_message.ODLMessageType}");
                            MetricsProxy.Instance.OnError("EventsListener");
                        }
                    }
                    catch (Exception ex)
                    {
                        _queue.Dequeue(out d_message);

                        theLogger.Error($"(Exception::{ex.Message}) message={p_message}");
                        MetricsProxy.Instance.OnError("EventsListener");
                    }
                }

            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                MetricsProxy.Instance.OnError("EventsListener");
            }
            finally
            {
                this.StartTimer();
            }
        }
    }
}
