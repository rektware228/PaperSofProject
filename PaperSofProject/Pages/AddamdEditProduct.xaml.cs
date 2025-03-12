using System;
using PaperSofProject.DB;
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
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace PaperSofProject.PAges
{
    /// <summary>
    /// Логика взаимодействия для AddamdEditProduct.xaml
    /// </summary>
    public partial class AddamdEditProduct : Page
    {
        Product product;
        private bool isEditMode = false;
        private List<Material> allMaterials;

        public AddamdEditProduct(Product _product)
        {
            InitializeComponent();

            ProdTypeCBox.ItemsSource = App.db.TypeProduct.ToList();

            if (_product != null)
            {
                this.product = _product;
                isEditMode = true;
                ImageProduct.Source = product.Images;
            }
            else
            {
                this.product = new Product();
               
                if (this.product.ProductMaterial == null)
                    this.product.ProductMaterial = new HashSet<ProductMaterial>();
            }

            allMaterials = App.db.Material.ToList();

            
            if (product.ProductMaterial != null && product.ProductMaterial.Any())
            {
                foreach (var item in product.ProductMaterial)
                {
                    allMaterials.RemoveAll(m => m.ID == item.Material.ID);
                }
            }

            MaterialsComboBox.ItemsSource = allMaterials;
            UpdateMaterialsDataGrid();

            this.DataContext = this.product;

        }
        //
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
       
            if (product.Articul == null ||
                product.Name == null ||
                product.TypeProduct == null ||
                product.Min_Price_For_Agent == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!decimal.TryParse(product.Min_Price_For_Agent.ToString(), out decimal minCost) || minCost <= 0)
            {
                MessageBox.Show("Минимальная стоимость должна быть числом больше 0!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
          
            if (product.Count_Employee.HasValue && product.Count_Employee < 0)
            {
                MessageBox.Show("Количество людей должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (product.Departament.HasValue && product.Departament < 0)
            {
                MessageBox.Show("Номер цеха должен быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (App.db.Product.FirstOrDefault(x => x.Articul == product.Articul && x.ID != product.ID) != null)
            {
                MessageBox.Show("Артикул занят", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
          
                    var existingProduct = App.db.Product.FirstOrDefault(p => p.ID == product.ID);
                    if (existingProduct != null)
                    {
                        existingProduct.Articul = product.Articul;
                        existingProduct.Name = product.Name;
                        existingProduct.ID = product.ID;
                        existingProduct.Count_Employee = product.Count_Employee;
                        existingProduct.Departament = product.Departament;
                        existingProduct.Min_Price_For_Agent = product.Min_Price_For_Agent;
                        existingProduct.Description = product.Description;
                        existingProduct.Image = product.Image;
             
                    }
                }
                else
                {
                  App.db.Product.Add(product);
                }

                App.db.SaveChanges();
                MessageBox.Show("Продукт сохранен", "Норм", MessageBoxButton.OK, MessageBoxImage.Information);
                App.main.myframe.NavigationService.Navigate(new PAges.ProductList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    product.Image = imageBytes;
                    if (imageBytes.Length > 5000000)
                        throw new OutOfMemoryException("Вес фотографии превышает 4,5 Mb");
                    BitmapImage bitmap = new BitmapImage();
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    ImageProduct.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (product.ProductSale.Count != 0)
            {
                MessageBox.Show("Удаление невозможно, так как есть продажи");
            }
            if (product.ID == 0)
            {
                MessageBox.Show("Удаление невозможно, объект не найден");
            }
            else
            {
                App.db.Product.Remove(product);
                foreach (var item in App.db.ProductMaterial.Where(x => x.ID == product.ID).ToList())
                {
                    App.db.ProductMaterial.Remove(item);
                }
                App.db.SaveChanges();
                MessageBox.Show("Успешно удалено");
                App.main.myframe.NavigationService.Navigate(new PAges.ProductList());
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            App.main.myframe.NavigationService.Navigate(new PAges.ProductList());
        }

       
        private void UpdateMaterialsDataGrid()
        {
            MaterialsDataGrid.ItemsSource = product.ProductMaterial != null
                ? product.ProductMaterial.ToList()
                : new List<ProductMaterial>();
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsComboBox.SelectedItem is Material selectedMaterial)
            {
                if (int.TryParse(MaterialCountTextBox.Text, out int count) && count > 0)
                {
                 
                    if (product.ProductMaterial == null)
                        product.ProductMaterial = new HashSet<ProductMaterial>();

                    var newMaterial = new ProductMaterial
                    {
                        Material = selectedMaterial,
                        Count_Mat_Fot_One = count
                    };

                    product.ProductMaterial.Add(newMaterial);
                    App.db.ProductMaterial.Add(newMaterial);
                    App.db.SaveChanges();

                    MessageBox.Show("Материал добавлен!");
                    UpdateMaterialsDataGrid();

                    allMaterials.RemoveAll(m => m.ID == selectedMaterial.ID);
                    MaterialsComboBox.ItemsSource = null;
                    MaterialsComboBox.ItemsSource = allMaterials;

                    MaterialCountTextBox.Text = "0";
                    MaterialsComboBox.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Введите корректное количество!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Выберите материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsDataGrid.SelectedItem is ProductMaterial selectedMaterial)
            {
                product.ProductMaterial.Remove(selectedMaterial);
                App.db.ProductMaterial.Remove(selectedMaterial);
                App.db.SaveChanges();

                MessageBox.Show("Материал удален!");
                allMaterials.Add(selectedMaterial.Material);
                MaterialsComboBox.ItemsSource = null;
                MaterialsComboBox.ItemsSource = allMaterials;

                MaterialCountTextBox.Text = "0";
                MaterialsComboBox.SelectedIndex = -1;

                UpdateMaterialsDataGrid();
            }
            else
            {
                MessageBox.Show("Выберите материал для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
