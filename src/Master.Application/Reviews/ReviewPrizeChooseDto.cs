using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    /// <summary>
    /// 评审活动选择专业及评审类型 dto
    /// </summary>
    [AutoMap(typeof(Review))]
    public  class ReviewPrizeChooseDto
    {
        public int ReviewId { get; set; }
        public int MatchInstanceId { get; set; }
        public int MajorId { get; set; }
        public int? SubMajorId { get; set; }
        public ReviewType ReviewType { get; set; }
    }
}
