using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient; // CRITICAL FIX: Idinagdag ang MySQL driver connection link
using SmartComponents.LocalEmbeddings;

public class SemanticSearchService
{
    private readonly AI_Inventory_Final.DatabaseHelper _db = new AI_Inventory_Final.DatabaseHelper();
    private readonly LocalEmbedder _embedder = new LocalEmbedder();

    // Convert raw text into a Base64 string for XAMPP MySQL text column compatibility
    public string GetEmbeddingAsString(string text)
    {
        EmbeddingF32 embedding = _embedder.Embed(text);
        byte[] rawBytes = embedding.Buffer.ToArray();
        return Convert.ToBase64String(rawBytes);
    }

    // STRICT AI Semantic Search engine - Filters out irrelevant and unrelated results
    public DataTable SearchProducts(string userQuery, int maxResults = 5)
    {
        string cleanedQuery = userQuery.Trim().ToLower();

        // 1. Convert user search input into an AI vector
        EmbeddingF32 queryEmbedding = _embedder.Embed(userQuery);

        // 2. Fetch all products from XAMPP database
        DataTable allProducts = _db.GetAllProductsForSearch();
        var scoredResults = new List<(DataRow Row, float Score)>();

        foreach (DataRow row in allProducts.Rows)
        {
            string productName = row["ProductName"].ToString()!.ToLower();
            string description = row["Description"].ToString()!.ToLower();

            if (row["ProductEmbedding"] != DBNull.Value && !string.IsNullOrEmpty(row["ProductEmbedding"].ToString()))
            {
                // Parse Base64 string back into binary byte weights
                string base64Str = row["ProductEmbedding"].ToString()!;
                byte[] rawBytes = Convert.FromBase64String(base64Str);

                var productEmbedding = new EmbeddingF32(rawBytes);

                // Compute Cosine Similarity (Returns a score between 0.0 to 1.0)
                float similarityScore = LocalEmbedder.Similarity(queryEmbedding, productEmbedding);

                // --- CRITICAL FIX: STRICT SEMANTIC COHESION FILTER ---
                // 1. We raised the threshold to 0.55f (Strict boundary for localized semantic models).
                // 2. If the user types an exact keyword match, it must strictly be an exact word match 
                //    in the product name to bypass the threshold, preventing letter-matching contamination.
                bool isExactKeywordMatch = productName == cleanedQuery || productName.StartsWith(cleanedQuery + " ") || productName.EndsWith(" " + cleanedQuery) || productName.Contains(" " + cleanedQuery + " ");

                if (similarityScore >= 0.55f || isExactKeywordMatch)
                {
                    // If it's an exact name match, boost its score to ensure it lands at the very top row
                    float finalScore = isExactKeywordMatch ? similarityScore + 1.00f : similarityScore;
                    scoredResults.Add((row, finalScore));
                }
            }
        }

        // 3. Sort from highest score to lowest
        var topResults = scoredResults
            .OrderByDescending(r => r.Score)
            .Take(maxResults)
            .Select(r => r.Row)
            .ToList();

        // 4. Populate clean specific results back into a DataTable for the WinForms Grid view
        DataTable finalTable = allProducts.Clone();
        foreach (var row in topResults)
        {
            finalTable.ImportRow(row);
        }

        return finalTable;
    }
}
