using Abp.Application.Navigation;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master.Menu
{
    [AutoMap(typeof(MenuItemDefinition))]
    public class MenuTreeDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public IList<MenuTreeDto> Items { get; set; }
    }
}
