using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SPERCommonLib
{
    public static class ERItemSPExtension
    {
        //static SPUser svcUserForEmptyResponse;
        public static string[] GetCodesFromDeptCodeField(this SPListItem item, string DeptCodeFieldName)
        {
            dynamic CodeNameValues;
            string[] CodeNames = new String[] { };
            List<string> CodeNamesList = new List<string>();
            string splitPattern = @";#\d+;#";
            string removePattern = @"\d+;#";

            CodeNameValues = item.GetFieldValue(DeptCodeFieldName);

            if (CodeNameValues.GetType().Name == "String")
            {
                if (CodeNameValues == "")
                {
                    return CodeNames;
                }

                CodeNames = Regex.Split(CodeNameValues, splitPattern);
                CodeNames[0] = Regex.Replace(CodeNames[0], removePattern, "");
            }
            if (CodeNameValues.GetType().Name == "SPFieldLookupValueCollection")
            {
                foreach (SPFieldLookupValue CodeValue in CodeNameValues)
                {
                    CodeNamesList.Add(CodeValue.LookupValue);
                    CodeNames = CodeNamesList.ToArray();
                }
            }

            return CodeNames;
        }

        public static List<SPPrincipal> GetGroupsByDeptCodeField(this SPListItem item, string DeptCodeFieldName, string GroupSuffix)
        {
            List<SPPrincipal> codeFieldGroups = new List<SPPrincipal>();
            string[] codeNames;

            if (!item.ParentList.Fields.ContainsField(DeptCodeFieldName))
            {
                return codeFieldGroups;
            }

            codeNames = item.GetCodesFromDeptCodeField(DeptCodeFieldName);

            foreach (string code in codeNames)
            {
                string groupName = code + GroupSuffix;
                SPPrincipal CodeGroup;
                try
                {
                    CodeGroup = item.ParentList.ParentWeb.SiteGroups.GetByName(groupName);
                }
                catch
                {
                    continue;
                }
                codeFieldGroups.Add(CodeGroup);
            }

            return codeFieldGroups;
        }

        public static List<SPPrincipal> GetUsersFromUsersFieldsAfter(this IERItem item, List<string> usersFields)
        {
            return GetUsersFromUsersFieldsByType<IERItem>(usersFields, item);
        }

        public static List<SPPrincipal> GetUsersFromUsersFields(this SPListItem item, List<string> usersFields)
        {
            return GetUsersFromUsersFieldsByType<SPListItem>(usersFields, item);
        }

        private static List<SPPrincipal> GetUsersFromUsersFieldsByType<T>(List<string> usersFields, T itemT)
        {
            List<SPPrincipal> fieldsPrincipals = new List<SPPrincipal>() { };
            string userLogin;
            dynamic fieldValue;
            SPListItem item;

            bool valueAfter;

            Type itemType = typeof(T);
            if (itemType == typeof(IERItem))
            {
                item = ((IERItem)itemT).listItem;
                valueAfter = true;
            }
            else if (itemType == typeof(SPListItem))
            {
                item = itemT as SPListItem;
                valueAfter = false;
            }
            else
            {
                throw new Exception("Not supported item type");
            }


            //if (svcUserForEmptyResponse == null)
            //{
            //    svcUserForEmptyResponse = item.Web.EnsureUser("app@sharepoint");
            //}

            foreach (string fieldTitle in usersFields)
            {
                fieldValue = valueAfter ? ((IERItem)itemT).GetFieldValueAfter(fieldTitle) : item.GetFieldValue(fieldTitle);

                if (fieldValue == null || (fieldValue.GetType().Name == "String" && fieldValue == ""))
                {
                    continue;
                }

                if ((fieldValue.GetType().Name == "Int32") || (fieldValue.GetType().Name == "String" && Regex.IsMatch(fieldValue, @"^\d+$")))
                {
                    SPPrincipal principal = item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(fieldValue.ToString()));
                    fieldsPrincipals.Add(principal);
                }
                else
                {
                    SPFieldUserValueCollection fieldValueUsers = new SPFieldUserValueCollection(item.Web, fieldValue.ToString());
                    foreach (SPFieldUserValue fieldUser in fieldValueUsers)
                    {
                        SPPrincipal principal;
                        if (fieldUser.User != null && fieldUser.User.LoginName != "")
                        {
                            userLogin = fieldUser.User.LoginName;
                        }
                        else
                        {
                            userLogin = fieldUser.LookupValue;
                        }

                        userLogin = userLogin.Substring(userLogin.IndexOf("\\") + 1);

                        try
                        {
                            principal = item.Web.EnsureUser(userLogin);
                        }
                        catch
                        {
                            try
                            {
                                principal = item.ParentList.ParentWeb.SiteGroups.GetByName(userLogin);
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        fieldsPrincipals.Add(principal);
                    }
                }
            }

            //if (fieldsPrincipals.Count == 0)
            //{
            //    fieldsPrincipals.Add(svcUserForEmptyResponse);
            //}

            return fieldsPrincipals;
        }

        //TO ERItem !!
        public static dynamic GetFieldValue(this SPListItem item, string fieldTitle)
        {
            dynamic fieldValue;
            string fieldInternalName;

            try
            {
                fieldInternalName = item.ParentList.Fields[fieldTitle].InternalName;
            }
            catch
            {
                fieldInternalName = fieldTitle;
            }

            fieldValue = item[fieldInternalName];

            if (fieldValue == null)
            {
                fieldValue = String.Empty;
            }

            return fieldValue;
        }

        public static dynamic GetFieldValueAfter(this IERItem item, string fieldTitle)
        {
            dynamic fieldValueAfter;
            string fieldInternalName;
            string fieldStaticName;

            try
            {
                fieldInternalName = item.listItem.ParentList.Fields[fieldTitle].InternalName;
                fieldStaticName = item.listItem.ParentList.Fields[fieldTitle].StaticName;
            }
            catch
            {
                fieldInternalName = fieldTitle;
                fieldStaticName = fieldTitle;
            }

            fieldValueAfter = item.eventProperties.AfterProperties[fieldInternalName];

            if (fieldValueAfter == null)
            {
                fieldValueAfter = item.eventProperties.AfterProperties[fieldTitle];
                if (fieldValueAfter == null)
                {
                    fieldValueAfter = item.eventProperties.AfterProperties[fieldStaticName];
                    if (fieldValueAfter == null)
                    {
                        fieldValueAfter = item.listItem[fieldTitle];
                    }
                }
            }

            if (fieldValueAfter == null)
            {
                fieldValueAfter = String.Empty;
            }

            return fieldValueAfter;
        }

        public static List<SPPrincipal> GetRelatedItemUsers(this SPListItem item)
        {
            List<SPPrincipal> arrRealtedItemUsers = new List<SPPrincipal>();
            dynamic relatedItems;
            try
            {
                relatedItems = item[SPBuiltInFieldId.RelatedItems];
                if (relatedItems == null)
                {
                    relatedItems = item[SPBuiltInFieldId.RelatedItems];
                }
            }
            catch
            {
                relatedItems = null;
            }

            if (relatedItems == null)
            {
                return arrRealtedItemUsers;
            }
            String relatedItemsString = relatedItems.ToString();

            dynamic jsonRelatedItems = JsonConvert.DeserializeObject(relatedItemsString);

            foreach (dynamic relItem in jsonRelatedItems)
            {
                int relatedItemId = (int)relItem["ItemId"];
                string relatedlistIdString = relItem["ListId"];
                Guid relatedlistId = new Guid(relatedlistIdString);

                SPList relatedList = item.Web.Lists[relatedlistId];
                SPListItem relatedItem;
                try
                {
                    relatedItem = relatedList.GetItemById(relatedItemId);
                }
                catch (Exception)
                { 
                    continue; 
                }

                List<string> arrRelatedListUserFields = relatedList.GetListUserFields();

                arrRealtedItemUsers.AddRange(relatedItem.GetUsersFromUsersFields(arrRelatedListUserFields));
                }

            return arrRealtedItemUsers;

        }
        public static string GetItemFullUrl(this SPListItem itemSP)
        {
            string itemFullUrl = itemSP.Web.Site.Url + itemSP.ParentList.DefaultDisplayFormUrl + "?ID=" + itemSP.ID;
            return itemFullUrl;
        }
    }
}
