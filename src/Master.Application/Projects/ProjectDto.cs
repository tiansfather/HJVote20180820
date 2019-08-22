using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Projects
{
    [AutoMap(typeof(Project))]
    public class ProjectDto
    {
        public int Id { get; set; }
        public int MatchInstanceId { get; set; }
        public int PrizeId { get; set; }
        /// <summary>
        /// 对应子专业，仅专业类混排类有效
        /// </summary>
        public int? PrizeSubMajorId { get; set; }
        /// <summary>
        /// 申报序号
        /// </summary>
        public string ReportSN { get; set; }
        public string ProjectName { get; set; }
        public string ProjectSN { get; set; }
        /// <summary>
        /// 是否原创
        /// </summary>
        public bool IsOriginal { get; set; }
        /// <summary>
        /// 设计单位
        /// </summary>
        public int? DesignOrganizationId { get; set; }
        public string DesignOrganizationBriefName { get; set; }
        /// <summary>
        /// 设计单位联系人
        /// </summary>
        public string DesignOrganizationContact { get; set; }
        public string DesignOrganizationMobile { get; set; }
        public string DesignOrganizationPhone { get; set; }
        public string DesignOrganizationEmail { get; set; }
        /// <summary>
        /// 是否有合作单位
        /// </summary>
        public bool HasCoorparation { get; set; }
        public string Coorparation { get; set; }
        /// <summary>
        /// 建设单位
        /// </summary>
        public string BuildingCompany { get; set; }
        public string BuildingCountry { get; set; }
        public string BuildingProvince { get; set; }
        public string BuildingCity { get; set; }

        public ProjectStatus ProjectStatus { get; set; }
        public ProjectSource ProjectSource { get; set; } = ProjectSource.Apply;
        public virtual List<ProjectMajorInfoDto> ProjectMajorInfos { get; set; }
        public List<string> ThirdLevelMajors { get; set; }
    }
}
