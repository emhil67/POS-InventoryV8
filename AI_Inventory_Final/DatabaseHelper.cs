using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace AI_Inventory_Final
{
    public class DatabaseHelper
    {
        private readonly string _connectionString = "Server=localhost;Database=ai_inventory_db;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public DataTable GetCategories()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryID ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                }
            }
            return dt;
        }

        public void AddNewCategoryDirect(string categoryName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO Categories (CategoryName) VALUES (@catName)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@catName", categoryName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetAllProductsForSearch()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT p.ProductID, p.Barcode, p.ProductName, p.Description, c.CategoryName, 
                                        p.Price, p.CurrentStock, p.LowStockThreshold, p.ExpirationDate, p.ProductEmbedding 
                                 FROM Products p 
                                 JOIN Categories c ON p.CategoryID = c.CategoryID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                }
            }
            return dt;
        }

        public DataTable GetLowStockProducts()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT p.ProductID, p.ProductName, p.CurrentStock, p.LowStockThreshold, c.CategoryName 
                                 FROM Products p
                                 JOIN Categories c ON p.CategoryID = c.CategoryID
                                 WHERE p.CurrentStock <= p.LowStockThreshold ORDER BY p.CurrentStock ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                }
            }
            return dt;
        }

        public DataTable GetExpiringProducts()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT ProductID, Barcode, ProductName, CurrentStock, ExpirationDate,
                                        DATEDIFF(ExpirationDate, CURDATE()) AS DaysRemaining
                                 FROM Products 
                                 WHERE ExpirationDate IS NOT NULL 
                                   AND CurrentStock > 0 
                                   AND DATEDIFF(ExpirationDate, CURDATE()) <= 180
                                 ORDER BY ExpirationDate ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                }
            }
            return dt;
        }

        public void AddProductWithBarcode(string barcode, string name, string description, int categoryId, decimal price, int stock, int threshold, DateTime? expiryDate, string embeddingText)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO Products (Barcode, ProductName, Description, CategoryID, Price, CurrentStock, LowStockThreshold, ExpirationDate, ProductEmbedding) 
                                 VALUES (@barcode, @name, @desc, @catId, @price, @stock, @threshold, @expiry, @embedding)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@barcode", string.IsNullOrEmpty(barcode) ? (object)DBNull.Value : barcode);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@catId", categoryId);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@threshold", threshold);
                    cmd.Parameters.AddWithValue("@expiry", expiryDate.HasValue ? (object)expiryDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@embedding", embeddingText);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ProcessProductRestock(int productId, int quantity, DateTime? expiryDate, string remarks)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string selectQuery = "SELECT Barcode, ProductName, Description, CategoryID, Price, LowStockThreshold, ProductEmbedding FROM Products WHERE ProductID = @pid";
                        string barcode = "", name = "", desc = "", embedding = "";
                        int catId = 0, threshold = 5;
                        decimal price = 0;

                        using (var selectCmd = new MySqlCommand(selectQuery, conn, transaction))
                        {
                            selectCmd.Parameters.AddWithValue("@pid", productId);
                            using (var reader = selectCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    barcode = reader["Barcode"].ToString();
                                    name = reader["ProductName"].ToString();
                                    desc = reader["Description"].ToString();
                                    catId = Convert.ToInt32(reader["CategoryID"]);
                                    price = Convert.ToDecimal(reader["Price"]);
                                    threshold = Convert.ToInt32(reader["LowStockThreshold"]);
                                    embedding = reader["ProductEmbedding"].ToString();
                                }
                            }
                        }

                        string insertBatchQuery = @"INSERT INTO Products (Barcode, ProductName, Description, CategoryID, Price, CurrentStock, LowStockThreshold, ExpirationDate, ProductEmbedding) 
                                                    VALUES (@barcode, @name, @desc, @catId, @price, @stock, @threshold, @expiry, @embedding)";
                        using (var insertCmd = new MySqlCommand(insertBatchQuery, conn, transaction))
                        {
                            insertCmd.Parameters.AddWithValue("@barcode", barcode);
                            insertCmd.Parameters.AddWithValue("@name", name);
                            insertCmd.Parameters.AddWithValue("@desc", desc);
                            insertCmd.Parameters.AddWithValue("@catId", catId);
                            insertCmd.Parameters.AddWithValue("@price", price);
                            insertCmd.Parameters.AddWithValue("@stock", quantity);
                            insertCmd.Parameters.AddWithValue("@threshold", threshold);
                            insertCmd.Parameters.AddWithValue("@expiry", expiryDate.HasValue ? (object)expiryDate.Value.Date : DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@embedding", embedding);
                            insertCmd.ExecuteNonQuery();
                        }

                        string logQuery = "INSERT INTO StockTransactions (ProductID, Barcode, UserID, TransactionType, Quantity, Remarks) VALUES (@pid, @barcode, 1, 'STOCK-IN', @qty, @remarks)";
                        using (var logCmd = new MySqlCommand(logQuery, conn, transaction))
                        {
                            logCmd.Parameters.AddWithValue("@pid", productId);
                            logCmd.Parameters.AddWithValue("@barcode", barcode);
                            logCmd.Parameters.AddWithValue("@qty", quantity);
                            logCmd.Parameters.AddWithValue("@remarks", remarks);
                            logCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public void ProcessMultiItemFIFOSale(string orderGroupId, int productId, string barcode, int qty, string discType, decimal discAmt, decimal finalTotal, decimal cash, decimal change, int customerId, string remarks)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = "UPDATE Products SET CurrentStock = CurrentStock - @qty WHERE ProductID = @pid";
                        using (var updateCmd = new MySqlCommand(updateQuery, conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@qty", qty);
                            updateCmd.Parameters.AddWithValue("@pid", productId);
                            updateCmd.ExecuteNonQuery();
                        }
                        string logQuery = @"INSERT INTO StockTransactions (ProductID, OrderGroupId, Barcode, UserID, CustomerID, TransactionType, DiscountType, DiscountAmount, FinalTotal, CashTendered, ChangeDue, Quantity, Remarks)
VALUES (@pid, @ogid, @barcode, 1, @cid, 'STOCK-OUT', @dType, @dAmt, @fTotal, @cash, @change, @qty, @remarks)";
                        using (var logCmd = new MySqlCommand(logQuery, conn, transaction))
                        {
                            logCmd.Parameters.AddWithValue("@pid", productId);
                            logCmd.Parameters.AddWithValue("@ogid", orderGroupId);
                            logCmd.Parameters.AddWithValue("@barcode", barcode);
                            logCmd.Parameters.AddWithValue("@cid", customerId);
                            logCmd.Parameters.AddWithValue("@dType", discType);
                            logCmd.Parameters.AddWithValue("@dAmt", discAmt);
                            logCmd.Parameters.AddWithValue("@fTotal", finalTotal);
                            logCmd.Parameters.AddWithValue("@cash", cash);
                            logCmd.Parameters.AddWithValue("@change", change);
                            logCmd.Parameters.AddWithValue("@qty", qty);
                            logCmd.Parameters.AddWithValue("@remarks", remarks);
                            logCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }
        public DataTable GetCustomers()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT CustomerID, CustomerName FROM Customers ORDER BY CustomerID ASC";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                }
            }
            return dt;
        }
        public void AddUserAccount(string username, string password, string fullName, string role)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO users (Username, Password, FullName, Role) VALUES (@user, @pass, @name, @role)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@name", fullName);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateProductEntry(int productId, string name, string description, decimal price, int threshold)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "UPDATE Products SET ProductName = @name, Description = @desc, Price = @price, LowStockThreshold = @threshold WHERE ProductID = @pid";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name); cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@price", price); cmd.Parameters.AddWithValue("@threshold", threshold);
                    cmd.Parameters.AddWithValue("@pid", productId); cmd.ExecuteNonQuery();
                }
            }
        }
        public void DeleteProduct(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Products WHERE ProductID = @pid";
                using (var cmd = new MySqlCommand(query, conn)) { cmd.Parameters.AddWithValue("@pid", productId); cmd.ExecuteNonQuery(); }
            }
        }
        public void ClearTodayTransactions()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM StockTransactions WHERE DATE(TransactionDate) = CURDATE()";
                using (var cmd = new MySqlCommand(query, conn)) { cmd.ExecuteNonQuery(); }
            }
        }
        public void DeleteSingleTransaction(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM StockTransactions WHERE TransactionID = @id";
                using (var cmd = new MySqlCommand(query, conn)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            }
        }
        public void LogActivity(string username, string role, string actionType, string description, string recordId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO activity_logs (Username, Role, ActionType, Description, RecordID)
VALUES (@user, @role, @action, @desc, @recId)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username); cmd.Parameters.AddWithValue("@role", role);
                        cmd.Parameters.AddWithValue("@action", actionType); cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@recId", string.IsNullOrEmpty(recordId) ? (object)DBNull.Value : recordId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
        // --- PERIODIC SALES LOGS MATRIX FILTERS ---
        public DataTable GetTodaySalesLog()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT t.TransactionID, t.OrderGroupId, p.ProductName, t.Quantity, t.FinalTotal AS TotalSales, t.TransactionDate
