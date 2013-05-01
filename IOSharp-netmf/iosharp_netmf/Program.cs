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
            for (int i = 0; i < 5; i++)
            {
                o.Write(true);
                Console.WriteLine("Readed: " + o.Read());
                Thread.Sleep(500);
                o.Write(false);
                Console.WriteLine("Readed: " + o.Read());
                Thread.Sleep(500);
            }
            o.Dispose();

            InputPort input = new InputPort(Cpu.Pin.GPIO_Pin17, true, Port.ResistorMode.Disabled);
            Console.WriteLine("Readed: " + input.Read());
            input.Dispose();
            Console.WriteLine("Finishing");
        }
    }
}
