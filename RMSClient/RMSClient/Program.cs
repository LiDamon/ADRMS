using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using CCC.RMSLib;

namespace RMSClient
{
    class Program
    {
        [DllImport("IpcManagedAPI.dll")]

        public static extern Int32 placeHolder();

        public const string filePath = @"C:\Users\sghaida\Documents\Visual Studio 2012\Projects\RMSLib\RMSClient\RMSClient\bin\Debug";

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("* Start of Transmission");
            SafeNativeMethods.IpcInitialize();
            
            //String inputFile = "docxFile.docx";
            String inputFile = "ptestFile.pdf";
            String templateLocally = "serialized_license.xml";
            String templateLocallyContent = File.ReadAllText(filePath + @"\" + templateLocally);


            Collection<TemplateInfo> myTemplateInfo = SafeNativeMethods.IpcGetTemplateList(null, true, true, false, true, null, null);

            bool inputFileIsEncrypted = SafeFileApiNativeMethods.IpcfIsFileEncrypted(inputFile);
            
            if (inputFileIsEncrypted)
            {
                Console.WriteLine("** DECRYPT: File " + inputFile + " is encrypted");

                var decrypter = new EncryptionAndDecryption();
                decrypter.DecryptFile(filePath + @"\" + inputFile);
            }
            else
            {
                Console.WriteLine("** File " + inputFile + " is not encrypted");

                string owner = "sghaida@ccc.gr";
                string user = "aalhour@ccc.gr";

                var rights = new Collection<UserRights>();

                UserRights owner_rights = new UserRights(UserIdType.Email, owner, new Collection<string>() 
                {
                    CommonRights.OwnerRight
                });

                UserRights user_rights = new UserRights(UserIdType.Email, user, new Collection<string>() 
                {
                    CommonRights.ViewRightsDataRight,
                    CommonRights.ViewRight,
                    CommonRights.PrintRight,
                    CommonRights.ForwardRight
                });
                
                rights.Add(owner_rights);
                rights.Add(user_rights);

                var encrypter = new EncryptionAndDecryption();
                
                encrypter.EncryptFile(owner, rights, filePath + @"\" + inputFile);
                //  encrypter.EncryptFile(filePath + @"\" + inputFile, myTemplateInfo[0].TemplateId);

            }//End-Else

        }//End-Main

    }

}
