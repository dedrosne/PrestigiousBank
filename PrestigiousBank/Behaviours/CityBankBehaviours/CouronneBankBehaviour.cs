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
    public class CouronneBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _CouronneTownID = "town_comp_CO1";
        public static CouronneBank _bankCouronne = null;

        public static CouronneBank BankInstance
        {
            get
            {
                if (_bankCouronne == null)
                {
                    if (Town.AllTowns != null)
                    {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _CouronneTownID)
                            {
                                _bankCouronne = new CouronneBank(town.Settlement);
                                break;
                            }

                        }

                    }

                }
                if (_bankCouronne != null && _bankCouronne.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _CouronneTownID)
                        {
                            _bankCouronne.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bankCouronne;

            }
            set
            {
                _bankCouronne = value;
            }
        }

        public CouronneBankCampaignBehavior() : base()
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
            new CouronneBankMenu().RegisterBankMenu(campaignGameStarter, BankInstance);
        }

        private void DailyTickClan()
        {
            //Ajout de la chivalrie
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "Chivalry")
                Hero.MainHero.AddCultureSpecificCustomResource(BankInstance.CalculateChivalryInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( DefaultSkills.Riding, BankInstance.GetDailySkillXP());

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
                    dataStore.SyncData<CouronneBank>("CouronneBank", ref _bankCouronne);
                }
                else
                {
                    dataStore.SyncData<CouronneBank>("CouronneBank", ref _bankCouronne);
                }
            }
            catch
            {
            }
        }
        

    }
}