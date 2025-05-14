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

        private string _selectedCategory;
        public string SelectedCategory
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
        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

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
            var categoriesFromDb = _context.Products
                .Select(p => p.CategoryId)
                .Distinct()
                .ToList();

            foreach (var category in categoriesFromDb)
            {
                Categories.Add(category);
            }

            OnPropertyChanged(nameof(Categories));
        }

        private void FilterItemsByCategory()
        {
            if (string.IsNullOrEmpty(SelectedCategory))
            {
                LoadProducts();
                return;
            }

            var filteredProducts = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.CategoryId == SelectedCategory)
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
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}