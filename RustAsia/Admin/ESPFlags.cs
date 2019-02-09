using System;

namespace RustAsia.Admin
{
    [Flags]
    public enum ESPFlags
    {
        None = 0,
        Players = 1,
        Sleepers = 2,
        ToolCupboards = 4,
        Stashes = 8,
        Animals = 16
    }
}
