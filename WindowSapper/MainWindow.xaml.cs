using System.Windows;

namespace WindowSapper
{
    public partial class MainWindow : Window
    {
        private readonly ViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new ViewModel();
            DataContext = _viewModel;
            _viewModel.RequestClose += (s, e) => this.Close();
            this.Closed += MainWindow_Closed;
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _viewModel.DeleteFile();
        }
    }
}