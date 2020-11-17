using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace SPERCommonLib
{
    public class ERItem<T> : IERItem, IERConf<T>
    {
        public SPListItem listItem { get; }
        public SPItemEventProperties eventProperties { get; }
        public string itemTitle { get; }
        public string eventType { get; }
        public T ERConf { get; }


        public ERItem(SPItemEventProperties properties, string ListRootFolderConfPropertyName)
        {
            using (SPSite site = new SPSite(properties.WebUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    List<SPPrincipal> fieldsAssignees = new List<SPPrincipal>();

                    try
                    {
                        listItem = web.Lists[properties.ListId].GetItemById(properties.ListItemId);
                    }
                    catch
                    {
                        listItem = properties.ListItem;
                    }

                    if (listItem == null)
                    {
                        throw new ERItemListItemNullException("ERItem ListItem not found");
                    }
                }
            }

            eventProperties = properties;

            itemTitle = (listItem.Title != "" && listItem.Title != null) ? listItem.Title : listItem["FileLeafRef"].ToString();

            eventType = properties.EventType.ToString();

            ERConf = ERListConf<T>.Get(listItem.ParentList, ListRootFolderConfPropertyName);
        }

    }
}