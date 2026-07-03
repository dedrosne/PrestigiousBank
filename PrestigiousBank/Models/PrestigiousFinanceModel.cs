using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.LordConversationsCampaignBehavior;


namespace PrestigiousBank
{
    public class PrestigiousFinanceModel: ClanFinanceModel
    {
        ClanFinanceModel _previousModel;

        public override int PartyGoldLowerThreshold => _previousModel.PartyGoldLowerThreshold;

        public PrestigiousFinanceModel(ClanFinanceModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultClanFinanceModel();
        }

        // =========================================================
        // Detailed Income (Expected Gold Change)
        // =========================================================
        public override ExplainedNumber CalculateClanIncome(
            Clan clan,
            bool includeDescriptions = false,
            bool applyWithdrawals = false,
            bool includeDetails = false)
        {
            ExplainedNumber result = _previousModel.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);

            if (clan.StringId == "player_faction")
            {
                AddBankInterestToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
            }
            //Last Stand : Infinite number of gold
            else if (Campaign.Current?.GetCampaignBehavior<LastStandCampaignBehavior>() != null
                && LastStandCampaignBehavior.LastStands.GetLastStandForCulture(clan.Culture.StringId).IsConsumed)
            {
                result.Add(100_000, new TextObject("Dernier rempart de " + clan.Culture.StringId));
            }

            return result;
        }

        public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
        {
            ExplainedNumber result = _previousModel.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals, includeDetails);

            if (clan.StringId == "player_faction")
            {
                AddLoanRefoundToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
            }


