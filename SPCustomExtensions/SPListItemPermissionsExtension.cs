using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Web;
using System.Text.RegularExpressions;


namespace SPERCommonLib
{
    public static class SPListItemPermissionsExtension
    {
        public static List<SPPrincipal> GetAssignmentsPrincipals(SPRoleAssignmentCollection assignments)
        {
            List<SPPrincipal> actualAssignees = new List<SPPrincipal>();

            foreach (SPRoleAssignment assignment in assignments)
            {
                if (Regex.IsMatch(assignment.Member.Name, @"svc_|system"))
                {
                    continue;
                }

                foreach (SPRoleDefinition assignmentBinding in assignment.RoleDefinitionBindings)
                {
                    if (assignmentBinding.Name != "Ограниченный доступ")
                    {
                        actualAssignees.Add(assignment.Member);
                    }
                }
            }
            return actualAssignees;
        }

        public static List<SPPrincipal> ACLGetGroupsByPattern(this SPListItem item, List<string> namesPatterns)
        {
            List<SPPrincipal> itemGroupsByPattern = new List<SPPrincipal>();

            foreach (SPGroup itemGroup in item.RoleAssignments.Groups)
            {
                foreach (string pattern in namesPatterns)
                {
                    if (itemGroup.Name.Contains(pattern))
                    {
                        itemGroupsByPattern.Add(itemGroup);
                    }
                }
            }
            return itemGroupsByPattern;
        }
        public static void RemovePermissions(this SPListItem item, List<SPPrincipal> principalsToRemove)
        {
            foreach (SPPrincipal principal in principalsToRemove)
            {
                if (!item.HasUniqueRoleAssignments)
                {
                    item.BreakRoleInheritance(true);
                }
                item.RoleAssignments.Remove(principal);
            }
        }


        public static void AddPermissions(this SPListItem item, List<SPPrincipal> principals, int roleId)
        {
            SPRoleDefinitionCollection webroledefinitions = item.Web.RoleDefinitions;

            foreach (SPPrincipal principal in principals)
            {
                if (!item.IsPrincipalInItemRole(principal, roleId) && !Regex.IsMatch(principal.Name, @"svc_|system|app@sharepoint"))
                {
                    if (!item.HasUniqueRoleAssignments)
                    {
                        item.BreakRoleInheritance(true);
                    }

                    SPRoleAssignment assignment = new SPRoleAssignment(principal);
                    assignment.RoleDefinitionBindings.Add(webroledefinitions.GetById(roleId));
                    item.RoleAssignments.Add(assignment);
                }
            }
        }

        public static bool IsPrincipalInItemRole(this SPListItem item, SPPrincipal principal, int roleId)
        {
            if (principal.GetType().Name == "SPUser")
            {
                return item.IsUserInItemRole(principal, roleId);
            }

            if (principal.GetType().Name == "SPGroup")
            {
                return item.IsGroupInItemRole(principal, roleId);
            }

            return false;
        }
        public static bool IsGroupInItemRole(this SPListItem item, SPPrincipal group, int roleId)
        {
            SPRoleAssignment AssignmentsOfGroup;
            SPRoleDefinition roleDefinition = item.Web.RoleDefinitions.GetById(roleId);
            try
            {
                AssignmentsOfGroup = item.RoleAssignments.GetAssignmentByPrincipal(group);
            }
            catch
            {
                return false;
            }

            SPRoleDefinitionBindingCollection RoleBindingsOfGroup = AssignmentsOfGroup.RoleDefinitionBindings;


            if (RoleBindingsOfGroup.Contains(roleDefinition))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsUserInItemRole(this SPListItem item, SPPrincipal user, int roleId)
        {
            SPPermissionInfo userEffectivePermissions = item.GetUserEffectivePermissionInfo(user.LoginName);
            SPRoleDefinition roleDefinition = item.Web.RoleDefinitions.GetById(roleId);

            foreach (SPRoleAssignment roleAssignment in userEffectivePermissions.RoleAssignments)
            {
                if (roleAssignment.RoleDefinitionBindings.Contains(roleDefinition))
                    return true;
            }

            return false;
        }

        public static List<SPPrincipal> GetExtraAssignees(this SPListItem item, List<SPPrincipal> principals)
        {
            List<SPPrincipal> extraAssignees = new List<SPPrincipal>();

            List<SPPrincipal> listActualPrincipals = GetAssignmentsPrincipals(item.ParentList.RoleAssignments);
            List<SPPrincipal> itemActualPrincipals = GetAssignmentsPrincipals(item.RoleAssignments);

            List<string> listActualLogins = SPCommon.GetLoginsFromPrincipals(listActualPrincipals);
            List<string> principalsLogins = SPCommon.GetLoginsFromPrincipals(principals);

            foreach (SPPrincipal itemPrincipal in itemActualPrincipals)
            {
                if (!listActualLogins.Contains(itemPrincipal.LoginName) && !principalsLogins.Contains(itemPrincipal.LoginName))
                {
                    extraAssignees.Add(itemPrincipal);
                }
            }

            return extraAssignees;
        }
    }
}
