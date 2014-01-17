using System;
using System.Collections.Generic;
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
    }

    
}
