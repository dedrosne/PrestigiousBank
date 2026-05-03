using PrestigiousBank.Entities;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class BankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public BankSaveableTypeDefiner()
          : base(597513465)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(Bank.UniteeRecrutable), 1);
            this.AddClassDefinition(typeof(AltdorfBank), 2);
            this.AddClassDefinition(typeof(AverheimBank), 3);
            this.AddClassDefinition(typeof(CouronneBank), 4);
            this.AddClassDefinition(typeof(DrakenhofBank), 5);
            this.AddClassDefinition(typeof(MiddenheimBank), 6);
            this.AddClassDefinition(typeof(ParravonBank), 7);
            this.AddClassDefinition(typeof(TorLithanelBank), 8);
            this.AddClassDefinition(typeof(YnEdrylKoiranBank), 9);
            this.AddClassDefinition(typeof(KarakIzorBank), 10);



            //Others
            this.AddClassDefinition(typeof(NulnFactory), 101);
            this.AddClassDefinition(typeof(ClanHideout), 102);
            this.AddClassDefinition(typeof(ClanAgency), 103);
            this.AddClassDefinition(typeof(ClanAgencies), 104);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<ClanAgency>));
            ConstructContainerDefinition(typeof(List<Bank.UniteeRecrutable>));
        }
    }
}