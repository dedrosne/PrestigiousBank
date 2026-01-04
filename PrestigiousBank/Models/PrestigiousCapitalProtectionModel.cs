using PrestigiousBank;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousCapitalProtectionModel : DefaultTargetScoreCalculatingModel
    {
        //Clé = Settlement.StringId, value = Kingdom.StringId
        public static Dictionary<string, string> CapitalPerKingdom = new Dictionary<string, string>
        {
            //Capitales de l'empire
            {"town_RL1","reikland" },
            {"town_ML1","middenland" },
            {"town_OL1","ostland" },
            {"town_NL1", "nordland"},
            {"town_WA1", "wasteland" },
            {"town_TB1","talabecland" },
            { "town_HL1", "hochland" },
            {"town_WI1","wissenland" },
            {"town_AV1","averland" },
            {"town_ST1","stirland" },
            {"town_OM1","ostermark" },
            //Vampires
            {"castle_MT1","necrachs" },
            {"town_SY1","sylvania" },
            {"castle_BK1","blooddragons" },
            {"town_MS1","mousillon" },
            //Elfes
            {"town_AL1","athel_loren"},
            {"town_LL1","laurelorn"},
            
            //Bretonniens
            {"town_CC1","carcassonne"},
            {"town_BL1","bordeleaux"},
            {"town_PA1","parravon"},
            {"town_BA1","bastonne"},
            {"town_GX1","gisoreux"},
            {"town_CO1","couronne"},
            {"town_LA1","anguille"},
            {"town_LY1","lyonesse"},
            {"town_AS1","artois"},
            {"town_MO1","montfort"},
            {"town_QU1","quenelles"},
            {"town_BE1","brionne"},

            {"castle_BK2","brasskeep"}

            //Manque Aqyutaine et lesss spouilleux chaosssssssssssssssss
        };

public override float GetTargetScoreForFaction(Settlement targetSettlement, Army.ArmyTypes missionType, MobileParty mobileParty, float ourStrength, int numberOfEnemyFactionSettlements = -1, float totalEnemyMobilePartyStrength = -1f)
        {
            if (missionType == Army.ArmyTypes.Besieger && CapitalPerKingdom.ContainsKey(targetSettlement.StringId) && CapitalPerKingdom[targetSettlement.StringId] == targetSettlement.OwnerClan.Kingdom.StringId) {
                if (targetSettlement.OwnerClan.Kingdom.StringId == "mousillon") return 0;
                return 0f; }
            return base.GetTargetScoreForFaction(targetSettlement, missionType, mobileParty, ourStrength);
        }
    }
}