using System;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        // Create a logger factory
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Log to the console
            // Add more providers as needed (e.g., file, debug, etc.)
        });

        // Create a logger instance
        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Application started.");

        // Your application logic
        try
        {
            Console.WriteLine("Press Ctrl+C to simulate termination or close via Task Manager.");

            // Registering for Ctrl+C termination
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // Prevent immediate termination
                logger.LogInformation("Ctrl+C pressed. Cleaning up...");
                CleanupAndExit(logger);
            };

            // Simulate application running
            while (true)
            {
                Console.WriteLine("Application is running. Press Ctrl+C to terminate.");
                // Simulate some work
                System.Threading.Thread.Sleep(5000); // Simulate work
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions gracefully
            logger.LogError(ex, "Exception occurred.");
        }
    }

    static void CleanupAndExit(ILogger logger)
    {
        try
        {
            // Simulate cleanup or service stopping
            logger.LogInformation("Cleaning up resources or stopping services...");
            // Perform actual cleanup here

            logger.LogInformation("Cleanup complete.");
        }
        catch (Exception ex)
        {
            // Handle cleanup exceptions
            logger.LogError(ex, "Cleanup exception occurred.");
        }
        finally
        {
            // Exit the application
            Environment.Exit(0);
        }
    }
}
