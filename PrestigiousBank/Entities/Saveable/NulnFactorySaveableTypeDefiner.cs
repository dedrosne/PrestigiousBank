using PrestigiousBank.Entities;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class NulnFactorySaveableTypeDefiner : SaveableTypeDefiner
    {
        public NulnFactorySaveableTypeDefiner()
          : base(99999992)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(NulnFactory), 1);
            //this.AddClassDefinition(typeof(InsuredCaravan), 2);
            //this.AddClassDefinition(typeof(InsuredSettlement), 3);
        }

        /*        protected override void DefineContainerDefinitions()
                {
                    ConstructContainerDefinition(typeof(List<InsuredCaravan>));
                    ConstructContainerDefinition(typeof(List<InsuredSettlement>));
                }*/
    }
}