using System;
using System.Collections.Generic;
using System.Linq;

namespace WatchZone.Domain.Model.Cart
{
    public class Cart
    {
        private List<CartItem> _items = new List<CartItem>();

        public IEnumerable<CartItem> Items => _items;

        public void AddItem(CartItem item)
        {
            var existingItem = _items.FirstOrDefault(i => i.WatchId == item.WatchId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                item.AddedDate = DateTime.Now;
                _items.Add(item);
            }
        }

        public void RemoveItem(int watchId)
        {
            _items.RemoveAll(i => i.WatchId == watchId);
        }

        public void UpdateQuantity(int watchId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.WatchId == watchId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }

        public decimal GetTotal()
        {
            return _items.Sum(i => i.Price * i.Quantity);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
} 