using DemoExamSolution.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DemoExamSolution.AdditionalWindows
{
    /// <summary>
    /// Логика взаимодействия для ProductForm.xaml
    /// </summary>
    public partial class ProductForm : Window
    {
        private readonly AppDbContext _context;
        private Product _currentProduct;
        private bool _isEditMode;

        public ProductForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadComboBoxData();
            InitializeForAdd();
        }

        public ProductForm(Product product) : this()
        {
            _currentProduct = product;
            _isEditMode = true;
            InitializeForEdit();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Загрузка данных для выпадающих списков
                ProductTypeCbx.ItemsSource = _context.ProductTypes.AsNoTracking().ToList();
                SupplierCbx.ItemsSource = _context.Suppliers.AsNoTracking().ToList();
                ManufacturerCbx.ItemsSource = _context.Manufacturers.AsNoTracking().ToList();
                CategoryCbx.ItemsSource = _context.Categories.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void InitializeForAdd()
        {
            _currentProduct = new Product();
            Title = "Добавление товара";
            IdTxt.Text = "Автоматически";
        }

        private void InitializeForEdit()
        {
            Title = "Редактирование товара";

            if (_currentProduct != null)
            {
                // Заполнение полей данными выбранного продукта
                IdTxt.Text = _currentProduct.Id.ToString();
                ArticulTxt.Text = _currentProduct.Articul;
                ProductTypeCbx.SelectedValue = _currentProduct.IdProductType;
                UnitOfMeasurementTxt.Text = _currentProduct.UnitOfMeasurement;
                PriceTxt.Text = _currentProduct.Price.ToString();
                SupplierCbx.SelectedValue = _currentProduct.IdSupplier;
                ManufacturerCbx.SelectedValue = _currentProduct.IdManufacturer;
                CategoryCbx.SelectedValue = _currentProduct.IdCategory;
                DiscountTxt.Text = _currentProduct.Discount.ToString();
                QuantityInStockTxt.Text = _currentProduct.QuantityInStock.ToString();
                DescriptionTxt.Text = _currentProduct.Description.ToString();
                PhotoPathTxt.Text = _currentProduct?.PhotoPath;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                using (var context = new AppDbContext())
                {
                    if (_isEditMode)
                    {
                        var existingProduct = context.Products
                            .FirstOrDefault(p => p.Id == _currentProduct.Id);

                        if (existingProduct != null)
                        {
                            // Обновляем только нужные поля
                            UpdateProductFields(existingProduct);
                            context.Products.Update(existingProduct);
                        }
                    }
                    else
                    {
                        var newProduct = new Product();
                        newProduct.Id = GenerateNewProductId(context);
                        UpdateProductFields(newProduct);
                        context.Products.Add(newProduct);
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные сохранены успешно");
                    this.Close();
                }
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = "Ошибка сохранения в базу данных:\n";

                // Получаем внутренние исключения
                var innerException = dbEx.InnerException;
                while (innerException != null)
                {
                    errorMessage += $"- {innerException.Message}\n";
                    innerException = innerException.InnerException;
                }

                MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {GetFullExceptionMessage(ex)}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GenerateNewProductId(AppDbContext context)
        {
            int maxId = context.Products.Any()
                ? context.Products.Max(p => p.Id)
                : 0;

            return maxId + 1;
        }

        private void UpdateProductFields(Product product)
        {
            // Обновляем только ID связей, а не целые объекты
            product.Articul = ArticulTxt.Text;
            product.IdProductType = (int)ProductTypeCbx.SelectedValue;
            product.UnitOfMeasurement = UnitOfMeasurementTxt.Text;
            product.Price = decimal.Parse(PriceTxt.Text);
            product.IdSupplier = (int)SupplierCbx.SelectedValue;
            product.IdManufacturer = (int)ManufacturerCbx.SelectedValue;
            product.IdCategory = (int)CategoryCbx.SelectedValue;

            if (int.TryParse(DiscountTxt.Text, out int discount))
                product.Discount = discount;

            product.QuantityInStock = int.Parse(QuantityInStockTxt.Text);
            product.Description = DescriptionTxt.Text;
            product.PhotoPath = string.IsNullOrWhiteSpace(PhotoPathTxt.Text)
                ? "picture.png"
                : PhotoPathTxt.Text;
        }

        private bool ValidateInput()
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(ArticulTxt.Text))
            {
                MessageBox.Show("Введите артикул!");
                return false;
            }

            if (ProductTypeCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип продукта!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UnitOfMeasurementTxt.Text))
            {
                MessageBox.Show("Введите единицу измерения!");
                return false;
            }

            if (!decimal.TryParse(PriceTxt.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!");
                return false;
            }

            if (SupplierCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return false;
            }

            if (ManufacturerCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя!");
                return false;
            }

            if (CategoryCbx.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию!");
                return false;
            }

            if (!int.TryParse(QuantityInStockTxt.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество на складе!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTxt.Text))
            {
                MessageBox.Show("Введите описание!");
                return false;
            }

            return true;
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            if (ex == null) return string.Empty;

            string message = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                message += $"\n→ {inner.Message}";
                inner = inner.InnerException;
            }
            return message;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы уверены, что хотите выйти? Изменения не будут сохранены.");
            this.Close();
        }

        private void PhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
                openFileDialog.Title = "Выберите изображение товара";

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    // Получаем путь к папке Resources в output директории
                    string resourcesPath = System.IO.Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Resources");

                    // Создаем папку Resources, если её нет
                    if (!Directory.Exists(resourcesPath))
                    {
                        Directory.CreateDirectory(resourcesPath);
                    }

                    // Генерируем уникальное имя файла
                    string articul = string.IsNullOrEmpty(ArticulTxt.Text) ? "product" : ArticulTxt.Text;
                    string fileExtension = System.IO.Path.GetExtension(selectedFilePath);
                    string fileName = $"{articul}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    string destinationPath = System.IO.Path.Combine(resourcesPath, fileName);

                    // Копируем файл в папку Resources
                    File.Copy(selectedFilePath, destinationPath, true);

                    // Сохраняем только имя файла для БД
                    PhotoPathTxt.Text = fileName;

                    MessageBox.Show("Изображение успешно сохранено!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}");
            }
        }
    }
}
