using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KoboWorklist.WorklistSCP.Model;

namespace KoboWorklist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WorklistItemsProvider _worklistProvider;
        private List<WorklistItem> _worklistItems;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the WorklistItemsProvider with the database path and configuration
            _worklistProvider = new WorklistItemsProvider(App.DatabasePath, App.configuration);

            // Load the worklist items into the DataGrid
            RefreshWorklist();
        }

        private void RefreshWorklist()
        {
            _worklistItems = _worklistProvider.GetAllCurrentWorklistItems();
            WorklistDataGrid.ItemsSource = _worklistItems;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the EditWorklistItemWindow for adding a new item
            var newItem = new WorklistItem();
            var editWindow = new EditWorklistItemWindow(newItem);
            if (editWindow.ShowDialog() == true)
            {
                // Add the new item to the database
                _worklistProvider.AddWorklistItem(newItem);
                RefreshWorklist();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the DataGrid
            if (WorklistDataGrid.SelectedItem is WorklistItem selectedItem)
            {
                var editWindow = new EditWorklistItemWindow(selectedItem);
                if (editWindow.ShowDialog() == true)
                {
                    // Update the item in the database
                    _worklistProvider.UpdateWorklistItem(selectedItem);
                    RefreshWorklist();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to edit.", "Edit Worklist Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh the DataGrid
            RefreshWorklist();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the DataGrid
            if (WorklistDataGrid.SelectedItem is WorklistItem selectedItem)
            {
                // Confirm deletion
                var result = MessageBox.Show($"Are you sure you want to delete the item with Accession Number {selectedItem.AccessionNumber}?",
                    "Delete Worklist Item", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete the item from the database
                    _worklistProvider.DeleteWorklistItem(selectedItem);
                    RefreshWorklist();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to delete.", "Delete Worklist Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}