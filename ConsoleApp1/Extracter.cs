using PatioFIX.Common;
using PatioFIX.Common.FixSupport;
using System.IO;

namespace ConsoleApp1
{
    internal class Extracter
    {

        /// <summary>
        /// Βγαζει τα FIXMessages απο το log
        /// </summary>
        public void Start()
        {
            var _filePath = @"C:\Users\gmilonakis\Desktop\logs\PatioFIXBroker\Logs\PatioFIXBroker.20230302.log";
            var outputPath = @"C:\Users\gmilonakis\Desktop\logs\FIX_MESSAGES_IN_01.txt";

            StreamWriter ofile = new(outputPath, append: false);
            using (StreamReader file = new StreamReader(_filePath))
            {
                string line = "";
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("in [<]"))
                    {
                        if (line.Contains("\u000135=0\u0001"))
                            continue;
                        if (line.Contains("\u000135=1\u0001"))
                            continue;
                        if (line.Contains("\u000135=2\u0001"))
                            continue;
                        if (line.Contains("\u000135=3\u0001"))
                            continue;
                        if (line.Contains("\u000135=4\u0001"))
                            continue;
                        if (line.Contains("\u000135=5\u0001"))
                            continue;
                        if (line.Contains("\u000135=A\u0001"))
                            continue;


                        //if (line.Contains("\u000135=f\u0001") == true)//security status/ security price
                        //    continue;
                        //if (line.Contains("\u000135=B\u0001") == true)//credit limit / exchange notes
                        //    continue;
                        //if (line.Contains("\u000135=h\u0001") == true)//market statuses
                        //   continue;
                        //if (line.Contains("\u000135=f\u0001") != true && line.Contains("\u000135=B\u0001") != true && line.Contains("\u000135=h\u0001") != true)
                        //    continue;

                        //if (line.Contains("\u000135=8\u0001") == true)//execution reports
                        //    continue;
                        //if (line.Contains("\u0001150=9\u0001") == false)//ExecType = Replace 
                        //   continue;

                        //if (line.Contains("\u000159=") == true)//ExecType = Restated 
                        //    continue;
                        //if (line.Contains("\u000139=4\u0001") == true)//ExecType = Restated 
                        //    continue;



                        var index = line.IndexOf("8=FIX.4.4");
                        if (index > 0)
                        {
                            var subline = line.Substring(index).Trim();
                            //Console.WriteLine(line.Substring(index));


                            ofile.WriteLine(subline);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        void ValidateMessages()
        {
            var outputPath = @"C:\Logs\out.txt";
            StreamWriter ofile = new(outputPath, append: true);
            FIXMessage m_inbound = new FIXMessage();

            using (StreamReader file = new StreamReader(@"C:\Projects\Patio2FixClients\PatioFIX.Admin\TestFiles\FIX_MESSAGES_IN_02.txt"))
            {
                string line;
                int counter = 0;
                while ((line = file.ReadLine()) != null)
                {

                    counter++;

                    var tbuffer = CharEncoding.DefaultEncoding.GetBytes(line.Trim());
                    m_inbound.Clear();
                    m_inbound.Parse(tbuffer, 0, tbuffer.Length, null);

                    if (m_inbound.Valid)
                    {
                        ofile.WriteLine(line.Trim());
                    }
                }
            }
        }
    }
}
