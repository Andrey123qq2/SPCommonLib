using System;
using Microsoft.SharePoint;
using Newtonsoft.Json;
using System.Collections;

namespace SPSCommon.SPJsonConf
{
    public class SPJsonConf<T>
    {
        public static T Get(SPList List, string RootFolderPropertyName)
        {
            string RootFolderPropertyValue;
            T ERConfByType;

            Hashtable ListRootFolderProperties = List.RootFolder.Properties;

            if (!ListRootFolderProperties.ContainsKey(RootFolderPropertyName))
            {
                Type ERType = typeof(T);
                ERConfByType = (T)Activator.CreateInstance(ERType);
            }
            else
            {
                RootFolderPropertyValue = ListRootFolderProperties[RootFolderPropertyName].ToString();
                ERConfByType = JsonConvert.DeserializeObject<T>(RootFolderPropertyValue);
            }

            return ERConfByType;
        }
        public static void Set(SPList List, string RootFolderPropertyName, T RootFolderPropertyValue)
        {
            string RootFolderPropertyValueString = JsonConvert.SerializeObject(RootFolderPropertyValue, Formatting.Indented);
            List.RootFolder.Properties[RootFolderPropertyName] = RootFolderPropertyValueString;
            List.Update();
        }
    }
}
