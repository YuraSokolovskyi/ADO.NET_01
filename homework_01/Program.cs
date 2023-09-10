using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using CustomMenu;
using System.Diagnostics;

namespace homework_01
{
    class Program
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        private static string ProviderName = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;

        private static Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();

        private static List<int> findAllIndexesWithValue(string columnName, string value)
        {
            return data[columnName]
                .Select((item, index) => new { Item = item, Index = index })
                .Where(item => item.Item == value)
                .Select(item => item.Index)
                .ToList();
        }
        
        private static List<int> findAllIndexesWithLessValue(string columnName, double value)
        {
            return data[columnName]
                .Select((item, index) => new { Item = item, Index = index })
                .Where(item => Double.Parse(item.Item) < value)
                .Select(item => item.Index)
                .ToList();
        }
        
        private static List<int> findAllIndexesWithMoreValue(string columnName, double value)
        {
            return data[columnName]
                .Select((item, index) => new { Item = item, Index = index })
                .Where(item => Double.Parse(item.Item) > value)
                .Select(item => item.Index)
                .ToList();
        }
        
        private static List<int> findAllIndexesWithValueRange(string columnName, double valueFirst, double valueSecond)
        {
            return data[columnName]
                .Select((item, index) => new { Item = item, Index = index })
                .Where(item => Double.Parse(item.Item) >= valueFirst && Double.Parse(item.Item) <= valueSecond)
                .Select(item => item.Index)
                .ToList();
        }
        
