﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.NetworkTables2.Thread
{
    public interface NTThread
    {
         void Stop();

         bool IsRunning();
    }
}
