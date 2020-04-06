using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP
{
    // Thread unsafe!
    public unsafe class TypeTools
    {
        #region getbytes
        static byte[] bytes2 = new byte[2];
        static byte[] bytes4 = new byte[4];
        static byte[] bytes8 = new byte[8];

        public unsafe static byte[] GetBytes(int value)
        {
            byte[] bytes = bytes4;
            fixed (byte* b = bytes)
                *((int*)b) = value;
            return bytes;
        }

        public unsafe static byte[] GetBytes(uint value)
        {
            return GetBytes((int)value);
        }
        public unsafe static byte[] GetBytes(long value)
        {
            byte[] bytes = bytes8;
            fixed (byte* b = bytes)
                *((long*)b) = value;
            return bytes;
        }
        public static byte[] GetBytes(ushort value)
        {
            return GetBytes((short)value);
        }

        public unsafe static byte[] GetBytes(short value)
        {
            byte[] bytes = bytes2;
            fixed (byte* b = bytes)
                *((short*)b) = value;
            return bytes;
        }

        public unsafe static byte[] GetBytes(float value)
        {
            return GetBytes(*(int*)&value);
        }

        public unsafe static byte[] GetBytes(double value)
        {
            return GetBytes(*(long*)&value);
        }
        #endregion


        public static uint asuint(float value)
        {
            return BitConverter.ToUInt32(GetBytes(value),0);
        }

        public static float asfloat(uint value)
        {
            return BitConverter.ToSingle(GetBytes(value), 0);
        }
        public static float asfloat(int value)
        {
            return BitConverter.ToSingle(GetBytes(value), 0);
        }



    }
}
