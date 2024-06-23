using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSeparators : MonoBehaviour
{
    [SerializeField] private PortalApartment portalApartment;

    [Header("Color indication")]
    [SerializeField] private ColorIndication leftColors;
    [SerializeField] private ColorIndication rightColors;

    private void OnEnable()
    {
        portalApartment.OnRoomIndexChanged += OnRoomIndexCHanged;
        portalApartment.OnApartmentMorphed += OnApartmentMorphed;
    }

    private void OnDisable()
    {
        portalApartment.OnRoomIndexChanged -= OnRoomIndexCHanged;
        portalApartment.OnApartmentMorphed -= OnApartmentMorphed;
    }

    private void OnRoomIndexCHanged(object sender, int roomIndex)
    {
        PipesRoom pipesRoom = portalApartment.GetPipesRoom(0);

        leftColors.SeparateByColor(pipesRoom.GetLeftColors());
        rightColors.SeparateByColor(pipesRoom.GetRightColors());
    }

    private void OnApartmentMorphed(object sender, System.EventArgs e)
    {
        PipesRoom pipesRoom = portalApartment.GetPipesRoom(0);

        leftColors.SetColors(pipesRoom.GetLeftColors());
        rightColors.SetColors(pipesRoom.GetLeftColors());

        // TODO: Add maybe "SeparateAll"?
        //       Also, maybe add "Get 0 input" for the apartment, in case this is not the 0 room.
        leftColors.SeparateByColor(pipesRoom.GetLeftColors());
        rightColors.SeparateByColor(pipesRoom.GetRightColors());
    }
}
