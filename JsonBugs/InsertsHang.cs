using System.Data;
using Microsoft.Data.SqlClient;

namespace JsonBugs;

public static class InsertsHang
{
    private const string DatabaseName = "JsonUpdateJsonTypeTest";
    private const string TableName = "JsonScratchH1";

    public static async Task Repro()
    {
        var password = UserSecretsExtensions.Read("SqlTestPass");
        using (var connection = new SqlConnection(
                   $"Data Source=10.224.90.149,1433;Initial Catalog={DatabaseName};User ID=sa;Encrypt=False;Password={password};Trust Server Certificate=True;"))
        {
            await connection.OpenAsync();

            await InitializeTable(connection);
            await Insert(connection);
            await Query(connection);
 
            await connection.CloseAsync();
        }
    }

    private static async Task InitializeTable(SqlConnection connection1)
    {
        await using (var command = connection1.CreateCommand())
        {
            Console.WriteLine("Dropping table...");
            try
            {
                command.CommandText = $"DROP TABLE [{TableName}]";
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // ignored
            }

            Console.WriteLine("Dropped.");
        }

        await using (var command = connection1.CreateCommand())
        {
            Console.WriteLine("Creating table...");
            command.CommandText =
                $"""
                 CREATE TABLE [{TableName}] (
                 [Id] int NOT NULL,
                 [TestDefaultStringCollection] json NULL,
                 [TestMaxLengthStringCollection] json NULL,
                 [TestInt16Collection] json NULL,
                 [TestInt32Collection] json NULL,
                 [TestDecimalCollection] json NULL,
                 [TestDateTimeCollection] json NULL,
                 [TestDateTimeOffsetCollection] json NULL,
                 [TestTimeSpanCollection] json NULL,
                 [TestInt64Collection] json NULL,
                 [TestDoubleCollection] json NULL,
                 [TestSingleCollection] json NULL,
                 [TestBooleanCollection] json NULL,
                 [TestCharacterCollection] json NULL,
                 [TestByteCollection] json NULL,
                 [TestGuidCollection] json NOT NULL,
                 [TestUnsignedInt16Collection] json NULL,
                 [TestUnsignedInt32Collection] json NULL,
                 [TestUnsignedInt64Collection] json NULL,
                 [TestSignedByteCollection] json NULL,
                 [TestNullableInt32Collection] json NULL,
                 [TestEnumCollection] json NULL,
                 [TestEnumWithIntConverterCollection] json NULL,
                 [TestNullableEnumCollection] json NULL,
                 [TestNullableEnumWithIntConverterCollection] json NULL,
                 [TestNullableEnumWithConverterThatHandlesNullsCollection] json NULL,
                 [Collection] json NULL,
                 [Reference] json NULL,
                 CONSTRAINT [PK_{TableName}] PRIMARY KEY ([Id]));
                 """;

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Created.");
        }
    }

    private static async Task Query(SqlConnection sqlConnection)
    {
        Console.WriteLine("Querying...");
        await using (var command = sqlConnection.CreateCommand())
        {
            command.CommandText =
                $"""
                 SELECT *
                 FROM [{TableName}] AS [j]
                 """;

            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetInt32(0)}: {(reader.IsDBNull(1) ? "<null>" : reader.GetString(1))}");
            }
        }

        Console.WriteLine("Queried.");
    }

    private static async Task Insert(SqlConnection sqlConnection1)
    {
        Console.WriteLine("Inserting...");
        await using (var command = sqlConnection1.CreateCommand())
        {
            var param = command.CreateParameter();
            for (var i = 0; i <= 26; i++)
            {
                param = command.CreateParameter();
                param.ParameterName = $"p{i}";

                if (i == 0)
                {
                    param.SqlDbType = (SqlDbType)35;
                    param.Value =
                        """
                        [{"TestBoolean":false,"TestBooleanCollection":[],"TestByte":0,"TestByteArray":null,"TestByteCollection":null,"TestCharacter":"\u0000","TestCharacterCollection":[],"TestDateOnly":"0001-01-01","TestDateOnlyCollection":[],"TestDateTime":"0001-01-01T00:00:00","TestDateTimeCollection":[],"TestDateTimeOffset":"0001-01-01T00:00:00+00:00","TestDateTimeOffsetCollection":[],"TestDecimal":0,"TestDecimalCollection":[],"TestDefaultString":null,"TestDefaultStringCollection":[],"TestDouble":0,"TestDoubleCollection":[],"TestEnum":0,"TestEnumCollection":[],"TestEnumWithIntConverter":0,"TestEnumWithIntConverterCollection":[],"TestGuid":"00000000-0000-0000-0000-000000000000","TestGuidCollection":[],"TestInt16":0,"TestInt16Collection":[],"TestInt32":0,"TestInt32Collection":[],"TestInt64":0,"TestInt64Collection":[],"TestMaxLengthString":null,"TestMaxLengthStringCollection":[],"TestNullableEnum":null,"TestNullableEnumCollection":[],"TestNullableEnumWithConverterThatHandlesNulls":null,"TestNullableEnumWithConverterThatHandlesNullsCollection":[],"TestNullableEnumWithIntConverter":null,"TestNullableEnumWithIntConverterCollection":[],"TestNullableInt32":null,"TestNullableInt32Collection":[],"TestSignedByte":0,"TestSignedByteCollection":[],"TestSingle":0,"TestSingleCollection":[],"TestTimeOnly":"00:00:00.0000000","TestTimeOnlyCollection":[],"TestTimeSpan":"0:00:00","TestTimeSpanCollection":[],"TestUnsignedInt16":0,"TestUnsignedInt16Collection":[],"TestUnsignedInt32":0,"TestUnsignedInt32Collection":[],"TestUnsignedInt64":0,"TestUnsignedInt64Collection":[]}]
                        """;
                }
                else if (i == 1)
                {
                    param.Value = 1;
                    param.DbType = DbType.Int32;
                }
                else if (i == 3 || i == 18)
                {
                    param.Value = DBNull.Value;
                    param.SqlDbType = (SqlDbType)35;
                }
                else
                {
                    param.Value = "[]";
                    param.SqlDbType = (SqlDbType)35;
                }

                command.Parameters.Add(param);

                command.CommandText =
                    $"""
                     INSERT INTO [{TableName}] ([Collection], [Id], [TestBooleanCollection], [TestByteCollection], [TestCharacterCollection], [TestDateTimeCollection], [TestDateTimeOffsetCollection], [TestDecimalCollection], [TestDefaultStringCollection], [TestDoubleCollection], [TestEnumCollection], [TestEnumWithIntConverterCollection], [TestGuidCollection], [TestInt16Collection], [TestInt32Collection], [TestInt64Collection], [TestMaxLengthStringCollection], [TestNullableEnumCollection], [TestNullableEnumWithConverterThatHandlesNullsCollection], [TestNullableEnumWithIntConverterCollection], [TestNullableInt32Collection], [TestSignedByteCollection], [TestSingleCollection], [TestTimeSpanCollection], [TestUnsignedInt16Collection], [TestUnsignedInt32Collection], [TestUnsignedInt64Collection])
                     VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21, @p22, @p23, @p24, @p25, @p26);
                     """;

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Inserted.");
            }
        }
    }
}
