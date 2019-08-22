using Abp.AutoMapper;
using Abp.Localization;
using Master.Dto;
using Master.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.Menu
{
    public class MenuAppService: MasterAppServiceBase, IMenuAppService
    {
        private readonly IMenuManager _menuManager;
        private readonly ILocalizationContext _localizationContext;
        public MenuAppService(IMenuManager menuManager,ILocalizationContext localizationContext)
        {
            _menuManager = menuManager;
            _localizationContext = localizationContext;
        }

        /// <summary>
        /// 获取用户的菜单
        /// </summary>
        /// <returns></returns>
        public object GetMenuTreeJson()
        {
            List<MenuTreeDto> menuTreeDtos = new List<MenuTreeDto>();
            //获取用户设置的菜单
            string usermenudata = _menuManager.LoadUserSettingMenusData(AbpSession.UserId);
            //有设定的菜单
            if(!string.IsNullOrEmpty(usermenudata))
            {

                menuTreeDtos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MenuTreeDto>>(usermenudata);
            }
            //没有从默认的菜单中获取
            else
            {
                var menu = _menuManager.LoadDefaultMenus();
                menuTreeDtos = menu.MapTo<List<MenuTreeDto>>();
            }


            
            return menuTreeDtos;
        }

        /// <summary>
        /// 获取用户的初始格式的菜单
        /// </summary>
        /// <returns></returns>
        public List<Abp.Application.Navigation.MenuItemDefinition> GetDefinitionMenu()
        {
            List<Abp.Application.Navigation.MenuItemDefinition> menuItemDefinitions  = new List<Abp.Application.Navigation.MenuItemDefinition>();
            //获取用户设置的菜单
            string usermenudata = _menuManager.LoadUserSettingMenusData(AbpSession.UserId);
            //有设定的菜单
            if (!string.IsNullOrEmpty(usermenudata))
            {

                var menuTreeDtos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MenuTreeDto>>(usermenudata);

                menuItemDefinitions = GetMenuItemDefinitionByMenuTreeDto(menuTreeDtos).ToList();
            }
            //没有从默认的菜单中获取
            else
            {
                menuItemDefinitions = _menuManager.LoadDefaultMenus();
            }



            return menuItemDefinitions;
        }
        /// <summary>
        /// 获取用户可见菜单
        /// </summary>
        /// <param name="userIdentifier"></param>
        /// <returns></returns>
        public async  Task<List<CustomUserMenuItem>> GetUserMenu(Abp.UserIdentifier userIdentifier)
        {
            var userMenus = new List<CustomUserMenuItem>();
            var menus = GetDefinitionMenu();
            await _menuManager.FillUserMenuItems(userIdentifier, menus, userMenus);

            return userMenus;
        }




        /// <summary>
        /// 获取默认菜单
        /// </summary>
        /// <returns></returns>
        public object GetDefaultMenusTreeJson()
        {
            var menu = _menuManager.LoadDefaultMenus();
            return menu.MapTo<List<MenuTreeDto>>();
        }

        /// <summary>
        /// 保存设置的菜单
        /// </summary>
        /// <param name="menuTreeDtos"></param>
        /// <returns></returns>
        public async Task SaveMenuSetting(List<MenuTreeDto> menuTreeDtoList)
        {

            if (menuTreeDtoList.Count >0)
            {
                var settingStr = Newtonsoft.Json.JsonConvert.SerializeObject(menuTreeDtoList);

                //var menu = GetMenuItemDefinitionByMenuTreeDto(menuTreeDtoList).ToList();
                await _menuManager.SaveSettingMenusAsync(null, settingStr);
            }
        }

        public IList<Abp.Application.Navigation.MenuItemDefinition> GetMenuItemDefinitionByMenuTreeDto(IList<MenuTreeDto> menuTreeDtos)
        {
            IList<Abp.Application.Navigation.MenuItemDefinition> menuItemDefinitions = new List<Abp.Application.Navigation.MenuItemDefinition>();
            foreach(var menuTreeDto in menuTreeDtos)
            {
                var temp= _menuManager.LoadMenusByName(menuTreeDto.Name);
                if (temp != null)
                {
                    temp.DisplayName = new LocalizableString(menuTreeDto.DisplayName, MasterConsts.LocalizationSourceName);

                    if (menuTreeDto.Items.Count > 0)
                    {
                        var items = GetMenuItemDefinitionByMenuTreeDto(menuTreeDto.Items);
                        foreach (var item in items)
                        {
                            temp.Items.Add(item);
                        }
                    }
                    menuItemDefinitions.Add(temp);
                }
            }
            return menuItemDefinitions;
        }

    }
}
