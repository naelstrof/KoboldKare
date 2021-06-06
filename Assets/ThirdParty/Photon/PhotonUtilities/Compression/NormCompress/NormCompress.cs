// Copyright 2019, Davin Carten, All rights reserved
// This code may only be used in game development, but may not be used in any tools or assets that are sold or made publicly available to other developers.

namespace Photon.Compression
{
    public enum NormalizedFloatCompression
    {
        Bits2 = 2, Bits3, Bits4, Bits5, Bits6, Bits7, Bits8, Bits9, Bits10, Bits12 = 12, Bits14 = 14,
        Half16 = 16, Full32 = 32
    }

    /// <summary>
    /// Very basic and lightweight compression of floats ranging from 0-1 in value. 
    /// No clamping or other checks are employed, so be sure you are passing only values
    /// in the 0-1 range.
    /// </summary>
    public static class NormCompress
    {


        public static NormCompressCodec[] codecForBit = new NormCompressCodec[33];

        static NormCompress()
        {
            for (int i = 0; i <= 32; ++i)
            {
                uint maxval = GetMaxValueForBits(i);
                codecForBit[i] = new NormCompressCodec(i, maxval, 1f / maxval);
            }
        }

        public struct NormCompressCodec
        {
            public readonly int bits;
            public readonly float encoder, decoder;

            public NormCompressCodec(int bits, float encoder, float decoder)
            {
                this.bits = bits;
                this.encoder = encoder;
                this.decoder = decoder;
            }
        }

        /// Hard coded compression settings for normalized 0-1 range floats

        private const float NORM_COMP_ENCODE15 = 32767;
        private const float NORM_COMP_DECODE15 = 1f / NORM_COMP_ENCODE15;

        private const float NORM_COMP_ENCODE14 = 16383;
        private const float NORM_COMP_DECODE14 = 1f / NORM_COMP_ENCODE14;

        private const float NORM_COMP_ENCODE13 = 8191;
        private const float NORM_COMP_DECODE13 = 1f / NORM_COMP_ENCODE13;

        private const float NORM_COMP_ENCODE12 = 4095;
        private const float NORM_COMP_DECODE12 = 1f / NORM_COMP_ENCODE12;

        private const float NORM_COMP_ENCODE11 = 2047;
        private const float NORM_COMP_DECODE11 = 1f / NORM_COMP_ENCODE11;

        private const float NORM_COMP_ENCODE10 = 1023;
        private const float NORM_COMP_DECODE10 = 1f / NORM_COMP_ENCODE10;

        private const float NORM_COMP_ENCODE9 = 511;
        private const float NORM_COMP_DECODE9 = 1f / NORM_COMP_ENCODE9;

        private const float NORM_COMP_ENCODE8 = 255;
        private const float NORM_COMP_DECODE8 = 1f / NORM_COMP_ENCODE8;

        private const float NORM_COMP_ENCODE7 = 127;
        private const float NORM_COMP_DECODE7 = 1f / NORM_COMP_ENCODE8;

        private const float NORM_COMP_ENCODE6 = 63;
        private const float NORM_COMP_DECODE6 = 1f / NORM_COMP_ENCODE6;

        private const float NORM_COMP_ENCODE5 = 31;
        private const float NORM_COMP_DECODE5 = 1f / NORM_COMP_ENCODE5;

        private const float NORM_COMP_ENCODE4 = 15;
        private const float NORM_COMP_DECODE4 = 1f / NORM_COMP_ENCODE4;

        private const float NORM_COMP_ENCODE3 = 7;
        private const float NORM_COMP_DECODE3 = 1f / NORM_COMP_ENCODE3;

        private const float NORM_COMP_ENCODE2 = 3;
        private const float NORM_COMP_DECODE2 = 1f / NORM_COMP_ENCODE2;

        private const float NORM_COMP_ENCODE1 = 1;
        private const float NORM_COMP_DECODE1 = 1;

        private const float NORM_COMP_ENCODE0 = 0;
        private const float NORM_COMP_DECODE0 = 0;

        /// <summary>
        /// Compress a Normalized float. For bit-based settings, you must supply floats in the range of 0 to 1.
        /// Values outside of that range will produce a looping modulus behaviour.
        /// </summary>
        /// <returns>Returns the compressed value as a uint, with an out bits value for convenience with bitpacking.</returns>
        public static uint CompressNorm(this float value, int bits)
        {
            value = (value > 1) ? 1 : (value < 0) ? 0 : value;

            switch (bits)
            {
                case 0:
                    return (uint)0;

                case 1:
                    return (uint)value;

                case 2:
                    return (uint)(value * NORM_COMP_ENCODE2);

                case 3:
                    return (uint)(value * NORM_COMP_ENCODE3);

                case 4:
                    return (uint)(value * NORM_COMP_ENCODE4);

                case 5:
                    return (uint)(value * NORM_COMP_ENCODE5);

                case 6:
                    return (uint)(value * NORM_COMP_ENCODE6);

                case 7:
                    return (uint)(value * NORM_COMP_ENCODE7);

                case 8:
                    return (uint)(value * NORM_COMP_ENCODE8);

                case 9:
                    return (uint)(value * NORM_COMP_ENCODE9);

                case 10:
                    return (uint)(value * NORM_COMP_ENCODE10);

                case 11:
                    return (uint)(value * NORM_COMP_ENCODE11);

                case 12:
                    return (uint)(value * NORM_COMP_ENCODE12);

                case 13:
                    return (uint)(value * NORM_COMP_ENCODE13);

                case 14:
                    return (uint)(value * NORM_COMP_ENCODE14);

                case 15:
                    return (uint)(value * NORM_COMP_ENCODE15);

                case 16:
                    return HalfFloat.HalfUtilities.Pack(value);

                default:
                    return (ByteConverter)value;
            }
        }

