using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
namespace DoomSRP
{

    public class Utils
    {
        public static int StringFind(string self,/*[in]*/  string src,
             /*[in]*/  int startIndex,
             /*[in]*/ bool bCaseSensitive = true)
        {
            //===================================================
            // Do basic checking for NULL strings
            //===================================================
            if (self.Length == 0)
                return -1;

            if (src.Length == 0)
                return -1;

            //===================================================
            // If case senistive == false, 
            // we make it upper case before comparing
            //===================================================
            char startChar = src[0];
            if (bCaseSensitive == false)
                startChar = startChar.ToString().ToUpper()[0];// toupper(startChar);

            //===================================================
            // Start comparing character by character
            //===================================================
            int i = startIndex;

            while (i < self.Length)
            {
                char thisChar = self[i];
                if (bCaseSensitive == false)
                    thisChar = thisChar.ToString().ToUpper()[0];

                if (thisChar == startChar)
                {
                    int j = 1;

                    char srcChar = '\0';
                    if (src.Length > 1)
                    {
                        srcChar = src[j];
                    }
                    if (bCaseSensitive == false)
                        srcChar = srcChar.ToString().ToUpper()[0];

                    while (srcChar != (char)0)
                    {
                        thisChar = self[i + j];
                        if (bCaseSensitive == false)
                            thisChar = thisChar.ToString().ToUpper()[0];

                        if (thisChar != srcChar)
                            break;

                        j++;

                        srcChar = src[j];
                        if (bCaseSensitive == false)
                            srcChar = srcChar.ToString().ToUpper()[0];
                    } // End while

                    if (srcChar == (char)0)
                        return i;
                } // End if

                i++;
            } // End while

            return -1;
        } // End of Find

        public static float InvApproxExp2(float f) { return Mathf.Log10(f) / Mathf.Log10(2); }


        public static bool FloatEqual(float a, float b)
        {
            if (Mathf.Abs(a - b) < 0.0001f)
            {
                return true;
            }
            return false;
        }
    }
    public class StructHelper
    {
        public static T AllocMemForStructure<T>() where T : struct
        {
            var handle = GCHandle.Alloc(new byte[(int)Marshal.SizeOf<T>()], GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
        public static T ReadStructFromFile<T>(FileStream fp) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            fp.Read(buffer, 0, size);
            return ByteArrayToStructure<T>(buffer);
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
        public static void ByteArrayToStructArray<T>(byte[] bytes, T[] structs) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] subbytes = new byte[size];
            int structCount = structs.Length;
            int bytesLength = bytes.Length;

            int minCount = bytesLength / size;
            if (minCount > structCount)
            {
                minCount = structCount;
            }
            for (int i = 0; i < minCount; ++i)
            {
                int offset = i * size;
                for (int j = 0; j < size; ++j)
                {
                    subbytes[j] = bytes[offset + j];
                }
                structs[i] = ByteArrayToStructure<T>(subbytes);
            }
        }
    }

}
