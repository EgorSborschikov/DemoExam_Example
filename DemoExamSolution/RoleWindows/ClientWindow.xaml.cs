using DemoExamSolution.DTO;
using DemoExamSolution.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;

namespace DemoExamSolution.RoleWindows
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public User CurrentUser { get; set; }

        public ClientWindow()
        {
            InitializeComponent();
        }

        public ClientWindow(User user) : this() {
            CurrentUser = user;
            IntializeUserInfo();
            LoadProducts();
        }

        private void IntializeUserInfo()
        {
            if (CurrentUser != null)
            {
                string fullName = $"{CurrentUser.Surname} {CurrentUser.Name} {CurrentUser.Patronymic}";
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
                using (var context = AppDbContext.GetContext())
                {
                    var productsCount = context.Products.Count();
                    MessageBox.Show($"Всего товаров: {productsCount}");

                    var products = context.Products
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

                    ProductsListBox.ItemsSource = products;
                    MessageBox.Show($"Успешно загружено товаров: {products.Count}");
                }
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
    }
}
