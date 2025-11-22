using DemoExamSolution.Entities;
using DemoExamSolution.RoleWindows;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;

namespace DemoExamSolution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Кнопка авторизации
            string login = LoginTextBox.Text.Trim();
            string password = PasswordTextBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля или войдите через гостевой режим");
                return;
            }

            try
            {
                var user = AppDbContext.GetContext().Users
                    .Include(u => u.IdRoleNavigation) // Явная загрузка связанных данных
                    .AsEnumerable()
                    .FirstOrDefault(u => u.Login.Trim() == login && u.Password.Trim() == password);

                if (user != null)
                {
                    string roleName = user.IdRoleNavigation.RoleName.ToString().Trim() ?? "Неизвестная роль";
                    MessageBox.Show($"Вы вошли под {roleName}");
                    LoadRoleWindow(roleName, user);
                } 
                else
                {
                    MessageBox.Show("Ошибка входа! Проверьте логин и пароль");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
        }

        private void GuestLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Кнопка входа в гостевой режим
            MessageBox.Show("Вы вошли в гостевой режим");
            var guestWindow = new GuestWindow();
            guestWindow.Show();
            this.Close();
        }

        private void LoadRoleWindow(string _role, User user)
        {
            MessageBox.Show($"Полученная роль: '{_role}'");
            Debug.WriteLine($"Роль: '{_role}'");

            string normalizedRole = _role?.Trim() ?? "";

            Window newWindow = _role switch
            {
                "Администратор" => new AdminWindow(user),
                "Менеджер" => new ManagerWindow(user),
                "Авторизированный клиент" => new ClientWindow(user),
                _ => new GuestWindow() 
            };

            newWindow.Show();
            this.Close();
        }
    }
}