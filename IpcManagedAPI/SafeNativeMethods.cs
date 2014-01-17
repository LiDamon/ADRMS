//-----------------------------------------------------------------------------
// Created By Sghaida
// Description:  Wrappers for the private pinvoke calls declared in UnsafeNativeMethods.cs
//-----------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;

namespace CCC.RMSLib
{
    public static class SafeNativeMethods
    {
        // Configures the dll directory to include msipc.dll path. This function must be called before any other MSIPC function.
        public static void IpcInitialize()
        {
            const string MSIPC_CURRENT_VERSION_KEY = "SOFTWARE\\Microsoft\\MSIPC\\CurrentVersion";
            const string INSTALL_LOCATION_VALUE = "InstallLocation";

            RegistryKey key = Registry.LocalMachine.OpenSubKey(MSIPC_CURRENT_VERSION_KEY);
            if (null == key)
            {
                throw new Exception(MSIPC_CURRENT_VERSION_KEY + " not found");
            }

            string installLocation = (string)key.GetValue(INSTALL_LOCATION_VALUE);
            if (String.IsNullOrWhiteSpace(installLocation))
            {
                throw new Exception(INSTALL_LOCATION_VALUE + " not found");
            }

            bool configSuccessful = UnsafeNativeMethods.SetDllDirectory(installLocation);
            if (false == configSuccessful)
            {
                throw new Exception("SetDllDirectory failed with " + Marshal.GetLastWin32Error());
            }
            else
            {
                //Call a quick MSIPC method to load all the MSIPC.dll function pointers
                IpcGetAPIMode();
            }
        }

        // Environment Properties - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535247(v=vs.85).aspx

        // IpcGetGlobalProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535262(v=vs.85).aspx
        public static APIMode IpcGetAPIMode()
        {
            APIMode securityMode = APIMode.Client;
            int hr = 0;

            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetGlobalProperty(Convert.ToUInt32(EnvironmentInformationType.SecurityMode), out propertyPtr);
                ThrowOnErrorCode(hr);

                int temp = Marshal.ReadInt32(propertyPtr);
                securityMode = (APIMode)Enum.ToObject(typeof(APIMode), temp);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(propertyPtr);
            }

            return securityMode;
        }

        // IpcSetGlobalProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535270(v=vs.85).aspx
        public static void IpcSetAPIMode(APIMode securityMode)
        {
            int hr = 0;

            IntPtr propertyPtr = Marshal.AllocHGlobal(sizeof(uint));
            try
            {
                Marshal.WriteInt32(propertyPtr, (int)securityMode);

                hr = UnsafeNativeMethods.IpcSetGlobalProperty(Convert.ToUInt32(EnvironmentInformationType.SecurityMode), propertyPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(propertyPtr);
            }
        }

        // IpcGetTemplateList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535267(v=vs.85).aspx
        public static Collection<TemplateInfo> IpcGetTemplateList(
                            ConnectionInfo connectionInfo,
                            bool forceDownload,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            System.Windows.Forms.Form parentForm,
                            CultureInfo cultureInfo)
        {
            Collection<TemplateInfo> templateList = null;
            int hr = 0;

            uint flags = 0;
            if (forceDownload)
            {
                flags |= Convert.ToUInt32(GetTemplateListFlags.ForceDownload);
            }

            uint lcid = 0;
            if (null != cultureInfo)
            {
                lcid = (uint)(cultureInfo.LCID);
            }

            IpcConnectionInfo ipcConnectionInfo = ConnectionInfoToIpcConnectionInfo(connectionInfo);

            IpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentForm);

            IntPtr ipcTilPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetTemplateList(
                                ipcConnectionInfo,
                                flags,
                                lcid,
                                ipcPromptContext,
                                IntPtr.Zero,
                                out ipcTilPtr);
                ThrowOnErrorCode(hr);

                templateList = new Collection<TemplateInfo>();

                MarshalIpcTilToManaged(ipcTilPtr, templateList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(ipcTilPtr);
            }

            return templateList;

        }

        // IpcGetTemplateIssuerList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535266(v=vs.85).aspx
        public static Collection<TemplateIssuer> IpcGetTemplateIssuerList(
                            ConnectionInfo connectionInfo,
                            bool defaultServerOnly,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            System.Windows.Forms.Form parentForm)
        {
            Collection<TemplateIssuer> templateIssuerList = null;
            int hr = 0;

            uint flags = 0;
            if (defaultServerOnly)
            {
                flags |= Convert.ToUInt32(GetTemplateIssuerListFlags.DefaultServerOnly);
            }

            IpcConnectionInfo ipcConnectionInfo = ConnectionInfoToIpcConnectionInfo(connectionInfo);

            IpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentForm);
            
            IntPtr ipcTemplateIssuerListPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetTemplateIssuerList(
                                ipcConnectionInfo,
                                flags,
                                ipcPromptContext,
                                IntPtr.Zero,
                                out ipcTemplateIssuerListPtr);
                ThrowOnErrorCode(hr);

                templateIssuerList = new Collection<TemplateIssuer>();

