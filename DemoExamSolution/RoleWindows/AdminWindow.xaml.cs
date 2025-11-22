using DemoExamSolution.AdditionalWindows;
using DemoExamSolution.DTO;
using DemoExamSolution.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DemoExamSolution.RoleWindows
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public User CurrentUser { get; set; }
        private ObservableCollection<ProductViewModel> _allProducts;
        private readonly AppDbContext _context;

        public AdminWindow()
        {
            InitializeComponent();
            _context= new AppDbContext();
        }

        // Дополнительный конструктор класса для отображения ФИО пользователя
        public AdminWindow(User user) : this()
        {
            CurrentUser = user;
            InitializeUserInfo();
            LoadProducts();
        }

        // Инициализация ФИО пользователя
        private void InitializeUserInfo()
        {
            if (CurrentUser != null)
            {
                string fullName = $"{CurrentUser.Surname.Trim()} {CurrentUser.Name.Trim()} {CurrentUser.Patronymic.Trim()}";
                UserNameTextBlock.Text = fullName.Trim();
            }
            else
            {
                UserNameTextBlock.Text = "Неизвестный пользователь";
            }
        }

        private void LoadProducts()
        {
            // Загрузка данных о продуктах
            try
            {
                var productsCount = _context.Products.Count();
                MessageBox.Show($"Всего товаров: {productsCount}");

                var products = _context.Products
                    .Include(p => p.IdCategoryNavigation)
                    .Include(p => p.IdManufacturerNavigation)
                    .Include(p => p.IdSupplierNavigation)
                    .Include(p => p.IdProductTypeNavigation)
                    .AsEnumerable()
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        Articul = p.Articul,
                        ProductName = p.IdProductTypeNavigation.Type,
                        CategoryName = p.IdCategoryNavigation.CategoryName,
                        Description = p.Description,
                        Manufacturer = p.IdManufacturerNavigation.ManufacturerName,
                        Supplier = p.IdSupplierNavigation.SupplierName,
                        Price = p.Price,
                        UnitOfMeasurement = p.UnitOfMeasurement,
                        QuantityInStock = p.QuantityInStock,
                        Discount = p.Discount,
                        PhotoPath = p.PhotoPath
                    }).ToList();

                MessageBox.Show($"Загружено товаров: {products.Count}");
                foreach (var product in products)
                {
                    Debug.WriteLine($"Товар: {product.ProductName}, Цена: {product.Price}, Категория: {product.CategoryName}");
                }

                _allProducts = new ObservableCollection<ProductViewModel>(products);
                ProductsListBox.ItemsSource = _allProducts;

                MessageBox.Show($"Успешно загружено товаров: {products.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void SortCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Применение фильтров (поиск, сортировка, фильтрация)
        private void ApplyFilters()
        {
            if (_allProducts == null) return;

            var filtered = _allProducts.AsEnumerable();

            // Контекстный поиск
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                var search = SearchTextBox.Text.Trim().ToLower();
                filtered = filtered.Where(p =>
                    (p.ProductName?.Trim().ToLower().Contains(search) ?? false) ||
                    (p.Articul?.Trim().ToLower().Contains(search) ?? false) ||
                    (p.Description?.Trim().ToLower().Contains(search) ?? false) ||
                    (p.CategoryName?.Trim().ToLower().Contains(search) ?? false));
            }

            // Фильтрация по поставщику
            if (FilterCbx.SelectedIndex > 0)
            {
                var selectedItem = FilterCbx.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    var supplierName = selectedItem.Content.ToString().Trim();
                    filtered = filtered.Where(p => p.Supplier?.Trim() == supplierName);
                }
            }

            // Сортировка по возрастанию и убыванию
            if (SortCbx.SelectedIndex == 1)
                filtered = filtered.OrderBy(p => p.QuantityInStock);
            else if (SortCbx.SelectedIndex == 2)
                filtered = filtered.OrderByDescending(p => p.QuantityInStock);

            // Обновление данных
            ProductsListBox.ItemsSource = filtered.ToList();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new ProductForm();
            if (addWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListBox.SelectedItem is ProductViewModel selectedProduct)
            {
                using var context = new AppDbContext();
                var product = context.Products
                    .Include(p => p.IdProductTypeNavigation)
                    .FirstOrDefault(p => p.Id == selectedProduct.Id);

                if (product != null)
                {
                    var editWindow = new ProductForm(product);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadProducts();
                    }
                }
            }
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsListBox.SelectedItem is ProductViewModel selectedProduct)
                {
                    using var context = new AppDbContext();
                    var product = context.Products
                        .Include(p => p.IdProductTypeNavigation)
                        .FirstOrDefault(p => p.Id == selectedProduct.Id);

                    if (product == null || product.Id == 0)
                    {
                        MessageBox.Show("Товар для удаления не выбран! Пожалуйста, выберите товар.");
                        return;
                    }

                    if (IsProductInOrders(product.Id))
                    {
                        MessageBox.Show("Невозможно удалить товар, так как он содержится в заказах!",
                          "Ошибка удаления",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{product.Articul}\"?",
                                   "Подтверждение удаления",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var productToDelete = context.Products
                            .FirstOrDefault(p => p.Id == product.Id);

                        if (productToDelete != null)
                        {
                            context.Products.Remove(productToDelete);
                            context.SaveChanges();

                            MessageBox.Show("Товар успешно удален!");
                            LoadProducts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}");
            }
        }

        // Метод проверки товара на нахождение в заказе
        private bool IsProductInOrders(int productId)
        {
            using (var context = new AppDbContext())
            {
                return context.Orders.Any(o => o.IdProduct == productId);
            }
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ProductsListBox.SelectedItem != null;
            EditBtn.IsEnabled = hasSelection;
            DelBtn.IsEnabled = hasSelection;
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow(CurrentUser, "admin");
            orderWindow.Show();
            this.Hide();
        }
    }
}
