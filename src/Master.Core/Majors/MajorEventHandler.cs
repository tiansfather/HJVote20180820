using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Majors
{
    public class MajorEventHandler :
        IEventHandler<EntityDeletedEventData<Major>>,
        ITransientDependency
    {
        private readonly IRepository<MajorExpert> _majorExpertRepository;
        private readonly IRepository<MajorCharger> _majorChargerRepository;
        public MajorEventHandler(
            IRepository<MajorExpert> majorExpertRepository,
            IRepository<MajorCharger> majorChargerRepository)
        {
            _majorExpertRepository = majorExpertRepository;
            _majorChargerRepository = majorChargerRepository;
        }
        [UnitOfWork]
        public void HandleEvent(EntityDeletedEventData<Major> eventData)
        {
            _majorExpertRepository.Delete(o => o.MajorId == eventData.Entity.Id);
            _majorChargerRepository.Delete(o => o.MajorId == eventData.Entity.Id);
        }
    }
}
