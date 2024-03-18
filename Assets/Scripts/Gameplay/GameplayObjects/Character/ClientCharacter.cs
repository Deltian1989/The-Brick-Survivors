using Unity.Netcode;
using UnityEngine;

namespace TBS.Gameplay.GameplayObjects.Character
{
    public class ClientCharacter : NetworkBehaviour
    {
        [SerializeField]
        Animator m_ClientVisualsAnimator;

        [SerializeField]
        NetworkPlayerProcessor m_NetworkPlayerProcessor;

        public Animator OurAnimator => m_ClientVisualsAnimator;

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (m_ClientVisualsAnimator)
            {
                OurAnimator.SetBool("isWalking", m_NetworkPlayerProcessor.IsPlayerMoving);
            }
        }
    }
}
