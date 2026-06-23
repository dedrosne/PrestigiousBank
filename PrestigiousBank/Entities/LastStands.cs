using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TOR_Core.Utilities;

namespace PrestigiousBank.Entities
{
    public class LastStandPerCulture
    {
        [SaveableProperty(1)]
        public string Culture { get; set; }

        [SaveableProperty(2)]
        public bool IsConsumed { get; set; }
        [SaveableProperty(3)]
        public int Strength { get; set; }
        public LastStandPerCulture(string culture)
        {
            Culture = culture;  
            IsConsumed = false;
            Strength = 0;
        }
    }

    public class LastStands
    {
        [SaveableProperty(1)]
        public List<LastStandPerCulture> LastStandPerCulture { get; set; }

        public LastStands()
        {
            LastStandPerCulture = new List<LastStandPerCulture>();
            foreach (TORConstants.Cultures culture in Enum.GetValues(typeof(TORConstants.Cultures)))
            {
                LastStandPerCulture.Add(new Entities.LastStandPerCulture(culture.ToString()));
            }
        }

        public LastStandPerCulture GetLastStandForCulture(string culture)
        {
            return LastStandPerCulture.FirstOrDefault(ls => ls.Culture == culture);
        }

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