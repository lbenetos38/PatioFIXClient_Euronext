using System;
using System.Threading;

namespace ConsoleApp1
{

    internal class Program
    {
        static void Main(string[] args)
        {
            //Extracter e = new Extracter();
            //e.Start();


            DateTime LastFixStopDT = DateTime.Now;


            Thread.Sleep(4000);

            Console.WriteLine(DateTime.Now.Subtract(LastFixStopDT).TotalMilliseconds);
        }
    }
}
