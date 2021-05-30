using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerHandler
{
    private BoardEvaluator evaluator;

    public ComputerHandler()
    {
        evaluator = new BoardEvaluator();
    }

    public BoardState InitiateSearch(BoardState boardToSearch)
    {
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


            if (score > highestScore)
            {
                highestScore = score;
                index = i;
            }
        }

        return available[index].realBoard;
    }
}