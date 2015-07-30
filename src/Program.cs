using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Security;

namespace Random_Name_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title= "Random Name Generator by John Kenneth Ganzan";

            ServerConnection serverConn = new ServerConnection();
            string answer = "";
            
            Console.Write("Number of Names to generate: ");
            int x = int.Parse(Console.ReadLine());
            
            string[] firstNames = serverConn.initializeFirstNames();
            string[] lastNames = serverConn.initializeLastNames();
            serverConn.generateRandomName(x, firstNames, lastNames);


            //converts names to SQL query (INSERT INTO {tablename} VALUES ({id}, 'firstName', 'lastName'))
            //output in C:\output.txt
            try
            {
                Console.Write("\r\nConvert to SQL Query [Y/N]? ");
                answer = Console.ReadLine();
                if (answer.ToLower() == "y")
                    serverConn.exportToSql();
            } 
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(@"Error: Drive C: access is denied.");
            } 
            catch (IOException)
            {
                Console.WriteLine(@"Error: Drive C: required privilege not held by client.");
            }
            catch (Exception)
            {
                Console.WriteLine(@"Error: Cannot write to Drive C:");
            }

            try
            {
                //add names directly into server database table
                Console.Write("\r\nAdd to Database [Y/N]? ");
                answer = Console.ReadLine().Trim();
                if (answer.ToLower() == "y")
                {
                    serverConn.checkConnection();
                    serverConn.addToDB();
                }

                //read contents of database table
                Console.Write("\r\nRead Database [Y/N]? ");
                answer = Console.ReadLine().Trim();
                if (answer.ToLower() == "y")
                {
                    serverConn.checkConnection();
                    serverConn.readDB();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error Occured: Please check inputs and try again.");
            }

            //closes SQL connection if open
            if (serverConn.con.State == ConnectionState.Open)
                serverConn.con.Close();

            Console.WriteLine("\r\nPress any key to exit...");
            Console.ReadKey();
        }        
    }    
}
