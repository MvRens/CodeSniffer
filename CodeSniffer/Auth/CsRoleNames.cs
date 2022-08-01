using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSniffer.Auth
{
    public static class CsRoleNames
    {
        [RoleInfo(0, "Developer")]
        public const string Developer = "developer";

        [RoleInfo(1, "Admin")]
        public const string Admin = "admin";


        public static IEnumerable<RoleInfo> Info { get; }


        static CsRoleNames()
        {
            Info = typeof(CsRoleNames)
                .GetFields()
                .Select(f => 
                {
                    var roleInfoAttribute = f.GetCustomAttribute<RoleInfoAttribute>();
                    if (roleInfoAttribute == null)
                        return null;

                    return new RoleInfo(
                        (string)f.GetRawConstantValue()!, 
                        roleInfoAttribute.DisplayName,
                        roleInfoAttribute.DisplayOrder);
                })
                .Where(r => r != null)
                .Cast<RoleInfo>()
                .OrderBy(r => r.DisplayOrder)
                .ToList();
        }
    }



    public class RoleInfo
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int DisplayOrder { get; }


        public RoleInfo(string id, string displayName, int displayOrder)
        {
            Id = id;
            DisplayName = displayName;
            DisplayOrder = displayOrder;
        }
    }


    public class RoleInfoAttribute : Attribute
    {
        public int DisplayOrder { get; }
        public string DisplayName { get; }


        public RoleInfoAttribute(int displayOrder, string displayName)
        {
            DisplayOrder = displayOrder;
            DisplayName = displayName;
        }
    }
}
