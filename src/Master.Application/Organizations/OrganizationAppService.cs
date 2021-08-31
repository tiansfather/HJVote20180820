using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Extensions;
using Abp.UI;
using Master.Authentication;
using Master.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Organizations
{
    public class OrganizationAppService:MasterAppServiceBase<Organization,int>
    {
        public override async Task FormSubmit(FormSubmitRequestDto request)
        {
            switch (request.Action)
            {
                case "Submit":
                    await DoSubmit(request);
                    break;
            }
        }

        private async Task DoSubmit(FormSubmitRequestDto request)
        {
            var manager = Manager as OrganizationManager;
            Organization organization = null;
            
            if (request.Datas["Id"]=="0")
            {
                //添加
                if(request.Datas.ContainsKey("IsActive") && string.IsNullOrEmpty(request.Datas["IsActive"]))
                {
                    request.Datas["IsActive"] = "false";
                }
                organization =await manager.LoadEntityFromDatas(request.Datas);
                //var organization = new Organization()
                //{
                //    BriefCode=request.Datas["briefCode"],
                //    BriefName=request.Datas["briefName"],
                //    DisplayName=request.Datas["displayName"],
                //    Sort=int.Parse(request.Datas["sort"]),
                //    Remarks=request.Datas["remarks"]
                //};
                //if (!string.IsNullOrEmpty(request.Datas["parentId"]))
                //{
                //    organization.ParentId = int.Parse(request.Datas["parentId"]);
                //}
                //else
                //{
                //    organization.ParentId = null;
                //}
                organization.SetData("ExtendData1", request.Datas["ExtendData1"]);
                organization.SetData("ExtendData2", request.Datas["ExtendData2"]);
                organization.SetData("ExtendData3", request.Datas["ExtendData3"]);
                await manager.CreateAsync(organization);
            }
            else
            {
                var id = Convert.ToInt32(request.Datas["Id"]);
                var oriOrganization = await Repository.GetAsync(id);//旧组织实体
                int? newParentId = null;
                if (!string.IsNullOrEmpty(request.Datas["ParentId"]))
                {
                    if(int.TryParse(request.Datas["ParentId"],out var _))
                    {
                        newParentId = int.Parse(request.Datas["ParentId"]);
                    }
                    else
                    {
                        request.Datas["ParentId"] = "";
                    }
                    
                }
                //仅当父级变动
                if (oriOrganization.ParentId != newParentId)
                {
                    var childIds = (await manager.FindChildrenAsync(oriOrganization.Id, true)).Select(o => o.Id).ToList();
                    if (oriOrganization.Id == newParentId)
                    {
                        throw new UserFriendlyException("不允许设置父级为自己");
                    }
                    else if (newParentId != null && childIds.Contains(newParentId.Value))
                    {
                        throw new UserFriendlyException("不允许设置父级为子级");
                    }
                    if (newParentId == null)
                    {
                        await manager.MoveAsync(id, null);
                    }
                    else
                    {
                        await manager.MoveAsync(id, newParentId.Value);
                    }
                }
                organization = await manager.GetByIdAsync(id);
                organization = await manager.LoadEntityFromDatas(request.Datas,organization);
                organization.SetData("ExtendData1", request.Datas["ExtendData1"]);
                organization.SetData("ExtendData2", request.Datas["ExtendData2"]);
                organization.SetData("ExtendData3", request.Datas["ExtendData3"]);

                await manager.UpdateAsync(organization);
            }
            
        }

        public virtual async Task<object> GetTreeJson(int? parentId,int maxLevel=0)
        {
            var manager = Manager as OrganizationManager;
            var ous = await manager.FindChildrenAsync(parentId, true);
            if (maxLevel > 0)
            {
                ous = ous.Where(o => o.Code.ToCharArray().Count(c => c == '.') < maxLevel).ToList();
            }
            return ous.Select(o =>
            {
                var dto = o.MapTo<OrganizationDto>();
                dto.ExtendData1 = o.GetData<string>("ExtendData1");
                dto.ExtendData2 = o.GetData<string>("ExtendData2");
                dto.ExtendData3 = o.GetData<string>("ExtendData3");

                return dto;
            }
            );
        }

        public virtual async Task MoveUserIntoOrganization(int organizationId,long[] userIds)
        {
            var users = await UserManager.GetListByIdsAsync(userIds.AsEnumerable());
            if (users.Count(o => o.OrganizationId != null) > 0)
            {
                throw new UserFriendlyException("用户无法加入到两个组织机构中");
            }

            foreach(var user in users)
            {
                user.OrganizationId = organizationId;
            }
        }

        public virtual async Task MoveUserOutOrganization(long[] userIds)
        {
            var users = await UserManager.GetListByIdsAsync(userIds.AsEnumerable());
            foreach(var user in users)
            {
                user.OrganizationId = null;
            }
        }
    }
}
