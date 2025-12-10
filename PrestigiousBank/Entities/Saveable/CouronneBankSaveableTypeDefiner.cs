using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class CouronneBankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public CouronneBankSaveableTypeDefiner()
          : base(99999996)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(CouronneBank), 1);
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