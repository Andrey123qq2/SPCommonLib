using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Text.RegularExpressions;

namespace SPSCommon.SPCustomExtensions
{
    public static class SPListExtension
    {
        public static List<string> GetListUserFields(this SPList list)
        {
            List<string> arrListUserFields = new List<string>();

            foreach (SPField fieldSP in list.Fields)
            {
                string fTypeName = fieldSP.Type.ToString();
                string fTitle = fieldSP.Title;
                bool notMatch = !Regex.IsMatch(fTitle, "Editor|PreviouslyAssignedTo");
                if (fTypeName == "User" && notMatch)
                {
                    arrListUserFields.Add(fTitle);
                }
            }

            return arrListUserFields;
        }
    }
}
