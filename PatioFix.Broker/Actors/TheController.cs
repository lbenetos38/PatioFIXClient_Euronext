using PatioFIX.Common;
using PatioFIX.Common.FixSupport;
using System;
using System.Reflection;
using System.Threading;

namespace PatioFix.Broker
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class TheController : ActiveObject
    {
        IFixClient theFixClient = null;
        EventsListener theListener = null;
        OrdersDispatcher theOrdersDispather;
        readonly ManualResetEvent theForceCancelEvent = new ManualResetEvent(false);
        bool m_isStarted = false;
        readonly object _ourLock = new object();



        /// <summary>
        /// 
        /// </summary>
        public static readonly TheController Instance = new TheController();


        void CreateAnFixClient()
        {
            if (Globals.Emulation.Enable)
            {
                theFixClient = new FixTestClient(Globals.Configuration.GetFixConfiguration(), LocalSystem.FixSessionRootPath);
            }
            else
            {
                theFixClient = new FixClient(Globals.Configuration.GetFixConfiguration(), LocalSystem.FixSessionRootPath);
            }
        }

        #region Οι παρακάτω μέθοδοι καλούνται απο το νήμα του caller
        /// <summary>
        /// 
        /// </summary>
        TheController() : base("TheController", Globals.Configuration.ControllerTimer)
        {
            if (LocalSystem.IsInitialized == false)
                throw new Exception("You must call the LocalSystem.Initialize() static method first!");

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            theLogger.Info($"NEW RUN, AssemblyVersion = {assemblyVersion}");

            this.EnableNightTimeSlow = true;
            this.EnableWeekEndSlow = true;
        }
        #endregion


        #region Οι παρακάτω μέθοδοι εκτελούνται απο το νήμα του TheController
        protected override void OnStartEvent()
        {
            if (m_isStarted == false)
            {
                lock (_ourLock)
                {
                    if (m_isStarted == false)
                    {
                        theLogger.Info("OnStartEvent() entering...");
                        try
                        {
                            theForceCancelEvent.Reset();

                            /*
                             * Καλουμε μια dummy method στην MetricsProxy για να ξεκινησει
                             * την συνδεση της με τον AggregatorServer...
                             */
                            MetricsProxy.Instance.Warmup();
                            Thread.Sleep(120);

                            //Δημιουργούμε ένα FixClient:
                            CreateAnFixClient();


                            //Δημιουργούμε και ξεκινάμε ενα EventsListener:
                            theListener = new EventsListener(theForceCancelEvent);
                            theListener.SetFixClient(theFixClient);
                            theListener.Start();



                            //Δημιουργούμε ένα IOrdersDispatcher
                            theOrdersDispather = new OrdersDispatcher();

                            //Ξεκιναμε τον timer
                            this.StartTimerAuthoritative(true);


                            m_isStarted = true;
                        }
                        catch (Exception ex)
                        {
                            theLogger.Error(ex);
                        }
                    }
                }
            }
            theLogger.Verbose("OnStartEvent() leaving...");
        }
        protected override bool OnStopEvent()
        {
            if (m_isStarted == true)
            {
                lock (_ourLock)
                {
                    if (m_isStarted == true)
                    {
                        theLogger.Info("OnStopEvent() entering...");

                        _Stop();
                    }
                }
            }
            theLogger.Verbose("OnStopEvent() leaving...");
            return false;
        }
        protected override bool OnQuitEvent()
        {
            if (m_isStarted == true)
            {
                lock (_ourLock)
                {
                    if (m_isStarted == true)
                    {
                        theLogger.Info("OnQuitEvent() entering...");

                        _Stop();
                    }
                }
            }
            theLogger.Verbose("OnQuitEvent() leaving...");
            return true;
        }
        void _Stop()
        {
            try
            {
                //Σταματάμε τον OrdersDispatcher:
                theOrdersDispather?.Stop();

                //Σταματαμε τον timer....
                this.StopTimerAuthoritative();

                //απεμπλοκή του EventsListener απο τον FixClient
                theListener?.UnSetFixClient();

                //Σταματαμε τον FixClient
                theFixClient?.Stop();


                //Ειδοποιύμε τον theListener να σταματήσει
                theListener?.Quit();


                //Σε αυτο το σημείο ενεργοποιούμε το theForceCancelEvent, για οποιο ActiveObject
                //εχει τυχών κολλήσει μέσα σε κάποιο loop.
                //Το κατεχουν τα κατωθι AO: theListener
                theForceCancelEvent.Set();

                /*
                 * 
                 */
                theFixClient?.Dispose();
                theFixClient = null;

                /*
                 * Δινουμε ελαχιστο χρονο ια να τερματίσει ο EventsListener
                 */
                Thread.Sleep(180);
                theListener = null;



                m_isStarted = false;
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
            }
        }
        #endregion



        bool? _IsScheduleOn = null;

        protected override void OnHeartBeatEvent()
        {
            MetricsProxy.Instance.OnControllerHeartBeat();



            try
            {
                #region Guard (just in case....)
                if (m_isStarted == false)
                {
                    /*Αυτο κανονικα δεν θα συμβει ποτε. Οι μεδθοδοι Start(), Stop() και Quit() φροντιζουν να ανοιγοκλεινουν τον timer μας*/
                    MetricsProxy.Instance.OnError("TheController");
                    theLogger.Error("OnHeartBeatEvent() called, but m_isStarted == false");
                    return;
                }
                #endregion

                #region μήπως πρεπει να τρεξουμε την LocalSystem.DBHouseKeeping?
                if (LocalSystem.Do_DBHouseKeeping)
                {
                    LocalSystem.DBHouseKeeping(theLogger);
                    if (LocalSystem.Do_DBHouseKeeping)
                        return;
                }
                #endregion
                #region Do_StartOfDayTasks?
                if (LocalSystem.Do_StartOfDayTasks)
                {
                    theListener.StartOfDayTasks();
                    LocalSystem.Do_StartOfDayTasks = false;
                }
                #endregion

                if (totalHeartBeats++ % 20 == 0)
                {
                    LocalSystem.PeriodicTasks(theLogger);
                    theLogger.Verbose($"HeartBeat() called by threadid = {Thread.CurrentThread.ManagedThreadId}, totalHeartBeats={totalHeartBeats}");
                }



                if (Globals.Schedule.IsScheduleOnOrDisabled())
                {
                    if (Globals.Schedule.IsDisabled == false && (_IsScheduleOn == null || _IsScheduleOn == false))
                    {
                        theLogger.Info($"IsScheduleOn == true");
                        _IsScheduleOn = true;
                    }

                    #region Normal flow
                    if (theFixClient == null)
                    {
                        /*
                         * Δημιουργούμε ένα νέο FixClient και τον δίνουμε και στον EventsListener...
                         */
                        CreateAnFixClient();
                        theListener.SetFixClient(theFixClient);
                    }

                    /*
                     * Ξεκιναμε τον Client εαν δεν εχει ήδη ξεκινήσει....
                     */
                    if (theFixClient.IsStarted == false && DateTime.Now.Subtract(theFixClient.LastFixStopDT).TotalMilliseconds >= Globals.Configuration.FIXReconnectInterval)
                    {
                        //!!εαν τυχων ο theOrdersDispather ειναι ενεργος τον σταματαμε!!
                        if (theOrdersDispather != null && theOrdersDispather.IsAlive)
                        {
                            theOrdersDispather?.Stop();
                        }

                        /*
                         * Μπορει να ειμαστε σε φαση επανασύνδεσης (δηλαδη ειμασταν συνδεδεμενοι, η συνδεση χαθηκε,
                         * και τωρα επιχειρουμε να συνδεθουμε ξανα). 
                         * Υπαρχει περιπτωση ο EventListener να μην εχει προλαβει να επεξεργαστει ολα τα μηνυματα που 
                         * συλεχτηκαν στην διαρκεια της προηγουμενης συνδεσης. Δηλαδη να εχει ακομα messages στο QUEUE του.
                         * Σε μια τετοια περιπτωση, εαν επιχειρησουμε να συνδεθουμε καπάκι, η ReadStatusFromDB(),
                         * θα μας δωσει σαν LastMsgSeqNum μονο οτι εχουμε αποθηκευσει στην βαση (και οχι το τελευταιο MsgSeqNum
                         * που υπαρχει στο QUEUE των messages στο EventListener).
                         * Αυτο θα εχει σαν αποτελεσμα, μετα το login στο FIX Server να παρουμε στην φαση του συγχρονισμου, messages
                         * που τα εχουμε ηδη παραλαβει. 
                         * Δηλαδη διπλα μηνυματα. 
                         * Αν και εχουμε το 'Application Unique Number/MsgID' (tag198) σε καθε μηνυμα, προτιμουμε να μην
                         * παρουμε διπλα μηνυματα (αφου μπορουμε να το αποφυγουμε περιμενοντας να αδειασει το queue, 
                         * πριν συνδεθουμε)
                         */
                        var pendingMessages = theListener.PendingNewMessages;
                        if (pendingMessages > 0)
                        {
                            theLogger.Info($"There are {pendingMessages} pending messages in our EventsListener's queue. We will not start yet FixClient....");
                            return;
                        }

                        /*
                         * Διαβαζουμε το τρεχων status μας απο την βαση μας (ODL)
                         */
                        var status = LocalSystem.ReadStatusFromDB(theLogger);

                        /*
                         * Ξεκιναμε τον FixClient...
                         */
                        theFixClient.Start(status);

                        //
                        theOrdersDispather?.SetFixClient(theFixClient);
                        theOrdersDispather?.Start();


                        return;
                    }

                    #endregion
                }
                else
                {
                    #region O Scheduler ειναι ενεργος και ειμαστε σε χρονικη περιοδο idle
                    if (_IsScheduleOn == null || _IsScheduleOn == true)
                    {
                        theLogger.Info($"IsScheduleOn == false");
                        _IsScheduleOn = false;
                    }


                    if (theFixClient != null)
                    {
                        theLogger.Info($"We are in IDLE PERIOD, and the theFixClient was not null...");
                        if (theFixClient.IsStarted)
                        {
                            theFixClient.Stop();
                            return;
                        }


                        #region
                        theListener.UnSetFixClient();

                        theOrdersDispather?.Stop();
                        theOrdersDispather?.UnSetFixClient();

                        theFixClient.Dispose();
                        theFixClient = null;
                        #endregion
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                MetricsProxy.Instance.OnError("TheController");
                theLogger.Error(ex.Message);
            }
            finally
            {
                this.StartTimer();
            }
        }

    }
}
