using System;
using System.Collections.Generic;
using System.Text;

namespace standard_lib
{
    public interface IPermissions
    {
        void Request();
        event EventHandler OnRequestResult;
        bool Check();
    }
}
