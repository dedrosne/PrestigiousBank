using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class DrakenhofBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _drakenhofTownID = "town_comp_SY1";
        public static DrakenhofBank _bankDrakenhof = null;
        //public static string BankMenuLinkText = "<a style=\"Link.Settlement\" href=\"event:Concept-str_game_objects_simple_bank\"><b>" + PrestigiousBank.Config.BankName + "</b></a>";

        public static DrakenhofBank BankInstance
        {
            get
            {
                if (_bankDrakenhof == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _drakenhofTownID)
                            {
                                _bankDrakenhof = new DrakenhofBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }

                }
                if (_bankDrakenhof != null && _bankDrakenhof.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _drakenhofTownID)
                        {
                            _bankDrakenhof.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bankDrakenhof;

            }
            set
            {
                _bankDrakenhof = value;
            }
        }

        public DrakenhofBankCampaignBehavior() : base()
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, this.HourlyTickEvent);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new DrakenhofBankMenu().RegisterBankMenu(campaignGameStarter, BankInstance);
        }

        private void DailyTickClan()
        {
            //Ajout de l'énergie noire
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "DarkEnergy")
                Hero.MainHero.AddCultureSpecificCustomResource(BankInstance.CalculateDarkEnergyInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( DefaultSkills.Charm, BankInstance.GetDailySkillXP());

            //Ajout des Mercenaires
            if (BankInstance.CanRecruitMercenariesInThisBank) BankInstance.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            var time = Campaign.CurrentTime;
            if ((int)time % 24 == 14)
            {
                if (BankInstance.LoanAmount > 0)
                {
                    BankInstance.ApplyLoanRefound();
                }
            }
            BankInstance.ApplyDiamondLevelGoldTownIncrease();
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<DrakenhofBank>("DrakenhofBank", ref _bankDrakenhof);
                }
                else
                {
                    dataStore.SyncData<DrakenhofBank>("DrakenhofBank", ref _bankDrakenhof);
                }
            }
            catch
            {
            }
        }

    }
}