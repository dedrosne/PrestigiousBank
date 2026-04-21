/*using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Party;
using TOR_Core.Models;


namespace PrestigiousBank
{
    [HarmonyPatch(typeof(TORAbilityModel), "GetMaximumWindsOfMagic")]
    public static class MobilePartyPatch
    {
        private static readonly Action<MobileParty> InternalMethodDelegate =
            AccessTools.MethodDelegate<Action<MobileParty>>(
            AccessTools.Method(typeof(MobileParty), "RemoveParty")
            );

        public static void DeleteParty(this MobileParty mobileParty)
        {
            InternalMethodDelegate(mobileParty);



        }
        
    }
}*/
