using System.Data;
using System.Data.SqlClient;

namespace homework_01
{
    class Program
    {
        private static void printHeader()
        {
            Console.WriteLine("View table: T\n" +
                              "View names: N\n" +
                              "View colors: C\n" +
                              "View max calories: M\n" +
                              "View min calories: L\n" +
                              "View average calories: A\n" +
                              "View fruits: F\n" +
                              "View vegetables: V\n" +
                              "View fr/vg with color: 1\n" +
                              "View fr/vg with less calories: 2\n" +
                              "View fr/vg with more calories: 3\n" +
                              "View fr/vg with calories in range: 4\n" +
                              "View yellow and red fr/vg: 5\n"
            );
        }
        
        private static void printTable(SqlDataAdapter adapter, List<string> columns, DataSet dataSet)
        {
            dataSet.Clear();
            adapter.Fill(dataSet);
            
            foreach (string dataColumn in columns)
            {
                Console.Write("{0,-10} ",  dataColumn);
            }
            Console.WriteLine("\n");
                            
            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
            {
                foreach (string column in columns)
                {
                    Console.Write("{0,-10} ",  dataRow[column].ToString().Trim());
                }
                Console.WriteLine();
            }
        }
        
        static void Main(string[] args)
        { 
            string connectionString = "Data Source=localhost;Initial Catalog=Test;Integrated Security=true;Connection Timeout=30;";
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Connected");
            DataSet dataSet = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            
            ConsoleKey key = ConsoleKey.NoName;
            List<string> columns = new List<string>();
            string inputOne = "";
            string inputTwo = "";
            while (key != ConsoleKey.Q)
            {
                printHeader();
                switch (key)
                {
                    case ConsoleKey.T:
                        columns = new List<string>() { "id", "name", "type", "color", "calories" };
                        printTable(new SqlDataAdapter("SELECT id, name, type, color, calories FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.N:
                        columns = new List<string>() { "name" };
                        printTable(new SqlDataAdapter("SELECT name FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.C:
                        columns = new List<string>() { "color" };
                        printTable(new SqlDataAdapter("SELECT color FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.M:
                        columns = new List<string>() { "max_calories" };
                        printTable(new SqlDataAdapter("SELECT MAX(calories) AS max_calories FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.L:
                        columns = new List<string>() { "min_calories" };
                        printTable(new SqlDataAdapter("SELECT MIN(calories) AS min_calories FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.A:
                        columns = new List<string>() { "average_calories" };
                        printTable(new SqlDataAdapter("SELECT AVG(calories) AS average_calories FROM FruitsAndVegetables", connection), columns, dataSet);
                        break;
                    case ConsoleKey.F:
                        columns = new List<string>() { "fruits" };
                        printTable(new SqlDataAdapter("SELECT name AS fruits FROM FruitsAndVegetables WHERE (type=0)", connection), columns, dataSet);
                        break;
                    case ConsoleKey.V:
                        columns = new List<string>() { "vegetables" };
                        printTable(new SqlDataAdapter("SELECT name AS vegetables FROM FruitsAndVegetables WHERE (type=1)", connection), columns, dataSet);
                        break;
                    case ConsoleKey.D1:
                        Console.Write("Enter color: ");
                        inputOne = Console.ReadLine();
                        Console.WriteLine();
                        
                        columns = new List<string>() { "fr/vg_with_color" };
                        printTable(new SqlDataAdapter($"SELECT name AS 'fr/vg_with_color' FROM FruitsAndVegetables WHERE (color='{inputOne}')", connection), columns, dataSet);
                        break;
                    case ConsoleKey.D2:
                        Console.Write("Enter calories: ");
                        inputOne = Console.ReadLine();
                        Console.WriteLine();
                        
                        columns = new List<string>() { "fr/vg_with_less_calories" };
                        printTable(new SqlDataAdapter($"SELECT name AS 'fr/vg_with_less_calories' FROM FruitsAndVegetables WHERE (calories<'{inputOne}')", connection), columns, dataSet);
                        break;
                    case ConsoleKey.D3:
                        Console.Write("Enter calories: ");
                        inputOne = Console.ReadLine();
                        Console.WriteLine();
                        
                        columns = new List<string>() { "fr/vg_with_more_calories" };
                        printTable(new SqlDataAdapter($"SELECT name AS 'fr/vg_with_more_calories' FROM FruitsAndVegetables WHERE (calories>'{inputOne}')", connection), columns, dataSet);
                        break;
                    case ConsoleKey.D4:
                        Console.Write("Enter min calories: ");
                        inputOne = Console.ReadLine();
                        Console.Write("Enter max calories: ");
                        inputTwo = Console.ReadLine();
                        Console.WriteLine();
                        
                        columns = new List<string>() { "fr/vg_with_calories_in_range" };
                        printTable(new SqlDataAdapter($"SELECT name AS 'fr/vg_with_calories_in_range' FROM FruitsAndVegetables WHERE (calories BETWEEN {inputOne} AND {inputTwo})", connection), columns, dataSet);
                        break;
                    case ConsoleKey.D5:
                        columns = new List<string>() { "red_yellow_fr/vg" };
                        printTable(new SqlDataAdapter($"SELECT name AS 'red_yellow_fr/vg' FROM FruitsAndVegetables WHERE (color='Red' OR color='Yellow')", connection), columns, dataSet);
                        break;
                }

                key = Console.ReadKey().Key;
                Console.Clear();
            }
            
            connection.Close();
            Console.WriteLine("Disconnected");
        }
    }
}