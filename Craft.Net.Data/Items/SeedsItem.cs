using Craft.Net.Data.Blocks;
namespace Craft.Net.Data.Items
{
    
    public class SeedsItem : Item
    {
        public override short Id
        {
            get
            {
                return 295;
            }
        }

        public override void OnItemUsedOnBlock(World world, Vector3 clickedBlock, Vector3 clickedSide, Vector3 cursorPosition, Entities.Entity usedBy)
        {
            if (world.GetBlock(clickedBlock) is FarmlandBlock && world.GetBlock(clickedBlock + clickedSide) == 0)
                world.SetBlock(clickedBlock + clickedSide, new SeedsBlock());
        }
    }
}
