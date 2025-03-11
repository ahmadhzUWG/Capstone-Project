using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskManager.Tests;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Xunit;

public class AdminControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
    private readonly ApplicationDbContext _dbContext;

    public AdminControllerTests()
    {
        _mockUserManager = GetMockUserManager();
        _mockRoleManager = GetMockRoleManager();
        _dbContext = TestHelper.GetDbContext();

        var mockRoles = new List<IdentityRole<int>>
        {
            new IdentityRole<int> { Id = 1, Name = "Admin" },
            new IdentityRole<int> { Id = 2, Name = "Employee" }
        }.AsQueryable();
        _mockRoleManager.Setup(rm => rm.Roles).Returns(mockRoles);

        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "Admin", "Manager", "Employee" });
    }

    private Mock<UserManager<User>> GetMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(userStoreMock.Object,
            null, null, null, null, null, null, null, null);
    }

    private Mock<RoleManager<IdentityRole<int>>> GetMockRoleManager()
    {
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
        return new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object,
            null, null, null, null);
    }

    // ––– Tests for User-related endpoints –––
    [Fact]
    public async Task UserAdd_InvalidModelState_ReturnsViewWithSameModel()
    {
        // Arrange
        var controller = new AdminController(this._dbContext, this._mockUserManager.Object, this._mockRoleManager.Object);

        controller.ModelState.AddModelError("UserName", "UserName is required");

        var viewModel = new UserViewModel
        {
            UserName = "",
            Email = "test@example.com",
            Password = "password",
            ConfirmPassword = "password"
        };

        // Act
        var result = await controller.UserAdd(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task Users_ReturnsViewWithUserList()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Users.Add(new User { Id = 1, UserName = "User1", Email = "user1@example.com" });
        dbContext.Users.Add(new User { Id = 2, UserName = "User2", Email = "user2@example.com" });
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.Users();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UserEdit_ReturnsNotFound_WithNoFoundUser()
    {
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var result = await controller.UserEdit("999");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserEdit_ReturnsViewResult_WithValidUser()
    {
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var user = new User { Id = 1, UserName = "User1", Email = "user1@example.com" };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        _mockRoleManager.Setup(rm => rm.Roles).Returns(new List<IdentityRole<int>>
        {
            new IdentityRole<int> { Name = "Admin" },
            new IdentityRole<int> { Name = "User" }
        }.AsQueryable());

        var result = await controller.UserEdit(user.Id.ToString());

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<User>(viewResult.Model);
        Assert.Equal(user.Id, model.Id);
        Assert.Equal(user.UserName, model.UserName);

        Assert.NotNull(controller.ViewBag.Roles);
        Assert.Contains("Admin", controller.ViewBag.Roles);
        Assert.Equal("Admin", controller.ViewBag.CurrentRole);
    }

    [Fact]
    public async Task UserAdd_ReturnsViewResult()
    {
        var controller = new AdminController(_dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var result = controller.UserAdd();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task UserDetails_ReturnsViewWithUser_IfExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var user = new User { Id = 10, UserName = "DetailUser", Email = "detail@example.com" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.UserDetails(10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelUser = Assert.IsType<User>(viewResult.Model);
        Assert.Equal("DetailUser", modelUser.UserName);
    }

    [Fact]
    public async Task UserDetails_ReturnsNotFound_IfUserDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.UserDetails(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserDelete_ReturnsViewIfUserExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var user = new User { Id = 100, UserName = "DeleteUser", Email = "delete@example.com" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.UserDelete(100);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelUser = Assert.IsType<UserDeleteViewModel>(viewResult.Model);
        Assert.Equal("DeleteUser", modelUser.User.UserName);
    }

    [Fact]
    public async Task UserDelete_ReturnsNotFoundIfUserDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.UserDelete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_DeletesUserAndRedirects()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var user = new User { Id = 200, UserName = "ConfirmedUser", Email = "confirmed@example.com" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.DeleteConfirmed(200);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.False(dbContext.Users.Any(u => u.Id == 200));
    }

    // ––– Tests for Group-related endpoints –––

    [Fact]
    public async Task Groups_ReturnsViewWithGroupList()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Groups.Add(new Group { Id = 1, Name = "Group1", Description = "Test Description" });
        dbContext.Groups.Add(new Group { Id = 2, Name = "Group2", Description = "Test Description" });
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.Groups();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var groups = Assert.IsAssignableFrom<IEnumerable<Group>>(viewResult.Model);
        Assert.Equal(2, groups.Count());
    }

    [Fact]
    public void CreateGroup_Get_ReturnsViewWithManagersAndEmployees()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var employeeUser = new User { Id = 2, UserName = "EmployeeUser", Email = "employee@example.com" };
        dbContext.Users.Add(employeeUser);
        dbContext.SaveChanges();

        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 1), "Manager"))
            .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 2), "Manager"))
            .ReturnsAsync(false);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 1), "Employee"))
            .ReturnsAsync(false);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 2), "Employee"))
            .ReturnsAsync(true);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton(_mockUserManager.Object)
            .BuildServiceProvider();
        httpContext.RequestServices = services;

        controller.ControllerContext.HttpContext = httpContext;
        controller.TempData = new TempDataDictionary(httpContext, services.GetRequiredService<ITempDataProvider>());

        // Act
        var result = controller.CreateGroup();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.ViewData["Employees"]);
        var employees = viewResult.ViewData["Employees"] as List<User>;
        Assert.Single(employees);
        Assert.Equal("EmployeeUser", employees.First().UserName);
    }

    [Fact]
    public async Task GroupDetails_ReturnsNotFoundIfGroupDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.GroupDetails(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GroupDetails_ReturnsViewWithUsersAndManagers()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group
        {
            Id = 40,
            Name = "GroupDetails",
            Description = "Test Description"
        };
        dbContext.Groups.Add(group);

        var managerUser = new User { Id = 1, UserName = "ManagerUser", Email = "manager@example.com" };
        var employeeUser = new User { Id = 2, UserName = "EmployeeUser", Email = "employee@example.com" };
        dbContext.Users.AddRange(managerUser, employeeUser);

        dbContext.UserGroups.Add(new UserGroup { GroupId = 40, UserId = 1, Role = "Manager" });
        dbContext.UserGroups.Add(new UserGroup { GroupId = 40, UserId = 2, Role = "Member" });

        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.GroupDetails(40);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelGroup = Assert.IsType<Group>(viewResult.Model);
        Assert.Equal("GroupDetails", modelGroup.Name);

        Assert.NotNull(viewResult.ViewData["GroupUsers"]);
        var groupUsers = viewResult.ViewData["GroupUsers"] as List<UserGroup>;
        Assert.NotNull(groupUsers);
        Assert.Equal(2, groupUsers.Count);

        Assert.Contains(groupUsers, gu => gu.UserId == 1 && gu.Role == "Manager");
        Assert.Contains(groupUsers, gu => gu.UserId == 2 && gu.Role == "Member");
    }

    [Fact]
    public async Task CreateGroup_Post_InvalidPrimaryManager_ReturnsViewWithError()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();

        var groupViewModel = new GroupViewModel
        {
            Name = "NewGroup",
            Description = "Test Description",
            SelectedManagerId = 9,
            SelectedUserIds = [3]
        };
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton<UserManager<User>>(_mockUserManager.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;
        controller.ControllerContext.HttpContext = httpContext;

        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        // Act
        var result = await controller.CreateGroup(groupViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task CreateGroup_Post_Valid_ReturnsRedirect()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var primaryManager = new User { Id = 1, UserName = "Manager", Email = "pm@example.com" };
        var otherManager = new User { Id = 2, UserName = "OtherManager", Email = "om@example.com" };
        var employee = new User { Id = 3, UserName = "Employee", Email = "emp@example.com" };
        dbContext.Users.AddRange(primaryManager, otherManager, employee);
        await dbContext.SaveChangesAsync();

        var groupViewModel = new GroupViewModel
        {
            Name = "NewGroup",
            Description = "Test Description",
            SelectedManagerId = 1,
            SelectedUserIds = [3]
        };

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton<UserManager<User>>(_mockUserManager.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        controller.ControllerContext.HttpContext = httpContext;

        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        var mockUrlHelper = new Mock<IUrlHelper>();
        controller.Url = mockUrlHelper.Object;

        var result = await controller.CreateGroup(groupViewModel);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Groups", redirectResult.ActionName);
        Assert.Equal(1, groupViewModel.SelectedManagerId);
        Assert.Contains(3, groupViewModel.SelectedUserIds);
    }

    [Fact]
    public async Task CreateGroup_InvalidModelState_ReturnsViewWithEmployees()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Users.Add(new User { Id = 1, UserName = "User1", Email = "user1@example.com" });
        dbContext.Users.Add(new User { Id = 2, UserName = "User2", Email = "user2@example.com" });
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var model = new GroupViewModel
        {
            Name = "",
            Description = "Some Description",
            SelectedManagerId = 1,
            SelectedUserIds = new List<int> { 2 }
        };

        controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await controller.CreateGroup(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);

        var employees = viewResult.ViewData["Employees"] as List<User>;
        Assert.NotNull(employees);
        Assert.True(employees.Count >= 2);
    }

    [Fact]
    public async Task DeleteGroup_RemovesGroupIfExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group { Id = 20, Name = "GroupToDelete", Description = "Test Description" };
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.DeleteGroup(20);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.False(dbContext.Groups.Any(g => g.Id == 20));
    }

    [Fact]
    public async Task DeleteGroup_RedirectsIfGroupNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.DeleteGroup(999);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DeleteGroup_GroupAssignedToProject_ReturnsRedirectWithErrorMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        var group = new Group { Id = 1, Name = "Test Group", Description = "Test" };
        dbContext.Groups.Add(group);
        dbContext.GroupProjects.Add(new GroupProject { GroupId = 1, ProjectId = 1 });
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext,
            new Mock<ITempDataProvider>().Object);

        // Act
        var result = await controller.DeleteGroup(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Groups", redirectResult.ActionName);
        Assert.Equal("This group is assigned to one or more projects and cannot be deleted.",
            controller.TempData["ErrorMessage"]);
        var stillThere = await dbContext.Groups.FindAsync(1);
        Assert.NotNull(stillThere);
    }

    [Fact]
    public async Task DeleteGroup_DbUpdateException_SetsTempDataErrorMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var testContext = new TestApplicationDbContext(options);

        var group = new Group { Id = 1, Name = "Test Group", Description = "Test"};
        testContext.Groups.Add(group);
        await testContext.SaveChangesAsync();

        testContext.ThrowOnSaveChanges = true;

        var controller = new AdminController(testContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext,
            new Mock<ITempDataProvider>().Object);

        // Act
        var result = await controller.DeleteGroup(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Groups", redirectResult.ActionName);
        Assert.Equal("Unable to delete this group because it is referenced elsewhere. Check project assignments",
            controller.TempData["ErrorMessage"]);

        var existingGroup = await testContext.Groups.FindAsync(1);
        Assert.NotNull(existingGroup);
    }

    [Fact]
    public async Task AddUserToGroup_ReturnsNotFound_IfGroupOrUserNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act – group not found
        var result1 = await controller.AddUserToGroup(1, 1);
        Assert.IsType<NotFoundResult>(result1);

        dbContext.Groups.Add(new Group { Id = 50, Name = "AddUserGroup", Description = "Test Description" });
        await dbContext.SaveChangesAsync();

        var result2 = await controller.AddUserToGroup(50, 99);
        Assert.IsType<NotFoundResult>(result2);
    }

    [Fact]
    public async Task AddUserToGroup_AddsUserIfNotAlreadyAdded()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group { Id = 60, Name = "UserGroup", Description = "Test Description" };
        var user = new User { Id = 10, UserName = "GroupUser", Email = "groupuser@example.com" };
        dbContext.Groups.Add(group);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AddUserToGroup(60, 10);

        // Assert
        var redirectResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_GroupUserAssignmentPartial", redirectResult.ViewName);

        // ✅ Check in `UserGroups`
        Assert.Contains(dbContext.UserGroups, ug => ug.GroupId == 60 && ug.UserId == 10 && ug.Role == "Member");
    }

    [Fact]
    public async Task AddManagerToGroup_ReturnsNotFound_IfGroupOrUserNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act – group not found
        var result1 = await controller.AddManagerToGroup(1, 1, false);
        Assert.IsType<NotFoundResult>(result1);

        // Add group but not user
        dbContext.Groups.Add(new Group
        {
            Id = 70,
            Name = "ManagerGroup",
            Description = "Test description",
            Managers = new List<GroupManager>()
        });
        await dbContext.SaveChangesAsync();
        var result2 = await controller.AddManagerToGroup(70, 99, false);
        Assert.IsType<NotFoundResult>(result2);
    }

    [Fact]
    public async Task AddManagerToGroup_AddsManagerAndSetsPrimaryIfTrue()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group
        {
            Id = 80,
            Name = "ManagerGroup",
            Description = "Test Description",
            Managers = new List<GroupManager>()
        };
        var user = new User
        {
            Id = 20,
            UserName = "ManagerUser",
            Email = "manager@example.com"
        };
        var groupManager = new GroupManager
        {
            GroupId = 80,
            UserId = 20
        };
        dbContext.Groups.Add(group);
        dbContext.Users.Add(user);
        dbContext.GroupManagers.Add(groupManager);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AddManagerToGroup(80, 20, true);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GroupDetails", redirectResult.ActionName);
        Assert.Contains(dbContext.GroupManagers, gm => gm.GroupId == 80 && gm.UserId == 20);
        Assert.Equal(20, group.ManagerId);
    }


    // ––– Tests for Project-related endpoints –––

    [Fact]
    public async Task Projects_ReturnsViewWithProjectsList()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Projects.Add(new Project { Id = 1, Name = "Project1", Description = "Test Description" });
        dbContext.Projects.Add(new Project { Id = 2, Name = "Project2", Description = "Test Description" });
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.Projects();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.Model);
        Assert.Equal(2, projects.Count());
    }

    [Fact]
    public async Task CreateProject_Get_ReturnsViewWithProjectLeads()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Users.Add(new User { Id = 1, UserName = "Lead1", Email = "lead1@example.com" });
        dbContext.Users.Add(new User { Id = 2, UserName = "Lead2", Email = "lead2@example.com" });
        await dbContext.SaveChangesAsync();
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = 3, UserName = "Admin", Email = "admin@example.com" });

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        CreateProjectViewModel model = new CreateProjectViewModel
        {
            ProjectLeads = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lead1" },
                new SelectListItem { Value = "2", Text = "Lead2" }
            }
        };

        // Act
        var result = await controller.CreateProject();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(model.ProjectLeads);
        Assert.Equal(2, model.ProjectLeads.Count);
    }

    [Fact]
    public async Task CreateProject_Post_InvalidModel_ReturnsView()
    {
        CreateProjectViewModel model = new CreateProjectViewModel();
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = 3, UserName = "Admin", Email = "admin@example.com" });
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ModelState.AddModelError("Error", "Invalid");
        var project = new Project { Id = 1, Name = "InvalidProject" };
        var users = await dbContext.Users.ToListAsync();

        model.ProjectLeads = users.Select(u => new SelectListItem
        {
            Value = u.Id.ToString(),
            Text = u.UserName
        }).ToList();

        CreateProjectViewModel viewModel = new CreateProjectViewModel
        {
            ProjectLeads = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lead1" },
                new SelectListItem { Value = "2", Text = "Lead2" }
            }
        };
        // Act
        var result = await controller.CreateProject(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task CreateProject_Post_WithoutUserId_ReturnsViewWithError()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = 3, UserName = "Admin", Email = "admin@example.com" });

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);
        var project = new Project { Id = 1, Name = "ProjectNoUser" };
        var users = await dbContext.Users.ToListAsync();

        CreateProjectViewModel model = new CreateProjectViewModel
        {
            ProjectLeads = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList()
        };

        // Act
        var result = await controller.CreateProject(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("", controller.ModelState.Keys);
    }

    [Fact]
    public async Task CreateProject_Post_Valid_ReturnsRedirect()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        this._mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = 3, UserName = "Admin", Email = "admin@example.com" });

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .BuildServiceProvider();

        controller.ControllerContext.HttpContext.RequestServices = services;
        controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext,
            services.GetRequiredService<ITempDataProvider>());

        var mockUrlHelper = new Mock<IUrlHelper>();
        controller.Url = mockUrlHelper.Object;

        var users = await dbContext.Users.ToListAsync();

        CreateProjectViewModel model = new CreateProjectViewModel
        {
            Name = "ValidProject",
            Description = "Test Description",
            ProjectLeads = users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList()
        };

        // Act
        var result = await controller.CreateProject(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Projects", redirectResult.ActionName);
        Assert.True(dbContext.Projects.Any(p => p.Name == "ValidProject"));
    }

    [Fact]
    public async Task CreateProject_Post_WithValidGroupData_CallsAssignGroupToProject()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();

        var validUser = new User { Id = 1, UserName = "Lead1", Email = "lead1@example.com" };
        dbContext.Users.Add(validUser);
        await dbContext.SaveChangesAsync();

        dbContext.Groups.Add(new Group { Id = 1, ManagerId = 1, Description = "Test", Name = "TestGroup" });
        await dbContext.SaveChangesAsync();

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                        .ReturnsAsync(new User { Id = 3, UserName = "Admin", Email = "admin@example.com" });
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("3");

        var mockController = new Mock<AdminController>(dbContext, _mockUserManager.Object, _mockRoleManager.Object)
        {
            CallBase = true
        };

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "3") };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };

        httpContext.Request.ContentType = "application/x-www-form-urlencoded";

        var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "GroupId", new Microsoft.Extensions.Primitives.StringValues("1") }
        };
        httpContext.Request.Form = new FormCollection(formData);

        mockController.Object.ControllerContext.HttpContext = httpContext;
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .BuildServiceProvider();
        mockController.Object.ControllerContext.HttpContext.RequestServices = services;
        mockController.Object.TempData = new TempDataDictionary(httpContext, services.GetRequiredService<ITempDataProvider>());
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockController.Object.Url = mockUrlHelper.Object;

        var model = new CreateProjectViewModel
        {
            Name = "ProjectWithGroups",
            Description = "Test Description",
            ProjectLeadId = 1
        };

        // Act
        var result = await mockController.Object.CreateProject(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Projects", redirectResult.ActionName);

        mockController.Verify(c => c.AssignGroupToProject(It.IsAny<int>(), 1), Times.Once);
    }

    [Fact]
    public async Task ProjectDetails_ReturnsNotFoundIfProjectDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.ProjectDetails(9999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignGroupToProject_ReturnsNotFound_IfProjectOrGroupNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act: project not found
        var result1 = await controller.AssignGroupToProject(1, 1);
        Assert.IsType<NotFoundResult>(result1);

        // Add a project but not group
        dbContext.Projects.Add(new Project { Id = 1, Name = "AssignProject", Description = "Test Description" });
        await dbContext.SaveChangesAsync();
        var result2 = await controller.AssignGroupToProject(1, 99);
        Assert.IsType<NotFoundResult>(result2);
    }

    [Fact]
    public async Task AssignGroupToProject_AddsGroupWhenNotAssigned()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 1, Name = "AssignProject", ProjectGroups = new List<GroupProject>(), Description = "Test Description" };
        var group = new Group { Id = 10, Name = "TestGroup", Description = "Test Description" };
        dbContext.Projects.Add(project);
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AssignGroupToProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_ProjectGroupAssignmentPartial", redirectResult.ViewName);
        Assert.Contains(project.ProjectGroups, pg => pg.GroupId == 10);
    }

    [Fact]
    public async Task AssignGroupToProject_DoesNotAddDuplicateGroup()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var groupProject = new GroupProject { ProjectId = 1, GroupId = 10 };
        var project = new Project { Id = 1, Name = "AssignProject", ProjectGroups = new List<GroupProject> { groupProject }, Description = "Test Description" };
        var group = new Group { Id = 10, Name = "TestGroup", Description = "Test Description" };
        dbContext.Projects.Add(project);
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act – calling assign again should not duplicate the group assignment
        var result = await controller.AssignGroupToProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal(1, project.ProjectGroups.Count(pg => pg.GroupId == 10));
    }

    [Fact]
    public async Task RemoveGroupFromProject_ReturnsNotFound_IfProjectNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.RemoveGroupFromProject(1, 10);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveGroupFromProject_RemovesGroupIfExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var groupProject = new GroupProject { ProjectId = 1, GroupId = 10 };
        var project = new Project { Id = 1, Name = "RemoveProject", ProjectGroups = new List<GroupProject> { groupProject }, Description = "Test Description" };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.RemoveGroupFromProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_ProjectGroupAssignmentPartial", redirectResult.ViewName);
        Assert.DoesNotContain(project.ProjectGroups, pg => pg.GroupId == 10);
    }

    [Fact]
    public async Task EditProject_Get_ReturnsViewIfProjectExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 50, Name = "EditProject", Description = "Test Description" };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.EditProject(50);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelProject = Assert.IsType<Project>(viewResult.Model);
        Assert.Equal("EditProject", modelProject.Name);
        Assert.NotNull(viewResult.ViewData["ProjectLeads"]);
    }

    [Fact]
    public async Task EditProject_Get_ReturnsNotFoundIfProjectDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.EditProject(9999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditProject_Post_BadRequestIfIdMismatch()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 10, Name = "Mismatch" };
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.EditProject(5, project);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task EditProject_Post_MissingUserId_ReturnsViewWithError()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 20, Name = "MissingUser", Description = "Test Description" };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);

        // Act
        var result = await controller.EditProject(20, project);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task EditProject_Post_Valid_ReturnsRedirect()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 30, Name = "ValidEdit", Description = "Test Description" };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) };

        // Act
        var result = await controller.EditProject(30, project);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ProjectDetails", redirectResult.ActionName);
    }

    public class ConcurrencyDbContext : ApplicationDbContext
    {
        public ConcurrencyDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateConcurrencyException();
        }
    }

    [Fact]
    public async Task EditProject_Post_ConcurrencyException_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ConcurrencyTest")
            .Options;
        var dbContext = new ConcurrencyDbContext(options);
        var project = new Project { Id = 40, Name = "Concurrency" };
        // Do not add the project so that the check for existence fails.
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) };

        // Act
        var result = await controller.EditProject(40, project);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteProject_RemovesProjectIfExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 55, Name = "ToDelete", Description = "Test Description" };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.DeleteProject(55);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.False(dbContext.Projects.Any(p => p.Id == 55));
    }

    [Fact]
    public async Task DeleteProject_RedirectsIfProjectNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.DeleteProject(999);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Users_ReturnsEmptyViewIfNoUsers()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.Users();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
        Assert.Empty(model);
    }

    //[Fact]
    //public async Task CreateProject_ReturnsViewWithErrors_IfModelStateInvalid()
    //{
    //    // Arrange
    //    var dbContext = TestHelper.GetDbContext();
    //    var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

    //    controller.ModelState.AddModelError("Name", "Required");

    //    var users = await dbContext.Users.ToListAsync();

    //    CreateProjectViewModel model = new CreateProjectViewModel
    //    {
    //        ProjectLeads = users.Select(u => new SelectListItem
    //        {
    //            Value = u.Id.ToString(),
    //            Text = u.UserName
    //        }).ToList()
    //    };

    //    // Act
    //    var result = await controller.CreateProject(model);

    //    // Assert
    //    var viewResult = Assert.IsType<ViewResult>(result);
    //    Assert.False(controller.ModelState.IsValid);
    //}

    [Fact]
    public async Task UserAdd_CreatesUserAndRedirects_WhenSuccessful()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var model = new UserViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123", ConfirmPassword = "Test@123" };

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "Employee"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await controller.UserAdd(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirectResult.ActionName);
    }

    [Fact]
    public async Task UserAdd_ReturnsViewWithErrors_WhenUserCreationFails()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var model = new UserViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123", ConfirmPassword = "Test@123" };

        var identityErrors = new IdentityError[] { new IdentityError { Code = "Error1", Description = "User creation failed" } };
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await controller.UserAdd(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("User creation failed", controller.ModelState[string.Empty].Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task UserAdd_ReturnsViewWithErrors_WhenRoleAssignmentFails()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var model = new UserViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123", ConfirmPassword = "Test@123" };

        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);

        var identityErrors = new IdentityError[] { new IdentityError { Code = "Error2", Description = "Role assignment failed" } };
        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "Employee"))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await controller.UserAdd(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("Role assignment failed", controller.ModelState[string.Empty].Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task UserEdit_UpdatesUserAndRedirects_WhenSuccessful()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var user = new User { Id = 1, UserName = "OldUser", Email = "old@example.com" };

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Employee" });
        _mockUserManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await controller.UserEdit(user.Id.ToString(), "UpdatedUser", "updated@example.com", "Admin");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirectResult.ActionName);
        Assert.Equal("UpdatedUser", user.UserName);
        Assert.Equal("updated@example.com", user.Email);
    }

    [Fact]
    public async Task UserEdit_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        _mockUserManager.Setup(um => um.FindByIdAsync("999"))
            .ReturnsAsync((User)null);

        // Act
        var result = await controller.UserEdit("999", "UpdatedUser", "updated@example.com", "Admin");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UserEdit_RemovesExistingRolesBeforeAddingNewRole()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var user = new User { Id = 1, UserName = "TestUser", Email = "test@example.com" };

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _mockUserManager.Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Manager", "Employee" });
        _mockUserManager.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await controller.UserEdit(user.Id.ToString(), "UpdatedUser", "updated@example.com", "Admin");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirectResult.ActionName);

        _mockUserManager.Verify(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _mockUserManager.Verify(um => um.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    [Fact]
    public async Task ChangeManager_GroupNotFound_ReturnsNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Users.Add(new User { Id = 1, UserName = "Manager", Email = "manager@example.com" });
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.ChangeManager(1, 1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangeManager_NewManagerNotFound_ReturnsNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        dbContext.Groups.Add(new Group { Id = 1, Name = "Group 1", Description = "Test" });
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.ChangeManager(1, 99);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangeManager_NewManagerEntryCreated_NoPreviousManager_ReturnsRedirect()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group { Id = 1, Name = "Group 1", ManagerId = null, Description = "Test" };
        dbContext.Groups.Add(group);
        var newManager = new User { Id = 2, UserName = "NewManager", Email = "newmanager@example.com" };
        dbContext.Users.Add(newManager);
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.ChangeManager(1, 2);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GroupDetails", redirect.ActionName);

        var updatedGroup = await dbContext.Groups.FindAsync(1);
        Assert.Equal(2, updatedGroup.ManagerId);

        var userGroup = await dbContext.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == 1 && ug.UserId == 2);
        Assert.NotNull(userGroup);
        Assert.Equal("Manager", userGroup.Role);
    }

    [Fact]
    public async Task ChangeManager_NewManagerEntryCreated_WithPreviousManager_UpdatesPreviousManagerEntry()
    {
        // Arrange:
        var dbContext = TestHelper.GetDbContext();
        var previousManager = new User { Id = 1, UserName = "OldManager", Email = "oldmanager@example.com" };
        var newManager = new User { Id = 2, UserName = "NewManager", Email = "newmanager@example.com" };
        dbContext.Users.Add(previousManager);
        dbContext.Users.Add(newManager);
        var group = new Group { Id = 1, Name = "Group 1", ManagerId = 1, Description = "Test" };
        dbContext.Groups.Add(group);
        dbContext.UserGroups.Add(new UserGroup { GroupId = 1, UserId = 1, Role = "Manager" });
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await controller.ChangeManager(1, 2);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GroupDetails", redirect.ActionName);

        var updatedGroup = await dbContext.Groups.FindAsync(1);
        Assert.Equal(2, updatedGroup.ManagerId);

        var newManagerEntry = await dbContext.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == 1 && ug.UserId == 2);
        Assert.NotNull(newManagerEntry);
        Assert.Equal("Manager", newManagerEntry.Role);

        var previousManagerEntry = await dbContext.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == 1 && ug.UserId == 1);
        Assert.NotNull(previousManagerEntry);
        Assert.Equal("Member", previousManagerEntry.Role);
    }
}
