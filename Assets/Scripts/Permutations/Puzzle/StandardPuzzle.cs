using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StandardPuzzle: MonoBehaviour
{
    public static IEnumerable<(int x, int y)> Box(int x, int y, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                yield return (x + i, y + j);
            }
        }
    }

    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;

    [SerializeField] private List<PuzzlePiece> pieces;

    private PuzzlePiece[,] occupied;

    private void Awake()
    {
        occupied = new PuzzlePiece[width,height];
    }

    private void OnDrawGizmos()
    {
        foreach (var (x,y) in Box(0, 0, width, height))
        {
            Gizmos.DrawWireCube(new Vector3(transform.position.x + x + 0.5f, transform.position.y + y + 0.5f, 0), Vector3.one);
        }
    }


    internal bool TryRemovePiece(PuzzlePiece piece)
    {
        if (pieces.Contains(piece))
        {
            foreach (var (x, y) in Box(0, 0, width, height))
            {
                if (occupied[x, y] == piece)
                    occupied[x, y] = null;
            }
            return true;
        }
        return false;
    }

    private void OnPieceLifted(object sender, Vector2 e)
    {
        PuzzlePiece piece = sender as PuzzlePiece;
        if (pieces.Contains(piece)){ 
            foreach (var (x,y) in Box(0, 0, width, height))
            {
                if (occupied[x, y] == piece)
                    occupied[x, y] = null;
            }
        }
    }

    public class PieceAddedArgs
    {
        public PuzzlePiece piece;
        public Vector2Int bottomLeft;
    }

    public event EventHandler<PieceAddedArgs> OnPieceAdded;

    public bool TryAddPiece(PuzzlePiece piece, Vector2 bottomLeft)
    {
        bottomLeft = transform.InverseTransformPoint(bottomLeft);
        Vector2Int round = new Vector2Int(Mathf.RoundToInt(bottomLeft.x), Mathf.RoundToInt(bottomLeft.y));

        int pWidth, pHeight;
        (pWidth, pHeight) = piece.GetDimension();

        if (round.x < 0 || width < round.x + pWidth || round.y < 0 || height < round.y + pHeight)
        {
            return false;
        }

        if (Box(round.x, round.y, pWidth, pHeight).Any(pos => (occupied[pos.x, pos.y] != null)))
        {
            piece.ToBeforeDragging();
            return false;
        }



        foreach (var (x,y) in Box(round.x, round.y, pWidth, pHeight))
        {
            occupied[x, y] = piece;
        }

        piece.SetBottomLeft(transform.TransformPoint(new Vector3(round.x, round.y, -1)));
        pieces.Add(piece);
        Debug.Log("Managed to set Piece");
        OnPieceAdded?.Invoke(this, new PieceAddedArgs() { piece=piece, bottomLeft=round});
        return true;
    }



}
