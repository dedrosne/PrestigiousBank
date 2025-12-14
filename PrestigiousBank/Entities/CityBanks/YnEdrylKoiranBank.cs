//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999995)]
    public class YnEdrylKoiranBank : Bank
    {
        [SaveableProperty(2)]
        public int ForestHarmonyAccountSolde { get; set; }

        [SaveableProperty(3)]
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

    }
}