//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class TorLithanelBank : Bank
    {

        public TorLithanelBank(Settlement ville) : base(ville)
        {

        }


        protected override void InitMercenariesUnits()
        {
            InitMercenariesUnitFromListString(new List<string> { 
                "tor_eo_queens_guard_halberd",
                "tor_eo_queens_guard_sword",
                "tor_eo_envoy_guard" });

            //Eonir Tor Lithanel Guar, Enonir Cityborn Militia, White lion of chrace, see elf sentinel
            SortAndCleanMercenaryUnitList();
        }
    }
}