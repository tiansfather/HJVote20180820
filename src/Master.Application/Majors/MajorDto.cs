using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Majors
{
    [AutoMap(typeof(Major))]
    public class MajorDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name
        {
            get
            {
                return BriefName;
            }
        }
        public bool IsActive { get; set; }
        public string DisplayName { get; set; }
        public string BriefName { get; set; }
        public string BriefCode { get; set; }
        public int Sort { get; set; }
        public string Remarks { get; set; }
        public string ExtendData1 { get; set; }
        public string ExtendData2 { get; set; }
        public int Depth { get; set; }
    }
}
