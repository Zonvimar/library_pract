using library.ApplicationData;
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

namespace library.Views
{
    /// <summary>
    /// Логика взаимодействия для ReaderDashboardPage.xaml
    /// </summary>
    public partial class ReaderDashboardPage : Page
    {
        private List<Books> _allBooks;


        public ReaderDashboardPage()
        {
            InitializeComponent();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBooks();
            var currentUser = AppConnect.CurrentUser;
            if (currentUser != null)
            {
                WelcomeTextBlock.Text = $"Добро пожаловать, {currentUser.FullName}!";
            }
        }

        private void LoadBooks()
        {
            try
            {
                _allBooks = AppConnect.modelOdb.Books
                                .Include("Authors")
                                .Include("Genres")
                                .Include("Publishers")
                                .Where(b => b.IsAvailable)
                                .OrderBy(b => b.Title)
                                .ToList();
                BooksDataGrid.ItemsSource = _allBooks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки каталога книг: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBooks();
        }


        private void FilterBooks()
        {
            string searchTerm = SearchTextBox.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                BooksDataGrid.ItemsSource = _allBooks;
                return;
            }

            if (_allBooks == null) return;

            var filteredBooks = _allBooks.Where(b =>
                (b.Title != null && b.Title.ToLower().Contains(searchTerm)) ||
                (b.Authors != null && b.Authors.FullName != null && b.Authors.FullName.ToLower().Contains(searchTerm)) ||
                (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm)) ||
                (b.Genres != null && b.Genres.GenreName != null && b.Genres.GenreName.ToLower().Contains(searchTerm))
            ).ToList();

            BooksDataGrid.ItemsSource = filteredBooks;
        }

        private void ResetSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }


        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.Logout();
        }



        private void SearchOrFilter_Changed(object sender, RoutedEventArgs e)
        {
            //ApplyCatalogFilters();
        }
        private void SearchOrFilter_Changed(object sender, EventArgs e)
        {
            //ApplyCatalogFilters();
        }

        private void GenreFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ApplyCatalogFilters();
        }
        private void AuthorFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ApplyCatalogFilters();
        }
        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e) // Для CheckBox
        {
            //ApplyCatalogFilters();
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MyBooksDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BooksDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MyBooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CatalogDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
