using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public BoardState currentBoard = null;
    BoardState rootBoard = null;

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

    public List<Piece> playerPieces;
    public List<Piece> computerPieces;

    public bool settingUp = true;

    private bool runningMCTS = false;

    private ComputerHandler CH = new ComputerHandler();

    // Start is called before the first frame update
    void Start()
    {
        InitializeBounds();

        GenerateInitialBoard();
    }

    public void ClickedSetupButton()
    {
        if (currentBoard.board.Count >= 21) StartGame();
        else RandomPlayerSetup();
    }

    private void RandomPlayerSetup()
    {
        currentBoard.SetupBoardRandomPlayer();

        SetPiecePositionsOnBoard(playerPieces);

        onAllPlayerPiecesOnBoard.Raise();
    }

    private void StartGame()
    {
        currentBoard.SetupBoardRandomComputer();

        SetAllPiecePositions();

        rootBoard = currentBoard;

        settingUp = false;

        GM.StartGame();
    }

    private void InitializeBounds()
    {
        Vector3 extents = SR.bounds.extents;

        spriteWidth = extents.x * 2.0f;
        spriteHeight = extents.y * 2.0f;

        tileWidth = spriteWidth / boardWidth;
        tileHeight = spriteHeight / boardHeight;
    }

    private void GenerateInitialBoard()
    {
        currentBoard = new BoardState(null, new List<BoardState>(),
                                      new Dictionary<string, Piece>(),
                                      playerPieces, computerPieces,
                                      new List<Piece>(), new List<Piece>(),
                                      SearchForFlag(computerPieces),
                                      true);
    }

    private Piece SearchForFlag(List<Piece> pieces)
    {
        foreach(Piece piece in pieces)
        {
            if (piece.pieceType == PieceType.Flag)
                return piece;
        }

        return null;
    }

    private void SetAllPiecePositions()
    {
        SetPiecePositionsOnBoard(currentBoard.alivePlayerPieces);
        SetPiecePositionsOnBoard(currentBoard.aliveComputerPieces);

        SetPiecePositionsDead(currentBoard.deadPlayerPieces);
        SetPiecePositionsDead(currentBoard.deadComputerPieces);
    }

    private void SetPiecePositionsOnBoard(List<Piece> pieces)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.tileCoordinates.x >= 0)
                piece.lastPosition = GetCenterTilePosition(piece.tileCoordinates);
            
            piece.transform.position = piece.lastPosition;
        }
    }

    private void SetPiecePositionsDead(List<Piece> pieces)
    {
        if (pieces.Count <= 0) return;

        int modifier = pieces[0].playerPiece ? -1 : 1;

        for (int i = 0; i < pieces.Count; i++)
        {
            Vector3 deadPosition = Vector3.zero;

            int row = i / 3;
            int col = i % 3;

            if (col == 0) deadPosition.x = 8.3f * modifier;
            else if (col == 1) deadPosition.x = 7.2f * modifier;
            else deadPosition.x = 6.1f * modifier;

            deadPosition.y = 3.5f - row;

            pieces[i].lastPosition = deadPosition;
            pieces[i].transform.position = deadPosition;
        }
    }

    private void SetPiecePositionOnBoard(Piece piece)
    {
        if (piece.tileCoordinates.x >= 0)
            piece.lastPosition = GetCenterTilePosition(piece.tileCoordinates);

        piece.transform.position = piece.lastPosition;
    }

    private Vector3 GetCenterTilePosition(Vector2 tileCoordinates)
    {
        Vector3 centerPosition = Vector3.zero;

        centerPosition.x = ((-spriteWidth * 0.5f) + (tileWidth * 0.5f)) + (tileWidth * tileCoordinates.x);
        centerPosition.y = ((-spriteHeight * 0.5f) + (tileHeight * 0.5f)) + (tileHeight * tileCoordinates.y);

        return centerPosition;
    }

    /* PLAYER FUNCTIONS START */
    public void PlacePiece(Vector3 mousePos, Piece selectedPiece)
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Getting Coordinates
        int mouseX = Mathf.FloorToInt(((mousePos.x + (spriteWidth * 0.5f)) / spriteWidth) * boardWidth);
        int mouseY = Mathf.FloorToInt(((mousePos.y + (spriteHeight * 0.5f)) / spriteHeight) * boardHeight);

        if (mouseX == boardWidth) mouseX--;
        if (mouseY == boardHeight) mouseY--;

        Vector2 tileCoordinates = new Vector2(mouseX, mouseY);

        if (currentBoard.TileIsValid(tileCoordinates, selectedPiece, settingUp))
        {
            if (settingUp)
            {
                currentBoard.PlacePiece(tileCoordinates, selectedPiece, settingUp);
                SetPiecePositionOnBoard(selectedPiece);
            }
            else
            {
                currentBoard = currentBoard.GenerateChildOfBoardState();
                currentBoard.PlacePiece(tileCoordinates, selectedPiece, settingUp);
                currentBoard.ResetInfo();
                SetAllPiecePositions();
                currentBoard.AddAllPossibleFutureBoardStates();
                Debug.Log("SET UP AVAILABLE COMPUTER VALID MOVES");
                currentBoard = CH.InitiateMCTS(currentBoard);
                currentBoard.ResetInfo();
                SetAllPiecePositions();
            }
        }
        else Debug.Log("Is Invalid");

        if (settingUp && currentBoard.board.Count >= 21) onAllPlayerPiecesOnBoard.Raise();
    }
    
    /* PLAYER FUNCTIONS END */

    public void UndoState()
    {
        Debug.Log("Start Undo");

        if (currentBoard.parentBoard == null) return;

        Debug.Log("Got Passed Parent Board");

        currentBoard = currentBoard.parentBoard;
        currentBoard.childrenBoard.Clear();

        currentBoard.ResetInfo();

        SetAllPiecePositions();
    }
}
