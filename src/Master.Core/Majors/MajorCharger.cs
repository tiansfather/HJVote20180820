using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Majors
{
    /// <summary>
    /// 大专业负责人
    /// </summary>
    public class MajorCharger : CreationAuditedEntity<int>
    {
        public int MajorId { get; set; }
        public virtual Major Major { get; set; }
        public long UserId { get; set; }
    }
}
