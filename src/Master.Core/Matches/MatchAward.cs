using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Matches
{
    public class MatchAward : CreationAuditedEntity<int>
    {
        public int MatchId { get; set; }
        public virtual Match Match { get; set; }
        public string AwardName { get; set; }
        public int AwardRank { get; set; }
    }
}
