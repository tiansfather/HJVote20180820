using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Majors
{
    public class MajorExpert:CreationAuditedEntity<int>
    {
        public int MajorId { get; set; }
        public long UserId { get; set; }
        public MajorExpertRank MajorExpertRank { get; set; } = MajorExpertRank.Senior;
    }

    public enum MajorExpertRank
    {
        /// <summary>
        /// 高级
        /// </summary>
        Senior=1,
        /// <summary>
        /// 资深
        /// </summary>
        Expert =2
    }
}
