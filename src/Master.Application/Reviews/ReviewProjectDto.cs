using Abp.AutoMapper;
using Master.Projects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Reviews
{
    [AutoMap(typeof(ReviewProject),typeof(Project))]
    public class ReviewProjectDto
    {
        public int Id { get; set; }
        public int Sort { get; set; }
        public int? BaseScore { get; set; }
        public string ExcludeExpertIDs { get; set; }
        public string ProjectName { get; set; }
        public string DesignOrganizationName { get; set; }
        /// <summary>
        /// 奖项大类
        /// </summary>
        public string PrizeName { get; set; }
        /// <summary>
        ///  奖项子类
        /// </summary>
        public string SubMajorName { get; set; }
    }
}
