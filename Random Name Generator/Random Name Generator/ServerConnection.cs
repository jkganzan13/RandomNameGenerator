using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Random_Name_Generator
{
    class ServerConnection
    {
        public SqlConnection con = new SqlConnection();        
        private List<Person> lst = new List<Person>();
        
        //add more names in array if needed
        public string[] initializeFirstNames()
        {
            string[] firstName = { "Anna", "Bart", "Cloud", "Desmond", "Erika", "Farrah", "Gaia", "Hailey", "Ina", "Jackie", "Kenneth", "LeBron", "Mufasa", "Nicole", "Odyn", "Pietro", "Queen", "Rasheed", "Sasha", "Taylor", "Uranus", "Victor", "Willy", "Xander", "Yvonne", "Zedge" };
            return firstName;
        }

        //add more names in array if needed
        public string[] initializeLastNames()
        {
            string[] lastName = { "Anthony", "Bryant", "Cooper", "Durant", "Edwards", "Favors", "Grey", "Hill", "Irving", "James", "Klein", "Lionheart", "Millsap", "Noel", "Okafor", "Parker", "Quentin", "Roy", "Swift", "Trife", "Ur", "Valentine", "West", "Xavier", "Young", "Zeller" };
            return lastName;
        }

        //outputs the names generated to console
        public void generateRandomName(int x, string[] first, string[] last)
        {
            Random rnd = new Random();
            for (int i = 0; i < x; i++)
            {
                string randFirst = first[rnd.Next(first.Length)];
                string randLast = last[rnd.Next(last.Length)];
                insertToList(i + 1, randFirst, randLast);
                string fullName = randFirst + " " + randLast;
                Console.WriteLine("{0}. {1}", i + 1, fullName);
            }
        }

        //adds to Person list
        public void insertToList(int x, string first, string last)
        {
            lst.Add(new Person() { ID = x, fName = first, lName = last });
        }     

        //converts names to SQL query (INSERT INTO {tablename} VALUES ({id}, 'firstName', 'lastName'))
        //output in C:\output.txt            
        public void exportToSql()
        {
            using (StreamWriter output = new StreamWriter(@"C:\output.txt"))
            {
                Console.Write("Database Table Name: ");
                string tableName = Console.ReadLine();
                foreach (Person prson in lst)
                {
                    output.WriteLine("INSERT INTO {0} VALUES ({1}, '{2}', '{3}');", tableName, prson.ID, prson.fName, prson.lName);
                }
                Console.WriteLine("Names Added.");
            }
        }

        //checks SQL connection status and intializes SQL connection if not open
        public void checkConnection()
        {
            if (con.State != ConnectionState.Open)
            {
                con.Close();
                initializeSQL();
                con.Open();
            }
        }   

        //initializes SQL connection
        //throws exception if it cannot connect
        public void initializeSQL()
        {
            DBserver server = new DBserver();
            Console.WriteLine("\r\n" + String.Concat(Enumerable.Repeat("*", 32)));
            Console.WriteLine("Initializing SQL Server");
            Console.WriteLine(String.Concat(Enumerable.Repeat("*", 32)));
            con.ConnectionString = getSource(server);
            Console.WriteLine(String.Concat(Enumerable.Repeat("*", 32)));
            Console.WriteLine("Testing Connection...");
            if (testConnection(con.ConnectionString) == true)
                Console.WriteLine("Connected.");
            else
                throw new Exception();           
        }

        //gets data source
        public string getSource(DBserver server)
        {
            Console.Write("Server Name: ");
            server.serverName = Console.ReadLine().Trim();
            Console.Write("Database Name: ");
            server.dbName = Console.ReadLine().Trim();

            string source = "";
            Console.Write("Use Windows Authentication [Y/N]? ");
            string auth = Console.ReadLine().Trim();

            if (auth.ToLower() == "y")
            {
                server.winAuth = true;
                source = String.Format("Data source={0}; Initial Catalog={1}; Integrated Security=True; Connection Timeout=15;", server.serverName, server.dbName);
            }
            else if (auth.ToLower() == "n")
            {
                server.winAuth = false;
                Console.WriteLine("\r\nUsing SQL Authentication:");
                Console.Write("{0,12}", "Username: ");
                server.uName = Console.ReadLine().Trim();
                server.pwd = maskPassword();
                source = String.Format("Data source={0}; Initial Catalog={1}; User id= {2}; password={3}; Connection Timeout=15;", server.serverName, server.dbName, server.uName, server.pwd);
            }
            else
            {
                throw new Exception();
            }

            return source;
        }

        //hides password as * when typed in console
        public string maskPassword()
        {
            string password = "";

            ConsoleKeyInfo key;
            Console.Write("{0,12}", "Password: ");

            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {

                    password = password.Remove(password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return password;
        }      

        //add generated names to DB table
        public void addToDB()
        {
            SqlCommand comm = new SqlCommand();
            comm.Connection = con;
            int count = 0;
            comm.CommandText = "SELECT COUNT(*) FROM Person";
            int rowNum = (int)comm.ExecuteScalar() + 1;
            string insertComm = "";
            Console.Write("\r\nDatabase Table Name: ");
            string tableName = Console.ReadLine().Trim();
            foreach (Person prson in lst)
            {
                insertComm = String.Format("INSERT INTO {0} (PersonID, FirstName, LastName) VALUES ({1}, '{2}', '{3}')", tableName, rowNum++, prson.fName, prson.lName);
                comm.CommandText = insertComm;
                comm.ExecuteNonQuery();
                count++;
            }
            Console.WriteLine("{0} rows affected", count);

        }

        //read contents of DB table
        public void readDB()
        {
            SqlDataReader reader = null;
            SqlCommand comm = new SqlCommand();
            Console.Write("\r\nDatabase Table Name: ");
            string tableName = Console.ReadLine();
            comm.CommandText = String.Format("SELECT * FROM {0}", tableName);
            comm.Connection = con;
            reader = comm.ExecuteReader();
            string id, fn, ln;
            Console.WriteLine("\r\n{0, -8} {1, -10} {2, -15}", "PersonID".PadLeft(5), "First Name", "Last Name");
            Console.WriteLine(String.Concat(Enumerable.Repeat("-", 32)));
            while (reader.Read())
            {
                id = reader["PersonID"].ToString();
                fn = reader["FirstName"].ToString();
                ln = reader["LastName"].ToString();
                Console.WriteLine("{0, -9} {1, -10} {2,-15}", id.PadLeft(5), fn, ln);
            }
        }

        //tests SQL Connection to server
        public bool testConnection(string source)
        {
            try
            {
                SqlConnection test = new SqlConnection();
                test.ConnectionString = source;
                test.Open();
                bool state = (test.State == ConnectionState.Open);
                test.Close();
                return state;
            }
            catch (SqlException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class Person
    {
        public int ID;
        public string fName;
        public string lName;
    }

    class DBserver
    {
        public string serverName;
        public string dbName;
        public string uName;
        public string pwd;
        public bool winAuth;
    }
}