        private static async Task<(long, Dictionary<string, List<string>>)> ReadDataAsync(DbProviderFactory factory, List<string> columns)
        {
            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
            foreach (string column in columns) data[column] = new List<string>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync();

                using (DbCommand command = factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM FruitsAndVegetables";
                
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            foreach (string column in columns) data[column].Add(reader[column].ToString().Trim());
                        }
                    }
                }

                await connection.CloseAsync();
            }
            
            stopwatch.Stop();

            return (stopwatch.ElapsedMilliseconds, data);
        }
        
        static async Task Main(string[] args)
        {
            Menu menu = new Menu();
            // set up header
            menu.addHeaderRow(
            new List<string>(){
                "View table: T",
                "View names: N",
                "View colors: C",
                "View max calories: M",
                "View min calories: L",
                "View average calories: A",
                "View fruits: F", 
                "View vegetables: V",
                "View fr/vg with color: 1",
                "View fr/vg with less calories: 2",
                "View fr/vg with more calories: 3",
                "View fr/vg with calories in range: 4",
                "View yellow and red fr/vg: 5",
                "Update data: 7",
                "Delete data: 8",
                "Change DBMS: 9",
            });
            menu.setHeaderDivider("\n");
            menu.setHeaderEndString("\n\n");

            DbProviderFactories.RegisterFactory(ProviderName, SqlClientFactory.Instance);
            DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
            
            List<string> fruitsAndVegetablesColumns = new List<string>() { "id", "name", "type", "color", "calories" };
            string singleAnswer = "";
            string inputOne = "";
            string inputTwo = "";
            long time = 0;

            (time, data) = await ReadDataAsync(factory, fruitsAndVegetablesColumns);
            Console.WriteLine($"Time spent reading: {time}ms\n");
           
           // add options for main loop
           menu.addMainLoopOption(new Dictionary<ConsoleKey, Menu.MainLoopOptionDelegate>()
           {
               { ConsoleKey.T , () =>
               {
                   menu.printTableColumn(data, showRowNumber:true, name:"Table");
               }},
               { ConsoleKey.N, () =>
               {
                   List<string> columns = new List<string>() { "name" };
                   menu.printTableColumn(columns, new List<List<string>>(){data["name"]}, name:"All names", showRowNumber:true);
               }},
               { ConsoleKey.C, () =>
               {
                   List<string> columns = new List<string>() { "color" };
                   menu.printTableColumn(columns, new List<List<string>>(){data["color"].ToHashSet().ToList()}, name:"All colors", showRowNumber:true);
               }},
               { ConsoleKey.M, () =>
               {
                   string maxValue = data["calories"].Max(item => Int32.Parse(item)).ToString();
                   menu.printTableColumn(data, name:"Max calories", showRowNumber:true, rowsToShow:findAllIndexesWithValue("calories", maxValue));
               }},
               { ConsoleKey.L, () =>
               {
                   string minValue = data["calories"].Min(item => Int32.Parse(item)).ToString();
                   menu.printTableColumn(data, name:"Min calories", showRowNumber:true, rowsToShow:findAllIndexesWithValue("calories", minValue));
               }},
               { ConsoleKey.A, () =>
               {
                   menu.printSingleAnswer("average calories",
                       (data["calories"].Sum(item => Int32.Parse(item)) / data["calories"].Count).ToString());
               }},
               { ConsoleKey.F, () =>
               {
                   menu.printTableColumn(data, "fruits", rowsToShow:findAllIndexesWithValue("type", "0"));
               }},
               { ConsoleKey.V, () =>
               {
                   menu.printTableColumn(data, "vegetables", rowsToShow:findAllIndexesWithValue("type", "1"));
               }},
               { ConsoleKey.D1, () =>
               {
                   Console.Write("Enter color: ");
                   inputOne = Console.ReadLine();
                   Console.WriteLine();
                    
                   menu.printTableColumn(data, $"{inputOne} fruits or vegetables", rowsToShow:findAllIndexesWithValue("color", inputOne));
               }},
               { ConsoleKey.D2, () =>
               {
                   Console.Write("Enter calories: ");
                   inputOne = Console.ReadLine();
                   Console.WriteLine();
                    
                   menu.printTableColumn(data, $"fruits or vegetables with less than {inputOne} calories", 
                       rowsToShow:findAllIndexesWithLessValue("calories", Double.Parse(inputOne)));
               }},
               { ConsoleKey.D3, () =>
               {
                   Console.Write("Enter calories: ");
                   inputOne = Console.ReadLine();
                   Console.WriteLine();
                    
                   menu.printTableColumn(data, $"fruits or vegetables with more than {inputOne} calories", 
                       rowsToShow:findAllIndexesWithMoreValue("calories", Double.Parse(inputOne)));
               }},
               { ConsoleKey.D4, () =>
               {
                   Console.Write("Enter min calories: ");
                   inputOne = Console.ReadLine();
                   Console.Write("Enter max calories: ");
                   inputTwo = Console.ReadLine();
                   Console.WriteLine();
                    
                   menu.printTableColumn(data, $"fruits or vegetables with more than {inputOne} and less than {inputTwo} calories", 
                       rowsToShow:findAllIndexesWithValueRange("calories", Double.Parse(inputOne), Double.Parse(inputTwo)));
               }},
               { ConsoleKey.D5, () =>
               {
                   var res = data["color"]
                       .Select((item, index) => new { Item = item, Index = index })
                       .Where(item => item.Item == "Red" || item.Item == "Yellow")
                       .Select(item => item.Index)
                       .ToList();
                   menu.printTableColumn(data, "red and yellow fruits/vegetables", rowsToShow:res);
               }},
               { ConsoleKey.D7, async () =>
               {
                   long timeUpdating = await updateMenu(data, factory);
                   (time, data) = await ReadDataAsync(factory, fruitsAndVegetablesColumns);
                   Console.Clear();
                   Console.WriteLine($"Time spent updating: {timeUpdating}ms\n");
                   Console.WriteLine($"Time spent reading: {time}ms\n");
                   menu.printHeader();
               }},
               { ConsoleKey.D8, async () =>
               {
                   long timeDeleting = await deleteMenu(data, factory);
                   (time, data) = await ReadDataAsync(factory, fruitsAndVegetablesColumns);
                   Console.Clear();
                   Console.WriteLine($"Time spent deleting: {timeDeleting}ms\n");
                   Console.WriteLine($"Time spent reading: {time}ms\n");
                   menu.printHeader();
               }},
               { ConsoleKey.D9, async () =>
               {
                   changeDBMS();
                   Console.Clear();
                   (time, data) = await ReadDataAsync(factory, fruitsAndVegetablesColumns);
                   Console.WriteLine($"Time spent reading: {time}ms\n");
                   menu.printHeader();
               }},
           });
           menu.startMainLoop();
        }

        private static void changeDBMS()
        {
            ConnectionStringSettingsCollection strings = ConfigurationManager.ConnectionStrings;
            List<string> keys = new List<string>();
            foreach (ConnectionStringSettings  str in strings) keys.Add(str.Name);
            Menu menu = new Menu();
            menu.printTableColumn(new List<string>(){"Name"}, new List<List<string>>(){keys.GetRange(1,keys.Count - 1)}, autoGenerateId:true);
            Console.Write("Enter row number: ");
            int number = Int32.Parse(Console.ReadLine());
            ConnectionString = ConfigurationManager.ConnectionStrings[keys[number + 1]].ToString();
            ProviderName = ConfigurationManager.ConnectionStrings[keys[number + 1]].ProviderName;
        }

        private static async Task<long> updateMenu(Dictionary<string, List<string>> data, DbProviderFactory factory)
        {
            Console.Clear();
            Menu updateMenu = new Menu();
            updateMenu.setHeaderDivider("\n");
            updateMenu.setHeaderEndString("\n\n");
            int id = -1;
            updateMenu.printTableColumn(data, "Select id");
            Console.Write("Enter id: ");
            id = Int32.Parse(Console.ReadLine());
            Console.Clear();
            
            Console.WriteLine("Update name: 1\n" +
                              "Update type(0-fruit, 1-vegetable): 2\n" +
                              "Update color: 3\n" +
                              "Update calories: 4\n");

            ConsoleKey key = Console.ReadKey().Key;
            
            Console.Clear();
            
            Console.Write("Enter new value: ");

            string newValue = Console.ReadLine();
            long time = 0;
            
            if (key == ConsoleKey.D1) time = await updateAsync("name", $"'{newValue}'", id, factory);
            else if (key == ConsoleKey.D2) time = await updateAsync("type", $"{newValue}", id, factory);
            else if (key == ConsoleKey.D3) time = await updateAsync("color", $"'{newValue}'", id, factory);
            else if (key == ConsoleKey.D4) time = await updateAsync("calories", $"{newValue}", id, factory);

            return time;
        }

        private static async Task<long> updateAsync(string column, string newValue, int id, DbProviderFactory factory)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync();

                DbCommand command = factory.CreateCommand();
                command.CommandText = $"UPDATE FruitsAndVegetables SET {column}={newValue} WHERE id={id}";
                command.Connection = connection;
                await command.ExecuteNonQueryAsync();
                
                await connection.CloseAsync();
            }
            
            stopwatch.Stop();
            
            return stopwatch.ElapsedMilliseconds;
        }

        private static async Task<long> deleteMenu(Dictionary<string, List<string>> data, DbProviderFactory factory)
        {
            Console.Clear();
            Menu updateMenu = new Menu();
            int id = -1;
            updateMenu.printTableColumn(data, "Select id");
            Console.Write("Enter id: ");
            id = Int32.Parse(Console.ReadLine());

            long time = await deleteAsync(id, factory);

            return time;
        }

        private static async Task<long> deleteAsync(int id, DbProviderFactory factory)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync();

                DbCommand command = factory.CreateCommand();
                command.CommandText = $"DELETE FROM FruitsAndVegetables WHERE id={id}";
                command.Connection = connection;
                await command.ExecuteNonQueryAsync();
                
                await connection.CloseAsync();
            }
            
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}