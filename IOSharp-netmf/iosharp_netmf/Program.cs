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
            o.Write(true);
            Console.ReadLine();
        }
    }
}
