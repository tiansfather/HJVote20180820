using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Projects
{
    /// <summary>
    /// 项目的评审详情
    /// </summary>
    public class ProjectReviewSummary
    {
        public int Id { get; set; }
        public int Round { get; set; }
        public string ProjectName { get; set; }
        public string PrizeName { get; set; }
        public string SubMajorName { get; set; }
        public string DesignOrganizationName { get; set; }
        public int Sort { get; set; }
        public Boolean NeedConfirm { get; set; }
        /// <summary>
        /// 本轮实际分，如果是加权的则是加权后的分
        /// </summary>
        public decimal Score { get; set; }
        /// <summary>
        /// 本轮打分
        /// </summary>
        public decimal OriScore { get; set; }
        /// <summary>
        /// 用来排序的混合分
        /// </summary>
        public decimal TotalScore { get; set; }
        public int Rank { get; set; }
        public int MaxScore { get; set; }
        public List<int> SubScores { get; set; } = new List<int>();
    }
}
