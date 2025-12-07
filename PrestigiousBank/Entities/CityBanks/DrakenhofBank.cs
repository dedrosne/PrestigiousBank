//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999997)]
    public class DrakenhofBank : Bank
    {
        [SaveableProperty(2)]
        public int DarkEnergyAccountSolde { get; set; }

        public DrakenhofBank(Settlement ville) : base(ville)
        {
            DarkEnergyAccountSolde = 0;
        }

        public int CalculateDarkEnergyInterests()
        {
           return (int)(DarkEnergyAccountSolde * (1f/10000f));
        }

    }
}