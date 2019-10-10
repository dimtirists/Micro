using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections;


namespace Micro_IN
{
    /// <summary>
    /// Interaction logic for main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public int RowNumber;
        readonly  int iDUserConnected2;
        readonly MySqlConnection conn = new MySqlConnection(Properties.Resources.MySQLConnectionString);

        public Main(int iDUserConnected, string userDisplayed)
        {
           
            InitializeComponent();

            TopGrid.MouseDown += TopGrid_MouseDown;
            
            ClientDataGrid.MouseDoubleClick += ClientDataGrid_MouseDoubleClick;
            FillCLientDataGrid();
            iDUserConnected2 = iDUserConnected;
            UserLabelName.Content = userDisplayed;


            ClientDataGrid.RowStyle = new Style(typeof(DataGridRow));
            ClientDataGrid.RowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
                                     new MouseButtonEventHandler(Row_DoubleClick)));

        }

        private void TopGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
             if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        
        private void ClientDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView data = (DataRowView)ClientDataGrid.SelectedItem;
            string sValue = "";
            int IdValue =0;

            if(data != null)
            {
                sValue = data.Row["Sirname"] as string;
                IdValue = (Int32)data.Row.ItemArray[0];
            }
            


            ClientForm form = new ClientForm(sValue, IdValue);
            form.ShowDialog();
            
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            
        }

       

        private void InsertClientButton_click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTXT.Text) && string.IsNullOrEmpty(SirnameTXT.Text))
            {


                MessageBox.Show("Please Enter Name and Sirname");
            }
            else
            {
                conn.Open();
                using (conn)
                {

                    MySqlCommand insertClint = new MySqlCommand("INSERT INTO Micro.Clients (idClients, Name, Sirname, Telephone, Date) VALUES ((select IDusers from Micro.Users WHERE Micro.Users.Username ='" + UserLabelName.Content + "'" + "), " + "'" + NameTXT.Text + "'" + "," + "'" + SirnameTXT.Text + "'" + "," + "'" + TelephoneTXT.Text + "'" + ", NOW())", conn);
                    try
                    {
                        MySqlDataReader dr = insertClint.ExecuteReader();
                    }
                    catch(MySqlException tryClient)
                    {
                        MessageBox.Show(tryClient.Message);
                    }
                    insertClint.Dispose();
                }
                
                conn.Close();
                conn.Dispose();
               

                NameTXT.Clear();
                SirnameTXT.Clear();
                TelephoneTXT.Clear();
                FillCLientDataGrid();
            }
        }

        private void ExitButtonMain_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void FillCLientDataGrid()
        {
            conn.Open();
            using (conn)
            {
                MySqlCommand fillData = new MySqlCommand("select Micro.Clients.KeyClients, Micro.Clients.Name, Micro.Clients.Sirname, Micro.Clients.Telephone, Active, date FROM Micro.Clients order by Date DESC LIMIT 10;", conn);
                MySqlDataReader dataRead = fillData.ExecuteReader();
                if (dataRead.HasRows)
                {
                    DataTable dr = new DataTable();
                    
                    dr.Load(dataRead);
                    ClientDataGrid.ItemsSource = dr.DefaultView;
                    ClientDataGrid.DataContext = dr.DefaultView;

                    dr.Dispose();
                    fillData.Dispose();
                }

            }
            conn.Close();
            
        }

        private void RefreshClientButton_Click(object sender, RoutedEventArgs e)
        {
            FillCLientDataGrid();
        }

        private void AdminUserButton_Click(object sender, RoutedEventArgs e)
        {
            conn.Open();
            using (conn)
            {


                MySqlCommand fillData = new MySqlCommand("select Micro.Clients.Name, Micro.Clients.Sirname, Micro.Clients.Active from Micro.Clients where Micro.Clients.Name='" + SearchUserButton.Text + "' or Micro.Clients.Sirname='" + SearchUserButton.Text + "';", conn);
                MySqlDataReader dataRead = fillData.ExecuteReader();
                if (dataRead.HasRows)
                {
                    DataTable dr = new DataTable();
                    dr.Load(dataRead);
                    //  ClientDataGrid.ItemsSource = dr.DefaultView;
                    ClientDataGrid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = dr });

                    dr.Dispose();
                    fillData.Dispose();
                }
            }
            conn.Close();
            conn.Dispose();
        }



        private void ClientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}
