using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999990)]
    public static class ClanAgencies
    {
        [SaveableProperty(1)]
        private static List<ClanAgency> _clanAgenciesList;
        public static List<ClanAgency> ClanAgenciesList {get {
            _clanAgenciesList ??= new List<ClanAgency> { };
            return _clanAgenciesList;} 
            set {_clanAgenciesList = value;}}

        public static ClanAgency GetAgencyByTownStringId(string townId)
        {
            foreach(ClanAgency listMember in ClanAgenciesList)
            {
                if (listMember.TownID == townId) return listMember;
            }
            return null;
        }

        public static ClanAgency CreateAgencyFromTownID(string TownId)
        {
            ClanAgency agency = new(townID);
            ClanAgenciesList.Add(agency);
            return agency;
        }
 
    }
}