            return result;
        }

        // =========================================================
        // Consolidated result (Summary of Expected Gold Change)
        // =========================================================
        public override ExplainedNumber CalculateClanGoldChange(
            Clan clan,
            bool includeDescriptions = true,
            bool applyWithdrawals = false,
            bool includeDetails = false)
        {
            ExplainedNumber result = _previousModel.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);

            try
            {
                AddBankInterestToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
                AddLoanRefoundToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
            }
            catch (Exception ex)
            {
#if DEBUG
                PrestigiousBank.LogMessage($"[PrestigiousBank][FinanceModel] Error calculating expected gold change: {ex.Message}");
#endif
            }
            return result;
        }

        private static void AddBankInterestToExplainedNumber(Clan clan, ref ExplainedNumber result, bool includeDescriptions, bool includeDetails)
        {
            if (clan == null || clan.Leader == null || clan != Clan.PlayerClan)
                return;

            var hero = Hero.MainHero;
            if (hero == null || string.IsNullOrEmpty(hero.StringId))
                return;

            ExplainedNumber goldChange = new ExplainedNumber(0f, includeDetails, null);

            //Altdorf
            AltdorfBankCampaignBehavior AltdorfBankBehavior = Campaign.Current?.GetCampaignBehavior<AltdorfBankCampaignBehavior>();
            if (AltdorfBankBehavior != null)
            {
                int InterestsAltdorfBank = AltdorfBankCampaignBehavior.BankInstance.CalculateInterests();
                
                if (InterestsAltdorfBank != 0)
                    goldChange.Add(InterestsAltdorfBank, new TextObject("Banque d'Altdorf"));

                if (AltdorfBankCampaignBehavior.BankInstance.ChanelerNumber != 0)
                {
                    result.Add(-AltdorfBankCampaignBehavior.BankInstance.CalculateChannelerCostPerDay(), new TextObject("Canalysateurs d'Altdorf"));
                }
            }

            //Drakenhof
            DrakenhofBankCampaignBehavior DrakenhofBankBehavior = Campaign.Current?.GetCampaignBehavior<DrakenhofBankCampaignBehavior>();

            if (DrakenhofBankBehavior != null)
            {

                int InterestsDrakenhoffBank = DrakenhofBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsDrakenhoffBank != 0)
                    goldChange.Add(InterestsDrakenhoffBank, new TextObject("Banque de Drakenhof"));
            }

            //YnEdrylKoiran
            YnEdrylKoiranBankCampaignBehavior ynEdrylKoiranBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<YnEdrylKoiranBankCampaignBehavior>();

            if (ynEdrylKoiranBankCampaignBehavior != null)
            {
                int InterestsYnEdrylKoiran = YnEdrylKoiranBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsYnEdrylKoiran != 0)
                    goldChange.Add(InterestsYnEdrylKoiran, new TextObject("Banque d'Yn Edryl Koiran"));

                int IshaBlessingUpkeep = YnEdrylKoiranBankCampaignBehavior.BankInstance.CalculateBlessingUpkeep();
                if (IshaBlessingUpkeep != 0) result.Add(-IshaBlessingUpkeep, new TextObject("Bénédiction d'Isha"));
            }


            //Couronne
            CouronneBankCampaignBehavior CouronneBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<CouronneBankCampaignBehavior>();

            if (CouronneBankCampaignBehavior != null)
            {
                int InterestsCouronne = CouronneBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsCouronne != 0)
                    goldChange.Add(InterestsCouronne, new TextObject("Banque de Couronne"));
            }

            //Averheim
            AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();

            if (AverheimBankCampaignBehavior != null)
            {
                int InterestsAverheim = AverheimBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsAverheim != 0)
                    goldChange.Add(InterestsAverheim, new TextObject("Banque d'Averheim"));
            }

            //Middenheim
            MiddenheimBankCampaignBehavior MiddenheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<MiddenheimBankCampaignBehavior>();
            if (MiddenheimBankCampaignBehavior != null)
            {
                int InterestsMiddenheim = MiddenheimBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsMiddenheim != 0)
                    goldChange.Add(InterestsMiddenheim, new TextObject("Banque de Middenheim"));

                int PartyHelperUpkeep = MiddenheimBankCampaignBehavior.BankInstance.CalculatePartyHelperUpkeep();
                if (PartyHelperUpkeep != 0) result.Add(-PartyHelperUpkeep, new TextObject("Aides de camp de Middenheim"));
            }


            //Parravon
            ParravonBankCampaignBehavior ParravonBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<ParravonBankCampaignBehavior>();
            if (ParravonBankCampaignBehavior != null)
            {
                int InterestsParravon = ParravonBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsParravon != 0)
                    goldChange.Add(InterestsParravon, new TextObject("Banque de Parravon"));
            }


            //Tor Lithanel
            TorLithanelBankCampaignBehavior torLithanelBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<TorLithanelBankCampaignBehavior>();
            if (torLithanelBankCampaignBehavior != null)
            {
                int InterestsTorLithanel = TorLithanelBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsTorLithanel != 0)
                    goldChange.Add(InterestsTorLithanel, new TextObject("Banque de Tor Lithanel"));
            }

            //Karak Izor
            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();
            if (KarakIzorBankCampaignBehavior != null)
            {

                int InterestsKarakIzor = KarakIzorBankCampaignBehavior.BankInstance.CalculateInterests();
                if (InterestsKarakIzor != 0)
                    goldChange.Add(InterestsKarakIzor, new TextObject("Banque de Karak Izor"));
            }


            if (!includeDetails)
            {
                result.Add(goldChange.ResultNumber, new TextObject("Bank Interests"));
            }
            else
            {
                result.AddFromExplainedNumber(goldChange, new TextObject("Bank Interests"));
            }


            //Nuln Factory
            NulnFactoryCampaignBehavior NulnFactoryCampaignBehavior = Campaign.Current?.GetCampaignBehavior<NulnFactoryCampaignBehavior>();
            if (NulnFactoryCampaignBehavior != null)
            {
                if (NulnFactoryCampaignBehavior.NulnFactory.PreviousDayBenefits != 0)
                {
                    result.Add(NulnFactoryCampaignBehavior.NulnFactory.PreviousDayBenefits, new TextObject("Usine de Nuln"));
                }
            }

            //Clan Agencies Upkeep
            ClanAgenciesBehaviour ClanAgenciesBehaviour = Campaign.Current?.GetCampaignBehavior<ClanAgenciesBehaviour>();
            if (ClanAgenciesBehaviour != null)
            {
                if (ClanAgenciesBehaviour.ClanAgencies.GetClanAgenciesList().Count > 0)
                {
                    int agencyUpkeep = 0;
                    foreach (ClanAgency agency in ClanAgenciesBehaviour.ClanAgencies.GetClanAgenciesList())
                    {
                        agencyUpkeep += agency.LevelAgency * ClanAgency.AgencyUpkeepPerLevel;
                    }
                    result.Add(-agencyUpkeep, new TextObject("Entretien des Agences"));
                }
            }

            //Clan Hideout Gangstrenght Upkeep
            ClanHideoutCampaignBehavior ClanHideoutCampaignBehavior = Campaign.Current?.GetCampaignBehavior<ClanHideoutCampaignBehavior>();
            if (ClanHideoutCampaignBehavior != null)
            {
                if (ClanHideoutCampaignBehavior.ClanHideout.BanditsGangStrenght > 0)
                {
                    int value = (int)(ClanHideoutCampaignBehavior.ClanHideout.BanditsGangStrenght * ClanHideout.GangStrenghtUpkeep);
                    result.Add(-value, new TextObject("Partage du butin de la planque"));
                }

                //Clan Hideout Casino
                if (ClanHideoutCampaignBehavior.ClanHideout.Casino_Level > 0)
                {
                    int value = (int)ClanHideoutCampaignBehavior.ClanHideout.Casino_PreviousBenefits;
                    result.Add(value, new TextObject("Casino"));
                }
            }


        }


        private void AddLoanRefoundToExplainedNumber(Clan clan, ref ExplainedNumber result, bool includeDescriptions, bool includeDetails)
        {
            ExplainedNumber goldChange = new ExplainedNumber(0f, includeDescriptions, null);
            //Altdorf
            AltdorfBankCampaignBehavior AltdorfBankBehavior = Campaign.Current?.GetCampaignBehavior<AltdorfBankCampaignBehavior>();
            if (AltdorfBankBehavior != null)
            {
                if (AltdorfBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-AltdorfBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Altdorf loan refound"));
            }

            //Drakenhof
            DrakenhofBankCampaignBehavior DrakenhofBankBehavior = Campaign.Current?.GetCampaignBehavior<DrakenhofBankCampaignBehavior>();
            if (DrakenhofBankBehavior != null)
            {
                if (DrakenhofBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-DrakenhofBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Drakenhof loan refound"));
            }

            //YnEdrylKoiran
            YnEdrylKoiranBankCampaignBehavior ynEdrylKoiranBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<YnEdrylKoiranBankCampaignBehavior>();
            if (ynEdrylKoiranBankCampaignBehavior != null)
            {
                if (YnEdrylKoiranBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-YnEdrylKoiranBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Yn Edryl Koiran loan refound"));
            }


            //Couronne
            CouronneBankCampaignBehavior CouronneBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<CouronneBankCampaignBehavior>();
            if (CouronneBankCampaignBehavior != null)
            {
                if (CouronneBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-CouronneBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Couronne loan refound"));
            }

            //Averheim
            AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();
            if (AverheimBankCampaignBehavior != null)
            {
                if (AverheimBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-AverheimBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Averheim loan refound"));
            }

            //Middenheim
            MiddenheimBankCampaignBehavior MiddenheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<MiddenheimBankCampaignBehavior>();
            if (MiddenheimBankCampaignBehavior != null)
            {
                if (MiddenheimBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-MiddenheimBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Middenheim loan refound"));
            }


            //Parravon
            ParravonBankCampaignBehavior ParravonBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<ParravonBankCampaignBehavior>();
            if (ParravonBankCampaignBehavior != null)
            {
                if (ParravonBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-ParravonBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Parravon loan refound"));
            }


            //Tor Lithanel
            TorLithanelBankCampaignBehavior torLithanelBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<TorLithanelBankCampaignBehavior>();
            if (torLithanelBankCampaignBehavior != null)
            {
                if (TorLithanelBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-TorLithanelBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Tor Lithanel loan refound"));
            }

            //Karak Izor
            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();
            if (KarakIzorBankCampaignBehavior != null)
            {
                if (KarakIzorBankCampaignBehavior.BankInstance.LoanAmount > 0)
                    goldChange.Add(-KarakIzorBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Karak Izor loan refound"));
            }

            if (!includeDetails)
            {
                result.Add(goldChange.ResultNumber, new TextObject("Loan Refounds"));
            }
            else
            {
                result.AddFromExplainedNumber(goldChange, new TextObject("Loan Refounds"));
            }
        }












        public override ExplainedNumber CalculateTownIncomeFromTariffs(Clan clan, Town town, bool applyWithdrawals = false)
        {
            return _previousModel.CalculateTownIncomeFromTariffs(clan, town, applyWithdrawals);
        }

        public override int CalculateTownIncomeFromProjects(Town town)
        {
            return _previousModel.CalculateTownIncomeFromProjects(town);
        }

        public override int CalculateNotableDailyGoldChange(Hero hero, bool applyWithdrawals)
        {
            return _previousModel.CalculateNotableDailyGoldChange(hero, applyWithdrawals);
        }

        public override int CalculateVillageIncome(Clan clan, Village village, bool applyWithdrawals = false)
        {
            return _previousModel.CalculateVillageIncome(clan, village, applyWithdrawals);
        }

        public override int CalculateOwnerIncomeFromCaravan(MobileParty caravan)
        {
            return _previousModel.CalculateOwnerIncomeFromCaravan(caravan);
        }

        public override int CalculateOwnerIncomeFromWorkshop(Workshop workshop)
        {
            return _previousModel.CalculateOwnerIncomeFromWorkshop(workshop);
        }

        public override float RevenueSmoothenFraction()
        {
            return _previousModel.RevenueSmoothenFraction();
        }

        //private static void AddLoanRefoundToExplainedNumber(Clan clan, ref ExplainedNumber result, bool includeDescriptions, bool includeDetails)
        //{
        //    if (clan == null || clan.Leader == null || clan != Clan.PlayerClan)
        //        return;

        //    var hero = Hero.MainHero;
        //    if (hero == null || string.IsNullOrEmpty(hero.StringId))
        //        return;


        //    //Altdorf
        //    AltdorfBankCampaignBehavior AltdorfBankBehavior = Campaign.Current?.GetCampaignBehavior<AltdorfBankCampaignBehavior>();
        //    if (AltdorfBankBehavior != null && AltdorfBankCampaignBehavior.BankInstance.LoanAmount > 0)
        //    {
        //        result.Add(-AltdorfBankCampaignBehavior.BankInstance.CalculateLoanRefound(tmpLoanAmout:-1,isEstimation: false,result.RoundedResultNumber), new TextObject("Altdorf Loan Refound"));
        //    }

        //    //Drakenhof
        //    DrakenhofBankCampaignBehavior DrakenhofBankBehavior = Campaign.Current?.GetCampaignBehavior<DrakenhofBankCampaignBehavior>();
        //    if (DrakenhofBankBehavior != null && DrakenhofBankCampaignBehavior.BankInstance.LoanAmount > 0)
        //    {
        //        result.Add(-DrakenhofBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Remboursement de prêt de Drakenhof"));
        //    }
        //    //YnEdrylKoiran
        //    YnEdrylKoiranBankCampaignBehavior ynEdrylKoiranBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<YnEdrylKoiranBankCampaignBehavior>();
        //    if (ynEdrylKoiranBankCampaignBehavior != null && YnEdrylKoiranBankCampaignBehavior.BankInstance.LoanAmount > 0)
        //    {
        //        result.Add(-YnEdrylKoiranBankCampaignBehavior.BankInstance.CalculateLoanRefound(), new TextObject("Remboursement de prêt d'Yn Edryl Koiran"));
        //    }

    }
}