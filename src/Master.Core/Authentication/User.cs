using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Entity;
using Master.Majors;
using Master.MultiTenancy;
using Master.Organizations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Authentication
{
    public class User:FullAuditedEntity<long>, IMayHaveTenant,  IPassivable, IExtendableObject,IAutoEntity,IMayHaveOrganization
    {
        public const string AdminUserName= "boss";
        public const string AdminUserPassword = "12345678";
        public virtual int? TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
        public virtual string UserName { get; set; }        
        public virtual string Name { get; set; }
        public virtual string Password { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool IsActive { get; set; } = true;
        [ForeignKey("UserId")]
        public virtual ICollection<UserPermissionSetting> Permissions { get; set; }
        [ForeignKey("UserId")]
        public virtual ICollection<UserRole> Roles { get; set; }
        [ForeignKey("UserId")]
        public virtual ICollection<UserSpeciality> Specialities { get; set; }
        public virtual DateTime? LastLoginTime { get; set; }
        public virtual string ExtensionData { get; set; }
        public virtual User CreatorUser { get; set; }
        public virtual User LastModifierUser { get; set; }
        public virtual User DeleterUser { get; set; }
        public int? OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        /// <summary>
        /// 生成账套管理员用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static User CreateTenantAdminUser(int tenantId)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Password=Abp.Runtime.Security.SimpleStringCipher.Instance.Encrypt(AdminUserPassword)
            };

            return user;
        }

        /// <summary>
        /// 生成主体管理员用户
        /// </summary>
        /// <returns></returns>
        public static User CreateHostAdminUser()
        {
            var user = new User
            {
                TenantId = null,
                UserName = AdminUserName,
                Name = AdminUserName,
                Password = Abp.Runtime.Security.SimpleStringCipher.Instance.Encrypt(AdminUserPassword)
            };

            return user;
        }

    }
    public class UserEntityMapConfiguration : EntityMappingConfiguration<User>
    {
        public override void Map(EntityTypeBuilder<User> b)
        {
            b.HasOne(p => p.DeleterUser)
                .WithMany()
                .HasForeignKey(p => p.DeleterUserId);

            b.HasOne(p => p.CreatorUser)
                .WithMany()
                .HasForeignKey(p => p.CreatorUserId);

            b.HasOne(p => p.LastModifierUser)
                .WithMany()
                .HasForeignKey(p => p.LastModifierUserId);
            b.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId);

            b.HasOne(p => p.Organization)
                .WithMany()
                .HasForeignKey(p => p.OrganizationId);
        }
    }
}
