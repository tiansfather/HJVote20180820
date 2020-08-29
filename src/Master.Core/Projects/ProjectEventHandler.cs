using Abp.Dependency;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master.Projects
{
    public class ProjectEventHandler : IAsyncEventHandler<EntityChangedEventData<Project>>, ITransientDependency
    {
        public ICacheManager CacheManager { get; set; }
        public async Task HandleEventAsync(EntityChangedEventData<Project> eventData)
        {
            await CacheManager.GetCache<int, object>("ProjectResultCache").RemoveAsync(eventData.Entity.Id);
        }
    }
}
