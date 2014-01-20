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
        //[DllImport("IpcManagedAPI.dll")]

        //public static extern Int32 placeHolder();

        //public const string filePath = @"D:\rms-data\";

        static void Main(string[] args)
        {
            //Console.Clear();
            //Console.WriteLine("* Start of Transmission");

            ////String inputFile = "docxFile.docx";
            //String inputFile = @"ptestFile.pdf";

            //var encryptor = new EncryptionAndDecryption();

            ////Collection<TemplateInfo> myTemplateInfo = encryptor.GetTemplatesInfo();

            //bool inputFileIsEncrypted = encryptor.IsEncrypted(filePath + @"\" + inputFile);

            //if (inputFileIsEncrypted)
            //{
            //    Console.WriteLine("** DECRYPT: File " + inputFile + " is encrypted");

            //    encryptor.DecryptFile(filePath + @"\" + inputFile);
            //}
            //else
            //{
            //    Console.WriteLine("** File " + inputFile + " is not encrypted");

            //    string owner = "sghaida@ccc.gr";
            //    string user = "aalhour@ccc.gr";

            //    var rights = new Collection<UserRights>();

            //    UserRights owner_rights = encryptor.SetUserRights(UserIdType.Email, owner, new Collection<string>() 
            //    {
            //        CommonRights.OwnerRight
            //    });

            //    UserRights user_rights = encryptor.SetUserRights(UserIdType.Email, user, new Collection<string>() 
            //    {
            //        CommonRights.ViewRightsDataRight,
            //        CommonRights.ViewRight,
            //        CommonRights.PrintRight,
            //        CommonRights.ForwardRight
            //    });

            //    rights.Add(owner_rights);
            //    rights.Add(user_rights);

            //    encryptor.EncryptFile(owner, rights, filePath + @"\" + inputFile);
            //    encryptor.EncryptFile(filePath + @"\" + inputFile, myTemplateInfo[0].TemplateId);

            //}//End-Else

            
            var encryptionAndDecryption = new EncryptionAndDecryption();
            var options = new Options();


            if (CommandLine.Parser.Default.ParseArguments(args, options)) 
            {
                string action = options.action.ToLower();
                string file = options.InputFile.ToLower();
                string templateName = string.Empty;
                Collection<UserRights> usersRightsCollection = new Collection<UserRights>();
                Collection<TemplateInfo> templatesInfo = encryptionAndDecryption.GetTemplatesInfo();

                if (options.action.ToLower() == "protect" || options.action.ToLower() == "unprotect")
                {
                    Console.WriteLine("Action: {0} File", options.action);

                    //PROTECT CASE
                    if (action == "protect" && encryptionAndDecryption.IsEncrypted(file) == false)
                    {
                        //IN CASE OF RIGHTS LIST
                        if (!string.IsNullOrEmpty(options.rights))
                        {
                            usersRightsCollection = UserRightsHelper.ConvertRightsStringToCollection(options.rights);

                            encryptionAndDecryption.EncryptFile("aalhour@ccc.gr", usersRightsCollection, file);
                        }

                        //IN CASE OF TEMPLATE NAME
                        else if (!string.IsNullOrEmpty(options.templateName))
                        {
                            templateName = options.templateName.ToLower();

                            var template = templatesInfo.FirstOrDefault(item => item.Name.ToLower() == templateName);

                            if (template != null)
                            {
                                encryptionAndDecryption.EncryptFile(file, template.TemplateId);
                            }
                            else
                            {
                                Console.WriteLine("Template does not exist, please choose an available template name.");
                                Console.WriteLine(options.GetUsage());
                            }
                        }

                        else
                        {
                            Console.WriteLine("Please pass either a rights string or a template name.");
                            Console.WriteLine(options.GetUsage());
                        }

                        //Console.WriteLine("Action was completed successfully.");
                    }
                    else if (action == "unprotect" && encryptionAndDecryption.IsEncrypted(file) == true)
                    {
                        encryptionAndDecryption.DecryptFile(file);
                        //Console.WriteLine("Action was completed successfully.");
                    }
                }
                else 
                {
                    Console.WriteLine(options.GetUsage());
                }
                
            }

        }//End-Main

    }

}
