using Abp.Dependency;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Matches
{
    public class MatchResourceCacheItemInvalidator :
        IEventHandler<EntityChangedEventData<MatchResource>>,
        IEventHandler<EntityDeletedEventData<MatchResource>>,
        ITransientDependency
    {
        private readonly ICacheManager _cacheManager;
        public MatchResourceCacheItemInvalidator(
            ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        public void HandleEvent(EntityChangedEventData<MatchResource> eventData)
        {
            var cacheKey = $"{eventData.Entity.MatchInstanceId}_{eventData.Entity.MajorId}";
            _cacheManager.GetCache("FormDesign").Remove(cacheKey);
        }

        public void HandleEvent(EntityDeletedEventData<MatchResource> eventData)
        {
            var cacheKey = $"{eventData.Entity.MatchInstanceId}_{eventData.Entity.MajorId}";
            _cacheManager.GetCache("FormDesign").Remove(cacheKey);
        }
    }
}
