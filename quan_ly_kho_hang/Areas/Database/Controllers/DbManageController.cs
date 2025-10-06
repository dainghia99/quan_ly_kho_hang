using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public DbManageController(
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            SignInManager<AppUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // ✅ Cho phép truy cập Index khi DB chưa có admin và role
        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem DB đã có role Administrator và user admin chưa
            var hasAdminRole = await _roleManager.RoleExistsAsync(RoleName.Administrator);
            var hasAdminUser = (await _userManager.FindByEmailAsync("admin@gmail.com")) != null;

            // Nếu chưa có admin hoặc role => cho phép truy cập tự do để seed lần đầu
            if (!hasAdminRole || !hasAdminUser)
            {
                ViewBag.AllowSeed = true;
                return View();
            }

            // Nếu đã có admin + role => chỉ admin mới được truy cập
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(RoleName.Administrator))
                return Forbid();

            ViewBag.AllowSeed = false;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult DeleteDb()
        {
            return View();
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> SeedDataAsync()
        {
            // Tạo các Role mặc định
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if (rfound == null)
                {
                    await _roleManager.CreateAsync(new AppRole(rolename));
                }
            }

            // Tạo user admin mặc định nếu chưa có
            var useradmin = await _userManager.FindByEmailAsync("admin@gmail.com");
            if (useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(useradmin, "admin123");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
                await _signInManager.SignInAsync(useradmin, false);

                StatusMessage = "Đã tạo tài khoản admin mặc định.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu đã có admin => chỉ Administrator mới được phép chạy lại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(RoleName.Administrator))
                return Forbid();

            StatusMessage = "Vừa seed Database.";
            return RedirectToAction(nameof(Index));
        }
    }
}
