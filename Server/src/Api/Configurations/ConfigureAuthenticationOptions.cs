using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace API.Configurations;

public class ConfigureAuthenticationOptions : IConfigureNamedOptions<AuthenticationOptions>
{
    public void Configure(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    }

    public void Configure(string? name, AuthenticationOptions options)
    {
        Configure(options);
    }
}