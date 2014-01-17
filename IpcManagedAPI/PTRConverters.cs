using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CCC.RMSLib
{
    public class  PTRConverters
    {
        public static IntPtr StringArrayToIntPtr<GenChar>(string[] array) where GenChar : struct
        {

            //build array of pointers to string
            IntPtr[] InPointers = new IntPtr[array.Length];

            int size = IntPtr.Size * array.Length;

            IntPtr ptr = Marshal.AllocCoTaskMem(size);
            
            for (int i = 0; i < array.Length; i++)
            {

                if (typeof(GenChar) == typeof(char))
                    InPointers[i] = Marshal.StringToCoTaskMemUni(array[i]);
                
                else if (typeof(GenChar) == typeof(byte))
                    InPointers[i] = Marshal.StringToCoTaskMemAnsi(array[i]);
                
                else if (typeof(GenChar) == typeof(IntPtr))//assune BSTR for IntPtr param
                    InPointers[i] = Marshal.StringToBSTR(array[i]);
            }

            //copy the array of pointers
            Marshal.Copy(InPointers, 0, ptr, array.Length);
            return ptr;
        }

        public static string[] IntPtrToStringArray<GenChar>(int size, IntPtr ptr) where GenChar : struct
        {
            //get the output array of pointers
            IntPtr[] OutPointers = new IntPtr[size];

            Marshal.Copy(ptr, OutPointers, 0, size);

            string[] strArray = new string[size];
            for (int i = 0; i < size; i++)
            {
                if (typeof(GenChar) == typeof(char))
                    strArray[i] = Marshal.PtrToStringUni(OutPointers[i]);
                else
                    strArray[i] = Marshal.PtrToStringAnsi(OutPointers[i]);

                //dispose of unneeded memory
                Marshal.FreeCoTaskMem(OutPointers[i]);
            }

            //dispose of the pointers array
            Marshal.FreeCoTaskMem(ptr);

            return strArray;
        }   
    }
}
