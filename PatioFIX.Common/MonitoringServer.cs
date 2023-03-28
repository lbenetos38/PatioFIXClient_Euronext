using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace PatioFIX.Common
{
    /// <summary>
    /// https://stackoverflow.com/questions/4019466/httplistener-access-denied/4115328
    /// </summary>
    public class MonitoringServer
    {
        readonly HttpListener listener = new HttpListener();
        readonly ManualResetEvent _forceCancelEvent;
        readonly protected Logger theLogger = null;
        readonly string listenURI;
        readonly bool debugLoggingRequests;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerName"></param>
        /// <param name="forceCancelEvent"></param>
        /// <param name="uri"></param>
        /// <param name="logRequests"></param>
        public MonitoringServer(string ownerName, ManualResetEvent forceCancelEvent, string uri, bool logRequests)
        {
            _forceCancelEvent = forceCancelEvent;
            this.listenURI = uri;
            theLogger = new Logger(ownerName);
            this.debugLoggingRequests = logRequests;

            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($".ctor() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId}), listenURI = {listenURI}");
            else
                theLogger.Verbose($".ctor() called by '{Thread.CurrentThread.Name}', listenURI = {listenURI}");
        }


        public void Start()
        {
            try
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Start() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
                else
                    theLogger.Verbose($"Start() called by '{Thread.CurrentThread.Name}'");

                listener.Prefixes.Add(listenURI);
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5)
                {
                    theLogger.Error("Access is denied. MonitoringServer failed to start!");
                }
                else
                {
                    theLogger.Error(ex);
                }
                return;
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
                return;
            }


            ThreadPool.QueueUserWorkItem((object state) =>
            {
                while (_forceCancelEvent.WaitOne(0) == false)
                {
                    try
                    {
                        HttpListenerContext request = listener.GetContext();
                        ThreadPool.QueueUserWorkItem(ProcessRequestInternal, request);
                    }
                    catch (HttpListenerException ex)
                    {
                        if (ex.ErrorCode == 995)
                        {
                            //The I/O operation has been aborted because of either a thread exit or an application request
                            theLogger.Info(ex.Message);
                        }
                        else
                        {
                            theLogger.Warning(ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        theLogger.Warning(ex);
                    }
                }
                theLogger.Info("::_forceCancelEvent.IsSet!");
            });
        }

        public void Stop()
        {
            try
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                    theLogger.Verbose($"Stop() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
                else
                    theLogger.Verbose($"Stop() called by '{Thread.CurrentThread.Name}'");

                listener.Stop();
            }
            catch (ObjectDisposedException ex)
            {
                theLogger.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                theLogger.Error(ex);
            }
        }


        public void ProcessRequestInternal(object listenerContext)
        {
            try
            {
                var context = (HttpListenerContext)listenerContext;

                if (debugLoggingRequests)
                {
                    theLogger.Verbose($"HttpMethod={context.Request.HttpMethod}, URL={context.Request.Url.LocalPath}");
                }

                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                theLogger.Warning(ex);
            }
        }

        public virtual void ProcessRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var s = string.Format("<html><head><title>TextConsole</title></head><body><h2>Hello George!</h2><p>ticks = {0}</p></body></html>", DateTime.Now.Ticks);
            var msg = Encoding.UTF8.GetBytes(s);
            context.Response.ContentLength64 = msg.Length;
            using (Stream str = context.Response.OutputStream)
                str.Write(msg, 0, msg.Length);
        }

    }
}
