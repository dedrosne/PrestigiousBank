//using Birke.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PrestigiousBank
{
    public class PrestigiousBank : MBSubModuleBase
    {
        public static Configuration Config { get; set; }
        private static FileSystemWatcher _fileWatcher;
        private static string _configPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\configuration"));
        private static string _version = "v1.3.9.0";
        private static string _previousVersion = "v1.3.8.0";

        public PrestigiousBank()
        {

        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony harmony = new Harmony("PrestigiousBank");
            harmony.PatchAll();
        }
        protected override void OnGameStart(Game game, IGameStarter starter)
        {
            base.OnGameStart(game, starter);

            if (!(game.GameType is Campaign) || !(starter is CampaignGameStarter campaignGameStarter))
                return;

            try
            {
                CreateDefaultConfiguration();
                // ============================================================
                // Core persistence and behaviors
                // ============================================================
                campaignGameStarter.AddBehavior(new AltdorfBankCampaignBehavior());
                campaignGameStarter.AddBehavior(new DrakenhofBankCampaignBehavior());

                // ============================================================
                // Core models and processors
                // ============================================================
                //campaignGameStarter.AddModel(new FinanceProcessor());
                //campaignGameStarter.AddBehavior(new BankLoanProcessor());
                //campaignGameStarter.AddModel(new BankProsperityModel());
                starter.AddModel((ClanFinanceModel)new PrestigiousFinanceModel());

                // ============================================================
                // Bank menus
                // ============================================================
                //AltdorfBankMenu.registerMenu(campaignGameStarter);
                //BankMenu_Loan.RegisterMenu(campaignStarter, bankBehavior);
                //BankMenu_LoanPay.RegisterMenu(campaignStarter, bankBehavior);

                // ============================================================
                // Initialization message (localization-safe)
                // ============================================================
                GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BanksOfCalradia][ERROR] Initialization failed: " + e.Message,
                    Color.FromUint(0xFFFF6666)
                ));
            }
        }

        /*private static void CreateConfig()
         {
             try
             {
                 if (File.Exists(_configPath + _version + ".xml"))
                 {
                    // Read config
                    PrestigiousBank.Config = ReadConfig<Configuration>(_configPath + _version + ".xml");
                 }
                 else
                 {
                     // Check if previous config exists
                     if (File.Exists(_configPath + _previousVersion + ".xml"))
                     {
                         // Merge
                         var oldConfig = ReadConfig<Configuration>(_configPath + _previousVersion + ".xml");
                         Config = CreateDefaultConfiguration(oldConfig);
                         CreateConfigFile<Configuration>(Config, _configPath + _version + ".xml");
                     }
                     else if (File.Exists(_configPath + ".xml"))
                     {
                         // CAN BE DELETED IN FEW VERSIONS ONWARDS
                         // Merge
                         var oldConfig = ReadConfig<Configuration.Configuration>(_configPath + ".xml");
                         Config = CreateDefaultConfiguration(oldConfig);
                         CreateConfigFile<Configuration>(Config, _configPath + _version + ".xml");
                         // Save old config in right format
                         CreateConfigFile<Configuration.Configuration>(oldConfig, _configPath + _previousVersion + ".xml");
                     }
                     else
                     {
                         // Create new default configurations
                         Config = CreateDefaultConfiguration();
                         CreateConfigFile<Configuration>(Config, _configPath + _version + ".xml");
                     }
                 }
             }
             catch
             {
                 Config = CreateDefaultConfiguration();
                 CreateConfigFile<Configuration>(Config, _configPath + _version + ".xml");
             }

             var directory = System.IO.Path.GetDirectoryName(_configPath + _version + ".xml");
             var filename = System.IO.Path.GetFileName(_configPath + _version + ".xml");

             _fileWatcher = new FileSystemWatcher(directory, filename);

             _fileWatcher.NotifyFilter = NotifyFilters.Attributes
                                  | NotifyFilters.CreationTime
                                  | NotifyFilters.DirectoryName
                                  | NotifyFilters.FileName
                                  | NotifyFilters.LastAccess
                                  | NotifyFilters.LastWrite
                                  | NotifyFilters.Security
                                  | NotifyFilters.Size;

             _fileWatcher.Changed += OnChanged;
             _fileWatcher.IncludeSubdirectories = true;
             _fileWatcher.EnableRaisingEvents = true;
         }*/

        private static T ReadConfig<T>(string configPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (Stream reader = new FileStream(configPath, FileMode.Open))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                Config = ReadConfig<Configuration>(_configPath + _version + ".xml");
            }
            catch (Exception)
            {

            }
        }

        private static void CreateConfigFile<T>(object config, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            TextWriter writer = new StreamWriter(path);
            ser.Serialize(writer, config);
            writer.Close();
        }

        private static Configuration CreateDefaultConfiguration(Configuration oldConfig = null)
        {
            var config = new Configuration()
            {
                Rent = (oldConfig == null) ? 0.05 : oldConfig.Rent,
                Interest = (oldConfig == null) ? 0.001 : oldConfig.Interest,
                MaxTotalLoanAmount = (oldConfig == null) ? 20000 : oldConfig.MaxTotalLoanAmount,
                MaxLoanAmount = (oldConfig == null) ? 1000 : oldConfig.MaxLoanAmount,
                //BankName = (oldConfig == null || oldConfig.BankName == "Bronislaw's National Bank of Krenn") ? GameTexts.FindText("str_configBankName", null).ToString() : oldConfig.BankName,
                //BankMenuName = (oldConfig == null || oldConfig.BankMenuName == "Visit Bronislaw's Bank") ? GameTexts.FindText("str_configBankMenuName", null).ToString() : oldConfig.BankMenuName,
                //BankMenuBorrow = (oldConfig == null || oldConfig.BankMenuBorrow == "Take money from account") ? GameTexts.FindText("str_configBankMenuBorrow", null).ToString() : oldConfig.BankMenuBorrow,
                //BankMenuRepay = (oldConfig == null || oldConfig.BankMenuRepay == "Put money into account") ? GameTexts.FindText("str_configBankMenuRepay", null).ToString() : oldConfig.BankMenuRepay,
                //BankMenuSendCoinsToHero = (oldConfig == null || oldConfig.BankMenuSendCoinsToHero == "Send coins to a lord") ? GameTexts.FindText("str_configBankMenuSendCoinsToHero", null).ToString() : oldConfig.BankMenuSendCoinsToHero,
                //BankMenuLeave = (oldConfig == null || oldConfig.BankMenuLeave == "Back to city") ? GameTexts.FindText("str_configBankMenuLeave", null).ToString() : oldConfig.BankMenuLeave,
                //BankMenuSolde = (oldConfig == null || oldConfig.BankMenuSolde == "Balance") ? GameTexts.FindText("str_configBankMenuSaldo", null).ToString() : oldConfig.BankMenuSolde,
                //ClerkNames = (oldConfig == null) ? GameTexts.FindText("str_configClerkNames", null).ToString().Split(';').ToList() : oldConfig.ClerkNames,
                DisablePopups = (oldConfig == null) ? false : oldConfig.DisablePopups,
                DoBalancing = (oldConfig == null) ? true : oldConfig.DoBalancing,
                BalanceWeakestNationAmount = (oldConfig == null) ? 20000 : oldConfig.BalanceWeakestNationAmount,
                BalanceNobelGoldLimit = (oldConfig == null) ? 1000 : oldConfig.BalanceNobelGoldLimit,
                PutInterestIntoAccount = (oldConfig == null) ? true : oldConfig.PutInterestIntoAccount,
                MaxRelationChange = (oldConfig == null) ? 10 : oldConfig.MaxRelationChange,
                FiefCultureChangeCost = (oldConfig == null) ? 10000 : oldConfig.FiefCultureChangeCost,
                VillageLostCompensation = (oldConfig == null) ? 15000 : oldConfig.VillageLostCompensation,
                PercentCostToRiseLoyality = (oldConfig == null) ? 5000 : oldConfig.PercentCostToRiseLoyality,
                PercentCostToLowerLoyality = (oldConfig == null) ? 10000 : oldConfig.PercentCostToLowerLoyality,
                Cheat = (oldConfig == null) ? false : oldConfig.Cheat,
                RaidRestorationProtectionTimeInHouers = (oldConfig == null) ? 48 : oldConfig.RaidRestorationProtectionTimeInHouers,
                InfluenceTransferCost = (oldConfig == null) ? 10000 : oldConfig.InfluenceTransferCost,
                InfluenceRiseCost = (oldConfig == null) ? 8000 : oldConfig.InfluenceRiseCost,
                InfluenceLowerCost = (oldConfig == null) ? 12000 : oldConfig.InfluenceLowerCost,
/*                CaravanInsurancePeriods = (oldConfig == null) ? new List<Configuration.KeyValuePair1>() {
                new Configuration.KeyValuePair1(){ Key = $"1 {GameTexts.FindText("str_configday", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 24, Value = 100 }},
                new Configuration.KeyValuePair1(){ Key = $"2 {GameTexts.FindText("str_configdays", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 48, Value = 200 }},
                new Configuration.KeyValuePair1(){ Key = $"3 {GameTexts.FindText("str_configdays", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 72, Value = 300 }},
                new Configuration.KeyValuePair1(){ Key = $"1 {GameTexts.FindText("str_configweek", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 168, Value = 600 }},
                new Configuration.KeyValuePair1(){ Key = $"4 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 672, Value = 2000 }},
                new Configuration.KeyValuePair1(){ Key = $"16 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 4032, Value = 8000 }},
                new Configuration.KeyValuePair1(){ Key = $"64 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 16128, Value = 30000 }},
                } : oldConfig.CaravanInsurancePeriods,
                SettlementInsurancePeriods = (oldConfig == null) ? new List<Configuration.KeyValuePair1>() {
                new Configuration.KeyValuePair1(){ Key = $"1 {GameTexts.FindText("str_configday", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 24, Value = 100 }},
                new Configuration.KeyValuePair1(){ Key = $"2 {GameTexts.FindText("str_configdays", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 48, Value = 200 }},
                new Configuration.KeyValuePair1(){ Key = $"3 {GameTexts.FindText("str_configdays", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 72, Value = 300 }},
                new Configuration.KeyValuePair1(){ Key = $"1 {GameTexts.FindText("str_configweek", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 168, Value = 600 }},
                new Configuration.KeyValuePair1(){ Key = $"4 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 672, Value = 2000 }},
                new Configuration.KeyValuePair1(){ Key = $"16 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 4032, Value = 8000 }},
                new Configuration.KeyValuePair1(){ Key = $"64 {GameTexts.FindText("str_configweeks", null).ToString()}", Value = new Configuration.KeyValuePair2(){ Key = 16128, Value = 30000 }},
                } : oldConfig.SettlementInsurancePeriods,*/
            };

            return config;
        }


        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);
        }


/*        public override void OnGameLoaded(Game game, object initializerObject)
        {
            base.OnGameLoaded(game, initializerObject);

            try
            {
                var saldoItem = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().Where(x => x.StringId == "bank_account_saldo").FirstOrDefault();
                if (saldoItem != null)
                {
                    Game.Current.ObjectManager.UnregisterObject(saldoItem);
                }

                var noOfDaysItem = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().Where(x => x.StringId == "bank_account_DaysSinceLastPayment").FirstOrDefault();
                if (noOfDaysItem != null)
                {
                    Game.Current.ObjectManager.UnregisterObject(noOfDaysItem);
                }
            }
            catch
            {

            }
        }*/

         
        public static void LogMessage(string message, UInt32 color = 0xFFBBAA00)
        {
            InformationManager.DisplayMessage(new InformationMessage(message, Color.FromUint(color)));
        }
    }
}
// ui.set_debug_mode 1




//Campaign.Current.MainParty.CurrentSettlement;

