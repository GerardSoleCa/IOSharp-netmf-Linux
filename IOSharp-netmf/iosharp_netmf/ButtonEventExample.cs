using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Linux.SPOT.Hardware
{
    class ButtonEventExample
    {
        static void Main(string[] args)
        {
            // Specify the GPIO pin we want to use as an interrupt 
            // source, specify the edges the interrupt should trigger on 
            InterruptPort button = new InterruptPort(Cpu.Pin.GPIO_Pin17, true,
                Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);

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
        }
    }
}
