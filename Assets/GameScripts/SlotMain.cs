using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using UnityEngine;

namespace Task3.Slot
{
    public class SlotMain : MonoBehaviourExt
    {
        [SerializeField] private ReelView reelView;

        [OnStart]
        private void StartThis()
        {
            Settings.Model.Set("IsSpinning", false);
            Settings.Model.Set("CanStop", false);
            Settings.Model.Set("Speed", 0f);
            Settings.Model.Set("ResultIndex", -1);

            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new SlotInitState(reelView));
            Settings.Fsm.Add(new SlotIdleState());
            Settings.Fsm.Add(new SlotSpinState(reelView));
            Settings.Fsm.Add(new SlotStoppingState(reelView));
            Settings.Fsm.Add(new SlotResultState());

            Settings.Fsm.Start("SlotInit");
        }

        [OnUpdate]
        private void UpdateThis()
        {
            Settings.Fsm.Update(Time.deltaTime);
        }
    }
}