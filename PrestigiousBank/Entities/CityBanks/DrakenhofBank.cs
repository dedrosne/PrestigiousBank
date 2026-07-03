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
    public class DrakenhofBank : Bank
    {
        public static int PricePerAttributePoint = 30_000;
        public static int PricePerFocusPoint = 25_000;
        public static int PricePerCompanionAttributePoint = 15_000;
        public static int PricePerCompanionFocusPoint = 10_000;

        [SaveableProperty(12)]
        public int DarkEnergyAccountSolde { get; set; }

        [SaveableProperty(13)]
        public int AttributePointBought { get; set; }

        [SaveableProperty(14)]
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