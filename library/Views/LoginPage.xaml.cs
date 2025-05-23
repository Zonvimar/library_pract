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

    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorMessageTextBlock.Text = "Email и пароль не должны быть пустыми.";
                return;
            }

            try
            {
                var user = AppConnect.modelOdb.Users
                              .FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    ErrorMessageTextBlock.Text = "";
                    AppConnect.CurrentUser = user;

                    MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        var userRole = AppConnect.modelOdb.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);

                        if (userRole != null)
                        {
                            if (userRole.RoleName == "Администратор")
                            {
                                mainWindow.NavigateTo(new AdminDashboardPage());
                            }
                            else if (userRole.RoleName == "Читатель")
                            {
                                mainWindow.NavigateTo(new ReaderDashboardPage());
                            }
                            else
                            {
                                ErrorMessageTextBlock.Text = "Роль пользователя не определена.";
                            }
                        }
                        else
                        {
                            ErrorMessageTextBlock.Text = "Не удалось определить роль пользователя.";
                        }
                    }
                }
                else
                {
                    ErrorMessageTextBlock.Text = "Неверный email или пароль.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessageTextBlock.Text = "Ошибка подключения к базе данных. " + ex.Message;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateTo(new RegisterPage());
            }
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(null, null);
            }
        }
    }
}