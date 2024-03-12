using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;

namespace Test
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Person> peopleList = new ObservableCollection<Person>();
        private SQLiteConnection dbConnection;

        public MainWindow()
        {
            InitializeComponent();
            dbConnection = DatabaseHelper.OpenConnection();
            DatabaseHelper.InitializeDatabase();
            LoadPeopleData();
            lvList.ItemsSource = peopleList;
        }

        private void LoadPeopleData()
        {
            //load data and populate list
            peopleList.Clear();
            foreach (Person person in DatabaseHelper.GetPeople())
            {
                peopleList.Add(person);
            }
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lvList.SelectedItem != null)
            {
                //update selected person
                Person selectedPerson = (Person)lvList.SelectedItem;
                selectedPerson.FirstName = txtFirstName.Text;
                selectedPerson.LastName = txtLastName.Text;
                selectedPerson.Passport = txtPassport.Text;
                selectedPerson.ExpirationDate = dpExpirationDate.SelectedDate;
                selectedPerson.IsValid = chkValid.IsChecked ?? false;
                DatabaseHelper.UpdatePerson(selectedPerson);

                //update observable collection
                int index = peopleList.IndexOf(selectedPerson);
                if (index != -1)
                {
                    peopleList[index] = selectedPerson;
                }
            }
            else
            {
                //insert new person
                Person newPerson = new Person
                {
                    FirstName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    Passport = txtPassport.Text,
                    ExpirationDate = dpExpirationDate.SelectedDate,
                    IsValid = chkValid.IsChecked ?? false
                };

                DatabaseHelper.InsertPerson(newPerson);
                peopleList.Add(newPerson);
            }

            //reload after update
            LoadPeopleData();

            btnClear_Click(sender, e);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //clear all fields
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtPassport.Text = "";
            dpExpirationDate.SelectedDate = null;
            chkValid.IsChecked = false;

            btnInsert.Content = "Insert";
            lvList.SelectedItem = null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //delete selected person 
            if (lvList.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this person?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Person selectedPerson = (Person)lvList.SelectedItem;
                    DatabaseHelper.DeletePerson(selectedPerson);
                    peopleList.Remove(selectedPerson);
                    btnClear_Click(sender, e);
                }
            }
        }

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = lvList.SelectedItem != null;
            btnInsert.Content = lvList.SelectedItem != null ? "Update" : "Insert";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DatabaseHelper.CloseConnection(dbConnection);
        }
    }

    //person class
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Passport { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsValid { get; set; }
    }

}
