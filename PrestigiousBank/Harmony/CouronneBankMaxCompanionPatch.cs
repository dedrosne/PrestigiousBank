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
    public class MaxCompanionPatch
    {
        public static void Postfix(ref int __result)
        {
            CouronneBankCampaignBehavior CouronneBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<CouronneBankCampaignBehavior>();

            if (CouronneBankCampaignBehavior != null)
                __result +=CouronneBankCampaignBehavior.BankInstance.FriendshipQty; // Increase the max companion limit by 10.


        }

    }
}
