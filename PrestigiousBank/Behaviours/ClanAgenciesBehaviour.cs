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
using TaleWorlds.CampaignSystem.Settlements.Workshops;
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
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using TaleWorlds.CampaignSystem.GameComponents;

namespace PrestigiousBank
{
    public class ClanAgenciesBehaviour : CampaignBehaviorBase
    {

        private static ClanAgencies _agencies = null;

        public static ClanAgencies ClanAgencies
        {
            get
            {
                if (_agencies == null) _agencies = new ClanAgencies();
                return _agencies;

            }
            set
            {
                _agencies = value;
            }
        }

        public ClanAgenciesBehaviour() : base()
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
            new ClanAgencyMenu().RegisterClanAgencyMenu(campaignGameStarter);
        }
        

        private void DailyTickEvent()
        {
            ////Production des matières premières
            //NulnFactory.TryToProduceRessources();




        }

        public void OnTroopRecruitedEvent(Hero recruiter, Settlement settlement, Hero recruitmentSource, CharacterObject troop, int amount)
        {
            if (settlement!= null && settlement.IsTown && 
                settlement.Town.StringId != NulnFactoryCampaignBehavior.townID &&//Dont re-apply event catched in NulnFactoryBehaviour
                NulnFactoryCampaignBehavior.NulnFactory.NbDaysLeftBetweenProductionChange == 0)
            {
                ClanAgency agency = ClanAgencies.GetAgencyByTownStringId(settlement.Town.StringId);
                if (agency != null)
                {
                    NulnFactoryCampaignBehavior.NulnFactory.ApplyTroopRecruited(recruiter, settlement, recruitmentSource, troop, amount, agency.LevelAgency * ClanAgency.AgencyProductionFactorPerLevel);
                }
            }

        }

        public void HourlyTickEvent()
        {
            var time = Campaign.CurrentTime;

            if ((int)time % 24 == 13)
            {

                //Gestion des Workshops si production MachiningPart
                if (NulnFactoryCampaignBehavior.NulnFactory.chosenProduction == NulnFactory.PossibleProduction.MachiningPart 
                    && NulnFactoryCampaignBehavior.NulnFactory.NbDaysLeftBetweenProductionChange == 0) //Check days left not necessary, but optimization ?
                {
                    if (ClanAgencies.GetClanAgenciesList().Count > 0) 
                    {
                        foreach (var agency in ClanAgencies.GetClanAgenciesList())
                        {

                            Workshop[] workshops = agency.Town.Workshops;
                            if (agency.TownID != NulnFactoryCampaignBehavior.townID && workshops.Length != 0)//Dont re-apply event catched in NulnFactoryBehaviour
                            {
                                foreach (Workshop workshop in workshops)
                                {
                                    //PrestigiousBank.LogMessage("Workshop profit :" + workshop.ProfitMade);
                                    //PrestigiousBank.LogMessage("Workshop Capital :" + workshop.Capital);
                                    //PrestigiousBank.LogMessage("Workshop Expense :" + workshop.Expense);
                                    NulnFactoryCampaignBehavior.NulnFactory.ApplyWorkshopGains(workshop, agency.LevelAgency * ClanAgency.AgencyProductionFactorPerLevel);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (settlement != null && settlement.IsTown && hero == Hero.MainHero)
                ClanAgencies.RefreshCurrentSettlementAgency();


            //Clan Hideout Racketeering
            
            if (settlement != null && settlement.IsTown && mobileParty != null  && mobileParty.IsVillager && settlement.Town.StringId != ClanHideoutCampaignBehavior.ClanHideout.TownID )
            {
                ClanAgency agency = ClanAgencies.GetAgencyByTownStringId(settlement.Town.StringId);
                if (agency != null)
                {
                    float factor = agency.LevelAgency * ClanAgency.AgencyProductionFactorPerLevel;
                    ClanHideoutCampaignBehavior.ClanHideout.Apply_Racketeering(mobileParty, settlement, factor);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<ClanAgencies>("ClanAgencies", ref _agencies);
                }
                else
                {
                    dataStore.SyncData<ClanAgencies>("ClanAgencies", ref _agencies);
                }
            }
            catch
            {
            }
        }

    }
}