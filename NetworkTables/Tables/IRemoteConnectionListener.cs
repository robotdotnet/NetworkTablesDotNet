﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.Tables
{
    public interface IRemoteConnectionListener
    {
         void Connected(IRemote remote);
         void Disconnected(IRemote remote);
    }
}
