using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Prizes
{
    [AutoMap(typeof(Prize))]
    public class PrizeSubmitDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int MajorId { get; set; }
        public PrizeType PrizeType { get; set; }
        public bool IsActive { get; set; }
        public string PrizeName { get; set; }
        public string Remarks { get; set; }

        public List<PrizeSubmitSubMajorDto> SubMajors { get; set; }
    }
    public class PrizeSubmitSubMajorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Percent { get; set; }
        public bool Checked { get; set; }
        public decimal? Ratio { get; set; }
    }
}
