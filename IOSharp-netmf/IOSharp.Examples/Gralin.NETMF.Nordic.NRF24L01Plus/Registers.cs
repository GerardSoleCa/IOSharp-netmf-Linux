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

namespace Gralin.NETMF.Nordic
{
    /// <summary>
    ///   Registers for NRF24L01Plus
    ///   Can be read with Commands.R_REGISTER and written by Commands.W_REGISTER
    /// </summary>
    public static class Registers
    {
        /// <summary>
        ///   Configuration Register
        /// </summary>
        public const byte CONFIG = 0x00;

        /// <summary>
        ///   Enable 'Auto Acknowledgment' Function. Disable this functionality to be compatible with nRF2401.
        /// </summary>
        public const byte EN_AA = 0x01;

        /// <summary>
        ///   Enabled RX Addresses
        /// </summary>
        public const byte EN_RXADDR = 0x02;

        /// <summary>
        ///   Setup of Address Widths (common for all data pipes)
        /// </summary>
        public const byte SETUP_AW = 0x03;

        /// <summary>
        ///   Setup of Automatic Retransmission
        /// </summary>
        public const byte SETUP_RETR = 0x04;

        /// <summary>
        ///   RF Channel
        /// </summary>
        public const byte RF_CH = 0x05;

        /// <summary>
        ///   RF Setup Register
        /// </summary>
        public const byte RF_SETUP = 0x06;

        /// <summary>
        ///   Status Register (In parallel to the SPI command word applied on the MOSI pin, the STATUS register is shifted serially out on the MISO pin)
        /// </summary>
        public const byte STATUS = 0x07;

        /// <summary>
        ///   Transmit observe register
        /// </summary>
        public const byte OBSERVE_TX = 0x08;

        /// <summary>
        ///   Received Power Detector.
        /// </summary>
        public const byte RPD = 0x09;

        /// <summary>
        ///   Receive address data pipe 0. 5 Bytes maximum length. (LSByte is written first. Write the number of bytes defined by SETUP_AW)
        /// </summary>
        public const byte RX_ADDR_P0 = 0x0A;

        /// <summary>
        ///   Receive address data pipe 1. 5 Bytes maximum length. (LSByte is written first. Write the number of bytes defined by SETUP_AW)
        /// </summary>
        public const byte RX_ADDR_P1 = 0x0B;

        /// <summary>
        ///   Receive address data pipe 2. Only LSB. MSBytes are equal to RX_ADDR_P1
        /// </summary>
        public const byte RX_ADDR_P2 = 0x0C;

        /// <summary>
        ///   Receive address data pipe 3. Only LSB. MSBytes are equal to RX_ADDR_P1
        /// </summary>
        public const byte RX_ADDR_P3 = 0x0D;

        /// <summary>
        ///   Receive address data pipe 4. Only LSB. MSBytes are equal to RX_ADDR_P1
        /// </summary>
        public const byte RX_ADDR_P4 = 0x0E;

        /// <summary>
        ///   Receive address data pipe 5. Only LSB. MSBytes are equal to RX_ADDR_P1
        /// </summary>
        public const byte RX_ADDR_P5 = 0x0F;

        /// <summary>
        ///   Transmit address. Used for a PTX device only. (LSByte is written first) 
        ///   Set RX_ADDR_P0 equal to this address to handle automatic acknowledge if this is a PTX device with Enhanced ShockBurst™ enabled.
        /// </summary>
        public const byte TX_ADDR = 0x10;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 0
        /// </summary>
        public const byte RX_PW_P0 = 0x11;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 1
        /// </summary>
        public const byte RX_PW_P1 = 0x12;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 2
        /// </summary>
        public const byte RX_PW_P2 = 0x13;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 3
        /// </summary>
        public const byte RX_PW_P3 = 0x14;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 4
        /// </summary>
        public const byte RX_PW_P4 = 0x15;

        /// <summary>
        ///   Number of bytes in RX payload in data pipe 5
        /// </summary>
        public const byte RX_PW_P5 = 0x16;

        /// <summary>
        ///   FIFO Status Register
        /// </summary>
        public const byte FIFO_STATUS = 0x17;

        /// <summary>
        ///   Enable dynamic payload length
        /// </summary>
        public const byte DYNPD = 0x1C;

        /// <summary>
        ///   Feature Register
        /// </summary>
        public const byte FEATURE = 0x1D;
    }
}