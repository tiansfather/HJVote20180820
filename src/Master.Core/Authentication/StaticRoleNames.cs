using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Authentication
{
    /// <summary>
    /// 静态角色
    /// </summary>
    public static class StaticRoleNames
    {
        public static class Host
        {
            public const string Admin = "Admin|上帝";
            /// <summary>
            /// 赛事管理员
            /// </summary>
            public const string MatchManager = "MatchManager|赛事管理员";
            /// <summary>
            /// 项目填报人
            /// </summary>
            public const string ProjectReporter = "ProjectReporter|项目填报人";
            /// <summary>
            /// 项目查看人员
            /// </summary>
            public const string ProjectViewer = "ProjectViewer|项目查看人";
            /// <summary>
            /// 分子公司科管
            /// </summary>
            public const string SubManager = "SubManager|分子公司科管";
            /// <summary>
            /// 大专业负责人
            /// </summary>
            public const string MajorManager = "MajorManager|大专业负责人";
            /// <summary>
            /// 集团公司科管
            /// </summary>
            public const string GroupManager = "GroupManager|集团公司科管";
            /// <summary>
            /// 专家
            /// </summary>
            public const string Expert = "Expert|专家";
            /// <summary>
            /// 系统管理员
            /// </summary>
            public const string SystemManager = "SystemManager|系统管理员";
        }

        public static class Tenants
        {
            public const string Admin = "Admin";
        }
    }
}
