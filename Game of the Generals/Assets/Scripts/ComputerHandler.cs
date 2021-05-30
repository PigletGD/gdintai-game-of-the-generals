using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerHandler
{
    private BoardState currentBoard;
    private BoardState rootBoard;
    private BoardManager BM;

    private BoardEvaluator evaluator;

    private int iterations;
    private int depthLimit = 2;
    private int currentDepth = 0;

    private float spriteWidth = 0.0f;
    private float spriteHeight = 0.0f;

    private float tileWidth = 0.0f;
    private float tileHeight = 0.0f;

    public ComputerHandler(Vector3 extents)
    {
        currentBoard = null;
        rootBoard = null;

        evaluator = new BoardEvaluator();

        InitializeBounds(extents);
    }

    public BoardState InitiateSearch(BoardState boardToSearch)
    {
        currentBoard = rootBoard = boardToSearch;

        List<BoardState> available = boardToSearch.childrenBoard;

        foreach (BoardState boardState in available)
        {            
            evaluator.Evaluate(boardState);

            boardState.AddAllPossibleFutureBoardStates();

            foreach (BoardState childBoard in boardState.childrenBoard)
            {
                evaluator.Evaluate(childBoard);
            }
        }

        int index = -1;
        float highestScore = -999999999;
        float averageScore = 0;

        for (int i  = 0; i < available.Count; i++)
        {
            float compScore = available[i].evaluationScore;

            float playerAverage = 0;
            float[] highestScores = new float[5] { -99999999, -99999999, -99999999, -99999999, -99999999 };
            foreach (BoardState child in available[i].childrenBoard)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (highestScores[j] < child.evaluationScore)
                    {
                        highestScores[j] = child.evaluationScore;
                        break;
                    }
                }
            }

            for(int j = 0; j < 5; j++)
                playerAverage += highestScores[j];

            playerAverage /= 5;

            float score = compScore - playerAverage;
            averageScore += score;

            //Debug.Log($"Index {i} Score: {score} Is Player Turn: {available[i].playerTurn}");

            if (score > highestScore)
            {
                highestScore = score;
                index = i;
            }
        }

        Debug.Log($"Highest Index {index} Score: {highestScore}");
        Debug.Log($"Average Score: {averageScore / available.Count}");

        return available[index].realBoard;
    }

    public IEnumerator CInitiateSearch(BoardState boardToSearch)
    {
        currentBoard = rootBoard = boardToSearch;

        List<BoardState> available = boardToSearch.childrenBoard;

        foreach (BoardState boardState in available)
        {
            evaluator.Evaluate(boardState);

            SetAllPiecePositions(boardState);

            yield return new WaitForSeconds(1f);

            boardState.AddAllPossibleFutureBoardStates();

            foreach (BoardState childBoard in boardState.childrenBoard)
            {
                evaluator.Evaluate(childBoard);

                SetAllPiecePositions(childBoard);

                yield return new WaitForSeconds(1f);
            }
        }

        int index = -1;
        float highestScore = -999999999;
        float averageScore = 0;

        for (int i = 0; i < available.Count; i++)
        {
            float compScore = 0;
            compScore = available[i].evaluationScore;

            float playerAverage = 0;
            foreach (BoardState child in available[i].childrenBoard)
            {
                playerAverage += child.evaluationScore;
            }

            playerAverage /= available[i].childrenBoard.Count;

            float score = compScore - playerAverage;
            averageScore += score;

            //Debug.Log($"Index {i} Score: {score} Is Player Turn: {available[i].playerTurn}");

            if (score > highestScore)
            {
                highestScore = score;
                index = i;
            }
        }

        Debug.Log($"Highest Index {index} Score: {highestScore}");
        Debug.Log($"Average Score: {averageScore / available.Count}");

        //return available[index];
    }

    public BoardState AltInitiateSearch(BoardState boardToSearch)
    {
        currentBoard = rootBoard = boardToSearch;

        List<BoardState> available = boardToSearch.childrenBoard;

        iterations = 0;

        do
        {
            BoardState explore = Selection(available);

            if (explore.timesVisited == 0 || currentDepth == 2) Simulation(explore);
            else
            {
                Debug.Log("EXPAND");

                if (!explore.atTerminalState)
                {
                    explore.AddAllPossibleFutureBoardStates();

                    Simulation(Selection(explore.childrenBoard));
                }
                else Backpropagation(explore);
            }

            explore.ResetInfo();

            iterations++;
        } while (iterations < 100);

        int index = -1;
        float score = -999999999;

        for (int i = 0; i < available.Count; i++)
        {
            Debug.Log($"Index {i} Score: {available[i].evaluationScore}    Times Visited: {available[i].timesVisited}");

            if (available[i].evaluationScore > score)
            {
                score = available[i].evaluationScore;
                index = i;
            }
        }

        return available[index];
    }

    private BoardState Selection(List<BoardState> available)
    {
        List<BoardState> children = available;
        BoardState leafNode = null;

        currentDepth = 0;

        do
        {
            Debug.Log(currentDepth);
            Debug.Log("Children Count: " + children.Count);

            float highestConfidence = -99999999;
            BoardState highestState = null;

            foreach (BoardState boardState in children)
            {
                float confidence = UCB1(boardState);

                if (confidence > highestConfidence)
                {
                    //Debug.Log("Confidence: " + confidence);

                    highestConfidence = confidence;
                    highestState = boardState;
                }
            }

            leafNode = highestState;
            children = highestState.childrenBoard;

            currentDepth++;
        } while (children.Count > 0 && currentDepth < depthLimit);

        return leafNode;
    }

    private float UCB1(BoardState boardState)
    {
        if (boardState.timesVisited == 0) return 9999999;
        else return (boardState.evaluationScore / (boardState.childrenBoard.Count > 0 ? boardState.childrenBoard.Count : 1)) + (2 * (Mathf.Sqrt(Mathf.Log(boardState.parentBoard != rootBoard ? boardState.parentBoard.timesVisited : iterations) / boardState.timesVisited)));
    }

    private void Simulation(BoardState leaf)
    {
        BoardState currentState = leaf;

        currentState.ResetInfo();

        int loopFail = 0;

        while (!currentState.atTerminalState && loopFail <= 100)
        {
            // get a random available move
            //currentBoard = current
            currentState = currentState.GetRandomFutureBoardState();

            loopFail++;
        }

        if (loopFail >= 100)
        {
            Debug.Log("Too many board states");
        }

        Backpropagation(currentState);
    }

    private void Backpropagation(BoardState terminal)
    {
        int won = !terminal.playerWon ? 1 : -1;

        if (!terminal.atTerminalState)
            won = 0;

        BoardState currentState = terminal;

        currentState.timesVisited++;
        currentState.evaluationScore += won;

        do
        {
            currentState = currentState.parentBoard;

            currentState.timesVisited++;
            currentState.evaluationScore += won;
        } while (currentState.parentBoard != null && currentState != rootBoard);

        /*if (currentState == rootBoard)
            Debug.Log("Found Root");
        else if (currentState.parentBoard == null)
            Debug.Log("Failed to find original state");*/
    }

    //Temp

    private void SetAllPiecePositions(BoardState currentBoard)
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

    private void InitializeBounds(Vector3 extents)
    {
        spriteWidth = extents.x * 2.0f;
        spriteHeight = extents.y * 2.0f;

        tileWidth = spriteWidth / 9;
        tileHeight = spriteHeight / 8;
    }
}