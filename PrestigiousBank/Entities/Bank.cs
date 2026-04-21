//using Birke.UI;
using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TOR_Core.BattleMechanics.StatusEffect;

namespace PrestigiousBank
{
    public class Bank
    {
        


        [SaveableProperty(1)]
        public int Solde { get; set; }


        public Settlement _ville;

        public Settlement Ville
        {
            get { return _ville; }
            set
            {
                _ville = value;
            }
        }

        public Bank(Settlement ville)
        {
            Solde = 0;
            ListUniteesRecrutables = null;
            Ville = ville;
            CanRecruitMercenariesInThisBank = false;
        }

        public float CalculateInterestRate()
        {
            if (Ville != null)
            {
                return (Ville.Town.Prosperity/3500) * 0.001f;
            }
            else return 0f;
            //Prosperity between 0 and 10 000. Usually at 3500-4000
            //3500 = 0.1% interest/day
        }

        public int CalculateInterests()
        {
            float interestRate = CalculateInterestRate();

            if (interestRate == 0f) return 0;

            return (int)(Solde * interestRate);
        }

        public int GetCustomerLevel()
        {
            if (Solde <= 49_999) return 1;
            if (Solde <= 149_999) return 2;
            if (Solde <= 299_999) return 3;
            if (Solde <= 499_999) return 4;
            if (Solde <= 999_999) return 5;
            else return 6;
        }

        public String GetCustomerLevelString()
        {
            int level = GetCustomerLevel();
            return GetCustomerLevelStringPerLevel(level);
        }

        public static string GetCustomerLevelStringPerLevel(int level)
        {
            if (level == 1) return "Bronze";
            if (level == 2) return "Argent";
            if (level == 3) return "Or";
            if (level == 4) return "Mythril";
            if (level == 5) return "Diamant";
            else return "Malepierre";
        }

        public int GetDailySkillXP()
        {
            //SkillXp = 1XP/1000or pour tout or au dessus du palier Or. Max 1000/jour
            return Math.Min(Math.Max((Solde - 150_000) / 1000, 0),1000);
        }

        public void ApplyDiamondLevelGoldTownIncrease()
        {
            if (GetCustomerLevel() == 5) {
                int newGoldMaximum = Convert.ToInt32(Ville.Town.Prosperity * 10f) + (Solde - 500_000)/10;
                int currentgold = Ville.Town.Gold;
                if (currentgold < newGoldMaximum) Ville.Town.ChangeGold(1000+(newGoldMaximum-currentgold)/20);
            }
        }

        #region Mercenaries

        /// <summary>
        /// Variables
        /// </summary>

        public static Dictionary<int, (int clanTiers, int bankLevel)> mercenariesRequirementPerUnitTiers = new Dictionary<int, (int clanTiers, int bankLevel)>
        {
            { 1, (1,1) },
            { 2, (2,1) },
            { 3, (2,1) },
            { 4, (3,2) },
            { 5, (3,2) },
            { 6, (4,3) },
            { 7, (4,4) },
            { 8, (5,5) },
            { 9, (5,6) },
            { 10, (6,6) }
        };

        public static float initRegenPerDayMercenaries = 0.2f;
        public static int initMaxMercenaries = 5;

        public class UniteeRecrutable
        {
            [SaveableProperty(1)]
            public string IdString {  get; set; }

            [SaveableProperty(2)]
            public float NbRecrutable {  get; set; }

            public UniteeRecrutable(string idString, float nbRecrutable=0)
            {
                IdString = idString;
                NbRecrutable = nbRecrutable;
            }
        }

        [SaveableProperty(2)]
        public List<UniteeRecrutable> ListUniteesRecrutables { get; set; }

        [SaveableProperty(3)]
        public bool CanRecruitMercenariesInThisBank { get; set; }

        [SaveableProperty(4)]
        public float RegenPerDayMercenaries {  get; set; }

        [SaveableProperty(5)]
        public int MaxMercenaries { get; set; }

        //End Variables

        public void InitMercenariesUnitFromListString(List<string> listIdString)
        {
            ListUniteesRecrutables = new List<UniteeRecrutable>();
            foreach (string idString in listIdString)
            {
                ListUniteesRecrutables.Add(new UniteeRecrutable(idString)); 
            }
        }

        //Can recruit if the clan is part or allied of Bank Kingdom
        public bool CheckKingdomsRequirement()
        {
            var clanKingdom = Clan.PlayerClan.Kingdom;
            var bankKingdom = _ville.OwnerClan.Kingdom;
            if (clanKingdom != null && bankKingdom != null)
            {
                if (clanKingdom.StringId == bankKingdom.StringId) return true;
                else if (clanKingdom.AlliedKingdoms != null && clanKingdom.AlliedKingdoms.Count != 0)
                {
                    foreach (Kingdom allied in clanKingdom.AlliedKingdoms)
                    {
                        if (allied.StringId == bankKingdom.StringId) { return true; }
                    }
                }
            }
            return false;
        }

        public bool CheckClanAndBankRequirement(int unitTiers)
        {
            return Clan.PlayerClan.Tier >= mercenariesRequirementPerUnitTiers[unitTiers].clanTiers &
                this.GetCustomerLevel() >= mercenariesRequirementPerUnitTiers[unitTiers].bankLevel;
        }

        public static int GetRecruitmentCostMercenaries(CharacterObject characterObject)
        {
            var tmp = (int)Math.Pow(characterObject.Tier, 2);
            var tmp2 = tmp * characterObject.HitPoints;
            //return (characterObject.Tier ^ 2) * characterObject.HitPoints; 
            return tmp2;
        }

        public static CharacterObject GetUnitPerStringID(string id)
        {
            return MBObjectManager.Instance.GetObject<CharacterObject>(id);
        }

        private void AddMercenaryToPlayerParty(CharacterObject characterObject)
        {
            PartyBase.MainParty.AddMember(characterObject, 1, 0);
        }

        public void ApplyMercenaryRecruited(string idUnit)
        {
            CharacterObject characterObject = GetUnitPerStringID(idUnit);
            ListUniteesRecrutables.ForEach(recruitment => { if (recruitment.IdString == idUnit) recruitment.NbRecrutable -= 1; }); 
            AddMercenaryToPlayerParty(characterObject);
            Hero.MainHero.ChangeHeroGold(-GetRecruitmentCostMercenaries(characterObject));
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));

        }

        public void InitMercenariesVariables()
        {
            CanRecruitMercenariesInThisBank = true;
            InitMercenariesUnits();
        }

        protected virtual void InitMercenariesUnits()
        {

        }

        protected void SortAndCleanMercenaryUnitList()
        {
            ListUniteesRecrutables = ListUniteesRecrutables.WhereQ(x => GetUnitPerStringID(x.IdString) != null).ToList();

            if (ListUniteesRecrutables != null && ListUniteesRecrutables.Count > 1)
            {
                ListUniteesRecrutables = ListUniteesRecrutables.OrderBy(i => GetUnitPerStringID(i.IdString) == null ? 0 : GetUnitPerStringID(i.IdString).Tier).ToList();
                var party = PartyBase.MainParty.MemberRoster;
            }
        }

        public void ApplyRegenMercenariesPerDay()
        {
            if (ListUniteesRecrutables != null && ListUniteesRecrutables.Count != 0)
            {
                ListUniteesRecrutables.ForEach(i => { i.NbRecrutable = Math.Min(i.NbRecrutable + RegenPerDayMercenaries,MaxMercenaries); });
            }
        }
        #endregion
    }
}