using Cinemachine;
using UnityEngine;

namespace TBS.CameraUtils
{
    public class CameraHolder : MonoBehaviour
    {
        [SerializeField]
        private CinemachineFreeLook m_CinemachineFreeLook;

        [SerializeField]
        private Camera m_camera;

        public Camera Camera => m_camera;

        public void SetCameraLookAtTransform(Transform lookAtTransform)
        {
            m_CinemachineFreeLook.LookAt = lookAtTransform;
            m_CinemachineFreeLook.Follow = lookAtTransform;
        }
    }
}
