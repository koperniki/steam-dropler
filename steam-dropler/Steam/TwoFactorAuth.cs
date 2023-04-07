using System.Threading.Tasks;
using SteamKit2.Authentication;
using SteamKit2.Internal;

namespace steam_dropler.Steam;

public class TwoFactorAuth : IAuthenticator
{
    private readonly string _token;

    public TwoFactorAuth(string token)
    {
        _token = token;
    }
    
    public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
    {
        return await SteamGuardAuthenticator.GetGuardCode(_token);
    }

    public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
    {
        return Task.FromResult("");
    }

    public Task<bool> AcceptDeviceConfirmationAsync()
    {
        return Task.FromResult( true );
    }

    public EAuthSessionGuardType NeedGuardType()
    {
        return EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode;
    }
}