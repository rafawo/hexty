// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

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
            Grid.ColorCell(cell, leftClick ? Grid.HexParams.SelectedColor : Grid.HexParams.DefaultColor, true);
            Grid.UpdateColors();
        }
    }
}
