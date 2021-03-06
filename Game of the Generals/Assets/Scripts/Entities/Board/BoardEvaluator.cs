﻿using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator
{
    public BoardState toEvaluate = null;

    private List<Piece> pieces = new List<Piece>();
    private List<Piece> enemies = new List<Piece>();
    private List<Piece> dead = new List<Piece>();

    private static float OFFENSIVE_MULTIPLIER = 20.0f;
    private static float FORWARD_MULTIPLIER = 12.0f;
    private static float CHASE_MULTIPLIER = 20.0f;
    private static float DEFENSE_DEDUCTION = 3.0f;
    private static float OPENNESS_VALUE = 2.0f;
    private static float ADVANTAGE_BIAS = 100.0f;
    private static float FREE_COLUMN_DEDUCTION = 30.0f;
    private static float WIN_LOSS_VALUE = 9999999.0f;
    private static float DISADVANTAGE_BIAS = 50.0f;

    private float freeColumns = 0;

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

        if (!toEvaluate.playerTurn)
        {
            pieces = toEvaluate.alivePlayerPieces;
            enemies = toEvaluate.aliveComputerPieces;
            dead = toEvaluate.deadPlayerPieces;
        }
        else
        {
            pieces = toEvaluate.aliveComputerPieces;
            enemies = toEvaluate.alivePlayerPieces;
            dead = toEvaluate.deadPlayerPieces;
        }

        AnalyzeBoard();

        foreach (Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
                flagMissing = false;

            float pieceScore = ComputeScore(piece);
            computedScore += pieceScore;
        }

        computedScore /= pieces.Count;
        computedScore += (pieces.Count - enemies.Count) * ADVANTAGE_BIAS;
        computedScore -= CalculateDisadvantage();
        computedScore -= freeColumns * FREE_COLUMN_DEDUCTION;

        if (flagMissing) computedScore = -WIN_LOSS_VALUE;
        toEvaluate.evaluationScore = computedScore;
    }

    public void AnalyzeBoard()
    {
        freeColumns = 0;
        bool playerTurn = toEvaluate.playerTurn;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (toEvaluate.board[i, j] != null &&
                    toEvaluate.board[i, j].playerPiece != playerTurn)
                {
                    break;
                }

                freeColumns++;
            }
        }
    }

    public float CalculateDisadvantage()
    {
        float sum = 0;
        for (int i = 0; i < dead.Count; i++)
            sum += dead[i].pieceValue;

        return sum * DISADVANTAGE_BIAS;
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
        float forwardness = computingPiece.pieceType == PieceType.Flag ? (pieces.Count > 7 ? 0 : ComputeForwardness(forwardValue)) : ComputeForwardness(forwardValue);
        float chasing = (pieces.Count <= 10 || enemies.Count <= 10) ? ComputeChase(computingPiece) : 0;
        float openess = ComputeOpeness(computingPiece.pieceValue, numOpenSpaces);
        float defensiveness = ComputeDefensiveness(computingPiece.pieceValue, numAdjacentPieces);

        float pieceScore = offensiveness + forwardness + openess - defensiveness - chasing;

        return pieceScore;
    }

    public float ComputeOffensiveness(float pieceValue, float numAdjacentPieces, float forwardValue)
    {
        float offensiveScore = pieceValue * OFFENSIVE_MULTIPLIER * numAdjacentPieces * forwardValue;

        return offensiveScore;
    }

    public float ComputeForwardness(float forwardValue)
    {
        float forwardScore = (forwardValue * FORWARD_MULTIPLIER) + (7 - forwardValue) * 10.0f;

        return forwardScore;
    }

    public float ComputeOpeness(float pieceValue, float numOpenSpaces)
    {
        float openessScore = pieceValue * OPENNESS_VALUE * numOpenSpaces;

        return openessScore;
    }

    public float ComputeChase(Piece computingPiece)
    {
        if (computingPiece.pieceType == PieceType.Flag) return 100;

        int closestDistance = 99999;
        for (int i = 1; i < enemies.Count; i++)
        {
            int x = Mathf.Abs(computingPiece.xCoord - enemies[i].xCoord);
            int y = Mathf.Abs(computingPiece.yCoord - enemies[i].yCoord);
            if (closestDistance < x + y)
                closestDistance = x + y;
        }

        return (computingPiece.pieceValue - closestDistance) * CHASE_MULTIPLIER;
    }

    public float ComputeDefensiveness(float pieceValue, float numAdjacentPieces)
    {
        float defensiveScore = ((DEFENSE_DEDUCTION * numAdjacentPieces) - pieceValue) * DEFENSE_DEDUCTION;

        return defensiveScore;
    }

    public bool FlagIsAtRisk()
    {
        Piece flag;
        if (!toEvaluate.playerTurn) flag = toEvaluate.playerFlag;
        else flag = toEvaluate.computerFlag;

        int x = flag.xCoord;
        int y = flag.yCoord;

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