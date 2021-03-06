﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Web;
using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;

namespace SPSCommon.SPCustomExtensions
{
    public static class SPUserExtension
    {
        public static List<SPPrincipal> GetUserManagers(this SPUser user)
        {
            List<SPPrincipal> userManagers = new List<SPPrincipal>();
            UserProfile[] userManagersProfiles;

            SPServiceContext spServiceContext = SPServiceContext.GetContext(user.ParentWeb.Site);
            UserProfileManager userProfileManager = new UserProfileManager(spServiceContext);
            if (userProfileManager.UserExists(user.LoginName))
            {
                UserProfile userProfile = userProfileManager.GetUserProfile(user.LoginName);
                userManagersProfiles = userProfile.GetManagers();
            }
            else
            {
                return userManagers;
            }

            foreach (UserProfile managerProfile in userManagersProfiles)
            {
                SPUser manager = user.ParentWeb.EnsureUser(managerProfile.AccountName);
                userManagers.Add(manager);
            }

            return userManagers;
        }

        public static SPPrincipal GetUserAssistant(this SPUser user)
        {
            SPPrincipal usersAssistant = null;

            SPServiceContext spServiceContext = SPServiceContext.GetContext(user.ParentWeb.Site);
            UserProfileManager userProfileManager = new UserProfileManager(spServiceContext);

            if (!userProfileManager.UserExists(user.LoginName))
            {
                return usersAssistant;
            }

            UserProfile userProfile = userProfileManager.GetUserProfile(user.LoginName);
            string assistantLogin = userProfile["Assistant"].Value as string;

            if (String.IsNullOrEmpty(assistantLogin))
            {
                return usersAssistant;
            }

            usersAssistant = user.ParentWeb.EnsureUser(assistantLogin);

            return usersAssistant;
        }
    }
}