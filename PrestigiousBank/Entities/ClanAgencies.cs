using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999990)]
    public class ClanAgencies
    {
        [SaveableProperty(1)]
        private List<ClanAgency> ClanAgenciesList { get; set; }

        [SaveableProperty(2)]
        public int MaxLimitAgencyUpgradeBought { get; set; }

        private ClanAgency _currentSettlementAgency;

        public ClanAgency CurrentSettlementAgency
        {
            get {
                if (_currentSettlementAgency == null) RefreshCurrentSettlementAgency();
                if (_currentSettlementAgency == null && GetClanAgenciesList().Count != 0) _currentSettlementAgency = ClanAgenciesList[0];//Afraid that CurrentSettlementAgency could be called outside of settlement
                return _currentSettlementAgency; }
        }

        public static int InitialPriceIncreaseMaxLimitAgency = 20_000;

        public static int InitialMaxLimitAgency = 1;

        public static int UpgradeMaxLimitAgencyCost = 50_000;


        public List<ClanAgency> GetClanAgenciesList() {
            if (ClanAgenciesList == null )ClanAgenciesList = new List<ClanAgency> { };
            return ClanAgenciesList;} 
         

        public ClanAgency GetAgencyByTownStringId(string townId)
        {
            if (GetClanAgenciesList().Count != 0)
            {
                foreach (ClanAgency listMember in GetClanAgenciesList())
                {
                    if (listMember.TownID == townId) return listMember;
                }
            }
            return null;
        }
        

        public ClanAgency CreateAgencyFromTownID(string TownId)
        {
            ClanAgency agency = new ClanAgency(TownId);
            ClanAgenciesList.Add(agency);
            agency.LevelAgency = 1;
            agency.SelectedLevel = 1;
            _currentSettlementAgency = agency;
            return agency;
        }

        public ClanAgencies()
        {
            MaxLimitAgencyUpgradeBought = InitialMaxLimitAgency;
        }

        public void RefreshCurrentSettlementAgency()
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.Town != null)
                _currentSettlementAgency = GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
        }

        public bool DoDisplayOptionToBuyAgency()
        {
            if (NulnFactoryCampaignBehavior.NulnFactory.FactoryLevel > 0 || 
                AltdorfBankCampaignBehavior.BankAltdorf.IsTeleportUnblocked || 
                ClanHideoutCampaignBehavior.ClanHideout.LevelHideout > 0) 
                return true;

            else return false;
        }

        public int GetMaxAgencies()
        {
            return NulnFactoryCampaignBehavior.NulnFactory.FactoryLevel+Clan.PlayerClan.Tier+MaxLimitAgencyUpgradeBought;
        }

        public List<ClanAgency> GetAgenciesTeleportUnblocked()
        {
            return GetClanAgenciesList().Where(x => x.IsTeleportUnlocked).ToList();
        }

    }
}