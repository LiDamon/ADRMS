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
        public const bool SHOW_MSGBOX = true;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_HIDE = 0;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            IntPtr winHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(winHandle, SW_HIDE);

            //Console.Clear();

            var encryptionAndDecryption = new EncryptionAndDecryption();
            var options = new Options();

            string action = string.Empty;
            string file = string.Empty;
            string templateName = string.Empty;
            Collection<UserRights> usersRightsCollection = new Collection<UserRights>();
            Collection<TemplateInfo> templatesInfo = encryptionAndDecryption.GetTemplatesInfo();


            if (CommandLine.Parser.Default.ParseArguments(args, options)) 
            {
                file = options.inputFile.ToLower();

                if (options.fileInfo)
                {
                    if (encryptionAndDecryption.IsEncrypted(file))
                    {
                        //DialogResult messageBox = MessageBox.Show("File is protected!", "CCC RMS", MessageBoxButtons.OK);
                        if (SHOW_MSGBOX == true)
                            MessageBox.Show("File is protected!", "CCC RMS", MessageBoxButtons.OK);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("File is protected!");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        if (SHOW_MSGBOX == true)
                            MessageBox.Show("File is not protected!", "CCC RMS", MessageBoxButtons.OK);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("File is not protected!");
                            Console.WriteLine();
                        }
                    }
                }

                else if (options.action.ToLower() == "protect" || options.action.ToLower() == "unprotect")
                {
                    action = options.action.ToLower();

                    //PRINT ACTION
                    if (SHOW_MSGBOX == false)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Action: {0} File", options.action);
                        Console.WriteLine();
                    }

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
                                    encryptionAndDecryption.EncryptFile("rmsprotector@ccc.gr", usersRightsCollection, file);
                                }
                                else 
                                {
                                    //SEND FEEDBACK
                                    if (SHOW_MSGBOX == true)
                                        MessageBox.Show("Please pass a valid rights string!", "CCC RMS", MessageBoxButtons.OK);
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Please pass a valid rights string.");
                                        Console.WriteLine();
                                        Console.WriteLine(options.GetUsage());
                                    }
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
                                    //SEND FEEDBACK
                                    if (SHOW_MSGBOX == true)
                                        MessageBox.Show("Template does not exist, please choose an available template name.", "CCC RMS", MessageBoxButtons.OK);
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Template does not exist, please choose an available template name.");
                                        Console.WriteLine();
                                        Console.WriteLine(options.GetUsage());
                                    }
                                }
                            }

                            else
                            {
                                //SEND FEEDBACK
                                if (SHOW_MSGBOX == true)
                                    MessageBox.Show("Please pass either a rights string or a template name.", "CCC RMS", MessageBoxButtons.OK);
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Please pass either a rights string or a template name.");
                                    Console.WriteLine();
                                    Console.WriteLine(options.GetUsage());
                                }
                            }
                        }
                        else
                        {
                            //SEND FEEDBACK
                            if (SHOW_MSGBOX == true)
                                MessageBox.Show("File is already protected.", "CCC RMS", MessageBoxButtons.OK);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("File is already protected.");
                                Console.WriteLine();
                                Console.WriteLine(options.GetUsage());
                            }
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
                            //SEND FEEDBACK
                            if (SHOW_MSGBOX == true)
                                MessageBox.Show("File is not protected.", "CCC RMS", MessageBoxButtons.OK);
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("File is not protected.");
                                Console.WriteLine();
                                Console.WriteLine(options.GetUsage());
                            }
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
