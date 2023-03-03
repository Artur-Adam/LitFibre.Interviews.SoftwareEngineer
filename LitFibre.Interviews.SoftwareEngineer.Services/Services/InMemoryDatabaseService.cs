using LitFibre.Interviews.SoftwareEngineer.Models.Data;
using LitFibre.Interviews.SoftwareEngineer.Services.Interfaces;


namespace LitFibre.Interviews.SoftwareEngineer.Services.Services
{
    public class InMemoryDatabaseService<T> : IMemoryDatabase<T>
        where T : DatabaseObject
    {
        // We are using a simple dict to store the orders in memory
        private Dictionary<string, T> _database = new Dictionary<string, T>();

        public void Delete(string id)
        {
            _database.Remove(id);
        }

        public void Delete(T item)
        {
            _database.Remove(item.Id);
        }

        public void Push(T item)
        {
            if (item == null)
            {
                return;
            }

            _database[item.Id] = item as T;
        }

        public IEnumerable<T> Query(Predicate<T> predicate)
        {
            return _database.Values.Where(o => predicate(o));
        }

        public T Read(string id)
        {
            if (_database.TryGetValue(id, out var value))
            {
                return _database[id];
            }
            return null;
        }
    }
}
