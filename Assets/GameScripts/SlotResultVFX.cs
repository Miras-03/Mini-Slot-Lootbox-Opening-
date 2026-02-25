using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;

namespace Task3.Slot
{
    public class SlotResultVFX : MonoBehaviourExtBind
    {
        [SerializeField] private ParticleSystem winParticles;
        private const int winIndex = 4;

        [Bind("SlotResultReady")]
        private void OnResult(int resultIndex)
        {
            if (winParticles != null&&resultIndex== winIndex)
                winParticles.Play();
        }
    }
}