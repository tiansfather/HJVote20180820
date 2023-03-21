using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Entity;
using Master.Majors;
using Master.Matches;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Prizes
{
    /// <summary>
    /// 奖项
    /// </summary>
    public class Prize : CreationAuditedEntity<int>, IExtendableObject, IHaveRemarks, IPassivable
    {
        public virtual int? MatchId { get; set; }
        public virtual Match Match { get; set; }
        public virtual int? MatchInstanceId { get; set; }
        public virtual MatchInstance MatchInstance { get; set; }
        public virtual int? PrizeGroupId { get; set; }
        public virtual PrizeGroup PrizeGroup { get; set; }
        public virtual string ExtensionData { get; set; }
        public virtual string Remarks { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// 奖项名称
        /// </summary>
        public virtual string PrizeName { get; set; }

        public virtual PrizeType PrizeType { get; set; }

        /// <summary>
        /// 奖项专业大类ｉｄ
        /// </summary>
        public virtual int MajorId { get; set; }

        /// <summary>
        /// 专业大类
        /// </summary>
        public virtual Major Major { get; set; }

        public virtual ICollection<PrizeSubMajor> PrizeSubMajors { get; set; }
    }

    /// <summary>
    /// 奖项子专业
    /// </summary>
    public class PrizeSubMajor : CreationAuditedEntity<int>
    {
        public virtual int PrizeId { get; set; }
        public virtual Prize Prize { get; set; }
        public virtual int MajorId { get; set; }
        public virtual Major Major { get; set; }
        public virtual int? Percent { get; set; }
        public virtual bool Checked { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public virtual decimal? Ratio { get; set; }
    }

    public class PrizeSubMajorEntityMapConfiguration : EntityMappingConfiguration<PrizeSubMajor>
    {
        public override void Map(EntityTypeBuilder<PrizeSubMajor> b)
        {
            b.HasOne<Major>(o => o.Major).WithMany().HasForeignKey(o => o.MajorId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
        }
    }

    public enum PrizeType
    {
        /// <summary>
        /// 综合类
        /// </summary>
        Multiple = 1,

        /// <summary>
        /// 专业类
        /// </summary>
        Major = 2,

        /// <summary>
        /// 混排类
        /// </summary>
        Mixed = 3,

        /// <summary>
        /// 基本类
        /// </summary>
        Base = 4,

        /// <summary>
        /// 复选类
        /// </summary>
        MultiCheck = 5,
    }
}