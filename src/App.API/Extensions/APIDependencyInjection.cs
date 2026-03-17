namespace App.API.Extensions;

public static class APIDependencyInjection
{
    public static IServiceCollection AddAPI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwagger();
        services.AddCorsPolicies();
        services.AddJwt(configuration);
        services.AddAuthorization();

        return services;
    }

    public static WebApplication UseAPI(
        this WebApplication app)
    {

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseStaticFiles();

        app.UseCors("AllowReactApp");

        app.UseHttpsRedirection();

        // Authentication and Authorization MUST come before custom middlewares
        app.UseAuthentication();
        app.UseAuthorization();

        // Add Middlewares Here
        app.AddMiddlewares();
        
        app.MapControllers();

        return app;
    }

    private static void AddMiddlewares(
        this IApplicationBuilder builder)
    {
        builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    private static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1",
                Description = "Backend API"
            });

            // JWT Bearer auth
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<SwaggerExcludeFormDataOperationFilter>();
        });

        return services;
    }

    private static IServiceCollection AddCorsPolicies(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:5173", // Vite dev
                        "http://localhost:5174" // Vite dev alternative
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static void AddJwt(this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:SecretKey"] ??
            throw new InvalidOperationException("JWT Secret Key is not configured.");
        var issuer = configuration["JwtSettings:Issuer"] ??
            throw new InvalidOperationException("JWT Issuer is not configured.");
        var audience = configuration["JwtSettings:Audience"] ??
            throw new InvalidOperationException("JWT Audience is not configured.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services.AddAuthentication(options =>
        {
            // Set JWT Bearer as the default authentication scheme
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = signingKey,
                LifetimeValidator = (notBefore, expires, tokenToValidate, tokenValidationParameters) =>
                {
                    return expires != null && expires > DateTime.UtcNow;
                }
            };
        });
    }
}
