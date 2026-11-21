using UnityEngine;

public enum HandType
{
    Left,
    Right,
    Unknown
}

public class HandIdentifier : MonoBehaviour
{
    [Tooltip("Select which hand this interactor represents.")]
    public HandType hand = HandType.Unknown;
}