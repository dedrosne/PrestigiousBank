using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class ClanHideoutSaveableTypeDefiner : SaveableTypeDefiner
    {
        public ClanHideoutSaveableTypeDefiner()
          : base(99999989)
        {
        }

        protected override void DefineClassTypes()
        {
            this.AddClassDefinition(typeof(ClanHideout), 1);
            //this.AddClassDefinition(typeof(InsuredCaravan), 2);
            //this.AddClassDefinition(typeof(InsuredSettlement), 3);
        }

    }
}