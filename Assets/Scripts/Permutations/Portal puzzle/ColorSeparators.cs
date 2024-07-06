using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorSeparators : MonoBehaviour
{
    [SerializeField] private PortalApartment portalApartment;

    [Header("Color indication")]
    [SerializeField] private ColorIndication leftColors;
    [SerializeField] private ColorIndication rightColors;

    private void OnEnable()
    {
        portalApartment.OnRoomChanged += OnRoomIndexChanged;
        portalApartment.OnApartmentMorphed += OnApartmentMorphed;
    }

    private void OnDisable()
    {
        portalApartment.OnRoomChanged -= OnRoomIndexChanged;
        portalApartment.OnApartmentMorphed -= OnApartmentMorphed;
    }

    private void OnRoomIndexChanged(object sender, PortalApartment.RoomInfo info)
    {
        var colorIDs = info.apartmentInput;

        var input = info.middleInput;
        var output = info.middleInput * info.permutation.Inverse();
        leftColors.SetSeparatorsOn(Enumerable.Zip(input, colorIDs, (i, j) => i == j).ToArray());
        rightColors.SetSeparatorsOn(Enumerable.Zip(output, colorIDs, (i, j) => i == j).ToArray());
    }

    private void OnApartmentMorphed(object sender, PortalApartment.RoomInfo info)
    {
        //PipesRoom pipesRoom = portalApartment.GetPipesRoom(0);

        var colorIDs = info.apartmentInput;

        Color[] colors = PipesRoom.ToColors(colorIDs);
        leftColors.SetColors(colors);
        rightColors.SetColors(colors);

        // TODO: Add maybe "SeparateAll"?
        var input = info.middleInput;
        var output = info.middleInput * info.permutation.Inverse();
        leftColors.SetSeparatorsOn( Enumerable.Zip(input,  colorIDs, (i, j) => i == j).ToArray());
        rightColors.SetSeparatorsOn(Enumerable.Zip(output, colorIDs, (i, j) => i == j).ToArray());
    }
}
