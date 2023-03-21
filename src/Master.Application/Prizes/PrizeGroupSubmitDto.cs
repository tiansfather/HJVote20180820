using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Prizes
{
    [AutoMap(typeof(PrizeGroup))]
    public class PrizeGroupSubmitDto
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string GroupName { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }

        public List<PrizeGroupSubmitPrizeDto> SubPrizes { get; set; }
    }

    public class PrizeGroupSubmitPrizeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
}