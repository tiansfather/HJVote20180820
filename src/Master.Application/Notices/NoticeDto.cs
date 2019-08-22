using Abp.AutoMapper;
using Master.Projects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Notices
{
    [AutoMap(typeof(Notice))]
    public class NoticeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NoticeStatus NoticeStatus { get; set; }
        public List<ProjectFile> Datas { get; set; }
    }
}
