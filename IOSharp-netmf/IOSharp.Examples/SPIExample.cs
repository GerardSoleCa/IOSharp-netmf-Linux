using System;
using System.Threading;
using IOSharp.NETMF.RaspberryPi.Hardware;
using Microsoft.SPOT.Hardware;

namespace IOSharp.Exmples
{
    class SPIExample
    {
        private SPI spiDevice;
        private OutputPort led;
        private InputPort button;
        private int bufferSize = 16;
        private int sendBuffer = 25;

        public static void Main()
        {
            new SPIExample().Run();
        }

        private void Run()
        {
            ConfigureSPI();
            ConfigureInOut();
        }

        private void ConfigureInOut()
        {
            led = new OutputPort(Pins.ONBOARD_LED, false);
            button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);

            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);

            Thread.Sleep(-1);
        }

        private void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            led.Write(true);
            ResetDevice();
            //ReadTag();
            TestSPI1();
            led.Write(false);
        }

        private void ConfigureSPI()
        {
            SPI.Configuration xSPIConfig;

            xSPIConfig = new SPI.Configuration(Pins.GPIO_PIN_D5,    //Chip Select pin
                                                false,              //Chip Select Active State
                                                50,                  //Chip Select Setup Time
                                                0,                  //Chip Select Hold Time
                                                false,              //Clock Idle State
                                                true,               //Clock Edge
                                                1000,               //Clock Rate (kHz)
                                                SPI.SPI_module.SPI1);//SPI Module
            spiDevice = new SPI(xSPIConfig);
        }

        private void ResetDevice()
        {
            byte reset = 0x0f;

            byte[] data = new byte[1];
            data[0] = FormCommand(reset);

            byte[] ReadBuffer = new byte[sendBuffer];
            byte[] WriteBuffer = new byte[data.Length + 1];

            WriteBuffer[0] = ((byte)(0x7E & (0x01 << 1)));

            for (int i = 0; i < data.Length; ++i)
            {
                WriteBuffer[i + 1] = data[i];
            }

            spiDevice.WriteRead(WriteBuffer, ReadBuffer);
        }

        private void _ReadTag()
        {
            // Create read buffer
            byte[] ReadBuffer = new byte[sendBuffer];

            // Create and fill write buffer
            byte[] WriteBuffer = new byte[sendBuffer];

            // Fill the write buffer with 0xFF, is not always necessary
            WriteBuffer[0] = 0x20;
            for (int c = 0; c < sendBuffer; c++)
            {
                ReadBuffer[c] = 0xff;

                if (c > 0)
                    WriteBuffer[c] = 0x20;
            }

            // Transfer data
            spiDevice.WriteRead(WriteBuffer, ReadBuffer);

            // Debug Read data
            Console.WriteLine("Recieved " + sendBuffer + " bytes from SPI Slave");
            //Debug.Print(new String(System.Text.Encoding.UTF8.GetChars(ReadBuffer)));

            String s = "";

            for (int c = 0; c < sendBuffer; c++)
            {
                //Debug.Print(c.ToString() + ": " + ReadBuffer[c].ToString());
                s += ReadBuffer[c].ToString() + " ";
            }

            Console.WriteLine(s);
        }

        private void ReadTag()
        {
            byte receive = 0x08;

            byte data = FormCommand(receive);

            WriteReg(0x01, data);
        }

        private void TestSPI1()
        {
            byte randomID = 0x0A;

            byte data = FormCommand(randomID);

            ReadReg(0x01, data);
        }

        private byte FormCommand(byte cmd)
        {
            return (byte)(0x30 | (cmd & 0x0f));
        }

        private void WriteReg(byte addr, byte data)
        {
            byte[] ReadBuffer = new byte[2];
            byte[] WriteBuffer = new byte[2];

            WriteBuffer[0] = ((byte)(0x7E & (addr << 1)));

            WriteBuffer[1] = data;
            //for (int i = 0; i < data.Length; ++i)
            //{
            //    WriteBuffer[i + 1] = data[i];
            //}

            spiDevice.WriteRead(WriteBuffer, ReadBuffer);

            Console.WriteLine("Received " + ReadBuffer.Length + " bytes from SPI Slave");
            //Debug.Print(new String(System.Text.Encoding.UTF8.GetChars(ReadBuffer)));

            String s = "";

            for (int c = 0; c < 2; c++)
            {
                //Debug.Print(c.ToString() + ": " + ReadBuffer[c].ToString());
                s += ReadBuffer[c].ToString() + " ";
            }

            Console.WriteLine(s);
        }

        private void ReadReg(byte addr, byte data)
        {
            byte[] ReadBuffer = new byte[2];
            byte[] WriteBuffer = new byte[2];

            WriteBuffer[0] = ((byte)(0x80 | (0x7E & (addr << 1))));
            WriteBuffer[1] = 0;

            spiDevice.WriteRead(WriteBuffer, ReadBuffer);

            Console.WriteLine("Received " + ReadBuffer.Length + " bytes from SPI Slave");
            //Debug.Print(new String(System.Text.Encoding.UTF8.GetChars(ReadBuffer)));

            String s = "";

            for (int c = 0; c < 2; c++)
            {
                //Debug.Print(c.ToString() + ": " + ReadBuffer[c].ToString());
                s += ReadBuffer[c].ToString() + " ";
            }

            Console.WriteLine(s);
        }
    }
}
