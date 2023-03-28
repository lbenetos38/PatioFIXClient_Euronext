using Microsoft.Extensions.Configuration;
using System;

namespace PatioFIX.Common.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class TCPConnectionSection
    {
        public string SectionName { get; } = "TCPConnection";

        /// <summary>
        /// Improve latency at the expense of throughput (disable/enable Nagle's algorithm).
        /// </summary>
        public bool TcpNoDelay { get; } = true;
        /// <summary>
        /// TCP socket receive buffer size allocated to FIX connection, in bytes.
        /// </summary>
        public int? ReceiveBufferSize { get; } = 65535;

        public int? ReceiveTimeout { get; }

        /// <summary>
        /// TCP socket send buffer size allocated to FIX connection, in bytes.
        /// </summary>
        public int? SendBufferSize { get; } = 65535;
        public int? SendTimeout { get; }


        /// <summary>
        /// Το μεγιστο μεγεθος του receive buffer, του TCPConnector
        /// Εκει βαζει το συστημα τα bytes των δεδομενων που λαμβανει απο το m_socket.Receive()
        /// </summary>
        public int MaxRcvBuffer { get; } = 8192;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="required"></param>
        /// <exception cref="ArgumentException"></exception>
        internal TCPConnectionSection(IConfigurationSection root, bool required = false)
        {
            var section = root.GetSection(this.SectionName);
            if (section.Exists())
            {
                var value = section["TcpNoDelay"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.TcpNoDelay = Convert.ToBoolean(value);
                }

                value = section["ReceiveBufferSize"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.ReceiveBufferSize = Int32.Parse(value);
                }
                else
                {
                    this.ReceiveBufferSize = null;
                }

                value = section["ReceiveTimeout"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.ReceiveTimeout = Int32.Parse(value);
                }
                else
                {
                    this.ReceiveTimeout = null;
                }


                value = section["SendBufferSize"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.SendBufferSize = Int32.Parse(value);
                }
                else
                {
                    this.SendBufferSize = null;
                }

                value = section["SendTimeout"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.SendTimeout = Int32.Parse(value);
                }
                else
                {
                    this.SendTimeout = null;
                }


                value = section["MaxRcvBuffer"];
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    this.MaxRcvBuffer = Int32.Parse(value);
                }
            }
            else
            {
                if (required)
                {
                    throw new ArgumentException($"There is no {section.Path} section but is a required one");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="theLogger"></param>
        internal void DumpSettings(Logger theLogger)
        {
            theLogger.Info($"TCPConnection::TcpNoDelay = {this.TcpNoDelay}");

            if (this.ReceiveBufferSize.HasValue)
                theLogger.Info($"TCPConnection::ReceiveBufferSize = {this.ReceiveBufferSize}");
            else
                theLogger.Info("TCPConnection::ReceiveBufferSize = null");

            if (this.ReceiveTimeout.HasValue)
                theLogger.Info($"TCPConnection::ReceiveTimeout = {this.ReceiveTimeout}");
            else
                theLogger.Info("TCPConnection::ReceiveTimeout = null");

            if (this.SendBufferSize.HasValue)
                theLogger.Info($"TCPConnection::SendBufferSize = {this.SendBufferSize}");
            else
                theLogger.Info($"TCPConnection::SendBufferSize = null");

            if (this.SendTimeout.HasValue)
                theLogger.Info($"TCPConnection::SendTimeout = {this.SendTimeout}");
            else
                theLogger.Info("TCPConnection::SendTimeout = null");

            theLogger.Info($"TCPConnection::MaxRcvBuffer = {this.MaxRcvBuffer}");
        }
    }
}
