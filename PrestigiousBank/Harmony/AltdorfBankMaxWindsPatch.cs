using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TOR_Core.Models;


namespace PrestigiousBank
{
    [HarmonyPatch(typeof(TORAbilityModel), "GetMaximumWindsOfMagic")]
    public class AltdorfBankMaxWindsPatch
    {
        static void Postfix(ref float __result)
        {
            if (AltdorfBankCampaignBehavior.BankInstance != null && AltdorfBankCampaignBehavior.BankInstance.ManastoneNumber != 0)
            {
                __result += AltdorfBankCampaignBehavior.BankInstance.ManastoneNumber;
            }

            
        }
        
    }

    [HarmonyPatch(typeof(TORAbilityModel), "GetWindsRechargeRate")]
    public class AltdorfBankRechargeWindsPatch
    {
        static void Postfix(ref float __result)
        {
            if (AltdorfBankCampaignBehavior.BankInstance != null && AltdorfBankCampaignBehavior.BankInstance.ChanelerNumber != 0)
            {
                __result += AltdorfBankCampaignBehavior.BankInstance.ChanelerNumber * 0.01f;
            }


        }

    }


}
