using MongoDB.Bson;
using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Models.Reports;

namespace quan_ly_kho_hang.Repositories.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _db;
        private readonly IMongoCollection<ReceiptOut> _receiptOuts;
        private readonly IMongoCollection<ReceiptIn> _receiptIns;
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Inventory> _inventories;
        private readonly IMongoCollection<Alert> _alerts;
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public ReportRepository(AppDbContext db)
        {
            _db = db;
            _receiptOuts = _db.ReceiptOuts;
            _receiptIns = _db.ReceiptIns;
            _products = _db.Products;
            _inventories = _db.Inventories;
            _alerts = _db.Alerts;
            _auditLogs = _db.AuditLogs;
        }

        public async Task<IEnumerable<SalesSummaryDto>> GetSalesSummaryAsync(DateTime from, DateTime to, string groupBy)
        {
            // groupBy: 'day' or 'month'
            var project = new BsonDocument
            {
                { "items", "$Items" },
                { "createdAt", "$CreatedAt" }
            };

            var unwind = new BsonDocument("$unwind", "$items");

            // compute revenue = items.Quantity * items.UnitPrice
            var addFields = new BsonDocument("$addFields", new BsonDocument("revenue", new BsonDocument("$multiply", new BsonArray { "$items.Quantity", "$items.UnitPrice" })));

            // match by date
            var match = new BsonDocument("$match", new BsonDocument {
                { "CreatedAt", new BsonDocument { { "$gte", from }, { "$lte", to } } }
            });

            // group key
            BsonDocument groupKey;
            if (groupBy == "month")
            {
                groupKey = new BsonDocument {
                    { "year", new BsonDocument("$year", "$CreatedAt") },
                    { "month", new BsonDocument("$month", "$CreatedAt") }
                };
            }
            else // default day
            {
                groupKey = new BsonDocument {
                    { "year", new BsonDocument("$year", "$CreatedAt") },
                    { "month", new BsonDocument("$month", "$CreatedAt") },
                    { "day", new BsonDocument("$dayOfMonth", "$CreatedAt") }
                };
            }

            var group = new BsonDocument("$group", new BsonDocument {
                { "_id", groupKey },
                { "TotalQuantity", new BsonDocument("$sum", "$items.Quantity") },
                { "TotalRevenue", new BsonDocument("$sum", "$revenue") }
            });

            var sort = new BsonDocument("$sort", new BsonDocument("_id", 1));

            var pipeline = new[] { match, unwind, addFields, group, sort };

            var result = await _receiptOuts.AggregateAsync<BsonDocument>(pipeline);
            var list = await result.ToListAsync();

            var outList = list.Select(d =>
            {
                var idDoc = d["_id"].AsBsonDocument;
                DateTime dt;
                if (groupBy == "month")
                {
                    dt = new DateTime(idDoc["year"].AsInt32, idDoc["month"].AsInt32, 1);
                }
                else
                {
                    dt = new DateTime(idDoc["year"].AsInt32, idDoc["month"].AsInt32, idDoc["day"].AsInt32);
                }

                return new SalesSummaryDto
                {
                    Period = dt,
                    TotalQuantity = d["TotalQuantity"].AsInt32,
                    TotalRevenue = (decimal)d["TotalRevenue"].ToDecimal()
                };
            });

            return outList;
        }

        public async Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(DateTime from, DateTime to, int limit = 10)
        {
            var match = new BsonDocument("$match", new BsonDocument {
                { "CreatedAt", new BsonDocument { { "$gte", from }, { "$lte", to } } }
            });
            var unwind = new BsonDocument("$unwind", "$Items");
            var group = new BsonDocument("$group", new BsonDocument {
                { "_id", "$Items.ProductId" },
                { "TotalSold", new BsonDocument("$sum", "$Items.Quantity") },
                { "TotalRevenue", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) }
            });
            var sort = new BsonDocument("$sort", new BsonDocument("TotalSold", -1));
            var limitDoc = new BsonDocument("$limit", limit);

            var lookup = new BsonDocument("$lookup", new BsonDocument {
                { "from", "Products" },
                { "localField", "_id" },
                { "foreignField", "_id" },
                { "as", "product" }
            });
            var unwindProduct = new BsonDocument("$unwind", new BsonDocument { { "path", "$product" }, { "preserveNullAndEmptyArrays", true } });

            var project = new BsonDocument("$project", new BsonDocument {
                { "ProductId", "$_id" },
                { "TotalSold", 1 },
                { "TotalRevenue", 1 },
                { "ProductName", new BsonDocument("$ifNull", new BsonArray { "$product.Name", "Unknown" }) }
            });

            var pipeline = new[] { match, unwind, group, sort, limitDoc, lookup, unwindProduct, project };

            var agg = await _receiptOuts.AggregateAsync<BsonDocument>(pipeline);
            var docs = await agg.ToListAsync();

            return docs.Select(d => new TopProductDto
            {
                ProductId = d["ProductId"].ToString(),
                ProductName = d["ProductName"].AsString,
                TotalSold = d["TotalSold"].AsInt32,
                TotalRevenue = (decimal)d["TotalRevenue"].ToDecimal()
            });
        }

        public async Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime from, DateTime to, string groupBy)
        {
            // very similar to Sales but from ReceiptIns
            var match = new BsonDocument("$match", new BsonDocument {
                { "CreatedAt", new BsonDocument { { "$gte", from }, { "$lte", to } } }
            });
            var unwind = new BsonDocument("$unwind", "$Items");
            var addFields = new BsonDocument("$addFields", new BsonDocument("cost", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })));

            BsonDocument groupKey;
            if (groupBy == "month")
            {
                groupKey = new BsonDocument {
                    { "year", new BsonDocument("$year", "$CreatedAt") },
                    { "month", new BsonDocument("$month", "$CreatedAt") }
                };
            }
            else
            {
                groupKey = new BsonDocument {
                    { "year", new BsonDocument("$year", "$CreatedAt") },
                    { "month", new BsonDocument("$month", "$CreatedAt") },
                    { "day", new BsonDocument("$dayOfMonth", "$CreatedAt") }
                };
            }

            var group = new BsonDocument("$group", new BsonDocument {
                { "_id", groupKey },
                { "TotalQuantity", new BsonDocument("$sum", "$Items.Quantity") },
                { "TotalCost", new BsonDocument("$sum", "$cost") }
            });
            var sort = new BsonDocument("$sort", new BsonDocument("_id", 1));
            var pipeline = new[] { match, unwind, addFields, group, sort };

            var result = await _receiptIns.AggregateAsync<BsonDocument>(pipeline);
            var list = await result.ToListAsync();

            var outList = list.Select(d =>
            {
                var idDoc = d["_id"].AsBsonDocument;
                DateTime dt;
                if (groupBy == "month")
                {
                    dt = new DateTime(idDoc["year"].AsInt32, idDoc["month"].AsInt32, 1);
                }
                else
                {
                    dt = new DateTime(idDoc["year"].AsInt32, idDoc["month"].AsInt32, idDoc["day"].AsInt32);
                }

                return new PurchaseSummaryDto
                {
                    Period = dt,
                    TotalQuantity = d["TotalQuantity"].AsInt32,
                    TotalCost = (decimal)d["TotalCost"].ToDecimal()
                };
            });

            return outList;
        }

        public async Task<IEnumerable<InventoryValuationDto>> GetInventoryValuationAsync()
        {
            // Join Products collection (uses Quantity & Price)
            var project = new BsonDocument("$project", new BsonDocument {
                { "_id", 1 },
                { "Name", 1 },
                { "Quantity", 1 },
                { "Price", 1 }
            });

            var pipeline = new[] { project };

            var cursor = await _products.AggregateAsync<BsonDocument>(pipeline);
            var docs = await cursor.ToListAsync();

            return docs.Select(d => new InventoryValuationDto
            {
                ProductId = d["_id"].ToString(),
                ProductName = d.GetValue("Name", BsonNull.Value).AsString,
                Quantity = d.GetValue("Quantity", 0).ToInt32(),
                UnitPrice = d.GetValue("Price", 0).ToDecimal()
            });
        }

        public async Task<IEnumerable<LowStockDto>> GetLowStockAsync(int defaultThreshold = 10)
        {
            // Assumes 'Alert' collection may store thresholds; otherwise use defaultThreshold
            // Strategy: if Alerts collection has threshold entries for products, prefer them; else fallback to product.Quantity <= defaultThreshold

            // Simple: find products with Quantity <= defaultThreshold
            var filter = Builders<Product>.Filter.Lte(p => p.Quantity, defaultThreshold);
            var projection = Builders<Product>.Projection.Include(p => p.Id).Include(p => p.Name).Include(p => p.Quantity);
            var cursor = await _products.FindAsync(filter, new FindOptions<Product, BsonDocument> { Projection = projection });
            var docs = await cursor.ToListAsync();

            return docs.Select(d => new LowStockDto
            {
                ProductId = d["_id"].ToString(),
                ProductName = d.GetValue("Name", "").AsString,
                Quantity = d.GetValue("Quantity", 0).ToInt32(),
                Threshold = defaultThreshold
            });
        }

        public async Task<IEnumerable<InventoryDifferenceDto>> GetInventoryDifferencesAsync(string inventoryId)
        {
            var inv = await _inventories.Find(i => i.Id == inventoryId).FirstOrDefaultAsync();
            if (inv == null) return Enumerable.Empty<InventoryDifferenceDto>();

            // Map items
            var productIds = inv.Items.Select(x => x.ProductId).Where(x => !string.IsNullOrEmpty(x)).ToList();
            var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
            var prodDict = products.ToDictionary(p => p.Id!, p => p);

            var list = inv.Items.Select(it => new InventoryDifferenceDto
            {
                InventoryId = inv.Id!,
                ProductId = it.ProductId!,
                ProductName = it.ProductName ?? (prodDict.ContainsKey(it.ProductId) ? prodDict[it.ProductId].Name : ""),
                ExpectedQuantity = it.ExpectedQuantity,
                CountedQuantity = it.CountedQuantity
            });

            return list;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime from, DateTime to, int limit = 100)
        {
            var filter = Builders<AuditLog>.Filter.Gte(a => a.Timestamp, from) &
                         Builders<AuditLog>.Filter.Lte(a => a.Timestamp, to);
            var sort = Builders<AuditLog>.Sort.Descending(a => a.Timestamp);
            var cursor = await _auditLogs.Find(filter).Sort(sort).Limit(limit).ToListAsync();

            return cursor.Select(a => new AuditLogDto
            {
                Id = a.Id ?? "",
                Action = a.Action,
                PerformedByEmail = a.PerformedByEmail,
                Timestamp = a.Timestamp,
                Details = a.Details
            });
        }
    }
}
