using System;
using System.Collections.Generic;
using System.Text;

namespace standard_lib
{
    public static class Permissions
    {
        public static void SetInstance(IPermissions p)
        {
            Instance = p;
        }

        public static IPermissions Instance { get; private set; }
    }
}
