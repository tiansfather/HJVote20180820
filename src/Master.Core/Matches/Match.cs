using Abp.Domain.Entities;
using Master.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Matches
{
    /// <summary>
    /// 赛事
    /// </summary>
    public class Match:BaseFullEntity
    {
        public virtual string Name { get; set; }
        /// <summary>
        /// 展示名称
        /// </summary>
        public virtual string DisplayName { get; set; }
        public virtual bool IsDisplay { get; set; }
        public virtual ICollection<MatchInstance> MatchInstances { get; set; }
        public virtual ICollection<MatchAward> MatchAwards { get; set; }

    }
    /// <summary>
    /// 赛事+届实例
    /// </summary>
    public class MatchInstance : BaseFullEntity
    {
        public virtual int MatchId { get; set; }
        /// <summary>
        /// 届
        /// </summary>
        public virtual string Identifier { get; set; }
        public virtual int Year { get; set; }
        public virtual Match Match { get; set; }
        public virtual MatchInstanceStatus MatchInstanceStatus { get; set; } = MatchInstanceStatus.Draft;
        public virtual MatchInstanceDisplayScope DisplayScope { get; set; } = MatchInstanceDisplayScope.Normal;
        public virtual MatchInstanceType MatchInstanceType { get; set; } = MatchInstanceType.复杂建筑项目;
        public virtual MatchInstanceDisplayMode MatchInstanceDisplayMode { get; set; } = MatchInstanceDisplayMode.默认;
        [NotMapped]
        public virtual string Name
        {
            get
            {
                return Match.Name + "(" + Identifier + ")";
            }
        }
        [NotMapped]
        public string DataProjectPath
        {
            get
            {
                return this.GetData<string>("DataProjectPath");
            }
            set
            {
                this.SetData("DataProjectPath", value);
            }
        }
        [NotMapped]
        public string DataReviewPath
        {
            get
            {
                return this.GetData<string>("DataReviewPath");
            }
            set
            {
                this.SetData("DataReviewPath", value);
            }
        }
        [NotMapped]
        public string DisplayGroup
        {
            get
            {
                if (this.DisplayScope == MatchInstanceDisplayScope.Other)
                {
                    return "Other";
                }
                if (this.DisplayScope == MatchInstanceDisplayScope.History)
                {
                    return "History";
                }
                return Match.DisplayName;
            }
        }
    }
    public enum MatchInstanceStatus
    {
        /// <summary>
        /// 草稿
        /// </summary>
        Draft=1,
        /// <summary>
        /// 申报中
        /// </summary>
        Applying=2,
        /// <summary>
        /// 评审中
        /// </summary>
        //Reviewing=3,
        /// <summary>
        /// 评选完成
        /// </summary>
        Complete=4
    }
    /// <summary>
    /// 展示位置
    /// </summary>
    public enum MatchInstanceDisplayScope
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal,
        /// <summary>
        /// 其它
        /// </summary>
        Other,
        /// <summary>
        /// 历史
        /// </summary>
        History
    }
    /// <summary>
    /// 项目类型
    /// </summary>
    public enum MatchInstanceType
    {
        复杂建筑项目,
        简单评选项目
    }
    /// <summary>
    /// 项目内容展示方式
    /// </summary>

    public enum MatchInstanceDisplayMode
    {
        默认,
        仅显示名称
    }
}
