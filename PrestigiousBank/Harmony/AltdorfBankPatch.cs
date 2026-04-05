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
    public class AltdorfBankPatch
    {
        static void Postfix(ref float __result)
        {
            if (AltdorfBankCampaignBehavior.BankAltdorf != null && AltdorfBankCampaignBehavior.BankAltdorf.ChannelerNumber != 0)
            {
                //PrestigiousBank.LogMessage(String.Format("Ancienne valeur vent de magie :{0}", __result));
                __result += AltdorfBankCampaignBehavior.BankAltdorf.ChannelerNumber;
                //PrestigiousBank.LogMessage(String.Format("Nouvelle valeur vent de magie :{0}", __result));
            }

            
        }
        
    }
}
