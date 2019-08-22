using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    [AutoMap(typeof(Review))]
    public class ReviewDto
    {
        public int Id { get; set; }
        public string ReviewName { get; set; }
        public int CurrentRound { get; set; }
        public int CurrentTurn { get; set; }
        public string CurrentRoundC { get; set; }
    }
}
