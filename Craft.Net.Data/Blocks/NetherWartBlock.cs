using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public class NetherWartBlock : Block
    {
        public override short Id
        {
            get { return 115; }
        }

        public override BoundingBox? BoundingBox
        {
            get { return null; }
        }
    }
}
