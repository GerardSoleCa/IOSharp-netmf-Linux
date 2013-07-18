using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using Microsoft.SPOT.Hardware;
using IOSharp.Utils;
using System.Threading;

namespace IOSharp.Exmples
{
    class UARTExample
    {
        static SerialPort serial;
        static void Main(string[] args)
        {
            // provide some usage information
            System.Console.WriteLine("enter some text and hit ENTER.");
            System.Console.WriteLine("enter 'x' and hit ENTER to exit.");
            System.Console.WriteLine();

            // initialize the serial port for COM3 (could be other port, depends on system)
            serial = new SerialPort("/dev/ttyAMA0", 115200, Parity.None, 8, StopBits.One);
            // open the serial-port, so we can send & receive data
            serial.Open();
            // add an event-handler for handling incoming data
            serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            Thread.Sleep(-1);
            // this will hold each line entered
            string line = string.Empty;

            // as long as an x is not entered
            while (line.ToLowerInvariant() != "x")
            {
                // read a single line from the console
                line = System.Console.ReadLine();

                // convert the line to bytes
                byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(line);

                // send the bytes over the serial-port
                serial.Write(utf8Bytes, 0, utf8Bytes.Length);
                Console.WriteLine("stop");
                Thread.Sleep(-1);
                
            }
        }

        static void Read()
        {
            Console.WriteLine("Hola");
            // wait a little for the buffer to fill
            System.Threading.Thread.Sleep(100);

            // create an array for the incoming bytes
            byte[] bytes = new byte[serial.BytesToRead];
            // read the bytes
            serial.Read(bytes, 0, bytes.Length);
            // convert the bytes into a string
            string line = System.Text.Encoding.UTF8.GetString(bytes);

            // write the received bytes, as a string, to the console
            System.Console.WriteLine("echo: " + line);
            System.Console.WriteLine();
        }

        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("Hola");
            // wait a little for the buffer to fill
            System.Threading.Thread.Sleep(100);

            // create an array for the incoming bytes
            byte[] bytes = new byte[serial.BytesToRead];
            // read the bytes
            serial.Read(bytes, 0, bytes.Length);
            // convert the bytes into a string
            string line = System.Text.Encoding.UTF8.GetString(bytes);

            // write the received bytes, as a string, to the console
            System.Console.WriteLine("echo: " + line);
            System.Console.WriteLine();
        }
    }
}
