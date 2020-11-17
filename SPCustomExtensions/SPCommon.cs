using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace SPERCommonLib
{
    public static class SPCommon
    {
        public static bool IsUTCDateString(string str)
        {
            bool IsUTCDate;
            IsUTCDate = Regex.IsMatch(str, @"^\d{4}(-\d+){2}T[\d\:]+Z$");

            return IsUTCDate;
        }

        public static List<string> GetUserNames(List<SPPrincipal> principalsList)
        {
            List<string> userNames = new List<string>();

            foreach (SPPrincipal principal in principalsList)
            {
                userNames.Add(principal.Name);
            }

            return userNames;
        }

        public static List<string> GetUserMails(List<SPPrincipal> principalsList)
        {
            List<string> toMailsList = new List<string>() { };

            foreach (SPPrincipal principal in principalsList)
            {
                if (principal.GetType().Name == "SPUser")
                {
                    SPUser user = (SPUser)principal;
                    toMailsList.Add(user.Email);
                }

                if (principal.GetType().Name == "SPGroup")
                {
                    List<string> groupMembersMails = GetUserMails((((SPGroup)principal).Users).Cast<SPPrincipal>().ToList());
                    toMailsList.AddRange(groupMembersMails);
                }
            }

            return toMailsList;
        }

        public static List<string> GetLoginsFromPrincipals(List<SPPrincipal> principalsList)
        {
            List<string> logins = new List<string>();

            foreach (SPPrincipal principal in principalsList)
            {
                logins.Add(principal.LoginName);
            }

            return logins;
        }

        public static bool IsEventIng(SPItemEventProperties properties)
        {
            bool isEventIng = properties.EventType.ToString().Contains("ing");
            return isEventIng;
        }

        public static bool IsUpdatingByAccountMatch(SPItemEventProperties properties, string AccountMatch = @"app@sharepoint || svc_")
        {
            if ( Regex.IsMatch(properties.UserDisplayName, AccountMatch) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsJustCreated(SPListItem listItem)
        {
            DateTime itemTimeCreated = (DateTime)listItem["Created"];
            DateTime itemTimeModified = (DateTime)listItem["Modified"];
            Double diffInSeconds = (itemTimeModified - itemTimeCreated).TotalSeconds;

            if (diffInSeconds < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
