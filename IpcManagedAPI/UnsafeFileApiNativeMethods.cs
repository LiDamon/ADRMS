using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CCC.RMSLib
{
    internal static class UnsafeFileApiMethods
    {
        private const string fileAPIDLLName = "msipc.dll";
        public static string FileAPIDLLName
        {
            get { return fileAPIDLLName; }
        }

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfEncryptFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In] IntPtr pvLicenseInfo,
            [In, MarshalAs(UnmanagedType.U4)] uint dwType,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszOutputFileDirectory,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfDecryptFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszOutputFileDirectory,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfGetSerializedLicenseFromFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out IntPtr pvLicense);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfIsFileEncrypted(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out uint dwFileStatus);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcFreeMemory(
            [In] IntPtr handle);
    }
}
