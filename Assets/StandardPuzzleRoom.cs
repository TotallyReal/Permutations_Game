using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StandardPuzzleRoom : MonoBehaviour
{
    [SerializeField] private List<StandardPuzzle> puzzles;
    [SerializeField] private List<PuzzlePiece> pieces;

    private void OnEnable()
    {
        foreach (var piece in pieces)
        {
            piece.GetComponent<PuzzlePiece>().OnPieceLifted += OnPieceLifted;
            piece.GetComponent<PuzzlePiece>().OnPieceDropped += OnPieceDropped;
        }
    }

    private void OnDisable()
    {
        foreach (var piece in pieces)
        {
            piece.GetComponent<PuzzlePiece>().OnPieceLifted -= OnPieceLifted;
            piece.GetComponent<PuzzlePiece>().OnPieceDropped -= OnPieceDropped;
        }
    }

    private void OnPieceLifted(object sender, Vector2 e)
    {
        PuzzlePiece piece = sender as PuzzlePiece;
        foreach (var puzzle in puzzles)
        {
            if (puzzle.TryRemovePiece(piece))
                return;
        }
    }

    private void OnPieceDropped(object sender, Vector2 bottomLeft)
    {
        PuzzlePiece piece = sender as PuzzlePiece;
        foreach (var puzzle in puzzles)
        {
            if (puzzle.TryAddPiece(piece, bottomLeft))
                return;
        }

        piece.ToBeforeDragging();
    }
}
