using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public BoardState currentBoard = null;

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
    [SerializeField] private GameEventsSO onGameOver = null;

    [SerializeField] private GameObject testObject = null;

    [SerializeField] private List<BoardSetups> computerSetups = null;

    public List<Piece> playerPieces;
    public List<Piece> computerPieces;

    public bool settingUp = true;

    private ComputerHandler CH = null;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBounds();

        GenerateInitialBoard();

        CH = new ComputerHandler(SR.bounds.extents);
    }

    public void ClickedSetupButton()
    {
        int count = 0;

        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 8; y++)
                if (currentBoard.board[x, y] != null)
                    count++;

        if (count >= 21) StartGame();
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
        currentBoard.SetupBoardComputer(computerSetups[Random.Range(0, computerSetups.Count)]);

        SetAllPiecePositions();

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
        List<int> initialKills = new List<int>();

        for (int i = 0; i < 21; i++)
            initialKills.Add(0);

        currentBoard = new BoardState(null, new List<BoardState>(),
                                      null, playerPieces, computerPieces,
                                      new List<Piece>(), new List<Piece>(),
                                      SearchForFlag(playerPieces),
                                      SearchForFlag(computerPieces),
                                      initialKills, initialKills,
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
            if (piece.xCoord >= 0)
                piece.lastPosition = GetCenterTilePosition(piece.xCoord, piece.yCoord);
            
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
        if (piece.xCoord >= 0)
            piece.lastPosition = GetCenterTilePosition(piece.xCoord, piece.yCoord);

        piece.transform.position = piece.lastPosition;
    }

    private Vector3 GetCenterTilePosition(int x, int y)
    {
        Vector3 centerPosition = Vector3.zero;

        centerPosition.x = ((-spriteWidth * 0.5f) + (tileWidth * 0.5f)) + (tileWidth * x);
        centerPosition.y = ((-spriteHeight * 0.5f) + (tileHeight * 0.5f)) + (tileHeight * y);

        return centerPosition;
    }

    /* PLAYER FUNCTIONS START */
    public void PlacePiece(Vector3 mousePos, Piece selectedPiece)
    {
        // Getting Coordinates
        int mouseX = Mathf.FloorToInt(((mousePos.x + (spriteWidth * 0.5f)) / spriteWidth) * boardWidth);
        int mouseY = Mathf.FloorToInt(((mousePos.y + (spriteHeight * 0.5f)) / spriteHeight) * boardHeight);

        if (mouseX == boardWidth) mouseX--;
        if (mouseY == boardHeight) mouseY--;

        if (currentBoard.TileIsValid(mouseX, mouseY, selectedPiece, settingUp))
        {
            if (settingUp)
            {
                MusicManager.GetInstance().PlayMove();
                currentBoard.PlacePiece(mouseX, mouseY, selectedPiece, settingUp);
                SetPiecePositionOnBoard(selectedPiece);
            }
            else
            {
                currentBoard = currentBoard.GenerateChildOfBoardState();
                currentBoard.PlacePiece(mouseX, mouseY, selectedPiece, settingUp);
                currentBoard.ResetInfo();
                SetAllPiecePositions();

                if (!currentBoard.CheckIfFlagStillAlive())
                {
                    onGameOver.Raise();
                    return;
                }

                if (currentBoard.CheckIfFlagAtEnd())
                {
                    GM.SwitchTurns();
                    onGameOver.Raise();
                    return;
                }

                GM.SwitchTurns();

                currentBoard.AddAllPossibleFutureBoardStates();

                currentBoard = CH.InitiateSearch(currentBoard);
                currentBoard.ResetInfo();

                if (!currentBoard.CheckIfFlagStillAlive())
                {
                    onGameOver.Raise();
                    return;
                }

                if (currentBoard.CheckIfFlagAtEnd())
                {
                    GM.SwitchTurns();
                    onGameOver.Raise();
                    return;
                }

                GM.SwitchTurns();

                MusicManager.GetInstance().PlayMove();

                StartCoroutine("DelaySetPositions");
            }
        }

        //DebugShowBoard();

        int count = 0;

        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 8; y++)
                if (currentBoard.board[x, y] != null)
                    count++;

        if (settingUp && count >= 21) onAllPlayerPiecesOnBoard.Raise();
    }

    /* PLAYER FUNCTIONS END */

    IEnumerator DelaySetPositions()
    {
        float time = 0;

        while (time < 0.2f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        MusicManager.GetInstance().PlayMove();

        SetAllPiecePositions();
    }

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

    private void DebugShowBoard()
    {
        Debug.Log("Debug Start");

        string debug = "";

        for(int i = 7; i >= 0; i--)
        {
            for (int j = 0; j < 9; j++)
            {
                int value = currentBoard.board[j, i] != null ? (currentBoard.board[j, i].playerPiece ? 1 : 2) : 0;
                debug += value.ToString() + " ";
            }

            Debug.Log(debug);

            debug = "";
        }
    }
}
