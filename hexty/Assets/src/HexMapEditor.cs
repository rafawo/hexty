using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] Colors;
    public HexGrid Grid;

    private void Update()
    {
        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);
        if ((leftClick || rightClick) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput(leftClick);
        }
    }

    private void HandleInput(bool leftClick)
    {
        var cell = Grid.GetMouseCell();
        if (cell != null)
        {
            Grid.ColorCell(cell, leftClick ? Grid.SelectedColor : Grid.DefaultColor, true);
            Grid.UpdateColors();
        }
    }
}
