using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CCC.RMSLib
{
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
            }

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
