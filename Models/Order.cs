namespace RestoranoSistema.Models
{
    public class Order
    {
        public int Id { get; set; }
        private Item[] _items;
        public Order(int id, Item[] items) 
        {
            Id = id;
            _items = new Item[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                _items[i] = items[i];
            }
        }
        /// <summary>
        /// Gets all items from a list
        /// </summary>
        /// <returns>Item array</returns>
        public Item[] GetItems()
        {
            return (Item[])_items.Clone();
        }
    }
}
