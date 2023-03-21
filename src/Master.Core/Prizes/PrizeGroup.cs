using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Entity;
using Master.Matches;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Prizes
{
    /// <summary>
    /// 奖项分组
    /// </summary>
    public class PrizeGroup : CreationAuditedEntity<int>, IHaveRemarks, IPassivable
    {
        public virtual int? MatchId { get; set; }
        public virtual Match Match { get; set; }
        public string GroupName { get; set; }
        public virtual string Remarks { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Prize> Prizes { get; set; }
    }
}