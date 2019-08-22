using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Majors;
using Master.Matches;
using Master.Prizes;
using Master.Projects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Master.Reviews
{
    /// <summary>
    /// 评审活动
    /// </summary>
    public class Review : CreationAuditedEntity<int>, IExtendableObject
    {
        public virtual string ExtensionData { get; set; }
        public int MatchInstanceId { get; set; }
        public virtual MatchInstance MatchInstance { get; set; }
        public int MajorId { get; set; }
        public virtual Major Major{get;set;}
        public int? SubMajorId { get; set; }
        public virtual ICollection<ReviewRound> ReviewRounds { get; set; }
        /// <summary>
        /// 评选专业名称
        /// </summary>
        public string ReviewMajorName { get; set; }
        /// <summary>
        /// 评选活动名称
        /// </summary>
        public string ReviewName { get; set; }
        public string Remarks { get; set; }
        public ReviewType ReviewType { get; set; } = ReviewType.Initial;
        /// <summary>
        /// 评审状态
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.BeforePublish;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 专家数量
        /// </summary>
        [NotMapped]
        public int ExpertCount
        {
            get
            {
                return this.GetData<List<ReviewExpert>>("Experts").Count;
            }
        }
        /// <summary>
        /// 项目数量
        /// </summary>
        [NotMapped]
        public int ProjectCount
        {
            get
            {
                return this.GetData<List<ReviewProject>>("Projects").Count;
            }
        }
        /// <summary>
        /// 待评审项目
        /// </summary>
        [NotMapped]
        public List<ReviewProject> ReviewProjects
        {
            get
            {
                return this.GetData<List<ReviewProject>>("Projects");
            }
            set
            {
                this.SetData("Projects", value);
            }
        }
        /// <summary>
        /// 参选专家
        /// </summary>
        [NotMapped]
        public List<ReviewExpert> ReviewExperts
        {
            get
            {
                return this.GetData<List<ReviewExpert>>("Experts");
            }
            set
            {
                this.SetData("Experts", value);
            }
        }
        /// <summary>
        /// 当前评审轮次
        /// </summary>
        [NotMapped]
        public ReviewRound CurrentReviewRound
        {
            get
            {
                return ReviewRounds.OrderBy(o => o.Id).LastOrDefault();
            }
        }
        /// <summary>
        /// 当前评审轮
        /// </summary>
        [NotMapped]
        public int CurrentRound
        {
            get
            {
                var currentReviewRound = CurrentReviewRound;
                return currentReviewRound == null ? 0 : currentReviewRound.Round;
            }
        }
        /// <summary>
        /// 当前评审次
        /// </summary>
        [NotMapped]
        public int CurrentTurn
        {
            get
            {
                var currentReviewRound = CurrentReviewRound;
                return currentReviewRound == null ? 0 : currentReviewRound.Turn;
            }
        }
        /// <summary>
        /// 获取评审中项目的回避专家数
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public int GetProjectExcludeExpertCount(Project project)
        {
            var excludeExpertIds = ReviewProjects.First(p => p.Id == project.Id).ExcludeExpertIDs;
            if (!string.IsNullOrEmpty(excludeExpertIds))
            {
                return excludeExpertIds.Split(',').Count();
            }
            return 0;
        }
    }
    /// <summary>
    /// 评审类型
    /// </summary>
    public enum ReviewType
    {
        /// <summary>
        /// 初评
        /// </summary>
        Initial=1,
        /// <summary>
        /// 终评
        /// </summary>
        Finish=2
    }
    /// <summary>
    /// 评审状态
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// 未发布
        /// </summary>
        [Description("未发布")]
        BeforePublish = 0,
        /// <summary>
        /// 评审中
        /// </summary>
        [Description("评审中")]
        Reviewing = 1,
        /// <summary>
        /// 已评审
        /// </summary>
        [Description("已评审")]
        Reviewed = 2
    }
    /// <summary>
    /// 评审活动中的项目
    /// </summary>
    public class ReviewProject
    {
        public int Id { get; set; }
        /// <summary>
        /// 回避专家ids
        /// </summary>
        public string ExcludeExpertIDs { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 基础分
        /// </summary>
        public int? BaseScore { get; set; }
        /// <summary>
        /// 是否被某个专家回避
        /// </summary>
        /// <param name="expertId"></param>
        /// <returns></returns>
        public bool IsAvoidedByExpert(long expertId)
        {
            if (string.IsNullOrEmpty(ExcludeExpertIDs))
            {
                return false;
            }
            else
            {
                var excludeIdsArr = ExcludeExpertIDs.Split(',').Select(o => Convert.ToInt64(o));
                return excludeIdsArr.Contains(expertId);
            }
            
        }
    }
    public class ReviewExpert
    {
        public long Id { get; set; }
    }
}
