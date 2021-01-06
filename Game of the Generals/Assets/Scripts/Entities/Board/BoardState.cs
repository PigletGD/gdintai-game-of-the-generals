using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public BoardState parentBoard;
    public List<BoardState> childrenBoard;

    public Dictionary<string, Piece> board;
    public List<Piece> alivePlayerPieces;
    public List<Piece> aliveComputerPieces;
    public List<Piece> deadPlayerPieces;
    public List<Piece> deadComputerPieces;
    public Piece playerFlag;
    public Piece computerFlag;

    public bool playerTurn;
    public float evaluationScore;
    public int winCount;
    public int timesVisited;

    public bool atTerminalState;
    public bool playerWon;

    public BoardState(BoardState newParent, List<BoardState> newChildren, Dictionary<string, Piece> newBoard, List<Piece> newPlayers, List<Piece> newComputer, List<Piece> newDeadPlayers, List<Piece> newDeadComputer, Piece newComputerFlag, bool newTurn)
    {
        parentBoard = newParent;
        childrenBoard = new List<BoardState>(newChildren);
        board = new Dictionary<string, Piece>(newBoard);
        alivePlayerPieces = new List<Piece>(newPlayers);
        aliveComputerPieces = new List<Piece>(newComputer);
        deadPlayerPieces = new List<Piece>(newDeadPlayers);
        deadComputerPieces = new List<Piece>(newDeadComputer);
        computerFlag = newComputerFlag;
        playerTurn = newTurn;
        evaluationScore = 0.0f;
        atTerminalState = false;
        playerWon = false;
    }

    public BoardState GenerateChildOfBoardState()
    {
        BoardState child = new BoardState(this, new List<BoardState>(), board, alivePlayerPieces, aliveComputerPieces, deadPlayerPieces, deadComputerPieces, computerFlag, !playerTurn);
        childrenBoard.Add(child);
        return child;
    }

    public void SetupBoardRandomPlayer()
    {
        foreach (Piece piece in alivePlayerPieces)
        {
            if (piece.tileCoordinates.x < 0)
            {
                Vector2 randomTileCoord;

                do
                {
                    randomTileCoord = new Vector2(Random.Range(0, 9), Random.Range(0, 3));
                } while (board.ContainsKey(GenerateKey(randomTileCoord)));

                PlacePiece(randomTileCoord, piece, true);
            }
        }
    }

    // TEMP
    public void SetupBoardRandomComputer()
    {
        foreach (Piece piece in aliveComputerPieces)
        {
            Vector2 randomTileCoord;

            do
            {
                randomTileCoord = new Vector2(Random.Range(0, 9), Random.Range(5, 8));
            } while (board.ContainsKey(GenerateKey(randomTileCoord)));

            PlacePiece(randomTileCoord, piece, true);

            piece.gameObject.SetActive(true);
        }
    }

    public void PlacePiece(Vector2 tileCoordinates, Piece selectedPiece, bool settingUp)
    {
        // Set Tile Info
        if (!board.ContainsKey(GenerateKey(tileCoordinates)))
            SetTileInfo(tileCoordinates, selectedPiece);
        else
        {
            if (settingUp) TileSwap(tileCoordinates, selectedPiece);
            else PieceContest(selectedPiece, board[GenerateKey(tileCoordinates)]);
        }
    }

    public bool TileIsValid(Vector2 tileCoordinates, Piece selectedPiece, bool settingUp)
    {
        if (settingUp) return TileIsValidSetup(tileCoordinates, selectedPiece);
        else return TileIsValidInGame(tileCoordinates, selectedPiece);
    }

    private bool TileIsValidSetup(Vector2 tileCoordinates, Piece selectedPiece)
    {
        if (selectedPiece.playerPiece && tileCoordinates.y >= 0 && tileCoordinates.y <= 2)
            return true;
        else if (!selectedPiece.playerPiece && tileCoordinates.y >= 5 && tileCoordinates.y <= 7)
            return true;
        else
            return false;
    }

    private bool TileIsValidInGame(Vector2 tileCoordinates, Piece selectedPiece)
    {
        if (tileCoordinates.x >= 0 && tileCoordinates.x < 9 && tileCoordinates.y >= 0 && tileCoordinates.y < 8)
        {
            if (board.ContainsKey(GenerateKey(tileCoordinates)))
            {
                if (board[GenerateKey(tileCoordinates)].playerPiece != selectedPiece.playerPiece)
                {
                    int differenceX = (int)Mathf.Abs(tileCoordinates.x - selectedPiece.tileCoordinates.x);
                    int differenceY = (int)Mathf.Abs(tileCoordinates.y - selectedPiece.tileCoordinates.y);
                    int difference = differenceX + differenceY;

                    if (difference == 1) return true;
                    else return false;
                }
                else return false;
            }
            else
            {
                int differenceX = (int)Mathf.Abs(tileCoordinates.x - selectedPiece.tileCoordinates.x);
                int differenceY = (int)Mathf.Abs(tileCoordinates.y - selectedPiece.tileCoordinates.y);
                int difference = differenceX + differenceY;

                if (difference == 1) return true;
                else return false;
            }
        }
        else return false;
    }

    private void TileSwap(Vector2 tileCoordinates, Piece selectedPiece)
    {
        // Gets piece already on board
        Piece occupiedPiece = board[GenerateKey(tileCoordinates)];

        // Places occupied piece in selected piece's position
        if (selectedPiece.tileCoordinates.x < 0)
        {
            occupiedPiece.tileCoordinates = selectedPiece.tileCoordinates;
            occupiedPiece.transform.position = occupiedPiece.lastPosition = selectedPiece.lastPosition;
        }
        else
        {
            board.Remove(GenerateKey(selectedPiece.tileCoordinates));
            SetTileInfo(selectedPiece.tileCoordinates, occupiedPiece);
        }

        // Places selected piece in occupied piece's position
        board.Remove(GenerateKey(tileCoordinates));
        SetTileInfo(tileCoordinates, selectedPiece);
    }

    private void PieceContest(Piece attackingPiece, Piece defendingPiece)
    {
        if (attackingPiece.playerPiece != defendingPiece.playerPiece && TileIsValid(defendingPiece.tileCoordinates, attackingPiece, false))
        {
            switch (GetContestResult(attackingPiece.pieceType, defendingPiece.pieceType))
            {
                case 0: SplitLoss(attackingPiece, defendingPiece); break;
                case 1: AttackerWins(attackingPiece, defendingPiece); break;
                case 2: DefenderWins(attackingPiece); break;
            }
        }
    }

    private int GetContestResult(PieceType attack, PieceType defense)
    {
        if ((attack > defense && (attack != PieceType.Spy || defense != PieceType.Private)) || (attack == PieceType.Private && defense == PieceType.Spy) || (attack == PieceType.Flag && defense == PieceType.Flag)) return 1;
        else if (attack == defense) return 0;
        else return 2;
    }

    private void AttackerWins(Piece attackingPiece, Piece defendingPiece)
    {
        Vector2 contestingTileCoords = defendingPiece.tileCoordinates;

        board.Remove(GenerateKey(contestingTileCoords));
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

        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
        SetTileInfo(contestingTileCoords, attackingPiece);
    }

    private void DefenderWins(Piece attackingPiece)
    {
        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
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
        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
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

        board.Remove(GenerateKey(defendingPiece.tileCoordinates));
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

    private void SetTileInfo(Vector2 tileCoordinates, Piece value)
    {
        if (value.tileCoordinates.x >= 0)
            board.Remove(GenerateKey(value.tileCoordinates));
        value.tileCoordinates = tileCoordinates;

        board.Add(GenerateKey(tileCoordinates), value);
    }

    public void ResetInfo()
    {
        foreach (string key in board.Keys)
        {
            board[key].tileCoordinates = ParseKey(key);
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

    public string GenerateKey(Vector2 tileCoordinates)
    {
        return $"{tileCoordinates.x} {tileCoordinates.y}";
    }

    private Vector2 ParseKey(string key)
    {
        Vector2 coord;
        string[] keyCoords;

        keyCoords = key.Split(' ');
        coord = new Vector2(int.Parse(keyCoords[0]), int.Parse(keyCoords[1]));

        return coord;
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
            float row = piece.tileCoordinates.x;
            float column = piece.tileCoordinates.y;

            if (TileIsValid(new Vector2(row + 1, column), piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                possibleBoardState.PlacePiece(new Vector2(row + 1, column), piece, false);
                childrenBoard.Add(possibleBoardState);
                ResetInfo();
            }

            if (TileIsValid(new Vector2(row - 1, column), piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                possibleBoardState.PlacePiece(new Vector2(row - 1, column), piece, false);
                childrenBoard.Add(possibleBoardState);
                ResetInfo();
            }

            if (TileIsValid(new Vector2(row, column + 1), piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                possibleBoardState.PlacePiece(new Vector2(row, column + 1), piece, false);
                childrenBoard.Add(possibleBoardState);
                ResetInfo();
            }

            if (TileIsValid(new Vector2(row, column - 1), piece, false))
            {
                BoardState possibleBoardState = this.GenerateChildOfBoardState();
                possibleBoardState.PlacePiece(new Vector2(row, column - 1), piece, false);
                childrenBoard.Add(possibleBoardState);
                ResetInfo();
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
                float row = pieces[randomIndex].tileCoordinates.x;
                float column = pieces[randomIndex].tileCoordinates.y;

                switch (direction)
                {
                    case 0:
                        if (TileIsValid(new Vector2(row + 1, column), pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(new Vector2(row + 1, column), pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 1:
                        if (TileIsValid(new Vector2(row - 1, column), pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(new Vector2(row - 1, column), pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 2:
                        if (TileIsValid(new Vector2(row, column + 1), pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(new Vector2(row, column + 1), pieces[randomIndex], false);
                            //ResetInfo();
                            return possibleBoardState;
                        }
                        break;
                    case 3:
                        if (TileIsValid(new Vector2(row, column - 1), pieces[randomIndex], false))
                        {
                            BoardState possibleBoardState = this.GenerateChildOfBoardState();
                            possibleBoardState.PlacePiece(new Vector2(row, column - 1), pieces[randomIndex], false);
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

        Debug.Log("Ran out of possible moves");

        return null;
    }
}