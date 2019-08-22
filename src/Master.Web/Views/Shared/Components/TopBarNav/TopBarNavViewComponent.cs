using Abp.Application.Navigation;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Master.Menu;
using Master.Web.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Session.Dto;

namespace Master.Web.Views.Shared.Components.TopBarNav
{
    public class TopBarNavViewComponent:MasterViewComponent
    {
        private readonly IAbpSession _abpSession;
        private readonly IMenuAppService _menuAppService;

        public TopBarNavViewComponent(
            IUserNavigationManager userNavigationManager,
            IAbpSession abpSession, IMenuAppService menuAppService)
        {
            _abpSession = abpSession;
            _menuAppService = menuAppService;
        }

        public async Task<IViewComponentResult> InvokeAsync(LoginInformationDto loginInformationDto)
        {
            //var userMenus = await _menuManager.LoadUserMenu(_abpSession.ToUserIdentifier());
            //var userMenus = await _menuAppService.GetUserMenu(_abpSession.ToUserIdentifier());
            //var mainMenu = await _userNavigationManager.GetMenuAsync("MainMenu", _abpSession.ToUserIdentifier());


            return View(loginInformationDto);
        }
    }
}
