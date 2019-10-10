using System;
using System.Collections.Generic;
using System.Data;
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


namespace Micro_IN
{
    /// <summary>
    /// Interaction logic for ClientForm.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
       
       
        readonly MySqlConnection conn = new MySqlConnection(Properties.Resources.MySQLConnectionString);

        public ClientForm(string SelectedUser, int KeyClient)
        {
            
            InitializeComponent();
            SirnameLabel.Content = SelectedUser;
            IDLabel.Content = KeyClient.ToString();
            filldata();
        }

        private DataTable GetTable()
        {
            
            
                conn.Open();
                MySqlCommand NewCommand = new MySqlCommand("SELECT Micro.Devices.Brand, Micro.Devices.Model, Micro.Devices.Type FROM Micro.Devices WHERE Micro.Devices.IdClients ='" + IDLabel.Content + "'", conn);
                MySqlDataReader dr = NewCommand.ExecuteReader();

                DataTable devicesTable = new DataTable();
                devicesTable.Load(dr);

            
            return devicesTable;
        }

        public void filldata()
        {
            using(conn)
            {
                conn.Open();
                MySqlCommand NewCommand = new MySqlCommand("SELECT Micro.Clients.Name, Micro.Clients.Sirname, Micro.Clients.Telephone, Micro.Clients.Date FROM Micro.Clients WHERE Micro.Clients.KeyClients ='" + IDLabel.Content + "'", conn);
                MySqlDataReader dr = NewCommand.ExecuteReader();

                DataTable clientTable = new DataTable();

                clientTable.Load(dr);

                foreach(DataRow row in clientTable.Rows)
                {
                    NameLabel.Content = row["Name"].ToString();
                    SirnameLabel.Content = row["Sirname"].ToString();
                    TelephoneLabel.Content = row["Telephone"].ToString();
                    DateLabel.Content = row["Date"].ToString();
                }
                conn.Close();

                
            }

            using (conn)
            {
                devicesCombobox.Items.Clear();
                conn.Open();
                MySqlCommand NewCommand = new MySqlCommand("SELECT Micro.Devices.Brand, Micro.Devices.Model, Micro.Devices.Type FROM Micro.Devices WHERE Micro.Devices.IdClients ='" + IDLabel.Content + "'", conn);
                MySqlDataReader dr = NewCommand.ExecuteReader();

                DataTable devicesTable = new DataTable();
                devicesTable.Load(dr);

                foreach(DataRow row in devicesTable.Rows)
                {
                    
                    devicesCombobox.Items.Add(row["Brand"].ToString());
                    
                }
                conn.Close();

                devicesTable.Dispose();
            }


        }
        // Εδω πρεπει να δω τι να κανω με τα update απο τις συσκευες.
        // Επειγον
        //

        private void DevicesCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (conn)
            {
                
                conn.Open();
                MySqlCommand NewCommand = new MySqlCommand("SELECT Micro.Devices.Type FROM Micro.Devices WHERE Micro.Devices.IdClients ='" + IDLabel.Content + "'", conn);
                MySqlDataReader dr = NewCommand.ExecuteReader();

                DataTable devicesTable = new DataTable();
                devicesTable.Load(dr);

                foreach (DataRow row in devicesTable.Rows)
                {

                    CommentsTextblock.Text = row["Type"].ToString();
                }
                conn.Close();

                devicesTable.Dispose();
            }
        }

        private void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            using (conn)
            {
                conn.Open();
                MySqlCommand NewCommand = new MySqlCommand(" INSERT Micro.Devices(Brand, Model, Type, IdClients) VALUES ('" + BrandTextbox.Text + "','" + ModelTextBox.Text + "','" + CommentsTextbox.Text + "','"  + IDLabel.Content + "')", conn);
                MySqlDataReader dr = NewCommand.ExecuteReader();

                conn.Close();

                CommentsTextbox.Clear();
                filldata();
            }
        }
    }
}
