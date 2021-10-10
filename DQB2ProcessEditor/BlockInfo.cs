using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2ProcessEditor
{
    class BlockInfo : IComparable
    {
        public UInt16 ID { get; set; }
        public String Name { get; set; }

        public int CompareTo(object obj)
        {
            var info = obj as BlockInfo;
            if (info == null) return 0;

            if (ID < info.ID) return -1;
            else if (ID > info.ID) return 1;
            else return 0;
        }
    }
}
