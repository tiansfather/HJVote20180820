using Abp.Application.Navigation;
using Master.Menu;
using System.Collections.Generic;

namespace Master.Web.Views.Shared.Components.SideBarNav
{
    public class SideBarNavViewModel
    {
        public IList<CustomUserMenuItem> MenuItems { get; set; }

        public string ActiveMenuItemName { get; set; }
    }
}
