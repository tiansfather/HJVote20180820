using Abp.Web.Models;
using Abp.Runtime.Caching;
using Master.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Entities;
using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using Abp.Extensions;
using Master.Organizations;
using Master.Reviews;
using Master.Majors;
using Master.Matches;
using Master.Prizes;
using System.Linq.Dynamic.Core;

namespace Master.Projects
{
    public class ProjectAppService : MasterAppServiceBase<Project, int>
    {
        public IRepository<Review,int> ReviewRepository { get; set; }
        public IRepository<Major,int> MajorRepository { get; set; }
        public ProjectManager ProjectManager { get; set; }
        public IRepository<ProjectMajorInfo,int> ProjectMajorInfoRepository { get; set; }
        public IRepository<ProjectTraceLog,int> ProjectTraceLogRepository { get; set; }
        /// <summary>
        /// 分页返回
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DontWrapResult]
        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            var data=await pageResult.Queryable
                .Include(o=>o.DesignOrganization)
                .Include(o=>o.PrizeSubMajor).ThenInclude(o=>o.Major)
                .Include(o=>o.ProjectMajorInfos)
                .Include(o=>o.Prize).ThenInclude(o=>o.Major)
                .Include(o=>o.CreatorUser).ThenInclude(o=>o.Organization)
                .Select(o => new {
                    o.Id,
                    o.ProjectName,
                    o.PrizeId,
                    o.ReportSN,
                    o.Prize.PrizeName,
                    MajorName=o.Prize.Major.BriefName,
                    SubMajorId=o.PrizeSubMajor!=null?o.PrizeSubMajor.MajorId.ToString():"",
                    SubMajorName =o.PrizeSubMajor!=null?o.PrizeSubMajor.Major.BriefName:"-",
                    DesignOrganizationName= o.DesignOrganization!=null?o.DesignOrganization.BriefName:"",
                    IsOriginal=o.IsOriginalStr,
                    o.CreatorUser.Organization.BriefName,
                    o.BuildingCompany,
                    ProjectStatus=o.ProjectStatusStr,
                    CreatorOrganizationName=o.CreatorUser.Organization!=null? o.CreatorUser.Organization.BriefName:"",
                    o.ProjectSource,
                    o.ProjectSN,
                    o.CreatorUser.Name,
                    BuildingType=GetBuildingType(o),
                    Coorperation=GetCoorperation(o)
                }).ToListAsync();            

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }
        private string GetCoorperation(Project project)
        {
            var coorperations = new List<string>();
            coorperations.Add(project.Coorparation);
            coorperations.AddRange(project.ProjectMajorInfos.Select(o => o.GetData<string>("Coorperation")).ToList());
            coorperations.RemoveAll(o => string.IsNullOrEmpty(o));
            return string.Join(',', coorperations);
        }
        /// <summary>
        /// 获取建筑类别
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private string GetBuildingType(Project project)
        {
            var majorInfo = project.ProjectMajorInfos.Where(o => o.MajorId == null).SingleOrDefault();
            if (majorInfo == null)
            {
                return "";
            }
            else
            {
                var layouts = majorInfo.GetData<List<MatchResourceFormDesignItem>>("Layouts");
                var allControls = new List<MatchResourceFormDesignItem>();
                allControls.AddRange(layouts);
                foreach(var item in layouts)
                {
                    allControls.AddRange(GetChildren(item));
                }
                var control = allControls.Where(o => o.Id== "1536590402055720" || o.FormName== "建筑类别下拉").FirstOrDefault();
                return control != null ? control.Value : "";
            }
        }
        private List<MatchResourceFormDesignItem> GetChildren(MatchResourceFormDesignItem item)
        {
            var result = new List<MatchResourceFormDesignItem>();
            foreach(var subItem in item.Children)
            {
                result.Add(subItem);
                result.AddRange(GetChildren(subItem));
            }
            
            return result;
        }
        /// <summary>
        ///获取单个项目数据
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public virtual async Task<ProjectDto> GetProject(int projectId)
        {
            var project =await Repository.GetAllIncluding(o => o.ProjectMajorInfos, o => o.ProjectTraceLogs,o=>o.DesignOrganization,o=>o.ProjectTraceLogs).Where(o => o.Id == projectId).SingleAsync();

            var projectDto = project.MapTo<ProjectDto>();
            //三级专业数据另外存储
            projectDto.ThirdLevelMajors = project.GetData<List<string>>("ThirdLevelMajors");
            foreach (var majorInfoDto in projectDto.ProjectMajorInfos)
            {
                var majorInfo = project.ProjectMajorInfos.Single(o => o.MajorId == majorInfoDto.MajorId);
                majorInfoDto.SyncFrom(majorInfo);
            }

            return projectDto;
        }
        /// <summary>
        /// 提交项目
        /// </summary>
        /// <param name="projectDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitProject(ProjectDto projectDto)
        {
            Project project = null;
            if (projectDto.Id == 0)
            {
                project = projectDto.MapTo<Project>();
                //第三级专业另外存储
                project.SetData("ThirdLevelMajors", projectDto.ThirdLevelMajors);
                foreach(var majorInfoDto in projectDto.ProjectMajorInfos)
                {
                    var majorInfo = project.ProjectMajorInfos.Single(o => o.MajorId == majorInfoDto.MajorId);
                    majorInfoDto.SyncTo(majorInfo);
                }
                await ProjectManager.InsertAsync(project);
            }
            else
            {
                await ProjectMajorInfoRepository.DeleteAsync(o => o.ProjectId == projectDto.Id);
                //return;
                //await CurrentUnitOfWork.SaveChangesAsync();
                project = await Repository.GetAsync(projectDto.Id);
                var fromStatus = project.ProjectStatus;
                projectDto.MapTo(project);
                //如果是导入做的修改，项目状态保留原来状态
                if(fromStatus==ProjectStatus.UnderReview || fromStatus == ProjectStatus.Reviewing)
                {
                    project.ProjectStatus = fromStatus;
                }
                //modi:20181012如果是退回状态的暂存，保留退回状态
                if(fromStatus==ProjectStatus.Reject && project.ProjectStatus == ProjectStatus.Draft)
                {
                    project.ProjectStatus = fromStatus;
                }
                //第三级专业另外存储
                project.SetData("ThirdLevelMajors", projectDto.ThirdLevelMajors);
                project.ProjectMajorInfos.Clear();
                await ProjectManager.UpdateAsync(project);
                
                //await ProjectManager.ChangeProjectStatus(project.Id, fromStatus, project.ProjectStatus);
                await ProjectMajorInfoRepository.DeleteAsync(o => o.ProjectId == projectDto.Id);
                foreach (var majorInfoDto in projectDto.ProjectMajorInfos)
                {
                    var majorInfo = new ProjectMajorInfo()
                    {
                        ProjectId = project.Id,
                        MajorId = majorInfoDto.MajorId
                    };
                    majorInfoDto.SyncTo(majorInfo);

                    await ProjectMajorInfoRepository.InsertAsync(majorInfo);
                }
                
            }
            

            //项目状态变更
            if (project.ProjectStatus != ProjectStatus.Draft && project.ProjectStatus != ProjectStatus.Reject && project.ProjectSource!=ProjectSource.Import)
            {
                await ProjectManager.TraceLog(project.Id, "提交申报", project.ProjectStatus);
            }
            
        }

        /// <summary>
        /// 初审、终审提交
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="targetStatus"></param>
        /// <param name="reviewMsg"></param>
        /// <param name="isVerify"></param>
        /// <returns></returns>
        public virtual async Task Verify(int projectId, ProjectStatus targetStatus,string reviewMsg,bool isVerify)
        {
            var project = await Repository.GetAsync(projectId);
            string actionName="";
            if (!isVerify)
            {
                targetStatus = ProjectStatus.Reject;
                actionName = "退回";
            }
            else
            {
                //targetStatus = reviewType == 1 ? ProjectStatus.UnderFinalVerify : ProjectStatus.UnderReview;
                if (targetStatus == ProjectStatus.UnderMajorVerify)
                {
                    actionName = "初审通过";
                }else if (targetStatus == ProjectStatus.UnderFinalVerify)
                {
                    actionName = "专业鉴定通过";
                }else if (targetStatus == ProjectStatus.UnderReview)
                {
                    actionName = "审批通过";
                }
            }
            project.ProjectStatus = targetStatus;
            //记录日志
            await ProjectManager.TraceLog(projectId, actionName, targetStatus, reviewMsg);

            
        }

        /// <summary>
        /// 将reviewproject转换为reviewprojectdto,用于评选活动中展示及选择项目
        /// </summary>
        /// <param name="reviewProjects"></param>
        /// <returns></returns>
        public virtual async Task<List<ReviewProjectDto>> ProjectToReviewProjectDtos(List<ReviewProject> reviewProjects)
        {
            Logger.Error("1:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            List<ReviewProjectDto> reviewProjectDtos = new List<ReviewProjectDto>();
            var allProjects = await ProjectManager.GetReviewProjectsByCache(reviewProjects);
            /*var allProjects = await Repository.GetAllIncluding(o => o.DesignOrganization).Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major).Include(o => o.Prize).ThenInclude(o => o.Major)
                .Where(a => reviewProjects.Select(o => o.Id).Contains(a.Id))
                .ToListAsync();*/
            //.SingleAsync(o => o.Id == reviewProject.Id)
            Logger.Error("2:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            foreach (var reviewProject in reviewProjects)
            {
                Logger.Error("2.1:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                var reviewProjectDto = reviewProject.MapTo<ReviewProjectDto>();
                var project = allProjects.Single(o => o.Id == reviewProject.Id);
                Logger.Error("2.2:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                //var project = await Repository.GetAllIncluding(o => o.DesignOrganization).Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major).Include(o => o.Prize).ThenInclude(o => o.Major).SingleAsync(o => o.Id == reviewProject.Id);
                reviewProjectDto.ProjectName = project.ProjectName;
                reviewProjectDto.DesignOrganizationName = project.DesignOrganization.DisplayName;
                reviewProjectDto.PrizeName = project.Prize.PrizeName;
                reviewProjectDto.SubMajorName = project.PrizeSubMajor?.Major.BriefName;
                Logger.Error("2.3:" + DateTime.Now.ToString("HH:mm:ss:fff"));
                reviewProjectDtos.Add(reviewProjectDto);
            }
            Logger.Error("3:" + DateTime.Now.ToString("HH:mm:ss:fff"));
            return reviewProjectDtos;
        }
        /// <summary>
        /// 评选活动选择项目数据接口
        /// </summary>
        /// <returns></returns>
        [DontWrapResult]
        public virtual async Task<object> GetReviewProjects(int majorId, int? subMajorId,  string projectName,int? organizationId, string exclude,ReviewType reviewType)
        {
            var major = await MajorRepository.GetAsync(majorId);
            var projectQuery = Repository.GetAllIncluding(o => o.DesignOrganization).Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major).Include(o => o.Prize).ThenInclude(o => o.Major).Where(o => o.MatchInstanceId == major.MatchInstanceId && o.Prize.MajorId==majorId);
            if (!string.IsNullOrEmpty(projectName))
            {
                projectQuery = projectQuery.Where(o => o.ProjectName.Contains(projectName));
            }
            if (organizationId.HasValue)
            {
                projectQuery = projectQuery.Where(o => o.DesignOrganizationId==organizationId);
            }
            //
            if (reviewType == ReviewType.Initial)
            {
                //初评选择项目条件为待评选项目或者初评中项目
                projectQuery = projectQuery.Where(o => (o.ProjectStatus == ProjectStatus.UnderReview || o.ProjectStatus == ProjectStatus.Reviewing ));
            }
            else
            {
                projectQuery = projectQuery.Where(o => o.IsInFinalReview);
            }
            //是否有专业小类
            if (subMajorId == null)
            {
                //针对专业大类的项目
                //projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count == 1);
            }
            else
            {
                //有具体专业小类的项目
                projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count(p => p.MajorId == subMajorId) > 0);
            }
            //去除的项目
            if (!exclude.IsNullOrEmpty())
            {
                var excludeIds = exclude.Split(',').Select(o => Convert.ToInt32(o));
                projectQuery = projectQuery.Where(o => !excludeIds.Contains(o.Id));
            }
            //数据处理
            List<ReviewProjectDto> reviewProjectDtos = new List<ReviewProjectDto>();
            foreach (var project in await projectQuery.ToListAsync())
            {
                var reviewProjectDto = new ReviewProjectDto()
                {
                    Id=project.Id,

                };
                reviewProjectDto.ProjectName = project.ProjectName;
                reviewProjectDto.DesignOrganizationName = project.DesignOrganization.DisplayName;
                reviewProjectDto.PrizeName = project.Prize.PrizeName;
                reviewProjectDto.SubMajorName = project.PrizeSubMajor?.Major.BriefName;

                reviewProjectDtos.Add(reviewProjectDto);
            }

            var result = new ResultPageDto()
            {
                code = 0,
                count = reviewProjectDtos.Count(),
                data = reviewProjectDtos.OrderBy(o => o.ProjectName)
            };

            return result;
        }

        /// <summary>
        /// 跨赛事选取项目接口
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <param name="majorId"></param>
        /// <param name="subMajorId"></param>
        /// <param name="projectName"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [DontWrapResult]
        public virtual async Task<object> GetCrossProjects(int? matchInstanceId, int? majorId, int? subMajorId, string projectName, int? organizationId)
        {
            List<ReviewProjectDto> reviewProjectDtos = new List<ReviewProjectDto>();
            //没有指定赛事的查询不返回数据
            if (matchInstanceId != null)
            {
                var projectQuery = Repository.GetAllIncluding(o => o.DesignOrganization).Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major).Include(o => o.Prize).ThenInclude(o => o.Major).Where(o => o.MatchInstanceId == matchInstanceId.Value);
                if (majorId != null)
                {
                    projectQuery = projectQuery.Where(o => o.Prize.MajorId == majorId.Value);
                }
                if (!string.IsNullOrEmpty(projectName))
                {
                    projectQuery = projectQuery.Where(o => o.ProjectName.Contains(projectName));
                }
                if (organizationId.HasValue)
                {
                    projectQuery = projectQuery.Where(o => o.DesignOrganizationId == organizationId);
                }
                //是否有专业小类
                if (subMajorId == null)
                {
                    //针对专业大类的项目
                    //projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count == 1);
                }
                else
                {
                    //有具体专业小类的项目
                    projectQuery = projectQuery.Where(o => o.ProjectMajorInfos.Count(p => p.MajorId == subMajorId) > 0);
                }
                //数据处理

                foreach (var project in await projectQuery.ToListAsync())
                {
                    var reviewProjectDto = new ReviewProjectDto()
                    {
                        Id = project.Id,

                    };
                    reviewProjectDto.ProjectName = project.ProjectName;
                    reviewProjectDto.DesignOrganizationName = project.DesignOrganization.DisplayName;
                    reviewProjectDto.PrizeName = project.Prize.PrizeName;
                    reviewProjectDto.SubMajorName = project.PrizeSubMajor?.Major.BriefName;

                    reviewProjectDtos.Add(reviewProjectDto);
                }
            }
            

            var result = new ResultPageDto()
            {
                code = 0,
                count = reviewProjectDtos.Count(),
                data = reviewProjectDtos.OrderBy(o => o.ProjectName)
            };

            return result;
        }
        /// <summary>
        /// 评选活动的所有项目
        /// </summary>
        /// <param name="reviewId"></param>
        /// <returns></returns>
        public virtual async Task<object> GetReviewProjectsByReviewId(int reviewId,string sourceProjectIds="")
        {
            var review = await ReviewRepository.GetAsync(reviewId);

            var reviewProjectDtos = await ProjectToReviewProjectDtos(review.ReviewProjects.OrderBy(o=>o.Sort).ToList());
            if (!string.IsNullOrEmpty(sourceProjectIds))
            {
                var sourceProjectIdsArr = sourceProjectIds.Split(',').Select(o => int.Parse(o));
                reviewProjectDtos = reviewProjectDtos.Where(o => sourceProjectIdsArr.Contains(o.Id)).ToList();
            }

            return reviewProjectDtos;
        }

        #region 项目导入
        /// <summary>
        /// 导入项目
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual async Task<ResultDto> Import(int matchInstanceId, string filePath)
        {
            string[] columnNeededs = { "项目名称",  "是否原创", "设计单位", "联系人", "电话", "手机", "EMAIL", "申报奖项大类", "奖项子类", "建设单位", "建设国家","建设省份","建设城市" };

            try
            {
                var fileName = HostingEnvironment.WebRootPath + filePath.Replace("/", "\\");
                var dt = Common.ExcelHelper.ReadExcelToDataTable(fileName);
                var errors = new List<ImportErrorDto>();
                var projects = new List<Project>();
                //表头验证
                foreach (var columnNeeded in columnNeededs)
                {
                    if (!dt.Columns.Contains(columnNeeded))
                    {
                        errors.Add(new ImportErrorDto()
                        {
                            Message = $"列\"{columnNeeded}\"不在文件中存在"
                        });
                    }
                }
                if (errors.Count == 0)
                {
                    //每行导入
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];
                        var project = ReadRowToProject(matchInstanceId, row, i + 1, dt, out var error);
                        if (error != null)
                        {
                            errors.AddRange(error);
                        }
                        projects.Add(project);
                    }
                }
                if (errors.Count == 0)
                {
                    foreach (var project in projects)
                    {
                        //增加项目的专业信息
                        var prize = await PrizeRepository.GetAllIncluding(o=>o.PrizeSubMajors).Where(o=>o.Id==project.PrizeId).SingleAsync();
                        var allSubMajors = prize.PrizeSubMajors.Where(o => o.Checked);
                        //如果是专业类或混排类，只需要列出对应选中的专业
                        if (prize.PrizeType == PrizeType.Major || prize.PrizeType == PrizeType.Mixed)
                        {
                            allSubMajors = allSubMajors.Where(o => o.Id == project.PrizeSubMajorId.Value);

                        }                        

                        await ProjectManager.InsertAsync(project);
                        project.ProjectMajorInfos = new List<ProjectMajorInfo>();
                        var mainMajorInfo = new ProjectMajorInfo()
                        {
                            MajorId = null,
                            ProjectId = project.Id
                        };
                        mainMajorInfo.SetData("Files", new List<ProjectFile>());
                        mainMajorInfo.SetData("Layouts", new List<MatchResourceFormDesignItem>());
                        project.ProjectMajorInfos.Add(mainMajorInfo);
                        foreach (var subMajor in allSubMajors)
                        {
                            var subMajorInfo = new ProjectMajorInfo()
                            {
                                MajorId = subMajor.MajorId,
                                ProjectId = project.Id,
                            };
                            subMajorInfo.SetData("Files", new List<ProjectFile>());
                            subMajorInfo.SetData("Layouts", new List<MatchResourceFormDesignItem>());
                            project.ProjectMajorInfos.Add(subMajorInfo);
                        }
                        await ProjectManager.TraceLog(project.Id, "导入", ProjectStatus.UnderReview);
                    }

                }
                return new ResultDto()
                {
                    code = 0,
                    data = errors
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                throw new UserFriendlyException(ex.Message);
            }
        }
        /// <summary>
        /// 将行信息转换为项目信息
        /// </summary>
        /// <param name="row"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private Project ReadRowToProject(int matchInstanceId, DataRow row, int rowIndex, DataTable dt, out List<ImportErrorDto> errors)
        {

            errors = new List<ImportErrorDto>();
            var projectNameStr = row["项目名称"].ToString();
            var isOriginalStr = row["是否原创"].ToString();
            var designOrganizationStr = row["设计单位"].ToString();
            var designOrganizationContactStr = row["联系人"].ToString();
            var designOrganizationPhoneStr = row["电话"].ToString();
            var designOrganizationMobileStr = row["手机"].ToString();
            var designOrganizationEmailStr = row["EMAIL"].ToString();
            var prizeNameStr = row["申报奖项大类"].ToString();
            var subPrizeNameStr = row["奖项子类"].ToString();
            var buildingCompanyStr = row["建设单位"].ToString();
            var buildingCountryStr = row["建设国家"].ToString();
            var buildingProvinceStr = row["建设省份"].ToString();
            var buildingCityStr = row["建设城市"].ToString();

            var project = new Project()
            {
                MatchInstanceId = matchInstanceId,
                ProjectSource=ProjectSource.Import,
                ProjectStatus=ProjectStatus.UnderReview//导入默认为待评选
            };
            //数据验证
            //项目名称
            if (string.IsNullOrEmpty(projectNameStr))
            {
                errors.Add(new ImportErrorDto()
                {
                    Row = rowIndex,
                    Column = dt.Columns.IndexOf("项目名称") + 1,
                    Message = "项目名称不能为空"
                });
            }
            else if (Repository.Count(o => o.MatchInstanceId == matchInstanceId && o.ProjectName == projectNameStr) > 0)
            {
                errors.Add(new ImportErrorDto()
                {
                    Row = rowIndex,
                    Column = dt.Columns.IndexOf("项目名称") + 1,
                    Message = "相同项目名称已存在"
                });
            }
            else
            {
                project.ProjectName = projectNameStr;
            }
            //是否原创
            if (isOriginalStr.ToLower() != "y" && isOriginalStr.ToLower() != "n")
            {
                errors.Add(new ImportErrorDto()
                {
                    Row = rowIndex,
                    Column = dt.Columns.IndexOf("是否原创") + 1,
                    Message = "是否原创只支持Y,N"
                });
            }
            else
            {
                project.IsOriginal = isOriginalStr.ToLower() == "y" ? true : false;
            }
            //设计单位
            if (designOrganizationStr.IsNullOrEmpty())
            {
                errors.Add(new ImportErrorDto()
                {
                    Row = rowIndex,
                    Column = dt.Columns.IndexOf("设计单位") + 1,
                    Message = "设计单位不能为空"
                });
            }
            else
            {
                var designOrganization = OrganizationRepository.GetAll().Where(o => o.ParentId != null && (o.DisplayName == designOrganizationStr || o.BriefName == designOrganizationStr)).FirstOrDefault();
                if (designOrganization == null)
                {
                    errors.Add(new ImportErrorDto()
                    {
                        Row = rowIndex,
                        Column = dt.Columns.IndexOf("设计单位") + 1,
                        Message = $"设计单位\"{designOrganizationStr}\"未在系统中存在"
                    });
                }
                else
                {
                    project.DesignOrganizationId = designOrganization.Id;
                }
            }
            //申报奖项
            var prize = PrizeRepository.GetAll().Include(o => o.PrizeSubMajors).Where(o => o.MatchInstanceId == matchInstanceId && o.PrizeName == prizeNameStr).FirstOrDefault();
            if (prize == null)
            {
                errors.Add(new ImportErrorDto()
                {
                    Row = rowIndex,
                    Column = dt.Columns.IndexOf("申报奖项大类") + 1,
                    Message = $"申报奖项大类\"{prizeNameStr}\"不存在"
                });
            }
            else
            {
                project.PrizeId = prize.Id;
            }
            //奖项子类
            if ( prize!=null)
            {
                if((prize.PrizeType==Prizes.PrizeType.Major|| prize.PrizeType==Prizes.PrizeType.Mixed) && string.IsNullOrEmpty(subPrizeNameStr))
                {
                    errors.Add(new ImportErrorDto()
                    {
                        Row = rowIndex,
                        Column = dt.Columns.IndexOf("奖项子类") + 1,
                        Message = $"奖项大类\"{prizeNameStr}\"必须录入奖项子类"
                    });
                }
                else if ((prize.PrizeType == Prizes.PrizeType.Base || prize.PrizeType == Prizes.PrizeType.Multiple) && !string.IsNullOrEmpty(subPrizeNameStr))
                {
                    errors.Add(new ImportErrorDto()
                    {
                        Row = rowIndex,
                        Column = dt.Columns.IndexOf("奖项子类") + 1,
                        Message = $"奖项大类\"{prizeNameStr}\"不需要录入奖项子类"
                    });
                }else if(!string.IsNullOrEmpty(subPrizeNameStr))
                {
                    var subPrize = prize.PrizeSubMajors.Where(o => o.Major.BriefName == subPrizeNameStr || o.Major.DisplayName == subPrizeNameStr).SingleOrDefault();
                    if (subPrize == null)
                    {
                        errors.Add(new ImportErrorDto()
                        {
                            Row = rowIndex,
                            Column = dt.Columns.IndexOf("奖项子类") + 1,
                            Message = $"奖项子类\"{subPrizeNameStr}\"不在奖项大类\"{prizeNameStr}\"中存在"
                        });
                    }
                    else
                    {
                        project.PrizeSubMajorId = subPrize.Id;
                    }
                }
                
            }
            
            return project;
        }
        #endregion

        #region 项目导出
        public virtual async Task<string> DoExport(RequestPageDto requestPageDto)
        {
            var pageResult = await GetPageResultQueryable(requestPageDto);
            var projects = (await pageResult.Queryable
                .Include(o => o.DesignOrganization)
                .Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major)
                .Include(o => o.Prize).ThenInclude(o => o.Major)
                .ToListAsync());

            var dt = BuildExportDataTable();
            foreach(var project in projects)
            {
                var row=dt.NewRow();
                row["项目名称"] = project.ProjectName;
                row["是否原创"] = project.IsOriginalStr;
                row["设计单位"] = project.DesignOrganization?.DisplayName;
                row["联系人"] = project.DesignOrganizationContact;
                row["电话"] = project.DesignOrganizationPhone;
                row["手机"] = project.DesignOrganizationMobile;
                row["EMAIL"] = project.DesignOrganizationEmail;
                row["申报奖项大类"] = project.Prize?.PrizeName;
                row["奖项子类"] = project.PrizeSubMajor?.Major.DisplayName;
                row["建设单位"] = project.BuildingCompany;
                row["建设国家"] = project.BuildingCountry;
                row["建设省份"] = project.BuildingProvince;
                row["建设城市"] = project.BuildingCity;
                dt.Rows.Add(row);
            }
            EnsureTempDirectoryCreated();
            var filePath = "/temp/"+ Guid.NewGuid() + ".xlsx";
            var fileName = HostingEnvironment.WebRootPath + filePath.Replace("/", "\\");
            Common.ExcelHelper.DataTableToExcel(dt, fileName, "Sheet1", true);

            return filePath;
        }
        private DataTable BuildExportDataTable()
        {
            string[] columnNeededs = { "项目名称", "是否原创", "设计单位", "联系人", "电话", "手机", "EMAIL", "申报奖项大类", "奖项子类", "建设单位", "建设国家", "建设省份", "建设城市" };

            var dt = new DataTable();
            foreach(var columnName in columnNeededs)
            {
                DataColumn column = new DataColumn(columnName);
                dt.Columns.Add(column);
            }
            return dt;
        }
        private void EnsureTempDirectoryCreated()
        {
            var tempDirectory = HostingEnvironment.WebRootPath + "\\temp";
            if (!System.IO.Directory.Exists(tempDirectory))
            {
                System.IO.Directory.CreateDirectory(tempDirectory);
            }
        }
        #endregion

        /// <summary>
        /// 获取评选结果
        /// </summary>
        /// <returns></returns>
        [DontWrapResult]
        public virtual async Task<object> GetReviewResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);
            var data = (await pageResult.Queryable
                .Include(o => o.DesignOrganization)
                .Include(o=>o.ProjectMajorInfos)
                .Include(o => o.PrizeSubMajor).ThenInclude(o => o.Major)
                .Include(o => o.Prize).ThenInclude(o => o.Major)
                .ToListAsync())
                .Select(o => {
                    return CacheManager.GetCache<int, object>("ProjectResultCache").Get(o.Id, id => {
                        var initialExpertCount = ProjectManager.GetProjectExpertCount(o, ReviewType.Initial);
                        var finalExpertCount = ProjectManager.GetProjectExpertCount(o, ReviewType.Finish);
                        var projectMajorInfos = o.ProjectMajorInfos.Where(m => m.MajorId != null).OrderBy(m => m.MajorId.Value);
                        return new
                        {
                            o.Id,
                            o.ProjectName,
                            o.PrizeId,
                            o.ReportSN,
                            o.Prize.PrizeName,
                            IsOriginal=o.IsOriginal,
                            MajorName = o.Prize.Major.BriefName,
                            SubMajorId = o.PrizeSubMajor != null ? o.PrizeSubMajor.MajorId.ToString() : "",
                            SubMajorName = o.PrizeSubMajor != null ? o.PrizeSubMajor.Major.BriefName : "-",
                            DesignOrganizationName = o.DesignOrganization != null ? o.DesignOrganization.DisplayName : "",
                            o.IsInFinalReview,
                            o.ScoreInitial,
                            o.ScoreFinal,
                            o.RankFinal,
                            o.RankInitial,
                            ExpertCountAllInitial = initialExpertCount.allCount,
                            ExpertCountRankedInitial = initialExpertCount.rankedCount,
                            ExpertCountAllFinal = finalExpertCount.allCount,
                            ExpertCountRankedFinal = finalExpertCount.rankedCount,
                            MajorScore0Initial = projectMajorInfos.ElementAtOrDefault(0)?.ScoreInitial,
                            MajorScore0Final = projectMajorInfos.ElementAtOrDefault(0)?.ScoreFinal,
                            MajorScore1Initial = projectMajorInfos.ElementAtOrDefault(1)?.ScoreInitial,
                            MajorScore1Final = projectMajorInfos.ElementAtOrDefault(1)?.ScoreFinal,
                            MajorScore2Initial = projectMajorInfos.ElementAtOrDefault(2)?.ScoreInitial,
                            MajorScore2Final = projectMajorInfos.ElementAtOrDefault(2)?.ScoreFinal,
                            MajorScore3Initial = projectMajorInfos.ElementAtOrDefault(3)?.ScoreInitial,
                            MajorScore3Final = projectMajorInfos.ElementAtOrDefault(3)?.ScoreFinal,
                            MajorScore4Initial = projectMajorInfos.ElementAtOrDefault(4)?.ScoreInitial,
                            MajorScore4Final = projectMajorInfos.ElementAtOrDefault(4)?.ScoreFinal,
                            MajorScore5Initial = projectMajorInfos.ElementAtOrDefault(5)?.ScoreInitial,
                            MajorScore5Final = projectMajorInfos.ElementAtOrDefault(5)?.ScoreFinal,
                        };
                    });

                    
                }
                );


            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }
    }
}
