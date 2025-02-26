```mermaid

%%{
  init: {
    "theme": "default",
    "themeVariables": {
      "primaryColor": "#d6d6d6",
      "primaryTextColor": "#222222",
      "primaryBorderColor": "#666666",
      "tertiaryColor": "#f8f9fa",
      "edgeLabelBackground": "#ffffff",
      "secondaryColor": "#c0c0c0",
      "tertiaryTextColor": "#333333"
    }
  }
}%%

classDiagram
    %% Grouping Entities into Packages
    namespace Models {
        class User {
            +int Id
            +string UserName
            +string NormalizedUserName
            +string Email
            +string NormalizedEmail
            +bool EmailConfirmed
            +string PasswordHash
            +string SecurityStamp
            +string ConcurrencyStamp
            +string PhoneNumber
            +bool PhoneNumberConfirmed
            +bool TwoFactorEnabled
            +DateTime? LockoutEnd
            +bool LockoutEnabled
            +int AccessFailedCount
            +string? Role
        }

        class Admin {
            +int AdminId
            +string AdminName
            +List<string> Permissions
        }

        class Group {
            +int Id
            +string Name
            +string Description
            +ICollection<User> Users
            +ICollection<GroupManager> Managers
            +int? PrimaryManagerId
            +User? PrimaryManager
            +ICollection<GroupProject> GroupProjects
        }

        class GroupManager {
            +int GroupId
            +Group Group
            +int UserId
            +User User
        }

        class GroupProject {
            +int ProjectId
            +Project Project
            +int GroupId
            +Group Group
        }

        class GroupRequest {
            +int Id
            +int SenderId
            +int GroupId
            +Group Group
            +int ProjectId
            +Project Project
            +bool? Response
        }

        class Project {
            +int Id
            +string Name
            +string Description
            +int ProjectLeadId
            +User ProjectLead
            +int? ProjectCreatorId
            +ICollection<GroupProject> ProjectGroups
        }
    }

    namespace Database {
        class ApplicationDbContext {
            +DbSet<User> Users
            +DbSet<Group> Groups
            +DbSet<GroupManager> GroupManagers
            +DbSet<GroupProject> GroupProjects
            +DbSet<GroupRequest> GroupRequests
            +DbSet<Project> Projects
            +ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        }

        class DataSeeder {
            +Seed()
        }
    }

    namespace Controllers {
        class EmployeeController {
            +Users()
            +Groups()
            +Projects()
            +ProjectDetails(int id)
        }

        class AdminController {
            +ManageUsers()
            +ManageRoles()
            +ManageGroups()
            +ManageProjects()
            +Users()
            +UserDetails(int id)
            +CreateGroup()
            +GroupDetails(int id)
            +CreateProject()
            +EditProject(int id)
            +DeleteProject(int id)
        }

        class ManagerController {
            +AssignTasks()
            +ApproveProjects()
            +ManageGroups()
            +Users()
            +Groups()
            +Projects()
            +ProjectDetails(int id)
            +CreateProject()
            +EditProject(int id)
            +AssignGroupToProject(int projectId, int groupId)
            +RemoveGroupFromProject(int projectId, int groupId)
            +AcceptRequest(int requestId)
            +DenyRequest(int requestId)
            +DeleteGroupRequest(int requestId)
        }

        class HomeController {
            +Index()
            +Privacy()
            +AccessDenied()
        }

        class LoginController {
            +Login(string email, string password)
            +Logout()
        }
    }

    %% Relationships
    ApplicationDbContext <|-- User : Contains
    ApplicationDbContext <|-- Group : Contains
    ApplicationDbContext <|-- GroupManager : Manages
    ApplicationDbContext <|-- GroupProject : Manages
    ApplicationDbContext <|-- GroupRequest : Stores
    ApplicationDbContext <|-- Project : Contains

    AdminController --> User : Manages
    AdminController --> Group : Manages
    AdminController --> Project : Manages

    EmployeeController --> User : Views
    EmployeeController --> Group : Views
    EmployeeController --> Project : Views

    ManagerController --> User : Manages
    ManagerController --> Group : Manages
    ManagerController --> Project : Manages
    ManagerController --> GroupManager : Assigns
    ManagerController --> GroupProject : Assigns
    ManagerController --> GroupRequest : Processes

    HomeController --> User : Displays
    LoginController --> User : Authenticates

    Group --> User : Contains
    Group --> GroupManager : ManagedBy
    Group --> GroupProject : Includes
    Group --> User : PrimaryManager
    GroupRequest --> Group : References
    GroupRequest --> Project : References
    Project --> GroupProject : Includes
    Project --> User : ProjectLead

