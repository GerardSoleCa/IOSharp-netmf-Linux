using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace IOSharp.Exmples
{
    class ButtonEventExample
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting test");

            // Specify the GPIO pin we want to use as an interrupt 
            // source, specify the edges the interrupt should trigger on 
            InterruptPort button = new InterruptPort(Cpu.Pin.GPIO_Pin17, true,
                Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);

            // Hook up an event handler (delegate) to the OnInterrupt event 
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

            while (true)
            {
                // The main thread can now essentially sleep 
                // forever. 
                Console.WriteLine("Restart Waiting");
                Thread.Sleep(60 * 1000);
            }

        }

        static void button_OnInterrupt(uint port, uint state, DateTime time)
        {
            // This method is called whenever an interrupt occurs
            Console.WriteLine("The button is pressed");
            Console.WriteLine("Port: {0}", port);
            Console.WriteLine("State: {0}", state);
            Console.WriteLine("Time: {0}", time);
        }
    }
}
