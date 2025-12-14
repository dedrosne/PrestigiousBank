using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class MiddenheimBankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public MiddenheimBankSaveableTypeDefiner()
          : base(99999994)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(MiddenheimBank), 1);
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