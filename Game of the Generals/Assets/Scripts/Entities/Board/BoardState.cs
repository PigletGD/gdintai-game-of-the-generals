﻿using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public BoardState parentBoard;
    public BoardState realBoard;
    public List<BoardState> childrenBoard;

    public Piece[,] board;
    public List<Piece> alivePlayerPieces;
    public List<Piece> aliveComputerPieces;
    public List<Piece> deadPlayerPieces;
    public List<Piece> deadComputerPieces;
    public Piece playerFlag;
    public Piece computerFlag;
    public List<int> playerKills;
    public List<int> computerKills;

    public bool playerTurn;
    public float evaluationScore;
    public int winCount;
    public int timesVisited;

    public bool atTerminalState;
    public bool playerWon;
    public BoardState(BoardState newParent, List<BoardState> newChildren, Piece[,] newBoard, List<Piece> newPlayers, List<Piece> newComputer, List<Piece> newDeadPlayers, List<Piece> newDeadComputer, Piece newPlayerFlag, Piece newComputerFlag, List<int> newPlayerKills, List<int> newComputerKills, bool newTurn)
    {
        parentBoard = newParent;
        childrenBoard = new List<BoardState>(newChildren);

        board = new Piece[9, 8];

        if (newBoard != null)
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 8; y++)
                    board[x, y] = newBoard[x, y];
        }
        else
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 8; y++)
                    board[x, y] = null;
        }

        alivePlayerPieces = new List<Piece>(newPlayers);
        aliveComputerPieces = new List<Piece>(newComputer);
        deadPlayerPieces = new List<Piece>(newDeadPlayers);
        deadComputerPieces = new List<Piece>(newDeadComputer);
        playerFlag = newPlayerFlag;
        computerFlag = newComputerFlag;
        playerTurn = newTurn;
        playerKills = new List<int>(newPlayerKills);
        computerKills = new List<int>(newComputerKills);
        evaluationScore = 0.0f;
        atTerminalState = false;
        playerWon = false;
    }

    public BoardState GenerateChildOfBoardState()
    {
        BoardState child = new BoardState(this, new List<BoardState>(), board, alivePlayerPieces, aliveComputerPieces, deadPlayerPieces, deadComputerPieces, playerFlag, computerFlag, playerKills, computerKills, !playerTurn);
        //childrenBoard.Add(child);
        return child;
    }

    public BoardState GenerateCopyOfBoardState()
    {
        return new BoardState(parentBoard, new List<BoardState>(), board, alivePlayerPieces, aliveComputerPieces, deadPlayerPieces, deadComputerPieces, playerFlag, computerFlag, playerKills, computerKills, !playerTurn);
    }

    public void SetupBoardRandomPlayer()
    {
        foreach (Piece piece in alivePlayerPieces)
        {
            if (piece.xCoord < 0)
            {
                int randX;
                int randY;

                do
                {
                    randX = Random.Range(0, 9);
                    randY = Random.Range(0, 3);
                } while (board[randX, randY] != null);

                PlacePiece(randX, randY, piece, true);
            }
        }
    }

    // TEMP
    public void SetupBoardRandomComputer()
    {
        foreach (Piece piece in aliveComputerPieces)
        {
            int randX;
            int randY;

            do
            {
                randX = Random.Range(0, 9);
                randY = Random.Range(5, 8);
            } while (board[randX, randY] != null);

            PlacePiece(randX, randY, piece, true);

            piece.gameObject.SetActive(true);
        }
    }

    public void SetupBoardComputer(BoardSetups setup)
    {
        int xPos, yPos;
        bool flipped = Random.value > 0.5f ? true : false;


        foreach (AbsolutePieceInfo api in setup.AbsolutePiecePositions)
        {
            xPos = flipped ? 8 - (int)api.position.x : (int)api.position.x;
            yPos = (int)api.position.y;

            PlacePiece(xPos, yPos, aliveComputerPieces[api.pieceIndex], true);

            aliveComputerPieces[api.pieceIndex].gameObject.SetActive(true);
        }

        int leftMin = flipped ? 4 : 0;
        int leftMax = flipped ? 9 : 5;
        int rightMin = flipped ? 0 : 5;
        int rightMax = flipped ? 4 : 9;

        SetComputerPiecesOnSide(setup.FrontLeft, leftMin, leftMax, 5);
        SetComputerPiecesOnSide(setup.FrontRight, rightMin, rightMax, 5);
        SetComputerPiecesOnSide(setup.MiddleLeft, leftMin, leftMax, 6);
        SetComputerPiecesOnSide(setup.MiddleRight, rightMin, rightMax, 6);
        SetComputerPiecesOnSide(setup.BackLeft, leftMin, leftMax, 7);
        SetComputerPiecesOnSide(setup.BackRight, rightMin, rightMax, 7);
    }

    void SetComputerPiecesOnSide(int[] side, int minX, int maxX, int yPos){
        int rand;

        for (int i = 0; i < side.Length; i++)
        {
            do
            {
                rand = Random.Range(minX, maxX);
            } while (board[rand, yPos] != null);

            int xPos = rand;

            PlacePiece(xPos, yPos, aliveComputerPieces[side[i]], true);

            aliveComputerPieces[side[i]].gameObject.SetActive(true);
        }
    }

    public void PlacePiece(int x, int y, Piece selectedPiece, bool settingUp)
    {

        // Set Tile Info
        if (board[x, y] == null)
            SetTileInfo(x, y, selectedPiece);
        else
        {
            if (settingUp) TileSwap(x, y, selectedPiece);
            else PieceContest(selectedPiece, board[x, y]);
        }
    }

    public bool HypotheticalPlacePiece(int x, int y, Piece selectedPiece)
    {

        // Set Tile Info
        if (board[x, y] == null)
        {
            SetTileInfo(x, y, selectedPiece);
            return true;
        }
        else
            return HypotheticalPieceContest(selectedPiece, board[x, y]);
    }

    public bool TileIsValid(int x, int y, Piece selectedPiece, bool settingUp)
    {
        if (settingUp) return TileIsValidSetup(x, y, selectedPiece);
        else return TileIsValidInGame(x, y, selectedPiece);
    }

    private bool TileIsValidSetup(int x, int y, Piece selectedPiece)
    {
        if (selectedPiece.playerPiece && y >= 0 && y <= 2)
            return true;
        else if (!selectedPiece.playerPiece && y >= 5 && y <= 7)
            return true;
        else
            return false;
    }

    private bool TileIsValidInGame(int x, int y, Piece selectedPiece)
    {
        if (x >= 0 && x < 9 && y >= 0 && y < 8)
        {
            if (board[x, y] != null)
            {
                if (board[x, y].playerPiece != selectedPiece.playerPiece)
                {
                    int differenceX = (int)Mathf.Abs(x - selectedPiece.xCoord);
                    int differenceY = (int)Mathf.Abs(y - selectedPiece.yCoord);
                    int difference = differenceX + differenceY;

                    if (difference == 1) return true;
                    else return false;
                }
                else return false;
            }
            else
            {
                int differenceX = (int)Mathf.Abs(x - selectedPiece.xCoord);
                int differenceY = (int)Mathf.Abs(y - selectedPiece.yCoord);
                int difference = differenceX + differenceY;

                if (difference == 1) return true;
                else return false;
            }
        }
        else return false;
    }

    private void TileSwap(int x, int y, Piece selectedPiece)
    {
        // Gets piece already on board
        Piece occupiedPiece = board[x, y];

        // Places occupied piece in selected piece's position
        if (selectedPiece.xCoord < 0)
        {
            occupiedPiece.xCoord = selectedPiece.xCoord;
            occupiedPiece.yCoord = selectedPiece.yCoord;
            occupiedPiece.transform.position = occupiedPiece.lastPosition = selectedPiece.lastPosition;
        }
        else
        {
            board[selectedPiece.xCoord, selectedPiece.yCoord] = null;
            occupiedPiece.transform.position = occupiedPiece.lastPosition = selectedPiece.lastPosition;
            SetTileInfo(selectedPiece.xCoord, selectedPiece.yCoord, occupiedPiece);
        }

        // Places selected piece in occupied piece's position
        selectedPiece.xCoord = x;
        selectedPiece.yCoord = y;
        board[x, y] = selectedPiece;
    }

    private void PieceContest(Piece attackingPiece, Piece defendingPiece)
    {
        if (attackingPiece.playerPiece != defendingPiece.playerPiece && TileIsValid(defendingPiece.xCoord, defendingPiece.yCoord, attackingPiece, false))
        {
            switch (GetContestResult(attackingPiece.pieceType, defendingPiece.pieceType))
            {
                case 0: SplitLoss(attackingPiece, defendingPiece); break;
                case 1: AttackerWins(attackingPiece, defendingPiece);
                    if (attackingPiece.playerPiece) playerKills[attackingPiece.pieceID]++;
                    else computerKills[attackingPiece.pieceID]++;
                    break;
                case 2: DefenderWins(attackingPiece);
                    if (defendingPiece.playerPiece) playerKills[defendingPiece.pieceID]++;
                    else computerKills[defendingPiece.pieceID]++;
                    break;
            }
        }
    }

    private bool HypotheticalPieceContest(Piece attackingPiece, Piece defendingPiece)
    {
        if (defendingPiece.pieceType == PieceType.Flag)
        {
            AttackerWins(attackingPiece, defendingPiece);
            return true;
        }

        List<Piece> pieceList; List<Piece> enemyList; List<int> killList;
        PieceType randomPieceType;

        if (attackingPiece.playerPiece)
        {
            pieceList = alivePlayerPieces;
            enemyList = aliveComputerPieces;
            killList = computerKills;
        }
        else
        {
            pieceList = aliveComputerPieces;
            enemyList = alivePlayerPieces;
            killList = playerKills;
        }

        int highestRank = -1;
        for (int i = 0; i < pieceList.Count; i++)
        {
            if (highestRank < (int)pieceList[i].pieceType)
                highestRank = (int)pieceList[i].pieceType;

            if (highestRank == 15) break;
        }

        int kills = killList[defendingPiece.pieceID];
        int minRank = (int)((highestRank / 3.0f) * (kills > 3 ? 3 : kills));

        randomPieceType = (PieceType)Random.Range(minRank, highestRank);

        int result = GetContestResult(attackingPiece.pieceType, randomPieceType);
        if (result == 0 || result == 1) return HypotheticalContest(attackingPiece, defendingPiece, randomPieceType);
        
        return false;
    }

    public bool HypotheticalContest(Piece attackingPiece, Piece defendingPiece, PieceType randomPieceType)
    {

        switch (GetContestResult(attackingPiece.pieceType, randomPieceType))
        {
            case 0: SplitLoss(attackingPiece, defendingPiece); return true;
            case 1: AttackerWins(attackingPiece, defendingPiece); return true;
            case 2: return false;
        }

        return false;
    }

    private int GetContestResult(PieceType attack, PieceType defense)
    {
        if ((attack > defense && (attack != PieceType.Spy || defense != PieceType.Private)) || (attack == PieceType.Private && defense == PieceType.Spy) || (attack == PieceType.Flag && defense == PieceType.Flag)) return 1;
        else if (attack == defense) return 0;
        else return 2;
    }

    private void AttackerWins(Piece attackingPiece, Piece defendingPiece)
    {
        board[defendingPiece.xCoord, defendingPiece.yCoord] = null;
        if (defendingPiece.playerPiece)
        {
            alivePlayerPieces.Remove(defendingPiece);
            deadPlayerPieces.Add(defendingPiece);
        }
        else
        {
            aliveComputerPieces.Remove(defendingPiece);
            deadComputerPieces.Add(defendingPiece);
        }
        defendingPiece.isDead = true;

        if (defendingPiece.pieceType == PieceType.Flag)
        {
            atTerminalState = true;
            playerWon = defendingPiece.playerPiece;
        }

        board[attackingPiece.xCoord, attackingPiece.yCoord] = null;
        SetTileInfo(defendingPiece.xCoord, defendingPiece.yCoord, attackingPiece);
    }

    private void DefenderWins(Piece attackingPiece)
    {
        board[attackingPiece.xCoord, attackingPiece.yCoord] = null;
        if (attackingPiece.playerPiece)
        {
            alivePlayerPieces.Remove(attackingPiece);
            deadPlayerPieces.Add(attackingPiece);
        }
        else
        {
            aliveComputerPieces.Remove(attackingPiece);
            deadComputerPieces.Add(attackingPiece);
        }
        attackingPiece.isDead = true;

        if (attackingPiece.pieceType == PieceType.Flag)
        {
            atTerminalState = true;
            playerWon = attackingPiece.playerPiece;
        }
    }

    private void SplitLoss(Piece attackingPiece, Piece defendingPiece)
    {
        board[attackingPiece.xCoord, attackingPiece.yCoord] = null;
        if (attackingPiece.playerPiece)
        {
            alivePlayerPieces.Remove(attackingPiece);
            deadPlayerPieces.Add(attackingPiece);
        }
        else
        {
            aliveComputerPieces.Remove(attackingPiece);
            deadComputerPieces.Add(attackingPiece);
        }
        attackingPiece.isDead = true;

        if (attackingPiece.pieceType == PieceType.Flag)
        {
            atTerminalState = true;
            playerWon = attackingPiece.playerPiece;
        }

        board[defendingPiece.xCoord, defendingPiece.yCoord] = null;
        if (defendingPiece.playerPiece)
        {
            alivePlayerPieces.Remove(defendingPiece);
            deadPlayerPieces.Add(defendingPiece);
        }
        else
        {
            aliveComputerPieces.Remove(defendingPiece);
            deadComputerPieces.Add(defendingPiece);
        }
        defendingPiece.isDead = true;

        if (defendingPiece.pieceType == PieceType.Flag)
        {
            atTerminalState = true;
            playerWon = defendingPiece.playerPiece;
        }

    }

    private void SetTileInfo(int x, int y, Piece value)
    {
        if (value.xCoord >= 0)
            board[value.xCoord, value.yCoord] = null;
        value.xCoord = x;
        value.yCoord = y;

        board[x, y] = value;
    }

    public void ResetInfo()
    {
        // reset it to actual values

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    board[x, y].xCoord = x;
                    board[x, y].yCoord = y;
                }
            }
        }

        foreach (Piece piece in alivePlayerPieces)
        {
            piece.isDead = false;
        }

        foreach (Piece piece in aliveComputerPieces)
        {
            piece.isDead = false;
        }

        foreach (Piece piece in deadPlayerPieces)
        {
            piece.isDead = true;
        }

        foreach (Piece piece in deadComputerPieces)
        {
            piece.isDead = true;
        }
    }

    public BoardState ComputerTurn()
    {
        AddAllPossibleFutureBoardStates();

        return GetRandomChildBoardState();
    }

    public void AddAllPossibleFutureBoardStates()
    {
        if (playerTurn) AddAllPossibleFutureBoardStates(alivePlayerPieces);
        else AddAllPossibleFutureBoardStates(aliveComputerPieces);
    }

    public void AddAllPossibleFutureBoardStates(List<Piece> pieces)
    {
        foreach (Piece piece in pieces)
        {
            int row = piece.xCoord;
            int column = piece.yCoord;

            if (TileIsValid(row + 1, column, piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                if (possibleBoardState.HypotheticalPlacePiece(row + 1, column, piece))
                {
                    ResetInfo();
                    possibleBoardState.realBoard = GenerateCopyOfBoardState();
                    possibleBoardState.realBoard.PlacePiece(row + 1, column, piece, false);
                    ResetInfo();
                    childrenBoard.Add(possibleBoardState);
                }
                else ResetInfo();
            }

            if (TileIsValid(row - 1, column, piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                if (possibleBoardState.HypotheticalPlacePiece(row - 1, column, piece))
                {
                    ResetInfo();
                    possibleBoardState.realBoard = GenerateCopyOfBoardState();
                    possibleBoardState.realBoard.PlacePiece(row - 1, column, piece, false);
                    ResetInfo();
                    childrenBoard.Add(possibleBoardState);
                }
                else ResetInfo();
            }

            if (TileIsValid(row, column + 1, piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                if (possibleBoardState.HypotheticalPlacePiece(row, column + 1, piece))
                {
                    ResetInfo();
                    possibleBoardState.realBoard = GenerateCopyOfBoardState();
                    possibleBoardState.realBoard.PlacePiece(row, column + 1, piece, false);
                    ResetInfo();
                    childrenBoard.Add(possibleBoardState);
                }
                else ResetInfo();
            }

            if (TileIsValid(row, column - 1, piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                if (possibleBoardState.HypotheticalPlacePiece(row, column - 1, piece))
                {
                    ResetInfo();
                    possibleBoardState.realBoard = GenerateCopyOfBoardState();
                    possibleBoardState.realBoard.PlacePiece(row, column - 1, piece, false);
                    ResetInfo();
                    childrenBoard.Add(possibleBoardState);
                }
                else ResetInfo();
            }
        }
    }

    public BoardState GetRandomChildBoardState()
    {
        int randomIndex = Random.Range(0, childrenBoard.Count);

        return childrenBoard[randomIndex];
    }

    public BoardState GetRandomFutureBoardState()
    {
        if (playerTurn) return GetRandomFutureBoardState(alivePlayerPieces);
        else return GetRandomFutureBoardState(aliveComputerPieces);
    }

    public BoardState GetRandomFutureBoardState(List<Piece> pieces)
    {
        int randomIndex = Random.Range(0, pieces.Count);
        int indexPasses = 0;

        do
        {
            int direction = Random.Range(0, 4);
            int directionPasses = 0;
            do
            {
                int row = pieces[randomIndex].xCoord;
                int column = pieces[randomIndex].yCoord;

                switch (direction)
                {
                    case 0:
                        if (TileIsValid(row + 1, column, pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(row + 1, column, pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 1:
                        if (TileIsValid(row - 1, column, pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(row - 1, column, pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 2:
                        if (TileIsValid(row, column + 1, pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(row, column + 1, pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 3:
                        if (TileIsValid(row, column - 1, pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(row, column - 1, pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                }

                direction++;
                if (direction >= 4)
                    direction = 0;

                directionPasses++;
            } while (directionPasses < 4);

            randomIndex++;
            if (randomIndex >= pieces.Count)
                randomIndex = 0;

            indexPasses++;
        } while (indexPasses < pieces.Count);

        return null;
    }

    public bool CheckIfFlagStillAlive()
    {
        List<Piece> pieces;
        if (playerTurn) pieces = deadPlayerPieces;
        else pieces = deadComputerPieces;

        foreach (Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
                return false;
        }

        return true;
    }

    public bool CheckIfFlagAtEnd()
    {
        List<Piece> pieces;
        if (playerTurn) pieces = alivePlayerPieces;
        else pieces = aliveComputerPieces;

        foreach (Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
            {
                if (piece.playerPiece && piece.yCoord == 7) return true;
                else if (!piece.playerPiece && piece.yCoord == 0) return true;
                else return false;
            }       
        }

        return false;
    }
}