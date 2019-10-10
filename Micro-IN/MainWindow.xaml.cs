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



namespace Micro_IN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly  MySqlConnection conn = new MySqlConnection(Properties.Resources.MySQLConnectionString);

        public int IDUserConnected;
        public bool Connected;
        public string connString;

        public MainWindow()
        {
            
            InitializeComponent();
            
        }

        public void Connect()

        {

            try
            {
                conn.Open();
                
            }
            catch(Exception conne)
            {
                MessageBox.Show(conne.Message);
            }
            Connected = true;
           

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connect();
            MySqlCommand NewCommand = new MySqlCommand("select * from Micro.Users where Username ='" + UserNameTextBox.Text +"'" + " and Password=" + "'" + PasswordBX.Password.ToString() + "'", conn);

            MySqlDataReader dr = NewCommand.ExecuteReader();

            
            if (dr.HasRows)
            {
                conn.Close();


                using (MySqlConnection connection = conn)
                {

                    connection.Open();
                   
                        MySqlCommand SetUser = new MySqlCommand("select IDusers from Micro.Users where Username='" + UserNameTextBox.Text + "'", conn);
                        MySqlDataReader dr1 = SetUser.ExecuteReader();
                        while (dr1.Read())
                        {
                            IDUserConnected = dr1.GetInt32(0);
                        }
                        

                        connection.Close();
                    }
                    Main mainwin = new Main(IDUserConnected, UserNameTextBox.Text);
                    mainwin.Show();
                 
                this.Close();
                
            }
            else
            {
                MessageBox.Show("Login Failed");
            }
            conn.Close();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}



