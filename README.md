# API N-Layer Arch Backend (ASP.NET Core Web API)

> **Repository:** `ilkoninn/api-nlayer-arch`  
> **Tech:** C# / .NET (ASP.NET Core Web API)

## 1) Overview
API N-Layer Arch üçün backend API servisi. Layihə `App.API` (presentation), `App.Business` (biznes məntiqi) və `App.DAL` (data access) qatlarına bölünüb.

## 2) Solution & Layering
Repository daxilində əsas struktur:

- `src/App.API` — Web API host, middleware pipeline, auth, swagger, CORS, controller-lər
- `src/App.Business` — service-lər (biznes use-case-lər)
- `src/App.DAL` — EF Core DbContext, Identity, repository/unit of work, seed/migration

`Program.cs` minimal host qurur və dependency-ləri bu extension-lar vasitəsilə qoşur:  
- `builder.Services.AddAPI(...)`
- `builder.Services.AddDAL(...)`
- `builder.Services.AddBusiness(...)`

Start zamanı DB üçün avtomatik migrate/seed də işə düşür.
(Referans: `src/App.API/Program.cs`)

## 3) Core Runtime Logic (daxili məntiqlər)

### 3.1 Startup axını (Program.cs)
1. `WebApplication.CreateBuilder(args)`
2. Service registration:
   - API layer (`AddAPI`)
   - DAL layer (`AddDAL`)
   - Business layer (`AddBusiness`)
3. `app.Build()`
4. **Automated migration + seed**:
   - scope açılır
   - `AutomatedMigration.MigrateAsync(scope.ServiceProvider)` çağırılır
5. HTTP pipeline: `app.UseAPI()`
6. `app.Run()`

(Referans: `src/App.API/Program.cs`)

### 3.2 HTTP pipeline (UseAPI)
Pipeline sırası (vacib məntiq):
- Swagger + SwaggerUI
- Static files
- CORS: `AllowReactApp`
- HTTPS redirection
- **Authentication**
- **Authorization**
- Custom middleware-lər (burada Global exception handler)
- Controller mapping (`app.MapControllers()`)

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`)

### 3.3 Global error handling
Pipeline-a custom middleware kimi əlavə olunub:
- `GlobalExceptionHandlerMiddleware`

Bu middleware runtime xətalarını tutub standart response qaytarmaq üçün nəzərdə tutulub.

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`)

### 3.4 Swagger (OpenAPI)
Swagger konfiqurasiyası `AddSwaggerGen` ilə edilir:
- Title: **Memar Group API**
- Version: `v1`
- Description: **Memar Group Backend API**
- **JWT Bearer security definition** əlavə olunub (`Bearer` scheme)

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`)

### 3.5 CORS policy (AllowReactApp)
Frontend origin-lər whitelist edilib:
- `http://localhost:5173`
- `http://localhost:5174`
- `https://memar-group.netlify.app`
- `https://memar-group-admin.netlify.app`

Policy:
- AllowAnyHeader
- AllowAnyMethod
- AllowCredentials

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`)

### 3.6 Authentication: JWT Bearer
JWT konfiqurasiyası `JwtSettings:*` açarlarından oxunur:
- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`

Token validation parametrləri:
- ValidateIssuer = true
- ValidateAudience = true
- ValidateLifetime = true
- ValidateIssuerSigningKey = true
- `LifetimeValidator`: `expires > DateTime.UtcNow` olmalıdır

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`)

### 3.7 DAL: Database + Identity + Repository pattern

#### Database (EF Core)
DbContext `AppDbContext` kimi registrasiya olunur və **MySQL** provider istifadə edilir:
- Connection string: `ConnectionStrings:DefaultConnection`
- `UseMySql(..., MySqlServerVersion(8.0.21))`
- `EnableStringComparisonTranslations()`

(Referans: `src/App.DAL/Extensions/DALDependencyInjection.cs`)

> Qeyd: Kodda `UseSqlServer(...)` şərhə alınmış kimi görünür; hazırda aktiv konfiqurasiya MySQL-dir.

#### Identity (API üçün IdentityCore)
Cookie auth yerinə API-yə uyğun `AddIdentityCore<User>` istifadə olunur:
- Roles: `IdentityRole<Guid>`
- Stores: `AddEntityFrameworkStores<AppDbContext>()`
- Token providers: `AddDefaultTokenProviders()`

Password/lockout/user qaydaları konfiqurasiya edilib (minimum 8 simvol, digit, upper, lower, non-alphanumeric və s.).

(Referans: `src/App.DAL/Extensions/DALDependencyInjection.cs`)

#### Repository + UnitOfWork
Generic repository və UnitOfWork DI ilə qoşulub:
- `IGenericRepository<,>` -> `GenericRepository<,>`
- `IUnitOfWork` -> `UnitOfWork`

(Referans: `src/App.DAL/Extensions/DALDependencyInjection.cs`)

### 3.8 Automated migration & seeding
Startup-da çağırılan `AutomatedMigration.MigrateAsync` aşağıdakıları edir:
- `AppDbContext` götürür
- Əgər DB provider SQL Server olarsa `MigrateAsync()` işlədir (kodda bu şərt var)
- `UserManager<User>` və `RoleManager<IdentityRole<Guid>>` götürür
- `AppDbContextSeed.SeedDatabaseAsync(...)` çağırır (role/user seed məntiqi burada yerləşir)

(Referans: `src/App.DAL/Persistence/AutomatedMigration.cs`)

## 4) Business Layer (Services)
Business layer DI-də aşağıdakı service-lər registrasiya olunur:

**External Services**
- `IAuthService` -> `AuthService`
- `IFileService` -> `FileService`
- `IClaimService` -> `ClaimService`
- `IHttpContextAccessor`

**Internal Services**
- `IFaqService` -> `FaqService`
- `IUserService` -> `UserService`
- `ISettingService` -> `SettingService`
- `IContactService` -> `ContactService`
- `IServiceService` -> `ServiceService`
- `ITeamMemberService` -> `TeamMemberService`
- `IBlogService` -> `BlogService`
- `IBlogCategoryService` -> `BlogCategoryService`
- `IProjectService` -> `ProjectService`
- `IProjectCategoryService` -> `ProjectCategoryService`

(Referans: `src/App.Business/Extensions/BusinessDependencyInjection.cs`)

Bu service-lər controller-lərdən çağırılaraq biznes qaydalarını icra etməlidir.

## 5) Configuration
Minimum tələb olunan config açarları:

- `ConnectionStrings:DefaultConnection` — MySQL connection string
- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`

Bu dəyərlər yoxdursa start zamanı `InvalidOperationException` atıla bilər (JWT bölməsində).

(Referans: `src/App.API/Extensions/APIDependencyInjection.cs`, `src/App.DAL/Extensions/DALDependencyInjection.cs`)

## 6) Run locally

### 6.1 Prerequisites
- .NET SDK (layihənin target framework-ünə uyğun)
- MySQL Server (və ya uyğun connection string)

### 6.2 Build & Run
```bash
dotnet restore
dotnet build
dotnet run --project ./src/App.API/App.API.csproj
```

> Start zamanı seed/migration avtomatik işləyə bilər (AutomatedMigration).

## 7) Swagger
Adətən:
- `GET /swagger`
- `GET /swagger/v1/swagger.json`

## 8) Notes / Known behaviors
- CORS yalnız müəyyən origin-lərə icazə verir (`AllowReactApp`).
- JWT token müddəti bitibsə request-lər 401 qaytaracaq (lifetime validation aktivdir).
- Global exception middleware runtime xətalarını idarə edir.
