using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    public class ReviewUpdateDto
    {
        public int ReviewId { get; set; }
        public string ReviewName { get; set; }
        public List<ReviewExpertDto> Experts { get; set; }
        public List<ReviewProjectDto> Projects { get; set; }
    }
}
