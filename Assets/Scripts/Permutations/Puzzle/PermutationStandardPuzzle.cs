using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StandardPuzzle))]
public class PermutationStandardPuzzle : MonoBehaviour
{

    private StandardPuzzle standardPuzzle;
    [SerializeField] private PermutationLines lines;

    private void Awake()
    {
        standardPuzzle = GetComponent<StandardPuzzle>();
    }

    private void OnEnable()
    {
        standardPuzzle.OnPieceAdded += OnPieceAdded;
    }

    private void OnDisable()
    {
        standardPuzzle.OnPieceAdded -= OnPieceAdded;
    }

    private void OnPieceAdded(object sender, StandardPuzzle.PieceAddedArgs args)
    {
        PermutationLines pieceLines = args.piece.GetComponentInChildren<PermutationLines>();
        Permutation p = pieceLines.GetPermutation();
        int y = args.bottomLeft.y;

        Permutation currentPermutation = lines.GetPermutation();
        lines.UpdatePermutation(currentPermutation * p.TranslateUp(y, currentPermutation.size) );
        args.piece.gameObject.SetActive( false ); // TODO: find a better way to do this
    }
}
