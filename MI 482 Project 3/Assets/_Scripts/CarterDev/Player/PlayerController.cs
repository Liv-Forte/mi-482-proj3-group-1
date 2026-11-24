using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference LeftTrigger;
    public InputActionReference RightTrigger;
    public InputActionReference LeftGrip;
    public InputActionReference RightGrip;
    public InputActionReference rightJoystick;
    public InputActionReference AButton;
    public InputActionReference BButton;

    [Header("Movement")]
    [Tooltip("The Continuous Move Provider that handles left joystick movement. " +
             "This script turns on its Left Hand Move Input and reads the stick from it.")]
    public ContinuousMoveProvider moveProvider;

    [Header("Gravity")]
    [Tooltip("Requires a CharacterController on this object. " +
             "Turn OFF 'Use Gravity' on the Continuous Move Provider so gravity isn't applied twice.")]
    public float gravity = -9.81f;

    [Tooltip("Small downward push while grounded so isGrounded stays reliable.")]
    public float groundedStickForce = -2f;

    [Tooltip("Maximum fall speed (negative). Stops velocity from growing forever on long falls.")]
    public float terminalVelocity = -50f;

    [Header("Climbing")]
    [Tooltip("The XRI Climb Provider on the rig. While it's active, gravity is paused so it doesn't fight the climb.")]
    public ClimbProvider climbProvider;

    // Input values (static so other scripts can read them, e.g. PlayerController.LeftTriggerValue).
    // Note: static fields don't show in the Inspector.
    public static float LeftTriggerValue;
    public static float RightTriggerValue;
    public static float LeftGripValue;
    public static float RightGripValue;
    public static float Avalue;
    public static float Bvalue;
    public static Vector2 leftJoystickValue;   // read from the move provider's Left Hand Move Input
    public static Vector2 rightJoystickValue;

    /// <summary>True while the Climb Provider is moving the rig (gripping a Climb Interactable).</summary>
    public bool IsClimbing => climbProvider != null && climbProvider.isLocomotionActive;

    private CharacterController characterController;
    private float verticalVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogWarning($"{name}: PlayerController needs a CharacterController on the same object for gravity to work.", this);
        }

        // Fall back to finding the providers in the scene if none were assigned
        if (moveProvider == null)
        {
            moveProvider = FindAnyObjectByType<ContinuousMoveProvider>();
        }

        if (climbProvider == null)
        {
            climbProvider = FindAnyObjectByType<ClimbProvider>();
        }

        if (moveProvider != null && moveProvider.useGravity && characterController != null)
        {
            Debug.LogWarning($"{name}: 'Use Gravity' is on in the Continuous Move Provider AND PlayerController applies gravity. " +
                             "Turn it off on the move provider so gravity isn't doubled.", moveProvider);
        }
    }

    void OnEnable()
    {
        // Make sure the actions are actually enabled, otherwise ReadValue always returns 0
        EnableAction(LeftTrigger);
        EnableAction(RightTrigger);
        EnableAction(LeftGrip);
        EnableAction(RightGrip);
        EnableAction(rightJoystick);
        EnableAction(AButton);
        EnableAction(BButton);

        // Turn on the move provider's left stick action so it can actually move the player
        if (moveProvider != null)
        {
            EnableReader(moveProvider.leftHandMoveInput);
        }

        if (climbProvider != null)
        {
            climbProvider.locomotionStarted += OnClimbStarted;
            climbProvider.locomotionEnded += OnClimbEnded;
        }
    }

    void OnDisable()
    {
        if (climbProvider != null)
        {
            climbProvider.locomotionStarted -= OnClimbStarted;
            climbProvider.locomotionEnded -= OnClimbEnded;
        }
    }

    void EnableAction(InputActionReference reference)
    {
        if (reference != null && reference.action != null)
        {
            reference.action.Enable();
        }
    }

    void EnableReader(XRInputValueReader<Vector2> reader)
    {
        if (reader == null)
        {
            return;
        }

        // Only Action Reference mode needs enabling here. "Input Action" (direct) mode
        // is enabled by XRI itself, and other modes don't use actions.
        if (reader.inputSourceMode == XRInputValueReader.InputSourceMode.InputActionReference)
        {
            EnableAction(reader.inputActionReference);
        }
    }

    // Grabbed a climbable: throw away any fall speed built up before the grab
    void OnClimbStarted(LocomotionProvider provider)
    {
        verticalVelocity = 0f;
    }

    // Let go: start falling from rest instead of with stale velocity
    // (Later, for fling/launch, set this to the rig's upward release velocity instead.)
    void OnClimbEnded(LocomotionProvider provider)
    {
        verticalVelocity = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        ReadControllerInputs();
        HandleGravity();
    }

    void ReadControllerInputs()
    {
        LeftTriggerValue = LeftTrigger.action.ReadValue<float>();
        RightTriggerValue = RightTrigger.action.ReadValue<float>();
        LeftGripValue = LeftGrip.action.ReadValue<float>();
        RightGripValue = RightGrip.action.ReadValue<float>();
        Avalue = AButton.action.ReadValue<float>();
        Bvalue = BButton.action.ReadValue<float>();
        rightJoystickValue = rightJoystick.action.ReadValue<Vector2>();

        // Same stick the move provider uses, so other scripts can still read it
        leftJoystickValue = moveProvider != null ? moveProvider.leftHandMoveInput.ReadValue() : Vector2.zero;
    }

    void HandleGravity()
    {
        if (characterController == null)
        {
            return;
        }

        // While climbing, the Climb Provider owns the rig's movement. No gravity while gripping.
        if (IsClimbing)
        {
            verticalVelocity = 0f;
            return;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce; // small push to keep it grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
}