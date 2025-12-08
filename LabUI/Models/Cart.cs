using System.Text.Json;

namespace LabUI.Models
{
    public class Cart
    {
        public Dictionary<int, CartItem> CartItems { get; set; } = new();

        public virtual void AddToCart(Dish dish)
        {
            if (CartItems.ContainsKey(dish.Id))
            {
                CartItems[dish.Id].Qty++;
            }
            else
            {
                CartItems.Add(dish.Id, new CartItem
                {
                    Item = dish,
                    Qty = 1
                });
            }
        }

        public virtual void RemoveItems(int id)
        {
            CartItems.Remove(id);
        }

        public virtual void ClearAll()
        {
            CartItems.Clear();
        }

        public int Count => CartItems.Sum(item => item.Value.Qty);
        public decimal TotalPrice => CartItems.Sum(item => item.Value.Item.Price * item.Value.Qty);
    }
}