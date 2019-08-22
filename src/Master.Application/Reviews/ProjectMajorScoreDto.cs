using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    /// <summary>
    /// 项目的打分明细
    /// </summary>
    public class ProjectMajorScoreDto
    {
        /// <summary>
        /// 提示文本
        /// </summary>
        public string Tip { get; set; }
        public decimal? SubMajorScore { get; set; }
        public string SubMajorName { get; set; }
        public List<ProjectMajorRoundScoreDto> ProjectMajorRoundScoreDtos { get; set; }
    }

    /// <summary>
    /// 项目的每轮分数
    /// </summary>
    public class ProjectMajorRoundScoreDto
    {
        public int ReviewRoundId { get; set; }
        public int Round { get; set; }
        public decimal Score { get; set; }
        public bool HasRateTable { get; set; }
        public bool IsVote { get; set; }
        public List<ProjectMajorRoundExpertScoreDto> ProjectMajorRoundExpertScoreDtos { get; set; }
    }

    /// <summary>
    /// 专家打分明细
    /// </summary>
    public class ProjectMajorRoundExpertScoreDto
    {
        public long ExpertId { get; set; }
        public string ExpertName { get; set; }
        public decimal Score { get; set; }
        public bool IsAvoid { get; set; }
        public bool VoteFlag { get; set; }
        public List<bool?> SubVotes { get; set; }
    }
}
