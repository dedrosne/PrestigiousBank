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

            AltdorfBankCampaignBehavior AltdorfBankBehavior = Campaign.Current?.GetCampaignBehavior<AltdorfBankCampaignBehavior>();

            if (AltdorfBankBehavior == null) return;

            int InterestsAltdorfBank = AltdorfBankCampaignBehavior.BankAltdorf.CalculateInterests();
            if (InterestsAltdorfBank != 0) 
                result.Add(InterestsAltdorfBank, new TextObject("Banque d'Altdorf"));

            DrakenhofBankCampaignBehavior DrakenhofBankBehavior = Campaign.Current?.GetCampaignBehavior<DrakenhofBankCampaignBehavior>();

            if (DrakenhofBankBehavior == null) return;

            int InterestsDrakenhoffBank = DrakenhofBankCampaignBehavior.BankDrakenhof.CalculateInterests();
            if (InterestsDrakenhoffBank != 0)
                result.Add(InterestsDrakenhoffBank, new TextObject("Banque de Drakenhof"));


        }
    }
}