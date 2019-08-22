using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    [AutoMap(typeof(ReviewRound))]
    public class ReviewRoundDto
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string ReviewName { get; set; }
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
        public ReviewMethodSetting ReviewMethodSetting { get; set; }
        public bool HasRateTable { get; set; }
        /// <summary>
        /// 参选项目
        /// </summary>
        public string SourceProjectIds { get; set; }
    }
}
