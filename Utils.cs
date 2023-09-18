using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static ExileCore.PoEMemory.MemoryObjects.ServerInventory;

namespace ClickIt
{
    internal class Utils
    {
        public static Entity?[,] Get2DInventory(IList<InventSlotItem> InventorySlotItems)
        {
            Entity?[,] inv = new Entity?[12, 5];
            foreach (var item in InventorySlotItems)
            {
                for (int x = (int)item.PosX; x < (int)item.PosX + item.SizeX; ++x)
                {
                    for (int y = (int)item.PosY; y < (int)item.PosY + item.SizeY; ++y)
                    {
                        inv[x, y] = item.Item;
                    }
                }
            }
            return inv;
        }

        public static Vector2? FindSpotInInventory(Entity item, Entity?[,] inv, IList<Entity> items, bool strict = false)
        {
            if (inv == null) return null;
            //if item is stackable and there is another stackable item that can fit it, theres no need to check for a valid position
            if (item.HasComponent<Stack>())
            {
                if (strict)
                {
                    if (items.Any(x => CanItemBeStacked(item, x) == Stackable.Fully)) return new Vector2(-1, -1);
                }
                else
                {
                    if (items.Any(x => CanItemBeStackedSimple(item, x))) return new Vector2(-1, -1);
                }
            }

            var itemHeight = (int)item.GetComponent<Base>().ItemCellsSizeY;
            var itemWidth = (int)item.GetComponent<Base>().ItemCellsSizeX;
            //DebugWindow.LogMsg($"item: {item}, SizeY: {itemHeight}, SizeX: {itemWidth}");

            for (var yCol = 0; yCol < inv.GetLength(1) - (itemHeight - 1); yCol++)
            {
                for (var xRow = 0; xRow < inv.GetLength(0) - (itemWidth - 1); xRow++)
                {
                    var success = 0;
                    for (var xWidth = 0; xWidth < itemWidth; xWidth++)
                    {
                        for (var yHeight = 0; yHeight < itemHeight; yHeight++)
                        {
                            //DebugWindow.LogMsg($"{yCol} + {yHeight}, {xRow} + {xWidth} ");
                            if (inv[xRow + xWidth, yCol + yHeight] == null)
                            {
                                success++;
                            }
                        }
                    }
                    if (success >= itemHeight * itemWidth) return new Vector2(xRow, yCol);
                }
            }
            return null;
        }

        /// <summary>
        /// returns remaining stacksize until full
        /// </summary>
        /// <param name="item"></param>
        /// <param name="inventoryItem"></param>
        /// <returns></returns>
        public static Stackable CanItemBeStacked(Entity item, Entity inventoryItem)
        {
            // return false if not the same item
            if (item.Path != inventoryItem.Path || !inventoryItem.HasComponent<Stack>())
            {
                return Stackable.None;
            }

            var itemStack = item.GetComponent<Stack>();
            var inventoryitemStack = inventoryItem.GetComponent<Stack>();

            int remainingStackSpace = inventoryitemStack.Info.MaxStackSize - inventoryitemStack.Size;

            if (remainingStackSpace >= itemStack.Size)
            {
                return Stackable.Fully;
            }
            else if (remainingStackSpace == 0)
            {
                return Stackable.None;
            }
            else
            {
                return Stackable.Partial;
            }
        }

        public static bool CanItemBeStackedSimple(Entity item, Entity inventoryItem)
        {
            if (item.Path != inventoryItem.Path || !inventoryItem.HasComponent<Stack>())
            {
                return false;
            }

            var inventoryitemStack = inventoryItem.GetComponent<Stack>();
            if (inventoryitemStack.Size == inventoryitemStack.Info.MaxStackSize)
                return false;

            return true;
        }

        public enum Stackable
        {
            None = 0,
            Partial = 1,
            Fully = 2
        }
    }
}
