using PoplarLib;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Team))]
public class Bot : ExtendedMonoBehaviour
{
    private float _timerToMakeMove, _timerToMakeMoveMax = 5f;
    private Team _team;

    private void Start()
    {
        _team = GetComponent<Team>();
    }

    private void Update()
    {
        if (HandleTimer(ref _timerToMakeMove, _timerToMakeMoveMax))
        {
            SendPawnsToCapture();
        }
    }

    private void SendPawnsToCapture()
    {
        List<Pawn> availablePawns = _team.PawnsInTeam;

        // TODO Probably state matters??

        foreach (Pawn pawn in availablePawns)
        {
            pawn.OrderToMove(getClosestTilePos(pawn.transform.position));
        }
    }

    private Vector3 getClosestTilePos(Vector3 startPos)
    {
        Vector3 closestTilePos = GameManager.Instance.Tiles[0].transform.position;
        float closestDistance = Vector3.Distance(startPos, closestTilePos);

        foreach (TileBase tile in GameManager.Instance.Tiles)
        {
            Vector3 tilePos = tile.transform.position;
            float distance = Vector3.Distance(startPos, tilePos);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTilePos = tilePos;
            }
        }

        return closestTilePos;
    }
}
