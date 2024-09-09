using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EternalRunner
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerControl : MonoBehaviour
    {
        #region Defnining variables

        [SerializeField]
        private float initialPlayerSpeed = 4f;

        [SerializeField]
        private float maximumPlayerSpeed = 30f;

        [SerializeField]
        private float playerAcceleration = 0.1f;

        [SerializeField]
        private float jumpHeight = 1.0f;

        [SerializeField]
        private float initialGravity = -9.81f;

        [SerializeField]
        private LayerMask groundLayer;
        
        [SerializeField]
        private LayerMask turnLayer;

        private float playerCurrentSpeed;
        private float gravity;
        private Vector3 playerCurrentDirection = Vector3.forward;
        private Vector3 playerVelocity;

        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;

        private CharacterController characterController;

        #endregion

        #region private functions

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            characterController = GetComponent<CharacterController>();
            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];
        }

        private void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            jumpAction.performed += PlayerJump;
            slideAction.performed += PlayerSlide;
        }

        private void OnDisable()
        {
            turnAction.performed -= PlayerTurn;
            jumpAction.performed -= PlayerJump;
            slideAction.performed -= PlayerSlide;
        }

        private void Start()
        {
            playerCurrentSpeed = initialPlayerSpeed;
            gravity = initialGravity;
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            context.ReadValue<float>();

        }

        private Vector3? CheckTurnSide(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);

            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;

                if ((type == TileType.LEFT && turnValue == -1) || (type == TileType.RIGHT && turnValue == 1) || 
                    (type == TileType.SIDEWAYS))
                    return tile.pivot.position;
            }

            return null;
        }

        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (IsGrounded())
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
                characterController.Move(playerVelocity * Time.deltaTime);
            }
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {

        }

        private void Update()
        {
            characterController.Move(transform.forward * playerCurrentSpeed * Time.deltaTime);

            if (IsGrounded() && playerVelocity.y < 0)
                playerVelocity.y = 0;

            playerVelocity.y += gravity * Time.deltaTime;

            characterController.Move(playerVelocity * Time.deltaTime);
        }

        private bool IsGrounded(float length = 0.2f)
        {
            Vector3 firstRaycastOrigin = transform.position;
            Vector3 secondRaycastOrigin;

            float offsetFromGround = 0.1f;
            float offsetFromControllerPos = 0.2f;

            firstRaycastOrigin.y -= (characterController.height / 2f);
            firstRaycastOrigin.y += offsetFromGround;

            secondRaycastOrigin = firstRaycastOrigin;

            firstRaycastOrigin -= transform.forward * offsetFromControllerPos;
            secondRaycastOrigin += transform.forward * offsetFromControllerPos;

            Debug.DrawLine(firstRaycastOrigin, Vector3.down, Color.green, 2f);
            Debug.DrawLine(secondRaycastOrigin, Vector3.down, Color.red, 2f);

            if (Physics.Raycast(firstRaycastOrigin, Vector3.down, out RaycastHit hit1, length, groundLayer) ||
                Physics.Raycast(secondRaycastOrigin, Vector3.down, out RaycastHit hit2, length, groundLayer))
                return true;
            else
                return false;
        }

        #endregion


    }
}
