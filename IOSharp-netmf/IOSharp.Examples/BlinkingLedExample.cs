using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace IOSharp.Exmples
{
    class BlinkingLedExample
    {
        static void Main(string[] args)
        {
            OutputPort o = new OutputPort(Cpu.Pin.GPIO_Pin17, true);
            for (int i = 0; i < 5; i++)
            {
                o.Write(true);
                Console.WriteLine("Read: " + o.Read());
                Thread.Sleep(500);
                o.Write(false);
                Console.WriteLine("Read: " + o.Read());
                Thread.Sleep(500);
            }
            o.Dispose();

            InputPort input = new InputPort(Cpu.Pin.GPIO_Pin17, true, Port.ResistorMode.Disabled);
            Console.WriteLine("Read: " + input.Read());
            input.Dispose();
            Console.WriteLine("Finishing");
        }
    }
}
