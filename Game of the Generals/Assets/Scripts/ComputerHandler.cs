using System.Collections.Generic;
using UnityEngine;

public class ComputerHandler
{
    private BoardState currentBoard;
    private BoardState rootBoard;

    private BoardEvaluator evaluator;

    private int iterations;
    private int depthLimit = 2;
    private int currentDepth = 0;

    public ComputerHandler()
    {
        currentBoard = null;
        rootBoard = null;

        evaluator = new BoardEvaluator();
    }

    public BoardState InitiateMCTS(BoardState boardToSearch)
    {
        float previousTime = Time.time;

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

        return available[index];
    }

    public BoardState AltInitiateMCTS(BoardState boardToSearch)
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
}