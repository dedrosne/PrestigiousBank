//using Birke.UI;
using PrestigiousBank;
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
    [SaveableRootClass(99999999)]
    public class Bank
    {
        [SaveableProperty(1)]
        public int Solde { get; set; }

        //[SaveableProperty(2)]
        //public int DaysSinceLastPayment { get; set; }

        //[SaveableProperty(3)]
        //public Settlement VisitedSettlement { get; set; }



        //[SaveableProperty(4)]
        //public List<string> CultureChangedSettlements { get; set; }

        //[SaveableProperty(5)]
        //public List<InsuredSettlement> InsuredSettlements { get; set; }

        //[SaveableProperty(6)]
        //public List<InsuredCaravan> InsuredCaravans { get; set; }

        //[SaveableProperty(7)]
        //public Dictionary<string, CultureObject> CultureChangedSettlementsDic { get; set; }

        public bool CanDoLoan { get; set; }

        public bool CanAccessMarket { get; set; }

        public Settlement _ville;

        public Settlement Ville
        {
            get { return _ville; }
            set
            {
                _ville = value;
            }
        }

        public Bank(Settlement ville)
        {
            Solde = 0;
            //DaysSinceLastPayment = 0;
            CanDoLoan = true;
            CanAccessMarket = true;
            Ville = ville;
            //InsuredCaravans = new List<InsuredCaravan>();
            //InsuredSettlements = new List<InsuredSettlement>();
            //CultureChangedSettlements = new List<string>();
            //CultureChangedSettlementsDic = new Dictionary<string, CultureObject>();
        }

        public float CalculateInterestRate()
        {
            if (Ville != null)
            {
                return Ville.Town.Prosperity * 0.0000002f;
            }
            else return 0f;
            //Prosperity between 0 and 10 000.
            //10 000 = 0.02% interest/day
        }

        public int CalculateInterests()
        {
            float interestRate = CalculateInterestRate();

            if (interestRate == 0f) return 0;

            return (int)(Solde * interestRate);
        }

        public int GetCustomerLevel()
        {
            if (Solde < 49_999) return 1;
            if (Solde < 199_999) return 2;
            if (Solde < 499_999) return 3;
            else return 4;
        }

        public String GetCustomerLevelString()
        {
            int level = GetCustomerLevel();
            if (level == 1) return "Bronze";
            if (level == 2) return "Argent";
            if (level == 3) return "Or";
            else return "Platine";
        }

        public int GetDailySkillXP()
        {
            //TradeXP = 1XP/100or pour tout or au dessus de 200_000. Max 5000/jour
            return Math.Min(Math.Max((Solde - 200_000) / 100, 0),5000);
        }

    }
}