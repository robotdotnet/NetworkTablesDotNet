﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    [Flags]
    public enum NotifyFlags : uint
    {
        NotifyNone = 0,
        NotifyImmediate = 0x01, /* initial listener addition */
        NotifyLocal = 0x02,     /* changed locally */
        NotifyNew = 0x04,       /* newly created entry */
        NotifyDelete = 0x08,    /* deleted */
        NotifyUpdate = 0x10,    /* value changed */
        NotifyFlagsChanged = 0x20      /* flags changed */
    }

    [Flags]
    public enum EntryFlags : uint
    {
        None = 0x00,
        Persistent = 0x01
    }


}