using Master.Session.Dto;
using Master.Web.Components;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master.Web.Views.Shared.Components.TopBarNavExpert
{
    public class TopBarNav2ViewComponent: MasterViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(LoginInformationDto loginInformationDto)
        {
            //var userMenus = await _menuManager.LoadUserMenu(_abpSession.ToUserIdentifier());
            //var userMenus = await _menuAppService.GetUserMenu(_abpSession.ToUserIdentifier());
            //var mainMenu = await _userNavigationManager.GetMenuAsync("MainMenu", _abpSession.ToUserIdentifier());


            return View(loginInformationDto);
        }
    }
}