FROM StockTransactions t JOIN Products p ON t.ProductID = p.ProductID
WHERE t.TransactionType = 'STOCK-OUT' AND DATE(t.TransactionDate) = CURDATE() ORDER BY t.TransactionID DESC";
                using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); } }
            }
            return dt;
        }
        public DataTable GetWeeklySalesLog()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT t.TransactionID, t.OrderGroupId, p.ProductName, t.Quantity, t.FinalTotal AS TotalSales, t.TransactionDate
FROM StockTransactions t JOIN Products p ON t.ProductID = p.ProductID
WHERE t.TransactionType = 'STOCK-OUT' AND t.TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY) ORDER BY t.TransactionID DESC";
                using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); } }
            }
            return dt;
        }
        public DataTable GetMonthlySalesLog()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT t.TransactionID, t.OrderGroupId, p.ProductName, t.Quantity, t.FinalTotal AS TotalSales, t.TransactionDate
FROM StockTransactions t JOIN Products p ON t.ProductID = p.ProductID
WHERE t.TransactionType = 'STOCK-OUT' AND t.TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY) ORDER BY t.TransactionID DESC";
                using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); } }
            }
            return dt;
        }
        public decimal GetCalculatedRevenueByRange(int mode)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string intervalFilter = mode == 1 ? "INTERVAL 7 DAY" : "INTERVAL 30 DAY";
                string query = mode == 0
                ? "SELECT COALESCE(SUM(FinalTotal), 0) FROM StockTransactions WHERE TransactionType = 'STOCK-OUT' AND DATE(TransactionDate) = CURDATE()"
                : $"SELECT COALESCE(SUM(FinalTotal), 0) FROM StockTransactions WHERE TransactionType = 'STOCK-OUT' AND TransactionDate >= DATE_SUB(CURDATE(), {intervalFilter})";
                using (var cmd = new MySqlCommand(query, conn)) { return Convert.ToDecimal(cmd.ExecuteScalar()); }
            }
        }
        // 🔮 NEW SUPER RESILIENT AI LOGIC ENGINE: ENFORCES FULL BACKGROUND VALUE CAPTURE REGARDLESS OF BLANK TABLES
        public DataTable GetAIPredictiveStockForecast()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT p.ProductID, p.Barcode, p.ProductName, p.CurrentStock,
