using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Dictionary<string, Piece> board = new Dictionary<string, Piece>();
    public List<Piece> alivePlayerPieces = new List<Piece>();
    public List<Piece> aliveComputerPieces = new List<Piece>();
    public List<Piece> deadPlayerPieces = new List<Piece>();
    public List<Piece> deadComputerPieces = new List<Piece>();

    public List<Move> validPlayerMoves = new List<Move>();
    public List<Move> validComputerMoves = new List<Move>();

    [SerializeField] private SpriteRenderer SR = null;

    [SerializeField] private GameManager GM = null;

    [SerializeField] private GameEventsSO onTurnEnd = null;

    private float spriteWidth = 0.0f;
    private float spriteHeight = 0.0f;

    private float tileWidth = 0.0f;
    private float tileHeight = 0.0f;

    private int boardWidth = 9;
    private int boardHeight = 8;

    [SerializeField] private GameEventsSO onAllPlayerPiecesOnBoard = null;

    [SerializeField] private GameObject testObject = null;

    private bool runningMCTS = false;

    private void Start()
    {
        Vector3 extents = SR.bounds.extents;

        spriteWidth = extents.x * 2.0f;
        spriteHeight = extents.y * 2.0f;

        tileWidth = spriteWidth / boardWidth;
        tileHeight = spriteHeight / boardHeight;
    }

    public void SetupBoard()
    {
        //SetupBoardRandom();

        InitializeValidMoves();
    }

    // TEMP
    public void SetupBoardRandom()
    {
        foreach (Piece piece in aliveComputerPieces)
        {
            Vector2 randomTileCoord;

            do
            {
                randomTileCoord = new Vector2(Random.Range(0, 9), Random.Range(5, 8));
            } while (board.ContainsKey(GenerateKey(randomTileCoord)));

            PlacePiece(randomTileCoord, piece);

            piece.gameObject.SetActive(true);
        }
    }

    public void PlacePiece(Vector3 mousePos, Piece selectedPiece)
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Getting Coordinates
        int mouseX = Mathf.FloorToInt(((mousePos.x + (spriteWidth * 0.5f)) / spriteWidth) * boardWidth);
        int mouseY = Mathf.FloorToInt(((mousePos.y + (spriteHeight * 0.5f)) / spriteHeight) * boardHeight);

        if (mouseX == boardWidth) mouseX--;
        if (mouseY == boardHeight) mouseY--;

        Vector2 tileCoordinates = new Vector2(mouseX, mouseY);

        PlacePiece(tileCoordinates, selectedPiece);     
    }

    public void PlacePiece(Vector2 tileCoordinates, Piece selectedPiece)
    {
        // Set Tile Info
        if (!board.ContainsKey(GenerateKey(tileCoordinates)))
        {
            if (TileIsValid(tileCoordinates, selectedPiece))
            {
                SetTileInfo(tileCoordinates, selectedPiece);
                //if (GM.playerTurn && !GM.gameOver) onTurnEnd.Raise();
            }
        }
        else
        {
            if (GM.isSettingUp)
            {
                TileSwap(tileCoordinates, selectedPiece);
                //if (GM.playerTurn && !GM.gameOver) onTurnEnd.Raise();
            }
            else
            {
                PieceContest(selectedPiece, board[GenerateKey(tileCoordinates)]);
                //if (GM.playerTurn && !GM.gameOver) onTurnEnd.Raise();
            }

        }
    }

    private bool TileIsValid(Vector2 tileCoordinates, Piece selectedPiece)
    {
        if (GM.isSettingUp)
        {
            if (selectedPiece.playerPiece && tileCoordinates.y >= 0 && tileCoordinates.y <= 2)
                return true;
            else if (!selectedPiece.playerPiece && tileCoordinates.y >= 5 && tileCoordinates.y <= 7)
                return true;
        }
        else
        {
            if (board.ContainsKey(GenerateKey(tileCoordinates)))
            {
                if (board[GenerateKey(tileCoordinates)].playerPiece != selectedPiece.playerPiece)
                {
                    int differenceX = (int)Mathf.Abs(tileCoordinates.x - selectedPiece.tileCoordinates.x);
                    int differenceY = (int)Mathf.Abs(tileCoordinates.y - selectedPiece.tileCoordinates.y);
                    int difference = differenceX + differenceY;

                    if (tileCoordinates.x >= 0 && tileCoordinates.x < boardWidth &&
                        tileCoordinates.y >= 0 && tileCoordinates.y < boardHeight &&
                        difference == 1)
                        return true;
                }
            }
            else
            {
                int differenceX = (int)Mathf.Abs(tileCoordinates.x - selectedPiece.tileCoordinates.x);
                int differenceY = (int)Mathf.Abs(tileCoordinates.y - selectedPiece.tileCoordinates.y);
                int difference = differenceX + differenceY;

                if (tileCoordinates.x >= 0 && tileCoordinates.x < boardWidth &&
                    tileCoordinates.y >= 0 && tileCoordinates.y < boardHeight &&
                    difference == 1)
                    return true;
            }
            
        }

        return false;
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

    private void SetTileInfo(Vector2 tileCoordinates, Piece value)
    {
        if (value.tileCoordinates.x >= 0)
            board.Remove(GenerateKey(value.tileCoordinates));

        Vector3 newPosition = GetCenterTilePosition(tileCoordinates);

        value.transform.position = newPosition;
        value.lastPosition = newPosition;
        value.tileCoordinates = tileCoordinates;

        board.Add(GenerateKey(tileCoordinates), value);

        if (GM.isSettingUp && board.Count >= 1) onAllPlayerPiecesOnBoard.Raise();
    }

    public string GenerateKey(Vector2 tileCoordinates)
    {
        return $"{tileCoordinates.x} {tileCoordinates.y}";
    }

    private Vector3 GetCenterTilePosition(Vector2 tileCoordinates)
    {
        Vector3 centerPosition = Vector3.zero;

        centerPosition.x = ((-spriteWidth * 0.5f) + (tileWidth * 0.5f)) + (tileWidth * tileCoordinates.x);
        centerPosition.y = ((-spriteHeight * 0.5f) + (tileHeight * 0.5f)) + (tileHeight * tileCoordinates.y);

        return centerPosition;
    }

    private void PieceContest(Piece attackingPiece, Piece defendingPiece)
    {
        if (attackingPiece.playerPiece != defendingPiece.playerPiece && TileIsValid(defendingPiece.tileCoordinates, attackingPiece))
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
        PlaceDeadPiece(defendingPiece);

        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
        SetTileInfo(contestingTileCoords, attackingPiece);
    }

    private void DefenderWins(Piece attackingPiece)
    {
        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
        PlaceDeadPiece(attackingPiece);
    }

    private void SplitLoss(Piece attackingPiece, Piece defendingPiece)
    {
        board.Remove(GenerateKey(attackingPiece.tileCoordinates));
        PlaceDeadPiece(attackingPiece);

        board.Remove(GenerateKey(defendingPiece.tileCoordinates));
        PlaceDeadPiece(defendingPiece);
    }

    public void PlaceDeadPiece(Piece deadPiece)
    {
        deadPiece.isDead = true;

        if (deadPiece.playerPiece)
        {
            Vector3 deadPosition = Vector3.zero;

            int count = deadPlayerPieces.Count;
            int row = count / 3;
            int col = count % 3;

            if (col == 0) deadPosition.x = -8.3f;
            else if (col == 1) deadPosition.x = -7.2f;
            else deadPosition.x = -6.1f;

            deadPosition.y = 3.5f - row;

            deadPiece.transform.position = deadPosition;

            alivePlayerPieces.Remove(deadPiece);
            deadPlayerPieces.Add(deadPiece);
        }
        else
        {
            Vector3 deadPosition = Vector3.zero;

            int count = deadComputerPieces.Count;
            int row = count / 3;
            int col = count % 3;

            if (col == 0) deadPosition.x = 8.3f;
            else if (col == 1) deadPosition.x = 7.2f;
            else deadPosition.x = 6.1f;

            Debug.Log(col + " " + deadPosition.x);

            deadPosition.y = 3.5f - row;

            deadPiece.transform.position = deadPosition;

            aliveComputerPieces.Remove(deadPiece);
            deadComputerPieces.Add(deadPiece);
        }

        if (deadPiece.pieceType == PieceType.Flag) GM.ShowResults();
    }

    public void InitializeValidMoves()
    {
        InitializeValidMovesOfPieces(alivePlayerPieces);
        InitializeValidMovesOfPieces(aliveComputerPieces);
    }

    public void InitializeValidMovesOfPieces(List<Piece> pieces)
    {
        foreach(Piece piece in pieces)
        {
            Vector2 possibleCoords = piece.tileCoordinates;

            possibleCoords.x++;
            AddValidMove(validPlayerMoves, possibleCoords, piece);

            possibleCoords.x -= 2;
            AddValidMove(validPlayerMoves, possibleCoords, piece);

            possibleCoords.x++;
            possibleCoords.y++;
            AddValidMove(validPlayerMoves, possibleCoords, piece);

            possibleCoords.y -= 2;
            AddValidMove(validPlayerMoves, possibleCoords, piece);
        }
    }

    public void AddValidMove(List<Move> validMoves, Vector2 possibleCoords, Piece piece)
    {
        if (TileIsValid(possibleCoords, piece))
        {
            string key = GenerateKey(possibleCoords);
            if (board.ContainsKey(key))
                validMoves.Add(new Move(null, piece, piece.tileCoordinates, possibleCoords, board[key]));
            else
                validMoves.Add(new Move(null, piece, piece.tileCoordinates, possibleCoords));

            Instantiate(testObject, GetCenterTilePosition(possibleCoords), Quaternion.identity);
        }
    }
}