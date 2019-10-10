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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Data;


namespace Micro_IN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SQLServerLocation;
        public string Database;
        public string UidName;
        public string password;

        public string connString;

        public bool Connected;
        public string error;

        public int Port
        {
            get; set;
        }




        public MainWindow()
        {
            
            InitializeComponent();
          
        }

        public void Connect()

        {

            SQLServerLocation = "dimitsnas.synology.me";
            Database = "Micro";
            UidName = "global";
            password = "savatage1!";

            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = SQLServerLocation,
                Port = 3307,
                Database = Database,
                UserID = UidName,
                Password = password
            };

            try
            {
                MySql.Data.MySqlClient.MySqlConnection sqlConnection = new MySql.Data.MySqlClient.MySqlConnection(connectionString.ToString());
                sqlConnection.Open();

              
                

                string selectQuery = "SELECT * FROM Micro.Users where Username=@user and Password=@pass;";
                MySqlConnection mySql = new MySqlConnection(connString);
                MySqlCommand mySqlCommand = new MySqlCommand(selectQuery, mySql);
                mySql.Open();
                mySqlCommand.Parameters.AddWithValue("@user", UsernameCredentials.Text);
                mySqlCommand.Parameters.AddWithValue("@pass", UsersPassword.Password.ToString());
                MySqlDataReader reader;
                DataTable data = new DataTable();
                reader = mySqlCommand.ExecuteReader();
                
                    data.Load(reader);

                
                reader.Close();
                mySql.Close();


                if(reader.HasRows==true)
                {
                    Connected = true;
                    connString = connectionString.ToString();
                }
                else
                {
                    MessageBox.Show("No user");
                }
                


            }
            catch (Exception e)
            {
                error = e.Message;
                Connected = false;
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {


            Connect();
            if (Connected is false)
            {
                ConnectedLabel.Content = "Not Connected";
            }
            else
            {
                ConnectedLabel.Content = "Connected";
            }
        }

        
    }
}
