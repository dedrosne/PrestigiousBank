using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOR_Core.CampaignMechanics.TORCustomSettlement;
using TOR_Core.Utilities;

namespace PrestigiousBank.Entities
{
    public class LastStandPartyComponent : PartyComponent
    {

        private Settlement _home;
        private string _name;
        private int _strength;

        public Clan Clan { get; private set; }

        public override Hero PartyOwner => Clan?.Leader;
        public override TextObject Name => new TextObject(_name);
        public override Settlement HomeSettlement => _home;

        private LastStandPartyComponent(Settlement spawn, string name, int strength, Clan clan)
        {
            _home = spawn;
            _name = name;
            _strength = strength;
            Clan = clan;
        }

        public override Banner GetDefaultComponentBanner()
        {
            return Clan?.Banner;
        }

        protected override void OnMobilePartySetOnCreation()
        {
            InitializeLastStandParty();
        }

        private void InitializeLastStandParty()
        {
            // Set last stand clan
            var lastStandClan = Clan;
            MobileParty.ActualClan = lastStandClan;

            // Add troops to roster
            foreach (var (Id, MinCount, MaxCount) in LastStandPartyComponent.TroopsPerCulture[HomeSettlement.Culture.ToString()])
            {
                var character = MBObjectManager.Instance.GetObject<CharacterObject>(Id);
                if (character != null)
                {
                    int count = MBRandom.RandomInt(MinCount+_strength, MaxCount+_strength+1);
                    MobileParty.MemberRoster.AddToCounts(character, count);
                }
            }

            // Set up party properties
            MobileParty.Aggressiveness = 1f;
            MobileParty.Party.SetVisualAsDirty();
            MobileParty.InitializeMobilePartyAroundPosition(Clan.DefaultPartyTemplate, HomeSettlement.GatePosition, 30f);
            MobileParty.ItemRoster.Add(new ItemRosterElement(DefaultItems.Grain, 100));
            //Leader = 
        }

        public static MobileParty CreateLastStandParty(Settlement spawn, int strength, Clan clan)
        {
            var component = new LastStandPartyComponent(spawn, "Last Stand Defenders", strength, clan);
            return MobileParty.CreateParty(spawn.StringId + "_last_stand_defenders", component);
        }

        /// <summary>
        /// Destroys the temporary last stand party after battle
        /// </summary>
        public static void DestroyLastStandParty(MobileParty party)
        {
            if (party != null && party.IsActive)
            {
                DestroyPartyAction.Apply(null, party);
            }
        }



        public static Dictionary<string, List<(string Id, int MinCount, int MaxCount)>> TroopsPerCulture = new Dictionary<string, List<(string Id, int MinCount, int MaxCount)>>
        {
            {TORConstants.Cultures.EMPIRE.ToString(), new List<(string, int, int)> { 
                ("empire_recruit", 10, 20), 
                ("empire_militia", 5, 10) } },
            {TORConstants.Cultures.DAWI.ToString(), new List<(string, int, int)> { 
                ("desert_bandits", 10, 20), 
                ("desert_bandits", 5, 10) } },
            {TORConstants.Cultures.BRETONNIA.ToString(), new List<(string, int, int)> { 
                ("vlandia_recruit", 10, 20), 
                ("vlandia_militia", 5, 10) } },
            {TORConstants.Cultures.SYLVANIA.ToString(), new List<(string, int, int)> { 
                ("khuzait_recruit", 10, 20), 
                ("khuzait_militia", 5, 10) } },
            {TORConstants.Cultures.MOUSILLON.ToString(), new List<(string, int, int)> { 
                ("mousillon_recruit", 10, 20), 
                ("mousillon_militia", 5, 10) } },
            {TORConstants.Cultures.GREENSKIN.ToString(), new List<(string, int, int)> { 
                ("battania_recruit", 10, 20), 
                ("battania_militia", 5, 10) } },
            {TORConstants.Cultures.EONIR.ToString(), new List<(string, int  , int)> { 
                ("druchii_recruit", 10, 20), 
                ("druchii_militia", 5, 10) } },
            {TORConstants.Cultures.ASRAI.ToString(), new List<(string, int, int)> { 
                ("steppe_bandits", 10, 20), 
                ("steppe_bandits", 5, 10) } },
            {TORConstants.Cultures.CHAOS.ToString(), new List<(string, int, int)> { 
                ("chaos_culture_recruit", 10, 20), 
                ("chaos_culture_militia", 5, 10) } }
        };
    }
}
/*
public const string EMPIRE = "empire";

public const string HERRIMAULT = "desert_bandits";

public const string BRETONNIA = "vlandia";

public const string SYLVANIA = "khuzait";

public const string MOUSILLON = "mousillon";

public const string ASRAI = "battania";

public const string DRUCHII = "druchii";

public const string BEASTMEN = "steppe_bandits";

public const string CHAOS = "chaos_culture";

public const string EONIR = "eonir";

public const string DAWI = "sturgia";

public const string GREENSKIN = "aserai";

public const string GREENSKIN_BANDIT = "greenskin_bandit";

public const string GOBLIN_BANDIT = "looters";

public const string CHAOS_CULTIST = "forest_bandits";

public const string EMPIRE_DESERTERS = "mountain_bandits";
*/