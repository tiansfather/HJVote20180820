using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Master.Authentication;
using Master.Matches;
using Master.Organizations;
using Master.Prizes;
using Master.Reviews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master.Projects
{
    /// <summary>
    /// 评审项目
    /// </summary>
    public class Project : CreationAuditedEntity<int>, IExtendableObject
    {
        public virtual User CreatorUser { get; set; }
        public virtual string ExtensionData { get; set; }
        public int MatchInstanceId { get; set; }
        public virtual MatchInstance MatchInstance { get; set; }
        public int PrizeId { get; set; }
        public virtual Prize Prize { get; set; }
        /// <summary>
        /// 对应子专业，仅专业类混排类有效
        /// </summary>
        public int? PrizeSubMajorId { get; set; }
        /// <summary>
        /// 对应子专业,仅专业类混排类有效
        /// </summary>
        public virtual PrizeSubMajor PrizeSubMajor { get; set; }
        /// <summary>
        /// 申报序号
        /// </summary>
        public string ReportSN { get; set; }
        public string ProjectName { get; set; }
        public string ProjectSN { get; set; }
        /// <summary>
        /// 是否原创
        /// </summary>
        public bool IsOriginal { get; set; }
        /// <summary>
        /// 设计单位
        /// </summary>
        public int? DesignOrganizationId { get; set; }
        [ForeignKey("DesignOrganizationId")]
        public virtual Organization DesignOrganization { get; set; }
        /// <summary>
        /// 设计单位联系人
        /// </summary>
        public string DesignOrganizationContact { get; set; }
        public string DesignOrganizationMobile { get; set; }
        public string DesignOrganizationPhone { get; set; }
        public string DesignOrganizationEmail { get; set; }
        /// <summary>
        /// 是否有合作单位
        /// </summary>
        public bool HasCoorparation { get; set; }
        public string Coorparation { get; set; }
        /// <summary>
        /// 建设单位
        /// </summary>
        public string BuildingCompany { get; set; }
        public string BuildingCountry { get; set; }
        public string BuildingProvince { get; set; }
        public string BuildingCity { get; set; }

        public ProjectStatus ProjectStatus { get; set; }
        public ProjectSource ProjectSource { get; set; } = ProjectSource.Apply;
        /// <summary>
        /// 终评标志
        /// </summary>
        public bool IsInFinalReview { get; set; } = false;
        /// <summary>
        /// 决赛标志
        /// </summary>
        public bool IsInChampionReview { get; set; } = false;
        /// <summary>
        /// 初评分数
        /// </summary>
        public decimal? ScoreInitial { get; set; }
        /// <summary>
        /// 终评分数
        /// </summary>
        public decimal? ScoreFinal { get; set; }
        /// <summary>
        /// 决赛分数
        /// </summary>
        public decimal? ScoreChampion { get; set; }
        /// <summary>
        /// 奖项内初评排名
        /// </summary>
        public int? RankInitial { get; set; }
        /// <summary>
        /// 奖项内终评排名
        /// </summary>
        public int? RankFinal { get; set; }
        /// <summary>
        /// 决赛排名
        /// </summary>
        public int? RankChampion { get; set; }

        public virtual ICollection<ProjectMajorInfo> ProjectMajorInfos { get; set; }
        public virtual ICollection<ProjectTraceLog> ProjectTraceLogs { get; set; }
        #region 跨赛事相关
        /// <summary>
        /// 跨赛事关联项目
        /// </summary>
        public int? CrossProjectId { get; set; }
        public virtual Project CrossProject { get; set; }
        #endregion

        /// <summary>
        /// 评审序号
        /// </summary>
        [NotMapped]
        public int? ReviewSort
        {
            get
            {
                return this.GetData<int?>("ReviewSort");
            }
            set
            {
                this.SetData("ReviewSort", value);
            }
        }
        [NotMapped]
        public string IsOriginalStr
        {
            get
            {
                return IsOriginal ? "是" : "否";
            }
        }
        [NotMapped]
        public string ProjectStatusStr
        {
            get
            {
                string result = "";
                switch (ProjectStatus)
                {
                    case ProjectStatus.Reject:
                        result = "退回";
                        break;
                    case ProjectStatus.Draft:
                        result = "草稿";
                        break;
                    case ProjectStatus.UnderPreliminaryVerify:
                        result = "待初审";
                        break;
                    case ProjectStatus.UnderMajorVerify:
                        result = "待专业鉴定";
                        break;
                    case ProjectStatus.UnderFinalVerify:
                        result = "待终审";
                        break;
                    case ProjectStatus.UnderReview:
                        result = "待评选";
                        break;
                    case ProjectStatus.Reviewing:
                        result = "初评中";
                        break;
                    case ProjectStatus.FinalReviewing:
                        result = "终评中";
                        break;
                    case ProjectStatus.Complete:
                        result = "已结束";
                        break;
                }

                return result;
            }
        }
        public decimal? GetScoreForReviewType(ReviewType reviewType)
        {
            decimal? score = null;
            switch (reviewType)
            {
                case ReviewType.Initial:
                    score = ScoreInitial;
                    break;
                case ReviewType.Finish:
                    score = ScoreFinal;
                    break;
                case ReviewType.Champion:
                    score = ScoreChampion;
                    break;
            }
            return score;
        }
    }
    /// <summary>
    /// 项目专业信息
    /// </summary>
    public class ProjectMajorInfo : CreationAuditedEntity<int>, IExtendableObject
    {
        public virtual string ExtensionData { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public int? MajorId { get; set; }
        /// <summary>
        /// 针对专业的初评打分
        /// </summary>
        public decimal? ScoreInitial { get; set; }
        public decimal? ScoreFinal { get; set; }
    }
    /// <summary>
    /// 项目上传文件
    /// </summary>
    public class ProjectFile
    {
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
    public enum ProjectSource
    {
        /// <summary>
        ///  申报
        /// </summary>
        Apply=1,
        /// <summary>
        ///  导入 
        /// </summary>
        Import=2,
        /// <summary>
        /// 跨赛事
        /// </summary>
        CrossMatch=3,
    }
    public enum ProjectStatus
    {
        /// <summary>
        /// 退回
        /// </summary>
        Reject=-1,
        /// <summary>
        /// 草稿
        /// </summary>
        Draft=0,
        /// <summary>
        /// 待初审
        /// </summary>
        UnderPreliminaryVerify=1,
        /// <summary>
        /// 待专业鉴定
        /// </summary>
        UnderMajorVerify=2,
        /// <summary>
        /// 待终审
        /// </summary>
        UnderFinalVerify=3,
        /// <summary>
        /// 待评选
        /// </summary>
        UnderReview=4,
        /// <summary>
        /// 初评中
        /// </summary>
        Reviewing=5,
        /// <summary>
        /// 终评中
        /// </summary>
        FinalReviewing=6,
        /// <summary>
        /// 结束
        /// </summary>
        Complete=7,
        /// <summary>
        /// 决赛中
        /// </summary>
        ChampionReviewing=8
    }
}
