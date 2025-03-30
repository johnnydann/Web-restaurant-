namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Thuộc tính UserId để liên kết với người dùng
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId ==
            item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }
        // Các phuong th?c khác...
    }
}
