#region Licence

// Copyright (C) 2011 by Jakub Bartkowiak (Gralin)
// 
// MIT Licence
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#endregion

#define Nordic_BUG //si es treu no funciona la recepció de paquets !?!?!?
//#define Nordic_COMM_DEBUG

using System;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
#if Expand
    using OutputPort = DriverExpand.OutputPort;
    using SPI = DriverExpand.SPI;
#endif


namespace Gralin.NETMF.Nordic
{
    /// <summary>
    ///   Driver class for Nordic nRF24L01+ tranceiver
    /// </summary>
    public class NRF24L01Plus
    {
        #region Delegates

        public delegate void EventHandler();

        public delegate void OnDataRecievedHandler(byte[] data, byte pipe);

        public delegate void OnInterruptHandler(Status status);

        #endregion

        private static Object _mHandle = new Object();
        private byte[] _slot0Address;
        private byte[] _slot1Address;
        private byte[] _slot2Address;
        private byte[] _slot3Address;
        private byte[] _slot4Address;
        private byte[] _slot5Address;
        private OutputPort _cePin;
        private bool _initialized;
        private InterruptPort _irqPin;
        private SPI _spiPort;
        private bool _enabled;
        private readonly ManualResetEvent _transmitSuccessFlag;
        private readonly ManualResetEvent _transmitFailedFlag;
        public readonly byte[] _def_rx_addr_p0 = Encoding.UTF8.GetBytes("0PIPE");
        public readonly byte[] _def_rx_addr_p1 = Encoding.UTF8.GetBytes("APIPE");
        public readonly byte[] _def_rx_addr_p2 = Encoding.UTF8.GetBytes("B");
        public readonly byte[] _def_rx_addr_p3 = Encoding.UTF8.GetBytes("C");
        public readonly byte[] _def_rx_addr_p4 = Encoding.UTF8.GetBytes("D");
        public readonly byte[] _def_rx_addr_p5 = Encoding.UTF8.GetBytes("E");
        private byte[] _id;
        public readonly AddressSlot[] slot = new AddressSlot[6];

        public byte[] ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether module is enabled (RX or TX mode).
        /// </summary>
        public bool IsEnabled
        {
            get { return _cePin.Read(); }
        }

        public NRF24L01Plus()
        {
            _transmitSuccessFlag = new ManualResetEvent(false);
            _transmitFailedFlag = new ManualResetEvent(false);
            slot[0] = AddressSlot.Zero;
            slot[1] = AddressSlot.One;
            slot[2] = AddressSlot.Two;
            slot[3] = AddressSlot.Three;
            slot[4] = AddressSlot.Four;
            slot[5] = AddressSlot.Five;
        }

        /// <summary>
        ///   Enables the module
        /// </summary>
        public void Enable()
        {
            _enabled = true;
            SetEnabled();
        }

        /// <summary>
        ///   Disables the module
        /// </summary>
        public void Disable()
        {
            _enabled = false;
            SetDisabled();
        }

        /// <summary>
        ///   Initializes SPI connection and control pins
        /// </summary>
        public void Initialize(SPI spi, OutputPort chipEnablePin, InterruptPort interruptPort)
        {
            // Chip Select : Active Low
            // Clock : Active High, Data clocked in on rising edge
            _spiPort = spi;

            // Initialize IRQ Port
            _irqPin = interruptPort;
            _irqPin.OnInterrupt += HandleInterrupt;

            // Initialize Chip Enable Port
            _cePin = chipEnablePin;

            // Module reset time
            Thread.Sleep(100);

            _initialized = true;
        }

