using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    [AutoMap(typeof(ProjectReviewDetail))]
    public class ProjectReviewDetailDto
    {
        public int ProjectId { get; set; }
        /// <summary>
        /// 打分,此处用string类型，用于接收前台回避后提交的空数据
        /// </summary>
        public string Score { get; set; }
        /// <summary>
        /// 投票标记
        /// </summary>
        public Boolean VoteFlag { get; set; } = false;
        /// <summary>
        /// 回避标记
        /// </summary>
        public Boolean IsAvoid { get; set; } = false;
    }
}
