using PoplarLib;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    public List<Pawn> Pawns { get; }
    public int MaxPawns { get; }
    public Vector3 Destination { get; }

    public Group(List<Pawn> pawns, int maxPawns, Vector3 destination)
    {
        Pawns = pawns;
        MaxPawns = maxPawns < 1 ? 1 : maxPawns;
        Destination = destination;
    }

    public bool IsGroupFull()
    {
        return Pawns.Count >= MaxPawns;
    }

    // The smaller group comparing with it's max size, the lower chance of going to capture tile is
    public bool ShouldGoCapturing(float modifier = 1)
    {
        float fullnessOfGroup = Mathf.Clamp(Pawns.Count / (float)MaxPawns * 100, 0, 100);
        modifier = fullnessOfGroup < 90 ? modifier : 1; //If it's 90% save, don't count on modifier

        return ExtendedMonoBehaviour.EvaluateChance(fullnessOfGroup * modifier);
    }

    public void Order(PawnOrder order)
    {
        PawnTask.OrderPawns(order, Pawns, Destination);
    }
}
