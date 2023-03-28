using PatioFIX.Common.FixSupport;
using System;
using System.Collections.Generic;
using System.IO;

namespace PatioFIX.Common.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal class FixTestFileEnumerator
    {
        readonly Logger theLogger = new Logger("TestFileEnum");
        readonly string _filePath;
        readonly int _emulatePausePeriod;
        readonly FixConfiguration _settings;
        readonly ODLMesssageSource _role;
        FIXMessage m_inbound;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixMessagesFile"></param>
        /// <param name="emulatePausePeriod"></param>
        /// <param name="settings"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public FixTestFileEnumerator(string fixMessagesFile, int emulatePausePeriod, FixConfiguration settings)
        {
            if (string.IsNullOrWhiteSpace(fixMessagesFile))
            {
                throw new ArgumentNullException(nameof(fixMessagesFile));
            }
            if (!File.Exists(fixMessagesFile))
            {
                throw new Exception($"The logFile '{fixMessagesFile}' does not exist");
            }

            _filePath = fixMessagesFile;
            _settings = settings;
            _role = settings.ClientRole;
            _emulatePausePeriod = emulatePausePeriod;

            m_inbound = new FIXMessage(settings.MaxMessageLength, settings.MaxMessageFields, settings.ValidateCheckSum, settings.ValidateBodyLength);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FIXMessage> Enumerator()
        {
            using (StreamReader file = new StreamReader(_filePath))
            {
                string line;
                int counter = 0;
                while ((line = file.ReadLine()) != null)
                {
                    counter++;

                    var tbuffer = CharEncoding.DefaultEncoding.GetBytes(line);
                    m_inbound.Clear();
                    m_inbound.Parse(tbuffer, 0, tbuffer.Length, theLogger);

                    yield return m_inbound;
                }
            }
        }



    }
}
