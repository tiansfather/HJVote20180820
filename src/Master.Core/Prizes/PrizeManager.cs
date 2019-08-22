using Abp.Domain.Repositories;
using Master.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master.Prizes
{
    public class PrizeManager : DomainServiceBase<Prize, int>
    {
        public IRepository<PrizeSubMajor,int> PrizeSubMajorRepository { get; set; }
        public override async Task<Prize> GetByIdAsync(int id)
        {
            var prize = await Repository.GetAll()
                .Include(o => o.PrizeSubMajors)
                .SingleAsync(o => o.Id == id);

            foreach (var prizeSubMajor in prize.PrizeSubMajors)
            {
                PrizeSubMajorRepository.EnsurePropertyLoaded(prizeSubMajor, o => o.Major);
            }
            return prize;
        }
    }

    public class PrizeSubMajorManager : DomainServiceBase<PrizeSubMajor, int>
    {
    }
}
