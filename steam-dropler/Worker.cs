using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using steam_dropler.Model;
using steam_dropler.Steam;
using SteamKit2;

namespace steam_dropler
{
    public static class Worker
    {
        private const string AccountPath = "Configs\\Accounts";

        private static HashSet<AccountConfig> _accounts;

	    private static Dictionary<string, MobileAuth> _mobileAuths;

        private static readonly Dictionary<string, Task> TaskDictionary = new Dictionary<string, Task>();

        private static Timer _timer;

        public static void Start()
        {

            

            _timer = new Timer(1000 * MainConfig.Config.StartTimeOut);
            _timer.Elapsed += CheckToAdd;
            _timer.Start();
            Console.WriteLine($"Аккаунты запускаются с переодичностью {MainConfig.Config.StartTimeOut} секунд.");
            Console.WriteLine($"Всего аккаунтов {_accounts.Count}, будут фармиться {_accounts.Count(t=>t.IdleEnable && t.MobileAuth?.SharedSecret!=null)}");

        }

        public static void Run()
        {
            MainConfig.Load();
            LoadAccounts();
	        LoadMaFiles();
	        SetMaFiles();
            Start();
        }

        private static void CheckToAdd(object sender, ElapsedEventArgs e)
        {
            if (TaskDictionary.Count > MainConfig.Config.ParallelCount)
            {
                return;
            }

            var steamaccont = _accounts.FirstOrDefault(t =>
                t.LastRun < DateTime.UtcNow.AddHours(-t.TimeConfig.PauseBeatwinIdleTime) & t.IdleEnable && !t.IdleNow);

            if (steamaccont != null)
            {
                AddToIdlingQueue(steamaccont);
            }

        }


        private static void AddToIdlingQueue(AccountConfig account)
        {
            var id = Guid.NewGuid().ToString();

            TaskDictionary[id] = Task.Run(async () =>
            {
                try
                {
                    var machine = new SteamMachine(account);
                    var result = await machine.EasyIdling();
                    if (result != EResult.OK)
                    {
                        Console.WriteLine($"not login {result}");
                    }
                    machine.LogOf();
                    TaskDictionary.Remove(id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            });


        }

        private static void LoadAccounts()
        {
            _accounts = new HashSet<AccountConfig>(new AccontConfigComparer());

	        if (Directory.Exists(AccountPath))
	        {
		        var jsonPaths = Directory.GetFiles(AccountPath).Where(t => Path.GetExtension(t) == ".json");

		        foreach (var jsonPath in jsonPaths)
		        {
			        _accounts.Add(new AccountConfig(jsonPath));
		        }
                
	        }
	        else
	        {
		        throw new Exception("Account folder not exist");
	        }

        }

	    private static void LoadMaFiles()
	    {			
			var objects = new List<MobileAuth>();
		    if (!string.IsNullOrEmpty(MainConfig.Config.MaFileFolder) && Directory.Exists(MainConfig.Config.MaFileFolder))
		    {
				var maFilePaths = Directory.GetFiles(MainConfig.Config.MaFileFolder).Where(t => Path.GetExtension(t) == ".maFile");

			    foreach (var maFile in maFilePaths)
			    {
				    var obj = JsonConvert.DeserializeObject<MobileAuth>(File.ReadAllText(maFile));
					objects.Add(obj);
			    }
			    _mobileAuths = objects.ToDictionary(t => t.AccountName, t => t);

		    }
		    else
		    {
                _mobileAuths = new Dictionary<string, MobileAuth>();
			    Console.WriteLine("MaFile folder not exist");
		    }

		}

	    private static void SetMaFiles()
	    {
		    foreach (var accountConfig in _accounts)
		    {
			    if (_mobileAuths.ContainsKey(accountConfig.Name))
			    {
				    accountConfig.MobileAuth = _mobileAuths[accountConfig.Name];
			    }
		    }
	    }



    }
}
