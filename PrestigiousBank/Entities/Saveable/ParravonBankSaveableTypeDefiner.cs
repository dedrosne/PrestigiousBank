using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class ParravonBankSaveableTypeDefiner : SaveableTypeDefiner
    {
        public ParravonBankSaveableTypeDefiner()
          : base(99999988)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(ParravonBank), 1);
        }
    }
}