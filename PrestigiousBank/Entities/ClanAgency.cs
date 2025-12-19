using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999991)]
    public class ClanAgency
    {
        [SaveableProperty(1)]
        public string TownID {get;set;}

        [SaveableProperty(2)]
        public int LevelAgency {get;set;}

        [SaveableProperty(3)]
        public int SelectedLevel {get;set;}

        private Town _town;

        public Town Town { 
            get { 
                if (_town == null)
                {
                    foreach(Town town in Town.AllTowns)
                    {
                        if (town.StringId == TownID) { _town = town; break; }
                    }
                } 
                return _town;
            }
            set { _town = value; } }

        public static int AgencyInitialPrice = 10_000;

        public static int AgencyUpkeepPerLevel = 20;

        public ClanAgency(string townID)
        {
            TownID = townID;
        }

        public int CalculatePriceToLevelUpAgency()
        {
            return (LevelAgency + 1) * AgencyInitialPrice;
        }
    }
}