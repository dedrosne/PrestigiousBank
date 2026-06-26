using Newtonsoft.Json;
using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
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
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class TestBankCampaignBehavior : BankBehaviour
    {
        public static string _altdorfTownID = "town_comp_RL1";
        public new static AltdorfBank _bankInstance = null;


        public TestBankCampaignBehavior() : base()
        { }

        public override void RegisterEvents()
        {
            base.RegisterEvents();
        }

        protected override void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            base.OnSessionLaunched(campaignGameStarter);
            new AltdorfBankMenu().RegisterBankMenu(campaignGameStarter, BankInstance);
        }

        protected override void DailyTickClan()
        {
            base.DailyTickClan();
            //Ajout du prestige
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "Prestige")
                Hero.MainHero.AddCultureSpecificCustomResource(((AltdorfBank)BankInstance).CalculatePrestigiousInterests());

            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( TORSkills.Spellcraft,BankInstance.GetDailySkillXP());
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<AltdorfBank>("AltdorfBank", ref _bankInstance);
                }
                else
                {
                    dataStore.SyncData<AltdorfBank>("AltdorfBank", ref _bankInstance);
                }
            }
            catch
            {
            }
        }

    }
}