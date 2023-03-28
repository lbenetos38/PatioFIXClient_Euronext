using PatioFIX.Common;
using PatioFIX.Common.BLL;
using PatioFIX.Common.FixSupport;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace PatioFix.Broker
{
    internal sealed class OrdersDispatcher : BaseManager
    {
        IFixClient theFixClient = null;
        readonly Logger theLogger = null;
        System.Timers.Timer m_Timer = null;
        int m_tickCounter = 0;
        int m_cannotSendCounter = 0;
        bool m_isactive = false;
        readonly OutboundMessages outboundMessages = null;
        readonly static Object _lockElapsedEventHandler = new object();//Ενα _lockElapsedEventHandler ακομα και ενα καταλαθος εχω πολλα OrdersDispatcherNative instances!!
        DateTime m_lastSendMessageDT = DateTime.MinValue;
        readonly FIXMessageWriter m_outboundMessage;

        /// <summary>
        /// 
        /// </summary>
        public OrdersDispatcher()
        {
            theLogger = new Logger("Dispatcher");
            outboundMessages = new OutboundMessages();
            m_outboundMessage = new FIXMessageWriter(Globals.FixClient.MaxMessageLength);

            this.m_Timer = new System.Timers.Timer();
            this.m_Timer.Interval = Globals.Configuration.Dispatcher.WaitInterval;
            this.m_Timer.Elapsed += OnTimerTick;
            this.m_Timer.AutoReset = true;
            this.m_Timer.Enabled = true;

            theLogger.Verbose($".ctor() called by '{Thread.CurrentThread.Name}'");
        }



        #region Οι παρακάτω μέθοδοι καλούνται απο το νήμα του caller

        public bool IsAlive => m_isactive;

        public DateTime LastSendMessageDT => m_lastSendMessageDT;

        public void Start()
        {
            if (m_isactive == false)
            {
                m_isactive = true;

                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}). Timer ENABLED");
                else
                    theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'. Timer ENABLED");
            }
            else
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}). Timer IS ALREADY ENABLED");
                else
                    theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'. Timer IS ALREADY ENABLED");
            }

            //Επιστρέφουμε στον caller...
            return;
        }
        public void Stop()
        {
            if (m_isactive == true)
            {
                m_isactive = false;
                Thread.SpinWait(200);
                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}). Timer DISABLED");
                else
                    theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'. Timer DISABLED");
            }
            else
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}). Timer IS ALREADY DISABLED");
                else
                    theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'. Timer IS ALREADY DISABLED");
            }

            //Επιστρέφουμε στον caller...
            return;
        }

        public void SetFixClient(IFixClient fixClientInstance)
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"SetFixClient() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
            else
                theLogger.Verbose($"SetFixClient() called by '{Thread.CurrentThread.Name}'.");

            _unsetFixclient();

            if (fixClientInstance != null)
            {
                theFixClient = fixClientInstance;
                theFixClient.NewThrottlingPolicy += OnNewThrottlingPolicy;
            }
        }
        public void UnSetFixClient()
        {
            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"UnSetFixClient() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}).");
            else
                theLogger.Verbose($"UnSetFixClient() called by '{Thread.CurrentThread.Name}'.");

            _unsetFixclient();
        }
        void _unsetFixclient()
        {
            if (theFixClient != null)
            {
                theFixClient.NewThrottlingPolicy -= OnNewThrottlingPolicy;
                theFixClient = null;
            }
        }


        /*
         * Αυτο το καλει το νήμα του TCPConnector._ReceiveLoop
         * 1). Πρεπει να επιστρεψουμε αμεσα, οσο ειμαστε εδω δεν λαμβανουμε νεα incoming FIX Messages
         */
        public void OnNewThrottlingPolicy(int TransPerSecond, int OutstandingMsgs)
        {

        }
        #endregion


        /// <summary>
        /// Επιστρεφει το connection για το οποιο θα τραβήξουμε απο τηνβαση μας pending εντολεσ.
        /// Το connection μπορεί να είναι ETS(0), ORA(1) η και τα δύο (*)
        /// 
        ///	Εξαρταται απο το TargetConnection και απο το που ειμαστε συνδεδεμένοι αυτη
        ///	την στιγμη που καλούμαστε.
        /// </summary>
        short GetTargetConnection()
        {
            if (theFixClient == null)
                return -1;

            var canSendToETS = theFixClient.CanSend("ETS");
            var canSendToORA = theFixClient.CanSend("ORA");


            if (Globals.PatioOMS.TargetConnection == "ETS")
            {
                if (canSendToETS)
                    return 0;

                return -1;
            }
            else if (Globals.PatioOMS.TargetConnection == "ORA")
            {
                if (canSendToORA)
                    return 1;

                return -1;
            }

            if (canSendToETS && canSendToORA)
                return 2;
            else if (canSendToORA == true)
                return 1;
            else if (canSendToETS == true)
                return 0;

            return -1;
        }



        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            if (!m_isactive || theFixClient == null)
            {
                return;
            }


            lock (_lockElapsedEventHandler)//μονο και μονο γιατι ειναι κρισιμο κομματι, μην τυχων και πανε να μπουν ποτε δυο νηματα
            {
                try
                {
                    m_Timer.Enabled = false;
                    m_tickCounter++;
                    if (m_tickCounter % 60 == 0)
                        theLogger.Verbose($"OnTimerTick() called by threadid = {Thread.CurrentThread.ManagedThreadId}, tickCounter = {m_tickCounter}");



                    var _targetConnection = GetTargetConnection();
                    if (_targetConnection == -1)
                    {
                        int _divisor = m_cannotSendCounter < 100 ? 8 : m_cannotSendCounter < 500 ? 64 : m_cannotSendCounter < 1500 ? 128 : m_cannotSendCounter < 5000 ? 256 : 512;
                        if (m_cannotSendCounter++ % _divisor == 0)
                            theLogger.Info($"CANNOT SEND ΑΝΥ MESSAGE yet ({m_cannotSendCounter} times), Globals.TargetConnection={Globals.PatioOMS.TargetConnection}...");
                    }
                    else
                    {
                        #region
                        /*
						 * Καλουμε την GetOutboundMessages(), για να πάρουμε τυχών pending μηνύματα
						 */
                        var totalMessages = this.OdlDal.GetOutboundMessages(outboundMessages, _targetConnection, Globals.Dispatcher.TopRows);
                        if (totalMessages > 0)
                        {
                            var _tc = "ETS|ORA";
                            if (_targetConnection == 0)
                                _tc = "ETS";
                            else if (_targetConnection == 1)
                                _tc = "ORA";

                            theLogger.Info($"GetOutboundMessages('{_tc}') returned = {totalMessages} totalMessage(s), (Globals.TargetConnection = {Globals.PatioOMS.TargetConnection})");
                        }

                        foreach (var item in outboundMessages.Messages)
                        {
                            if (!m_isactive)
                            {
                                theLogger.Info($"SENDING OutboundMessages #1 STOPPED, because Stop() has been called!");
                                return;
                            }

                            #region throttling
                            bool _show = true;
                            while (theFixClient.CanSend(item.TargetConnection) == false || UnConfirmedPool.Instance.CanSend() == false)
                            {
                                //Δεν μπορουμε να στειλουμε αλλο message ακομα...
                                if (_show)
                                {
                                    theLogger.Info($"Throttling, CanSend('{item.TargetConnection}') == false");
                                    _show = false;
                                }

                                if (!m_isactive)
                                {
                                    theLogger.Info($"Throttling OutboundMessages STOPPED, because Stop() has been called!");
                                    return;
                                }

                                /*
								 * Περιμενουμε λιγο, για να ερθει κανα confirmation και ελευθερωθει χωρος
								 * (Τα confirmation τα ακουει ο EventListener)
								 */
                                if (UnConfirmedPool.Instance.NotifyEvent.WaitOne(200) == false)
                                {
                                    /*
									 * Δεν ειδοποιηθηκαμε οτι ελευθερωθηκε χωρος για αποστολη
									 * Ψαχνουμε για ξεχασμενα μηνύματα explicitly
									 */
                                    if (UnConfirmedPool.Instance.MarkAbandonedMessages() > 0)
                                    {
                                        //TODO??
                                        theLogger.Info("UnConfirmedPool.Instance.MarkAbandonedMessages() returned TRUE");
                                    }
                                }
                            }
                            #endregion

                            if (!m_isactive)
                            {
                                theLogger.Info($"SENDING OutboundMessages #2 STOPPED, because Stop() has been called!");
                                return;
                            }


                            /*
                             * Αλλαζουμε το status της εντολης/αλλαγης/ακυρωσης στην βαση
                             */
                            string methodName = string.Empty;
                            if (MarkOutBoundMessageAsSent(item, out methodName))
                            {
                                /*
                                 * ελεγχουμε μηπως εχουμε ξαναστείλει αυτό το item (RowID)
                                 * Λειτουργει σαν ενα ακομα δίχτυ ασφαλείας......
                                 */
                                if (SendMessagesGuard.Instance.Contains(item))
                                {
                                    //ΤΟ ΕΧΟΥΜΕ ΞΑΝΑΣΤΕΙΛΕΙ ΜΑΣ ΛΕΕΙ Η SendMessagesDB!!
                                    MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SENDMESSAGE_GUARD");
                                    theLogger.Warning($"SendMessage GUARD {item.ODLMessageType}, RowID={item.RowID} HAS BEEN SENT ALREADY");
                                    continue;
                                }


                                //Δημιουργούμε το FIXMessage που θα στειλουμε σε λίγο στον FIX Server...
                                try
                                {
                                    item.FormatMessage(m_outboundMessage);
                                }
                                catch (Exception ex)
                                {
                                    MetricsProxy.Instance.OnError("OrdersDispatcher", "FORMAT_MESSAGE");
                                    theLogger.Error($"FormatMessage {item.ODLMessageType}, RowID={item.RowID} EXCEPTION OCCURED");
                                    theLogger.Error(ex);
                                    continue;
                                }


                                /*
                                 * Σε αυτο το σημειο ξεκιναει ο βρογχος της αποστολης του FIXMessage
                                 */
                                var sendRetryCount = 0;
                                bool success = false;
                                while (sendRetryCount < 16)
                                {
                                    /*
                                     * Βαζω αυτο το item στο UnConfirmedPool.Θα αφαιρεθει απο καποιον Evaluator μολις λαβουμε το confirmation 
                                     * (RejectionEvaluator,OrderEntryConfirmationEvaluator, OrderEditConfirmationEvaluator ή OrderChangeConfirmationEvaluator)
                                     * 
                                     * ΝΑ ΜΗΝ ΑΛΛΑΞΕΙ το σημειο στο οποίο καλείται, διοτι τα Remove(s) θα καλουνται πριν τα Add(s)
                                     * <-----------------
                                     * Καλουμε την UnConfirmedPool.Instance.Add() πριν στειλουμε το m_outboundMessage για να αποφευχθει ενα race condition 
                                     * με την κληση του UnConfirmedPool.Instance.Remove() για το ιδιο μηνυμα που θα γινει ασυγχρονα απο καποιον evaluator...
                                     * ----------------->
                                     */
                                    UnConfirmedPool.Instance.Add(item);

                                    /*
                                     * Εδω γινεται η αποστολη του m_outboundMessage στον ATHEX Fix Server:
                                     */
                                    if (item is OrderEntryOutMessage)
                                    {
                                        success = theFixClient.SendMessage("D", m_outboundMessage);//NewOrderSingle
                                    }
                                    else if (item is OrderChangeOutMessage)
                                    {
                                        success = theFixClient.SendMessage("G", m_outboundMessage);//OrderCancelReplaceRequest
                                    }
                                    else if (item is OrderEditOutMessage)
                                    {
                                        success = theFixClient.SendMessage("F", m_outboundMessage);//OrderCancelRequest
                                    }


                                    if (success == false)
                                    {
                                        //H SendMessage ΑΠΟΤΥΧΕ!!

                                        UnConfirmedPool.Instance.RemoveInternal(item);


                                        if (theFixClient.IsConnected == false)
                                        {
                                            //Εχουμε χασει την TCP σύνδεση με τον FIX Server....
                                            theLogger.Error($"SendMessage FAILLED - MUST CONNECT AGAIN (retry={sendRetryCount}) {item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID}");
                                            MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SENDMESSAGE_FAILLED");

                                            /*
                                             * Εδω μπορω να απενεργοποιήσω τον OrdersDispatcher, αλλα κανενας δεν θα τον ενεργοποιήσει.
                                             * Θα επρεπε να ακουω events απο τον FixClient η ο FixClient να με ενεργοποιεί
                                             * Το κραταμε απλο ομοως τον κωδικα μας....
                                             * Η δικια μας GetTargetConnection() θα κανει τον απαραπιτητο ελεγχο εαν επανηλθε η δυνατοτητα αποστολης μηνυματων......
                                             * 
                                             * Stop();
                                             * /

                                            /*
                                             * Επαναφερουμε την εντολη/αλλαγη/ακυρωση στην προηγουμενη κατασταση της.....
                                             */
                                            #region Undo_MarkOutBoundMessageAsSent
                                            if (Undo_MarkOutBoundMessageAsSent(item) == true)
                                            {
                                                theLogger.Warning($"{item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID} RESTORED as 'Entolh_etoimh_gia_apostolh'");
                                            }
                                            else
                                            {
                                                theLogger.Warning($"{item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID} FAILLED TO BE RESTORED as 'Entolh_etoimh_gia_apostolh'");
                                                MetricsProxy.Instance.OnWarning("OrdersDispatcher", "RESTORE_FAILLED");
                                            }
                                            #endregion


                                            /*
                                             * Φευγουμε απο την διαδιακσια αποστολης των υπολοιποων items (εαν υπαρχουν)
                                             * Για να συνεχίσουμε πρεπει ο FIXClient να συνδεθεί ξανά με τον FIX Server.....
                                             */
                                            return;
                                        }



                                        theLogger.Error($"SendMessage FAILLED (retry={sendRetryCount}) {item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID}");

                                        //Ξαναπροσπαθουμε
                                        MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SENDMESSAGE_RETRY");
                                        sendRetryCount++;
                                        Thread.SpinWait(200 + (sendRetryCount * 500));
                                    }
                                    else
                                    {
                                        //ΟΛΑ ΚΑΛΑ
                                        SendMessagesGuard.Instance.Insert(item);

                                        MetricsProxy.Instance.OnSend(item.ODLMessageType);

                                        m_lastSendMessageDT = DateTime.Now;

                                        theLogger.Info($"SendMessage SUCCESS (retry={sendRetryCount}) {item.ODLMessageType}, RowID={item.RowID}, OrderID={item.OrderID}");

                                        break;//σπαμε το loop των Retries
                                    }
                                }

                                if (sendRetryCount >= 16)
                                {
                                    /*
                                     * H SendMessage απετυχε ολες τις φορες (16), και δεν εχουμε χασει την συνδεση με τον FIX Server....
                                     * 
                                     */
                                    MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SENDMESSAGE_EXPIRED");
                                    theLogger.Error($"SendMessage EXPIRED {item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID}");


                                    /*
                                    * Επαναφερουμε την εντολη/αλλαγη/ακυρωση στην προηγουμενη κατασταση της.....
                                    * 
                                    * ΔΕΝ ΤΟ ΚΑΝΟΥΜΕ ΓΙΑ ΝΑ ΜΗΝ ΜΠΛΕΞΟΥΜΕ ΣΕ ΚΑΜΜΙΑ ΠΕΡΙΕΡΓΗ ΛΟΥΠΑ....
                                    * Θα φανει στην πραξη τι πρεπει να κανουμε σε μια τετοια κτασταση.....
                                    */
                                    #region Undo_MarkOutBoundMessageAsSent
                                    //if (Undo_MarkOutBoundMessageAsSent(item) == true)
                                    //{
                                    //    theLogger.Warning($"{item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID} RESTORED as 'Entolh_etoimh_gia_apostolh'");
                                    //}
                                    //else
                                    //{
                                    //    theLogger.Warning($"{item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID} FAILLED TO BE RESTORED as 'Entolh_etoimh_gia_apostolh'");
                                    //    MetricsProxy.Instance.OnWarning("OrdersDispatcher", "RESTORE_FAILLED");
                                    //}
                                    #endregion
                                }
                            }
                            else
                            {
                                //Δεν γραψαμε στην βαση
                                theLogger.Error($"{methodName} EXPIRED {item.ODLMessageType}, RowID={item.RowID} OrderID={item.OrderID}");
                            }
                        }
                        #endregion

                        m_cannotSendCounter = 0;
                    }

                }
                catch (SqlException ex)
                {
                    MetricsProxy.Instance.OnError("OrdersDispatcher");
                    theLogger.Error($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                }
                catch (Exception ex)
                {
                    MetricsProxy.Instance.OnError("OrdersDispatcher");
                    theLogger.Error(ex);
                }
                finally
                {
                    m_Timer.Enabled = true;
                }
            }

        }


        bool MarkOutBoundMessageAsSent(IOutboundMessage item, out string methodName, int maxRetries = 5)
        {
            var retryCount = 0;
            methodName = (item.ODLMessageType == ODLMessageTypeEnum.Order_Change ? "SetChangeAsSent" : item.ODLMessageType == ODLMessageTypeEnum.Order_Edit ? "SetCancelAsSent" : "SetOrderAsSent");

            while (retryCount < maxRetries)
            {
                try
                {
                    if (item.ODLMessageType == ODLMessageTypeEnum.Order_Entry)
                    {
                        OdlDal.SetOrderAsSent(((OrderEntryOutMessage)item).OrderID, (int)OrderProcessCodeEnum.Se_katastash_apostolhs);
                    }
                    else if (item.ODLMessageType == ODLMessageTypeEnum.Order_Change)
                    {
                        OrderChangeOutMessage ocom = (OrderChangeOutMessage)item;
                        OdlDal.SetChangeAsSent(ocom.OrderID, (int)OrderProcessCodeEnum.H_allagh_taksideyei, ocom.ChngID, -5);
                    }
                    else if (item.ODLMessageType == ODLMessageTypeEnum.Order_Edit)
                    {
                        OdlDal.SetCancelAsSent(((OrderEditOutMessage)item).Cancelid, -5);
                    }

                    if (retryCount > 0)
                    {
                        theLogger.Info($"{methodName} SUCCESS {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount}");
                    }
                    else
                    {
                        theLogger.Info($"{methodName} SUCCESS {item.ODLMessageType}, RowID={item.RowID}");
                    }

                    return true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == -2/* Timeout expired*/)
                    {
                        MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SetOutBoundMsg_Timeout");
                        retryCount++;
                        theLogger.Warning($"{methodName} TIMEOUT HANDLED {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount} -> {ex.Message}");
                    }
                    else if (ex.Number == 1205/*deadlock*/)
                    {
                        MetricsProxy.Instance.OnWarning("OrdersDispatcher", "SetOutBoundMsg_Deadlock");
                        retryCount++;
                        theLogger.Warning($"{methodName} DEADLOCK HANDLED {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount} -> {ex.Message}");
                        Thread.SpinWait(retryCount * 200);
                    }
                    else
                    {
                        // Not a deadlock/timeout so throw the exception
                        theLogger.Fatal($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                        theLogger.Fatal($"Process.GetCurrentProcess().Kill();");
                        Thread.Sleep(120);//για να γραφτει το log
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }

            return false;
        }

        bool Undo_MarkOutBoundMessageAsSent(IOutboundMessage item, int maxRetries = 3)
        {
            var retryCount = 0;
            var methodName = (item.ODLMessageType == ODLMessageTypeEnum.Order_Change ? "UnSetOrderAsSent" : item.ODLMessageType == ODLMessageTypeEnum.Order_Edit ? "UnSetCancelAsSent" : "UnSetOrderAsSent");

            while (retryCount < maxRetries)
            {
                try
                {
                    if (item.ODLMessageType == ODLMessageTypeEnum.Order_Entry)
                    {
                        OdlDal.UnSetOrderAsSent(((OrderEntryOutMessage)item).OrderID);
                    }
                    else if (item.ODLMessageType == ODLMessageTypeEnum.Order_Change)
                    {
                        OrderChangeOutMessage ocom = (OrderChangeOutMessage)item;
                        OdlDal.UnSetChangeAsSent(ocom.OrderID, ocom.ChngID);
                    }
                    else if (item.ODLMessageType == ODLMessageTypeEnum.Order_Edit)
                    {
                        OdlDal.UnSetCancelAsSent(((OrderEditOutMessage)item).Cancelid);
                    }

                    if (retryCount > 0)
                    {
                        theLogger.Info($"{methodName} SUCCESS {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount}");
                    }
                    else
                    {
                        theLogger.Info($"{methodName} SUCCESS {item.ODLMessageType}, RowID={item.RowID}");
                    }

                    return true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == -2/* Timeout expired*/)
                    {
                        MetricsProxy.Instance.OnWarning("OrdersDispatcher", "UnSetOutBoundMsg_Timeout");
                        retryCount++;
                        theLogger.Warning($"{methodName} TIMEOUT HANDLED {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount} -> {ex.Message}");
                    }
                    else if (ex.Number == 1205/*deadlock*/)
                    {
                        MetricsProxy.Instance.OnWarning("OrdersDispatcher", "UnSetOutBoundMsg_Deadlock");
                        retryCount++;
                        theLogger.Warning($"{methodName} DEADLOCK HANDLED {item.ODLMessageType}, RowID={item.RowID}, retryCount = {retryCount} -> {ex.Message}");
                        Thread.SpinWait(retryCount * 200);
                    }
                    else
                    {
                        // Not a deadlock/timeout so throw the exception
                        theLogger.Fatal($"(SqlException) Class={ex.Class}, Number={ex.Number} Message={ex.Message}");
                        theLogger.Fatal($"Process.GetCurrentProcess().Kill();");
                        Thread.Sleep(120);//για να γραφτει το log
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }

            return false;
        }

    }
}
