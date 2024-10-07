using API.Core.Services;
using Hangfire;

namespace API.Configurations;

public class JobScheduler
{
    public static void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<ITokenService>(
            "remove-expired",
            service => service.RemoveExpiredTokensAsync(CancellationToken.None),
            "*/15 * * * *"
            );        
    }
}