        /// <summary>
        /// Configure the module basic settings. Module needs to be initiaized.
        /// </summary>
        /// <param name="address">RF address (3-5 bytes). The width of this address determins the width of all addresses used for sending/receiving.</param>
        /// <param name="channel">RF channel (0-127)</param>
        public void Configure(byte[] address, byte channel)
        {
            CheckIsInitialized();
            AddressWidth.Check(address);

            // Set radio channel
            Execute(Commands.W_REGISTER, Registers.RF_CH,
                    new[]
                        {
                            (byte) (channel & 0x7F) // channel is 7 bits
                        });

            // Enable dynamic payload length
            Execute(Commands.W_REGISTER, Registers.FEATURE,
                    new[]
                        {
                            (byte) (1 << Bits.EN_DPL)
                        });

            // Set auto-ack
            Execute(Commands.W_REGISTER, Registers.EN_AA,
                    new[] { (byte)(0x01) });

            // Set RX-addr EN_RXADDR
            Execute(Commands.W_REGISTER, Registers.EN_RXADDR,
                  new[] { (byte)(0x01) });

            // Set dynamic payload length for pipes
            Execute(Commands.W_REGISTER, Registers.DYNPD,
                    new[]
                        {
                            (byte) (1 << Bits.DPL_P0 |
                                    1 << Bits.DPL_P1)
                        });

            // Flush RX FIFO
            Execute(Commands.FLUSH_RX, 0x00, new byte[0]);

            // Flush TX FIFO
            Execute(Commands.FLUSH_TX, 0x00, new byte[0]);

            // Clear IRQ Masks
            Execute(Commands.W_REGISTER, Registers.STATUS,
                    new[]
                        {
                            (byte) (1 << Bits.MASK_RX_DR |
                                    1 << Bits.MASK_TX_DS |
                                    1 << Bits.MAX_RT)
                        });

            // Set default address
            Execute(Commands.W_REGISTER, Registers.SETUP_AW,
                    new[]
                        {
                            AddressWidth.Get(address)
                        });

            // Set module address
            SetAddress(AddressSlot.Zero, address, true);
            //Execute(Commands.W_REGISTER, (byte)AddressSlot.Zero, address); //No cal pq també s'executa quan es posa en receive mode al final de configure
            _slot1Address = _def_rx_addr_p1;
            _slot2Address = _def_rx_addr_p2;
            _slot3Address = _def_rx_addr_p3;
            _slot4Address = _def_rx_addr_p4;
            _slot5Address = _def_rx_addr_p5;

            // Set retransmission values
            Execute(Commands.W_REGISTER, Registers.SETUP_RETR,
                    new[]
                        {
                            (byte) (0x01 << Bits.ARD |
                                    0x00 << Bits.ARC)
                        });

            // Set RF Setup
            Execute(Commands.W_REGISTER, Registers.RF_SETUP,
                    new[]
                        {
                            (byte) (0xBF & 0x05) // bit 6 is reserved
                        });



            // Setup, CRC enabled, Power Up, PRX
            SetReceiveMode();
        }

