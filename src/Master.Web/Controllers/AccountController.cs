using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Controllers;
using Master.MultiTenancy;
using Master.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace Master.Web.Controllers
{
    public class AccountController : MasterControllerBase
    {
        private readonly TenantManager _tenantManager;
        public AccountController(
            TenantManager tenantManager
            )
        {
            _tenantManager = tenantManager;
        }
        public async Task<ActionResult> Login(string userName = "", string currentTenancyName = "", string returnUrl = "")
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Action("Index","Home");
            }
            var allTenants =await _tenantManager.All();

            return View(new LoginFormViewModel
            {
                UserName=userName,
                ReturnUrl = returnUrl,
                Tenants = allTenants,
                CurrentTenancyName = currentTenancyName
            });
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return RedirectToAction("Login");
        }
    }
}