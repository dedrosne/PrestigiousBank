/*using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class ClanAgencySaveableTypeDefiner : SaveableTypeDefiner
    {
        public ClanAgencySaveableTypeDefiner()
          : base(99999991)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(ClanAgency), 1);
            this.AddClassDefinition(typeof(InsuredCaravan), 2);
            //this.AddClassDefinition(typeof(InsuredSettlement), 3);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<InsuredCaravan>));
            ConstructContainerDefinition(typeof(List<InsuredSettlement>));
        }
    }
}*/