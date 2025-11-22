using DemoExamSolution.Entities;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace DemoExamSolution.AdditionalWindows
{
    public partial class OrderForm : Window
    {
        private readonly AppDbContext _context;
        private Order _currentOrder;
        private bool _isEditMode;

        public OrderForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadComboBoxData();
            InitializeForAdd();
        }

        public OrderForm(Order order) : this()
        {
            _currentOrder = order;
            _isEditMode = true;
            InitializeForEdit();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Загрузка товаров
                ProductCbx.ItemsSource = _context.Products
                    .AsNoTracking()
                    .ToList();

                // Загрузка статусов заказов
                StatusCbx.ItemsSource = _context.OrderStatuses
                    .AsNoTracking()
                    .ToList();

                // Загрузка пунктов выдачи
                DeliveryPlaceCbx.ItemsSource = _context.OrderDeliveryPlaces
                    .AsNoTracking()
                    .ToList();

                // Загрузка клиентов
                ClientCbx.ItemsSource = _context.Users
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void InitializeForAdd()
        {
            Title = "Добавление заказа";

            // Устанавливаем текущую дату для заказа
            OrderDatePicker.SelectedDate = DateTime.Today;

            // Устанавливаем дату выдачи на 3 дня вперед
            DeliveryDatePicker.SelectedDate = DateTime.Today.AddDays(3);

            // Генерируем номер заказа
            OrderNumberTxt.Text = GenerateOrderNumber().ToString();

            // Генерируем код получения
            CodeTxt.Text = GenerateCode().ToString();
        }

        private void InitializeForEdit()
        {
            Title = "Редактирование заказа";

            if (_currentOrder != null)
            {
                OrderNumberTxt.Text = _currentOrder.OrderNumber.ToString();
                ProductCbx.SelectedValue = _currentOrder.IdProduct;
                StatusCbx.SelectedValue = _currentOrder.IdOrderStatus;
                DeliveryPlaceCbx.SelectedValue = _currentOrder.IdOrderDeliveryPlace;
                ClientCbx.SelectedValue = _currentOrder.IdClient;
                CodeTxt.Text = _currentOrder.Code.ToString();

                OrderDatePicker.SelectedDate = new DateTime(
                    _currentOrder.OrderDate.Year,
                    _currentOrder.OrderDate.Month,
                    _currentOrder.OrderDate.Day);

                DeliveryDatePicker.SelectedDate = new DateTime(
                    _currentOrder.DeliveryDate.Year,
                    _currentOrder.DeliveryDate.Month,
                    _currentOrder.DeliveryDate.Day);
            }
        }

        private int GenerateOrderNumber()
        {
            using (var context = new AppDbContext())
            {
                int maxNumber = context.Orders.Any()
                    ? context.Orders.Max(o => o.OrderNumber)
                    : 1000;
                return maxNumber + 1;
            }
        }

        private int GenerateCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                using (var saveContext = new AppDbContext())
                {
                    if (_isEditMode)
                    {
                        // Редактирование существующего заказа
                        var existingOrder = saveContext.Orders
                            .FirstOrDefault(o => o.Id == _currentOrder.Id);

                        if (existingOrder != null)
                        {
                            UpdateOrderFromForm(existingOrder);
                            saveContext.Orders.Update(existingOrder);
                        }
                    }
                    else
                    {
                        // Добавление нового заказа
                        var newOrder = new Order();
                        UpdateOrderFromForm(newOrder);
                        saveContext.Orders.Add(newOrder);
                    }

                    saveContext.SaveChanges();
                    MessageBox.Show("Данные заказа успешно сохранены!");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(OrderNumberTxt.Text) ||
                !int.TryParse(OrderNumberTxt.Text, out int orderNumber))
            {
                MessageBox.Show("Введите корректный номер заказа!");
                OrderNumberTxt.Focus();
                return false;
            }

            if (ProductCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар!");
                ProductCbx.Focus();
                return false;
            }

            if (StatusCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус заказа!");
                StatusCbx.Focus();
                return false;
            }

            if (DeliveryPlaceCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи!");
                DeliveryPlaceCbx.Focus();
                return false;
            }

            if (ClientCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента!");
                ClientCbx.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(CodeTxt.Text) ||
                !int.TryParse(CodeTxt.Text, out int code))
            {
                MessageBox.Show("Введите корректный код получения!");
                CodeTxt.Focus();
                return false;
            }

            if (OrderDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа!");
                OrderDatePicker.Focus();
                return false;
            }

            if (DeliveryDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату выдачи!");
                DeliveryDatePicker.Focus();
                return false;
            }

            if (OrderDatePicker.SelectedDate > DeliveryDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата выдачи не может быть раньше даты заказа!");
                DeliveryDatePicker.Focus();
                return false;
            }

            return true;
        }

        private void UpdateOrderFromForm(Order order)
        {
            order.OrderNumber = int.Parse(OrderNumberTxt.Text);
            order.IdProduct = (int)ProductCbx.SelectedValue;
            order.IdOrderStatus = (int)StatusCbx.SelectedValue;
            order.IdOrderDeliveryPlace = (int)DeliveryPlaceCbx.SelectedValue;
            order.IdClient = (int)ClientCbx.SelectedValue;
            order.Code = int.Parse(CodeTxt.Text);

            var orderDate = OrderDatePicker.SelectedDate.Value;
            order.OrderDate = new DateOnly(orderDate.Year, orderDate.Month, orderDate.Day);

            var deliveryDate = DeliveryDatePicker.SelectedDate.Value;
            order.DeliveryDate = new DateOnly(deliveryDate.Year, deliveryDate.Month, deliveryDate.Day);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}