using System;
using UnityEngine;

namespace TPCharacter
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("Components")]
        public GameObject cameraObject;


        private GameObject playerObject;
        private PlayerMovementController playerMovementController;
        private bool isEnabled;

        public void Update()
        {
            // SetPlayer();
        }

        private void SetPlayer()
        {
            if (!InteractionManager.Instance.playerObject) return;

            playerObject = InteractionManager.Instance.playerObject;

            playerMovementController = playerObject.GetComponent<PlayerMovementController>();
        }

        /// <summary>
        /// Enables ability to move, and also sets the freelook cam as main cam
        /// </summary>
        public void EnableCharacter()
        {
            SetPlayer();
            if (playerMovementController == null)
                return;

            //Reset rotation
            playerObject.transform.rotation = new Quaternion(0.0f, playerObject.transform.rotation.y, 0.0f, 0.0f);
            // playerObject.GetComponent<Rigidbody>().isKinematic = true;
            playerMovementController.enabled = true;
            playerMovementController.active = true;
            //Enable camera
            cameraObject.SetActive(true);
        }

        /// <summary>
        /// Disables ability to move, and also disables the freelook cam
        /// </summary>
        public void DisableCharacter()
        {
            if (playerMovementController == null)
                return;
            // playerObject.GetComponent<Rigidbody>().isKinematic = false;

            playerMovementController.enabled = false;
            playerMovementController.active = false;
            cameraObject.SetActive(false);
        }
    }
}
