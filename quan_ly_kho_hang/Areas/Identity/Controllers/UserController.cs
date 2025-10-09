using App.Areas.Identity.Models.RoleViewModels;
using App.Areas.Identity.Models.UserViewModels;
using App.Data;
using App.ExtendMethods;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using quan_ly_kho_hang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Areas.Identity.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<UserController> logger, RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            int ITEMS_PER_PAGE = 10;
            var allUsers = _userManager.Users.ToList();
            int totalUsers = allUsers.Count;
            int countPages = (int)Math.Ceiling((double)totalUsers / ITEMS_PER_PAGE);

            var usersPage = allUsers
                .Skip((page - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            var model = new UserListModel
            {
                totalUsers = totalUsers,
                countPages = countPages,
                currentPage = page,
                users = new List<UserAndRole>()
            };

            foreach (var user in usersPage)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.users.Add(new UserAndRole
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleNames = roles.Count > 0 ? string.Join(", ", roles) : string.Empty
                });
            }

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không có user");

            var model = new AddUserRoleModel();
            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null) return NotFound($"Không thấy user, id = {id}.");

            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();
            var roleNames = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.allRoles = new SelectList(roleNames);

            await GetClaims(model);

            return View(model);
        }

        // POST: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, [Bind("RoleNames")] AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không có user");

            model.user = await _userManager.FindByIdAsync(id);
            if (model.user == null) return NotFound($"Không thấy user, id = {id}.");

            var oldRoles = (await _userManager.GetRolesAsync(model.user)).ToArray();
            var deleteRoles = oldRoles.Where(r => !model.RoleNames.Contains(r));
            var addRoles = model.RoleNames.Where(r => !oldRoles.Contains(r));

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user, deleteRoles);
            if (!resultDelete.Succeeded)
            {
                ModelState.AddModelError(string.Join(";", resultDelete.Errors.Select(e => e.Description)));
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(model.user, addRoles);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError(string.Join(";", resultAdd.Errors.Select(e => e.Description)));
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật role cho user: {model.user.UserName}";
            return RedirectToAction("Index");
        }

        // GET: /ManageUser/SetPassword/id
        [HttpGet("{id}")]
        public async Task<IActionResult> SetPasswordAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không có user");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound($"Không thấy user, id = {id}.");

            ViewBag.user = user;
            return View();
        }

        // POST: /ManageUser/SetPassword/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không có user");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound($"Không thấy user, id = {id}.");

            if (!ModelState.IsValid) return View(model);

            await _userManager.RemovePasswordAsync(user);
            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);

            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            StatusMessage = $"Vừa cập nhật mật khẩu cho user: {user.UserName}";
            return RedirectToAction("Index");
        }

        // GET: /ManageUser/AddClaim/userid
        [HttpGet("{userid}")]
        public async Task<IActionResult> AddClaimAsync(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            ViewBag.user = user;
            return View();
        }

        // POST: /ManageUser/AddClaim/userid
        [HttpPost("{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClaimAsync(string userid, AddUserClaimModel model)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            if (!ModelState.IsValid) return View(model);

            var claims = await _userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == model.ClaimType && c.Value == model.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Đặc tính này đã có");
                return View(model);
            }

            await _userManager.AddClaimAsync(user, new Claim(model.ClaimType, model.ClaimValue));

            StatusMessage = "Đã thêm đặc tính cho user";
            return RedirectToAction("AddRole", new { id = user.Id });
        }

        // POST: /ManageUser/DeleteClaim/claimType/claimValue
        [HttpPost("{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaimAsync(string userid, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            await _userManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));

            StatusMessage = "Bạn đã xóa claim";
            return RedirectToAction("AddRole", new { id = user.Id });
        }

        private async Task GetClaims(AddUserRoleModel model)
        {
            // Lấy claims từ vai trò
            var roles = await _userManager.GetRolesAsync(model.user);
            var claimsInRole = new List<Claim>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    claimsInRole.AddRange(roleClaims);
                }
            }

            model.claimsInRole = claimsInRole.Select(c => new IdentityRoleClaim<string>
            {
                ClaimType = c.Type,
                ClaimValue = c.Value
            }).ToList();

            // Lấy claims trực tiếp của user
            var userClaims = await _userManager.GetClaimsAsync(model.user);
            model.claimsInUserClaim = userClaims.Select(c => new IdentityUserClaim<string>
            {
                ClaimType = c.Type,
                ClaimValue = c.Value
            }).ToList();
        }
    }
}
