using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionReference triggerValue;
    public InputActionReference gripValue;

    public Animator handAnimator;
    


    private static readonly int TriggerHash = Animator.StringToHash("Trigger");
    private static readonly int GripHash = Animator.StringToHash("Grip");



    void Start()
    {

    }

    void Update()
    {

            float trigger = triggerValue.action.ReadValue<float>();
            float grip = gripValue.action.ReadValue<float>();

            handAnimator.SetFloat(TriggerHash, trigger);
            handAnimator.SetFloat(GripHash, grip);
        


    }
}