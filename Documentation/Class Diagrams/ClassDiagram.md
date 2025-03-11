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
    %% Models Namespace
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
            +ICollection<UserGroup> UserGroups
            +ICollection<GroupManager> Managers
            +int? PrimaryManagerId
            +User? PrimaryManager
            +ICollection<GroupProject> GroupProjects
        }

        class UserGroup {
            +int UserId
            +User User
            +int GroupId
            +Group Group
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
            +ICollection<GroupProject> GroupProjects
            +ProjectBoard Board
            +ICollection<Stage> Stages
        }

        class ProjectBoard {
            +int Id
            +string Title
            +string? Description
        }

        class Stage {
            +int Id
            +string Name
            +int Order
            +string? Description
        }
    }

    %% Database Namespace
    namespace Database {
        class ApplicationDbContext {
            +DbSet<User> Users
            +DbSet<Admin> Admins
            +DbSet<Group> Groups
            +DbSet<UserGroup> UserGroups
            +DbSet<GroupManager> GroupManagers
            +DbSet<GroupProject> GroupProjects
            +DbSet<GroupRequest> GroupRequests
            +DbSet<Project> Projects
            +DbSet<ProjectBoard> ProjectBoards
            +DbSet<Stage> Stages
            +ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        }

        class DataSeeder {
            +Seed()
        }
    }

    %% Controllers Namespace
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

    %% Relationships between ApplicationDbContext and Models
    Database.ApplicationDbContext <|-- Models.User : Contains
    Database.ApplicationDbContext <|-- Models.Admin : Contains
    Database.ApplicationDbContext <|-- Models.Group : Contains
    Database.ApplicationDbContext <|-- Models.UserGroup : Contains
    Database.ApplicationDbContext <|-- Models.GroupManager : Contains
    Database.ApplicationDbContext <|-- Models.GroupProject : Contains
    Database.ApplicationDbContext <|-- Models.GroupRequest : Contains
    Database.ApplicationDbContext <|-- Models.Project : Contains
    Database.ApplicationDbContext <|-- Models.ProjectBoard : Contains
    Database.ApplicationDbContext <|-- Models.Stage : Contains
    Database.DataSeeder ..> Database.ApplicationDbContext : seeds

    %% Controller Relationships
    AdminController --> Models.User : Manages
    AdminController --> Models.Group : Manages
    AdminController --> Models.Project : Manages

    EmployeeController --> Models.User : Views
    EmployeeController --> Models.Group : Views
    EmployeeController --> Models.Project : Views

    ManagerController --> Models.User : Manages
    ManagerController --> Models.Group : Manages
    ManagerController --> Models.Project : Manages
    ManagerController --> Models.GroupManager : Assigns
    ManagerController --> Models.GroupProject : Assigns
    ManagerController --> Models.GroupRequest : Processes

    HomeController --> Models.User : Displays
    LoginController --> Models.User : Authenticates

    %% Domain Relationships
    Models.Group "1" o-- "0..*" Models.UserGroup : contains
    Models.User "1" o-- "0..*" Models.UserGroup : belongsTo
    Models.Group "1" o-- "0..*" Models.GroupManager : managedBy
    Models.User "1" o-- "0..*" Models.GroupManager : manages
    Models.Group "1" o-- "0..*" Models.GroupProject : includes
    Models.Project "1" o-- "0..*" Models.GroupProject : assignedTo
    Models.GroupRequest --> Models.Group : references
    Models.GroupRequest --> Models.Project : references
    Models.Project "1" --> "1" Models.ProjectBoard : has
    Models.Project "1" o-- "0..*" Models.Stage : contains
    Models.Project --> Models.User : projectLead
