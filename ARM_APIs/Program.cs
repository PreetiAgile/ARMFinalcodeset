using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using ARMCommon.Services;
using ARM_APIs.Interface;
using ARM_APIs.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ARMCommon.Filter;
using ARMCommon.ActionFilter;
using ARM_APIs.Extension;
using ARMCommon.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using ARM_APIs.Services;
using ARM_APIs.Service;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// Add the appsettings.json file
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Load the appropriate settings file based on the environment
var env = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{env}.json", optional: true);

var _configuration = builder.Configuration;
var emailConfig = _configuration
        .GetSection("EmailConfiguration")
        .Get<EmailConfiguration>();
List<string> signalRIPList = _configuration.GetSection("SingalRIPList").Get<List<string>>();

var services = builder.Services;

bool enableLoggingFilter = _configuration.GetValue<bool>("LoggingConfig:IsLoggingEnabled");
if (enableLoggingFilter)
{
    builder.Services.AddControllers(opt =>
    {
        opt.Filters.Add<LoggingActionFilter>();

    });
}


services.AddControllers(options =>
{
    options.Filters.Add(new AppSettingsValidationFilter("Jwt:Audience", "Jwt:Issuer", "ARMAXPERTAI"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        //.WithOrigins(signalRIPList.ToArray())
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
        //.AllowCredentials());
});


builder.Services.AddSingleton<IPostgresHelper, PostgresHelper>();
builder.Services.AddSingleton<IOracleHelper, OracleHelper>();
builder.Services.AddTransient<IGetData, ARMGetData>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
services.AddTransient<IAPI, API>();
builder.Services.AddSingleton<IConfiguration>(_configuration);
builder.Services.AddDbContext<DataContext>();
services.AddTransient<IARMLogin, ARMLogin>();
services.AddTransient<IARMPEG, ARMPEG>();
services.AddTransient<IARMTstruct, ARMTstruct>();
services.AddTransient<IARMCheckService, ARMCheckService>();
services.AddTransient<IFiles, FileService>();
services.AddTransient<IARMenuV2, ARMMenuV2>();
services.AddTransient<IARMMenu, ARMMenu>();
services.AddTransient<IARMInlineForm, ARMInlineForm>();
services.AddTransient<IARMNotificationService, ARMNotificationService>();
services.AddTransient<IARMSigninDetails, ARMSigninDetails>();
services.AddTransient<IARMGeoFencing, ARMGeoFencing>();
services.AddTransient<IARMGetUserDetail, ARMGetUserDetail>();
services.AddTransient<INotificationHelper, NotificationHelper>();
builder.Services.AddTransient<IARMAppStatusV2, ARMAppStatusV2>();
services.AddTransient<IEntityService, EntityService>();
services.AddSingleton(emailConfig);
services.AddTransient<IFirebase, FirebaseTokenService>();
services.AddTransient<IARMUserGroups, ARMUserGroups>();
builder.Services.AddScoped<Utils>();
services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddTransient<ITokenService, TokenService>();
services.AddHttpContextAccessor();
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ValidateSessionFilter>();
builder.Services.AddTransient<ApiResponseFilter>(); 
builder.Services.AddTransient<LoggingActionFilter>();
builder.Services.AddSingleton<IRedisHelper, RedisHelper>();
services.AddSingleton<ARMLogger>();
services.AddHttpClient();
services.AddAuthorization();



services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(2, 1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _configuration["Jwt:Issuer"],
        ValidAudience = _configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.ConfigureCors();




builder.Services.AddDistributedMemoryCache();
// Configure SignalR
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
//app.UseHttpsRedirection();
// Shows UseCors with CorsPolicyBuilder.
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

app.Use(async (context, next) =>
{
    var token = context.Session.GetString("TokenId");
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }
    context.Request.EnableBuffering();
    await next();

});
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notificationHub");
 
});

app.Run();
