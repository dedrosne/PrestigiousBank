//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class YnEdrylKoiranBank : Bank
    {
        [SaveableProperty(11)]
        public int ForestHarmonyAccountSolde { get; set; }

        [SaveableProperty(12)]
        public int BlessingAmount { get; set; }

        public static int PricePerBlessing = 3000;


        public YnEdrylKoiranBank(Settlement ville) : base(ville)
        {
            ForestHarmonyAccountSolde = 0;
            BlessingAmount = 0;
        }

        public int CalculateForestHarmonyInterests()
        {
           return (int)(ForestHarmonyAccountSolde * (1f/10000f));
        }

        public int CalculateBlessingUpkeep()
        {
            return BlessingAmount * BlessingAmount;
        }

        protected override void InitMercenariesUnits()
        {
            InitMercenariesUnitFromListString(new List<string> { 
                "tor_we_treeman", 
                "tor_we_dryad",
                "tor_we_waywatcher_sentinel", 
                "tor_we_wildwood_warden", 
                "tor_we_eternal_warden",
                "tor_we_ancient_treeman",
                "tor_we_great_stag_knight" });
            SortAndCleanMercenaryUnitList();
        }
    }
}