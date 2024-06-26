using System.Data.SqlClient;
using Dapper;
using Newtonsoft.Json.Linq;

namespace Militaria1.Repositories;

public class BillingEntryRepository
{
    private readonly string _connectionString;

    public BillingEntryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveBillingEntriesToDatabase(List<JObject> billingEntries)
    {
        await using var connection = new SqlConnection(_connectionString);
        try
        {
            foreach (var entry in billingEntries)
            {
                var sql = @"
                MERGE [dbo].[BillingEntries] AS target
                USING (VALUES 
                    (@Id, @OccurredAt, @TypeId, @TypeName, @OfferId, 
                    @OfferName, @ValueAmount, @ValueCurrency, 
                    @TaxPercentage, @BalanceAmount, @BalanceCurrency, 
                    @OrderId)) AS source 
                    (Id, OccurredAt, TypeId, TypeName, OfferId, 
                    OfferName, ValueAmount, ValueCurrency, 
                    TaxPercentage, BalanceAmount, BalanceCurrency, 
                    OrderId)
                ON target.Id = source.Id
                WHEN MATCHED THEN 
                    UPDATE SET 
                        OccurredAt = source.OccurredAt,
                        TypeId = source.TypeId,
                        TypeName = source.TypeName,
                        OfferId = source.OfferId,
                        OfferName = source.OfferName,
                        ValueAmount = source.ValueAmount,
                        ValueCurrency = source.ValueCurrency,
                        TaxPercentage = source.TaxPercentage,
                        BalanceAmount = source.BalanceAmount,
                        BalanceCurrency = source.BalanceCurrency,
                        OrderId = source.OrderId
                WHEN NOT MATCHED THEN
                    INSERT (
                        [id], [occurredAt], [typeId], [typeName], [offerId], 
                        [offerName], [valueAmount], [valueCurrency], 
                        [taxPercentage], [balanceAmount], [balanceCurrency], 
                        [orderId]
                    ) 
                    VALUES (
                        source.Id, source.OccurredAt, source.TypeId, source.TypeName, source.OfferId, 
                        source.OfferName, source.ValueAmount, source.ValueCurrency, 
                        source.TaxPercentage, source.BalanceAmount, source.BalanceCurrency, 
                        source.OrderId
                    );";

                var parameters = new
                {
                    Id = (string)entry["id"],
                    OccurredAt = (DateTime)entry["occurredAt"],
                    TypeId = (string)entry["type"]["id"],
                    TypeName = (string)entry["type"]["name"],
                    OfferId = (string)entry["offer"]?["id"],
                    OfferName = (string)entry["offer"]?["name"],
                    ValueAmount = (decimal)entry["value"]["amount"],
                    ValueCurrency = (string)entry["value"]["currency"],
                    TaxPercentage = (decimal)entry["tax"]["percentage"],
                    BalanceAmount = (decimal)entry["balance"]["amount"],
                    BalanceCurrency = (string)entry["balance"]["currency"],
                    OrderId = (string)entry["order"]?["id"]
                };

                await connection.ExecuteAsync(sql, parameters);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}