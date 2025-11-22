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
    /// Логика взаимодействия для ManagerWindow.xaml
    /// </summary>
    public partial class ManagerWindow : Window
    {
        public User CurrentUser { get; set; }
        private ObservableCollection<ProductViewModel> _allProducts;
        private readonly AppDbContext _context;

        public ManagerWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        public ManagerWindow(User user) : this() {
            CurrentUser = user;
            IntializeUserInfo();
            LoadProducts();
        }

        private void IntializeUserInfo()
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

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow(CurrentUser, "manager");
            orderWindow.Show();
            this.Hide();
        }
    }
}