        /// <summary>
        /// Disables the auto-auknowledge for a specific module address slot
        /// </summary>
        public void DisableAA(AddressSlot s)
        {
            byte[] b = new byte[2];
            byte[] bb = new byte[1];
            switch (s)
            {
                case AddressSlot.Zero:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P0))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.One:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P1))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Two:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P2))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Three:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P3))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Four:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P4))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Five:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P5))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
            }
        }

        /// <summary>
        /// Enables the auto-auknowledge for a specific module address slot
        /// </summary>
        public void EnableAA(AddressSlot s)
        {
            byte[] b = new byte[2];
            byte[] bb = new byte[1];
            switch (s)
            {
                case AddressSlot.Zero:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P0)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.One:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P1)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Two:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P2)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Three:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P3)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Four:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P4)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
                case AddressSlot.Five:
                    b = Execute(Commands.R_REGISTER, Registers.EN_AA, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P5)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_AA, bb);
                    break;
            }
        }

        /// <summary>
        /// Disables a specific module address slot for reception
        /// </summary>
        public void DisableRX(AddressSlot s)
        {
            byte[] b = new byte[2];
            byte[] bb = new byte[1];
            switch (s)
            {
                case AddressSlot.Zero:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P0))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.One:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P1))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Two:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P2))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Three:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P3))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Four:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P4))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Five:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] & (~((byte)(1 << Bits.ENAA_P5))));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
            }
        }

        /// <summary>
        /// Enables a specific module address slot for reception
        /// </summary>
        public void EnableRX(AddressSlot s)
        {
            byte[] b = new byte[2];
            byte[] bb = new byte[1];
            switch (s)
            {
                case AddressSlot.Zero:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P0)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.One:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P1)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Two:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P2)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Three:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P3)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Four:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P4)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
                case AddressSlot.Five:
                    b = Execute(Commands.R_REGISTER, Registers.EN_RXADDR, new byte[1]);
                    b[1] = (byte)(b[1] | ((byte)(1 << Bits.ENAA_P5)));
                    Array.Copy(b, 1, bb, 0, 1);
                    Execute(Commands.W_REGISTER, Registers.EN_RXADDR, bb);
                    break;
            }
        }

        /// <summary>
        /// Set one of 6 available module addresses and enables it when in RX mode, sets also auto-ack if EN_AA is true
        /// </summary>
        public void SetAddress(AddressSlot s, byte[] address, bool EN_AA)
        {
            byte[] addr = new byte[1];

            CheckIsInitialized();

            if (s == AddressSlot.Zero || s == AddressSlot.One)
            {
                AddressWidth.Check(address);
                Execute(Commands.W_REGISTER, (byte)s, address);
            }
            else
            {
                addr[0] = (byte)address.GetValue(0);
                Execute(Commands.W_REGISTER, (byte)s, addr);
            }
            switch (s)
            {
                case AddressSlot.Zero: _slot0Address = address;
                    break;
                case AddressSlot.One: _slot1Address = address;
                    break;
                case AddressSlot.Two: _slot2Address = addr;
                    break;
                case AddressSlot.Three: _slot3Address = addr;
                    break;
                case AddressSlot.Four: _slot4Address = addr;
                    break;
                case AddressSlot.Five: _slot5Address = addr;
                    break;
            }
            if (EN_AA)
            {
                EnableAA(s);
            }
            EnableRX(s);
        }

        /// <sumary>
        /// Returns the default address value for the pipe
        /// </sumary>
        public byte[] GetDefaultAddress(AddressSlot s)
        {
            byte[] addr = new byte[1];

            switch (s)
            {
                case AddressSlot.Zero: addr = _def_rx_addr_p0;
                    break;
                case AddressSlot.One: addr = _def_rx_addr_p1;
                    break;
                case AddressSlot.Two: addr = _def_rx_addr_p2;
                    break;
                case AddressSlot.Three: addr = _def_rx_addr_p3;
                    break;
                case AddressSlot.Four: addr = _def_rx_addr_p4;
                    break;
                case AddressSlot.Five: addr = _def_rx_addr_p5;
                    break;
            }
            return addr;
        }

        /// <summary>
        /// Set one of 6 available module addresses to its default value and disables it
        /// </summary>
        public void SetToDefaultAddress(AddressSlot s)
        {
            byte[] addr = new byte[1];

            CheckIsInitialized();


            switch (s)
            {
                case AddressSlot.Zero:
                    AddressWidth.Check(_def_rx_addr_p0);
                    Execute(Commands.W_REGISTER, (byte)s, _def_rx_addr_p0);
                    _slot0Address = _def_rx_addr_p0;
                    break;
                case AddressSlot.One:
                    AddressWidth.Check(_def_rx_addr_p1);
                    Execute(Commands.W_REGISTER, (byte)s, _def_rx_addr_p1);
                    _slot0Address = _def_rx_addr_p1;
                    break;
                case AddressSlot.Two:
                    addr[0] = (byte)_def_rx_addr_p2.GetValue(0);
                    Execute(Commands.W_REGISTER, (byte)s, addr);
                    _slot2Address = addr;
                    break;
                case AddressSlot.Three:
                    addr[0] = (byte)_def_rx_addr_p3.GetValue(0);
                    Execute(Commands.W_REGISTER, (byte)s, addr);
                    _slot3Address = addr;
                    break;
                case AddressSlot.Four:
                    addr[0] = (byte)_def_rx_addr_p4.GetValue(0);
                    Execute(Commands.W_REGISTER, (byte)s, addr);
                    _slot4Address = addr;
                    break;
                case AddressSlot.Five:
                    addr[0] = (byte)_def_rx_addr_p5.GetValue(0);
                    Execute(Commands.W_REGISTER, (byte)s, addr);
                    _slot5Address = addr;
                    break;
            }

            DisableAA(s);
            DisableRX(s);
        }

        /// <summary>
        /// Read 1 of 6 available module addresses
        /// </summary>
        public byte[] GetAddress(AddressSlot slot, int width)
        {
            var read = new byte[width];
            var result = new byte[width];
            CheckIsInitialized();

            if (slot == AddressSlot.Zero || slot == AddressSlot.One)
            {
                AddressWidth.Check(width);
            }
            else
            {
                width = 1;
            }
            read = Execute(Commands.R_REGISTER, (byte)slot, new byte[width]);
            result = new byte[read.Length - 1];
            Array.Copy(read, 1, result, 0, result.Length);
            return result;
        }

        /// <summary>
        ///   Executes a command in NRF24L01+ (for details see module datasheet)
        /// </summary>
        /// <param name = "command">Command</param>
        /// <param name = "addres">Register to write to</param>
        /// <param name = "data">Data to write</param>
        /// <returns>Response byte array. First byte is the status register</returns>
        public byte[] Execute(byte command, byte addres, byte[] data)
        {
            CheckIsInitialized();

            // This command requires module to be in power down or standby mode
            if (command == Commands.W_REGISTER)
                SetDisabled();

            // Create SPI Buffers with Size of Data + 1 (For Command)
            var writeBuffer = new byte[data.Length + 1];
            var readBuffer = new byte[data.Length + 1];

            // Add command and adres to SPI buffer
            writeBuffer[0] = (byte)(command | addres);

            // Add data to SPI buffer
            Array.Copy(data, 0, writeBuffer, 1, data.Length);

            // Do SPI Read/Write
            _spiPort.WriteRead(writeBuffer, readBuffer);

            // Enable module back if it was disabled
            if (command == Commands.W_REGISTER && _enabled)
                SetEnabled();

            // Return ReadBuffer
            return readBuffer;
        }

        /// <summary>
        ///   Gets module basic status information
        /// </summary>
        public Status GetStatus()
        {
            CheckIsInitialized();

            var readBuffer = new byte[1];
            _spiPort.WriteRead(new[] { Commands.NOP }, readBuffer);

            return new Status(readBuffer[0]);
        }

        /// <summary>
        ///   Reads the current rf channel value set in module
        /// </summary>
        /// <returns></returns>
        public byte GetChannel()
        {
            CheckIsInitialized();

            var result = Execute(Commands.R_REGISTER, Registers.RF_CH, new byte[1]);
            return result[1];
        }

        /// <summary>
        ///   Gets the module radio frequency [MHz]
        /// </summary>
        /// <returns>Frequency in MHz</returns>
        public int GetFrequency()
        {
            return 2400 + GetChannel();
        }

        /// <summary>
        ///   Sets the rf channel value used by all data pipes
        /// </summary>
        /// <param name="channel">7 bit channel value</param>
        public void SetChannel(byte channel)
        {
            CheckIsInitialized();

            var writeBuffer = new[] { (byte)(channel & 0x7F) };
            Execute(Commands.W_REGISTER, Registers.RF_CH, writeBuffer);
        }

        /// <summary>
        ///   Send <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a non blocking method.
        /// </summary>
        public void SendTo(byte[] address, byte[] bytes)
        {
//            Debug.Print("Send Thread = " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(0); // garantees at least 20 ms to be executed in this thread for the next iteration
            // Chip enable low
            Disable();
            //SetDisabled();

            // Setup PTX (Primary TX)
            SetTransmitMode();

            // Write transmit adres to TX_ADDR register. 
            Execute(Commands.W_REGISTER, Registers.TX_ADDR, address);

            // Write transmit adres to RX_ADDRESS_P0 (Pipe0) (For Auto ACK)
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, address);

            // Print that it is sending 
