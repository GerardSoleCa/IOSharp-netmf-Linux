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
    ///   Commands for NRF24L01Plus
    /// </summary>
    public static class Commands
    {
        /// <summary>
        ///   Read command and status registers. 
        ///   AAAAA = 5 bit Register Map Address
        ///   Data bytes: 1 to 5 (LSByte first)
        /// </summary>
        public const byte R_REGISTER = 0x00;

        /// <summary>
        ///   Write command and status registers. 
        ///   AAAAA = 5 bit Register Map Address Executable in power down or standby modes only.
        ///   Data bytes: 1 to 5 (LSByte first)
        /// </summary>
        public const byte W_REGISTER = 0x20;

        /// <summary>
        ///   Read RX-payload: 1 – 32 bytes. 
        ///   A read operation always starts at byte 0. 
        ///   Payload is deleted from FIFO after it is read. Used in RX mode.
        ///   Data bytes: 1 to 32 (LSByte first)
        /// </summary>
        public const byte R_RX_PAYLOAD = 0x61;

        /// <summary>
        ///   Write TX-payload: 1 – 32 bytes. 
        ///   A write operation always starts at byte 0 used in TX payload.
        ///   Data bytes: 1 to 32 (LSByte first)
        /// </summary>
        public const byte W_TX_PAYLOAD = 0xA0;

        /// <summary>
        ///   Flush TX FIFO, used in TX mode
        ///   Data bytes: 0
        /// </summary>
        public const byte FLUSH_TX = 0xE1;

        /// <summary>
        ///   Flush RX FIFO, used in RX mode Should not be executed during transmission of acknowledge. 
        ///   Acknowledge package will not be completed.
        ///   Data bytes: 0
        /// </summary>
        public const byte FLUSH_RX = 0xE2;

        /// <summary>
        ///   Used for a PTX device Reuse last transmitted payload. 
        ///   TX payload reuse is active until W_TX_PAYLOAD or FLUSH TX is executed. 
        ///   TX payload reuse must not be activated or deactivated during package transmission.
        ///   Data bytes: 0
        /// </summary>
        public const byte REUSE_TX_PL = 0xE3;

        /// <summary>
        ///   Read RX payload width for the top R_RX_PAYLOAD in the RX FIFO.
        ///   Flush RX FIFO if the read value is larger than 32 bytes.
        ///   Data bytes: 1
        /// </summary>
        public const byte R_RX_PL_WID = 0x60;

        /// <summary>
        ///   Used in RX mode.
        ///   Write Payload to be transmitted together with ACK packet on PIPE PPP.
        ///   PPP valid in the range from 000 to 101.
        ///   Maximum three ACK packet payloads can be pending. 
        ///   Payloads with same PPP are handled using first in - first out principle. 
        ///   Write payload: 1– 32 bytes. 
        ///   A write operation always starts at byte 0.
        ///   Data bytes: 1 to 32 (LSByte first)
        /// </summary>
        public const byte W_ACK_PAYLOAD = 0xA8;

        /// <summary>
        ///   Used in TX mode. Disables AUTOACK on this specific packet.
        ///   Data bytes: 1 to 32 (LSByte first)
        /// </summary>
        public const byte W_TX_PAYLOAD_NO_ACK = 0xB0;

        /// <summary>
        ///   No Operation. Might be used to read the STATUS register
        ///   Data bytes: 0
        /// </summary>
        public const byte NOP = 0xFF;
    }
}