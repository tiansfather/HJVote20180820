using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Majors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Authentication
{
    public class UserSpeciality : CreationAuditedEntity<int>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }

        /// <summary>
        /// User id.
        /// </summary>
        public virtual long UserId { get; set; }
        public virtual User User { get; set; }
        public virtual int SpecialityId { get; set; }
        public virtual Speciality Speciality { get; set; }

        public UserSpeciality(int? tenantId, long userId, int specialityId)
        {
            TenantId = tenantId;
            UserId = userId;
            SpecialityId = specialityId;
        }
    }
}
