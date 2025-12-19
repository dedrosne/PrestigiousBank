using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class ClanAgenciesSaveableTypeDefiner : SaveableTypeDefiner
    {
        public ClanAgenciesSaveableTypeDefiner()
          : base(99999990)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(ClanAgencies), 1);
            this.AddClassDefinition(typeof(ClanAgency), 2);
            //this.AddClassDefinition(typeof(InsuredSettlement), 3);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<ClanAgency>));
            //ConstructContainerDefinition(typeof(List<InsuredSettlement>));
        }
    }
}