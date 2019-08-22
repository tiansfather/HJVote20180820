using Abp.Configuration;
using Abp.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Configuration
{
    public static class SettingNames
    {
        public const string MenuSetting = "Menu";
        public const string SoftTitle = "App.SoftTitle";
    }
    public class MasterSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            var interGroup = new SettingDefinitionGroup("InterSetting", L("内部设置"));
            var menuSettingDefinition = new SettingDefinition(SettingNames.MenuSetting, "", L("菜单"), group: interGroup, scopes: SettingScopes.Tenant | SettingScopes.User);


            var group = new SettingDefinitionGroup("Core", L("基本设置"));
            return new[]
            {
                menuSettingDefinition,
                new SettingDefinition(SettingNames.SoftTitle, "管理系统",L("系统标题"),group, scopes: SettingScopes.Tenant , isVisibleToClients: true)
            };
        }

        private static LocalizableString L(string name)
        {
            return new LocalizableString(name, MasterConsts.LocalizationSourceName);
        }
    }
}
