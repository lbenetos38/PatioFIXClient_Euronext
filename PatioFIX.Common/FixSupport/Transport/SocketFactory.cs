using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PatioFIX.Common.FixSupport.Transport
{
    /// <summary>
    /// 
    /// </summary>
    internal static class SocketFactory
    {
        static Logger theLogger = new Logger("SocketFactory");



        /// <summary>
        /// Δημιουργει ένα νεο Socket και προσπαθει να συνδεθεί στην ServerIP και Port1
        /// που μας λεει το settings. Εαν δεν ειναι δυνατη η συνδεση επιστρεφει NULL αφου
        /// πρωτα απελευθερωσει οτι resource χρησιμοποιηθηκε
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Socket CreateClientSocket(FixConfiguration settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));


            var remoteEP = new IPEndPoint(settings.ServerIP, settings.Port1);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (socket.NoDelay != settings.TcpNoDelay)
                socket.NoDelay = settings.TcpNoDelay;
            if (settings.ReceiveBufferSize.HasValue)
                socket.ReceiveBufferSize = settings.ReceiveBufferSize.Value;
            if (settings.SendBufferSize.HasValue)
                socket.SendBufferSize = settings.SendBufferSize.Value;

            try
            {
                socket.Connect(remoteEP);
            }
            catch (SocketException)
            {
                socket.Close();
                throw;
            }

            if (settings.ReceiveTimeout.HasValue)
                socket.ReceiveTimeout = settings.ReceiveTimeout.Value;
            if (settings.SendTimeout.HasValue)
                socket.SendTimeout = settings.SendTimeout.Value;

            theLogger.Info($"Connected to {socket.RemoteEndPoint}");
            //logger.Debug("NoDelay={0} KeepAlive={1} SendBuffer={2} SendTimeout={3} ReceiveBuffer={4} ReceiveTimeout={5}",
            //    socket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay),
            //    socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive),
            //    socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer),
            //    socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout),
            //    socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer),
            //    socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout));

            return socket;
        }


        static void SetSocketOptions(Socket _socket)
        {
            theLogger.Verbose(String.Format("Logon - NoDelay={0} KeepAlive={1} Send Buffer={2} Timeout={3} Receive Buffer={4} Timeout={5}",
                    _socket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout)));

            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 65536); // 64KB Buffer
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 65536); // 64KB Buffer

            theLogger.Verbose(String.Format("Logon - NoDelay={0} KeepAlive={1} Send Buffer={2} Timeout={3} Receive Buffer={4} Timeout={5}",
                    _socket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer),
                    _socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout)));
        }


        /// <summary>
        /// Την χρησιμοποιούμε για να δούμε ενα το socket είναι συνδεδεμενο
        ///<para>https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c</para> 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool SocketIsConnected(Socket socket)
        {
            if (socket != null)
            {
                bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Την χρησιμοποιούμε για να ρυθμισουμε τα keepAlive του socket
        ///<para>https://darchuk.net/2019/01/04/c-setting-socket-keep-alive/</para> 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="keepAliveInterval"></param>
        public static void SetSocketKeepAliveValues(Socket socket, int keepAliveTime, int keepAliveInterval)
        {
            /*
			 *			KeepAliveTime: default value is 2hr
			 *			KeepAliveInterval: default value is 1s and Detect 5 times
			 *			
			 *			the native structure
			 *			struct tcp_keepalive {
			 *				ULONG onoff;
			 *				ULONG keepalivetime;
			 *				ULONG keepaliveinterval;
			 *			}
			 */
            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(inOptionValues, size * 2);

            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

    }
}
