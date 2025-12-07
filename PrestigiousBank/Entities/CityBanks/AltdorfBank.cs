//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999998)]
    public class AltdorfBank : Bank
    {
        [SaveableProperty(2)]
        public int prestigiousAccountSolde { get; set; }

        public AltdorfBank(Settlement ville) : base(ville)
        {
            prestigiousAccountSolde = 0;
        }

        public int CalculatePrestigiousInterests()
        {
           return (int)(prestigiousAccountSolde * (1f/10000f));
        }

    }
}