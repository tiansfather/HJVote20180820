using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Domain.Repositories;
using Master.Authentication;
using Master.Controllers;
using Master.Majors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.Web.Controllers
{
    [AbpMvcAuthorize("Menu.systemmanager.user")]
    public class UserController : MasterControllerBase
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly SpecialityManager _specialityManager;
        public UserController(
            RoleManager roleManager,
            UserManager userManager,
            SpecialityManager specialityManager
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _specialityManager = specialityManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.GetAll().ToListAsync();
            ViewData["roles"] = roles;
            var specialities = (await _specialityManager.GetAll().ToListAsync()).OrderBy(o => o.Sort).ToList();
            ViewData["specialities"] = specialities;
            return View();
        }

        public async Task<IActionResult> Add()
        {
            var roles = await _roleManager.GetAll().ToListAsync();
            var specialities = (await _specialityManager.GetAll().ToListAsync()).OrderBy(o=>o.Sort).ToList();
            ViewData["roles"] = roles;
            ViewData["specialities"] = specialities;
            return View();
        }

        public async Task<IActionResult> Edit(string data)
        {
            var user = await _userManager.GetByIdAsync(Convert.ToInt32(data));

            await _userManager.Repository.EnsureCollectionLoadedAsync(user, o => o.Roles);
            await _userManager.Repository.EnsureCollectionLoadedAsync(user, o => o.Specialities);

            var roles = await _roleManager.GetAll().ToListAsync();
            var specialities = (await _specialityManager.GetAll().ToListAsync()).OrderBy(o => o.Sort).ToList();
            ViewData["roles"] = roles;
            ViewData["specialities"] = specialities;
            return View(user);
        }

    }
}