#if  Nordic_COMM_DEBUG
            char[] converted = new char[bytes.Length];
            int i = 0;
            foreach (byte b in bytes)
                converted[i++] = (char)b;

            Debug.Print("... Sending -> " + new String(converted) + " using pipe -> " + new String(Encoding.UTF8.GetChars(address)) + " " + DateTime.Now.Second + ":" + DateTime.Now.Millisecond);
#endif
                        
            // Send payload
            Execute(Commands.W_TX_PAYLOAD, 0x00, bytes);

            // Pulse for CE -> starts the transmission.
            Enable();
            //SetEnabled();
            // There is no need to set it to RX mode because it will be set as RX mode when an Interrupt is Handled
        
            //For transmission:
            //To enter this mode, the nRF24L01 must have the PWR_UP bit set high, PRIM_RX bit set low, a payload in the TX FIFO and, a high
            //pulse on the CE for more than 10μs.
            //PRIM_RX = 0
            //CE = 1 for more than 10μs
        }

        /// <summary>
        ///   Sends <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a blocking method that returns true if data was received by the recipient or false if timeout occured.
        /// </summary>
        public bool SendTo(byte[] address, byte[] bytes, int timeout)
        {
            var startTime = DateTime.Now;

            while (true)
            {
                _transmitSuccessFlag.Reset();
                _transmitFailedFlag.Reset();

                SendTo(address, bytes);

                if (WaitHandle.WaitAny(new[] { _transmitSuccessFlag, _transmitFailedFlag }, 200, true) == 0)
                    return true; // when handler function executes in order to turn on the transmitSuccessFlag or the transmitFailedFlag, it changes to receive mode

                if (DateTime.Now.CompareTo(startTime.AddMilliseconds(timeout)) > 0)
                {   // when the sending ends because the timeout overflows, changing to receive mode as well as cleaning registers and enable the NRF24L01 has to be done manually.
#if Nordic_COMM_DEBUG
                    Debug.Print("timeout");
#endif
                    //// Disable RX/TX
                    //Disable();
                    //// Set PRX
                    //SetReceiveMode();
                    //// Flush TX FIFO 
                    //Execute(Commands.FLUSH_TX, 0x00, new byte[0]);
                    //// Enable RX
                    //SetEnabled();
                    //_transmitFailedFlag.Set();
                    //OnTransmitFailed();
                    //Enable();
                    //_irqPin.ClearInterrupt();
                    return false;
                }
#if Nordic_COMM_DEBUG
                Debug.Print("Retransmitting packet...");
#endif
            }
        }

        /// <summary>
        /// Sets fixed payload paquets of <param name="size">size</param> bytes weitdh.
        /// </summary>
        public void SetFixedPayload(int size)
        {
            Execute(Commands.W_REGISTER, Registers.FEATURE,
                   new[]
                        {
                            (byte) (0x00)
                        });

            Execute(Commands.W_REGISTER, Registers.DYNPD,
                  new[]
                        {
                            (byte) (0x00)
                        });

            Execute(Commands.W_REGISTER, Registers.RX_PW_P0, new[] { (byte)size }); // Posem payload de la pipe0 a size
            Execute(Commands.W_REGISTER, Registers.RX_PW_P1, new[] { (byte)size }); // Posem payload de la pipe1 a size
            Execute(Commands.W_REGISTER, Registers.RX_PW_P2, new[] { (byte)size }); // Posem payload de la pipe2 a size
            Execute(Commands.W_REGISTER, Registers.RX_PW_P3, new[] { (byte)size }); // Posem payload de la pipe3 a size
            Execute(Commands.W_REGISTER, Registers.RX_PW_P4, new[] { (byte)size }); // Posem payload de la pipe4 a size
            Execute(Commands.W_REGISTER, Registers.RX_PW_P5, new[] { (byte)size }); // Posem payload de la pipe5 a size
        }

        /// <summary>
        /// returns the first free pipe loacted or 9 if there are not free pipes available
        /// </summary>
        private void HandleInterrupt(uint data1, uint data2, DateTime dateTime)
        {
            //Debug.Print("Interrupt Thread = " + Thread.CurrentThread.ManagedThreadId);
            if (!_initialized)
                return;

            if (!_enabled)
            {
                // Flush RX FIFO
                Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);
                return;
            }

            // Disable RX/TX
            //SetDisabled();
            Disable();

            // Set PRX
            SetReceiveMode();

            // there are 3 rx pipes in rf module so 3 arrays should be enough to store incoming data
            // sometimes though more than 3 data packets are received somehow
            var payloads = new byte[6][];

            var status = GetStatus();
            byte pipe = status.DataPipe;
            byte payloadCount = 0;
            var payloadCorrupted = false;

            OnInterrupt(status);

            if (status.DataReady)
            {
                while (!status.RxEmpty)
                {
                    // Read payload size
                    var payloadLength = Execute(Commands.R_RX_PL_WID, 0x00, new byte[1]);

                    // this indicates corrupted data
                    if (payloadLength[1] > 32)
                    {
                        payloadCorrupted = true;

                        // Flush anything that remains in buffer
                        Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                    }
                    else
                    {
                        if (payloadCount >= payloads.Length)
                        {
#if Nordic_COMM_DEBUG
                            Debug.Print("Unexpected payloadCount value = " + payloadCount);
#endif
                            Execute(Commands.FLUSH_RX, 0x00, new byte[0]);
                        }
                        else
                        {
                            // Read payload data
                            payloads[payloadCount] = Execute(Commands.R_RX_PAYLOAD, 0x00, new byte[payloadLength[1]]);
                            payloadCount++;
                        }
                    }

                    // Clear RX_DR bit 
                    var result = Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.RX_DR) });
                    status.Update(result[0]);
                }
            }

            if (status.ResendLimitReached)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);

                // Clear MAX_RT bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.MAX_RT) });
            }

            if (status.TxFull)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new byte[0]);
            }

            if (status.DataSent)
            {
                // Clear TX_DS bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (byte)(1 << Bits.TX_DS) });
            }

            // Enable RX
            //SetEnabled();
            Enable();
          //  _irqPin.ClearInterrupt();

            if (payloadCorrupted)
            {
#if Nordic_COMM_DEBUG
                Debug.Print("Corrupted data received");
#endif
            }
            else if (payloadCount > 0)
            {
                if (payloadCount > payloads.Length)
#if Nordic_BUG
                    Debug.Print("Unexpected payloadCount value = " + payloadCount);
#endif

                for (var i = 0; i < System.Math.Min(payloadCount, payloads.Length); i++)
                {
                    var payload = payloads[i];
                    var payloadWithoutCommand = new byte[payload.Length - 1];
                    Array.Copy(payload, 1, payloadWithoutCommand, 0, payload.Length - 1);
                    OnDataReceived(payloadWithoutCommand, pipe);
                }
            }
            else if (status.DataSent)
            {
                _transmitSuccessFlag.Set();
                OnTransmitSuccess();
            }
            else
            {
                _transmitFailedFlag.Set();
                OnTransmitFailed();
            }
            if (!GetStatus().RxEmpty)
            {
#if Nordic_COMM_DEBUG
                Debug.Print("!!!!!!!!!!!!!!!! RxEmpty és false --- DataPipe " + GetStatus().DataPipe);
#endif
                //   HandleInterrupt(data1, data2, dateTime);
            }

            ////Enables RX/TX
            //Enable();
            //_irqPin.ClearInterrupt();
        }

        private void SetEnabled()
        {
            //cal netejat interrupcions?
           // _irqPin.ClearInterrupt();
            _irqPin.EnableInterrupt();
            _cePin.Write(true);
        }

        private void SetDisabled()
        {
            _cePin.Write(false);
            _irqPin.DisableInterrupt();
        }

        private void SetTransmitMode()
        {
            Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (byte) (1 << Bits.PWR_UP |
                                    1 << Bits.EN_CRC |
                                    1 << Bits.CRCO   |
                                    0 << Bits.PRIM_RX)   //1- recibe, 0-transmit
                        });
        }

        private void SetReceiveMode()
        {
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, _slot0Address);
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P1, _slot1Address);
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P2, _slot2Address);
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P3, _slot3Address);
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P4, _slot4Address);
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P5, _slot5Address);

            Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (byte) (1 << Bits.PWR_UP |
                                    1 << Bits.EN_CRC |
                                    1 << Bits.CRCO |
                                    1 << Bits.PRIM_RX)
                        });
        }

        private void CheckIsInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Initialize method needs to be called before this call");
            }
        }

        private static bool equals(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///   Called on every IRQ interrupt
        /// </summary>
        public event OnInterruptHandler OnInterrupt = delegate { };

        /// <summary>
        ///   Occurs when data packet has been received
        /// </summary>
        public event OnDataRecievedHandler OnDataReceived = delegate { };

        /// <summary>
        ///   Occurs when ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitSuccess = delegate { };

        /// <summary>
        ///   Occurs when no ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitFailed = delegate { };
    }
}