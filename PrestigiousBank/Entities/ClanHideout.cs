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
    [SaveableRootClass(99999989)]
    public class ClanHideout
    {
        [SaveableProperty(1)]
        public string TownID {get;set;}

        [SaveableProperty(2)]
        public int LevelHideout {get;set;}

        [SaveableProperty(3)]
        public int SelectedLevelHideout { get;set;}
        
        [SaveableProperty(4)]
        public int BanditsGangStrenght { get; set; }


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

        public static int HideoutInitialPrice = 20_000;


        public ClanHideout(string townID)
        {
            TownID = townID;
        }

        public int CalculatePriceToLevelUpHideout()
        {
            return (LevelHideout + 1) * HideoutInitialPrice;
        }


        public int GetDailySkillXP()
        {
            return 1000 * LevelHideout;
        }
    }
}