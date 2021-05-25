﻿using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator
{
    public BoardState toEvaluate = null;

    public static float OFFENSIVE_MULTIPLIER = 7.0f;
    public static float DEFENSE_DEDUCTION = 3.0f;
    public static float OPENNESS_VALUE = 5.0f;
    public static float ADVANTAGE_BIAS = 50.0f;
    public static float WIN_LOSS_VALUE = 9999.0f;

    public BoardEvaluator()
    {
        toEvaluate = null;
    }

    public void Evaluate(BoardState board)
    {
        toEvaluate = board;

        toEvaluate.ResetInfo();

        if (FlagIsAtRisk())
        {
            toEvaluate.evaluationScore = -WIN_LOSS_VALUE;
            return;
        }
        else if (EnemyFlagCaptured())
        {
            toEvaluate.evaluationScore = WIN_LOSS_VALUE;
            return;
        }

        float computedScore = 0;

        bool flagMissing = true;

        List<Piece> pieces;
        List<Piece> enemies;
        if (!toEvaluate.playerTurn)
        {
            pieces = toEvaluate.alivePlayerPieces;
            enemies = toEvaluate.aliveComputerPieces;
        }
        else
        {
            pieces = toEvaluate.aliveComputerPieces;
            enemies = toEvaluate.alivePlayerPieces;
        }

        foreach (Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
                flagMissing = false;

            float pieceScore = ComputeScore(piece);
            computedScore += pieceScore;
        }

        computedScore /= pieces.Count;

        if (flagMissing) computedScore = -WIN_LOSS_VALUE;

        //computedScore += (pieces.Count - enemies.Count) * ADVANTAGE_BIAS;
        toEvaluate.evaluationScore = computedScore;
    }

    public float ComputeScore(Piece computingPiece)
    {
        float forwardValue;

        if (computingPiece.playerPiece) forwardValue = computingPiece.yCoord;
        else forwardValue = (7 - computingPiece.yCoord);

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
        float offensiveScore = pieceValue * OFFENSIVE_MULTIPLIER * numAdjacentPieces * forwardValue;

        return offensiveScore;
    }

    public float ComputeOpeness(float pieceValue, float numOpenSpaces)
    {
        float openessScore = pieceValue * OPENNESS_VALUE * numOpenSpaces;

        return openessScore;
    }

    public float ComputeDefensiveness(float pieceValue, float numAdjacentPieces)
    {
        float defensiveScore = ((3.5f - pieceValue) / 3.5f) * (DEFENSE_DEDUCTION * numAdjacentPieces);

        return defensiveScore;
    }

    public bool FlagIsAtRisk()
    {
        Piece flag;
        if (!toEvaluate.playerTurn) flag = toEvaluate.playerFlag;
        else flag = toEvaluate.computerFlag;

        int x = flag.xCoord;
        int y = flag.yCoord;

        string key;

        x++;
        if (x < 9)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != flag.playerPiece)
                    return true;


        x -= 2;
        if (x >= 0)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != flag.playerPiece)
                    return true;

        x++;
        y++;
        if (y < 8)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != flag.playerPiece)
                    return true;

        y -= 2;
        if (y >= 0)
            if (toEvaluate.board[x, y] != null)
                if (toEvaluate.board[x, y].playerPiece != flag.playerPiece)
                    return true;

        return false;
    }

    private bool EnemyFlagCaptured()
    {
        List<Piece> pieces;
        if (toEvaluate.playerTurn) pieces = toEvaluate.alivePlayerPieces;
        else pieces = toEvaluate.aliveComputerPieces;

        foreach (Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
                return false;
        }

        return true;
    }
}