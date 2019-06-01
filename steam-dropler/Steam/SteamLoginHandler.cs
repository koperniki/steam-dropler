using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using steam_dropler.Model;
using SteamKit2;
using SteamKit2.Discovery;

namespace steam_dropler.Steam
{
    public class SteamLoginHandler
    {
        private readonly SteamClient _client;
        private readonly SteamUser _sUser;
        private int tryLoginCount;
        private string _authCode;
        private string _twoFactorAuth;

        private readonly AccountConfig _steamAccount;

        private readonly TaskCompletionSource<EResult> _loginTcs;

        private ServerRecord _serverRecord;

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
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            manager.Subscribe<SteamUser.LoginKeyCallback>(OnKeyCallback);

        }


        public Task<EResult> Login(ServerRecord serverRecord)
        {
            _serverRecord = serverRecord;
            _client.Connect(_serverRecord);
            return _loginTcs.Task;
        }



        private void OnKeyCallback(SteamUser.LoginKeyCallback obj)
        {
            _steamAccount.LoginKey = obj.LoginKey;
            _steamAccount.Save();
        }


        void OnConnected(SteamClient.ConnectedCallback callback)
        {
           // Console.WriteLine("Connected to Steam! Logging in '{0}'...", _steamAccount.UserName);
            _steamAccount.SteamId = _client.SteamID;
            byte[] sentryHash = null;
            string loginKey = null;
            if (_steamAccount.SentryHash != null)
            {

                sentryHash = _steamAccount.SentryHash;
            }
            if (_steamAccount.LoginKey != null)
            {
                loginKey = _steamAccount.LoginKey;
            }
            _sUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = _steamAccount.Name,
                Password = _steamAccount.Password,
                LoginKey = loginKey,
                AuthCode = _authCode,
                TwoFactorCode = _twoFactorAuth,
                SentryFileHash = sentryHash,
                ShouldRememberPassword = true,
            });
            _authCode = null;
            _twoFactorAuth = null;
        }


        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _client.Connect(_serverRecord);

        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2Fa = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;
            tryLoginCount++;
            if (tryLoginCount > 5)
            {
                _loginTcs?.SetResult(callback.Result);
            }
            if (isSteamGuard || is2Fa)
            {

                // Console.WriteLine("This account is SteamGuard protected!");

                if (is2Fa)
                {

                    _twoFactorAuth = _steamAccount.MobileAuth.GenerateSteamGuardCode();

                }
                else
                {
                    throw new NotImplementedException("Not inplemented auth code. Only mobile auth");
                    
                }
                return;
            }

            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.InvalidPassword)
                {
                    _steamAccount.LoginKey = null;
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
 

            _loginTcs?.SetResult(callback.Result);

            Console.WriteLine("Successfully logged on!");



        }


        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);

        }


        void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            int fileSize;
            byte[] sentryHash;
            // using (var fs = File.Open(_steamAccount.UserName + "sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var fs = new MemoryStream())
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    sentryHash = sha.ComputeHash(fs);
                    _steamAccount.SentryHash = sentryHash;
                }
            }

            _sUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

    }
}
