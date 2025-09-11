namespace RestoranoSistema.Models
{
    public class Hall
    {
        public Hall(int id, string name, Table[] tables)
        {
            Id = id;
            Name = name;
            _tables = new Table[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                _tables[i] = tables[i];
            }
        } 
        public int Id { get; set; }
        public string Name { get; set; }
        private Table[] _tables { get; set; }
        
    }
}
