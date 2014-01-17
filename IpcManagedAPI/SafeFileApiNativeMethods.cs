//-----------------------------------------------------------------------------
// Created By Sghaida
// Description:  Enums and classes needed for the MSIPC Managed API.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CCC.RMSLib
{
    public static class SafeFileApiNativeMethods
    {

        public static string IpcfEncryptFile(
            string inputFile,
            string templateId,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            System.Windows.Forms.Form parentForm,
            string outputDirectory = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;

            IpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentForm);

            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(templateId);

            try
            {
                hr = UnsafeFileApiMethods.IpcfEncryptFile(
                    inputFile,
                    licenseInfoPtr,
                    (uint)EncryptLicenseInfoTypes.IPCF_EF_TEMPLATE_ID,
                    (uint)flags,
                    ipcContext,
                    outputDirectory,
                    out encryptedFileName);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFile;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
            }

            return outputFileName;
        }

        public static string IpcfEncryptFile(
            string inputFile,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            System.Windows.Forms.Form parentForm,
            string outputDirectory = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;

            IpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentForm);

            try
            {
                hr = UnsafeFileApiMethods.IpcfEncryptFile(
                    inputFile,
                    licenseHandle.Value,
                    (uint)EncryptLicenseInfoTypes.IPCF_EF_LICENSE_HANDLE,
                    (uint)flags,
                    ipcContext,
                    outputDirectory,
                    out encryptedFileName);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFile;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
            }

            return outputFileName;
        }


        public static string IpcfDecryptFile(
            string inputFile,
            DecryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            System.Windows.Forms.Form parentForm,
            string outputDirectory = null)
        {
            int hr = 0;
            IntPtr decryptedFileNamePtr = IntPtr.Zero;
            string decryptedFileName = null;

            IpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentForm);

            try
            {
                hr = UnsafeFileApiMethods.IpcfDecryptFile(
                    inputFile,
                    (uint)flags,
                    ipcContext,
                    outputDirectory,
                    out decryptedFileNamePtr);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                decryptedFileName = Marshal.PtrToStringUni(decryptedFileNamePtr);
                if (null == decryptedFileName || 0 == decryptedFileName.Length)
                {
                    decryptedFileName = inputFile;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(decryptedFileNamePtr);
            }

            return decryptedFileName;
        }

        public static byte[] IpcfGetSerializedLicenseFromFile(string inputFile)
        {
            byte[] license = null;
            int hr = 0;

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                hr = UnsafeFileApiMethods.IpcfGetSerializedLicenseFromFile(
                    inputFile,
                    out licensePtr);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                license = SafeNativeMethods.MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(licensePtr);
            }
            return license;
        }

        public static bool IpcfIsFileEncrypted(string inputFile)
        {
            uint fileStatus;
            int hr = UnsafeFileApiMethods.IpcfIsFileEncrypted(inputFile, out fileStatus);
            SafeNativeMethods.ThrowOnErrorCode(hr);

            return (FileEncryptedStatus)fileStatus != FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED;
        }

        [Flags]
        private enum FileEncryptedStatus
        {
            IPCF_FILE_STATUS_DECRYPTED                              = 0,
            IPCF_FILE_STATUS_ENCRYPTED_CUSTOM                       = 1,
            IPCF_FILE_STATUS_ENCRYPTED                              = 2
        }

        [Flags]
        public enum EncryptLicenseInfoTypes
        {
            IPCF_EF_TEMPLATE_ID                                     = 0,
            IPCF_EF_LICENSE_HANDLE                                  = 1
        }

        [Flags]
        public enum EncryptFlags
        {
            IPCF_EF_FLAG_DEFAULT                                     = 0x00000000,
            IPCF_EF_FLAG_UPDATELICENSE_BLOCKED                       = 0x00000001,
            IPCF_EF_FLAG_KEY_NO_PERSIST                              = 0x00000002,
            IPCF_EF_FLAG_KEY_NO_PERSIST_DISK                         = 0x00000004,
            IPCF_EF_FLAG_KEY_NO_PERSIST_LICENSE                      = 0x00000008
        }

        [Flags]
        public enum DecryptFlags
        {
            IPCF_DF_FLAG_DEFAULT            = 0x00000000,
            IPCF_DF_FLAG_OPEN_AS_RMS_AWARE =  0x00000001
        }
    }
}
