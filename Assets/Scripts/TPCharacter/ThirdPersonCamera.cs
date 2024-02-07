using Cinemachine;
using UnityEngine;

namespace TPCharacter
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        private GameObject player;
        private Transform orientation;
        private CinemachineFreeLook cinemachineFreeLook;
    
        public float rotationSpeed;

        private void OnEnable()
        {
            if (!cinemachineFreeLook)
            {
                cinemachineFreeLook = gameObject.GetComponent<CinemachineFreeLook>();
            }
        
            player = InteractionManager.Instance.playerObject;
            orientation = player.transform.GetChild(1).transform;

            cinemachineFreeLook.LookAt = player.transform;
            cinemachineFreeLook.Follow = player.transform;
        }

        void Update()
        {
            //Set forward rotation ot view direction
            var transformPosition = transform.position;
            var playerPosition = player.transform.position;
            Vector3 viewDirection = playerPosition - new Vector3(transformPosition.x, playerPosition.y, transformPosition.z);

            orientation.forward = viewDirection.normalized;

            //If there's any input, turn to new forward direction
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
            if(inputDirection != Vector3.zero)
                player.transform.forward = Vector3.Slerp(player.transform.forward, inputDirection.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