        /// <summary>
        /// Compress and Write a Normalized float. For bit-based settings, you must supply floats in the range of 0 to 1.
        /// Values outside of that range will produce a looping modulus behavior.
        /// </summary>
        /// <returns>Returns the compressed value as a UInt32.</returns>
        public static uint WriteNorm(this byte[] buffer, float value, ref int bitposition, int bits)
        {
            uint cval;

            value = (value > 1) ? 1 : (value < 0) ? 0 : value;

            switch (bits)
            {
                case 0:
                    cval = 0;
                    break;

                case 1:
                    cval = (uint)value;
                    break;

                case 2:
                    cval = (uint)(value * NORM_COMP_ENCODE2);
                    break;

                case 3:
                    cval = (uint)(value * NORM_COMP_ENCODE3);
                    break;

                case 4:
                    cval = (uint)(value * NORM_COMP_ENCODE4);
                    break;

                case 5:
                    cval = (uint)(value * NORM_COMP_ENCODE5);
                    break;

                case 6:
                    cval = (uint)(value * NORM_COMP_ENCODE6);
                    break;

                case 7:
                    cval = (uint)(value * NORM_COMP_ENCODE7);
                    break;

                case 8:
                    cval = (uint)(value * NORM_COMP_ENCODE8);
                    break;

                case 9:
                    cval = (uint)(value * NORM_COMP_ENCODE9);
                    break;

                case 10:
                    cval = (uint)(value * NORM_COMP_ENCODE10);
                    break;

                case 11:
                    cval = (uint)(value * NORM_COMP_ENCODE11);
                    break;

                case 12:
                    cval = (uint)(value * NORM_COMP_ENCODE12);
                    break;

                case 13:
                    cval = (uint)(value * NORM_COMP_ENCODE13);
                    break;

                case 14:
                    cval = (uint)(value * NORM_COMP_ENCODE14);
                    break;

                case 15:
                    cval = (uint)(value * NORM_COMP_ENCODE15);
                    break;

                case 16:
                    cval = HalfFloat.HalfUtilities.Pack(value);
                    break;

                default:
                    cval = (ByteConverter)value;
                    break;
            }

            buffer.Write(cval, ref bitposition, bits);
            return cval;
        }

        /// <summary>
        /// Read out a serialized compressed float.
        /// </summary>
        public static float ReadNorm(this byte[] buffer, ref int bitposition, int bits)
        {
            switch (bits)
            {
                case 0:
                    return 0;

                case 1:
                    return buffer.Read(ref bitposition, 1) * NORM_COMP_DECODE1;

                case 2:
                    return buffer.Read(ref bitposition, 2) * NORM_COMP_DECODE2;

                case 3:
                    return buffer.Read(ref bitposition, 3) * NORM_COMP_DECODE3;

                case 4:
                    return buffer.Read(ref bitposition, 4) * NORM_COMP_DECODE4;

                case 5:
                    return buffer.Read(ref bitposition, 5) * NORM_COMP_DECODE5;

                case 6:
                    return buffer.Read(ref bitposition, 6) * NORM_COMP_DECODE6;

                case 7:
                    return buffer.Read(ref bitposition, 7) * NORM_COMP_DECODE7;

                case 8:
                    return buffer.Read(ref bitposition, 8) * NORM_COMP_DECODE8;

                case 9:
                    return buffer.Read(ref bitposition, 9) * NORM_COMP_DECODE9;

                case 10:
                    return buffer.Read(ref bitposition, 10) * NORM_COMP_DECODE10;

                case 11:
                    return buffer.Read(ref bitposition, 11) * NORM_COMP_DECODE11;

                case 12:
                    return buffer.Read(ref bitposition, 12) * NORM_COMP_DECODE12;

                case 13:
                    return buffer.Read(ref bitposition, 13) * NORM_COMP_DECODE13;

                case 14:
                    return buffer.Read(ref bitposition, 14) * NORM_COMP_DECODE14;

                case 15:
                    return buffer.Read(ref bitposition, 15) * NORM_COMP_DECODE15;

                case 16:
                    return buffer.ReadHalf(ref bitposition);

                default:
                    return buffer.ReadFloat(ref bitposition);
            }
        }

        public static uint GetMaxValueForBits(int bitcount)
        {
            return (uint)(((ulong)1 << bitcount) - 1);
        }

    }
}
