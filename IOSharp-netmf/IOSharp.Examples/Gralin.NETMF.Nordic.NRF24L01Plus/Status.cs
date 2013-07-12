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
    public class Status
    {
        private byte _reg;

        public bool DataReady           { get { return (_reg & (1 << Bits.RX_DR)) > 0; } }
        public bool DataSent            { get { return (_reg & (1 << Bits.TX_DS)) > 0; } }
        public bool ResendLimitReached  { get { return (_reg & (1 << Bits.MAX_RT)) > 0; } }
        public bool TxFull              { get { return (_reg & (1 << Bits.TX_FULL)) > 0; } }
        public byte DataPipe            { get { return (byte)((_reg >> 1) & 7); } }
        public bool DataPipeNotUsed     { get { return DataPipe == 6; } }
        public bool RxEmpty             { get { return DataPipe == 7; } }

        public Status(byte reg)
        {
            _reg = reg;
        }

        public void Update(byte reg)
        {
            _reg = reg;
        }

        public override string ToString()
        {
            return "DataReady: " + DataReady +
                   ", DateSent: " + DataSent +
                   ", ResendLimitReached: " + ResendLimitReached +
                   ", TxFull: " + TxFull +
                   ", RxEmpty: " + RxEmpty +
                   ", DataPipe: " + DataPipe +
                   ", DataPipeNotUsed: " + DataPipeNotUsed;
        }
    }
}