                MarshalIpcTemplateIssuerListToManaged(ipcTemplateIssuerListPtr, templateIssuerList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(ipcTemplateIssuerListPtr);
            }

            return templateIssuerList;
        }

        // IpcCreateLicenseFromTemplateId() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535257(v=vs.85).aspx
        public static SafeInformationProtectionLicenseHandle IpcCreateLicenseFromTemplateId(string templateId)
        {
            SafeInformationProtectionLicenseHandle licenseHandle = null;
            int hr = 0;

            hr = UnsafeNativeMethods.IpcCreateLicenseFromTemplateID(templateId,
                                                                    0,
                                                                    IntPtr.Zero,
                                                                    out licenseHandle);
            ThrowOnErrorCode(hr);

            return licenseHandle;
        }

        // IpcCreateLicenseFromScratch() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535256(v=vs.85).aspx
        public static SafeInformationProtectionLicenseHandle IpcCreateLicenseFromScratch(TemplateIssuer templateIssuer)
        {
            SafeInformationProtectionLicenseHandle licenseHandle = null;
            int hr = 0;

            IpcTemplateIssuer ipcTemplateIssuer = TemplateIssuerToIpcTemplateIssuer(templateIssuer);
            
            hr = UnsafeNativeMethods.IpcCreateLicenseFromScratch(ipcTemplateIssuer, 
                                                                 0,
                                                                 IntPtr.Zero,
                                                                 out licenseHandle);
            ThrowOnErrorCode(hr);

            return licenseHandle;
        }

        // IpcSerializeLicense() using Template Id - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                string templateId,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                System.Windows.Forms.Form parentForm,
                                out SafeInformationProtectionKeyHandle keyHandle)
        {
            byte[] license = null;
            int hr = 0;

            IpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentForm);

            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(templateId);

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcSerializeLicense(
                                        licenseInfoPtr,
                                        SerializationInputType.TemplateId,
                                        (uint)flags,
                                        ipcPromptContext,
                                        out keyHandle,
                                        out licensePtr);

                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
                UnsafeNativeMethods.IpcFreeMemory(licensePtr);
            }

            return license;
        }

        // IpcSerializeLicense() using License Handle - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                SafeInformationProtectionLicenseHandle licenseHandle,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                System.Windows.Forms.Form parentForm,
                                out SafeInformationProtectionKeyHandle keyHandle)
        {
            byte[] license = null;
            int hr = 0;

            IpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentForm);

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcSerializeLicense(
                                        licenseHandle.Value,
                                        SerializationInputType.License,
                                        (uint)flags,
                                        ipcPromptContext,
                                        out keyHandle,
                                        out licensePtr);

                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licensePtr);
            }

            return license;
        }


        // License Properties - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        
        // IpcSetLicenseProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535271(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseValidityTime(SafeInformationProtectionLicenseHandle licenseHandle, Term validityTime)
        {
            int hr = 0;

            IpcTerm ipcValidityTime = TermToIpcTerm(validityTime);

            IntPtr licenseInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcTerm)));
            try
            {
                Marshal.StructureToPtr(ipcValidityTime, licenseInfoPtr, false);
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                false,
                                (uint)LicensePropertyType.ValidityTime,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
            }
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseIntervalTime(SafeInformationProtectionLicenseHandle licenseHandle, uint intervalTime)
        {
            int hr = 0;

            IntPtr LicenseInfoPtr = Marshal.AllocHGlobal(sizeof(uint));
            try
            {
                Marshal.WriteInt32(LicenseInfoPtr, (int)intervalTime);

                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.IntervalTime,
                            LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(LicenseInfoPtr);
            }
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseDescriptor(SafeInformationProtectionLicenseHandle licenseHandle, TemplateInfo templateInfo)
        {
            int hr = 0;
            IntPtr LicenseInfoPtr = IntPtr.Zero;

            if (null == templateInfo)
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    true,
                                    (uint)LicensePropertyType.Descriptor,
                                    LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                LicenseInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcTemplateInfo)));
                try
                {
                    IpcTemplateInfo ipcTemplateInfo = TemplateInfoToIpcTemplateInfo(templateInfo);

                    Marshal.StructureToPtr(ipcTemplateInfo, LicenseInfoPtr, false);
                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    false,
                                    (uint)LicensePropertyType.Descriptor,
                                    LicenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    Marshal.FreeHGlobal(LicenseInfoPtr);
                }
            }
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseOwner(SafeInformationProtectionLicenseHandle licenseHandle, string owner)
        {
            int hr = 0;
            IntPtr pvLicenseInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcUser)));
            try
            {
                // Create a userinfo object
                IpcUser uInfo = new IpcUser();
                uInfo.userID = owner;
                uInfo.userType = UserIdType.Email;

                Marshal.StructureToPtr(uInfo, pvLicenseInfo, false);
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                false,
                                (uint)LicensePropertyType.Owner,
                                pvLicenseInfo);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(pvLicenseInfo);
            }
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseUserRightsList(SafeInformationProtectionLicenseHandle licenseHandle, Collection<UserRights> userRightsList)
        {
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;

            if (0 == userRightsList.Count)
            {
                // If there are no user entries, we can just delete the entire list
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                true,
                                (uint)LicensePropertyType.UserRightsList,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                // the buffers allocated in the unmanaged memory when constructing the IpcUserRightsList structure
                Collection<IntPtr> allocatedBuffers = new Collection<IntPtr>();

                try
                {
                    licenseInfoPtr = MarshalUserRightsListToNative(userRightsList, allocatedBuffers);

                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                     licenseHandle,
                                     false,
                                     (uint)LicensePropertyType.UserRightsList,
                                     licenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    foreach (IntPtr buffer in allocatedBuffers)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }
        }

        // IPC_LI_APP_SPECIFIC_DATA - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseAppSpecificData(SafeInformationProtectionLicenseHandle licenseHandle, NameValueCollection applicationSpecificData)
        {
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;

            if (applicationSpecificData.Count == 0)
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                true,
                                (uint)LicensePropertyType.AppSpecificData,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                licenseInfoPtr = MarshalNameValueListToNative(applicationSpecificData);
                try
                {
                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    false,
                                    (uint)LicensePropertyType.AppSpecificData,
                                    licenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    Marshal.FreeHGlobal(licenseInfoPtr);
                }
            }
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseReferralInfoUrl(SafeInformationProtectionLicenseHandle licenseHandle, string referralInfoUrl)
        {
            int hr = 0;

            IntPtr LicenseInfoPtr = Marshal.StringToHGlobalUni(referralInfoUrl);
            try
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.ReferralInfoUrl,
                            LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(LicenseInfoPtr);
            }
        }

        // IPC_LI_CONTENT_KEY - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseContentKey(SafeInformationProtectionLicenseHandle licenseHandle, SafeInformationProtectionKeyHandle hKey)
        {
            int hr = 0;
            hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.ContentKey,
                            hKey.Value);
            ThrowOnErrorCode(hr);
        }


        // IpcGetLicenseProperty - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535265(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Term IpcGetLicenseValidityTime(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            IpcTerm ipcValidityTime = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ValidityTime,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcValidityTime = (IpcTerm)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTerm));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcTermToTerm(ipcValidityTime);
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static uint IpcGetLicenseIntervalTime(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            uint intervalTime = 0;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.IntervalTime,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                intervalTime = (uint)Marshal.ReadInt32(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return intervalTime;
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetLicenseOwner(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            string owner = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.Owner,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                IpcUser userInfo = (IpcUser)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcUser));
                owner = userInfo.userID;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return owner;
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Collection<UserRights> IpcGetLicenseUserRightsList(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            Collection<UserRights> userRightsList = new Collection<UserRights>();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.UserRightsList,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalUserRightsListToManaged(licenseInfoPtr, userRightsList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return userRightsList;
        }

        // IPC_LI_APP_SPECIFIC_DATA - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static NameValueCollection IpcGetLicenseAppSpecificData(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            NameValueCollection applicationSpecificData = new NameValueCollection();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.AppSpecificData,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalNameValueListToManaged(licenseInfoPtr, applicationSpecificData);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return applicationSpecificData;
        }

        // IPC_LI_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static ConnectionInfo IpcGetLicenseConnectionInfo(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ConnectionInfo,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcConnectionInfo = (IpcConnectionInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcConnectionInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcConnectionInfoToConnectionInfo(ipcConnectionInfo);
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static TemplateInfo IpcGetLicenseDescriptor(SafeInformationProtectionLicenseHandle licenseHandle, CultureInfo cultureInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                uint lcid = 0;
                if (null != cultureInfo)
                {
                    lcid = (uint)(cultureInfo.LCID);
                }

                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.Descriptor,
                                lcid,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTemplateInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcTemplateInfoToTemplateInfo(ipcTemplateInfo);
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetLicenseReferralInfoUrl(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            string referralInfoUrl = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                referralInfoUrl = Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return referralInfoUrl;
        }

        // IPC_LI_CONTENT_KEY - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static SafeInformationProtectionKeyHandle IpcGetLicenseContentKey(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            SafeInformationProtectionKeyHandle keyHandle = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                keyHandle = new SafeInformationProtectionKeyHandle(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return keyHandle;
        }

        // IpcGetSerializedLicenseProperty - http://msdn.microsoft.com/en-us/library/windows/desktop/hh995038(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Term IpcGetSerializedLicenseValidityTime(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            IpcTerm ipcValidityTime = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                ipcBuffer,
                                (uint)LicensePropertyType.ValidityTime,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcValidityTime = (IpcTerm)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTerm));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return IpcTermToTerm(ipcValidityTime);
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static uint IpcGetSerializedLicenseIntervalTime(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            uint intervalTime = 0;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                ipcBuffer,
                                (uint)LicensePropertyType.IntervalTime,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                intervalTime = (uint)Marshal.ReadInt32(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return intervalTime;
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetSerializedLicenseOwner(byte[] license)
        {
            string owner = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicensePropertyWithoutKey(
                                ipcBuffer,
                                (uint)LicensePropertyType.Owner,
                                IntPtr.Zero,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                IpcUser userInfo = (IpcUser)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcUser));
                owner = userInfo.userID;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return owner;
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Collection<UserRights> IpcGetSerializedLicenseUserRightsList(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            Collection<UserRights> userRightsList = new Collection<UserRights>();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                ipcBuffer,
                                (uint)LicensePropertyType.UserRightsList,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalUserRightsListToManaged(licenseInfoPtr, userRightsList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return userRightsList;
        }

        // IPC_LI_APP_SPECIFIC_DATA - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static NameValueCollection IpcGetSerializedLicenseAppSpecificData(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            return IpcGetSerializedLicenseAppSpecificData(license, keyHandle, LicensePropertyType.AppSpecificData);
        }

        public static NameValueCollection IpcGetSerializedLicenseAppSpecificDataNoEncryption(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            return IpcGetSerializedLicenseAppSpecificData(license, keyHandle, LicensePropertyType.AppSpecificDataNoEncryption);
        }

        private static NameValueCollection IpcGetSerializedLicenseAppSpecificData(byte[] license, SafeInformationProtectionKeyHandle keyHandle, LicensePropertyType type)
        {
            NameValueCollection applicationSpecificData = new NameValueCollection();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                ipcBuffer,
                                (uint)type,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalNameValueListToManaged(licenseInfoPtr, applicationSpecificData);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return applicationSpecificData;
        }

        // IPC_LI_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static ConnectionInfo IpcGetSerializedLicenseConnectionInfo(byte[] license)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicensePropertyWithoutKey(
                                ipcBuffer,
                                (uint)LicensePropertyType.ConnectionInfo,
                                IntPtr.Zero,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcConnectionInfo = (IpcConnectionInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcConnectionInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return IpcConnectionInfoToConnectionInfo(ipcConnectionInfo);
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static TemplateInfo IpcGetSerializedLicenseDescriptor(byte[] license,
                                                                       SafeInformationProtectionKeyHandle keyHandle,
                                                                       CultureInfo cultureInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                uint lcid = 0;
                if (null != cultureInfo)
                {
                    lcid = (uint)(cultureInfo.LCID);
                }

                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                ipcBuffer,
                                (uint)LicensePropertyType.Descriptor,
                                keyHandle,
                                lcid,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTemplateInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return IpcTemplateInfoToTemplateInfo(ipcTemplateInfo);
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetSerializedLicenseReferralInfoUrl(byte[] license)
        {
            string referralInfoUrl = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicensePropertyWithoutKey(
                                ipcBuffer,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                IntPtr.Zero,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                referralInfoUrl = Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return referralInfoUrl;
        }

        // IpcGetKey() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535263(v=vs.85).aspx
        public static SafeInformationProtectionKeyHandle IpcGetKey(
                                        byte[] license,
                                        bool suppressUI, 
                                        bool offline,
                                        bool hasUserConsent,
                                        System.Windows.Forms.Form parentForm)
        {
            SafeInformationProtectionKeyHandle keyHandle = null;
            int hr = 0;

            IpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentForm);

            IpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetKey(
                            ipcBuffer,
                            0,
                            ipcPromptContext,
                            IntPtr.Zero,
                            out keyHandle);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
            }

            return keyHandle;
        }

        // IpcAccessCheck() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535253(v=vs.85).aspx
        public static bool IpcAccessCheck(SafeInformationProtectionKeyHandle keyHandle, string right)
        {
            bool accessGranted = false;
            int hr = UnsafeNativeMethods.IpcAccessCheck(
                                keyHandle,
                                right,
                                out accessGranted);
            ThrowOnErrorCode(hr);
            return accessGranted;
        }

        // IpcEncrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535259(v=vs.85).aspx
        public static void IpcEncrypt(SafeInformationProtectionKeyHandle keyHandle,
                                    UInt32 blockNumber,
                                    bool final,
                                    ref byte[] data)
        {
            int hr = 0;
            
            uint inputDataSize = (uint)data.Length;
            uint encryptedDataSize = 0;

            hr = UnsafeNativeMethods.IpcEncrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                null,
                                0,
                                out encryptedDataSize);
            ThrowOnErrorCode(hr);

            if (encryptedDataSize > inputDataSize)
            {
                Array.Resize(ref data, (int)encryptedDataSize);
            }

            hr = UnsafeNativeMethods.IpcEncrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                data,
                                encryptedDataSize,
                                out encryptedDataSize);
            ThrowOnErrorCode(hr);

            Array.Resize(ref data, (int)encryptedDataSize);
        }

        // IpcDecrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535258(v=vs.85).aspx
        public static void IpcDecrypt(SafeInformationProtectionKeyHandle keyHandle,
                                    UInt32 blockNumber,
                                    bool final,
                                    ref byte[] data)
        {
            int hr = 0;

            uint inputDataSize = (uint)data.Length;
            uint decryptedDataSize = 0;

            hr = UnsafeNativeMethods.IpcDecrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                null,
                                0,
                                out decryptedDataSize);
            ThrowOnErrorCode(hr);

            if (decryptedDataSize > inputDataSize)
            {
                Array.Resize(ref data, (int)decryptedDataSize);
            }

            hr = UnsafeNativeMethods.IpcDecrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                data,
                                decryptedDataSize,
                                out decryptedDataSize);
            ThrowOnErrorCode(hr);

            Array.Resize(ref data, (int)decryptedDataSize);
        }

        // IpcDecrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535258(v=vs.85).aspx
        public static byte[] IpcDecrypt(SafeInformationProtectionKeyHandle keyHandle,
                                        UInt32 blockNumber,
                                        bool final,
                                        byte[] data)
        {
            int hr = 0;

            uint inputDataSize = (uint)data.Length;
            uint decryptedDataSize = 0;

            if (0 != data.Length)
            {
                hr = UnsafeNativeMethods.IpcDecrypt(
                                    keyHandle,
                                    blockNumber,
                                    final,
                                    data,
                                    inputDataSize,
                                    null,
                                    0,
                                    out decryptedDataSize);
                ThrowOnErrorCode(hr);
            }

            byte[] decryptedData = new byte[decryptedDataSize];
            if (0 < decryptedDataSize)
            {
                hr = UnsafeNativeMethods.IpcDecrypt(
                                    keyHandle,
                                    blockNumber,
                                    final,
                                    data,
                                    inputDataSize,
                                    decryptedData,
                                    decryptedDataSize,
                                    out decryptedDataSize);
                ThrowOnErrorCode(hr);
            }
            Array.Resize(ref decryptedData, (int)decryptedDataSize);
            return decryptedData;
        }
        
        // IpcGetKeyProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx

        // IPC_KI_BLOCK_SIZE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static int IpcGetKeyBlockSize(SafeInformationProtectionKeyHandle keyHandle)
        {
            int blockSize = 0;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.BlockSize,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                blockSize = Marshal.ReadInt32(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return blockSize;
        }

        // IPC_KI_LICENSE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static byte[] IpcGetKeyLicense(SafeInformationProtectionKeyHandle keyHandle)
        {
            byte[] license = null;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.License,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return license;
        }

        // IPC_KI_USER_DISPLAYNAME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static string IpcGetKeyUserDisplayName(SafeInformationProtectionKeyHandle keyHandle)
        {
            string userDisplayName = null;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.UserDisplayName,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                userDisplayName = Marshal.PtrToStringUni(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return userDisplayName;
        }

        // IpcProtectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535268(v=vs.85).aspx
        public static void IpcProtectWindow(System.Windows.Forms.Form window)
        {
            int hr = UnsafeNativeMethods.IpcProtectWindow(window.Handle);

            ThrowOnErrorCode(hr);
        }

        // IpcUnprotectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535272(v=vs.85).aspx
        public static void IpcUnprotectWindow(System.Windows.Forms.Form window)
        {
            int hr = UnsafeNativeMethods.IpcUnprotectWindow(window.Handle);

            ThrowOnErrorCode(hr);
        }

        // IpcCloseHandle() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535254(v=vs.85).aspx
        public static int IpcCloseHandle(IntPtr handle)
        {
            return UnsafeNativeMethods.IpcCloseHandle(handle);
        }

        // Private Helpers

        public static IpcPromptContext CreateIpcPromptContext(bool suppressUI, bool offline, bool hasUserConsent, System.Windows.Forms.Form parentForm)
        {
            IpcPromptContext ipcPromptContext = new IpcPromptContext();
            ipcPromptContext.flags = 0;
            if (suppressUI)
            {
                ipcPromptContext.flags |= (uint)PromptContextFlag.Slient;
            }

            if (offline)
            {
                ipcPromptContext.flags |= (uint)PromptContextFlag.Offline;
            }

            if (hasUserConsent)
            {
                ipcPromptContext.flags |= (uint)PromptContextFlag.HasUserConsent;
            }

            if (null != parentForm)
            {
                ipcPromptContext.hWndParent = parentForm.Handle;
            }
            else 
            {
                ipcPromptContext.hWndParent = IntPtr.Zero;
            }

            // We don't support these
            ipcPromptContext.hCancelEvent = IntPtr.Zero;
            ipcPromptContext.pcCredential = IntPtr.Zero;

            return ipcPromptContext;
        }

        private static ConnectionInfo IpcConnectionInfoToConnectionInfo(IpcConnectionInfo ipcConnectionInfo)
        {
            ConnectionInfo connectionInfo = null;
            if (ipcConnectionInfo == null)
            {
                connectionInfo = null;
            }
            else if (String.IsNullOrEmpty(ipcConnectionInfo.ExtranetUrl) && String.IsNullOrEmpty(ipcConnectionInfo.IntranetUrl))
            {
                connectionInfo = null;
            }
            else
            {
                Uri extranetUrl = null;
                if (!String.IsNullOrEmpty(ipcConnectionInfo.ExtranetUrl))
                    extranetUrl = new Uri(ipcConnectionInfo.ExtranetUrl);
                Uri intranetUrl = null;
                if (!String.IsNullOrEmpty(ipcConnectionInfo.IntranetUrl))
                    intranetUrl = new Uri(ipcConnectionInfo.IntranetUrl);
                connectionInfo = new ConnectionInfo(extranetUrl, intranetUrl);
            }
            return connectionInfo;
        }

        private static IpcConnectionInfo ConnectionInfoToIpcConnectionInfo(ConnectionInfo connectionInfo)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            if (ipcConnectionInfo != null)
            {
                ipcConnectionInfo = new IpcConnectionInfo();
                if (connectionInfo.IntranetUrl != null)
                    ipcConnectionInfo.IntranetUrl = connectionInfo.IntranetUrl.ToString();
                if (connectionInfo.ExtranetUrl != null)
                    ipcConnectionInfo.ExtranetUrl = connectionInfo.ExtranetUrl.ToString();
            }
            return ipcConnectionInfo;
        }

        private static IpcTemplateInfo TemplateInfoToIpcTemplateInfo(TemplateInfo templateInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = new IpcTemplateInfo();
            
            ipcTemplateInfo.templateID = templateInfo.TemplateId;
            ipcTemplateInfo.lcid = (uint)templateInfo.CultureInfo.LCID;
            ipcTemplateInfo.templateName = templateInfo.Name;
            ipcTemplateInfo.templateDescription = templateInfo.Description;
            ipcTemplateInfo.issuerDisplayName = templateInfo.IssuerDisplayName;
            ipcTemplateInfo.fromTemplate = templateInfo.FromTemplate;

            return ipcTemplateInfo;
        }

        private static TemplateInfo IpcTemplateInfoToTemplateInfo(IpcTemplateInfo ipcTemplateInfo)
        {
            return new TemplateInfo(
                            ipcTemplateInfo.templateID,
                            CultureInfo.GetCultureInfo((int)ipcTemplateInfo.lcid),
                            ipcTemplateInfo.templateName,
                            ipcTemplateInfo.templateDescription,
                            ipcTemplateInfo.issuerDisplayName,
                            ipcTemplateInfo.fromTemplate);
        }

        private static TemplateIssuer IpcTemplateIssuerToTemplateIssuer(IpcTemplateIssuer ipcTemplateIssuer)
        {
            ConnectionInfo issuerConnectionInfo = IpcConnectionInfoToConnectionInfo(ipcTemplateIssuer.connectionInfo);

            return new TemplateIssuer(
                                issuerConnectionInfo,
                                ipcTemplateIssuer.wszDisplayName,
                                ipcTemplateIssuer.fAllowFromScratch);
        }

        private static IpcTemplateIssuer TemplateIssuerToIpcTemplateIssuer(TemplateIssuer templateIssuer)
        {
            IpcTemplateIssuer ipcTemplateIssuer = new IpcTemplateIssuer();
            ipcTemplateIssuer.connectionInfo = ConnectionInfoToIpcConnectionInfo(templateIssuer.ConnectionInfo);
            ipcTemplateIssuer.wszDisplayName = templateIssuer.DisplayName;
            ipcTemplateIssuer.fAllowFromScratch = templateIssuer.AllowFromScratch;
            return ipcTemplateIssuer;
        }

        private static IpcTerm TermToIpcTerm(Term term)
        {
            IpcTerm ipcTerm = new IpcTerm();
            ipcTerm.ftStart = new FileTime((long)term.From.ToFileTime());
            ipcTerm.dwDuration = (ulong)term.Duration.Ticks;

            return ipcTerm;
        }

        private static Term IpcTermToTerm(IpcTerm ipcTerm)
        {
            Term term = new Term();
            term.From = DateTime.FromFileTime(ipcTerm.ftStart);
            term.Duration = new TimeSpan((long)ipcTerm.dwDuration);

            return term;
        }

        // Manually marshals a IPC_TIL structure in unmanaged memory into a Collection<TemplateInfo>.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535283(v=vs.85).aspx
        private static void MarshalIpcTilToManaged(IntPtr ipcTilPtr, Collection<TemplateInfo> templateList)
        {
            // the number of templates goes first
            int templateCount = Marshal.ReadInt32(ipcTilPtr);

            // go to the first template
            IntPtr currentPtr = ipcTilPtr + Marshal.SizeOf(typeof(IntPtr));

            for (int i = 0; i < templateCount; i++)
            {
                IpcTemplateInfo ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(currentPtr, typeof(IpcTemplateInfo));

                templateList.Add(IpcTemplateInfoToTemplateInfo(ipcTemplateInfo));

                // go to the next template
                currentPtr = currentPtr + Marshal.SizeOf(ipcTemplateInfo);
            }
        }

        // Manually marshals a IPC_TEMPLATE_ISSUER_LIST structure in unmanaged memory into a Collection<TemplateIssuer>.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535281(v=vs.85).aspx
        private static void MarshalIpcTemplateIssuerListToManaged(IntPtr ipcTemplateIssuerListPtr, Collection<TemplateIssuer> templateIssuerList)
        {
            // the number of template issuers goes first
            int templateIssuerCount = Marshal.ReadInt32(ipcTemplateIssuerListPtr);

            // go to the first template issuer
            IntPtr currentPtr = ipcTemplateIssuerListPtr + Marshal.SizeOf(typeof(IntPtr));

            for (int i = 0; i < templateIssuerCount; i++)
            {
                IpcTemplateIssuer ipcTemplateIssuer = (IpcTemplateIssuer)Marshal.PtrToStructure(currentPtr, typeof(IpcTemplateIssuer));

                templateIssuerList.Add(IpcTemplateIssuerToTemplateIssuer(ipcTemplateIssuer));

                // go to the next template issuer
                currentPtr = currentPtr + Marshal.SizeOf(ipcTemplateIssuer);
            }
        }

        // Manually marshals a Collection<UserRights> into a IPC_USER_RIGHTS_LIST structure in unmanaged memory.
        //  See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
        // Upon return, the allocatedBuffers contains all the allocated native buffers. We use this to free the
        // native memory used by the right strings after the native call has been made.
        private static IntPtr MarshalUserRightsListToNative(Collection<UserRights> userRightsList, Collection<IntPtr> allocatedBuffers)
        {
            // allocate memory for the IPC_USER_RIGHTS_LIST variable size array
            IntPtr ipcUserRightsListPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcUserRightsList_Header))
                                                            + (userRightsList.Count * Marshal.SizeOf(typeof(IpcUserRights))));

            allocatedBuffers.Add(ipcUserRightsListPtr);

            IpcUserRightsList_Header ipcUserRightsListHeader = new IpcUserRightsList_Header();
            ipcUserRightsListHeader.cbSize = (uint)Marshal.SizeOf(typeof(IpcUserRightsList_Header));
            ipcUserRightsListHeader.cUserRights = (uint)userRightsList.Count;
            Marshal.StructureToPtr(ipcUserRightsListHeader, ipcUserRightsListPtr, false);

            // go to the first IpcUserRights struct entry
            IntPtr currentPtr = ipcUserRightsListPtr + Marshal.SizeOf(typeof(IpcUserRightsList_Header));

            foreach (UserRights userRights in userRightsList)
            {
                // Create and initialize an instance of IpcUserRights structure
                IpcUserRights ipcUserRights = new IpcUserRights();
                ipcUserRights.user = new IpcUser();
                ipcUserRights.user.userType = userRights.UserIdType;
                ipcUserRights.user.userID = userRights.UserId;
                ipcUserRights.cRights = (uint)userRights.Rights.Count;

                // Allocate memory for the right string array
                ipcUserRights.rgwszRights = Marshal.AllocHGlobal((int)ipcUserRights.cRights * Marshal.SizeOf(typeof(IntPtr)));

                allocatedBuffers.Add(ipcUserRights.rgwszRights);

                // go to the first right string entry
                IntPtr currentRightPtr = ipcUserRights.rgwszRights;
                foreach (string right in userRights.Rights)
                {
                    // the right string itself
                    IntPtr RightStringPtr = Marshal.StringToHGlobalUni(right);

                    allocatedBuffers.Add(RightStringPtr);

                    // assign the pointer
                    Marshal.WriteIntPtr(currentRightPtr, 0, RightStringPtr);

                    // go to the next right string entry
                    currentRightPtr = new IntPtr(currentRightPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                }

                Marshal.StructureToPtr(ipcUserRights, currentPtr, false);

                // go to the next IpcUserRights struct entry
                currentPtr = currentPtr + Marshal.SizeOf(typeof(IpcUserRights));
            }

            return ipcUserRightsListPtr;
        }

        // Manually marshals a IPC_USER_RIGHTS_LIST structure in unmanaged memory into a Collection<UserRights>.
        //  See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
        private static void MarshalUserRightsListToManaged(IntPtr ipcUserRightsListPtr, Collection<UserRights> userRightsList)
        {
            // the "header" is the IPC_USER_RIGHTS_LIST structure without the variable size array of IPC_USER_RIGHTS
            IpcUserRightsList_Header ipcUserRightsListHeader =
                    (IpcUserRightsList_Header)Marshal.PtrToStructure(ipcUserRightsListPtr, typeof(IpcUserRightsList_Header));

            // go to first element in the array of IPC_USER_RIGHTS
            IntPtr currentPtr = ipcUserRightsListPtr + Marshal.SizeOf(typeof(IpcUserRightsList_Header));

            for (int i = 0; i < ipcUserRightsListHeader.cUserRights; i++)
            {
                IpcUserRights ipcUserRights = (IpcUserRights)Marshal.PtrToStructure(currentPtr, typeof(IpcUserRights));

                Collection<string> rightList = new Collection<string>();
                for (int j = 0; j < ipcUserRights.cRights; j++)
                {
                    // Get the j-th right from the ipcUserRights.rgwszRights string array
                    IntPtr RightStringPtr = Marshal.ReadIntPtr(ipcUserRights.rgwszRights, j * Marshal.SizeOf(typeof(IntPtr)));

                    // Read the string
                    string right = Marshal.PtrToStringUni(RightStringPtr);

                    rightList.Add(right);
                }

                UserRights userRights = new UserRights(ipcUserRights.user.userType, ipcUserRights.user.userID, rightList);
                userRightsList.Add(userRights);

                // go to the next element in the array of IPC_USER_RIGHTS
                currentPtr = currentPtr + Marshal.SizeOf(typeof(IpcUserRights));
            }
        }
        
        // Manually marshals a NameValueCollection into a IPC_NAME_VALUE_LIST structure in unmanaged memory.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535277(v=vs.85).aspx
        private static IntPtr MarshalNameValueListToNative(NameValueCollection nameValueList)
        {
            // allocate memory for IPC_NAME_VALUE_LIST variable size structure
            IntPtr nameValueListPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcNameValue)) * (nameValueList.Count) + Marshal.SizeOf(typeof(IntPtr)));

            try
            {
                // the number of IPC_NAME_VALUE entries goes first
                Marshal.WriteInt32(nameValueListPtr, (int)nameValueList.Count);

                // go to the first IPC_NAME_VALUE struct entry
                IntPtr currentPtr = (IntPtr)((long)nameValueListPtr + Marshal.SizeOf(typeof(IntPtr)));

                for (int i = 0; i < nameValueList.Count; i++)
                {
                    IpcNameValue ipcNameValue = new IpcNameValue();
                    ipcNameValue.Name = nameValueList.GetKey(i);
                    // We assume that the Value field is always in en-us (1033 is the locale id of the en-us locale).
                    // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535276(v=vs.85).aspx
                    ipcNameValue.lcid = (uint)1033; 
                    ipcNameValue.Value = nameValueList.Get(i);

                    Marshal.StructureToPtr(ipcNameValue, currentPtr, false);

                    // go to the next IPC_NAME_VALUE struct entry
                    currentPtr = currentPtr + Marshal.SizeOf(ipcNameValue);
                }
            }
            catch(Exception)
            {
                Marshal.FreeHGlobal(nameValueListPtr);
                throw;
            }

            return nameValueListPtr;
        }

        // Manually marshals a IPC_NAME_VALUE_LIST structure in unmanaged memory into a NameValueCollection.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535277(v=vs.85).aspx
        private static void MarshalNameValueListToManaged(IntPtr nameValueListPtr, NameValueCollection nameValueList)
        {
            // the number of (name, value) pairs is the first in the IPC_NAME_VALUE_LIST struct
            int nameValuePairCount = Marshal.ReadInt32(nameValueListPtr);

            // go to the first IPC_NAME_VALUE entry
            IntPtr currentPtr = nameValueListPtr + Marshal.SizeOf(typeof(IntPtr));

            for (int i = 0; i < nameValuePairCount; i++)
            {
                IpcNameValue pair = (IpcNameValue)Marshal.PtrToStructure(currentPtr, typeof(IpcNameValue));
                nameValueList.Add(pair.Name, pair.Value);

                // go to the next IPC_NAME_VALUE entry
                currentPtr = (IntPtr)((long)currentPtr + Marshal.SizeOf(pair));
            }
        }

        // Manually marshals a byte array into a IPC_BUFFER structure in unmanaged memory.
        // See  http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
        private static IpcBuffer MarshalIpcBufferToNative(byte[] buffer)
        {
            IpcBuffer ipcBuffer = new IpcBuffer();
            ipcBuffer.cbBuffer = (uint)buffer.Length;
            ipcBuffer.pvBuffer = Marshal.AllocHGlobal(buffer.Length);
            try
            {
                Marshal.Copy(buffer, 0, ipcBuffer.pvBuffer, buffer.Length);
            }
            catch (Exception)
            {
                Marshal.FreeHGlobal(ipcBuffer.pvBuffer);
                throw;
            }
            return ipcBuffer;
        }

        // Manually marshals a IPC_BUFFER structure in unmanaged memory into a byte array.
        // See  http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
        public static byte[] MarshalIpcBufferToManaged(IntPtr ipcBufferPtr)
        {
            IpcBuffer ipcBuffer = (IpcBuffer)Marshal.PtrToStructure(ipcBufferPtr, typeof(IpcBuffer));

            byte[] buffer = new byte[ipcBuffer.cbBuffer];
            Marshal.Copy(ipcBuffer.pvBuffer, buffer, 0, (int)ipcBuffer.cbBuffer);

            return buffer;
        }

        public static void ThrowOnErrorCode(int hrError)
        {
            if (hrError < 0)
            {
                IntPtr errorMessageTextPtr = IntPtr.Zero;
                try
                {
                    int hrGetErrorMessage = UnsafeNativeMethods.IpcGetErrorMessageText(hrError, 0, out errorMessageTextPtr);

                    string errorMessageText = Marshal.PtrToStringUni(errorMessageTextPtr);

                    if (hrGetErrorMessage >= 0)
                    {
                        throw new InformationProtectionException(hrError, errorMessageText);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(hrError);
                    }
                }
                finally
                {
                    UnsafeNativeMethods.IpcFreeMemory(errorMessageTextPtr);
                }
            }
        }
    }
}
