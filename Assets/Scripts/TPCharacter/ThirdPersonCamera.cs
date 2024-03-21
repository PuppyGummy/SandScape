using Cinemachine;
using UnityEngine;

namespace TPCharacter
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        private GameObject player;
        private Transform orientation;
        private CinemachineFreeLook cinemachineFreeLook;
        public float distanceToTarget = 1.5f;


        public float rotationSpeed;

        private void OnEnable()
        {
            if (!cinemachineFreeLook)
            {
                cinemachineFreeLook = gameObject.GetComponent<CinemachineFreeLook>();
            }

            player = InteractionManager.Instance.playerObject;
            orientation = player.transform.Find("Orientation");

            cinemachineFreeLook.LookAt = player.transform;
            cinemachineFreeLook.Follow = player.transform;
            AdjustCameraFOV();
        }
        private void OnDisable()
        {
            Camera.main.fieldOfView = 50f;
        }

        void AdjustCameraFOV()
        {
            if (cinemachineFreeLook != null && player != null)
            {
                float targetFOV = CalculateFOV(player.transform.localScale, distanceToTarget);
                // Update the FOV for each Rig (TopRig, MiddleRig, BottomRig)
                cinemachineFreeLook.m_Lens.FieldOfView = targetFOV;
                // Debug.Log("Target Scale: " + player.transform.localScale);
                // Debug.Log("Camera FOV: " + targetFOV);
                // Debug.Log("freeLookCamera.m_Lens.FieldOfView: " + cinemachineFreeLook.m_Lens.FieldOfView);
            }
        }

        float CalculateFOV(Vector3 targetScale, float distance)
        {
            float objectSize = Mathf.Max(targetScale.x, targetScale.y, targetScale.z); // Consider the largest dimension
            float fov = 2.0f * Mathf.Atan(objectSize / (2.0f * distance)) * Mathf.Rad2Deg; // Basic FOV calculation

            return Mathf.Clamp(fov, 40, 90); // Clamp the FOV to reasonable limits
        }
    }
}
