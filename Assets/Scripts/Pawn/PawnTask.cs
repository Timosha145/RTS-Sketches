using System.Collections.Generic;
using UnityEngine;

public static class PawnTask
{

    public static void LineUpPawnsOnTarget(List<Pawn> pawnList, Vector3 centerPoint)
    {
        int rows = Mathf.FloorToInt(Mathf.Sqrt(pawnList.Count));
        int cols = Mathf.CeilToInt((float)pawnList.Count / rows);
        float halfRows = (rows - 1) * 0.5f;
        float halfCols = (cols - 1) * 0.5f;

        for (int pawnIndex = 0; pawnIndex < pawnList.Count; pawnIndex++)
        {
            Pawn pawn = pawnList[pawnIndex];

            int row = pawnIndex / cols;
            int col = pawnIndex % cols;

            Vector3 offsetFromCenter = new Vector3((row - halfRows) * 1, 0, (col - halfCols) * pawn.Offset);
            Vector3 posInLine = centerPoint + offsetFromCenter;

            pawn.MoveToTarget(posInLine);
            pawn.StayingPosition = posInLine;
            pawn.StopFollowing();
        }
    }

    public static void CircleTarget(List<Pawn> pawnList, Vector3 centerPoint)
    {
        for (int pawnIndex = 0; pawnIndex < pawnList.Count; pawnIndex++)
        {
            Pawn pawn = pawnList[pawnIndex];
            float radius = pawn.Offset * 0.75f;

            Vector3 posInCircle = new Vector3(
                centerPoint.x + radius * Mathf.Cos(2 * Mathf.PI * pawnIndex / pawnList.Count),
                centerPoint.y,
                centerPoint.z + radius * Mathf.Sin(2 * Mathf.PI * pawnIndex / pawnList.Count)
            );

            pawn.MoveToTarget(posInCircle);
            pawn.StayingPosition = posInCircle;
            pawn.StopFollowing();
        }
    }
}
