================================================================================
  API AND LAYER ARCHITECTURE
  ASP.NET Core Web API — n-layer layout (four projects, wires point one way)
================================================================================

WHAT THIS IS
------------
  Backend API template: presentation, business, data access, plus shared core.
  Goal: ship features without inventing new architecture every week. Complexity
  very bad; boring layers good.

REPO LAYOUT (src/)
------------------
  App.API       Web host — controllers, Swagger, JWT, CORS, middleware
  App.Business  Services, DTOs, rules (talks to DAL via interfaces)
  App.DAL       EF Core + MySQL, Identity, repos, unit of work, migrations, seed
  App.Core      Entities, enums, contracts (thin types, no big brain patterns)

STARTUP (Program.cs)
--------------------
  1. Serilog (console + config)
  2. AddAPI, AddDAL, AddBusiness
  3. Build, run migrations/seed (see note below)
  4. UseAPI — pipeline: Swagger, static files, CORS, HTTPS, auth, exception handler
  5. When the server is up, Serilog prints each listen URL and a Swagger link

  Console will show lines like:
    Now listening at http://localhost:5124
    Now listening at https://localhost:7190   (if HTTPS profile)
    Swagger UI: http://localhost:5124/swagger

  Exact host/port = your launch profile (see Properties/launchSettings.json), not
  magic 5000. Change applicationUrl there if you want different ports.

DEFAULT LOCAL URLS (Development, from launchSettings)
-------------------------------------------------------
  HTTP  — http://localhost:5124
  HTTPS — https://localhost:7190  (when using https profile; both URLs may show)

SWAGGER
-------
  Path: /swagger
  Title in UI comes from OpenApiInfo in App.API/Extensions/APIDependencyInjection.cs
  ("API and layer architecture").

CORS
----
  Policy AllowReactApp — localhost Vite ports in code. Edit there for new frontends.

JWT
---
  appsettings: JwtSettings:SecretKey, Issuer, Audience. Missing = startup throws
  (so you fix config, not debug silent 401s for three hours).

DATABASE
--------
  ConnectionStrings:DefaultConnection — MySQL 8 (UseMySql in DAL).

  AutomatedMigration.MigrateAsync only runs EF MigrateAsync when provider is SQL
  Server. On MySQL, apply schema with: dotnet ef database update (or your deploy).

CONFIG CHECKLIST
----------------
  ConnectionStrings:DefaultConnection
  JwtSettings:SecretKey
  JwtSettings:Issuer
  JwtSettings:Audience

RUN LOCALLY
-----------
  dotnet restore
  dotnet build
  dotnet run --project ./src/App.API/App.API.csproj

  Watch the console for "Now listening at ..." and "Swagger UI: ...".

CONTROLLERS (App.API/Controllers)
---------------------------------
  Auth, User, Settings, Contact, Faq, Service, TeamMember, Blog, BlogCategory,
  Project, ProjectCategory

BUSINESS SERVICES (BusinessDependencyInjection)
-----------------------------------------------
  External: Auth, File, Claim (+ IHttpContextAccessor)
  Internal:  Faq, User, Setting, Contact, Service, TeamMember, Blog, BlogCategory,
             Project, ProjectCategory

DAL
---
  Generic repository + IUnitOfWork, AppDbContext, Identity Core (API-style, no cookies)

MORE NOTES
----------
  Global errors: App.API/Middlewares/GlobalExceptionHandlerMiddleware.cs
  CORS is not AllowAnyOrigin — add origins in code when you add a new site.

================================================================================
