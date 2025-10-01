using MongoDB.Driver;
using quan_ly_kho_hang.Models;
using Microsoft.Extensions.Configuration;

namespace quan_ly_kho_hang.Data
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;

        public AppDbContext(IConfiguration configuration)
        {
            
            var connectionString = configuration.GetConnectionString("MongoDb");
            var dbName = configuration.GetConnectionString("DatabaseName");

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<ReceiptIn> ReceiptIns => _database.GetCollection<ReceiptIn>("ReceiptIns");
        public IMongoCollection<ReceiptOut> ReceiptOuts => _database.GetCollection<ReceiptOut>("ReceiptOuts");
        public IMongoCollection<Inventory> Inventories => _database.GetCollection<Inventory>("Inventories");
        public IMongoCollection<Alert> Alerts => _database.GetCollection<Alert>("Alerts");
        public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("AuditLogs");


    }
}
