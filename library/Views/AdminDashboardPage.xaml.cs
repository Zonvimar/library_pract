using library.ApplicationData;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
    public partial class AdminDashboardPage : Page
    {
        private List<Books> _allBooks;
        private List<Users> _allUsers;
        private List<BorrowingRecords> _allBorrowingRecords;
        private Books _selectedBook;
        private BorrowingRecords _selectedBorrowingRecord;

        public AdminDashboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInitialData();
            var currentUser = AppConnect.CurrentUser;
            if (currentUser != null)
            {
                WelcomeTextBlock.Text = $"Панель администратора - {currentUser.FullName}";
            }
        }

        private void LoadInitialData()
        {
            LoadBooks();
            LoadUsers();
            LoadBorrowingRecords();
            LoadBookFilters();
        }

        #region Управление книгами

        private void LoadBooks()
        {
            try
            {
                _allBooks = AppConnect.modelOdb.Books
                    .Include("Authors")
                    .Include("Genres")
                    .Include("Publishers")
                    .OrderBy(b => b.Title)
                    .ToList();

                ApplyBookFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookFilters()
        {
            try
            {
                var genres = AppConnect.modelOdb.Genres.OrderBy(g => g.GenreName).ToList();
                genres.Insert(0, new Genres { GenreID = 0, GenreName = "Все жанры" });
                BookGenreFilterComboBox.ItemsSource = genres;
                BookGenreFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фильтров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyBookFilters()
        {
            if (_allBooks == null) return;

            try
            {
                var filteredBooks = _allBooks.AsEnumerable();

                string searchTerm = BookSearchTextBox?.Text?.ToLower()?.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredBooks = filteredBooks.Where(b =>
                        (b.Title != null && b.Title.ToLower().Contains(searchTerm)) ||
                        (b.Authors != null && b.Authors.FullName != null && b.Authors.FullName.ToLower().Contains(searchTerm)) ||
                        (b.ISBN != null && b.ISBN.ToLower().Contains(searchTerm))
                    );
                }

                if (BookGenreFilterComboBox?.SelectedValue != null)
                {
                    int selectedGenreId = (int)BookGenreFilterComboBox.SelectedValue;
                    if (selectedGenreId > 0)
                    {
                        filteredBooks = filteredBooks.Where(b => b.GenreID == selectedGenreId);
                    }
                }

                if (ShowUnavailableCheckBox?.IsChecked == false)
                {
                    filteredBooks = filteredBooks.Where(b => b.IsAvailable);
                }

                AdminBooksDataGrid.ItemsSource = filteredBooks.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка применения фильтров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyBookFilters();
        }

        private void BookFilter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyBookFilters();
        }

        private void BookFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyBookFilters();
        }

        private void ResetBookFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            BookSearchTextBox.Text = "";
            BookGenreFilterComboBox.SelectedIndex = 0;
            ShowUnavailableCheckBox.IsChecked = true;
            ApplyBookFilters();
        }

        private void AdminBooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBook = AdminBooksDataGrid.SelectedItem as Books;
            EditBookButton.IsEnabled = _selectedBook != null;
            DeleteBookButton.IsEnabled = _selectedBook != null;
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addBookWindow = new BookEditWindow();
                addBookWindow.BookSaved += (s, args) => LoadBooks();

                if (addBookWindow.ShowDialog() == true)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия окна добавления книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editBookWindow = new BookEditWindow(_selectedBook);
                editBookWindow.BookSaved += (s, args) => LoadBooks();

                if (editBookWindow.ShowDialog() == true)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия окна редактирования книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBook == null) return;

            try
            {
                var result = MessageBox.Show($"Удалить книгу '{_selectedBook.Title}'?\nЭто действие нельзя отменить.",
                                           "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var activeBorrowings = AppConnect.modelOdb.BorrowingRecords
                        .Where(br => br.BookId == _selectedBook.BookID && br.ReturnDate == null)
                        .Count();

                    if (activeBorrowings > 0)
                    {
                        MessageBox.Show("Нельзя удалить книгу, которая находится на руках у читателей.",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    AppConnect.modelOdb.Books.Remove(_selectedBook);
                    AppConnect.modelOdb.SaveChanges();

                    MessageBox.Show("Книга успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении книги: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Управление читателями

        private void LoadUsers()
        {
            try
            {
                _allUsers = AppConnect.modelOdb.Users
                    .Include("Roles")
                    .Where(u => u.Roles.RoleName == "Читатель")
                    .OrderBy(u => u.FullName)
                    .ToList();

                foreach (var user in _allUsers)
                {
                    var activeBorrowingsCount = AppConnect.modelOdb.BorrowingRecords
                        .Where(br => br.UserID == user.UserID && br.ReturnDate == null)
                        .Count();

                    user.GetType().GetProperty("ActiveBorrowingsCount")?.SetValue(user, activeBorrowingsCount);
                }

                ApplyUserFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyUserFilters()
        {
            if (_allUsers == null) return;

            try
            {
                var filteredUsers = _allUsers.AsEnumerable();

                string searchTerm = UserSearchTextBox?.Text?.ToLower()?.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredUsers = filteredUsers.Where(u =>
                        (u.FullName != null && u.FullName.ToLower().Contains(searchTerm)) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTerm))
                    );
                }

                UsersDataGrid.ItemsSource = filteredUsers.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyUserFilters();
        }

        private void ResetUserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            UserSearchTextBox.Text = "";
            ApplyUserFilters();
        }

        #endregion


        private void LoadBorrowingRecords()
        {
            try
            {
                _allBorrowingRecords = AppConnect.modelOdb.BorrowingRecords
                    .Include("Users")
                    .Include("Books")
                    .OrderByDescending(br => br.BorrowDate)
                    .ToList();

                ApplyBorrowingFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки записей о выдачах: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyBorrowingFilters()
        {
            if (_allBorrowingRecords == null) return;

            try
            {
                var filteredRecords = _allBorrowingRecords.AsEnumerable();

                if (BorrowingStatusComboBox?.SelectedIndex > 0)
                {
                    switch (BorrowingStatusComboBox.SelectedIndex)
                    {
                        case 1: // Активные
                            filteredRecords = filteredRecords.Where(br => br.ReturnDate == null);
                            break;
                        case 2: // Возвращенные
                            filteredRecords = filteredRecords.Where(br => br.ReturnDate != null);
                            break;
                        case 3: // Просроченные
                            filteredRecords = filteredRecords.Where(br => br.ReturnDate == null && br.DueDate < DateTime.Now.Date);
                            break;
                    }
                }

                string searchTerm = BorrowingSearchTextBox?.Text?.ToLower()?.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredRecords = filteredRecords.Where(br =>
                        (br.Users?.FullName != null && br.Users.FullName.ToLower().Contains(searchTerm)) ||
                        (br.Books?.Title != null && br.Books.Title.ToLower().Contains(searchTerm))
                    );
                }

                BorrowingRecordsDataGrid.ItemsSource = filteredRecords.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации выдач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BorrowingFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyBorrowingFilters();
        }

        private void BorrowingSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyBorrowingFilters();
        }

        private void BorrowingRecordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBorrowingRecord = BorrowingRecordsDataGrid.SelectedItem as BorrowingRecords;
            ForceReturnButton.IsEnabled = _selectedBorrowingRecord != null && _selectedBorrowingRecord.ReturnDate == null;
        }

        private void ForceReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBorrowingRecord == null) return;

            try
            {
                var result = MessageBox.Show($"Принудительно вернуть книгу '{_selectedBorrowingRecord.Books.Title}' от читателя '{_selectedBorrowingRecord.Users.FullName}'?",
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

                    MessageBox.Show("Книга принудительно возвращена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadBorrowingRecords();
                    LoadBooks();
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
    }
}