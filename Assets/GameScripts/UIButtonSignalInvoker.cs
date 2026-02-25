using AxGrid;
using AxGrid.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Task3.Slot
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSignalInvoker : MonoBehaviourExt
    {
        [SerializeField] private string signalName = "SlotStartPressed";

        private Button btn;

        [OnAwake]
        private void AwakeThis()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Settings.Invoke(signalName);
        }

        [OnDestroy]
        private void DestroyThis()
        {
            if (btn != null) 
                btn.onClick.RemoveListener(OnClick);
        }
    }
}