using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SPOT.Hardware;
using Gralin.NETMF.Nordic;

namespace IOSharp.Examples
{
    class SimpleNordicWriteAddress
    {

        static void Main(string[] args)
        {
            SPI SPIport = new Microsoft.SPOT.Hardware.SPI(new Microsoft.SPOT.Hardware.SPI.Configuration(Cpu.Pin.GPIO_NONE, false, 0, 0, false, true, 2000, SPI.SPI_module.SPI1));
            OutputPort nCE = new OutputPort(Cpu.Pin.GPIO_Pin2, true);
            InterruptPort nINT = new InterruptPort(Cpu.Pin.GPIO_Pin4, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            NRF24L01Plus n = new NRF24L01Plus();
            n.Initialize(SPIport, nCE, nINT);

            byte[] address = n.GetAddress(AddressSlot.Zero, 5);
            Console.WriteLine("First Address: " + ByteArrayToHexString(address));

            byte[] b = new byte[] { 0x04, 0x09, 0x02, 0x03, 0x04 };
            n.SetAddress(AddressSlot.Zero, b, false);

            address = n.GetAddress(AddressSlot.Zero, 5);
            Console.WriteLine("Second Address: " + ByteArrayToHexString(address));

            nCE.Dispose();
            nINT.Dispose();
        }

        private static String ByteArrayToHexString(byte[] p)
        {
            char[] c = new char[p.Length * 4 + (int)p.Length / 8];
            byte b;

            for (int y = 0, x = 0; y < p.Length; ++y, ++x)
            {
                b = ((byte)(p[y] >> 4));
                c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(p[y] & 0xF));
                c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                c[++x] = ' ';

                if ((y + 1) % 8 == 0)
                {
                    c[++x] = '\n';
                }
            }
            return new String(c);
        }

    }

}
