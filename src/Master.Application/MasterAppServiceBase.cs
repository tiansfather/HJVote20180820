using Abp.Application.Services;
using Abp.Auditing;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Web.Models;
using Master.Authentication;
using Master.Domain;
using Master.Dto;
using Master.Extension;
using Master.MultiTenancy;
using Master.Organizations;
using Master.Prizes;
using Master.Projects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Master
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class MasterAppServiceBase : ApplicationService
    {
        public IHostingEnvironment HostingEnvironment { get; set; }
        public IRepository<Organization, int> OrganizationRepository { get; set; }
        public IRepository<Prize,int> PrizeRepository { get; set; }
        public IRepository<Project,int> ProjectRepository { get; set; }
        public TenantManager TenantManager { get; set; }
        public ICacheManager CacheManager { get; set; }

        public UserManager UserManager { get; set; }
        protected MasterAppServiceBase()
        {
            LocalizationSourceName = MasterConsts.LocalizationSourceName;
        }
        protected virtual Task<User> GetCurrentUserAsync()
        {
            var user = UserManager.GetByIdAsync(AbpSession.GetUserId());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }
        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }
    }

    /// <summary>
    /// appservice层基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimary"></typeparam>
    public abstract class MasterAppServiceBase<TEntity, TPrimary> : MasterAppServiceBase
        where TEntity : class, IEntity<TPrimary>, new()
    {
        public IRepository<TEntity, TPrimary> Repository { get; set; }
        /// <summary>
        /// 获取实体的管理类
        /// </summary>
        
        public virtual DomainServiceBase<TEntity, TPrimary> Manager
        {
            get
            {
                var managerType = GetEntityManagerType(typeof(TEntity));
                using (var managerWrapper = IocManager.Instance.ResolveAsDisposable(managerType))
                {
                    var manager = managerWrapper.Object as DomainServiceBase<TEntity, TPrimary>;
                    return manager ;
                }
            }
        }

        private Type GetEntityManagerType(Type entityType)
        {
            var asmQualifiedname = entityType.AssemblyQualifiedName.Split(',')[1];
            var prefix=entityType.FullName.Substring(0, entityType.FullName.LastIndexOf('.'));
            var returnType= Type.GetType($"{prefix}.I{entityType.Name}Manager,{asmQualifiedname}");
            if (returnType == null)
            {
                returnType= Type.GetType($"{prefix}.{entityType.Name}Manager,{asmQualifiedname}");
            }
            return returnType;
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task DeleteEntity(IEnumerable<TPrimary> ids)
        {
            await Manager.DeleteAsync(ids);
        }
        /// <summary>
        /// 通过客户端查询条件返回查询,在派生类中重写
        /// </summary>
        /// <param name="searchKeys"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual async Task<IQueryable<TEntity>> BuildSearchQueryAsync(IDictionary<string, string> searchKeys, IQueryable<TEntity> query)
        {
            return await Task.FromResult(query);
        }
        protected virtual async Task<PagedResult<TEntity>> GetPageResultQueryable(RequestPageDto request)
        {
            if (request.Page <= 0)
            {
                request.Page = 1;
            }
            if (request.Limit <= 0)
            {
                request.Limit = 10;
            }
            var query = Manager.GetAll();
            if (!request.Where.IsNullOrWhiteSpace())
            {
                query = query.Where(request.Where);
            }
            if (!request.SearchKeys.IsNullOrWhiteSpace())
            {
                query = await BuildSearchQueryAsync(Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, string>>(request.SearchKeys), query);
            }
            if (!request.OrderField.IsNullOrWhiteSpace())
            {
                //提交过来的排序
                query = query.OrderBy($"{request.OrderField} {request.OrderType}");
            }
            var pageResult = query.PageResult(request.Page, request.Limit);

            return pageResult;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DontWrapResult]
        public async virtual Task<ResultPageDto> GetPageResult(RequestPageDto request)
        {

            var pageResult = await GetPageResultQueryable(request);

            var result = new ResultPageDto()
            {
                code = 0,
                count = pageResult.RowCount,
                data = await pageResult.Queryable.ToListAsync()
            };

            return result;
        }

        public virtual async Task FormSubmit(FormSubmitRequestDto request)
        {
            //通用模块增加修改表单提交
            //暂只支持添加和修改的提交
            if (request.Action == "Add")
            {
                await Manager.DoAdd(request.Datas);
            }
            else if (request.Action == "Edit")
            {
                var id = (TPrimary)Convert.ChangeType(request.Datas.GetDataOrException("ids"), typeof(TPrimary));
                await Manager.DoEdit(request.Datas, id);
            }
        }
    }
}