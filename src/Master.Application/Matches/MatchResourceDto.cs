using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Matches
{
    [AutoMap(typeof(MatchResource<>))]
    public class MatchResourceDto<T>
    {
        public int Id { get; set; }
        public virtual int? MatchId { get; set; }
        public virtual int? MatchInstanceId { get; set; }
        public virtual int MajorId { get; set; }
        public virtual int? SubMajorId { get; set; }
        public virtual MatchResourceType MatchResourceType { get; set; }
        public virtual MatchResourceStatus MatchResourceStatus { get; set; }
        public List<T> Datas { get; set; }
    }
}
