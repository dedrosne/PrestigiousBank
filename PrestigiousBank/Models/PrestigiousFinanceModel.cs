using System;
using PrestigiousBank;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TOR_Core.Models;
using TaleWorlds.Library;
using TaleWorlds.Localization;


namespace PrestigiousBank
{
    public class PrestigiousFinanceModel: TORClanFinanceModel
    {

        // =========================================================
        // Detailed Income (Expected Gold Change)
        // =========================================================
        public override ExplainedNumber CalculateClanIncome(
            Clan clan,
            bool includeDescriptions = false,
            bool applyWithdrawals = false,
            bool includeDetails = false)
        {
            ExplainedNumber result = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);
            //var result = new TOR_Core.Models.TORClanFinanceModel().CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);

            if (clan.StringId == "player_faction")
            {
                try
                {
                    AddBankInterestToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
                }
                catch (Exception ex)
                {
#if DEBUG
                    PrestigiousBank.LogMessage($"[BanksOfCalradia][FinanceModel] Error calculating bank income: {ex.Message}");
#endif
                }
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
            ExplainedNumber result = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);

            //ExplainedNumber result = new TORClanFinanceModel().CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);

            try
            {
                // Rendimentos de poupança
                AddBankInterestToExplainedNumber(clan, ref result, includeDescriptions, includeDetails);
                // Visualização das parcelas de empréstimos (somente detalhado)
                //AddLoanPreviewVisual(clan, ref result, includeDescriptions, includeDetails);
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

            //Altdorf
            AltdorfBankCampaignBehavior AltdorfBankBehavior = Campaign.Current?.GetCampaignBehavior<AltdorfBankCampaignBehavior>();
            if (AltdorfBankBehavior == null) return;

            int InterestsAltdorfBank = AltdorfBankCampaignBehavior.BankAltdorf.CalculateInterests();
            if (InterestsAltdorfBank != 0) 
                result.Add(InterestsAltdorfBank, new TextObject("Banque d'Altdorf"));
            if (AltdorfBankCampaignBehavior.BankAltdorf.ChannelerNumber != 0)
            {
                result.Add(-AltdorfBankCampaignBehavior.BankAltdorf.CalculateChannelerCostPerDay(), new TextObject("Canalysateurs d'Altdorf"));
            }

            //Drakenhof
            DrakenhofBankCampaignBehavior DrakenhofBankBehavior = Campaign.Current?.GetCampaignBehavior<DrakenhofBankCampaignBehavior>();

            if (DrakenhofBankBehavior == null) return;

            int InterestsDrakenhoffBank = DrakenhofBankCampaignBehavior.BankDrakenhof.CalculateInterests();
            if (InterestsDrakenhoffBank != 0)
                result.Add(InterestsDrakenhoffBank, new TextObject("Banque de Drakenhof"));


            //YnEdrylKoiran
            YnEdrylKoiranBankCampaignBehavior ynEdrylKoiranBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<YnEdrylKoiranBankCampaignBehavior>();

            if (ynEdrylKoiranBankCampaignBehavior == null) return;

            int InterestsYnEdrylKoiran = YnEdrylKoiranBankCampaignBehavior.YnEdrylKoiranBank.CalculateInterests();
            if (InterestsYnEdrylKoiran != 0)
                result.Add(InterestsYnEdrylKoiran, new TextObject("Banque d'Yn Edryl Koiran"));

            int IshaBlessingUpkeep = YnEdrylKoiranBankCampaignBehavior.YnEdrylKoiranBank.CalculateBlessingUpkeep();
            if (IshaBlessingUpkeep != 0) result.Add(-IshaBlessingUpkeep, new TextObject("Bénédiction d'Isha"));

            //Couronne
            CouronneBankCampaignBehavior CouronneBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<CouronneBankCampaignBehavior>();

            if (CouronneBankCampaignBehavior == null) return;

            int InterestsCouronne = CouronneBankCampaignBehavior.BankCouronne.CalculateInterests();
            if (InterestsCouronne != 0)
                result.Add(InterestsCouronne, new TextObject("Banque de Couronne"));


            //Averheim
            AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();

            if (AverheimBankCampaignBehavior == null) return;

            int InterestsAverheim = AverheimBankCampaignBehavior.AverheimBank.CalculateInterests();
            if (InterestsAverheim != 0)
                result.Add(InterestsAverheim, new TextObject("Banque d'Averheim"));


            //Middenheim
            MiddenheimBankCampaignBehavior MiddenheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<MiddenheimBankCampaignBehavior>();

            if (MiddenheimBankCampaignBehavior == null) return;

            int InterestsMiddenheim = MiddenheimBankCampaignBehavior.MiddenheimBank.CalculateInterests();
            if (InterestsMiddenheim != 0)
                result.Add(InterestsMiddenheim, new TextObject("Banque de Middenheim"));

            int PartyHelperUpkeep = MiddenheimBankCampaignBehavior.MiddenheimBank.CalculatePartyHelperUpkeep();
            if (PartyHelperUpkeep != 0) result.Add(-PartyHelperUpkeep, new TextObject("Aides de camp de Middenheim"));

            //Nuln Factory
            NulnFactoryCampaignBehavior NulnFactoryCampaignBehavior = Campaign.Current?.GetCampaignBehavior<NulnFactoryCampaignBehavior>();

            if (NulnFactoryCampaignBehavior == null) return;
            if (NulnFactoryCampaignBehavior.NulnFactory.PreviousDayBenefits != 0)
            {
                result.Add(NulnFactoryCampaignBehavior.NulnFactory.PreviousDayBenefits, new TextObject("Usine de Nuln"));
            }

            //Clan Agencies Upkeep
            ClanAgenciesBehaviour ClanAgenciesBehaviour = Campaign.Current?.GetCampaignBehavior<ClanAgenciesBehaviour>();
            if (NulnFactoryCampaignBehavior == null) return;
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
    }
}