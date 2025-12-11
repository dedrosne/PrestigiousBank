using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class YnEdrylKoiranBankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public YnEdrylKoiranBankSaveableTypeDefiner()
          : base(99999995)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(YnEdrylKoiranBank), 1);
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