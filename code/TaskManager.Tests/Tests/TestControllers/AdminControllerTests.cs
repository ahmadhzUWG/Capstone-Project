using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskManager.Tests;
using TaskManagerWebsite.Controllers;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;
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

        // Setup some default roles for the RoleManager
        var mockRoles = new List<IdentityRole<int>>
        {
            new IdentityRole<int> { Id = 1, Name = "Admin" },
            new IdentityRole<int> { Id = 2, Name = "Manager" },
            new IdentityRole<int> { Id = 3, Name = "Employee" }
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
        var modelUser = Assert.IsType<User>(viewResult.Model);
        Assert.Equal("DeleteUser", modelUser.UserName);
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
        // Add users for testing
        var managerUser = new User { Id = 1, UserName = "ManagerUser", Email = "manager@example.com" };
        var employeeUser = new User { Id = 2, UserName = "EmployeeUser", Email = "employee@example.com" };
        dbContext.Users.Add(managerUser);
        dbContext.Users.Add(employeeUser);
        dbContext.SaveChanges();

        // Setup IsInRoleAsync so that user 1 is Manager and user 2 is Employee
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 1), "Manager"))
            .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 2), "Manager"))
            .ReturnsAsync(false);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 1), "Employee"))
            .ReturnsAsync(false);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 2), "Employee"))
            .ReturnsAsync(true);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Setup HttpContext with required TempData services.
        var httpContext = new DefaultHttpContext();
        // Create a minimal service provider with a dummy ITempDataProvider.
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton(_mockUserManager.Object)  // Register your mock UserManager
            .BuildServiceProvider();
        httpContext.RequestServices = services;

        controller.ControllerContext.HttpContext = httpContext;
        controller.TempData = new TempDataDictionary(httpContext, services.GetRequiredService<ITempDataProvider>());

        // Act
        var result = controller.CreateGroup();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.ViewData["Managers"]);
        Assert.NotNull(viewResult.ViewData["Employees"]);
        var managers = viewResult.ViewData["Managers"] as List<User>;
        var employees = viewResult.ViewData["Employees"] as List<User>;
        Assert.Single(managers);
        Assert.Single(employees);
        Assert.Equal("ManagerUser", managers.First().UserName);
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
            Users = new List<User>(),
            Managers = new List<GroupManager>(),
            Description = "Test Description"
        };
        dbContext.Groups.Add(group);

        var managerUser = new User { Id = 1, UserName = "ManagerUser", Email = "manager@example.com" };
        var employeeUser = new User { Id = 2, UserName = "EmployeeUser", Email = "employee@example.com" };
        dbContext.Users.AddRange(managerUser, employeeUser);
        await dbContext.SaveChangesAsync();

        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 1), "Manager"))
            .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.IsInRoleAsync(It.Is<User>(u => u.Id == 2), "Manager"))
            .ReturnsAsync(false);

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton<UserManager<User>>(_mockUserManager.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        controller.ControllerContext.HttpContext = httpContext;

        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        // Act
        var result = await controller.GroupDetails(40);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var modelGroup = Assert.IsType<Group>(viewResult.Model);
        Assert.Equal("GroupDetails", modelGroup.Name);
        Assert.NotNull(viewResult.ViewData["Users"]);
        Assert.NotNull(viewResult.ViewData["Managers"]);
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
        // Use an invalid primaryManagerId (0)
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Create a service collection with necessary registrations.
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton<UserManager<User>>(_mockUserManager.Object)  // Register your mocked UserManager
            .BuildServiceProvider();

        // Create and configure a DefaultHttpContext with the service provider.
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;
        controller.ControllerContext.HttpContext = httpContext;

        // Manually create and set the TempData to avoid DI lookup for ITempDataDictionaryFactory.
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
        // Add a primary manager and other users
        var primaryManager = new User { Id = 1, UserName = "PrimaryManager", Email = "pm@example.com" };
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

        // Create the controller with required mocked dependencies.
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Build a service provider with necessary services.
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .AddSingleton<UserManager<User>>(_mockUserManager.Object)
            .BuildServiceProvider();

        // Set up the HttpContext with the service provider.
        var httpContext = new DefaultHttpContext { RequestServices = services };
        controller.ControllerContext.HttpContext = httpContext;

        // Manually create and assign TempData to avoid DI lookup for ITempDataDictionaryFactory.
        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        // Assign a mocked IUrlHelper to bypass IUrlHelperFactory resolution when RedirectToAction is called.
        var mockUrlHelper = new Mock<IUrlHelper>();
        controller.Url = mockUrlHelper.Object;

        // Act
        var result = await controller.CreateGroup(groupViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Groups", redirectResult.ActionName);
        Assert.Equal(1, groupViewModel.SelectedManagerId);
        Assert.Contains(3, groupViewModel.SelectedUserIds);
    }

    [Fact]
    public async Task DeleteGroup_RemovesGroupIfExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group { Id = 20, Name = "GroupToDelete", Description = "Test Description"};
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
    public async Task AddUserToGroup_ReturnsNotFound_IfGroupOrUserNotFound()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act – group not found
        var result1 = await controller.AddUserToGroup(1, 1);
        Assert.IsType<NotFoundResult>(result1);

        // Add group but not user
        dbContext.Groups.Add(new Group { Id = 50, Name = "AddUserGroup", Users = new List<User>(), Description = "Test Description"});
        await dbContext.SaveChangesAsync();
        var result2 = await controller.AddUserToGroup(50, 99);
        Assert.IsType<NotFoundResult>(result2);
    }

    [Fact]
    public async Task AddUserToGroup_AddsUserIfNotAlreadyAdded()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var group = new Group { Id = 60, Name = "UserGroup", Users = new List<User>(), Description = "Test Description"};
        var user = new User { Id = 10, UserName = "GroupUser", Email = "groupuser@example.com" };
        dbContext.Groups.Add(group);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AddUserToGroup(60, 10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GroupDetails", redirectResult.ActionName);
        Assert.Contains(group.Users, u => u.Id == 10);
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
            Description = "Test Description", // Added required property
            Managers = new List<GroupManager>()
        };
        var user = new User
        {
            Id = 20,
            UserName = "ManagerUser",
            Email = "manager@example.com"
        };
        dbContext.Groups.Add(group);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AddManagerToGroup(80, 20, true);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GroupDetails", redirectResult.ActionName);
        Assert.Contains(dbContext.GroupManagers, gm => gm.GroupId == 80 && gm.UserId == 20);
        Assert.Equal(20, group.PrimaryManagerId);
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
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.CreateProject();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var projectLeads = viewResult.ViewData["ProjectLeads"] as List<User>;
        Assert.NotNull(projectLeads);
        Assert.Equal(2, projectLeads.Count);
    }

    [Fact]
    public async Task CreateProject_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        controller.ModelState.AddModelError("Error", "Invalid");
        var project = new Project { Id = 1, Name = "InvalidProject" };

        // Act
        var result = await controller.CreateProject(project);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(project, viewResult.Model);
    }

    [Fact]
    public async Task CreateProject_Post_WithoutUserId_ReturnsViewWithError()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(string.Empty);
        var project = new Project { Id = 1, Name = "ProjectNoUser" };

        // Act
        var result = await controller.CreateProject(project);

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
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Setup the UserManager mock to return a user id.
        _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");

        // Create a test user with the required claim.
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };

        // Build a service provider with necessary services (TempData, etc.)
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider>(new Mock<ITempDataProvider>().Object)
            .BuildServiceProvider();

        // Configure HttpContext and TempData.
        controller.ControllerContext.HttpContext.RequestServices = services;
        controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext,
            services.GetRequiredService<ITempDataProvider>());

        var mockUrlHelper = new Mock<IUrlHelper>();
        controller.Url = mockUrlHelper.Object;

        // Create a valid Project with the required Description.
        var project = new Project
        {
            Id = 1,
            Name = "ValidProject",
            Description = "Test project description"
        };

        // Act
        var result = await controller.CreateProject(project);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Projects", redirectResult.ActionName);
        Assert.True(dbContext.Projects.Any(p => p.Name == "ValidProject"));
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
        dbContext.Projects.Add(new Project { Id = 1, Name = "AssignProject", Description = "Test Description"});
        await dbContext.SaveChangesAsync();
        var result2 = await controller.AssignGroupToProject(1, 99);
        Assert.IsType<NotFoundResult>(result2);
    }

    [Fact]
    public async Task AssignGroupToProject_AddsGroupWhenNotAssigned()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 1, Name = "AssignProject", ProjectGroups = new List<GroupProject>(), Description = "Test Description"};
        var group = new Group { Id = 10, Name = "TestGroup", Description = "Test Description" };
        dbContext.Projects.Add(project);
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.AssignGroupToProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ProjectDetails", redirectResult.ActionName);
        Assert.Contains(project.ProjectGroups, pg => pg.GroupId == 10);
    }

    [Fact]
    public async Task AssignGroupToProject_DoesNotAddDuplicateGroup()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var groupProject = new GroupProject { ProjectId = 1, GroupId = 10 };
        var project = new Project { Id = 1, Name = "AssignProject", ProjectGroups = new List<GroupProject> { groupProject } , Description = "Test Description" };
        var group = new Group { Id = 10, Name = "TestGroup" , Description = "Test Description" };
        dbContext.Projects.Add(project);
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act – calling assign again should not duplicate the group assignment
        var result = await controller.AssignGroupToProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
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
        var project = new Project { Id = 1, Name = "RemoveProject", ProjectGroups = new List<GroupProject> { groupProject }, Description = "Test Description"};
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        // Act
        var result = await controller.RemoveGroupFromProject(1, 10);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ProjectDetails", redirectResult.ActionName);
        Assert.DoesNotContain(project.ProjectGroups, pg => pg.GroupId == 10);
    }

    [Fact]
    public async Task EditProject_Get_ReturnsViewIfProjectExists()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var project = new Project { Id = 50, Name = "EditProject", Description = "Test Description"};
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
        var project = new Project { Id = 30, Name = "ValidEdit", Description = "Test Description"};
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

    [Fact]
    public async Task CreateProject_ReturnsViewWithErrors_IfModelStateInvalid()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);

        controller.ModelState.AddModelError("Name", "Required");

        var project = new Project { Description = "No Name" };

        // Act
        var result = await controller.CreateProject(project);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task UserAdd_CreatesUserAndRedirects_WhenSuccessful()
    {
        // Arrange
        var dbContext = TestHelper.GetDbContext();
        var controller = new AdminController(dbContext, _mockUserManager.Object, _mockRoleManager.Object);
        var model = new AdminViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123" , ConfirmPassword = "Test@123" };

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
        var model = new AdminViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123", ConfirmPassword = "Test@123" };

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
        var model = new AdminViewModel { UserName = "NewUser", Email = "new@example.com", Password = "Test@123", ConfirmPassword = "Test@123" };

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

}
