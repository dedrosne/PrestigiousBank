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
using TOR_Core.CharacterDevelopment;
using PrestigiousBank.Entities;

namespace PrestigiousBank
{
    public class NulnFactoryCampaignBehavior : CampaignBehaviorBase
    {
        public static string townID = "town_comp_WI1";
        public static NulnFactory _factory = null;

        public static NulnFactory NulnFactory
        {
            get
            {
                if (_factory == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == townID)
                            {
                                _factory = new NulnFactory(town.Settlement);
                                break;
                            }
                                
                        }

                    }
                }
                if (_factory != null && _factory.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == townID)
                        {
                            _factory.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _factory;

            }
            set
            {
                _factory = value;
            }
        }

        public NulnFactoryCampaignBehavior() : base()
        {
            //MBTextManager.SetTextVariable("Birke_Bank_Encyclopedia_Main", PrestigiousBank.Config.BankName);
            //bankAltdorf = bankAltdorf;

            //_bankTrait = Game.Current.ObjectManager.RegisterPresumedObject<TraitObject>(new TraitObject("bank"));
            //_bankTrait.Initialize(new TextObject(GameTexts.FindText("str_trait_bankName").ToString()), new TextObject(GameTexts.FindText("str_trait_bankDescription").ToString()), false, 0, 4);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));

            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.OnTroopRecruitedEvent.AddNonSerializedListener(this, new Action<Hero, Settlement, Hero, CharacterObject, int>(this.OnTroopRecruitedEvent));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTickEvent);

        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            //new NulnFactoryBankMenu().RegisterBankMenu(campaignGameStarter, NulnFactory);
        }

        private void DailyTickClan()
        {
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Engineering, NulnFactory.GetDailySkillXP());
        }

        public void OnTroopRecruitedEvent(Hero recruiter, Settlement settlement, Hero recruitmentSource, CharacterObject troop, int amount)
        {
            if (NulnFactory.Level == 0) return;
            if (recruiter == null) return;
            if (settlement == null) return;
            if (settlement.Town == null) return;
            if (settlement.Town.StringId != townID) return;
            int valueGained = NulnFactory.ValuePerTiers[troop.Tier] * amount * NulnFactory.Level;
             NulnFactory.Benefits += valueGained;

        }

        public void HourlyTickEvent()
        {
            var time = Campaign.CurrentTime;

            if ((int)time % 24 == 9)
            {
                NulnFactory.Benefits = 0;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<NulnFactory>("NulnFactory", ref _factory);
                }
                else
                {
                    dataStore.SyncData<NulnFactory>("NulnFactory", ref _factory);
                }
            }
            catch
            {
            }
        }

    }
}