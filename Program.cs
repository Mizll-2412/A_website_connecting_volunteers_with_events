using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using khoaluantotnghiep.Services;
using System.Text;
using System.Globalization;

// Đã chuyển sang SQL Server, không cần thiết lập cho PostgreSQL nữa
// AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Cấu hình múi giờ Vietnam (UTC+7) cho toàn bộ ứng dụng
TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

var builder = WebApplication.CreateBuilder(args);

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITinhNguyenVienService, TinhNguyenVienService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IEventService, EventSerVice>();
builder.Services.AddScoped<IRegistrationFormService, RegistrationFormService>();
builder.Services.AddScoped<IDanhGiaService, DanhGiaService>();
builder.Services.AddScoped<ILegalDocumentService, LegalDocumentService>();
builder.Services.AddScoped<ILinhVucService, LinhVucService>();
builder.Services.AddScoped<IKyNangService, KyNangService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<ICertificateGeneratorService, CertificateGeneratorService>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer chưa được cấu hình");
    var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience chưa được cấu hình");
    var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key chưa được cấu hình");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorizationBuilder();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Add CORS headers to static files
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
