using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Dynamic;

//MySql.Data.MySqlClient 8.0.25 Works -> Any higher version is broken

//appsettings.json will need to be updated to your connection string and own query below

class Program
{
    static async Task Main(string[] args)
    {
        //Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        //Get connection string
        string connectionString = configuration.GetConnectionString("DefaultConnection");


        Console.WriteLine("Program running...\n");

        //CHANGE QUERY AS REQUIRED
        string query = "SELECT * FROM UpdateConfig_LastUpdated";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        {
                            while (await reader.ReadAsync())
                            {
                                var obj = new ExpandoObject();
                                var d = obj as IDictionary<string, object>;
                                for (int index = 0; index < reader.FieldCount; index++)
                                {
                                    Type type = reader.GetFieldType(index);

                                    switch (Type.GetTypeCode(type))
                                    {
                                        case TypeCode.String:
                                            d[reader.GetName(index)] = await reader.IsDBNullAsync(index) ? null : reader.GetString(index);
                                            break;
                                        case TypeCode.Int32:
                                            d[reader.GetName(index)] = await reader.IsDBNullAsync(index) ? null : reader.GetInt32(index);
                                            break;
                                        case TypeCode.Decimal:
                                            d[reader.GetName(index)] = await reader.IsDBNullAsync(index) ? null : reader.GetDecimal(index);
                                            break;
                                        case TypeCode.DateTime:
                                            d[reader.GetName(index)] = await reader.IsDBNullAsync(index) ? null : reader.GetDateTime(index);
                                            break;
                                        case TypeCode.Boolean:
                                            d[reader.GetName(index)] = await reader.IsDBNullAsync(index) ? null : reader.GetBoolean(index);
                                            break;
                                        //Fallback to GetValue for types not handled explicitly
                                        default:
                                            d[reader.GetName(index)] = reader.GetValue(index);
                                            break;
                                    }
                                }
                                foreach (var kvp in d)
                                {
                                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                                }
                            }
                        }
                    }
                    Console.WriteLine("\nQuery successfully ran :)");
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
