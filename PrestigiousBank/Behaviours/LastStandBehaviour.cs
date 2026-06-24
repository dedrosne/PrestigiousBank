using Helpers;
using Newtonsoft.Json;
using PrestigiousBank;
using PrestigiousBank.Entities;
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
using TaleWorlds.CampaignSystem.Party.PartyComponents;
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
using TOR_Core.Utilities;

namespace PrestigiousBank
{
    public class LastStandCampaignBehavior : CampaignBehaviorBase
    {
        private static LastStands _lastStands;
        public static LastStands LastStands
        {
            get
            {
                if (_lastStands == null)
                {
                    _lastStands = new LastStands();
                }

                return _lastStands;

            }
            set
            {
                _lastStands = value;
            }
        }

        public LastStandCampaignBehavior() : base()
        {
            //MBTextManager.SetTextVariable("Birke_Bank_Encyclopedia_Main", PrestigiousBank.Config.BankName);
            //bankAltdorf = bankAltdorf;

            //_bankTrait = Game.Current.ObjectManager.RegisterPresumedObject<TraitObject>(new TraitObject("bank"));
            //_bankTrait.Initialize(new TextObject(GameTexts.FindText("str_trait_bankName").ToString()), new TextObject(GameTexts.FindText("str_trait_bankDescription").ToString()), false, 0, 4);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(this.OnSettlementOwnerChanged));
        }

        public void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByRebellion &&
                detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege) return;

            var oldCulture = oldOwner?.Culture.ToString();
            PrestigiousBank.LogMessage("settlement owner changed, oldCulture: " + oldCulture + ", newOwner: " + newOwner?.Name.ToString() + ", settlement: " + settlement?.Name.ToString());
            //LastStand desactivation check
            //If it is the second city captured by a faction in lastStand, then disable lastStand for this culture, as it is not the last city of this culture
            if (LastStands.GetLastStandForCulture(settlement.Culture.ToString())?.IsConsumed == true)
            {
                //If it is its second city
                foreach (Town town in Town.AllTowns)
                {
                    if (settlement.Culture.ToString() == town.Culture.ToString() && settlement.Town.StringId != town.StringId)
                    {
                        LastStands.GetLastStandForCulture(settlement.Culture.ToString()).IsConsumed = false;
                        LastStands.GetLastStandForCulture(settlement.Culture.ToString()).Strength = 0;  
                    }
                }
            }

            //Laststand activation check
            //Check if the culture of the old owner has still a city, if it is the case, then we can return without triggering last stand, as it is not the last city of this culture
            foreach (Town town in Town.AllTowns)
            {
                if (town.OwnerClan != Clan.PlayerClan && oldCulture == town.Culture.ToString())
                {
                    return;
                }
            }
            PrestigiousBank.LogMessage("LastStand triggered for culture: " + oldCulture + ", settlement: " + settlement?.Name.ToString() + ", newOwner: " + newOwner?.Name.ToString());
            //If you get there, it means that the culture of the old owner has no more town, and thus the last stand is triggered for this culture
            if (oldCulture != null)
            {
                var lastStand = LastStands.GetLastStandForCulture(oldCulture);
                if (lastStand != null)
                {
                    lastStand.IsConsumed = true;
                    lastStand.Strength = 0;
                }
                TriggerLastStand(oldCulture, settlement, newOwner.Clan.Kingdom, oldOwner);
            }
        }

        private void TriggerLastStand(string culture, Settlement settlement, Kingdom kingdomToDeclareWar, Hero oldOwner)
        {
            PrestigiousBank.LogMessage("TriggerLastStand function");
            //Goal : Give to this culture few armies to it has a last chance to fight back.
            var clan = Clan.All.FirstOrDefault(c => c.StringId == oldOwner.Clan.StringId);

            foreach (WarPartyComponent comp in clan.WarPartyComponents)
            {
                var mobileParty = comp.MobileParty;
                var relocationPosition = NavigationHelper.FindReachablePointAroundPosition(
                    settlement.GatePosition,
                    mobileParty.NavigationCapability,
                    8f,
                    1f);

                mobileParty.Position = relocationPosition;
                comp.Party.SetVisualAsDirty();

                MobileParty.CreateParty(culture.ToString() + "_lastStand", comp);
            }
            
            
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
        }


        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<LastStands>("LastStands", ref _lastStands);
                }
                else
                {
                    dataStore.SyncData<LastStands>("LastStands", ref _lastStands);
                }
            }
            catch
            {
            }
        }

    }
}