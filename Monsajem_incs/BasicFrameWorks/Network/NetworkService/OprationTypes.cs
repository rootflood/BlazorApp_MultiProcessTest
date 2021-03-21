using System;

namespace Monsajem_Incs.Net.Base
{
#if DEBUG
    public enum OprationType
    {
        BeginService = 1,
        EndService = 2,
        SendData = 3,
        GetData = 4,
        SendCondition = 5,
        GetCondition = 6,
        Exeption = 255,
    }
#endif
}