using PatioFIX.Common;
using PatioFIX.Common.Configuration;
using PatioFIX.Common.FixSupport;
using System;
namespace ConsoleApp1
{
    internal class FixMessageTest
    {
        Logger theLogger;
        FIXMessage m_inbound = new FIXMessage();


        public FixMessageTest()
        {
            var configuration = new PatioFIXClientConfiguration("PatioFIXAdmin");
            Globals.ServiceName = "PatioFIXAdmin";
            Globals.ClientRole = ODLMesssageSource.Administrator;
            Globals.InitializationDT = DateTime.Now;
            Globals.SetDayOfYear();
            Globals.SetConfiguration(configuration);

            theLogger = new Logger("ConsoleApp2");
        }


        public void Start()
        {
            theLogger.Info("FixMessageTest::Start()");

            var m_inbound = new FIXMessage();

            var f1 = "8=FIX.4.49=22835=f49=ATHEX56=EB_Sender634=1143=Y97=Y52=20220512-07:18:16.721122=20220512-07:07:52.08548=TELL22=8207=XATH454=1455=BBG000BVLMW1456=A60=20220512-07:06:37.9760005574=CA5530=20.5198=115511= 5531=005502=M5522=A10=152";
            var f2 = "8=FIX.4.49=61435=849=ATHEX56=EB_Sender634=4917952=20220512-09:35:15.45637=0001485820220512198=4892511=1994834017=007855FB150=F453=8448=EB447=D452=1448=EB00C447=D452=36448=EB11447=D452=4448=0000181173447=P452=32376=24448=0000000000447=P452=122448=0000000003447=D452=12448=0000000000447=P452=26448=0000447=D452=11=18117332=8731=4.78528=A48=MEDIC22=8207=XATH39=244=4.7877=O63=0151=06=4.7854=1851=260=20220512-09:35:14.511000381=415865501=C5506=M5509=15522=A5529=MB5545=194213.621724=02593=32594=42595=N2594=22595=N2594=32595=N645=0646=4.78134=0135=35604=GR5-710=088";
            var f3 = "8=FIX.4.49=32535=849=ATHEX56=EB_Sender634=4919952=20220512-09:35:18.15037=0001405520220512198=4894541=1994748411=1994748417=48945150=4453=2448=EB447=D452=1448=EB00C447=D452=361=17795348=ANEK22=8207=XATH39=438=154210=0151=15414=06=054=260=20220512-09:35:17.8640005501=C5506=M5545=174328.275508=C5604=GR5-710=188";


            var tbuffer = CharEncoding.DefaultEncoding.GetBytes(f1);
            m_inbound.Clear();
            m_inbound.Parse(tbuffer, 0, tbuffer.Length);
            var msg1 = new FIXMessage(m_inbound);

            tbuffer = CharEncoding.DefaultEncoding.GetBytes(f2);
            m_inbound.Clear();
            m_inbound.Parse(tbuffer, 0, tbuffer.Length);
            var msg2 = new FIXMessage(m_inbound);

            tbuffer = CharEncoding.DefaultEncoding.GetBytes(f3);
            m_inbound.Clear();
            m_inbound.Parse(tbuffer, 0, tbuffer.Length);
            var msg3 = new FIXMessage(m_inbound);



        }
    }
}
