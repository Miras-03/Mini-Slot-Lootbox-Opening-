using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Task3.Slot
{
    [RequireComponent(typeof(Button))]
    public class UIButtonInteractableBind : MonoBehaviourExtBind
    {
        [SerializeField] private string modelBoolField = "CanStop";
        [SerializeField] private bool invert = false;

        private Button btn;

        [OnAwake]
        private void AwakeThis()
        {
            btn = GetComponent<Button>();
        }

        [OnStart]
        private void StartThis()
        {
            Apply();
        }

        [Bind("On{modelBoolField}Changed")]
        private void OnModelChanged()
        {
            Apply();
        }

        private void Apply()
        {
            bool v = Model.GetBool(modelBoolField, false);
            btn.interactable = invert ? !v : v;
        }
    }
}