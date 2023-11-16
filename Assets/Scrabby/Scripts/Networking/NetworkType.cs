using System;

namespace Scrabby.Networking
{
    [Flags]
    public enum NetworkType : short
    {
        None = 0,
        PyScrabby = 1 << 0,
        Ros = 1 << 1,
        Storm = 1 << 2,
        All = PyScrabby | Ros | Storm
    }
}