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
using System.Windows.Shapes;

namespace library.Views
{

    public partial class BookEditWindow : Window
    {
        private Books _currentBook;
        private bool _isEditMode;

        public event EventHandler BookSaved;

        public BookEditWindow(Books bookToEdit = null)
        {
            InitializeComponent();
            _currentBook = bookToEdit;
            _isEditMode = bookToEdit != null;

            LoadInitialData();
            SetupWindow();
        }

        private void LoadInitialData()
        {
            try
            {
                var authors = AppConnect.modelOdb.Authors.OrderBy(a => a.FullName).ToList();
                AuthorComboBox.ItemsSource = authors;

                var genres = AppConnect.modelOdb.Genres.OrderBy(g => g.GenreName).ToList();
                GenreComboBox.ItemsSource = genres;

                var publishers = AppConnect.modelOdb.Publishers.OrderBy(p => p.PublisherName).ToList();
                PublisherComboBox.ItemsSource = publishers;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void SetupWindow()
        {
            if (_isEditMode)
            {
                Title = "Редактирование книги";
                TitleTextBlock.Text = "Редактирование книги";
                FillFormWithBookData();
            }
            else
            {
                Title = "Добавление новой книги";
                TitleTextBlock.Text = "Добавление новой книги";
            }
        }

        private void FillFormWithBookData()
        {
            if (_currentBook == null) return;

            try
            {
                BookTitleTextBox.Text = _currentBook.Title;
                ISBNTextBox.Text = _currentBook.ISBN;
                PublicationDatePicker.SelectedDate = _currentBook.PublicationDate;
                IsAvailableCheckBox.IsChecked = _currentBook.IsAvailable;

                if (_currentBook.AuthorID.HasValue)
                {
                    AuthorComboBox.SelectedValue = _currentBook.AuthorID.Value;
                }

                if (_currentBook.GenreID.HasValue)
                {
                    GenreComboBox.SelectedValue = _currentBook.GenreID.Value;
                }

                if (_currentBook.PublisherID.HasValue)
                {
                    PublisherComboBox.SelectedValue = _currentBook.PublisherID.Value;
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка заполнения формы: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                if (_isEditMode)
                {
                    UpdateExistingBook();
                }
                else
                {
                    CreateNewBook();
                }

                AppConnect.modelOdb.SaveChanges();
                BookSaved?.Invoke(this, EventArgs.Empty);

                MessageBox.Show(_isEditMode ? "Книга успешно обновлена!" : "Книга успешно добавлена!",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка сохранения: " + ex.Message);
            }
        }

        private bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(BookTitleTextBox.Text))
                errors.Add("Название книги обязательно для заполнения");

            if (AuthorComboBox.SelectedValue == null && string.IsNullOrWhiteSpace(AuthorComboBox.Text))
                errors.Add("Необходимо выбрать или добавить автора");

            if (GenreComboBox.SelectedValue == null)
                errors.Add("Необходимо выбрать жанр");


            if (errors.Any())
            {
                ShowError(string.Join("\n", errors));
                return false;
            }

            HideError();
            return true;
        }

        private void CreateNewBook()
        {
            var newBook = new Books
            {
                Title = BookTitleTextBox.Text.Trim(),
                ISBN = string.IsNullOrWhiteSpace(ISBNTextBox.Text) ? null : ISBNTextBox.Text.Trim(),
                PublicationDate = PublicationDatePicker.SelectedDate,
                IsAvailable = IsAvailableCheckBox.IsChecked ?? true,
                AuthorID = GetSelectedAuthorId(),
                GenreID = (int?)GenreComboBox.SelectedValue,
                PublisherID = GetSelectedPublisherId()
            };

            AppConnect.modelOdb.Books.Add(newBook);
        }

        private void UpdateExistingBook()
        {
            _currentBook.Title = BookTitleTextBox.Text.Trim();
            _currentBook.ISBN = string.IsNullOrWhiteSpace(ISBNTextBox.Text) ? null : ISBNTextBox.Text.Trim();
            _currentBook.PublicationDate = PublicationDatePicker.SelectedDate;
            _currentBook.IsAvailable = IsAvailableCheckBox.IsChecked ?? true;
            _currentBook.AuthorID = GetSelectedAuthorId();
            _currentBook.GenreID = (int?)GenreComboBox.SelectedValue;
            _currentBook.PublisherID = GetSelectedPublisherId();
        }

        private int? GetSelectedAuthorId()
        {
            if (AuthorComboBox.SelectedValue != null)
                return (int)AuthorComboBox.SelectedValue;

            if (!string.IsNullOrWhiteSpace(AuthorComboBox.Text))
            {
                var authorName = AuthorComboBox.Text.Trim();
                var existingAuthor = AppConnect.modelOdb.Authors
                    .FirstOrDefault(a => a.FullName.ToLower() == authorName.ToLower());

                if (existingAuthor != null)
                    return existingAuthor.AuthorID;

                // Создаем нового автора
                var newAuthor = new Authors { FullName = authorName };
                AppConnect.modelOdb.Authors.Add(newAuthor);
                AppConnect.modelOdb.SaveChanges();
                return newAuthor.AuthorID;
            }

            return null;
        }

        private int? GetSelectedPublisherId()
        {
            if (PublisherComboBox.SelectedValue != null)
                return (int)PublisherComboBox.SelectedValue;

            if (!string.IsNullOrWhiteSpace(PublisherComboBox.Text))
            {
                var publisherName = PublisherComboBox.Text.Trim();
                var existingPublisher = AppConnect.modelOdb.Publishers
                    .FirstOrDefault(p => p.PublisherName.ToLower() == publisherName.ToLower());

                if (existingPublisher != null)
                    return existingPublisher.PublisherID;

                var newPublisher = new Publishers { PublisherName = publisherName };
                AppConnect.modelOdb.Publishers.Add(newPublisher);
                AppConnect.modelOdb.SaveChanges();
                return newPublisher.PublisherID;
            }

            return null;
        }

        private void AddAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Введите имя нового автора в поле 'Автор' и нажмите 'Сохранить'. Автор будет создан автоматически.",
                           "Добавление автора", MessageBoxButton.OK, MessageBoxImage.Information);
            AuthorComboBox.Focus();
        }

        private void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для добавления нового жанра обратитесь к администратору системы.",
                           "Добавление жанра", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddPublisherButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Введите название нового издательства в поле 'Издательство' и нажмите 'Сохранить'. Издательство будет создано автоматически.",
                           "Добавление издательства", MessageBoxButton.OK, MessageBoxImage.Information);
            PublisherComboBox.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}