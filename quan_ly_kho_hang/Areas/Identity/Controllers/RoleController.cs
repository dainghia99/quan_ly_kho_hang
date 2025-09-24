using App.Areas.Identity.Models.RoleViewModels;
using App.Data;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using quan_ly_kho_hang.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.Areas.Identity.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/Role/[action]")]
    public class RoleController : Controller
    {
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(ILogger<RoleController> logger, RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /Role/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rolesList = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var roles = new List<RoleModel>();

            foreach (var role in rolesList)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var claimsString = claims.Select(c => $"{c.Type}={c.Value}");

                roles.Add(new RoleModel
                {
                    Name = role.Name,
                    Id = role.Id,
                    Claims = claimsString.ToArray()
                });
            }

            return View(roles);
        }

        // GET: /Role/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Role/Create
        [HttpPost, ActionName(nameof(Create))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateRoleModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var newRole = new AppRole(model.Name);
            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa tạo role mới: {model.Name}";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Description)));
            return View();
        }

        // GET: /Role/Delete/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> DeleteAsync(string roleid)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            return View(role);
        }

        // POST: /Role/Delete/roleid
        [HttpPost("{roleid}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmAsync(string roleid)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa: {role.Name}";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Description)));
            return View(role);
        }

        // GET: /Role/Edit/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> EditAsync(string roleid, [Bind("Name")] EditRoleModel model)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            model.Name = role.Name;
            var claims = await _roleManager.GetClaimsAsync(role);
            model.Claims = claims.Select(c => new IdentityRoleClaim<string> { ClaimType = c.Type, ClaimValue = c.Value }).ToList();
            model.role = role;

            ModelState.Clear();
            return View(model);
        }

        // POST: /Role/Edit/roleid
        [HttpPost("{roleid}"), ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmAsync(string roleid, [Bind("Name")] EditRoleModel model)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            if (!ModelState.IsValid)
                return View(model);

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa đổi tên: {model.Name}";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Description)));
            return View(model);
        }

        // GET: /Role/AddRoleClaim/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> AddRoleClaimAsync(string roleid)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            return View(new EditClaimModel { role = role });
        }

        // POST: /Role/AddRoleClaim/roleid
        [HttpPost("{roleid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleClaimAsync(string roleid, [Bind("ClaimType", "ClaimValue")] EditClaimModel model)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            if (!ModelState.IsValid) return View(model);

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            if (existingClaims.Any(c => c.Type == model.ClaimType && c.Value == model.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
                return View(model);
            }

            var result = await _roleManager.AddClaimAsync(role, new Claim(model.ClaimType, model.ClaimValue));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Description)));
                return View(model);
            }

            StatusMessage = "Vừa thêm đặc tính (claim) mới";
            return RedirectToAction("Edit", new { roleid = role.Id });
        }

        // POST: /Role/DeleteClaim/roleid
        [HttpPost("{roleid}/DeleteClaim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaimAsync(string roleid, string claimType, string claimValue)
        {
            if (string.IsNullOrEmpty(roleid)) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Không tìm thấy role");

            var result = await _roleManager.RemoveClaimAsync(role, new Claim(claimType, claimValue));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, string.Join("; ", result.Errors.Select(e => e.Description)));
                return View();
            }

            StatusMessage = "Vừa xóa claim";
            return RedirectToAction("Edit", new { roleid = role.Id });
        }
    }
}
