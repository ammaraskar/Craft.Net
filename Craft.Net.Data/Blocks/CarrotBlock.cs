using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data.Items;

namespace Craft.Net.Data.Blocks
{
    public class CarrotBlock : Block
    {
        public override short Id
        {
            get { return 141; }
        }

        public override bool RequiresSupport
        {
            get { return true; }
        }

        public override Vector3 SupportDirection
        {
            get { return Vector3.Down; }
        }

        public override bool GetDrop(ToolItem tool, out Slot[] drops)
        {
            drops = new[] { new Slot(new CarrotItem(), (sbyte)MathHelper.Random.Next(1, 4)) };
            return true;
        }
    }
}
