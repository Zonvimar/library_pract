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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            ErrorMessageTextBlock.Text = "";

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorMessageTextBlock.Text = "Все поля обязательны для заполнения.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessageTextBlock.Text = "Пароли не совпадают.";
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                ErrorMessageTextBlock.Text = "Некорректный формат email.";
                return;
            }

            try
            {
                if (AppConnect.modelOdb.Users.Any(u => u.Email == email))
                {
                    ErrorMessageTextBlock.Text = "Пользователь с таким email уже существует.";
                    return;
                }


                var readerRole = AppConnect.modelOdb.Roles.FirstOrDefault(r => r.RoleName == "Читатель");
                if (readerRole == null)
                {
                    ErrorMessageTextBlock.Text = "Роль 'Читатель' не найдена в базе данных. Обратитесь к администратору.";
                    return;
                }

                Users newUser = new Users
                {
                    FullName = fullName,
                    Email = email,
                    Password = password,
                    RoleID = readerRole.RoleID
                };

                AppConnect.modelOdb.Users.Add(newUser);
                AppConnect.modelOdb.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.NavigateTo(new LoginPage());
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                ErrorMessageTextBlock.Text = $"Ошибка валидации: {fullErrorMessage}";
            }
            catch (Exception ex)
            {
                ErrorMessageTextBlock.Text = "Ошибка при регистрации: " + ex.Message;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateTo(new LoginPage());
            }
        }
    }
}
