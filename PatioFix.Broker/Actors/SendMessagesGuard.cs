using PatioFIX.Common;
using PatioFIX.Common.BLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace PatioFix.Broker
{
    /// <summary>
    /// 
    /// </summary>
    public class SendMessagesGuard
    {
        readonly Logger theLogger;
        List<Decimal> m_Order_Entries;
        List<Decimal> m_Order_Edits;
        List<Decimal> m_Order_Changes;
        StreamWriter m_fileWriter;
        char[] m_separators = new char[] { ',' };

        public static readonly SendMessagesGuard Instance = new SendMessagesGuard();


        /// <summary>
        /// Το directory που θα αποθηκευουμε τα αρχεια μας με τα δεδομενα
        /// </summary>
        static string OurRootDirectory
        {
            get
            {
                return LocalSystem.SendMessagesGuardRootPath;
            }
        }

        /// <summary>
        /// Η ονομασια του σημερινου αρχειου με τα δεδομενα
        /// </summary>
        static string CurrentFilenameName
        {
            get
            {
                return $"dbfile.{DateTime.Now.Year}.{DateTime.Now.Month:00}.{DateTime.Now.Day:00}.dat";
            }
        }
        /// <summary>
        /// Η ονομασια του σημερινου αρχειου με τα δεδομενα
        /// </summary>
        static string CurrentFilenamePath
        {
            get
            {
                return Path.Combine(OurRootDirectory, CurrentFilenameName);
            }
        }

        StreamWriter fileWriter
        {
            get
            {
                if (m_fileWriter == null)
                {
                    m_fileWriter = File.AppendText(CurrentFilenamePath);

                    theLogger.Info($"CurrentFilename = '{CurrentFilenamePath}'");
                }
                return m_fileWriter;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        SendMessagesGuard()
        {
            theLogger = new Logger("MessagesGuard");

            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($".ctor() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($".ctor() called by '{Thread.CurrentThread.Name}'");

            //Reset and ReadDataFromFile:
            Reset();
            ReadDataFromFile();

        }


        public void Insert(IOutboundMessage message)
        {
            if (message == null)
                return;

            if (message.ODLMessageType == ODLMessageTypeEnum.Order_Entry)
            {
                if (Globals.Dispatcher.LogMessageGuard)
                {
                    theLogger.Verbose($"Insert(Order_Entry, RowID={message.RowID})");
                }
                m_Order_Entries.InsertIntoSortedList(message.RowID);
                WriteDataToFile((int)ODLMessageTypeEnum.Order_Entry, message.RowID);
            }
            else if (message.ODLMessageType == ODLMessageTypeEnum.Order_Edit)
            {
                if (Globals.Dispatcher.LogMessageGuard)
                {
                    theLogger.Verbose($"Insert(Order_Edit, RowID={message.RowID})");
                }
                m_Order_Edits.InsertIntoSortedList(message.RowID);
                WriteDataToFile((int)ODLMessageTypeEnum.Order_Edit, message.RowID);
            }
            else if (message.ODLMessageType == ODLMessageTypeEnum.Order_Change)
            {
                if (Globals.Dispatcher.LogMessageGuard)
                {
                    theLogger.Verbose($"Insert(Order_Change, RowID={message.RowID})");
                }
                m_Order_Changes.InsertIntoSortedList(message.RowID);
                WriteDataToFile((int)ODLMessageTypeEnum.Order_Change, message.RowID);
            }
        }

        public bool Contains(IOutboundMessage message)
        {
            if (message == null)
                return false;

            if (message.ODLMessageType == ODLMessageTypeEnum.Order_Entry)
            {
                return m_Order_Entries.BinarySearch(message.RowID) >= 0;
            }
            else if (message.ODLMessageType == ODLMessageTypeEnum.Order_Edit)
            {
                return m_Order_Edits.BinarySearch(message.RowID) >= 0;
            }
            else if (message.ODLMessageType == ODLMessageTypeEnum.Order_Change)
            {
                return m_Order_Changes.BinarySearch(message.RowID) >= 0;
            }

            return false;
        }

        public void Reset()
        {
            m_Order_Entries = new List<decimal>();
            m_Order_Edits = new List<decimal>();
            m_Order_Changes = new List<decimal>();

            if (m_fileWriter != null)
            {
                m_fileWriter.Dispose();
                m_fileWriter = null;
            }

            if (Thread.CurrentThread.IsThreadPoolThread)
                theLogger.Verbose($"Reset() called by ThreadPoolThread (Id = {Thread.CurrentThread.ManagedThreadId})");
            else
                theLogger.Verbose($"Reset() called by '{Thread.CurrentThread.Name}'");
        }

        void WriteDataToFile(int type, decimal rowId)
        {
            try
            {
                fileWriter.WriteLine($"{type},{rowId}");
                fileWriter.Flush();
            }
            catch (Exception ex)
            {
                theLogger.Warning(ex.Message);
            }
        }


        void ReadDataFromFile()
        {
            try
            {
                using (var fileStream = File.OpenRead(CurrentFilenamePath))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        String line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var tokens = line.Split(m_separators, StringSplitOptions.RemoveEmptyEntries);
                            if (tokens.Length != 2)
                                continue;

                            if (int.TryParse(tokens[0], out int type))
                            {
                                if (type == (int)ODLMessageTypeEnum.Order_Entry)
                                {
                                    if (Decimal.TryParse(tokens[1], out decimal result))
                                    {
                                        m_Order_Entries.InsertIntoSortedList(result);
                                    }
                                }
                                else if (type == (int)ODLMessageTypeEnum.Order_Edit)
                                {
                                    if (Decimal.TryParse(tokens[1], out decimal result))
                                    {
                                        m_Order_Edits.InsertIntoSortedList(result);
                                    }
                                }
                                else if (type == (int)ODLMessageTypeEnum.Order_Change)
                                {
                                    if (Decimal.TryParse(tokens[1], out decimal result))
                                    {
                                        m_Order_Changes.InsertIntoSortedList(result);
                                    }
                                }
                            }
                        }
                    }
                }

                theLogger.Info($"I've read from file '{CurrentFilenameName}', {m_Order_Entries.Count} OrderEntries, {m_Order_Edits.Count} OrderCancels and {m_Order_Changes.Count} OrderChanges");
            }
            catch (Exception ex)
            {
                theLogger.Warning(ex.Message);
            }
        }





    }
}
