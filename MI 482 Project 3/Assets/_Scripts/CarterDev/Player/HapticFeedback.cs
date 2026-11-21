using UnityEngine;
using UnityEngine.XR;

public class HapticFeedback: MonoBehaviour
{
    static public HapticFeedback Instance;

    void Awake()
    {
        Instance = this;
    }
    public void PulseLeft(float amplitude = 0.5f, float duration = 0.1f) =>
        Pulse(XRNode.LeftHand, amplitude, duration);

    public void PulseRight(float amplitude = 0.5f, float duration = 0.1f) =>
        Pulse(XRNode.RightHand, amplitude, duration);

    private void Pulse(XRNode node, float amplitude, float duration)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        // amplitude: 0..1, duration: seconds
        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            // channel is almost always 0 on Touch controllers
            device.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), Mathf.Max(0f, duration));
        }
    }
}
