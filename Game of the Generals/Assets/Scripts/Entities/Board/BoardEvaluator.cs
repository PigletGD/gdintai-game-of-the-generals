using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator
{
    public BoardState toEvaluate = null;

    public static float OFFENSIVE_MULTIPLIER = 3.0f;
    public static float DEFENSE_DEDUCTION = 2.0f;
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
        float withinEnemyTerritory = 1;
        float forwardValue;

        if (computingPiece.playerPiece)
        {
            forwardValue = computingPiece.tileCoordinates.y * computingPiece.pieceValue * 0.5f;

            if (computingPiece.tileCoordinates.y > 3)
                withinEnemyTerritory = OFFENSIVE_MULTIPLIER;
        }
        else
        {
            forwardValue = (7 - computingPiece.tileCoordinates.y) * computingPiece.pieceValue * 0.5f;

            if (computingPiece.tileCoordinates.y < 4)
                withinEnemyTerritory = OFFENSIVE_MULTIPLIER;
        }

        int numAdjacentPieces = 0;
        float numOpenSpaces = 0;

        Vector2 pieceCoordinates = computingPiece.tileCoordinates;

        string key;

        pieceCoordinates.x++;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
        {
            if (toEvaluate.board[key].playerPiece != computingPiece.playerPiece)
                numAdjacentPieces++;
        }
        else numOpenSpaces++;


        pieceCoordinates.x -= 2;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
        {
            if (toEvaluate.board[key].playerPiece != computingPiece.playerPiece)
                numAdjacentPieces++;
        }
        else numOpenSpaces++;

        pieceCoordinates.x++;
        pieceCoordinates.y++;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
        {
            if (toEvaluate.board[key].playerPiece != computingPiece.playerPiece)
                numAdjacentPieces++;
        }
        else numOpenSpaces++;

        pieceCoordinates.y -= 2;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
        {
            if (toEvaluate.board[key].playerPiece != computingPiece.playerPiece)
                numAdjacentPieces++;
        }
        else numOpenSpaces++;

        float offensiveness = !toEvaluate.playerTurn ? 0 : ComputeOffensiveness(computingPiece.pieceValue, withinEnemyTerritory, numAdjacentPieces, forwardValue);
        float openess = ComputeOpeness(computingPiece.pieceValue, numOpenSpaces);
        float defensiveness = ComputeDefensiveness(computingPiece.pieceValue, numAdjacentPieces);

        float pieceScore = offensiveness + openess - defensiveness;

        return pieceScore;
    }

    public float ComputeOffensiveness(float pieceValue, float withinEnemyTerritory, float numAdjacentPieces, float forwardValue)
    {
        float offensiveScore = (pieceValue * withinEnemyTerritory * numAdjacentPieces) + forwardValue;

        return offensiveScore;
    }

    public float ComputeOpeness(float pieceValue, float numOpenSpaces)
    {
        float openessScore = (pieceValue * (4 - numOpenSpaces)) + (OPENNESS_VALUE * numOpenSpaces);

        return openessScore;
    }

    public float ComputeDefensiveness(float pieceValue, float numAdjacentPieces)
    {
        float defensiveScore = pieceValue - (DEFENSE_DEDUCTION * numAdjacentPieces);

        return defensiveScore;
    }

    public bool FlagIsAtRisk()
    {
        Vector2 pieceCoordinates = toEvaluate.computerFlag.tileCoordinates;

        string key;

        pieceCoordinates.x++;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
            if (toEvaluate.board[key].playerPiece != toEvaluate.computerFlag.playerPiece)
                return true;

        pieceCoordinates.x -= 2;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
            if (toEvaluate.board[key].playerPiece != toEvaluate.computerFlag.playerPiece)
                return true;

        pieceCoordinates.x++;
        pieceCoordinates.y++;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
            if (toEvaluate.board[key].playerPiece != toEvaluate.computerFlag.playerPiece)
                return true;

        pieceCoordinates.y -= 2;
        key = toEvaluate.GenerateKey(pieceCoordinates);
        if (toEvaluate.board.ContainsKey(key))
            if (toEvaluate.board[key].playerPiece != toEvaluate.computerFlag.playerPiece)
                return true;

        return false;
    }
}