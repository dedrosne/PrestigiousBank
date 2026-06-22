using Newtonsoft.Json;
using PrestigiousBank;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

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
        {}

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.SettlementEntered));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickEvent);
            CampaignEvents.OnTroopRecruitedEvent.AddNonSerializedListener(this, new Action<Hero, Settlement, Hero, CharacterObject, int>(this.OnTroopRecruitedEvent));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTickEvent);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new NulnFactoryMenu().RegisterFactoryMenu(campaignGameStarter, NulnFactory);
        }
        

        private void DailyTickEvent()
        {
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Engineering, NulnFactory.GetDailySkillXP());
            //Production des matières premières
            NulnFactory.TryToProduceRessources();




        }

        public void OnTroopRecruitedEvent(Hero recruiter, Settlement settlement, Hero recruitmentSource, CharacterObject troop, int amount)
        {
            if (settlement!= null && settlement.IsTown && settlement.Town.StringId == townID)
                NulnFactory.ApplyTroopRecruited(recruiter, settlement, recruitmentSource, troop, amount, factorAgency: 1, townID);
        }

        public void HourlyTickEvent()
        {
            var time = Campaign.CurrentTime;

            if ((int)time % 24 == 13)
            {
                //Gestion du délai de changement de production
                if (NulnFactory.NbDaysLeftBetweenProductionChange > 0) NulnFactory.NbDaysLeftBetweenProductionChange -= 1;
                else NulnFactory.ConsumeRessourcesToRun();

                //Gestion des Workshops si production MachiningPart
                if (NulnFactory.ChosenProduction == NulnFactory.PossibleProduction.MachiningPart && NulnFactory.NbDaysLeftBetweenProductionChange == 0)//Check days left not necessary, but optimization ?
                {
                    Workshop[] workshops = NulnFactory.Ville.Town.Workshops;
                    if (workshops.Length != 0)
                    {
                        foreach (Workshop workshop in workshops)
                        {
                            //PrestigiousBank.LogMessage("Workshop profit :" + workshop.ProfitMade);
                            //PrestigiousBank.LogMessage("Workshop Capital :" + workshop.Capital);
                            //PrestigiousBank.LogMessage("Workshop Expense :" + workshop.Expense);
                            NulnFactory.ApplyWorkshopGains(workshop);
                        }
                    }
                }

                NulnFactory.PreviousDayBenefits = NulnFactory.Benefits;
                NulnFactory.Benefits = 0;
            }

            //Mortar
            if (NulnFactory.NbHoursRemainingToBuildMortar > 0)
            {
                NulnFactory.NbHoursRemainingToBuildMortar -= 1;
                if (NulnFactory.NbHoursRemainingToBuildMortar == 0)
                {
                    var mortar = MBObjectManager.Instance.GetObject<ItemObject>("tor_empire_artillery_mortar_001");
                    NulnFactory.GetItemStash().Add(new ItemRosterElement(mortar));
                    PrestigiousBank.LogMessage("Le mortier est terminé à Nuln");
                }
            }
            //Canon
            if (NulnFactory.NbHoursRemainingToBuildCanon > 0)
            {
                NulnFactory.NbHoursRemainingToBuildCanon -= 1;
                if (NulnFactory.NbHoursRemainingToBuildCanon == 0)
                {
                    var cannons = MBObjectManager.Instance.GetObject<ItemObject>("tor_empire_artillery_cannon_001");
                    NulnFactory.GetItemStash().Add(new ItemRosterElement(cannons));
                    PrestigiousBank.LogMessage("Le canon est terminé à Nuln");
                }
            }
            
        }

        public void SettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (hero != Hero.MainHero) return;
            if (settlement.IsTown) { PrestigiousBank.LogMessage("SettlementID : " + settlement.StringId);
                
            };
            if (settlement.IsVillage) PrestigiousBank.LogMessage("SettlementID : " + settlement.StringId);
            if (settlement.IsCastle) PrestigiousBank.LogMessage("SettlementID : " +settlement.StringId);
            if (settlement.OwnerClan != null && settlement.OwnerClan.Kingdom != null)
                PrestigiousBank.LogMessage("KingdomID : " + settlement.OwnerClan.Kingdom.StringId);
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