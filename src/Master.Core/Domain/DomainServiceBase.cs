using Abp.Domain.Entities;
using Abp.Domain.Entities.Caching;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Domain
{
    /// <summary>
    /// 基于实体的领域基类，封装基本增删改查操作
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public abstract class DomainServiceBase<TEntity, TPrimaryKey> : DomainServiceBase
        where TEntity : class, IEntity<TPrimaryKey>, new()
    {
        public IEntityCache<TEntity> EntityCache { get; set; }
        public IRepository<TEntity, TPrimaryKey> Repository { get; set; }
        /// <summary>
        /// 表单提交建立实体
        /// </summary>
        /// <param name="Datas"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> DoAdd(IDictionary<string, string> Datas)
        {
            return await LoadEntityFromDatas(Datas);
        }

        /// <summary>
        /// 表单提交修改实体
        /// </summary>
        /// <param name="Datas"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> DoEdit(IDictionary<string, string> Datas, TPrimaryKey id)
        {
            var entity = await GetByIdAsync(id);
            return await LoadEntityFromDatas(Datas, entity);
        }


        public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            return await Repository.GetAsync(id);
        }
        public Task<TEntity> GetByIdFromCacheAsync(int id)
        {
            return Task.FromResult(EntityCache[id]);
            //return Task.FromResult(EntityCoreCache[id]);
            //return await CacheManager.GetCache<TPrimaryKey, TEntity>(typeof(TEntity).Name)
            //    .GetAsync(id, async () => { return await Repository.GetAsync(id); });
        }
        public virtual async Task<IEnumerable<TEntity>> GetListByIdsAsync(IEnumerable<TPrimaryKey> ids)
        {
            return await Repository.GetAll().Where(o => ids.Contains(o.Id)).ToListAsync();
        }
        public virtual IQueryable<TEntity> GetAll()
        {
            return Repository.GetAll();
        }
        public virtual async Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity)
        {
            return await Repository.InsertAndGetIdAsync(entity);
        }
        public virtual async Task InsertAsync(TEntity entity)
        {
            await Repository.InsertAsync(entity);
        }
        public virtual async Task UpdateAsync(TEntity entity)
        {
            await Repository.UpdateAsync(entity);
        }
        public virtual async Task DeleteAsync(IEnumerable<TPrimaryKey> ids)
        {
            await Repository.DeleteAsync(o => ids.Contains(o.Id));
        }
        public virtual async Task DeleteAsync(TEntity entity)
        {
            await Repository.DeleteAsync(entity);
        }
        /// <summary>
        /// 将提交来的数据存为实体
        /// </summary>
        /// <param name="Datas"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual Task<TEntity> LoadEntityFromDatas(IDictionary<string, string> Datas, TEntity obj = null)
        {
            return Task.Run(() => {
                var entity = obj;
                //如果是添加方式
                if (obj == null)
                {
                    //通过反序列化方式进行赋值
                    var serializedString = JsonConvert.SerializeObject(Datas);
                    entity = JsonConvert.DeserializeObject<TEntity>(serializedString);

                }
                else
                {
                    //遍历datas赋值
                    foreach (var property in typeof(TEntity).GetProperties())
                    {
                        if (Datas.ContainsKey(property.Name))
                        {
                            var propertyType = property.PropertyType;
                            object targetValue = null;
                            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            {
                                if (!string.IsNullOrEmpty(Datas[property.Name]))
                                {
                                    targetValue = Convert.ChangeType(Datas[property.Name], propertyType.GetGenericArguments()[0]);
                                }

                            }
                            else if (typeof(System.Enum).IsAssignableFrom(propertyType))
                            {
                                targetValue = Enum.Parse(propertyType, Datas[property.Name]);
                            }
                            else if (propertyType == typeof(bool) && string.IsNullOrEmpty(Datas[property.Name]))
                            {
                                targetValue = false;
                            }
                            else
                            {
                                targetValue = Convert.ChangeType(Datas[property.Name], propertyType);
                            }

                            property.SetValue(entity, targetValue);
                        }
                    }
                }

                return entity;
            });


        }
    }

    public abstract class DomainServiceBase : DomainService
    {
        public IAbpSession AbpSession { get; set; }
        public ICacheManager CacheManager { get; set; }
        public DomainServiceBase()
        {
            LocalizationSourceName = MasterConsts.LocalizationSourceName;
        }
    }
}
