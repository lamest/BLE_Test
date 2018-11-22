using System;

namespace standard_lib.Bluetooth
{
    public interface IPermissions
    {
        void Request(string[] requiredPermissions);
        event EventHandler OnRequestResult;
        bool Check(string[] requiredPermissions);
    }
}