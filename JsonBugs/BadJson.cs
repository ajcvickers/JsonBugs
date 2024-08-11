using System.Data;
using Microsoft.Data.SqlClient;

namespace JsonBugs;

public static class BadJson
{
  private const string DatabaseName = "JsonUpdateJsonTypeTest";
  private const string TableName = "JsonScratchBJ1";

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
         [SomeJson] json NULL,
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
         SELECT [m].[Id], [m].[SomeJson]
         FROM (
             SELECT * FROM [{TableName}] AS j
         ) AS [m]
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
      param.ParameterName = "p0";
      param.Value = 1;
      command.Parameters.Add(param);

      param = command.CreateParameter();
      param.SqlDbType = (SqlDbType)35;
      param.ParameterName = "p1";
      param.Value =
        """
        {
          "Name": "e1_r",
          "Names": [
            "e1_r1",
            "e1_r2"
          ],
          "Decimal": 10.0,
          "Numbers": [
            -2147483648,
            -1,
            0,
            1,
            2147483647
          ],
          "OwnedCollectionBranch": [
            {
              "Date": "2101-01-01T00:00:00",
              "Enum": 2,
              "Enums": [
                -1,
                -1,
                2
              ],
              "Fraction": 10.1,
              "NullableEnum": -1,
              "NullableEnums": [
                null,
                -1,
                2
              ],
              "OwnedCollectionLeaf": [
                {
                  "SomethingSomething": "e1_r_c1_c1"
                },
                {
                  "SomethingSomething": "e1_r_c1_c2"
                }
              ],
              "OwnedReferenceLeaf": {
                "SomethingSomething": "e1_r_c1_r"
              }
            },
            {
              "Date": "2102-01-01T00:00:00",
              "Enum": -3,
              "Enums": [
                -1,
                -1,
                2
              ],
              "Fraction": 10.2,
              "NullableEnum": 2,
              "NullableEnums": [
                null,
                -1,
                2
              ],
              "OwnedCollectionLeaf": [
                {
                  "SomethingSomething": "e1_r_c2_c1"
                },
                {
                  "SomethingSomething": "e1_r_c2_c2"
                }
              ],
              "OwnedReferenceLeaf": {
                "SomethingSomething": "e1_r_c2_r"
              }
            }
          ],
          "OwnedReferenceBranch": {
            "Date": "2100-01-01T00:00:00",
            "Enum": -1,
            "Enums": [
              -1,
              -1,
              2
            ],
            "Fraction": 10.0,
            "NullableEnum": null,
            "NullableEnums": [
              null,
              -1,
              2
            ],
            "OwnedCollectionLeaf": [
              {
                "SomethingSomething": "e1_r_r_c1"
              },
              {
                "SomethingSomething": "e1_r_r_c2"
              }
            ],
            "OwnedReferenceLeaf": {
              "SomethingSomething": "e1_r_r_r"
            }
          }
        }
        """;
      command.Parameters.Add(param);

      command.CommandText =
        $"""
         INSERT INTO [{TableName}] ([Id], [SomeJson])
         VALUES (@p0, @p1);
         """;

      await command.ExecuteNonQueryAsync();
      Console.WriteLine("Inserted.");
    }
  }
}
