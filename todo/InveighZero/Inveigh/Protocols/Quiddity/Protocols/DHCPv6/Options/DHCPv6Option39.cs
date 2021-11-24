﻿/*
 * BSD 3-Clause License
 *
 * Copyright (c) 2021, Kevin Robertson
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 * list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission. 
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using Quiddity.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Quiddity.DHCPv6
{
    class DHCPv6Option39 : DHCPv6Option
    {
        // https://datatracker.ietf.org/doc/html/rfc4704

        public byte Flags { get; set; }
        public string DomainName { get; set; }

        public DHCPv6Option39()
        {

        }

        public DHCPv6Option39(byte[] data)
        {
            ReadBytes(data, 0);
        }

        public DHCPv6Option39(byte[] data, int index)
        {
            ReadBytes(data, index);
        }

        public new void ReadBytes(byte[] data, int index)
        {

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                PacketReader packetReader = new PacketReader(memoryStream);
                memoryStream.Position = index;
                this.OptionCode = packetReader.BigEndianReadUInt16();
                this.OptionLen = packetReader.BigEndianReadUInt16();
                this.Flags = packetReader.ReadByte();
                this.DomainName = ConvertName(packetReader.ReadBytes(this.OptionLen - 1));
            }

        }

        public byte[] GetBytes()
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                PacketWriter packetWriter = new PacketWriter(memoryStream);
                packetWriter.Write(this.OptionCode);
                packetWriter.Write(this.OptionLen);
                packetWriter.Write(this.Flags);
                packetWriter.Write(this.DomainName);
                return memoryStream.ToArray();
            }

        }

        protected virtual string ConvertName(byte[]data)
        {
            string hostname = "";
            int hostnameLength = data[0];
            int index = 0;
            int i = 0;

            do
            {
                int hostnameSegmentLength = hostnameLength;
                byte[] hostnameSegment = new byte[hostnameSegmentLength];
                Buffer.BlockCopy(data, (index + 1), hostnameSegment, 0, hostnameSegmentLength);
                hostname += Encoding.UTF8.GetString(hostnameSegment);

                if (hostnameLength + 1 == data.Length)
                {
                    return hostname;
                }

                index += hostnameLength + 1;
                hostnameLength = data[index];
                i++;             

                if (hostnameLength > 0)
                {
                    hostname += ".";
                }
                
            }
            while (hostnameLength != 0 && i <= 127);

            return hostname;
        }

    }
}
