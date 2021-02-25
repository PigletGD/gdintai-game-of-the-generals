using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Formations", order = 1)]
public class Positions: ScriptableObject
{
    public Vector2[] PiecePositions;
}
