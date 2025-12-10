using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Add this using directive
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.Data;
using System.Text; // Add this using directive
using WarrantySystem.API.Middlewares;
using WarrantySystem.Model.Context;
using WarrantySystem.Model.DTO;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Repository.Repositories;
using WarrantySystem.Repository.Services;
using WarrantySystem.Shared.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add other necessary services and configurations here
builder.Services.AddDbContext<warranty_systemContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddTransient<IDbConnection>(sp =>
    new MySqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMvc().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddScoped<CurrentUser>(provider =>
{
    var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var claims = context?.User.Claims.ToDictionary(x => x.Type, x => x.Value);
    CurrentUser currentUser = ObjectMapper.GetCurrentUser(claims);
    return currentUser;
});
builder.Services.AddHttpContextAccessor();

#region DI

builder.Services.AddScoped<IGenericRepo, GenericRepo>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();

#endregion DI

// Load JWT settings
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            NameClaimType = "sub" // Để Middleware lấy đúng UserID
        };
    });
builder.Services.AddAuthentication();

//Config session
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(jwtSettings.ExpireMinutes);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name = "r-warranty";
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; // Chuyển tất cả URL thành chữ thường
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 🔐 Add Bearer Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào đây. Ví dụ: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("MyCors");

app.UseAuthorization();
app.UseSession();
app.UseMiddleware<DynamicAuthorizationMiddleware>();
app.MapControllers();

app.Run();