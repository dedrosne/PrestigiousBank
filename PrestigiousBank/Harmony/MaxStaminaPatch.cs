using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Party;
using TOR_Core.Models;


namespace PrestigiousBank
{
    [HarmonyPatch(typeof(CraftingCampaignBehavior), "GetMaxHeroCraftingStamina")]
    public class MaxStaminaPatch
    {
        static void Postfix(ref int __result)
        {
            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();
            if (KarakIzorBankCampaignBehavior != null && KarakIzorBankCampaignBehavior.BankKarakIzor != null && KarakIzorBankCampaignBehavior.BankKarakIzor.MaximumStaminaBought != 0)
            {
                __result += KarakIzorBankCampaignBehavior.BankKarakIzor.MaximumStaminaBought * KarakIzorBank.MaximumStaminaGainPerPurchaseBought;
            }


        }

    }
}
