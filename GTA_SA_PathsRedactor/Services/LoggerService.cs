using Serilog;

namespace GTA_SA_PathsRedactor.Services;

public sealed class LoggerService
{
    public ILogger CreateLogger(string fileName = "log.txt", 
                                Serilog.Events.LogEventLevel restrictedToMinimumLevel = Serilog.Events.LogEventLevel.Information)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            // .WriteTo.FastConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(restrictedToMinimumLevel: restrictedToMinimumLevel)
            .WriteTo.File(fileName, restrictedToMinimumLevel: restrictedToMinimumLevel)
            .CreateLogger();
    } 
}