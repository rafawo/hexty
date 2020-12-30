using UnityEngine;

/// <summary>
/// Collection of utilities to handle input in Unity.
/// </summary>
public static class InputUtilities
{
    /// <summary>
    /// Simple enumeration with the 4 common directions.
    /// </summary>
    public enum Direction
    {
        Left,
        Right,
        Down,
        Up
    }

    /// <summary>
    /// Returns whether the specified input direction is active.
    ///
    /// This function assumes that the input manager has setup
    /// left-right movement with "Horizontal" and
    /// down-up movement with "Vertical".
    ///
    /// -- Ideal Horizontal Input Setup --
    ///
    /// Left: [
    ///     A,
    ///     Left arrow,
    ///     Controller X axis (left joystick) < 0,
    ///     Controller 4th X axis (right joystick) < 0,
    ///     Controller 6th X axis (dpad) < 0,
    /// ]
    /// Right: [
    ///     D,
    ///     Right arrow,
    ///     Controller X axis (left joystick) > 0,
    ///     Controller 4th X axis (right joystick) > 0,
    ///     Controller 6th X axis (dpad) > 0,
    /// ]
    ///
    /// -- Ideal Vertical Input Setup --
    ///
    /// Down: [
    ///     S,
    ///     Down arrow,
    ///     Controller Y axis (left joystick) < 0,
    ///     Controller 5th Y axis (right joystick) < 0,
    ///     Controller 7th Y axis (dpad) < 0,
    /// ]
    /// Up: [
    ///     W,
    ///     Up arrow,
    ///     Controller inverted Y axis (left joystick) > 0,
    ///     Controller inverted 5th Y axis (right joystick) > 0,
    ///     Controller inverted 7th Y axis (dpad) > 0,
    /// ]
    /// </summary>
    /// <param name="direction">Input direction to be queried if active.</param>
    /// <returns></returns>
    public static bool IsDirectionActive(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                return Input.GetAxis("Horizontal") < 0;

            case Direction.Right:
                return Input.GetAxis("Horizontal") > 0;

            case Direction.Down:
                return Input.GetAxis("Vertical") < 0;

            case Direction.Up:
                return Input.GetAxis("Vertical") > 0;
        }
        return false;
    }
}