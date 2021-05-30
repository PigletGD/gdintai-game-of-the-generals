using UnityEngine;

[System.Serializable]
public struct AbsolutePieceInfo
{
    public int pieceIndex;
    public Vector2 position;
}

[CreateAssetMenu(fileName = "Data", menuName = "Formations", order = 1)]
public class BoardSetups: ScriptableObject
{
    public AbsolutePieceInfo[] AbsolutePiecePositions;

    public int[] FrontLeft;
    public int[] FrontRight;
    public int[] MiddleLeft;
    public int[] MiddleRight;
    public int[] BackLeft;
    public int[] BackRight;
}