COALESCE(SUM(t.Quantity) / 30.0, 0.0) AS DailyVelocity,
CASE
WHEN COALESCE(SUM(t.Quantity) / 30.0, 0.0) > 0
THEN CEIL(p.CurrentStock / (SUM(t.Quantity) / 30.0))
ELSE 999
END AS EstimatedDaysLeft,
CASE
WHEN COALESCE(SUM(t.Quantity) / 30.0, 0.0) > 0
THEN CEIL((SUM(t.Quantity) / 30.0) * 30) - p.CurrentStock
ELSE p.LowStockThreshold * 2
END AS RecommendedRestockQty
FROM Products p
LEFT JOIN StockTransactions t ON p.ProductID = t.ProductID
AND t.TransactionType = 'STOCK-OUT'
AND t.TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
GROUP BY p.ProductID, p.Barcode, p.ProductName, p.CurrentStock, p.LowStockThreshold
ORDER BY EstimatedDaysLeft ASC";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); }
                    }
                }
            }

            catch
            {
                // Fallback table definition structure injection to guarantee absolute compiler zero crash conditions
                dt.Columns.Add("ProductID", typeof(int)); dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("ProductName", typeof(string)); dt.Columns.Add("CurrentStock", typeof(int));
                dt.Columns.Add("DailyVelocity", typeof(double)); dt.Columns.Add("EstimatedDaysLeft", typeof(int));
                dt.Columns.Add("RecommendedRestockQty", typeof(int));
            }
            return dt;
        }
    
// NEW: Pulls aggregated daily net sales for the past 7 days to plot the trend graph
public DataTable GetWeeklySalesChartData()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT DATE_FORMAT(TransactionDate, '%a') AS SalesPeriod, COALESCE(SUM(FinalTotal), 0) AS TotalRevenue 
                         FROM StockTransactions 
                         WHERE TransactionType = 'STOCK-OUT' AND TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)
                         GROUP BY DATE(TransactionDate), DATE_FORMAT(TransactionDate, '%a')
                         ORDER BY DATE(TransactionDate) ASC";
                using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); } }
            }
            return dt;
        }

        // NEW: Pulls aggregated daily net sales for the past 30 days to plot the trend graph
        public DataTable GetMonthlySalesChartData()
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT DATE_FORMAT(TransactionDate, '%m-%d') AS SalesPeriod, COALESCE(SUM(FinalTotal), 0) AS TotalRevenue 
                         FROM StockTransactions 
                         WHERE TransactionType = 'STOCK-OUT' AND TransactionDate >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
                         GROUP BY DATE(TransactionDate), DATE_FORMAT(TransactionDate, '%m-%d')
                         ORDER BY DATE(TransactionDate) ASC";
                using (var cmd = new MySqlCommand(query, conn)) { using (var adapter = new MySqlDataAdapter(cmd)) { adapter.Fill(dt); } }
            }
            return dt;
        }
    }
}