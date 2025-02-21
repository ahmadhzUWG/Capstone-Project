using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/{id}
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Display list of groups
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        public IActionResult CreateGroup()
        {
            var users = _context.Users.ToList();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            var managers = users.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            var employees = users.Where(user => userManager.IsInRoleAsync(user, "Employee").Result).ToList();

            ViewBag.Managers = managers;
            ViewBag.Employees = employees;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(Group group, List<int> selectedManagers, List<int> selectedUsers, int primaryManagerId)
        {
            if (!selectedManagers.Contains(primaryManagerId))
            {
                ModelState.AddModelError("primaryManagerId", "Primary Manager must be one of the selected managers.");
            }

            if (ModelState.IsValid)
            {
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                var groupManagers = selectedManagers.Select(managerId => new GroupManager
                {
                    GroupId = group.Id,
                    UserId = managerId,
                    IsPrimaryManager = (managerId == primaryManagerId)
                }).ToList();

                if (!groupManagers.Any(gm => gm.IsPrimaryManager))
                {
                    ModelState.AddModelError("primaryManagerId", "At least one manager must be designated as the primary manager.");
                    return View(group); // Return view with validation error
                }

                _context.Set<GroupManager>().AddRange(groupManagers);

                var users = await _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();
                group.Users = users;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Groups));
            }

            var usersList = await _context.Users.ToListAsync();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            ViewBag.Managers = usersList.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            ViewBag.Employees = usersList.Where(user => userManager.IsInRoleAsync(user, "Employee").Result).ToList();

            return View(group);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Groups));
        }

        public async Task<IActionResult> ManageGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        public async Task<IActionResult> GroupDetails(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .Include(g => g.Managers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            ViewBag.Users = await _context.Users.ToListAsync();
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            var group = await _context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _context.Users.FindAsync(userId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (!group.Users.Contains(user))
            {
                group.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManagerToGroup(int groupId, int managerId, bool isPrimary)
        {
            var group = await _context.Groups.Include(g => g.Managers).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _context.Users.FindAsync(managerId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (isPrimary)
            {
                foreach (var manager in group.Managers)
                {
                    manager.IsPrimaryManager = false;
                }
            }

            if (!group.Managers.Any(m => m.UserId == managerId))
            {
                group.Managers.Add(new GroupManager
                {
                    GroupId = groupId,
                    UserId = managerId,
                    IsPrimaryManager = isPrimary
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }


        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            string currentRole = userRoles.FirstOrDefault();

            ViewBag.Roles = roles;
            ViewBag.CurrentRole = currentRole;

            return View(user);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string UserName, string Email, string Role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = UserName;
            user.Email = Email;
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            if (!string.IsNullOrEmpty(Role))
            {
                await _userManager.AddToRoleAsync(user, Role);
            }

            return RedirectToAction("Users");
        }

        // GET: Users/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/{id}
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
    }
}
