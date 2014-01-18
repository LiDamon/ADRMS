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

using CCC;

namespace RMSClient
{
    class Program
    {
        [DllImport("IpcManagedAPI.dll")]

        public static extern Int32 placeHolder();

        public const string filePath = @"D:\rms-data\";

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("* Start of Transmission");
            
            //String inputFile = "docxFile.docx";
            String inputFile = @"ptestFile.pdf";

            var encryptor = new EncryptionAndDecryption();

            //Collection<TemplateInfo> myTemplateInfo = encryptor.GetTemplatesInfo();

            bool inputFileIsEncrypted =encryptor.IsEncrypted(filePath + @"\" + inputFile);
            
            if (inputFileIsEncrypted)
            {
                Console.WriteLine("** DECRYPT: File " + inputFile + " is encrypted");

                encryptor.DecryptFile(filePath + @"\" + inputFile);
            }
            else
            {
                Console.WriteLine("** File " + inputFile + " is not encrypted");

                string owner = "sghaida@ccc.gr";
                string user = "aalhour@ccc.gr";

                var rights = new Collection<UserRights>();

                UserRights owner_rights = encryptor.SetUserRights(UserIdType.Email, owner, new Collection<string>() 
                {
                    CommonRights.OwnerRight
                });

                UserRights user_rights = encryptor.SetUserRights(UserIdType.Email, user, new Collection<string>() 
                {
                    CommonRights.ViewRightsDataRight,
                    CommonRights.ViewRight,
                    CommonRights.PrintRight,
                    CommonRights.ForwardRight
                });
                
                rights.Add(owner_rights);
                rights.Add(user_rights);

                encryptor.EncryptFile(owner, rights, filePath + @"\" + inputFile);
                //encryptor.EncryptFile(filePath + @"\" + inputFile, myTemplateInfo[0].TemplateId);

            }//End-Else

        }//End-Main

    }

}
