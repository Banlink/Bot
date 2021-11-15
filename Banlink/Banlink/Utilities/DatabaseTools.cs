using System;
using SQLite;

namespace Banlink.Utilities
{
    public sealed class DatabaseTools : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private bool _disposed = false;
        public DatabaseTools(string location)
        {
            _connection = new SQLiteConnection(location);
        }
    
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~DatabaseTools()
        {
            Dispose(false);
        }
        
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing) _connection?.Dispose();

            _disposed = true;
        }
    }
}
