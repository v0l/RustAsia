using System;
using System.Collections.Generic;
using System.Text;

namespace RustAsia.System
{
    public static class Variables
    {
        public static bool FreeBuild { get; set; }
        public static bool AdminNoTarget { get; set; } = true;
        public static bool LootableAdmins { get; set; }
        public static float ESPTickRate { get; set; } = 1f;
    }
}
