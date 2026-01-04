using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
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

        public static int HideoutLevelPrice = 50_000;
        public static int Racketeering_LevelPrice = 10_000;
        public static int Racketeering_MinimumValue = 20;
        public static int Racketeering_MaximumValue = 100;
        public static int Racketeering_BanditStrenghtNeededPerLevel = 10;

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

        public bool Racketeering_CalculateSuccess(ItemRoster roster)
        {
            return roster.TotalValue / (10 * Racketeering_Level) <= new Random().Next(0,100);
        }

        //Need to have verified before that the city is eligilable (ClanHideout or ClanAgency)
        public void Apply_Racketeering(MobileParty mobileParty, Settlement settlement, float agencyFactor = 1f)
        {
            if (mobileParty is null || !mobileParty.IsVillager) return;
            if (settlement == null || !settlement.IsTown) return;
            if (Racketeering_Level == 0) return;
            if (((int)(Racketeering_Level * Racketeering_BanditStrenghtNeededPerLevel * agencyFactor)) < BanditsGangStrenght) return;

            if (mobileParty.ItemRoster.Count != 0)
            {
                int maxRacketValue = (int)(Racketeering_CalculateMaxValueTaken()*agencyFactor);
                int remainingToRacket = maxRacketValue;
                ItemRoster rosterTmp = new ItemRoster();
                while (mobileParty.ItemRoster.Count != 0)
                {
                    var equipmentToRacket = mobileParty.ItemRoster[0].EquipmentElement;
                    if (equipmentToRacket.ItemValue > remainingToRacket)
                    //take equipment
                    {
                        remainingToRacket -= equipmentToRacket.ItemValue;
                        rosterTmp.AddToCounts(mobileParty.ItemRoster[0].EquipmentElement, 1);
                    }
                    else break;//Not enough value anymore for items to racket
                }

                if (rosterTmp.Count != 0)
                {
                    if (Racketeering_CalculateSuccess(rosterTmp)) //Case of success
                    {
                        GetItemStash().Add(rosterTmp);
                        foreach (var item in  rosterTmp)
                        {
                            mobileParty.ItemRoster.Remove(item);
                        }
                    }

                    else //Fail of racketeering.
                    {
                        BanditsGangStrenght -= Racketeering_Level * Racketeering_BanditStrenghtNeededPerLevel;
                        PrestigiousBank.LogMessage("Racket fail in " + settlement.GetName().Value);
                    }
                }

            }
        }
    }
}