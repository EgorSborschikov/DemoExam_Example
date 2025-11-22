using DemoExamSolution.DTO;
using DemoExamSolution.Entities;
using DemoExamSolution.RoleWindows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DemoExamSolution.AdditionalWindows
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private readonly string _previousWindow;

        public OrderWindow(User currentUser, string previousWindow = "admin")
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentUser = currentUser;
            _previousWindow = previousWindow;

            CheckUserPermissions();
            LoadOrderData();
        }

        private void CheckUserPermissions()
        {
            bool isAdmin = _currentUser?.IdRole == 1; // роль администратора

            // Показываем/скрываем кнопки в зависимости от роли
            AddBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DelBtn.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Обновляем заголовок в зависимости от роли
            Title = isAdmin ? "Заказы - Администратор" : "Заказы - Менеджер";
        }

        private void LoadOrderData()
        {
            try
            {
                var orders = _context.Orders
                        .Include(o => o.IdProductNavigation)
                        .Include(o => o.IdOrderStatusNavigation)
                        .Include(o => o.IdOrderDeliveryPlaceNavigation)
                        .Select(o => new OrderViewModel
                        {
                            Id = o.Id,
                            OrderNumber = o.OrderNumber,
                            Articul = o.IdProductNavigation.Articul,
                            StatusName = o.IdOrderStatusNavigation.StatusName,
                            OrderAddress = $"{o.IdOrderDeliveryPlaceNavigation.City}, " +
                                  $"{o.IdOrderDeliveryPlaceNavigation.Street}, " +
                                  $"{o.IdOrderDeliveryPlaceNavigation.HomeNumber}",
                            OrderDate = o.OrderDate.ToString("dd.MM.yyyy"),
                            DeliveryDate = o.DeliveryDate.ToString("dd.MM.yyyy"),
                            Code = o.Code
                        })
                        .ToList();

                OrdersListBox.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных заказов: {ex.Message}");
            }
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isAdmin = _currentUser?.IdRole == 1;
            EditBtn.IsEnabled = isAdmin && OrdersListBox.SelectedItem != null;
            DelBtn.IsEnabled = isAdmin && OrdersListBox.SelectedItem != null;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addForm = new OrderForm();
                if (addForm.ShowDialog() == true)
                {
                    LoadOrderData(); // Обновляем список после добавления
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заказа: {ex.Message}");
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OrdersListBox.SelectedItem is OrderViewModel selectedOrder)
                {
                    using (var context = new AppDbContext())
                    {
                        var order = context.Orders
                            .FirstOrDefault(o => o.Id == selectedOrder.Id);

                        if (order != null)
                        {
                            var editForm = new OrderForm(order);
                            if (editForm.ShowDialog() == true)
                            {
                                LoadOrderData(); // Обновляем список после редактирования
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заказ для редактирования!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании заказа: {ex.Message}");
            }
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OrdersListBox.SelectedItem is OrderViewModel selectedOrder)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{selectedOrder.OrderNumber}?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var context = new AppDbContext())
                        {
                            var orderToDelete = context.Orders
                                .FirstOrDefault(o => o.Id == selectedOrder.Id);

                            if (orderToDelete != null)
                            {
                                context.Orders.Remove(orderToDelete);
                                context.SaveChanges();

                                MessageBox.Show("Заказ успешно удален!");
                                LoadOrderData(); // Обновляем список
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заказ для удаления!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}");
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_previousWindow == "admin")
            {
                // Открываем окно администратора
                var adminWindow = new AdminWindow(_currentUser);
                adminWindow.Show();
            }
            else
            {
                // Открываем окно менеджера
                var managerWindow = new ManagerWindow(_currentUser);
                managerWindow.Show();
            }

            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
