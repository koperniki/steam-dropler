using System;
using System.Threading;
using System.Threading.Tasks;
using steam_dropler.Model;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Discovery;
using SteamKit2.Internal;

namespace steam_dropler.Steam
{
    public class SteamLoginHandler
    {
        private readonly SteamClient _client;
        private readonly SteamUser _sUser;
        private int tryLoginCount;

        private readonly AccountConfig _steamAccount;

        private readonly TaskCompletionSource<EResult> _loginTcs;

        private ServerRecord _serverRecord;
        private string _refreshToken;

        public ServerRecord ServerRecord => _serverRecord;


        public SteamLoginHandler(AccountConfig steamAccount, SteamClient client, CallbackManager manager)
        {
         
            _steamAccount = steamAccount;
            _client = client;
            _sUser = _client.GetHandler<SteamUser>();

            _loginTcs = new TaskCompletionSource<EResult>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

        }


        public Task<EResult> Login(ServerRecord serverRecord)
        {
            _serverRecord = serverRecord;
            _client.Connect(_serverRecord);
            return _loginTcs.Task;
        }


        async void OnConnected(SteamClient.ConnectedCallback callback)
        {
            
            Console.Write("Connected to Steam! Logging in '{0}'...", _steamAccount.Name);


            if (_steamAccount.AccessToken != null)
            {
                _sUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = _steamAccount.Name,
                    AccessToken = _steamAccount.AccessToken,
                    ShouldRememberPassword = true
                });
            }
            else
            {

                IAuthenticator auth;
                switch (_steamAccount.AuthType)
                {
                    case AuthType.Console:
                        auth = new UserConsoleAuthenticator();
                        break;
                   
                    case AuthType.WithSecretKey:
                        auth = new TwoFactorAuth(_steamAccount.SharedSecret ?? _steamAccount.MobileAuth.SharedSecret);
                        break;

                    case AuthType.Device:
                    default:
                        auth = new DeviceAuth();
                        break;

                }
                
                var authSession = await _client.Authentication.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails
                {
                    Username = _steamAccount.Name,
                    Password = _steamAccount.Password,
                    ClientOSType = EOSType.Windows10,
                    DeviceFriendlyName = _steamAccount.Name + "pc",
                    PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_SteamClient,
                    IsPersistentSession = true,
                    WebsiteID = "Client",
                    Authenticator = auth,
                });

                try
                {
                    var pollResponse = await authSession.PollingWaitForResultAsync();
                    _refreshToken = pollResponse.RefreshToken;
                    _sUser.LogOn(new SteamUser.LogOnDetails
                    {
                        Username = pollResponse.AccountName,
                        AccessToken = pollResponse.RefreshToken,
                        ShouldRememberPassword = true
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception while logon to Steam: {0}", e.Message);
                    _loginTcs?.TrySetResult(EResult.UnexpectedError);
                }
            }
        }


        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _client.Connect(_serverRecord);

        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            Console.WriteLine(callback.Result);

            tryLoginCount++;
            if (tryLoginCount > 5)
            {
                _loginTcs?.SetResult(callback.Result);
            }

            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.InvalidPassword)
                {
                    _steamAccount.SharedSecret = null;
                    _steamAccount.Save();
                }
                else if (callback.Result == EResult.NoConnection)
                {
                    SteamServerList.SetBadServer(_serverRecord);
                    _serverRecord = SteamServerList.GetServerRecord();
                }

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                return;
            }
            _steamAccount.SteamId = _client.SteamID;
            _steamAccount.AccessToken = _refreshToken;
            Console.WriteLine("Successfully logged on!");
            _loginTcs?.SetResult(callback.Result);
        }


        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
        
    }
}
