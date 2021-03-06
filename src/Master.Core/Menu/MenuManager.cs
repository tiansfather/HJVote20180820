﻿using Abp;
using Abp.Application.Features;
using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.Extensions;
using Abp.Localization;
using Abp.Runtime.Caching;
using Master.Configuration;
using Master.Domain;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Master.Menu
{
    public class MenuManager : DomainServiceBase, IMenuManager
    {
        private static List<MenuItemDefinition> asmMenus = null;
        private readonly ILocalizationContext _localizationContext;
        private readonly IIocResolver _iocResolver;

        public MenuManager( IIocResolver iocResolver, ILocalizationContext localizationContext)
        {
            _localizationContext = localizationContext;
            _iocResolver = iocResolver;
        }

        #region Private

        private string ReadEmbedString(Assembly asm, string path)
        {
            //从配置文件读取
            var fileProvider = new EmbeddedFileProvider(asm);
            byte[] buffer;
            var fileInfo = fileProvider.GetFileInfo(path);
            if (fileInfo.Exists)
            {
                using (Stream readStream = fileInfo.CreateReadStream())
                {
                    buffer = new byte[readStream.Length];
                    readStream.Read(buffer, 0, buffer.Length);
                }
                return Encoding.UTF8.GetString(buffer);
            }
            //如果文件不存在
            return string.Empty;

        }
        private List<MenuItemDefinition> BuildFromMenuJson(Assembly asm, string jsonName)
        {
            var menuContent = ReadEmbedString(asm, jsonName);
            if (!string.IsNullOrEmpty(menuContent))
            {
                var menuList = LoadFromJarray(Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(menuContent));
                return menuList;
            }
            return null;
        }

        protected List<MenuItemDefinition> LoadFromJarray(JArray arr)
        {
            var menus = new List<MenuItemDefinition>();
            foreach (JToken token in arr)
            {
                var menu = new MenuItemDefinition(
                    token["name"].ToString(),
                    L((token["displayName"] ?? token["name"]).ToString()),
                    url: token["url"]?.ToString(),
                    icon: token["icon"]?.ToString(),
                    requiresAuthentication: (token["requiredPermissionName"]?.ToString()).IsNullOrWhiteSpace() ? false : true,
                    requiredPermissionName: token["requiredPermissionName"]?.ToString()
                    );
                if (token["items"] != null)
                {
                    if ((token["items"] as JArray).Count > 0)
                    {
                        foreach (var item in LoadFromJarray(token["items"] as JArray))
                        {
                            menu.Items.Add(item);
                        }
                    }
                }

                menus.Add(menu);
            }
            return menus;
        }
        private List<MenuItemDefinition> LoadMenuFromAsms()
        {
            if (asmMenus != null)
            {
                return asmMenus;
            }
            else
            {
                var result = new List<MenuItemDefinition>();
                var asms = AppDomain.CurrentDomain.GetAssemblies().Where(o => o.FullName.StartsWith("Master"));
                foreach (var asm in asms)
                {
                    var menu = BuildFromMenuJson(asm, "menu.json");
                    if (menu != null)
                    {
                        result.AddRange(menu);
                    }

                }
                asmMenus = result;
                return result;
            }

        }
        #endregion

        public List<MenuItemDefinition> LoadDefaultMenus()
        {
            return CacheManager.GetCache<string, List<MenuItemDefinition>>("DefaultMenus")
                .Get("", () => {
                    var result = new List<MenuItemDefinition>();
                    //内置菜单
                    result.AddRange(LoadMenuFromAsms());

                    return result;
                });

            
        }

        /// <summary>
        /// 获取所有的菜单权限集合
        /// </summary>
        /// <returns></returns>
        public IList<Permission> GetAllMenuPermissions()
        {
            var menus = LoadDefaultMenus();

            return GetMenuPermissionsInner(menus);
        }

        private IList<Permission> GetMenuPermissionsInner(IList<MenuItemDefinition> menus)
        {
            var permissions = new List<Permission>();
            foreach (var menu in menus)
            {
                if (menu.IsLeaf)
                {
                    permissions.Add(new Permission("Menu." + menu.Name));
                }
                else
                {
                    permissions.AddRange(GetMenuPermissionsInner(menu.Items));
                }
            }
            return permissions;
        }
        /// <summary>
        /// 获取用户的菜单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //public List<MenuItemDefinition> LoadSettingMenus(long? userId)
        //{
        //    var settingStr = "";
        //    if (userId.HasValue)
        //    {
        //        //使用用户菜单
        //        settingStr = SettingManager.GetSettingValueForUser(AppSettingNames.MenuSetting, AbpSession.TenantId, userId.Value);
        //    }
        //    else if(AbpSession.TenantId.HasValue)
        //    {
        //        //使用租户级菜单
        //        settingStr = SettingManager.GetSettingValueForTenant(AppSettingNames.MenuSetting, AbpSession.TenantId.Value);
        //    }
        //    //settingStr = "";
        //    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MenuItemDefinition>>(settingStr);
        //    if (result == null)
        //    {
        //        result= LoadDefaultMenus();//使用默认菜单
        //    }
        //    return result;

        //}
        /// <summary>
        /// 获取用户设定的菜单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string LoadUserSettingMenusData(long? userId)
        {
            var settingStr = "";
            if (userId.HasValue)
            {
                //使用用户菜单
                settingStr = SettingManager.GetSettingValueForUser(SettingNames.MenuSetting, AbpSession.TenantId, userId.Value);
            }
            else if (AbpSession.TenantId.HasValue)
            {
                //使用租户级菜单
                settingStr = SettingManager.GetSettingValueForTenant(SettingNames.MenuSetting, AbpSession.TenantId.Value);
            }
            return settingStr;
        }

        public MenuItemDefinition LoadMenusByName(string name, IList<MenuItemDefinition> menus = null)
        {
            MenuItemDefinition rmenu = null;
            if (menus == null)
            {
                menus = LoadDefaultMenus();
            }
            foreach (var menu in menus)
            {
                if (menu.Name == name)
                {
                    //List<MenuItemDefinition> t = new List<MenuItemDefinition>();
                    //menu.Items = null;
                    rmenu = new MenuItemDefinition(menu.Name, menu.DisplayName, menu.Icon, menu.Url, menu.RequiresAuthentication, menu.RequiredPermissionName, menu.Order, menu.CustomData, menu.FeatureDependency, menu.Target, menu.IsEnabled, menu.IsVisible, menu.PermissionDependency);


                    break;
                }

                if (menu.Items.Count > 0)
                {
                    var tempmenu = LoadMenusByName(name, menu.Items);
                    if (tempmenu != null)
                    {
                        rmenu = new MenuItemDefinition(tempmenu.Name, tempmenu.DisplayName, tempmenu.Icon, tempmenu.Url, tempmenu.RequiresAuthentication, tempmenu.RequiredPermissionName, tempmenu.Order, tempmenu.CustomData, tempmenu.FeatureDependency, tempmenu.Target, tempmenu.IsEnabled, tempmenu.IsVisible, tempmenu.PermissionDependency);

                        //rmenu.Items.Clear();
                        break;
                    }
                }
            }
            return rmenu;
        }

        public async Task SaveSettingMenusAsync(long? userId, List<MenuItemDefinition> menuItems)
        {
            var settingStr = Newtonsoft.Json.JsonConvert.SerializeObject(menuItems);
            if (userId.HasValue)
            {
                var userIdentifier = new Abp.UserIdentifier(AbpSession.TenantId, userId.Value);
                await SettingManager.ChangeSettingForUserAsync(userIdentifier, SettingNames.MenuSetting, settingStr);
            }
            else if (AbpSession.TenantId.HasValue)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, SettingNames.MenuSetting, settingStr);
            }

        }

        public async Task SaveSettingMenusAsync(long? userId, string settingStr)
        {
            if (userId.HasValue)
            {
                var userIdentifier = new Abp.UserIdentifier(AbpSession.TenantId, userId.Value);
                await SettingManager.ChangeSettingForUserAsync(userIdentifier, SettingNames.MenuSetting, settingStr);
            }
            else if (AbpSession.TenantId.HasValue)
            {
                await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, SettingNames.MenuSetting, settingStr);
            }

        }

        //public virtual async Task<IList<CustomUserMenuItem>> LoadUserMenu(Abp.UserIdentifier userIdentifier)
        //{
        //    var menus = LoadSettingMenus(userIdentifier.UserId);
        //    var userMenus = new List<CustomUserMenuItem>();

        //    await FillUserMenuItems(userIdentifier, menus, userMenus);

        //    return userMenus;
        //}

        public async Task<int> FillUserMenuItems(UserIdentifier user, IList<MenuItemDefinition> menuItemDefinitions, IList<CustomUserMenuItem> userMenuItems)
        {
            //TODO: Can be optimized by re-using FeatureDependencyContext.

            var addedMenuItemCount = 0;
            
            using (var scope = _iocResolver.CreateScope())
            {
                var permissionDependencyContext = scope.Resolve<PermissionDependencyContext>();
                permissionDependencyContext.User = user;

                var featureDependencyContext = scope.Resolve<FeatureDependencyContext>();
                featureDependencyContext.TenantId = user == null ? null : user.TenantId;

                foreach (var menuItemDefinition in menuItemDefinitions)
                {
                    if (user == null)
                    {
                        continue;
                    }
                    if (menuItemDefinition.IsLeaf)
                    {
                        var permissionName = $"Menu.{menuItemDefinition.Name}";
                        var permissionDependency = new SimplePermissionDependency(permissionName);
                        if (!(await permissionDependency.IsSatisfiedAsync(permissionDependencyContext)))
                        {
                            continue;
                        }
                    }
                    //if (menuItemDefinition.RequiresAuthentication && user == null)
                    //{
                    //    continue;
                    //}

                    //if (!string.IsNullOrEmpty(menuItemDefinition.RequiredPermissionName))
                    //{
                    //    var permissionDependency = new SimplePermissionDependency(menuItemDefinition.RequiredPermissionName);
                    //    if (user == null || !(await permissionDependency.IsSatisfiedAsync(permissionDependencyContext)))
                    //    {
                    //        continue;
                    //    }
                    //}

                    //if (menuItemDefinition.PermissionDependency != null &&
                    //    (user == null || !(await menuItemDefinition.PermissionDependency.IsSatisfiedAsync(permissionDependencyContext))))
                    //{
                    //    continue;
                    //}

                    //if (menuItemDefinition.FeatureDependency != null &&
                    //    (AbpSession.MultiTenancySide == MultiTenancySides.Tenant || (user != null && user.TenantId != null)) &&
                    //    !(await menuItemDefinition.FeatureDependency.IsSatisfiedAsync(featureDependencyContext)))
                    //{
                    //    continue;
                    //}

                    var userMenuItem = new CustomUserMenuItem(menuItemDefinition, _localizationContext);
                    if (menuItemDefinition.IsLeaf || (await FillUserMenuItems(user, menuItemDefinition.Items, userMenuItem.Items)) > 0)
                    {
                        userMenuItems.Add(userMenuItem);
                        ++addedMenuItemCount;
                    }
                }
            }

            return addedMenuItemCount;
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, MasterConsts.LocalizationSourceName);
        }
    }
}
