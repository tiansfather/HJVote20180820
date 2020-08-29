using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Master.Auditing;
using Master.Authentication;
using Master.Configuration;
using Master.Entity;
using Master.EntityFrameworkCore.Repositories;
using Master.Majors;
using Master.Matches;
using Master.MultiTenancy;
using Master.Notices;
using Master.Organizations;
using Master.Prizes;
using Master.Projects;
using Master.Reviews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master.EntityFrameworkCore
{
    [AutoRepositoryTypes(
    typeof(IRepository<>),
    typeof(IRepository<,>),
    typeof(MasterRepositoryBase<>),
    typeof(MasterRepositoryBase<,>)
    )]
    public class MasterDbContext : AbpDbContext
    {
        #region Entities
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserSpeciality> UserSpecialities { get; set; }
        public virtual DbSet<UserLoginAttempt> UserLoginAttempts { get; set; }
        public virtual DbSet<PermissionSetting> Permissions { get; set; }
        public virtual DbSet<RolePermissionSetting> RolePermissions { get; set; }
        public virtual DbSet<UserPermissionSetting> UserPermissions { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<Match> Matches { get; set; }
        public virtual DbSet<MatchAward> MatchAwards { get; set; }
        public virtual DbSet<MatchInstance> MatchInstances { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<Speciality> Specialities { get; set; }
        public virtual DbSet<MajorExpert> MajorExperts { get; set; }
        public virtual DbSet<MajorCharger> MajorChargers { get; set; }
        public virtual DbSet<MatchResource> MatchResources { get; set; }
        public virtual DbSet<Prize> Prizes { get; set; }
        public virtual DbSet<PrizeSubMajor> PrizeSubMajors  {get;set;}
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectMajorInfo> ProjectMajorInfos { get; set; }
        public virtual DbSet<ProjectTraceLog> ProjectTraceLogs { get; set; }
        public virtual DbSet<Notice> Notices { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<ReviewRound> ReviewRounds { get; set; }
        #endregion

        public MasterDbContext(DbContextOptions<MasterDbContext> options) 
            : base(options)
        {

        }

        //#region DbFunction
        //[DbFunction(FunctionName = "json_extract")]
        //public static string GetJsonValueString(JsonObject<IDictionary<string, object>> obj, string PropertyPath)
        //{
        //    return "";
        //}
        //[DbFunction(FunctionName = "json_extract")]
        //public static decimal GetJsonValueNumber(JsonObject<IDictionary<string, object>> obj, string PropertyPath)
        //{
        //    return 0;
        //}
        //[DbFunction(FunctionName = "json_extract")]
        //public static DateTime GetJsonValueDate(JsonObject<IDictionary<string, object>> obj, string PropertyPath)
        //{
        //    return DateTime.Now;
        //}
        //[DbFunction(FunctionName = "json_extract")]
        //public static bool GetJsonValueBool(JsonObject<IDictionary<string, object>> obj, string PropertyPath)
        //{
        //    return true;
        //}
        //#endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ///动态加入实体
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes().Where(o => typeof(IAutoEntity).IsAssignableFrom(o) && o.IsClass && !o.IsAbstract))
                {
                    modelBuilder.Model.GetOrAddEntityType(type);
                }
                //通过反射加入实体配置
                modelBuilder.AddEntityConfigurationsFromAssembly(asm);
            }

            base.OnModelCreating(modelBuilder);

            
        }
    }
}
