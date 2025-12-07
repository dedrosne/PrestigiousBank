using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999997)]
    public class InsuredSettlement
    {
        //[SaveableProperty(1)]
        public uint SettlementId { get; set; }

        //[SaveableProperty(2)]
        public float InsuredUntil { get; private set; }

        //[SaveableProperty(3)]
        public bool Notified { get; set; } = false;

        //[SaveableProperty(4)]
        public float CantBeRaidedUntil { get; set; }

        public InsuredSettlement()
        {
        }

        public InsuredSettlement(uint id, float houersFromStart)
        {
            SettlementId = id;
            InsuredUntil = houersFromStart;
            Notified = false;
        }

        public void SetInsuredUntil(float time)
        {
            Notified = false;
            InsuredUntil = time;
        }
    }
}