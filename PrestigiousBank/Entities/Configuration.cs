using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PrestigiousBank
{
    public class Configuration
    {
        [XmlElement]
        public bool DisablePopups { get; set; }

        [XmlElement]
        public string BankName { get; set; } = "Banque Locale";

        [XmlElement]
        public string AltdorfBankName { get; set; } = "Banque de l'Ordre du Griffon";

        [XmlElement]
        public string BankMenuName { get; set; } = "Visiter la banque";

        [XmlElement]
        public string BankMenuBorrow { get; set; } = "Retirer de l'argent";

        [XmlElement]
        public string BankMenuRepay { get; set; } = "Déposer de l'argent";

        [XmlElement]
        public string BankMenuSolde { get; set; } = "Solde";

        [XmlElement]
        public string BankMenuSendCoinsToHero { get; set; }

        [XmlElement]
        public string BankMenuLeave { get; set; } = "Repartir en ville";

        [XmlArray]
        public List<string> ClerkNames { get; set; }

        [XmlElement]
        public double Rent { get; set; }

        [XmlElement]
        public double Interest { get; set; }

        [XmlElement]
        public int MaxTotalLoanAmount { get; set; }

        [XmlElement]
        public int MaxLoanAmount { get; set; }

        [XmlElement]
        public bool DoBalancing { get; set; }

        [XmlElement]
        public int BalanceWeakestNationAmount { get; set; }

        [XmlElement]
        public int BalanceNobelGoldLimit { get; set; }

        [XmlElement]
        public bool PutInterestIntoAccount { get; set; }

        [XmlElement]
        public int MaxRelationChange { get; set; }

        [XmlElement]
        public int FiefCultureChangeCost { get; set; }

/*        [XmlArray]
        public List<KeyValuePair1> CaravanInsurancePeriods { get; set; }

        [XmlArray]
        public List<KeyValuePair1> SettlementInsurancePeriods { get; set; }*/

        [XmlElement]
        public int VillageLostCompensation { get; set; }

        [XmlElement]
        public int PercentCostToRiseLoyality { get; set; }

        [XmlElement]
        public int PercentCostToLowerLoyality { get; set; }

        [XmlElement]
        public bool Cheat { get; set; }

        [XmlElement]
        public int RaidRestorationProtectionTimeInHouers { get; set; }

        [XmlElement]
        public int InfluenceTransferCost { get; set; }

        [XmlElement]
        public int InfluenceRiseCost { get; set; }

        [XmlElement]
        public int InfluenceLowerCost { get; set; }
    }
}