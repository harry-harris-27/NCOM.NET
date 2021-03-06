﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncom.Tests
{
    [TestClass()]
    public class NcomPacketFactoryTests
    {

        private static readonly Random random = new Random();
        private static readonly NcomPacketFactory factory = new NcomPacketFactory();
        private static readonly byte[] sampleNcom = GetSampleNcom();
        private static readonly byte[] sampleNcomSingle = sampleNcom.Take(NcomPacket.PacketLength).ToArray();


        [TestMethod()]
        public void TestProcessNcomLength()
        {
            // Parse the sample ncom
            List<NcomPacket> ncomPackets = factory.ProcessNcom(sampleNcom);

            Assert.AreEqual(sampleNcom.Length / NcomPacket.PacketLength, ncomPackets.Count);
        }

        [TestMethod()]
        public void TestProcessNcomSeek()
        {
            // Hide the NCOM byte data in some random data
            byte[] preamble = new byte[random.Next(100, 200)];
            byte[] postamble = new byte[random.Next(100, 200)];

            // Populate the pre and post amble
            for (int i = 0; i < preamble.Length; i++)
            {
                preamble[i] = (byte)random.Next(0, 0xFF);
                if (preamble[i] == NcomPacket.SyncByte) i--;
            }
            for (int i = 0; i < postamble.Length; i++)
            {
                postamble[i] = (byte)random.Next(0, 0xFF);
                if (postamble[i] == NcomPacket.SyncByte) i--;
            }

            // Create array with a hidden NCOM packet
            byte[] data = new byte[preamble.Length + sampleNcomSingle.Length + postamble.Length];
            Array.Copy(preamble, 0, data, 0, preamble.Length);
            Array.Copy(sampleNcomSingle, 0, data, preamble.Length, sampleNcomSingle.Length);
            Array.Copy(postamble, 0, data, preamble.Length + sampleNcomSingle.Length, postamble.Length);

            // Parse data as NCOM
            List<NcomPacket> pkt = factory.ProcessNcom(data);
            Assert.AreEqual(1, pkt.Count);
        }


        private static byte[] GetSampleNcom()
        {
            var assembly = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Assembly;
            Stream resource = assembly.GetManifestResourceStream("Ncom.Tests.Resources.sample.ncom");
            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                resource.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        
    }
}