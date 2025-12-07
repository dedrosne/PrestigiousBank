using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class DrakenhofBankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public DrakenhofBankSaveableTypeDefiner()
          : base(99999997)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(AltdorfBank), 1);
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