namespace NOT_Lonely_OPT
{
	using UnityEngine;
#if ENABLE_INPUT_SYSTEM
	using UnityEngine.InputSystem;
#endif

	public class Nl_DragObject : MonoBehaviour
	{
		public static Camera Cam;
		private Rigidbody rigidboy;
		private float distanceZ;
		private bool isTaken = false;
		private Vector3 offset;
		private Vector3 dir;

		void Start()
		{
			rigidboy = gameObject.GetComponent<Rigidbody>();
		}

		void Update()
		{
			if (isTaken)
			{

#if ENABLE_INPUT_SYSTEM
				if (Mouse.current.rightButton.isPressed)
				{
					Vector3 mousePos = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, distanceZ);
					Vector3 objPos = Cam.ScreenToWorldPoint(mousePos);
					rigidboy.MovePosition(objPos + offset);
				}
				else
				{
					rigidboy.useGravity = true;
					rigidboy.constraints = RigidbodyConstraints.None;
					isTaken = false;
				}

				if (Keyboard.current[Key.LeftAlt].isPressed)
				{
					float verticalAxis = 0;
					float horizontalAxis = 0;

					if (Keyboard.current[Key.W].isPressed) verticalAxis = 1;
					else if (Keyboard.current[Key.S].isPressed) verticalAxis = -1;

					if (Keyboard.current[Key.D].isPressed) horizontalAxis = 1;
					else if (Keyboard.current[Key.A].isPressed) horizontalAxis = -1;

					transform.Rotate(Vector3.right * 100 * Time.deltaTime * verticalAxis);
					transform.Rotate(Vector3.up * 100 * Time.deltaTime * horizontalAxis);
				}
#else
				if (Input.GetMouseButton(1))
				{
					Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceZ);
					Vector3 objPos = Cam.ScreenToWorldPoint(mousePos);
					rigidboy.MovePosition(objPos + offset);
				}
				else
				{
					rigidboy.useGravity = true;
					rigidboy.constraints = RigidbodyConstraints.None;
					isTaken = false;
				}
				if (Input.GetAxis("Horizontal") != 0 && Input.GetKey(KeyCode.LeftAlt))
				{
					transform.Rotate(Vector3.up * 100 * Time.deltaTime * Input.GetAxis("Horizontal"));
				}
				if (Input.GetAxis("Vertical") != 0 && Input.GetKey(KeyCode.LeftAlt))
				{
					transform.Rotate(Vector3.right * 100 * Time.deltaTime * Input.GetAxis("Vertical"));
				}
#endif
			}
		}
#if ENABLE_INPUT_SYSTEM
		private void FixedUpdate()
		{
			if (Keyboard.current[Key.LeftAlt].isPressed)
			{
				if (Mouse.current.rightButton.wasPressedThisFrame)
				{
					RaycastHit hit;
					Ray ray = Cam.ScreenPointToRay(Mouse.current.position.ReadValue());
					LayerMask layermask = ~0;

					if (Physics.Raycast(ray, out hit, 1000, layermask, QueryTriggerInteraction.Ignore))
					{
						if (hit.collider.gameObject != gameObject) return;

						isTaken = true;
						distanceZ = Vector3.Distance(Cam.transform.position, gameObject.transform.position);

						Vector3 mousePosition = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, distanceZ);
						Vector3 objPosition = Cam.ScreenToWorldPoint(mousePosition);

						offset = rigidboy.position - objPosition;

						rigidboy.velocity = Vector3.zero;
						rigidboy.useGravity = false;
						rigidboy.constraints = RigidbodyConstraints.FreezeRotation;
					}
				}
			}
		}
#else

		void OnMouseOver()
		{
			if (Input.GetKey(KeyCode.LeftAlt))
			{
				if (Input.GetMouseButtonDown(1))
				{
					isTaken = true;
					distanceZ = Vector3.Distance(Cam.transform.position, gameObject.transform.position);

					Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceZ);
					Vector3 objPosition = Cam.ScreenToWorldPoint(mousePosition);

					offset = rigidboy.position - objPosition;

					rigidboy.velocity = Vector3.zero;
					rigidboy.useGravity = false;
					rigidboy.constraints = RigidbodyConstraints.FreezeRotation;
				}
			}
		}
#endif
	}
}