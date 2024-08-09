using Microsoft.Data.SqlClient;
using System.Data;

var testDatabase = "JsonUpdateJsonTypeTest";
var password = UserSecretsExtensions.Read("SqlTestPass");

using (var connection = new SqlConnection(
           $"Data Source=10.224.90.149,1433;Initial Catalog={testDatabase};User ID=sa;Encrypt=False;Password={password};Trust Server Certificate=True;"))
{
    await connection.OpenAsync();
    await CreateTable(connection);
    await Insert(connection);
    await Query(connection);
    await Update(connection);
    await Query(connection);
}

async Task Query(SqlConnection sqlConnection)
{

    Console.WriteLine("Querying...");
    await using (var command = sqlConnection.CreateCommand())
    {
        command.CommandText =
            """
            SELECT [j].[Id], [j].[SomeJson]
            FROM [JsonScratch1] AS [j]
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            Console.WriteLine(reader.IsDBNull(1) ? "<null>" : reader.GetString(1));
        }
    }
    Console.WriteLine("Queried.");
}

async Task CreateTable(SqlConnection connection1)
{
    await using (var command = connection1.CreateCommand())
    {
      try
      {
        command.CommandText =
          """
          DROP TABLE [JsonScratch1];
          """;

        await command.ExecuteNonQueryAsync();
      }
      catch
      {
        // ignored
      }
    }

    await using (var command = connection1.CreateCommand())
    {
        Console.WriteLine("Creating table...");
        command.CommandText =
            """
            CREATE TABLE [JsonScratch1] (
                [Id] int NOT NULL,
                [SomeJson] json NULL,
                CONSTRAINT [PK_JsonScratch1] PRIMARY KEY ([Id]),
            );
            """;

        await command.ExecuteNonQueryAsync();
        Console.WriteLine("Created table.");
    }
}

async Task Insert(SqlConnection sqlConnection1)
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
              "Number": 10,
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
            """
            INSERT INTO [JsonScratch1] ([Id], [SomeJson])
            VALUES (@p0, @p1);
            """;

        await command.ExecuteNonQueryAsync();
        Console.WriteLine("Inserted.");
    }
}

async Task Update(SqlConnection connection2)
{
    Console.WriteLine("Updating...");
    await using (var command = connection2.CreateCommand())
    {
        var param = command.CreateParameter();
        param.ParameterName = "p0";
        param.Value =
            """
            {
              "Date": "2100-01-01T00:00:00",
              "Enum": -1,
              "Enums": [
                -1,
                -1,
                2
              ],
              "Fraction": 523.532,
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
                "SomethingSomething": "edit"
              }
            }
            """;

        param.DbType = DbType.String;
        param.SqlDbType = (SqlDbType)35;
        command.Parameters.Add(param);

        param = command.CreateParameter();
        param.ParameterName = "p1";
        param.Value = 1;
        command.Parameters.Add(param);

        command.CommandText =
            """
            UPDATE [JsonScratch1] SET [SomeJson] = JSON_MODIFY([SomeJson], 'strict $.OwnedReferenceBranch', CAST (JSON_QUERY(@p0) AS nvarchar(max)))
            OUTPUT 1
            WHERE [Id] = @p1;
            """;

        await using (var reader = await command.ExecuteReaderAsync())
        {
          while (reader.Read())
          {
          }
        }

        Console.WriteLine("Updated.");
    }
}