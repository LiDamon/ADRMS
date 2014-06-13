using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.ComponentModel.DataAnnotations;
using System.Text;

using CCC.RMSLib;
using System.Web;
using System.Collections.ObjectModel;

namespace rmsWebAPI.Controllers
{
    public class ValuesController : ApiController
    {
        private EncryptionAndDecryption protector = new EncryptionAndDecryption();


        [HttpGet]
        public string Protect(string filePath, string templateName)
        {
            string adTemplateName = HttpUtility.UrlDecode(templateName);
            string file = Encoding.UTF8.GetString(Convert.FromBase64String(filePath));

            Collection<TemplateInfo> templatesInfo = protector.GetTemplatesInfo();

            var template = templatesInfo.FirstOrDefault(item => item.Name.ToLower() == adTemplateName);

            if (!protector.IsEncrypted(file))
            {
                if (template == null)
                    return "Template doesn't exist or template name is invalid.";
                
                protector.EncryptFile(file, template.TemplateId);
                
                return "File has been proctected with template.";
            }
            else 
            {
                return "File is already Protected.";
            }
        }

        [HttpGet]
        public string Protect(string filePath, string ownerEmail, string listOfRights)
        {
            string adListOfRights = Encoding.UTF8.GetString(Convert.FromBase64String(listOfRights));
            
            string file = Encoding.UTF8.GetString(Convert.FromBase64String(filePath));

            //return file + "/" + ownerEmail + "/" + adListOfRights;

            Collection<UserRights> userRights = new Collection<UserRights>();

            if (!protector.IsEncrypted(file))
            {
                userRights = UserRightsHelper.ConvertRightsStringToCollection(adListOfRights);

                //return userRights.Count.ToString();

                if (userRights.Count > 0)
                {
                    protector.EncryptFile(ownerEmail, userRights, file);

                    return "File has been protected with list of rights.";
                }
                else
                {
                    return "Kindly pass a valid rights string.";
                }
            }
            else 
            {
                return "File is already Protected.";
            }

        }

        [HttpGet]
        public string Unprotect(string filePath)
        {
            string file =Encoding.UTF8.GetString(Convert.FromBase64String(filePath));

            if (protector.IsEncrypted(file))
            {
                protector.DecryptFile(file);
                return "File has been unprotected.";
            }
            else
            {
                return "File is not protected.";
            }
        }


        [HttpGet]
        public string IsProtected(string filePath) 
        {
            string file = Encoding.UTF8.GetString(Convert.FromBase64String(filePath));

            if (protector.IsEncrypted(file))
            {
                return "true";
            }
            else 
            {
                return "false";
            }

        }

    }
    
    class UserRightsHelper
    {
        string EmailAddress { get;set; }
        string RightsList { get; set; }


        public void AddRights(string EmailAddress, string RightsList)
        {
            this.EmailAddress = EmailAddress;
            this.RightsList = RightsList;
        }


        public static string ConvertRightsToString(UserRightsHelper rights) 
        {
           StringBuilder sb = new StringBuilder();

            sb.Append(rights.EmailAddress);
            sb.Append(":");
            sb.Append(rights.RightsList);

            return sb.ToString();

        }


        public static UserRightsHelper ConvertStringtoRights(string rights)
        {
            UserRightsHelper userRights = new UserRightsHelper();

            string[] rightsArray = rights.Split(':');

            userRights.AddRights(rightsArray[0], rightsArray[1]);

            return userRights;
        }


        public static Collection<UserRights> ConvertRightsStringToCollection(string rights)
        {
            UserRights userRightObject;
            Collection<string> commonRights;
            Collection<UserRights> usersRightsCollection = new Collection<UserRights>();
            Collection<string> SystemCommonRights = GetCommonRights();

            List<string> tempUserRights = new List<string>();

            try
            {
                tempUserRights = rights.ToLower().Split(';').ToList();
            }
            catch (Exception e)
            {
                //throw new Exception("exception happened here 2 : ", e);
            }


            try
            {
                //foreach user
                foreach (string tempUserRight in tempUserRights)
                {
                    commonRights = new Collection<string>();

                    var userRightsInfo = ConvertStringtoRights(tempUserRight);

                    if (!string.IsNullOrEmpty(userRightsInfo.RightsList))
                    {
                        var rightsList = userRightsInfo.RightsList.Split(',').ToList();

                        foreach (string right in rightsList)
                        {
                            if (SystemCommonRights.Contains(right.ToUpper()))
                                commonRights.Add(right.ToUpper());
                        }

                        if (commonRights.Count > 0)
                        {
                            userRightObject = new UserRights(UserIdType.Email, userRightsInfo.EmailAddress, commonRights);
                            usersRightsCollection.Add(userRightObject);
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                //throw new Exception("exception happened here 2 : ", e);
            }

            return usersRightsCollection;
        }


        public static Collection<string> GetCommonRights()
        {
            Collection<string> commonRights = new Collection<string>()
            {
                CommonRights.OwnerRight,
                CommonRights.ViewRight,
                CommonRights.EditRight,
                CommonRights.ExtractRight,
                CommonRights.ExportRight,
                CommonRights.PrintRight,
                CommonRights.CommentRight,
                CommonRights.ViewRightsDataRight,
                CommonRights.EditRightsDataRight,
                CommonRights.ForwardRight,
                CommonRights.ReplyRight,
                CommonRights.ReplyAllRight,
                CommonRights.ObjectModelRight,
                CommonRights.DocEditRight
            };

            return commonRights;
        }

    }

}