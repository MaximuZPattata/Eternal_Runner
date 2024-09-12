using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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
        private float scoreMultiplier = 10f;

        [SerializeField]
        private LayerMask groundLayer;
        
        [SerializeField]
        private LayerMask turnLayer;

        [SerializeField]
        private LayerMask obstacleLayer;

        [SerializeField]
        private UnityEvent<Vector3> turnEvent;

        [SerializeField]
        private UnityEvent<int> gameOverEvent;

        [SerializeField]
        private UnityEvent<int> scoreUpdateEvent;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AnimationClip slideAnimationClip;

        private int slidingAnimationId;
        private float score = 0f;
        private float playerCurrentSpeed;
        private float gravity;
        private bool playerCurrentlySliding = false;
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

            slidingAnimationId = Animator.StringToHash("Player_Basic_Slide");

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
            Vector3? turnPosition = CheckTurnPosition(context.ReadValue<float>());

            if (!turnPosition.HasValue)
            {
                GameOver();

                return;
            }

            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * playerCurrentDirection;

            turnEvent.Invoke(targetDirection);

            RotatePlayer(context.ReadValue<float>(), turnPosition.Value);
        }

        private void RotatePlayer(float turnValue, Vector3 targetPositionWherePlayerTurns)
        {
            Vector3 tempPlayerPosition = new Vector3(targetPositionWherePlayerTurns.x, transform.position.y, targetPositionWherePlayerTurns.z);
            characterController.enabled = false;
            transform.position = tempPlayerPosition;
            characterController.enabled = true;

            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90*turnValue, 0);
            transform.rotation = targetRotation;
            playerCurrentDirection = transform.forward.normalized;
        }

        private Vector3? CheckTurnPosition(float turnValue)
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
            if(!playerCurrentlySliding && IsGrounded())
                StartCoroutine(Slide());
        }

        private IEnumerator Slide()
        {
            playerCurrentlySliding = true;

            // Shrinking controller
            Vector3 originalControllerCenter = characterController.center;
            Vector3 newControllerCenter = originalControllerCenter;
            
            characterController.height /= 2;    
            newControllerCenter.y -= characterController.height/2;
            characterController.center = newControllerCenter;

            // Playing animation
            animator.Play(slidingAnimationId);
            yield return new WaitForSeconds(slideAnimationClip.length);

            // Reset Controller
            characterController.height *= 2;
            characterController.center = originalControllerCenter;

            playerCurrentlySliding = false;
        }

        private void Update()
        {
            if (!IsGrounded(20f))
            {
                GameOver();

                return;
            }

            score += scoreMultiplier * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)score);

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

            //Debug.DrawLine(firstRaycastOrigin, Vector3.down, Color.green, 2f);
            //Debug.DrawLine(secondRaycastOrigin, Vector3.down, Color.red, 2f);

            if (Physics.Raycast(firstRaycastOrigin, Vector3.down, out RaycastHit hit1, length, groundLayer) ||
                Physics.Raycast(secondRaycastOrigin, Vector3.down, out RaycastHit hit2, length, groundLayer))
                return true;
            else
                return false;
        }

        private void GameOver()
        {
            gameOverEvent.Invoke((int)score);
            gameObject.SetActive(false);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                GameOver();

            //if (hit.collider.gameObject.layer == obstacleLayer)
            //    GameOver();
        }
        #endregion

    }
}
