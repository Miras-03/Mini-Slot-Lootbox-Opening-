using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;
using UnityEngine;

namespace Task3.Slot
{
    [State("SlotInit")]
    public class SlotInitState : FSMState
    {
        private readonly ReelView reel;

        public SlotInitState(ReelView reelView)
        {
            reel = reelView;
        }

        [Enter]
        private void EnterThis()
        {
            reel.ResetLayoutToGrid();
            reel.RandomizeAllSymbols();

            Settings.Model.Set("IsSpinning", false);
            Settings.Model.Set("CanStop", false);
            Settings.Model.Set("Speed", 0f);
            Settings.Model.Set("ResultIndex", -1);

            Parent.Change("SlotIdle");
        }
    }

    [State("SlotIdle")]
    public class SlotIdleState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("IsSpinning", false);
            Settings.Model.Set("CanStop", false);
            Settings.Model.Set("Speed", 0f);
        }

        [Bind("SlotStartPressed")]
        private void StartPressed()
        {
            Parent.Change("SlotSpin");
        }
    }

    [State("SlotSpin")]
    public class SlotSpinState : FSMState
    {
        private readonly ReelView reel;

        private const float accelTime = 1.5f;
        private const float maxSpeed = 3000f; 
        private const float minStopDelay = 3.0f;

        public SlotSpinState(ReelView reelView)
        {
            reel = reelView;
        }

        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("IsSpinning", true);
            Settings.Model.Set("CanStop", false);

            float startSpeed = reel.GetSpeed();

            Settings.Model.Set("SpinT", 0f);
            Settings.Model.Set("SpinStartSpeed", startSpeed);
        }

        [Loop(0f)]
        private void UpdateSpin(float dt)
        {
            float t = Settings.Model.GetFloat("SpinT", 0f);
            t += dt;
            Settings.Model.Set("SpinT", t);

            float startSpeed = Settings.Model.GetFloat("SpinStartSpeed", 0f);
            float k = Mathf.Clamp01(t / accelTime);

            float eased = 1f - Mathf.Pow(1f - k, 3f);
            float speed = Mathf.Lerp(startSpeed, maxSpeed, eased);

            Settings.Model.Set("Speed", speed);
        }

        [One(minStopDelay)]
        private void AllowStop()
        {
            Settings.Model.Set("CanStop", true);
        }

        [Bind("SlotStopPressed")]
        private void StopPressed()
        {
            if (!Settings.Model.GetBool("CanStop", false)) return;
                Parent.Change("SlotStopping");
        }

        [Exit]
        private void ExitThis()
        {
        }
    }

    [State("SlotStopping")]
    public class SlotStoppingState : FSMState
    {
        private readonly ReelView reel;

        private const float decelTime = 1.2f;
        private const float snapTime = 0.2f;

        public SlotStoppingState(ReelView reelView)
        {
            reel = reelView;
        }

        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("CanStop", false);

            float startSpeed = Settings.Model.GetFloat("Speed", 0f);
            Settings.Model.Set("StopT", 0f);
            Settings.Model.Set("StopStartSpeed", startSpeed);

            Settings.Model.Set("StopPhase", 0);
        }

        [Loop(0f)]
        private void UpdateStop(float dt)
        {
            int phase = Settings.Model.GetInt("StopPhase", 0);

            if (phase == 0)
            {
                float t = Settings.Model.GetFloat("StopT", 0f);
                t += dt;
                Settings.Model.Set("StopT", t);

                float startSpeed = Settings.Model.GetFloat("StopStartSpeed", 0f);
                float k = Mathf.Clamp01(t / decelTime);

                float eased = Mathf.Pow(1f - k, 3f);
                float speed = startSpeed * eased;

                Settings.Model.Set("Speed", speed);

                if (k >= 1f)
                {
                    Settings.Model.Set("Speed", 0f);

                    int resultIdx = reel.SnapToCenterAndGetResultIndex(snapTime);
                    Settings.Model.Set("ResultIndex", resultIdx);

                    Settings.Model.Set("SnapT", 0f);
                    Settings.Model.Set("StopPhase", 1);
                }
            }
            else
            {
                float t = Settings.Model.GetFloat("SnapT", 0f);
                t += dt;
                Settings.Model.Set("SnapT", t);

                float k = Mathf.Clamp01(t / snapTime);
                float eased = 1f - Mathf.Pow(1f - k, 3f);

                reel.ApplySnapProgress(eased);

                if (k >= 1f)
                    Parent.Change("SlotResult");
            }
        }
    }

    [State("SlotResult")]
    public class SlotResultState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("IsSpinning", false);
            Settings.Model.Set("Speed", 0f);

            Settings.Invoke("SlotResultReady", Settings.Model.GetInt("ResultIndex", -1));

        }

        [Bind("SlotStartPressed")]
        private void StartAgain()
        {
            Parent.Change("SlotSpin");
        }
    }
}