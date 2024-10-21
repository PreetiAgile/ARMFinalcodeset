using ARMCommon.Helpers;
using ARMCommon.Helpers.RabbitMq;
using ARMCommon.Interface;
using ARMCommon.Model;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Hangfire.PostgreSql;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace ARMPeriodicNotificationService
{
    class Program
    {
        private static Timer timer;
        private static IHostApplicationLifetime applicationLifetime;

        static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = File.ReadAllText(appSettingsPath);
            dynamic config = JsonConvert.DeserializeObject(json);
            string queueName = config.AppConfig["QueueName"];

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
            serviceCollection.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
            serviceCollection.AddSingleton<CleanupService>();
            serviceCollection.AddLogging(); 
            serviceCollection.AddSingleton<CleanupService>();

            serviceCollection.AddHangfireServer();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var rabbitMQConsumer = serviceProvider.GetService<IRabbitMQConsumer>();
            var rabbitMQProducer = serviceProvider.GetService<IRabbitMQProducer>();
            var cleanupService = serviceProvider.GetService<CleanupService>();

            applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            applicationLifetime.ApplicationStopping.Register(() => cleanupService.OnProcessExit());



            //servicelog
            var context = new DataContext(configuration);
            var appConfigSection = configuration.GetSection("AppConfig");
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var otherInfoValue = JsonConvert.SerializeObject(appConfigSection.Get<Dictionary<string, object>>());
            Assembly assembly = Assembly.GetExecutingAssembly();
            string serviceName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            string exeName = assembly.Location;
            string exePath = Path.GetDirectoryName(exeName);
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;


            //var log = new ServiceLog(context);
            //var result = log.CreateServiceLog(new ARMServiceLogs
            //{
            //    ServiceName = serviceName,
            //    Server = ipv4Address?.ToString(),
            //    Folder = exePath,
            //    InstanceID = processId,
            //    OtherInfo = otherInfoValue,
            //    Status = "Started",
            //    LastOnline = DateTime.Now,
            //    StartOnTime = DateTime.Now,
            //});
            //WriteMessage("Service Started " + DateTime.Now);
            //TimeSpan interval = TimeSpan.FromMinutes(2);
            //timer = new Timer(Execute, result, TimeSpan.Zero, interval);



            host.RunAsync();

            //AppDomain.CurrentDomain.ProcessExit += OnProcessExit;


            async Task<string> OnConsuming(string message)
            {
                try
                {
                    WriteMessage("OnConsuming method called with message " + message);

                    JObject notificationObj = JObject.Parse(message);
                    string armResponseQueue = string.Empty;
                    string queueData = GetTokenIgnoreCase(notificationObj, "QueueJson")?.ToString();

                    if (queueData != null)
                    {
                        string queueDataEscape = queueData.Replace("\r\n", "").Replace("\\", "").Trim('"');
                        notificationObj = JsonConvert.DeserializeObject<JObject>(queueDataEscape);
                    }

                    string startDate = GetTokenIgnoreCase(notificationObj, "start_date")?.ToString();
                    string period = GetTokenIgnoreCase(notificationObj, "period")?.ToString();
                    string sendTime = GetTokenIgnoreCase(notificationObj, "sendtime")?.ToString();
                    string sendOn = GetTokenIgnoreCase(notificationObj, "sendon")?.ToString();
                    string project = GetTokenIgnoreCase(notificationObj, "project")?.ToString();
                    string notification = GetTokenIgnoreCase(notificationObj, "notification")?.ToString();
                    string userName = GetTokenIgnoreCase(notificationObj, "username")?.ToString();
                    string modifiedOn = GetTokenIgnoreCase(notificationObj, "modifiedon")?.ToString();
                    string Active = GetTokenIgnoreCase(notificationObj, "active")?.ToString();
                    string Name = GetTokenIgnoreCase(notificationObj, "name")?.ToString();

                    DateTime parsedDate;
                    string formattedDate = null;

                    if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        formattedDate = parsedDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        Console.WriteLine("Invalid date format for start_date");
                    }


                    var timeJson = new
                    {
                        startDate = formattedDate,
                        period = period,
                        sendTime = sendTime,
                        sendOn = sendOn,
                        firstDayOfWeek = config.AppConfig["FirstDayOfWeek"].ToString()
                    };

                    var scheduler = new AxpertScheduler();
                    var nextOccurrance = scheduler.GetNextOccurrence(JsonConvert.SerializeObject(timeJson));
                    TimeSpan timeDiff = nextOccurrance - DateTime.Now;
                    int delayInMs = (int)timeDiff.TotalMilliseconds;

                    if (string.IsNullOrEmpty(modifiedOn))
                    {
                        var dt = await GetNotificationModifiedOn(project, notification);
                        if (dt == null || dt.Rows.Count == 0)
                        {
                            Console.WriteLine($"Notification '{notification}' is not available in project '{project}'. Message is discarded. ");
                            return "";
                        }

                        modifiedOn = dt.Rows[0][0].ToString();
                        Active = dt.Rows[0][1].ToString();
                        Name = dt.Rows[0][2].ToString();
                        UpdateOrAddJsonKey(ref notificationObj, "modifiedon", modifiedOn);
                        UpdateOrAddJsonKey(ref notificationObj, "active", Active);
                        UpdateOrAddJsonKey(ref notificationObj, "modifiedon", Name);
                    }


                    var res1 = ScheduleJob(notificationObj, notification, project, userName, Active);
                    return "job scheduled";
                }
                catch (Exception ex)
                {
                    var errResult = JsonConvert.SerializeObject(new { error = new List<string> { ex.Message } });
                    WriteMessage(errResult);
                    return ex.Message;
                }


            }

            static void WriteMessage(string message)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - " + message);
            }

            static JToken GetTokenIgnoreCase(JObject jObject, string propertyName)
            {

                var property = jObject.Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                return property?.Value;
            }

            static void RemoveJsonKey(ref JObject jsonObject, string key)
            {
                foreach (var property in jsonObject.Properties())
                {
                    if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        jsonObject.Remove(property.Name);
                        break;
                    }
                }
            }

            static void UpdateOrAddJsonKey(ref JObject jsonObject, string key, JToken value)
            {
                bool keyExists = false;

                foreach (var property in jsonObject.Properties())
                {
                    if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        property.Value = value;
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists)
                {
                    jsonObject[key] = value;
                }
            }

            async Task<DataTable> GetNotificationModifiedOn(string appName, string notificationName)
            {
                var context = new ARMCommon.Helpers.DataContext(configuration);
                var redis = new RedisHelper(configuration);
                Utils utils = new Utils(configuration, context, redis);

                try
                {
                    Dictionary<string, string> config = await utils.GetDBConfigurations(appName);
                    string connectionString = config["ConnectionString"];
                    string dbType = config["DBType"];
                    string sql = Constants_SQL.GET_PERIODICNOTIFICATIION_MODIFIEDON;

                    string[] paramNames = { "@name" };
                    DbType[] paramTypes = { DbType.String };
                    object[] paramValues = { notificationName };

                    IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramNames, paramTypes, paramValues);
                    return await dbHelper.ExecuteQueryAsync(sql, connectionString, paramNames, paramTypes, paramValues);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:" + ex.Message);

                    return null;
                }
            }

            rabbitMQConsumer.DoConsume(queueName, OnConsuming);
        }

        private static APIResult ParseAxpertRestAPIResult(ARMResult armResult)
        {
            var result = new APIResult();
            var resultJson = JObject.Parse(armResult.result["message"].ToString());

            if (!(resultJson["status"]?.ToString().ToLower() == "true" || resultJson["status"]?.ToString().ToLower() == "success"))
            {
                result.error = resultJson["result"].ToString();
                return result;
            }

            foreach (var property in resultJson.Properties())
            {
                if (property.Name.ToLower() != "status")
                    result.data.Add(property.Name, property.Value);
            }

            return result;
        }

        public static async Task<string> ScheduleJob(JObject notificationObj, string notification, string project, string userName, string Active)
        {
            try
            {

                if (Active == "F")
                {
                    var deletionResult = await DeleteExistingJobs(notification);
                    WriteMessage($"Deleted Notification with Name: {notification}");
                    return $"Deleted Notification with Name: {notification}";
                }

                string startDate = notificationObj["start_date"].ToString();
                string period = notificationObj["period"].ToString();
                string sendTime = notificationObj["sendtime"].ToString();
                string sendOn = notificationObj["sendon"].ToString();


                DateTime parsedDate;
                string formattedDate = null;

                if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    formattedDate = parsedDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    Console.WriteLine("Invalid date format for start_date");
                }

                var timeJson1 = new
                {
                    startDate = formattedDate,
                    period = period,
                    sendTime = sendTime,
                    sendOn = sendOn
                };

                var scheduler = new AxpertScheduler();
                var nextOccurrance = scheduler.GetNextOccurrence(JsonConvert.SerializeObject(timeJson1));
                TimeSpan timeDiff = nextOccurrance - DateTime.Now;
                int delayInMs = (int)timeDiff.TotalMilliseconds;


                var periodicNotifyObject = new[]
                {
                    new
                    {
                        periodicnotifyname = notification,
                        trace = "t",
                        modifiedon = ""

                    }
                };

                var payload = new
                {
                    axnotify = new
                    {
                        axpapp = project,
                        scriptname = "axpeg_notification",
                        username = userName,
                        isperiodicnotification = "true",
                        notifications = periodicNotifyObject,
                        active = Active,
                    }
                };

                string mediaType = "application/json";
                var _api = new API();
                var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = File.ReadAllText(appSettingsPath);
                dynamic config = JsonConvert.DeserializeObject(json);
                string queueName = config.AppConfig["QueueName"];
                var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                var configuration = builder.Build();
                var context = new ARMCommon.Helpers.DataContext(configuration);
                var redis = new RedisHelper(configuration);
                Utils utils = new Utils(configuration, context, redis);
                string AxpertWebScriptsURL = config[$"ConnectionStrings:{project}_scriptsurl"];
                if (string.IsNullOrEmpty(AxpertWebScriptsURL))
                    AxpertWebScriptsURL = await utils.AxpertWebScriptsURL(project);
                string apiUrl = AxpertWebScriptsURL + "/ASBScriptRest.dll/datasnap/rest/TASBScriptRest/axnotify";
                var apiResult = await _api.POSTData(apiUrl, JsonConvert.SerializeObject(payload), mediaType);
                var result = JsonConvert.SerializeObject(ParseAxpertRestAPIResult(apiResult));
                WriteMessage("API URL: " + apiUrl);
                WriteMessage("Payload: " + JsonConvert.SerializeObject(payload));
                WriteMessage($"API Result : {result}");
                var HangfireScheduleJob = BackgroundJob.Schedule(() => ScheduleJob(notificationObj, notification, project, userName, Active), timeDiff);
                Console.WriteLine($"Notification '{notification}' is scheduled for: {nextOccurrance.ToShortDateString()} - {nextOccurrance.ToShortTimeString()}");
                return $"Notification '{notification}' scheduled";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ScheduleJob: " + ex.Message);
                return "Error in ScheduleJob";
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    new WebHostBuilder()
        .UseKestrel()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .ConfigureServices((context, services) =>
        {
            services.AddHangfire(config =>
            {
                var postgresConnectionString = context.Configuration.GetConnectionString("WebApiDatabase");
                config.UsePostgreSqlStorage(postgresConnectionString);
            });
        })
        .Configure(app =>
        {
            app.UseHangfireDashboard("/hangfire/PeriodicNotification", new DashboardOptions
            {
            });
            app.UseHangfireServer();
        });


        static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + " - " + message);
        }

        public static async Task<string> DeleteExistingJobs(string notification)
        {
            try
            {
                var monitor = JobStorage.Current?.GetMonitoringApi();
                if (monitor == null)
                {
                    return "Job monitor is not available.";
                }

                var jobsProcessing = monitor.ScheduledJobs(0, int.MaxValue)
                    .Where(x => x.Value.Job.Method.Name == nameof(ScheduleJob) && x.Value.Job.Args.Count > 4 && x.Value.Job.Args[1]?.ToString() == notification);
                foreach (var job in jobsProcessing)
                {
                    BackgroundJob.Delete(job.Key);
                }

                var jobsScheduled = monitor.ScheduledJobs(0, int.MaxValue)
                    .Where(x => x.Value.Job.Method.Name == nameof(ScheduleJob) && x.Value.Job.Args.Count > 4 && x.Value.Job.Args[1]?.ToString() == notification);
                foreach (var job in jobsScheduled)
                {
                    BackgroundJob.Delete(job.Key);
                }

                return $"Deleted job(s) with Name: {notification}";
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }

        private static async void Execute(object state)
        {
            try
            {

                var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();
                var context = new DataContext(configuration);
                Assembly assembly = Assembly.GetExecutingAssembly();
                string serviceName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                string exeName = assembly.Location;
                string exePath = Path.GetDirectoryName(exeName);
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var appConfigSection = configuration.GetSection("AppConfig");
                int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                var otherInfoValue = JsonConvert.SerializeObject(appConfigSection.Get<Dictionary<string, object>>());
                DateTime lastDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
                var newLog = new ARMServiceLogs()
                {
                    ServiceName = serviceName,
                    Server = ipv4Address?.ToString(),
                    Folder = exePath,
                    InstanceID = processId,
                    OtherInfo = otherInfoValue,
                    Status = "Running",
                    LastOnline = DateTime.Now,
                    StartOnTime = lastDate,
                    IsMailSent = true
                };

                context.Add(newLog);
                await context.SaveChangesAsync();
                WriteMessage("Service Running " + DateTime.Now);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting executable information: {ex.Message}");
            }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();
                var context = new DataContext(configuration);
                Assembly assembly = Assembly.GetExecutingAssembly();
                string serviceName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                string exeName = assembly.Location;
                string exePath = Path.GetDirectoryName(exeName);
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var appConfigSection = configuration.GetSection("AppConfig");
                int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                var otherInfoValue = JsonConvert.SerializeObject(appConfigSection.Get<Dictionary<string, object>>());
                DateTime lastDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
                if (DateTime.Now - lastDate > TimeSpan.FromMinutes(5))
                {
                    var newLog = new ARMServiceLogs()
                    {
                        ServiceName = serviceName,
                        Server = ipv4Address?.ToString(),
                        Folder = exePath,
                        InstanceID = processId,
                        OtherInfo = otherInfoValue,
                        Status = "Stopped",
                        LastOnline = DateTime.Now,
                        StartOnTime = lastDate,
                    };

                    context.Add(newLog);
                    context.SaveChanges();
                    WriteMessage("Service Stopped " + DateTime.Now);
                    timer?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on service stop: {ex.Message}");
            }
        }

        private static void CleanupOnExit()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();
                // Perform cleanup tasks when application exits
                var context = new DataContext(configuration);
                Assembly assembly = Assembly.GetExecutingAssembly();
                string serviceName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                string exeName = assembly.Location;
                string exePath = Path.GetDirectoryName(exeName);
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var appConfigSection = configuration.GetSection("AppConfig");
                int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                var otherInfoValue = JsonConvert.SerializeObject(appConfigSection.Get<Dictionary<string, object>>());
                DateTime lastDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

                var newLog = new ARMServiceLogs()
                {
                    ServiceName = serviceName,
                    Server = ipv4Address?.ToString(),
                    Folder = exePath,
                    InstanceID = processId,
                    OtherInfo = otherInfoValue,
                    Status = "Stopped",
                    LastOnline = DateTime.Now,
                    StartOnTime = lastDate,
                };

                context.Add(newLog);
                context.SaveChanges();
                Console.WriteLine("Service Stopped " + DateTime.Now);
                timer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on service stop: {ex.Message}");
            }
        }

        public class CleanupService
        {
            private readonly IConfiguration configuration;
            private readonly ILogger<CleanupService> logger;

            public CleanupService(IConfiguration configuration, ILogger<CleanupService> logger)
            {
                this.configuration = configuration;
                this.logger = logger;
            }

            public void OnProcessExit()
            {
                try
                {
                    var context = new DataContext(configuration);
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string serviceName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                    string exeName = assembly.Location;
                    string exePath = Path.GetDirectoryName(exeName);
                    string hostName = Dns.GetHostName();
                    IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                    IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    var appConfigSection = configuration.GetSection("AppConfig");
                    int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                    var otherInfoValue = JsonConvert.SerializeObject(appConfigSection.Get<Dictionary<string, object>>());
                    DateTime lastDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

                    var newLog = new ARMServiceLogs()
                    {
                        ServiceName = serviceName,
                        Server = ipv4Address?.ToString(),
                        Folder = exePath,
                        InstanceID = processId,
                        OtherInfo = otherInfoValue,
                        Status = "Stopped",
                        LastOnline = DateTime.Now,
                        StartOnTime = lastDate,
                    };

                    context.Add(newLog);
                    context.SaveChanges();
                    WriteMessage("Service Stopped " + DateTime.Now);
                    timer?.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error on service stop: {ex.Message}");
                }
            }

        }
    }
}


public class APIResult
{
    public string error { get; set; }
    public Dictionary<string, object> data = new Dictionary<string, object>();
}

