using Abp.AutoMapper;
using Master.Authentication;
using Master.Majors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    [AutoMap(typeof(User))]
    public class ReviewExpertDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<MajorExpertDto> MajorExperts { get; set; }
        public List<string> Specialities { get; set; }
        public string OrganizationDisplayName { get; set; }
        public string Remarks { get; set; }
    }
    public class MajorExpertDto
    {
        public string MajorName { get; set; }
        public MajorExpertRank Rank { get; set; }
    }
}
