using Abp.Dependency;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master.Authentication
{
    public static class AbpSessionExtension
    {

        public static bool IsReporter(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.ProjectReporter.Split('|')[0];
        }
        public static bool IsExpert(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.Expert.Split('|')[0];
        }
        public static bool IsMatchManager(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.MatchManager.Split('|')[0];
        }
        public static bool IsGroupManager(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.GroupManager.Split('|')[0];
        }
        public static bool IsSubManager(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.SubManager.Split('|')[0];
        }
        public static bool IsMajorManager(this IAbpSession abpSession)
        {
            return GetRoleName(abpSession) == StaticRoleNames.Host.MajorManager.Split('|')[0];
        }
        public static string GetRoleName(this IAbpSession abpSession)
        {
            var userid = abpSession.UserId;
            return GetClaimValue(AbpClaimTypes.Role);
        }


        private static string GetClaimValue(string claimType)
        {
            using (var principalAccessor = IocManager.Instance.ResolveAsDisposable<IPrincipalAccessor>())
            {
                var claimsPrincipal = principalAccessor.Object.Principal;
                var claim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == claimType);
                if (string.IsNullOrEmpty(claim?.Value))
                    return null;

                return claim.Value;
            }
            

            
        }

    }
}
