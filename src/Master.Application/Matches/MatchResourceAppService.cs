using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Web.Models;
using Master.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Matches
{
    public class MatchResourceAppService : MasterAppServiceBase<MatchResource, int>
    {
        private readonly IRepository<MatchInstance, int> _matchInstanceRepository;

        public MatchResourceAppService(IRepository<MatchInstance, int> matchInstanceRepository)
        {
            _matchInstanceRepository = matchInstanceRepository;
        }
        public virtual async Task<object> GetMatchResource(int id)
        {
            var resource = await Manager.GetByIdAsync(id);
            object result = null;
            switch (resource.MatchResourceType)
            {
                case MatchResourceType.DynamicForm:
                    result = new MatchResource<MatchResourceFormDesignItem>(resource).MapTo<MatchResourceDto<MatchResourceFormDesignItem>>();
                    break;
                case MatchResourceType.UploadList:
                    result = new MatchResource<MatchResourceUploadList>(resource).MapTo<MatchResourceDto<MatchResourceUploadList>>();
                    break;
                case MatchResourceType.DownloadList:
                    result = new MatchResource<MatchResourceDownloadList>(resource).MapTo<MatchResourceDto<MatchResourceDownloadList>>();
                    break;
                case MatchResourceType.RateTable:
                    result = new MatchResource<MatchResourceRateTable>(resource).MapTo<MatchResourceDto<MatchResourceRateTable>>();
                    break;
            }

            return result;
        }

        #region 表单设计 
        /// <summary>
        /// 获取某赛事实例下某个大专业附属的所有表单设计
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <param name="majorId"></param>
        /// <returns></returns>
        public virtual async Task<object> GetFormDesignDetail(int matchInstanceId,int majorId)
        {

            return await CacheManager.GetCache("FormDesign")
                .GetAsync($"{matchInstanceId}_{majorId}", async key =>
                {
                    var innerMatchInstanceId = int.Parse(key.Split('_')[0]);
                    var innerMajorId = int.Parse(key.Split('_')[1]);
                    var matchResources = await Repository.GetAll().Where(o => o.MatchInstanceId == innerMatchInstanceId && o.MatchResourceType == MatchResourceType.DynamicForm && o.MatchResourceStatus == MatchResourceStatus.Publish && o.MajorId == innerMajorId).ToListAsync();
                    return matchResources.Select(o => new { SubMajorId = o.SubMajorId.HasValue ? o.SubMajorId.ToString() : "", Layouts = o.GetData<List<MatchResourceFormDesignItem>>("Datas") });
                });
            //var matchResources = await Repository.GetAll().Where(o => o.MatchInstanceId == matchInstanceId && o.MatchResourceType == MatchResourceType.DynamicForm && o.MatchResourceStatus == MatchResourceStatus.Publish && o.MajorId == majorId).ToListAsync();

            //return matchResources.Select(o => new { SubMajorId = o.SubMajorId.HasValue ? o.SubMajorId.ToString() : "", Layouts = o.GetData<List<MatchResourceFormDesignItem>>("Datas") });
        }
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetFormDesignPageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Major).Include(o => o.SubMajor);
            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o => {
                    return new { o.Id, MatchName = o.Match.Name, MajorName = o.Major.BriefName, SubMajorName = o.SubMajor != null ? o.SubMajor.BriefName : "-", o.MatchResourceStatus };
                });


            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }


        /// <summary>
        /// 提交表单设计
        /// </summary>
        /// <param name="matchResourceDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitMatchResourceFormDesign(MatchResource<MatchResourceFormDesignItem> matchResourceDto)
        {
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if (await _matchInstanceRepository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}

            matchResourceDto.Init();
            MatchResource matchResource = null;

            //验证同一赛事、专业大类、专业小类发布中的数据只能为一条

            if (matchResourceDto.MatchResourceStatus == MatchResourceStatus.Publish)
            {
                var publishedCount = await Repository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceId == null && o.MajorId == matchResourceDto.MajorId && o.SubMajorId == matchResourceDto.SubMajorId && o.MatchResourceType == MatchResourceType.DynamicForm && o.Id != matchResourceDto.Id && o.MatchResourceStatus == MatchResourceStatus.Publish);
                if (publishedCount > 0)
                {
                    throw new UserFriendlyException("当前赛事专业已存在发布中状态的表单设计");
                }
            }

            if (matchResourceDto.Id == 0)
            {
                matchResource = new MatchResource();
                matchResourceDto.MapTo(matchResource);
                await Repository.InsertAsync(matchResource);
            }
            else
            {
                matchResource = await Manager.GetByIdAsync(matchResourceDto.Id);
                matchResourceDto.MapTo(matchResource);
                await Repository.UpdateAsync(matchResource);
            }



        }
        #endregion

        #region 上传清单
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetUploadListPageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Major).Include(o => o.SubMajor);

            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o => {
                    return new { o.Id, MatchName = o.Match.Name, MajorName = o.Major.BriefName, SubMajorName = o.SubMajor != null ? o.SubMajor.BriefName : "-", o.MatchResourceStatus };
                });


            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }


        /// <summary>
        /// 提交上传清单
        /// </summary>
        /// <param name="matchResourceDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitMatchResourceUploadList(MatchResource<MatchResourceUploadList> matchResourceDto)
        {
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if (await _matchInstanceRepository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}

            matchResourceDto.Init();
            MatchResource matchResource = null;

            //验证同一赛事、专业大类、专业小类发布中的数据只能为一条

            if (matchResourceDto.MatchResourceStatus == MatchResourceStatus.Publish)
            {
                var publishedCount = await Repository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceId == null && o.MajorId == matchResourceDto.MajorId && o.SubMajorId == matchResourceDto.SubMajorId && o.MatchResourceType==MatchResourceType.UploadList && o.Id != matchResourceDto.Id && o.MatchResourceStatus == MatchResourceStatus.Publish);
                if (publishedCount > 0)
                {
                    throw new UserFriendlyException("当前赛事专业已存在发布中状态的上传清单");
                }
            }

            if (matchResourceDto.Id == 0)
            {
                matchResource = new MatchResource();
                matchResourceDto.MapTo(matchResource);
                await Repository.InsertAsync(matchResource);
            }
            else
            {
                matchResource = await Manager.GetByIdAsync(matchResourceDto.Id);
                matchResourceDto.MapTo(matchResource);
                await Repository.UpdateAsync(matchResource);
            }



        }
        #endregion

        #region 样例下载
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetDownloadListPageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Major).Include(o => o.SubMajor);
            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o => {
                    return new { o.Id, MatchName = o.Match.Name, MajorName = o.Major.BriefName, SubMajorName = o.SubMajor != null ? o.SubMajor.BriefName : "-", o.MatchResourceStatus };
                });


            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }


        /// <summary>
        /// 提交样例下载
        /// </summary>
        /// <param name="matchResourceDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitMatchResourceDownloadList(MatchResource<MatchResourceDownloadList> matchResourceDto)
        {
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if (await _matchInstanceRepository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}

            matchResourceDto.Init();
            MatchResource matchResource = null;

            //验证同一赛事、专业大类、专业小类发布中的数据只能为一条

            if (matchResourceDto.MatchResourceStatus == MatchResourceStatus.Publish)
            {
                var publishedCount = await Repository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceId == null && o.MajorId == matchResourceDto.MajorId && o.SubMajorId == matchResourceDto.SubMajorId && o.MatchResourceType == MatchResourceType.DownloadList && o.Id!=matchResourceDto.Id && o.MatchResourceStatus==MatchResourceStatus.Publish);
                if (publishedCount > 0)
                {
                    throw new UserFriendlyException("当前赛事专业已存在发布中状态的样例下载");
                }
            }

            if (matchResourceDto.Id == 0)
            {
                matchResource = new MatchResource();
                matchResourceDto.MapTo(matchResource);
                await Repository.InsertAsync(matchResource);
            }
            else
            {
                matchResource = await Manager.GetByIdAsync(matchResourceDto.Id);
                matchResourceDto.MapTo(matchResource);
                await Repository.UpdateAsync(matchResource);
            }



        }
        #endregion

        #region 评分表
        [DontWrapResult]
        public virtual async Task<ResultPageDto> GetRateTablePageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);
            pageResult.Queryable = pageResult.Queryable.Include(o => o.Match).Include(o => o.Major).Include(o => o.SubMajor);

            var data = (await pageResult.Queryable.ToListAsync())
                .Select(o => {
                    return new { o.Id, MatchName = o.Match.Name, MajorName = o.Major.BriefName, SubMajorName = o.SubMajor != null ? o.SubMajor.BriefName : "-", o.MatchResourceStatus };
                });


            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = data
            };

            return result;
        }


        /// <summary>
        /// 提交评分表
        /// </summary>
        /// <param name="matchResourceDto"></param>
        /// <returns></returns>
        public virtual async Task SubmitMatchResourceRateTable(MatchResource<MatchResourceRateTable> matchResourceDto)
        {
            //如果有申报中的赛事实例，不允许修改
            //modi20181008 允许修改
            //if (await _matchInstanceRepository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceStatus == MatchInstanceStatus.Applying) > 0)
            //{
            //    throw new UserFriendlyException("赛事已在申报中,无法修改");
            //}

            matchResourceDto.Init();
            MatchResource matchResource = null;

            //验证同一赛事、专业大类、专业小类发布中的数据只能为一条

            if (matchResourceDto.MatchResourceStatus == MatchResourceStatus.Publish)
            {
                var publishedCount = await Repository.CountAsync(o => o.MatchId == matchResourceDto.MatchId && o.MatchInstanceId == null && o.MajorId == matchResourceDto.MajorId && o.SubMajorId == matchResourceDto.SubMajorId && o.MatchResourceType == MatchResourceType.RateTable && o.Id != matchResourceDto.Id && o.MatchResourceStatus == MatchResourceStatus.Publish);
                if (publishedCount > 0)
                {
                    throw new UserFriendlyException("当前赛事专业已存在发布中状态的样例下载");
                }
            }

            if (matchResourceDto.Id == 0)
            {
                matchResource = new MatchResource();
                matchResourceDto.MapTo(matchResource);
                await Repository.InsertAsync(matchResource);
            }
            else
            {
                matchResource = await Manager.GetByIdAsync(matchResourceDto.Id);
                matchResourceDto.MapTo(matchResource);
                await Repository.UpdateAsync(matchResource);
            }



        }
        #endregion
    }
}
