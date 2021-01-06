using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public Move parentMove;
    public List<Move> moves;
    public Piece movingPiece;
    public Vector2 previousCoords;
    public Piece occupyingPiece;
    public Vector2 desiredCoords;
    public float evaluationScore;
    public int timesVisited;

    public Move(Move newParent, Piece newMovingPiece, Vector2 newPrevious, Vector2 newDesired, Piece newOccupying = null)
    {
        parentMove = newParent;
        moves = new List<Move>();
        movingPiece = newMovingPiece;
        previousCoords = newPrevious;
        occupyingPiece = newOccupying;
        desiredCoords = newDesired;
        evaluationScore = 0;
        timesVisited = 0;
    }
}