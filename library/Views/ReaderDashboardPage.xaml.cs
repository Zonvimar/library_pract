using library.ApplicationData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

    public partial class ReaderDashboardPage : Page
    {
        private List<Books> _allBooks;
        private Books _selectedBook;
        private BorrowingRecords _selectedBorrowingRecord;

        public ReaderDashboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInitialData();
            var currentUser = AppConnect.CurrentUser;
            if (currentUser != null)
            {
                WelcomeTextBlock.Text = $"Добро пожаловать, {currentUser.FullName}!";
            }
        }

        private void LoadInitialData()
        {
            LoadBooks();
            LoadFilters();
            LoadMyBooks();
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

        private void LoadFilters()
        {
            try
            {
                var genres = AppConnect.modelOdb.Genres.OrderBy(g => g.GenreName).ToList();
                genres.Insert(0, new Genres { GenreID = 0, GenreName = "Все жанры" });
                GenreFilterComboBox.ItemsSource = genres;
                GenreFilterComboBox.SelectedIndex = 0;

                var authors = AppConnect.modelOdb.Authors.OrderBy(a => a.FullName).ToList();
                authors.Insert(0, new Authors { AuthorID = 0, FullName = "Все авторы" });
                AuthorFilterComboBox.ItemsSource = authors;
                AuthorFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фильтров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMyBooks()
        {
            try
            {
                var currentUser = AppConnect.CurrentUser;
                if (currentUser != null)
                {
                    var myBorrowedBooks = AppConnect.modelOdb.BorrowingRecords
                        .Include("Books")
                        .Include("Books.Authors")
                        .Where(br => br.UserID == currentUser.UserID && br.ReturnDate == null)
                        .OrderBy(br => br.DueDate)
                        .ToList();

                    MyBooksDataGrid.ItemsSource = myBorrowedBooks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки взятых книг: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchOrFilter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyCatalogFilters();
        }

        private void SearchOrFilter_Changed(object sender, EventArgs e)
        {
            ApplyCatalogFilters();
        }

        private void ApplyCatalogFilters()
        {
            if (_allBooks == null) return;

            try
            {
                var filteredBooks = _allBooks.AsEnumerable();

                string searchTerm = SearchTextBox?.Text?.ToLower()?.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredBooks = filteredBooks.Where(b =>
                        (b.Title != null && b.Title.ToLower().Contains(searchTerm)) ||
                        (b.Authors != null && b.Authors.FullName != null && b.Authors.FullName.ToLower().Contains(searchTerm)) ||
                        (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm)) ||
                        (b.Genres != null && b.Genres.GenreName != null && b.Genres.GenreName.ToLower().Contains(searchTerm))
                    );
                }

                if (GenreFilterComboBox?.SelectedValue != null)
                {
                    int selectedGenreId = (int)GenreFilterComboBox.SelectedValue;
                    if (selectedGenreId > 0)
                    {
                        filteredBooks = filteredBooks.Where(b => b.GenreID == selectedGenreId);
                    }
                }

                if (AuthorFilterComboBox?.SelectedValue != null)
                {
                    int selectedAuthorId = (int)AuthorFilterComboBox.SelectedValue;
                    if (selectedAuthorId > 0)
                    {
                        filteredBooks = filteredBooks.Where(b => b.AuthorID == selectedAuthorId);
                    }
                }

                BooksDataGrid.ItemsSource = filteredBooks.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка применения фильтров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchTextBox.Text = "";
                GenreFilterComboBox.SelectedIndex = 0;
                AuthorFilterComboBox.SelectedIndex = 0;
                BooksDataGrid.ItemsSource = _allBooks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сброса фильтров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CatalogDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBook = BooksDataGrid.SelectedItem as Books;
            BorrowButton.IsEnabled = _selectedBook != null;
        }

        private void MyBooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBorrowingRecord = MyBooksDataGrid.SelectedItem as BorrowingRecords;
            ReturnButton.IsEnabled = _selectedBorrowingRecord != null;
        }

        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для выдачи.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var currentUser = AppConnect.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("Ошибка: пользователь не авторизован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingBorrow = AppConnect.modelOdb.BorrowingRecords
                    .FirstOrDefault(br => br.UserID == currentUser.UserID &&
                                         br.BookId == _selectedBook.BookID &&
                                         br.ReturnDate == null);

                if (existingBorrow != null)
                {
                    MessageBox.Show("Вы уже взяли эту книгу.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var borrowingRecord = new BorrowingRecords
                {
                    UserID = currentUser.UserID,
                    BookId = _selectedBook.BookID,
                    BorrowDate = DateTime.Now.Date,
                    DueDate = DateTime.Now.Date.AddDays(14),
                    ReturnDate = null
                };

                AppConnect.modelOdb.BorrowingRecords.Add(borrowingRecord);

                _selectedBook.IsAvailable = false;

                AppConnect.modelOdb.SaveChanges();

                MessageBox.Show($"Книга '{_selectedBook.Title}' успешно выдана! Вернуть до: {borrowingRecord.DueDate:dd.MM.yyyy}",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadBooks();
                LoadMyBooks();
                BorrowButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выдаче книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBorrowingRecord == null)
            {
                MessageBox.Show("Выберите книгу для возврата.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = MessageBox.Show($"Вернуть книгу '{_selectedBorrowingRecord.Books.Title}'?",
                                           "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedBorrowingRecord.ReturnDate = DateTime.Now.Date;

                    var book = AppConnect.modelOdb.Books.Find(_selectedBorrowingRecord.BookId);
                    if (book != null)
                    {
                        book.IsAvailable = true;
                    }

                    AppConnect.modelOdb.SaveChanges();

                    MessageBox.Show("Книга успешно возвращена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadBooks();
                    LoadMyBooks();
                    ReturnButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при возврате книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.Logout();
        }

        private void BooksDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e) { }
        private void MyBooksDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e) { }
    }
}