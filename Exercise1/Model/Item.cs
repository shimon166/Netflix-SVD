using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exercise1.Model
{
    public class Item
    {
        public Item(int itemId, string sourceCategory)
        {
            ItemId = itemId;
            SourceCategory = sourceCategory;
            MatrixIndex = -1;
        }

        public int MatrixIndex { get; set; }
        public string SourceCategory { get; set; }

        public Int32 ItemId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Item)
                return ((Item)obj).ItemId == ItemId && ((Item)obj).SourceCategory == SourceCategory;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ItemId.GetHashCode() + SourceCategory.GetHashCode();
        }
    }
}
