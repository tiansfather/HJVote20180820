using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Majors
{
    /// <summary>
    /// 专家专业
    /// </summary>
    public class Speciality : CreationAuditedEntity<int>, IHaveSort, IPassivable
    {
        public bool IsActive { get; set; }
        public int Sort { get; set; }
        public string Name { get; set; }
    }
}
