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
        static void Main(string[] args)
        {
            Console.Clear();

            var encryptionAndDecryption = new EncryptionAndDecryption();
            var options = new Options();

            string action = options.action.ToLower();
            string file = options.InputFile.ToLower();
            string templateName = string.Empty;
            Collection<UserRights> usersRightsCollection = new Collection<UserRights>();
            Collection<TemplateInfo> templatesInfo = encryptionAndDecryption.GetTemplatesInfo();


            if (CommandLine.Parser.Default.ParseArguments(args, options)) 
            {
                if (options.action.ToLower() == "protect" || options.action.ToLower() == "unprotect")
                {
                    Console.WriteLine();
                    Console.WriteLine("Action: {0} File", options.action);
                    Console.WriteLine();

                    //PROTECT CASE
                    if (action == "protect")
                    {
                        if (encryptionAndDecryption.IsEncrypted(file) == false)
                        {
                            //IN CASE OF RIGHTS LIST
                            if (!string.IsNullOrEmpty(options.rights))
                            {
                                usersRightsCollection = UserRightsHelper.ConvertRightsStringToCollection(options.rights);

                                if (usersRightsCollection.Count > 0)
                                {
                                    encryptionAndDecryption.EncryptFile("aalhour@ccc.gr", usersRightsCollection, file);
                                }
                                else 
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Please pass a valid rights string.");
                                    Console.WriteLine();
                                    Console.WriteLine(options.GetUsage());
                                }
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
                                    Console.WriteLine();
                                    Console.WriteLine("Template does not exist, please choose an available template name.");
                                    Console.WriteLine();
                                    Console.WriteLine(options.GetUsage());
                                }
                            }

                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Please pass either a rights string or a template name.");
                                Console.WriteLine();
                                Console.WriteLine(options.GetUsage());
                            }
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("File is already protected.");
                            Console.WriteLine();
                            Console.WriteLine(options.GetUsage());
                        }
                    }

                    else if (action == "unprotect")
                    {
                        if (encryptionAndDecryption.IsEncrypted(file) == true)
                        {
                            encryptionAndDecryption.DecryptFile(file);
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("File is not protected.");
                            Console.WriteLine();
                            Console.WriteLine(options.GetUsage());
                        }
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
