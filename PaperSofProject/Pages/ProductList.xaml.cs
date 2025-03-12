using PaperSofProject.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace PaperSofProject.PAges
{
    /// <summary>
    /// Логика взаимодействия для ProductList.xaml
    /// </summary>
    public partial class ProductList : Page
    {
        public List<Product> allProducts = App.db.Product.ToList();
        public List<Product> FilterProducts = App.db.Product.ToList();
        public List<TypeProduct> AllTypeProduct = App.db.TypeProduct.ToList();
        public int _currentPage = 1;

        public int _productsPerPage = 4;
        public int _totalPages;

        public ProductList()
        {
            InitializeComponent();
            LoadProducts();
            List<TypeProduct> type = App.db.TypeProduct.ToList();
            type.Insert(0, new TypeProduct() { ID = 1000000, Name = "Все типы"});
            
        }
        
        private void LoadProducts()
        {
            ProductsItemsControl.Items.Clear();
            int a = 20 * _currentPage;
            int b = a - 20;

            for (int i = b; i <= a; i++)
            {
                if (FilterProducts.Count > i)
                {
                    ProductsItemsControl.Items.Add(FilterProducts[i]);
                }
            }


            PageInfoTextBlock.Text = $"{_currentPage} / {FilterProducts.Count / 20}";
            

        }

        

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void ApplyFiltersAndSorting()
        {
            string searchText = SearchTextBox.Text.ToLower();

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                FilterProducts = allProducts.Where(x => x.Name.ToLower().Contains(searchText) ||
                (x.Description ?? "").ToLower().Contains(searchText)).ToList();

            }
           
            else
            {
                FilterProducts = allProducts;
            }

            // Фильтрация по типу продукта

            SortTypeProduct();

            // Применение сортировки
            SortProducts();

            _currentPage = 1;
            LoadProducts();
        }

        private void SortProducts()
        {
            switch (SortComboBox.SelectedIndex)
            {
                case 0: // Наименование (А-Я)
                    FilterProducts = FilterProducts.OrderBy(p => p.Name).ToList();
                    break;
                case 1: // Наименование (Я-А)
                    FilterProducts = FilterProducts.OrderByDescending(p => p.Name).ToList();
                    break;
                case 2: // Цех (по возрастанию)
                    FilterProducts = FilterProducts.OrderBy(p => p.Departament).ToList();
                    break;
                case 3: // Цех (по убыванию)
                    FilterProducts = FilterProducts.OrderByDescending(p => p.Departament).ToList();
                    break;
                case 4: // Минимальная стоимость (по возрастанию)
                    FilterProducts = FilterProducts.OrderBy(p => Convert.ToDecimal(p.Min_Price_For_Agent)).ToList();
                    break;
                case 5: // Минимальная стоимость (по убыванию)
                    FilterProducts = FilterProducts.OrderByDescending(p => Convert.ToDecimal(p.Min_Price_For_Agent)).ToList();
                    break;
            }
        }

        private void SortTypeProduct()
        {
            if (FilterComboBox.SelectedIndex != 0)
            {
                if (FilterComboBox.SelectedIndex == 0) // все типы
                {
                    FilterProducts = FilterProducts.ToList();
                }
                if (FilterComboBox.SelectedIndex == 1) // три слоя
                {
                    FilterProducts = (List<Product>)FilterProducts.Where(x => x.ID_Type == 1).ToList();
                }
                if (FilterComboBox.SelectedIndex == 2) // два слоя
                {
                    FilterProducts = (List<Product>)FilterProducts.Where(x => x.ID_Type == 2).ToList();
                }
                if (FilterComboBox.SelectedIndex == 3) // один слой
                {
                    FilterProducts = (List<Product>)FilterProducts.Where(x => x.ID_Type == 5).ToList();
                }
                if (FilterComboBox.SelectedIndex == 4) //детская
                {
                    FilterProducts = (List<Product>)FilterProducts.Where(x => x.ID_Type == 3).ToList();
                }
                if (FilterComboBox.SelectedIndex == 5) //супер мягкая
                {
                    FilterProducts = (List<Product>)FilterProducts.Where(x => x.ID_Type == 4).ToList();
                }
            }
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage != 1)
            {
                _currentPage--;
                LoadProducts();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage != FilterProducts.Count / 20)
                _currentPage++;
            LoadProducts();
        }

        private void ProductsItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому был произведен двойной клик
            var clickedElement = e.OriginalSource as DependencyObject;

            // Ищем родительский элемент типа FrameworkElement (Border, StackPanel и т.д.)
            while (clickedElement != null && !(clickedElement is ContentPresenter))
            {
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            // Проверяем, что клик был сделан внутри ItemsControl
            if (clickedElement is ContentPresenter presenter)
            {
                // Получаем объект Product из DataContext
                if (presenter.DataContext is Product selectedProduct)
                {
                    // Открываем страницу редактирования с передачей объекта
                    App.main.myframe.NavigationService.Navigate(new PAges.AddamdEditProduct(selectedProduct));
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            App.main.myframe.NavigationService.Navigate(new PAges.AddamdEditProduct(null));
        }

        

    }
}
