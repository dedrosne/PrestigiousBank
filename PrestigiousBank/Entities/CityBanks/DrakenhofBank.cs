//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;
using TOR_Core.BattleMechanics.StatusEffect;

namespace PrestigiousBank
{
    [SaveableRootClass(99999997)]
    public class DrakenhofBank : Bank
    {
        public static int PricePerAttributePoint = 60_000;
        public static int PricePerFocusPoint = 50_000;
        public static int PricePerCompanionAttributePoint = 30_000;
        public static int PricePerCompanionFocusPoint = 25_000;

        [SaveableProperty(2)]
        public int DarkEnergyAccountSolde { get; set; }

        [SaveableProperty(3)]
        public int AttributePointBought { get; set; }

        [SaveableProperty(4)]
        public int FocusPointBought { get; set; }

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