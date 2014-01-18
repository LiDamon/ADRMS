using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CCC.RMSLib;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;


namespace RMSAPI
{
   
    public class RMSAPI
    {
        [DllImport("IpcManagedAPI.dll")]
        public static extern Int32 placeHolder();

        public RMSAPI()
        {
            SafeNativeMethods.IpcInitialize();
        }


        //Decrypt Procedure
        public void DecryptFile(string filePath)
        {
           SafeFileApiNativeMethods.IpcfDecryptFile(filePath, SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_DEFAULT, true, false, true, null);
        }

        public Collection<TemplateInfo> GetTemplatesInfo() 
        {
            return SafeNativeMethods.IpcGetTemplateList(null, true, true, false, true, null, null);
        }

        public bool IsEncrypted(string filePath) 
        {
            return SafeFileApiNativeMethods.IpcfIsFileEncrypted(filePath);
        }

        public UserRights SetUserRights(UserIdType userType, string userName, Collection<string> rights) 
        {
            return new UserRights(userType, userName, rights);
        }


        //Encrypt Procedure
        public void EncryptFile(string owner, Collection<UserRights> listOfRights, string filePath)
        {
            string fileName;
            string pathToFile;
            
            TemplateIssuer issuer;
            SafeInformationProtectionKeyHandle keyhandle;
            SafeInformationProtectionLicenseHandle licenseHandle;
            
            fileName = Path.GetFileName(filePath);
            pathToFile = Path.GetDirectoryName(filePath);

            issuer = new TemplateIssuer(null, owner, true);

            licenseHandle = SafeNativeMethods.IpcCreateLicenseFromScratch(issuer);

            SafeNativeMethods.IpcSetLicenseOwner(licenseHandle, owner);

            SafeNativeMethods.IpcSetLicenseUserRightsList(licenseHandle, listOfRights);

            byte[] license = SafeNativeMethods.IpcSerializeLicense(licenseHandle,0, true, false, true, null, out keyhandle);

            Collection<UserRights> rights = SafeNativeMethods.IpcGetSerializedLicenseUserRightsList(license, keyhandle);

            SafeFileApiNativeMethods.IpcfEncryptFile(filePath, licenseHandle, SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT, true, false, true, null, pathToFile);
        }

        public void EncryptFile(string filePath, string templateId)
        {
            string fileName;
            string pathToFile;

            SafeInformationProtectionKeyHandle keyhandle;
            SafeInformationProtectionLicenseHandle licenseHandle;

            fileName = Path.GetFileName(filePath);
            pathToFile = Path.GetDirectoryName(filePath);
            
            licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId);

            byte[] license = SafeNativeMethods.IpcSerializeLicense(licenseHandle, 0, true, false, true, null, out keyhandle);

            Collection<UserRights> rights = SafeNativeMethods.IpcGetSerializedLicenseUserRightsList(license, keyhandle);

            SafeFileApiNativeMethods.IpcfEncryptFile(filePath, licenseHandle, SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT, false, true, true, null, pathToFile);
        }

    }
}

