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
    public class PrestigiousCharacterStatsModel: TORCharacterStatsModel
    {


        public override ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false)
        {
            var number = base.MaxHitpoints(character, includeDescriptions);
            return number;
        }

    }
}