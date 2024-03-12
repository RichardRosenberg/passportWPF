using System;
using System.Collections.ObjectModel;
using System.IO; 
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;
using Test;

//database saved in bin/debug
public static class DatabaseHelper
{
    //path and connection string
    private static readonly string DatabaseFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PeopleDatabase.db");
    private static readonly string ConnectionString = "Data Source=" + DatabaseFilePath + ";Version=3;";

    public static SQLiteConnection OpenConnection()
    {
        SQLiteConnection connection = new SQLiteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    //close connection
    public static void CloseConnection(SQLiteConnection connection)
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
        }
    }

    //initialize the db
    public static void InitializeDatabase()
    {
        if (!File.Exists(DatabaseFilePath))
        {
            SQLiteConnection.CreateFile(DatabaseFilePath);
            CreatePeopleTable();
        }
    }

    //create the db if it doesnt exist
    private static void CreatePeopleTable()
    {
        using (SQLiteConnection connection = OpenConnection())
        {
            string createTableQuery = "CREATE TABLE IF NOT EXISTS People (" +
                                     "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                     "FirstName TEXT, " +
                                     "LastName TEXT, " +
                                     "Passport TEXT, " +
                                     "ExpirationDate TEXT, " +
                                     "IsValid INTEGER);";

            using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    //retrive the people from the people table
    public static ObservableCollection<Person> GetPeople()
    {
        ObservableCollection<Person> people = new ObservableCollection<Person>();

        using (SQLiteConnection connection = OpenConnection())
        {
            string selectQuery = "SELECT * FROM People;";
            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        people.Add(new Person
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Passport = reader["Passport"].ToString(),
                            ExpirationDate = DateTime.Parse(reader["ExpirationDate"].ToString()),
                            IsValid = Convert.ToBoolean(reader["IsValid"])
                        });
                    }
                }
            }
        }

        return people;
    }

    //insert new person into people
    public static void InsertPerson(Person person)
    {
        using (SQLiteConnection connection = OpenConnection())
        {
            string insertQuery = "INSERT INTO People (FirstName, LastName, Passport, ExpirationDate, IsValid) VALUES " +
                                 "(@FirstName, @LastName, @Passport, @ExpirationDate, @IsValid);";

            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@FirstName", person.FirstName);
                command.Parameters.AddWithValue("@LastName", person.LastName);
                command.Parameters.AddWithValue("@Passport", person.Passport);
                command.Parameters.AddWithValue("@ExpirationDate", person.ExpirationDate.HasValue ? person.ExpirationDate.Value.ToString("yyyy-MM-dd") : DBNull.Value.ToString());
                command.Parameters.AddWithValue("@IsValid", person.IsValid);
                command.ExecuteNonQuery();
            }
        }
    }

    //update selected person in people
    public static void UpdatePerson(Person person)
    {
        using (SQLiteConnection connection = OpenConnection())
        {
            string updateQuery = "UPDATE People SET FirstName = @FirstName, LastName = @LastName, " +
                                 "Passport = @Passport, ExpirationDate = @ExpirationDate, " +
                                 "IsValid = @IsValid WHERE Id = @Id;";

            using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", person.Id);
                command.Parameters.AddWithValue("@FirstName", person.FirstName);
                command.Parameters.AddWithValue("@LastName", person.LastName);
                command.Parameters.AddWithValue("@Passport", person.Passport);
                command.Parameters.AddWithValue("@ExpirationDate", person.ExpirationDate?.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@IsValid", person.IsValid);

                command.ExecuteNonQuery();
            }
        }
    }

    //delete selected person from people
    public static void DeletePerson(Person person)
    {
        using (SQLiteConnection connection = OpenConnection())
        {
            string deleteQuery = "DELETE FROM People WHERE Id = @Id;";

            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", person.Id);

                command.ExecuteNonQuery();
            }
        }
    }
}
