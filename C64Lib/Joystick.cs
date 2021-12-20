using System;
using System.Collections.Generic;
using System.Text;

namespace C64Lib
{
    public class JoystickCollection
    {
        public Joystick Joystick1 { get; }
        public Joystick Joystick2 { get; }
    }

    public enum JoystickButtons
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Fire = 16
    }
    public class Joystick
    {
        public JoystickButtons Pressed { get; }
    }
}
