using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemListPageLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext _context;

        private ProductCategory _selectedCategory;
        public ProductCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                FilterItemsByCategory();
            }
        }

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<ProductCategory> Categories { get; set; } = new ObservableCollection<ProductCategory>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ItemListPageLogic()
        {
            _context = MyQuanLyTrangSucContext.Instance ?? throw new ArgumentNullException(nameof(MyQuanLyTrangSucContext));

            LoadProducts();
            LoadCategories();
        }

        public void LoadProducts()
        {
            Products.Clear();
            var productsFromDb = _context.Products.AsNoTracking().Where(p => !p.IsDeleted).ToList();

            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }

            OnPropertyChanged(nameof(Products));
        }

        private void LoadCategories()
        {
            Categories.Clear();

            var distinctCategories = Products
                .Where(p => p.Category != null)
                .Select(p => p.Category)
                .Distinct() // this works because EF attaches by reference
                .ToList();

            foreach (var category in distinctCategories)
            {
                Categories.Add(category);
            }

            OnPropertyChanged(nameof(Categories));
        }
        private void FilterItemsByCategory()
        {
            if (SelectedCategory == null)
            {
                LoadProducts();
                return;
            }

            var filteredProducts = Products
                .Where(p => p.Category?.CategoryId == SelectedCategory.CategoryId)
                .ToList();

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }

            OnPropertyChanged(nameof(Products));
        }

        public void SearchItemsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LoadProducts(); // Reset to full list when search is empty
                return;
            }

            var filteredProducts = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToList() // Load into memory first
                .Where(p => p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }

            OnPropertyChanged(nameof(Products));
        }

        public void LoadAddItemWindow()
        {
            Window addWindow = new AddItemWindow();
            addWindow.ShowDialog();
            LoadProducts();
            LoadCategories();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}