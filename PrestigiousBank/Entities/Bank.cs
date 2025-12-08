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
            if (level == 1) return "Argent";
            if (level == 1) return "Or";
            else return "Platine";
        }

        public int GetDailySkillXP()
        {
            //TradeXP = 1XP/100or pour tout or au dessus de 200_000. Max 5000/jour
            return Math.Min(Math.Max((Solde - 200_000) / 100, 0),5000);
        }


        /*internal void RestoreCulture()
        {
            //Can be removed in next release. Same for CultureChangedSettlements
            var fiefs = Campaign.Current.Settlements.Where(x => (x.IsCastle || x.IsTown) && CultureChangedSettlements.Contains(x.Name.ToString())).ToList();
            fiefs.ForEach(x =>
            {
                x.Town.Settlement.Culture = Clan.PlayerClan.Culture;
                Support.ChangeNotablesCulture(x.Town.Settlement, Clan.PlayerClan.Culture);
                foreach (var village in x.Town.Villages)
                    Support.ChangeNotablesCulture(village.Settlement, Clan.PlayerClan.Culture);
            });
            /// TO HERE

            var fiefs2 = Campaign.Current.Settlements.Where(x => (x.IsCastle || x.IsTown) && CultureChangedSettlementsDic.ContainsKey(x.Name.ToString())).ToList();
            fiefs2.ForEach(x =>
            {
                var culture = CultureChangedSettlementsDic[x.Name.ToString()];
                x.Town.Settlement.Culture = culture;
                Support.ChangeNotablesCulture(x.Town.Settlement, culture);
                foreach (var village in x.Town.Villages)
                    Support.ChangeNotablesCulture(village.Settlement, culture);
            });
        }*/

        //public bool DepositConditions(string input)
        //{
        //    int amount;
        //    return int.TryParse(input, out amount) && DepositConditions(amount);
        //}

        //public bool DepositConditions(int amount)
        //{
        //    return amount > 0 && amount <= Hero.MainHero.Gold;
        //}

        //public void Deposit(string amount)
        //{
        //    var repayAmount = int.Parse(amount);

        //    Deposit(repayAmount);

        //    Campaign.Current.CurrentMenuContext.Refresh();
        //}

        //public void Deposit(int amount)
        //{
        //    Hero.MainHero.ChangeHeroGold(amount * -1);
        //    if (Math.Abs(amount / (double)(Solde == 0 ? 1 : Solde)) >= 0.10)
        //    {
        //        DaysSinceLastPayment = 0;
        //        SetBankFlags();
        //    }

        //    var newSaldo = ((double)Solde) + amount;
        //    if (newSaldo < int.MaxValue && newSaldo > int.MinValue)
        //        Solde += amount;

        //    AltdorfBankCampaignBehavior.UpdateTraitLevel();
        //}




        /*        internal bool IsSettlementInsured(Settlement settlement)
                {
                    if (settlement == null || !settlement.IsVillage)
                        return false;

                    return InsuredSettlements.Any(x => x.SettlementId == settlement.Village.Id.InternalValue);
                }

                internal InsuredSettlement GetInsuredSettlement(Settlement settlement)
                {
                    if (settlement == null || !settlement.IsVillage)
                        return null;

                    return InsuredSettlements.FirstOrDefault(x => x.SettlementId == settlement.Village.Id.InternalValue);
                }

                internal void CancelFiefInsurances(IEnumerable<Village> villages)
                {
                    foreach (var village in villages)
                    {
                        CancelSettlementInsurance(village);
                    }
                }*/

        /*        internal void CancelSettlementInsurance(Village village)
                {
                    var insuredSettlement = InsuredSettlements.FirstOrDefault(x => x.SettlementId == village.Id.InternalValue);

                    if (insuredSettlement != null)
                        InsuredSettlements.Remove(insuredSettlement);
                }*/



        /*public void BalanceWeakestNation()
        {
            var amount = BirkeCityBank.Config.BalanceWeakestNationAmount;
            var kingdoms = Campaign.Current.Kingdoms.Where(x => x.IsMapFaction && !x.IsMinorFaction);
            var weakestKingdom = kingdoms.OrderByDescending(x => x.Settlements.Count()).FirstOrDefault();

            if (weakestKingdom != null)
            {
                var nobels = weakestKingdom.AliveLords.Where(n => n.Gold < BirkeCityBank.Config.BalanceNobelGoldLimit).OrderBy(x => x.Gold);

                if (nobels.Count() > 0)
                {
                    var nobelAmount = BirkeCityBank.Config.BalanceWeakestNationAmount / nobels.Count();
                    var rest = BirkeCityBank.Config.BalanceWeakestNationAmount % nobels.Count();

                    nobels.ToList().ForEach(n => n.ChangeHeroGold(nobelAmount));

                    if (rest != 0)
                        nobels.FirstOrDefault()?.ChangeHeroGold(rest);
                }
            }
        }*/
        /*        public string ToGoldString(int amount)
                {
                    return $"{amount.ToString("#,#", CultureInfo.InvariantCulture)} {GameTexts.FindText("str_bankCoions", null)}";
                }

                public string ToYesNoString(bool boolValue)
                {
                    if (boolValue)
                    {
                        return $"{GameTexts.FindText("str_boolYes", null)}";
                    }
                    else
                    {
                        return $"{GameTexts.FindText("str_boolNo", null)}";
                    }
                }

                public string ToDistanceString(string distance)
                {
                    return $"{distance} {GameTexts.FindText("str_bankDistance", null)}";
                }

                public string ToDistanceString(int distance)
                {
                    return $"{distance.ToString()} {GameTexts.FindText("str_bankDistance", null)}";
                }

            }*/
    }
}