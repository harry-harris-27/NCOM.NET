﻿using NCOM.Enumerations;
using NCOM.StatusChannels;
using NCOM.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCOM
{
    /// <summary>
    /// NCOM structure-A packets are intended to be used by OxTS customers. In NCOM structure-A 
    /// packets, the navigation status (byte 21) will have a value of 0, 1, 2, 3, 4, 5, 6, 7, 10, 
    /// 20, 21, 22.
    /// </summary>
    /// <remarks>
    /// The definition of a structure-A packet is shown below
    /// </remarks>
    public class NcomPacketA : NcomPacket
    {

        /* ---------- Constants ---------------------------------------------------------------/**/

        internal const float ACCELERATION_SCALING   = 1e-4f;
        internal const float ANGULAR_RATE_SCALING   = 1e-5f;
        internal const float VELOCITY_SCALING       = 1e-4f;
        internal const float ORIENTATION_SCALING    = 1e-6f;

        internal const int CHECKSUM_1_INDEX = 22;
        internal const int CHECKSUM_2_INDEX = 61;

        internal const int STATUS_CHANNEL_LENGTH = 8;

        private const ushort MAX_TIME_VALUE = 59999;
        
        private static readonly NavigationStatus[] ALLOWED_NAV_STATUSES = new NavigationStatus[]
        {
            NavigationStatus.Invalid,                           // 0
            NavigationStatus.RawIMUMeasurements,                // 1
            NavigationStatus.Initialising,                      // 2
            NavigationStatus.Locking,                           // 3
            NavigationStatus.Locked,                            // 4
            NavigationStatus.ReservedForUnlocked,               // 5
            NavigationStatus.ExpiredFirmware,                   // 6
            NavigationStatus.BlockedFirmware,                   // 7
            NavigationStatus.StatusOnly,                        // 10
            NavigationStatus.TriggerPacketWhileInitialising,    // 20
            NavigationStatus.TriggerPacketWhileLocking,         // 21
            NavigationStatus.TriggerPacketWhileLocked           // 22
        };


        /* ---------- Private variables -------------------------------------------------------/**/

        private ushort _time = 0;

        /* ---------- Constructors ------------------------------------------------------------/**/

        public NcomPacketA() :base() { }


        /* ---------- Properties --------------------------------------------------------------/**/
        
        /// <summary>
        /// Acceleration <i>X</i> is the host object's acceleration in the <i>X</i>-direction (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// m/s^-2.
        /// </summary>
        public float AccelerationX { get; set; }

        /// <summary>
        /// Acceleration <i>Y</i> is the host object's acceleration in the <i>Y</i>-direction (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// m/s^-2.
        /// </summary>
        public float AccelerationY { get; set; }

        /// <summary>
        /// Acceleration <i>Z</i> is the host object's acceleration in the <i>Z</i>-direction (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// m/s^-2.
        /// </summary>
        public float AccelerationZ { get; set; }

        /// <summary>
        /// The altitude of the INS. Expressed in units of radians.
        /// </summary>
        public float Altitude { get; set; }

        /// <summary>
        /// Angular rate <i>X</i> is the host object's angular rate about its <i>X</i>-axis (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// radians per second
        /// </summary>
        public float AngularRateX { get; set; }

        /// <summary>
        /// Angular rate <i>Y</i> is the host object's angular rate about its <i>Y</i>-axis (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// radians per second
        /// </summary>
        public float AngularRateY { get; set; }

        /// <summary>
        /// Angular rate <i>Z</i> is the host object's angular rate about its <i>Z</i>-axis (i.e. 
        /// after the IMU to host attitude matrix has been applied). It is expressed in units of 
        /// radians per second
        /// </summary>
        public float AngularRateZ { get; set; }

        /// <summary>
        /// Checksum 1 allows the software to verify the integrity of bytes 1-21. The sync byte if 
        /// ignored. In low-latency applications the inertial measurements in Batch A can be used 
        /// to update a previous solution without waiting for the rest of the packet to be 
        /// received.
        /// </summary>
        public bool Checksum1 { get; set; }

        /// <summary>
        /// Checksum 2 allows the software to verify the integrity of bytes 1-60. The sync byte if 
        /// ignored. For medium-latency output, the full navigation solution is now available 
        /// without waiting for the status updated in the rest of the packet.
        /// </summary>
        public bool Checksum2 { get; set; }

        /// <summary>
        /// Down velocity in units m/s
        /// </summary>
        public float DownVelocity { get; set; }

        /// <summary>
        /// East velocity in units m/s
        /// </summary>
        public float EastVelocity { get; set; }

        /// <summary>
        /// Heading in units of radians. Range +-PI
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// The latitude of the INS. Expressed in units of radians.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude of the INS. Expressed in units of radians.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// North velocity in units m/s
        /// </summary>
        public float NorthVelocity { get; set; }

        /// <summary>
        /// Pitch in units of radians. Range +-PI/2
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// Roll in units of radians. Range +-PI
        /// </summary>
        public float Roll { get; set; }
        
        /// <summary>
        /// Bytes 63 to 70 of an NCOM structure-A packet are collectively called Batch S. Batch S 
        /// contains status channel information from the INS. The information transmitted in Batch 
        /// S is defined by the value of the status channel byte, which defines the structure of 
        /// each status channel and the information it contains.
        /// </summary>
        public StatusChannel StatusChannel { get; set; }

        /// <summary>
        /// Time is transmitted as milliseconds into the current GPS minute. Range = [0 - 59,999].
        /// </summary>
        public ushort Time
        {
            get { return _time; }
            set
            {
                if (value > MAX_TIME_VALUE)
                {
                    throw new ArgumentOutOfRangeException("The value for time must be between 0 and " + MAX_TIME_VALUE);
                }
                _time = value;
            }
        }


        /* ---------- Public methods ----------------------------------------------------------/**/

        /// <summary>
        /// Marshals the data into a byte array of length <see cref="PACKET_LENGTH"/>.
        /// </summary>
        /// <returns></returns>
        public override byte[] Marshal()
        {
            // Call base method to get byte array to marshal into
            byte[] buffer = base.Marshal();
            int p = 1;


            // Batch A
            // --------

            // Insert Time
            Array.Copy(BitConverter.GetBytes(Time), 0, buffer, p, 2);
            p += 2;

            // Insert acceleration X
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AccelerationX / ACCELERATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert acceleration Y
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AccelerationY / ACCELERATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert acceleration Z
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AccelerationZ / ACCELERATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Angular Rate X
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AngularRateX / ANGULAR_RATE_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Angular Rate Y
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AngularRateY / ANGULAR_RATE_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Angular Rate Z
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(AngularRateZ / ANGULAR_RATE_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Skip over nav. status
            p++;

            // Calculate and insert Checksum 1
            buffer[p] = CalculateChecksum(buffer, 1, p - 2);
            p++;


            // Batch B
            // --------

            // Insert Latitude
            Array.Copy(BitConverter.GetBytes(Latitude), 0, buffer, p, 8);
            p += 8;

            // Insert Longitude
            Array.Copy(BitConverter.GetBytes(Longitude), 0, buffer, p, 8);
            p += 8;

            // Insert Altitude
            Array.Copy(BitConverter.GetBytes(Altitude), 0, buffer, p, 8);
            p += 4;

            // Insert North Velocity
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(NorthVelocity / VELOCITY_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert East Velocity
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(EastVelocity / VELOCITY_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Down Velocity
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(DownVelocity / VELOCITY_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Heading
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(Heading / ORIENTATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Pitch
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(Pitch / ORIENTATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Insert Roll
            Array.Copy(ByteHandling.CastInt32To3Bytes((int)(Roll / ORIENTATION_SCALING)), 0, buffer, p, 3);
            p += 3;

            // Calculate and insert Checksum 2
            buffer[p] = CalculateChecksum(buffer, 1, p - 2);
            p++;


            // Batch S
            // --------

            // Insert status channel byte
            buffer[p++] = StatusChannel != null ? StatusChannel.StatusChannelByte : (byte)0xFF;

            // Insert Status Channel
            if (StatusChannel != null) Array.Copy(StatusChannel.Marshal(), 0, buffer, p, 8);
            p += 8;

            // Calculate and insert Checksum 3
            buffer[p] = CalculateChecksum(buffer, 1, p - 2);
            p++;

            // Return the marshalled NCOM packet
            return buffer;
        }

        /// <summary>
        /// Unmarshals the data stored in <paramref name="buffer"/> into this instance of an NCOM 
        /// packet. If no marshalled NCOM packet can be found, false is returned.
        /// <para>
        /// If the first byte of the buffer is not <see cref="SYNC_BYTE"/> then the method will 
        /// look for the first occurance of the sync byte.
        /// </para>
        /// </summary>
        /// <param name="buffer">The byte array containing the marshalled NCOM packet.</param>
        /// <param name="offset">
        /// The zero-based index indicating the location in the buffer to start looking for a sync 
        /// byte from.
        /// </param>
        /// <returns>False if no marshalled NCOM packet can be found, otherwise true.</returns>
        public override bool Unmarshal(byte[] buffer, int offset)
        {
            // Seek the sync byte
            while (offset <= buffer.Length - PACKET_LENGTH)
            {
                // Have we found the sync byte?
                if (buffer[offset++] == SYNC_BYTE)
                {
                    int pkt_start = offset - 1;

                    // offset points to pkt[1]

                    // Call base method
                    if (!base.Unmarshal(buffer, offset - 1)) return false;

                    // Check the navigation status byte
                    if (!ALLOWED_NAV_STATUSES.Contains(NavigationStatus)) return false;


                    // Batch A
                    // --------

                    // Extract the time
                    Time = BitConverter.ToUInt16(buffer, offset);
                    offset += 2;

                    // Extract the Acceleration X
                    AccelerationX = ByteHandling.Cast3BytesToInt32(buffer, offset) * ACCELERATION_SCALING;
                    offset += 3;

                    // Extract the Acceleration Y
                    AccelerationY = ByteHandling.Cast3BytesToInt32(buffer, offset) * ACCELERATION_SCALING;
                    offset += 3;

                    // Extract the Acceleration Z
                    AccelerationZ = ByteHandling.Cast3BytesToInt32(buffer, offset) * ACCELERATION_SCALING;
                    offset += 3;

                    // Extract the Angular Rate X
                    AngularRateX = ByteHandling.Cast3BytesToInt32(buffer, offset) * ANGULAR_RATE_SCALING;
                    offset += 3;

                    // Extract the Angular Rate Y
                    AngularRateY = ByteHandling.Cast3BytesToInt32(buffer, offset) * ANGULAR_RATE_SCALING;
                    offset += 3;

                    // Extract the Angular Rate Z
                    AngularRateZ = ByteHandling.Cast3BytesToInt32(buffer, offset) * ANGULAR_RATE_SCALING;
                    offset += 3;

                    // Skip a bit for the nav status byte
                    offset++;

                    // Extract checksum 1
                    Checksum1 = CalculateChecksum(buffer, pkt_start + 1, 21) == buffer[offset];
                    offset++;


                    // Batch B
                    // --------

                    // Extract Latitude
                    Latitude = BitConverter.ToDouble(buffer, offset);
                    offset += 8;

                    // Extract Longitude
                    Longitude = BitConverter.ToDouble(buffer, offset);
                    offset += 8;

                    // Extract altitude
                    Altitude = BitConverter.ToSingle(buffer, offset);
                    offset += 4;

                    // Extract North velocity
                    NorthVelocity = ByteHandling.Cast3BytesToInt32(buffer, offset) * VELOCITY_SCALING;
                    offset += 3;

                    // Extract East velocity
                    EastVelocity = ByteHandling.Cast3BytesToInt32(buffer, offset) * VELOCITY_SCALING;
                    offset += 3;

                    // Extract Down velocity
                    DownVelocity = ByteHandling.Cast3BytesToInt32(buffer, offset) * VELOCITY_SCALING;
                    offset += 3;

                    // Extract Heading
                    Heading = ByteHandling.Cast3BytesToInt32(buffer, offset) * ORIENTATION_SCALING;
                    offset += 3;

                    // Extract Pitch
                    Pitch = ByteHandling.Cast3BytesToInt32(buffer, offset) * ORIENTATION_SCALING;
                    offset += 3;

                    // Extract Roll
                    Roll = ByteHandling.Cast3BytesToInt32(buffer, offset) * ORIENTATION_SCALING;
                    offset += 3;

                    // Extract checksum 2
                    Checksum2 = CalculateChecksum(buffer, pkt_start + 1, 60) == buffer[offset];
                    offset++;


                    // Batch S
                    // --------

                    StatusChannel = StatusChannelFactory.ProcessStatusChannel(buffer, offset);
                    offset += 9;

                    // Unmarshalled OK, return true
                    return true;
                }
            }

            // Couldn't find packet, return false;
            return false;
        }

    }
}