using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables
{
    public class SequenceNumber
    {
        uint m_value;
        public SequenceNumber()
        {
            m_value = 0;
        }

        public SequenceNumber(uint value)
        {
            m_value = value;
        }

        public SequenceNumber(SequenceNumber old)
        {
            m_value = old.m_value;
        }

        public uint Value() => m_value;

        public static SequenceNumber operator++(SequenceNumber input)
        {
            ++input.m_value;
            if (input.m_value > 0xffff) input.m_value = 0;
            return input;
        }
    }
}
