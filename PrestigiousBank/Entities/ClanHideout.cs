using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999989)]
    public class ClanHideout
    {
        [SaveableProperty(1)]
        public string TownID { get; set; }

        [SaveableProperty(2)]
        public int LevelHideout { get; set; }

        [SaveableProperty(3)]
        public int SelectedLevelHideout { get; set; }

        [SaveableProperty(4)]
        public int BanditsGangStrenght { get; set; }

        [SaveableProperty(5)]
        public int Racketeering_Level { get; set; }

        [SaveableProperty(6)]
        private ItemRoster ItemStash { get; set; }

        [SaveableProperty(7)]
        public bool IsSecretEntranceUnlocked { get; set; }

        [SaveableProperty(8)]
        public int Casino_Level { get; set; }

        [SaveableProperty(9)]
        public int Casino_IncreaseBought { get; set; }

        [SaveableProperty(10)]
        public int Casino_Benefits { get; set; }

        public static int HideoutLevelPrice = 50_000;
        public static float GangStrenghtUpkeep = 0.1f;
        public static int Racketeering_LevelPrice = 10_000;
        public static int Racketeering_MinimumValue = 20;
        public static int Racketeering_MaximumValue = 100;
        public static int Racketeering_BanditStrenghtNeededPerLevel = 10;

        public static int Casino_LevelPrice = 40_000;
        public static int Casino_MinimumValue = -2000;
        public static int Casino_MaximumValue = 2400;
        public static int Casino_IncreaseCost = 1000;
        public static int Casino_IncreaseMinValue = -20;
        public static int Casino_IncreaseMaxValue = 24;

        public static int Hideout_CostPerUnitRecruitTier = 10;
        public static int Hideout_UnblockSecretEntrancePrice = 30_000;

        public int CasinoMinValue {
            get { return Casino_Level * Casino_MinimumValue + Casino_IncreaseBought * Casino_IncreaseMinValue; }
        }

        public int CasinoMaxValue
        {
            get { return Casino_Level * Casino_MaximumValue + Casino_IncreaseBought * Casino_IncreaseMaxValue; }
        }

        private Town _town;

        public Town Town
        {
            get
            {
                if (_town == null)
                {
                    foreach (Town town in Town.AllTowns)
                    {
                        if (town.StringId == TownID) { _town = town; break; }
                    }
                }
                return _town;
            }
            set { _town = value; }
        }



        public ItemRoster GetItemStash()
        {
            if (ItemStash == null) ItemStash = new ItemRoster();
            return ItemStash;
        }

        public ClanHideout(string townID)
        {
            TownID = townID;
        }

        public int CalculatePriceToLevelUpHideout()
        {
            return (LevelHideout + 1) * HideoutLevelPrice;
        }


        public int GetDailySkillXP()
        {
            return 100 * LevelHideout;
        }

        public int Racketeering_CalculateMaxValueTaken()
        {
            int random = new Random().Next(Racketeering_MinimumValue, Racketeering_MaximumValue);
            return random * Racketeering_Level;
        }

        public bool Racketeering_CalculateSuccess(ItemRoster roster, float agencyFactor)
        {
            float random = new Random().Next(0, 100);
            float val = ((float)(roster.TotalValue) / (float)(Racketeering_Level) / (Racketeering_MaximumValue * agencyFactor)  ) * 30f;
            return val <= random;
        }

        //Need to have verified before that the city is eligilable (ClanHideout or ClanAgency)
        public void Apply_Racketeering(MobileParty mobileParty, Settlement settlement, float agencyFactor = 1f)
        {
            if (mobileParty is null || !mobileParty.IsVillager) return;
            if (settlement == null || !settlement.IsTown) return;
            if (Racketeering_Level == 0) return;
            if (((int)(Racketeering_Level * Racketeering_BanditStrenghtNeededPerLevel * agencyFactor)) > BanditsGangStrenght) return;

            if (mobileParty.ItemRoster.Count != 0)
            {
                int maxRacketValue = (int)(Racketeering_CalculateMaxValueTaken()*agencyFactor);
                int remainingToRacket = maxRacketValue;
                ItemRoster rosterTakenTmp = new ItemRoster();


                //Copy party roster to tmp
                ItemRoster mobilePartyRosterTmp = new ItemRoster();
                foreach (var item in mobileParty.ItemRoster)
                {
                    if (item.EquipmentElement.Item.StringId != "sumpter_horse")
                    {
                        mobilePartyRosterTmp.Add(item);
                    }
                }

                //try to steal
                while (mobilePartyRosterTmp.Count != 0)
                {
                    var equipmentToRacket = mobilePartyRosterTmp[0].EquipmentElement;

                    if (equipmentToRacket.ItemValue <= remainingToRacket)
                    //take equipment
                    {
                        remainingToRacket -= equipmentToRacket.ItemValue;
                        rosterTakenTmp.AddToCounts(equipmentToRacket, 1);
                        mobileParty.ItemRoster.AddToCounts(equipmentToRacket, -1);
                    }
                    else break;//Not enough value anymore for items to racket
                }

                if (rosterTakenTmp.Count != 0)
                {
                    if (Racketeering_CalculateSuccess(rosterTakenTmp, agencyFactor)) //Case of success
                    {
                        GetItemStash().Add(rosterTakenTmp);

                    }

                    else //Fail of racketeering.
                    {
                        BanditsGangStrenght -=(int) ( Racketeering_Level * Racketeering_BanditStrenghtNeededPerLevel*agencyFactor);
                        PrestigiousBank.LogMessage("Racket fail in " + settlement.GetName().Value);
                        //Restitue Items to Mobile Party
                        foreach (var item in rosterTakenTmp)
                        {
                            mobileParty.ItemRoster.Add(item);
                        }
                    }
                }

            }
        }

        public static int CalculateHideoutRecruitValue()
        {
            int costValue = 0;
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout)
            {


                List<PartyBase> partiesGetDefenderParties = Settlement.CurrentSettlement.Hideout.GetDefenderParties(MapEvent.BattleTypes.Hideout).Where(party => party.NumberOfAllMembers != 0).ToList();

                foreach (PartyBase partyBase in partiesGetDefenderParties)
                {
                    if (partyBase.MemberRoster != null)
                    {
                        foreach (TroopRosterElement troop in partyBase.MemberRoster.GetTroopRoster()) costValue += troop.Character.Tier * troop.Number * 10;
                    }
                }
            }

            return costValue;
        }

        
    }
}