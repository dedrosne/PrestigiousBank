using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999998)]
    public class InsuredCaravan
    {
        //[SaveableProperty(1)]
        public uint CaravanId { get; set; }

        //[SaveableProperty(2)]
        public float FreePassageUntil { get; private set; }

        //[SaveableProperty(3)]
        public bool Notified { get; set; } = false;

        public InsuredCaravan()
        {
            Notified = false;
        }

        public InsuredCaravan(uint id, float houersFromStart)
        {
            CaravanId = id;
            FreePassageUntil = houersFromStart;
            Notified = false;
        }

        public void SetFreePassageUntil(float time)
        {
            Notified = false;
            FreePassageUntil = time;
        }
    }
}