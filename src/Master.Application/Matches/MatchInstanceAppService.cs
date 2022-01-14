using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Master.Authentication;
using Master.Dto;
using Master.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Master.Matches
{
    public class MatchInstanceAppService : MasterAppServiceBase<MatchInstance, int>
    {
        public IRepository<Review, int> ReviewRepository { get; set; }
        public override async Task FormSubmit(FormSubmitRequestDto request)
        {
            switch (request.Action)
            {
                case "Add":
                    await DoAdd(request);
                    break;
                case "Edit":
                    await DoEdit(request);
                    break;
            }
        }

        private async Task DoEdit(FormSubmitRequestDto request)
        {
            var id = Convert.ToInt32(request.Datas["Id"]);
            var matchInstance = await Manager.GetByIdAsync(id);

            var manager = Manager as MatchInstanceManager;
            var oriStatus = matchInstance.MatchInstanceStatus;
            await Manager.LoadEntityFromDatas(request.Datas, matchInstance);
            //todo:赛事状态切换有效性判断
            await manager.ChangeMatchInstanceStatus(matchInstance, oriStatus);

            await Manager.UpdateAsync(matchInstance);
            
        }

        private async Task DoAdd(FormSubmitRequestDto request)
        {
            var matchInstance = await Manager.LoadEntityFromDatas(request.Datas);

            await Manager.InsertAsync(matchInstance);
            
        }

        public override async Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {
            var pageResult = await GetPageResultQueryable(request);

            var data = (await pageResult.Queryable.Include(o => o.Match).ToListAsync())
                .Select(o => {
                    return new { o.Id, MatchName=o.Match.Name, o.Identifier,o.Year,o.Remarks,o.MatchInstanceStatus,o.DataProjectPath ,o.DataReviewPath,o.MatchInstanceDisplayMode};
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
        /// 获取所有可用的赛事实例
        /// </summary>
        /// <returns></returns>
        public virtual async Task<object> GetAvailableMatchInstance()
        {
            var query = Repository.GetAllIncluding(o => o.Match);
            //add 20200828 过滤不显示的
            query = query.Where(o => o.Match.IsDisplay);
            //申报者和分公司科管及专业负责人列出所有申报中的赛事实例
            if (AbpSession.IsReporter()||AbpSession.IsSubManager()||AbpSession.IsMajorManager())
            {
                query = query.Where(o => o.MatchInstanceStatus == MatchInstanceStatus.Applying);
            }
            //集团科管列出所有申报评选中以及评选完成的赛事实例
            if (AbpSession.IsGroupManager())
            {
                query = query.Where(o => o.MatchInstanceStatus == MatchInstanceStatus.Applying || o.MatchInstanceStatus == MatchInstanceStatus.Complete);
            }

            return (await query.ToListAsync())
                .Select(o => new {o.Id,MatchName=o.Match.Name,o.Identifier,o.DisplayGroup });
        }
        public virtual async Task<bool> GetIfMatchInstanceAvailable(int matchInstanceId)
        {            
            var matchInstance = await Repository.GetAsync(matchInstanceId);

            //申报者和分公司科管及专业负责人列出所有申报中的赛事实例
            if (AbpSession.IsReporter() || AbpSession.IsSubManager()||AbpSession.IsMajorManager())
            {
                return matchInstance.MatchInstanceStatus == MatchInstanceStatus.Applying;
            }
            //集团科管列出所有申报中评选中以及评选完成的赛事实例
            if (AbpSession.IsGroupManager())
            {
                return matchInstance.MatchInstanceStatus == MatchInstanceStatus.Applying || matchInstance.MatchInstanceStatus == MatchInstanceStatus.Complete;
            }

            return false;
        }
        /// <summary>
        /// 清空赛事
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task ClearMatchInstance(int matchInstanceId)
        {
            var manager = Manager as MatchInstanceManager;
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            await manager.ResetMatchInstance(matchInstance);
        }
        /// <summary>
        /// 重新发布赛事
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task ReSubmitMatchInstance(int matchInstanceId)
        {
            var manager = Manager as MatchInstanceManager;
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            await manager.ReSyncMatchInstanceResource(matchInstance);
        }
        /// <summary>
        /// 设置赛事的项目展示方式
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual async Task SetMatchInstanceDisplayMode(int matchInstanceId,MatchInstanceDisplayMode mode)
        {
            var manager = Manager as MatchInstanceManager;
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            matchInstance.MatchInstanceDisplayMode = mode;
        }

        #region 赛事导出
        /// <summary>
        /// 初始化项目导出
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task<object> InitProjectExport(int matchInstanceId)
        {
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            if (matchInstance.MatchInstanceStatus == MatchInstanceStatus.Draft)
            {
                throw new UserFriendlyException("无法导出草稿中的赛事");
            }
            var projectFolder = Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/项目");
            //删除原有项目导出文件
            var dataProjectPath = matchInstance.DataProjectPath;
            if (!string.IsNullOrEmpty(dataProjectPath) && System.IO.File.Exists(Common.PathHelper.VirtualPathToAbsolutePath(dataProjectPath)))
            {
                try
                {
                    System.IO.File.Delete(Common.PathHelper.VirtualPathToAbsolutePath(dataProjectPath));
                }
                catch (Exception ex)
                {

                }
            }
            try
            {
                //先删除原有文件夹
                System.IO.Directory.Delete(projectFolder, true);                
                
            }catch(Exception ex)
            {

            }            
            System.IO.Directory.CreateDirectory(projectFolder);//建立项目文件夹
            //将样式文件复制
            System.IO.File.Copy(Common.PathHelper.VirtualPathToAbsolutePath("/assets/layuiadmin/layui/css/layui.css"), Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/项目/layui.css"));
            System.IO.File.Copy(Common.PathHelper.VirtualPathToAbsolutePath("/assets/css/default.css"), Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/项目/default.css"));
            //获取赛事下的所有项目Id
            var projectIds = await ProjectRepository.GetAll().Where(o => o.MatchInstanceId == matchInstanceId && o.ProjectStatus!=Projects.ProjectStatus.Draft && o.ProjectStatus!=Projects.ProjectStatus.Reject).Select(o => o.Id).ToListAsync();
            return projectIds;
        }
        /// <summary>
        /// 初始化评审导出
        /// </summary>
        /// <param name="matchInstanceId"></param>
        /// <returns></returns>
        public virtual async Task<object> InitReviewExport(int matchInstanceId)
        {
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            var reviewFolder = Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/评审数据");
            try
            {
                //先删除原有文件夹
                System.IO.Directory.Delete(reviewFolder, true);
            }
            catch (Exception ex)
            {

            }
            System.IO.Directory.CreateDirectory(reviewFolder);//建立评审文件夹
            //将样式文件复制
            //System.IO.File.Copy(Common.PathHelper.VirtualPathToAbsolutePath("/assets/layuiadmin/layui/css/layui.css"), Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/评审数据/layui.css"));
            //System.IO.File.Copy(Common.PathHelper.VirtualPathToAbsolutePath("/assets/css/default.css"), Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/评审数据/default.css"));
            //赛事所有评审
            var reviewIds = await ReviewRepository.GetAll().Where(o => o.MatchInstanceId == matchInstanceId && o.ReviewStatus==ReviewStatus.Reviewed).Select(o => o.Id).ToListAsync();
            return reviewIds;
        }
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="matchInstanceId"></param>
        public virtual async Task Compress(int matchInstanceId)
        {
            var matchInstance = await Manager.GetByIdAsync(matchInstanceId);
            var projectFolder = Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/项目");
            var reviewFolder = Common.PathHelper.VirtualPathToAbsolutePath($"/MatchInstance/{matchInstance.Name}/评审数据");
            var dataProjectPath = $"/MatchInstance/{matchInstance.Name}_项目_{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip";
            var dataReviewPath = $"/MatchInstance/{matchInstance.Name}_评审数据_{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip";
            Common.ZipHelper.Zips(projectFolder, Common.PathHelper.VirtualPathToAbsolutePath(dataProjectPath));
            Common.ZipHelper.Zips(reviewFolder, Common.PathHelper.VirtualPathToAbsolutePath(dataReviewPath));
            matchInstance.DataProjectPath = dataProjectPath;
            matchInstance.DataReviewPath = dataReviewPath;
        }
        #endregion
    }
}
