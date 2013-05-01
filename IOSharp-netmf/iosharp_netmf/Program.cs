using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Linux.SPOT.Hardware
{
    class Program
    {
        static void Main(string[] args)
        {
            OutputPort o = new OutputPort(Cpu.Pin.GPIO_Pin17, true);
            for (int i = 0; i < 5;i++ )
            {
                o.Write(true);
                Thread.Sleep(500);
                o.Write(false);
                Thread.Sleep(500);
            }
            Console.WriteLine("Finishing");
        }
    }
}
