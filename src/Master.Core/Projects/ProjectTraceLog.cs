using Abp.Domain.Entities.Auditing;
using Master.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Projects
{
    /// <summary>
    /// 项目流转记录
    /// </summary>
    public class ProjectTraceLog : CreationAuditedEntity<int>
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        /// <summary>
        /// 提交后的项目状态
        /// </summary>
        public ProjectStatus TargetStatus { get; set; }
        /// <summary>
        /// 提交动作
        /// </summary>
        public string ActionName { get; set; }
        public virtual User CreatorUser { get; set; }
        public string Remarks { get; set; }
    }
}
