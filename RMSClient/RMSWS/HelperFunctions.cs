using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using CCC.RMSLib;

namespace RMSWS
{
    class HelperFunctions
    {

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        
        private string EmailAddress { get; set; }
        private string RightsList { get; set; }


        public static string GetMimeFromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found");

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= 256)
                    fs.Read(buffer, 0, 256);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }
            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                return "unknown/unknown";
            }
        }


        public void AddRights(string EmailAddress, string RightsList)
        {
            this.EmailAddress = EmailAddress;
            this.RightsList = RightsList;
        }


        public static string ConvertRightsToString(HelperFunctions rights) 
        {
           StringBuilder sb = new StringBuilder();

            sb.Append(rights.EmailAddress);
            sb.Append(":");
            sb.Append(rights.RightsList);

            return sb.ToString();

        }


        public static HelperFunctions ConvertStringtoRights(string rights)
        {
            HelperFunctions userRights = new HelperFunctions();

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
