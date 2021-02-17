using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator
{
    public BoardState toEvaluate = null;

    public static float OFFENSIVE_MULTIPLIER = 2.0f;
    public static float DEFENSE_DEDUCTION = 3.0f;
    public static float OPENNESS_VALUE = 5.0f;
    public static float WIN_LOSS_VALUE = 99999.0f;

    public BoardEvaluator()
    {
        toEvaluate = null;
    }

    public void Evaluate(BoardState board)
    {
        toEvaluate = board;

        toEvaluate.ResetInfo();

        float computedScore = 0;

        List<Piece> pieces;
        if (!toEvaluate.playerTurn) pieces = toEvaluate.alivePlayerPieces;
        else pieces = toEvaluate.aliveComputerPieces;

        foreach (Piece piece in pieces)
        {
            float pieceScore = ComputeScore(piece);
            computedScore += pieceScore;
        }

        if (toEvaluate.playerTurn && FlagIsAtRisk()) computedScore = -WIN_LOSS_VALUE;

        toEvaluate.evaluationScore = computedScore;
    }

    public float ComputeScore(Piece computingPiece)
    {
        float forwardValue;

        if (computingPiece.playerPiece)
            forwardValue = computingPiece.yCoord;
        else
            forwardValue = (7 - computingPiece.yCoord);

        int numAdjacentPieces = 0;
        float numOpenSpaces = 0;

        int x = computingPiece.xCoord;
        int y = computingPiece.yCoord;

        x++;
        if (x < 9)
        {
            if (toEvaluate.board[x, y] != null)
            {
                if (toEvaluate.board[x, y].playerPiece != computingPiece.playerPiece)
                    numAdjacentPieces++;
            }
            else numOpenSpaces++;
        }

        x -= 2;
        if (x >= 0)
        {
            if (toEvaluate.board[x, y] != null)
            {
                if (toEvaluate.board[x, y].playerPiece != computingPiece.playerPiece)
                    numAdjacentPieces++;
            }
            else numOpenSpaces++;
        }

        x++;
        y++;
        if (y < 8)
        {
            if (toEvaluate.board[x, y] != null)
            {
                if (toEvaluate.board[x, y].playerPiece != computingPiece.playerPiece)
                    numAdjacentPieces++;
            }
            else numOpenSpaces++;
        }

        y -= 2;
        if (y >= 0)
        {
            if (toEvaluate.board[x, y] != null)
            {
                if (toEvaluate.board[x, y].playerPiece != computingPiece.playerPiece)
                    numAdjacentPieces++;
            }
            else numOpenSpaces++;
        }

        float offensiveness = !toEvaluate.playerTurn ? 0 : ComputeOffensiveness(computingPiece.pieceValue, numAdjacentPieces, forwardValue);
        float openess = ComputeOpeness(computingPiece.pieceValue, numOpenSpaces);
        float defensiveness = ComputeDefensiveness(computingPiece.pieceValue, numAdjacentPieces);

        float pieceScore = offensiveness + openess - defensiveness;

        return pieceScore;
    }

    public float ComputeOffensiveness(float pieceValue, float numAdjacentPieces, float forwardValue)
    {
        float offensiveScore = pieceValue * numAdjacentPieces * forwardValue;

        return offensiveScore;
    }

    public float ComputeOpeness(float pieceValue, float numOpenSpaces)
    {
        float openessScore = pieceValue * OPENNESS_VALUE * numOpenSpaces;

        return openessScore;
    }

    public float ComputeDefensiveness(float pieceValue, float numAdjacentPieces)
    {
        float defensiveScore = pieceValue - (DEFENSE_DEDUCTION * numAdjacentPieces);

        return defensiveScore;
    }

    public bool FlagIsAtRisk()
    {
        int x = toEvaluate.computerFlag.xCoord;
        int y = toEvaluate.computerFlag.yCoord;

        string key;

        x++;
        if (x < 9)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != toEvaluate.computerFlag.playerPiece)
                    return true;


        x -= 2;
        if (x >= 0)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != toEvaluate.computerFlag.playerPiece)
                    return true;

        x++;
        y++;
        if (y < 8)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != toEvaluate.computerFlag.playerPiece)
                    return true;

        y -= 2;
        if (y >= 0)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != toEvaluate.computerFlag.playerPiece)
                    return true;

        return false;
    }
}