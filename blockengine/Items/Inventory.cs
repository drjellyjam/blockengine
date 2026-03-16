using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockengine.Items
{
    public class InventorySlot
    {
        public ItemType item;
        public int count;

        public InventorySlot(ItemType itemT = ItemType.ErrorItem, int startcount = 0)
        {
            item = itemT;
            count = startcount;
        }

        

        public void MakeItem(ItemType itemT,int amount)
        {
            item = itemT;
            count = amount;
        }

        public void MakeEmpty()
        {
            count = 0;
            item = ItemType.ErrorItem;
        }

        public int AddCount(int add)
        {
            if (add <= 0) { return 0; }

            if (add == 1)
            {
                IncrementCount();
                return 0;
            }

            int max = Globals.ItemDefinitions[item].GetMaxStack();

            count += add;

            if (count > max)
            {
                int remain = count - max;
                count = max;
                return remain;
            }

            return 0;
        }

        public bool IncrementCount() //returns true if reached max value
        {
            count++;
            int max = Globals.ItemDefinitions[item].GetMaxStack();
            if (count > max)
            {
                count = max;
                return true;
            }
            return false;
        }

        public bool DecrimentCount() //returns true if item is gone
        {
            count--;
            if (count <= 0)
            {
                MakeEmpty();
                return true;
            }
            return false;
        }

        public bool IsFull()
        {
            int max = Globals.ItemDefinitions[item].GetMaxStack();
            return count >= max;
        }

        public bool IsEmpty()
        {
            return count <= 0;
        }
    }
    public class Inventory
    {
        private List<InventorySlot> slots;

        public Inventory(int slotsnumber = 4)
        {
            slots = new List<InventorySlot>();
            for (int i = 0; i < slotsnumber; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        public int GetSlotCount()
        {
            return slots.Count;
        }

        public void OverwriteSlot(int slotidx, ItemType item, int count = 1)
        {
            slots[slotidx].MakeItem(item, count);
        }
        
        public void ClearSlot(int slotidx) {
            slots[slotidx].MakeEmpty();
        }

        public void PrintInventory()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot s = slots[i];
                if (s.count > 0)
                {
                    Console.WriteLine(Globals.ItemDefinitions[s.item].GetDisplayName() + ": " + s.count.ToString());
                }
            }
        }

        public int GetEmptySlotIndex()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot s = slots[i];
                if (s.count <= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public List<int> GetItemSlots(ItemType item) //gets all slots that are filled with an item
        {
            List<int> itemslots = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot s = slots[i];
                if (s.item == item && s.count > 0)
                {
                    itemslots.Add(i);
                }
            }
            return itemslots;
        }

        //adds a certain amount of items to your inventory neatly and returns whats left that could not fit
        public int AddItem(ItemType item,int count)
        {
            int left = count;
            List<int> itemslots = GetItemSlots(item);
            if (itemslots.Count > 0)
            {
                Console.WriteLine("Already has in inventory");
                for (int i = 0; i < itemslots.Count; i++)
                {
                    InventorySlot s = slots[itemslots[i]];
                    if (!s.IsFull())
                    {
                        int remaining = s.AddCount(left);
                        left = remaining;
                    }
                }
            }
            
            //NO EXISTING ITEM STACKS SO WE START PACKING IT INTO EMPTY SLOTS
            while (left > 0)
            {
                int empty = GetEmptySlotIndex();
                if (empty == -1)
                {
                    return left;
                }

                OverwriteSlot(empty, item, 0);
                int remaining = slots[empty].AddCount(left);
                left = remaining;
            }

            return 0;
        }

        public InventorySlot GetSlot(int slotidx) {
            return slots[slotidx];
        }

        public void SwapSlots(int slot1,int slot2)
        {
            ItemType itm2 = slots[slot2].item;
            int itm2_count = slots[slot2].count;

            slots[slot2].MakeItem(slots[slot1].item, slots[slot1].count);
            slots[slot1].MakeItem(itm2, itm2_count);
        }

        public void CombineSlots(int slot1, int slot2)
        {
            InventorySlot s1 = slots[slot1];
            InventorySlot s2 = slots[slot2];
            if (s1.item == s2.item && !s2.IsFull())
            {
                int remaining = s2.AddCount(s1.count);
                s1.count = remaining;
                if (remaining <= 0)
                {
                    s1.MakeEmpty();
                }
            }
        }

        public void SlotDragged(int slot1,int slot2)
        {
            if (slot1 == slot2 || slot1 < 0 || slot1 >= slots.Count || slot2 < 0 || slot2 >= slots.Count)
            {
                return;
            }

            InventorySlot s1 = slots[slot1];
            InventorySlot s2 = slots[slot2];
            if (s1.item == s2.item)
            {
                CombineSlots(slot1, slot2);
                return;
            }
            SwapSlots(slot1, slot2);
        }
    }
}
