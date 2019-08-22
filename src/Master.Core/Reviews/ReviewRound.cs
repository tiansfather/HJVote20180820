using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Reviews
{
    /// <summary>
    /// 评审轮次
    /// </summary>
    public class ReviewRound : CreationAuditedEntity<int>, IExtendableObject
    {
        public virtual string ExtensionData { get; set; }
        public int ReviewId { get; set; }
        public virtual Review Review { get; set; }
        /// <summary>
        /// 轮
        /// </summary>
        public int Round { get; set; }
        /// <summary>
        /// 次
        /// </summary>
        public int Turn { get; set; }
        /// <summary>
        /// 目标数量
        /// </summary>
        public int TargetNumber { get; set; }
        /// <summary>
        /// 评审方式
        /// </summary>
        public ReviewMethod ReviewMethod { get; set; }
        /// <summary>
        /// 评审状态
        /// </summary>
        public ReviewStatus ReviewStatus { get; set; }
        /// <summary>
        /// 参选项目
        /// </summary>
        public string SourceProjectIDs { get; set; }
        /// <summary>
        /// 评选结果项目
        /// </summary>
        public string ResultProjectIDs { get; set; }
        /// <summary>
        /// 评审参数设定
        /// </summary>
        [NotMapped]
        public ReviewMethodSetting ReviewMethodSetting
        {
            get
            {
                return this.GetData<ReviewMethodSetting>("ReviewMethodSetting");
            }
            set
            {
                this.SetData("ReviewMethodSetting", value);
            }
        }
        /// <summary>
        /// 专家打分明细
        /// </summary>
        [NotMapped]
        public List<ExpertReviewDetail> ExpertReviewDetails
        {
            get
            {
                var result= this.GetData<List<ExpertReviewDetail>>("ExpertReviewDetails");
                if (result == null)
                {
                    result= new List<ExpertReviewDetail>();
                }
                return result;
            }
            set
            {
                this.SetData("ExpertReviewDetails", value);
            }
        }
        /// <summary>
        /// 轮的中文显示
        /// </summary>
        [NotMapped]
        public string RoundC
        {
            get
            {
                return Common.Fun.NumberToChinese(Round);
            }
        }
    }

    /// <summary>
    /// 评审方法
    /// </summary>
    public enum ReviewMethod
    {
        /// <summary>
        /// 本轮平均
        /// </summary>
        [Description("本轮平均")]
        Average = 1,
        /// <summary>
        /// 与上轮加权
        /// </summary>
        [Description("与上轮加权")]
        Weighting = 2,
        /// <summary>
        /// 投票
        /// </summary>
        [Description("投票")]
        Vote = 3
    }
    public enum RateType
    {
        /// <summary>
        /// 直接打分
        /// </summary>
        Score=0,
        /// <summary>
        /// 评分表
        /// </summary>
        RateTable=1
    }
    /// <summary>
    /// 评审参数设置
    /// </summary>
    public class ReviewMethodSetting
    {

        public int MaxScore { get; set; }
        public int MinScore { get; set; }
        public decimal ScoreStep { get; set; }
        /// <summary>
        /// 上轮权重
        /// </summary>
        public decimal WeightLast { get; set; }
        /// <summary>
        /// 本轮权重
        /// </summary>
        public decimal WeightNow { get; set; }
        /// <summary>
        /// 票数
        /// </summary>
        public int VoteNumber { get; set; }
        /// <summary>
        /// 去掉最高最低分后的平均
        /// </summary>
        public Boolean CutOff { get; set; }
        public RateType RateType { get; set; }
        
    }

    /// <summary>
    /// 投票打分明细
    /// </summary>
    public class ProjectReviewDetail
    {
        public long ExpertId { get; set; }
        public int ProjectId { get; set; }
        /// <summary>
        /// 打分
        /// </summary>
        public decimal? Score { get; set; }
        /// <summary>
        /// 投票标记
        /// </summary>
        public Boolean VoteFlag { get; set; } = false;
        /// <summary>
        /// 回避标记
        /// </summary>
        public Boolean IsAvoid { get; set; } = false;

        public List<RateTableDetail> RateTableDetails { get; set; } = new List<RateTableDetail>();
    }

    public class RateTableDetail
    {
        public int Sort { get; set; }
        public decimal Score { get; set; }
    }
    /// <summary>
    /// 专家投票明细
    /// </summary>
    public class ExpertReviewDetail
    {
        public long ExpertID { get; set; }
        /// <summary>
        /// 投票时间
        /// </summary>
        public DateTime? FinishTime { get; set; }
        public List<ProjectReviewDetail> ProjectReviewDetails { get; set; }
    }
}
