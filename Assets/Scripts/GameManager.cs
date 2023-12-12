using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<TileBase> Tiles { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        Tiles = new List<TileBase>();
    }

    public void InitTile(TileBase tile)
    {
        Tiles.Add(tile);
    }

    public bool AreAllTilesCaptured()
    {
        return Tiles.All(tile => tile.IsCaptured());
    }
}
