using PatioFIX.WatchDog.Actors;
using System;
using System.Reflection;
using System.Threading;

namespace PatioFIX.WatchDog
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class TheController : ActiveObject
    {
        readonly ManualResetEvent theForceCancelEvent = new ManualResetEvent(false);
        AdminMonitoringServer theAdminWebServer = null;
        BrokerMonitoringServer theBrokerWebServer = null;
        AggregatorServer theServer = null;
        bool m_isStarted = false;
        readonly object _ourLock = new object();





        /// <summary>
        /// 
        /// </summary>
        public static readonly TheController Instance = new TheController();



        #region Οι παρακάτω μέθοδοι καλούνται απο το νήμα του caller
        /// <summary>
        /// 
        /// </summary>
        TheController() : base("TheController", Globals.ControllerTimer)
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


                            theServer = new AggregatorServer(theForceCancelEvent, Globals.ListenPort);
                            theServer.Start();

                            theAdminWebServer = new AdminMonitoringServer(theForceCancelEvent, Globals.AdminMonitorServer.URI, Globals.AdminMonitorServer.LogRequests);
                            theAdminWebServer.Start();

                            theBrokerWebServer = new BrokerMonitoringServer(theForceCancelEvent, Globals.BrokerMonitorServer.URI, Globals.BrokerMonitorServer.LogRequests);
                            theBrokerWebServer.Start();


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
                //Σταματαμε τον timer....
                this.StopTimerAuthoritative();


                //Σε αυτο το σημείο ενεργοποιούμε το theForceCancelEvent, για οποιο ActiveObject
                //εχει τυχών κολλήσει μέσα σε κάποιο loop.
                //Το κατεχουν τα κατωθι AO: theServer,theMonitoringServer
                theForceCancelEvent.Set();

                theServer?.Stop();

                theAdminWebServer?.Stop();
                theBrokerWebServer?.Stop();

                m_isStarted = false;
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
            }
        }
        #endregion




        protected override void OnHeartBeatEvent()
        {
            try
            {
                #region μήπως ηρθε η στιγμη για το καθε 24ωρο refresh μας?
                if (LocalSystem.Do_Every24hoursStuff)
                {
                    LocalSystem.Do_Every24hoursStuff = false;
                }
                #endregion

                if (totalHeartBeats++ % 20 == 0)
                {
                    LocalSystem.PeriodicTasks(theLogger);
                    theLogger.Verbose($"HeartBeat() called by threadid = {Thread.CurrentThread.ManagedThreadId}, totalHeartBeats={totalHeartBeats}");
                }





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

    }
}
