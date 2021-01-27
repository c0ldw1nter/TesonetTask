using System.Linq;
using System.Windows;

namespace TesonetTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int WindowHeight { get; set; }
        private DataManager _dataManager;

        public MainWindow()
        {
            Logger.LogLocation = "log.txt";
            DataContext = this;
            WindowHeight = 400;
            _dataManager = new DataManager("https://playground.tesonet.lt/v1/tokens", "https://playground.tesonet.lt/v1/servers",
                "Data Source = (LocalDB)\\MSSQLLocalDB; Initial Catalog=tesonet_db; Password=partyanimal; User Id=tesonet; Integrated Security = True");
            InitializeComponent();
        }

        private async void Login(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginPanel.IsEnabled = false;

                var token = await _dataManager.GetAuthToken(UsernameBox.Text, PWBox.Password);
                if (token == null)
                {
                    MessageBox.Show(_dataManager.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Logger.Log("Auth token received");
                var serverList = await _dataManager.GetServerList(token);

                if (serverList == null)
                {
                    MessageBox.Show(_dataManager.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int newServers = _dataManager.PersistData(serverList);
                if (newServers == -1)
                    MessageBox.Show(_dataManager.LastError);
                else
                    Logger.Log($"{newServers} new servers received.");

                var allServers = _dataManager.GetLocalData();
                if (allServers == null)
                    MessageBox.Show(_dataManager.LastError);

                if(newServers!=-1)
                    ServersLabel.Content = $"Servers: {allServers.Count} ({newServers} new)";
                else
                    ServersLabel.Content = $"Servers: {allServers.Count}";

                Logger.Log($"Received a list of {serverList.Count} servers.");
                LoginPanel.Visibility = Visibility.Collapsed;
                allServers = allServers.OrderByDescending(z => z.distance).ToList();
                ServerListView.ItemsSource = allServers;
                ServerListPanel.Visibility = Visibility.Visible;
            }finally
            {
                LoginPanel.IsEnabled = true;
            }
        }
    }
}
