//-----------------------------------------------------------------------------
// Created By Sghaida
// Description:  Structure declarations for interop services required to call into unmanaged IPC SDK APIs   
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace CCC.RMSLib
{
    // Environment Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535247(v=vs.85).aspx
    public enum EnvironmentInformationType : uint
    {
        SecurityMode = 3,
    };

    // Prompt Context flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535278(v=vs.85).aspx
    public enum PromptContextFlag : uint
    {
        Slient = 1,
        Offline = 2,
        HasUserConsent = 4
    };

    // IpcSerializeLicense() function input types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
    public enum SerializationInputType : uint
    {
        TemplateId = 1,
        License = 2,
    }

    // IpcGetTemplateList() function flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535267(v=vs.85).aspx
    public enum GetTemplateListFlags : uint
    {
        ForceDownload = 1
    }

    // IpcGetTemplateIssuerList() function flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535266(v=vs.85).aspx
    public enum GetTemplateIssuerListFlags : uint
    {
        DefaultServerOnly = 1
    }

    // License Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
    public enum LicensePropertyType : uint
    {
        ValidityTime = 1,
        IntervalTime = 2,
        Owner = 3,
        UserRightsList = 4,
        AppSpecificData = 5,
        DeprecatedEncryptionAlgorithms = 6,
        ConnectionInfo = 7,
        Descriptor = 8,
        ReferralInfoUrl = 10,
        ContentKey = 11,
        AppSpecificDataNoEncryption = 13,
    };

    // Key Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
    public enum KeyPropertyType : uint
    {
        BlockSize = 2,
        License = 6,
        UserDisplayName = 7,
    };

    // IPC_PROMPT_CONTEXT - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535278(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcPromptContext
    {
        public uint cbSize;
        public IntPtr hWndParent;
        public uint flags;
        public IntPtr hCancelEvent;
        public IntPtr pcCredential;

        public IpcPromptContext()
        {
            this.cbSize = (uint)Marshal.SizeOf(typeof(IpcPromptContext));
        }
    }

    // IPC_TEMPLATE_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535279(v=vs.85).aspx
    [StructLayout( LayoutKind.Sequential, CharSet=CharSet.Unicode )]
    public class IpcTemplateInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateID;
        public uint lcid;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateDescription;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string issuerDisplayName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fromTemplate;
    }

    // IPC_TEMPLATE_ISSUER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535280(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class IpcTemplateIssuer
    {
        public IpcConnectionInfo connectionInfo;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string wszDisplayName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fAllowFromScratch;

    }

    // IPC_USER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535284(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUser 
    {
        [MarshalAs(UnmanagedType.U4)]
        public UserIdType userType;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string userID;
    }

    // IPC_TERM - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535282(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcTerm 
    {
        public FileTime ftStart;
        public ulong dwDuration;
    }

    // IPC_NAME_VALUE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535276(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcNameValue
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        public uint lcid;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }

    // IPC_USER_RIGHTS - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535285(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUserRights
    {
        public IpcUser user;
        public uint cRights;
        public IntPtr rgwszRights;
    }

    // IPC_USER_RIGHTS_LIST (excluding the variable size array rgUserRights) - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
    // In the native API, a USER_RIGHTS_LIST is a structure with the below fields, followed by a list of USER_RIGHTS
    // structs using the ANYSIZE_ARRAY pattern. This does not match any automatic marshal type, so we instead
    // automatically marshal only the below fields and manually marshal the array.
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUserRightsList_Header
    {
        public uint cbSize;
        public uint cUserRights;
        //public UserRights[] rgUserRights; (variable size arrays are not supported by interop layer, see above)
    }

    // IPC_BUFFER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcBuffer
    {
        public IntPtr pvBuffer;
        public uint cbBuffer;
    }

    // IPC_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535274(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcConnectionInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ExtranetUrl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string IntranetUrl;
    }

    // FILETIME - http://msdn.microsoft.com/en-us/library/windows/desktop/ms724284(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public struct FileTime
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;

        public FileTime(long fileTime)
        {
            byte[] bytes = BitConverter.GetBytes(fileTime);
            dwLowDateTime = BitConverter.ToUInt32(bytes, 0);
            dwHighDateTime = BitConverter.ToUInt32(bytes, 4);
        }

        public static implicit operator long(FileTime fileTime)
        {
            long returnedLong;
            byte[] highBytes = BitConverter.GetBytes(fileTime.dwHighDateTime);
            Array.Resize(ref highBytes, 8);
            returnedLong = BitConverter.ToInt64(highBytes, 0);
            returnedLong = returnedLong << 32;
            returnedLong = returnedLong | fileTime.dwLowDateTime;
            return returnedLong;
        }
    }
}
