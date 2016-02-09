using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    [Flags]
    enum NT_NotifyKind : uint
    {
        NT_NOTIFY_NONE = 0,
        NT_NOTIFY_IMMEDIATE = 0x01, /* initial listener addition */
        NT_NOTIFY_LOCAL = 0x02,     /* changed locally */
        NT_NOTIFY_NEW = 0x04,       /* newly created entry */
        NT_NOTIFY_DELETE = 0x08,    /* deleted */
        NT_NOTIFY_UPDATE = 0x10,    /* value changed */
        NT_NOTIFY_FLAGS = 0x20      /* flags changed */
    };

    [Flags]
    enum NT_EntryFlags : uint
    {
        NT_PERSISTENT = 0x01
    };
}
