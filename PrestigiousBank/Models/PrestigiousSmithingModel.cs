using PrestigiousBank;
using System;
using System.Runtime.InteropServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousSmithingModel : TORSmithingModel
    {

        public override int GetPartResearchGainForSmeltingItem(ItemObject item, Hero hero)
        {
            int result = base.GetPartResearchGainForSmeltingItem(item, hero);

            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();

            if (KarakIzorBankCampaignBehavior != null)
            {
                result = result + MathF.Floor(result * (KarakIzorBankCampaignBehavior.BankInstance.ResearchPartFactorBought * KarakIzorBank.ResearchFactorGainPerPurchaseBought));
            }

            return result;
        }

        public override int GetPartResearchGainForSmithingItem(ItemObject item, Hero hero, bool isFreeBuild)
        {
            int result = base.GetPartResearchGainForSmithingItem(item, hero, isFreeBuild);

            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();

            if (KarakIzorBankCampaignBehavior != null)
            {
                result = result + MathF.Floor(result * (KarakIzorBankCampaignBehavior.BankInstance.ResearchPartFactorBought * KarakIzorBank.ResearchFactorGainPerPurchaseBought));
            }

            return result;
        }
    }
}