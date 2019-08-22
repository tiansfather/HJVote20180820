using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Notices
{
    public class Notice : AuditedEntity<int>, IExtendableObject
    {
        public virtual string ExtensionData { get; set; }
        public NoticeStatus NoticeStatus { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }
    }

    public enum NoticeStatus
    {
        Draft=1,
        Publish=2
    }
}
