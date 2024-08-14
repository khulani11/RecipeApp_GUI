using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using RecipeApp_GUI.Utilities;

namespace RecipeApp_GUI.Views
{
    public partial class RecipeList : UserControl
    {
        //these two collections are used for the recipeList and the filtered list as well.
        //the filterRecipeList is what allows the user to search for a recipe with the searchbox
        public ObservableCollection<RecipeFile> recipeList { get; set; }
        public ObservableCollection<RecipeFile> filteredRecipeList { get; set; }
        private string searchText;
        //This well let the XAML bind the data points to this file
        public RecipeList()
        {
            InitializeComponent();
            recipeList = new ObservableCollection<RecipeFile>();
            filteredRecipeList = new ObservableCollection<RecipeFile>();
            DataContext = this;

            Loaded += RecipeList_Loaded;
        }

        private void RecipeList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecipeFiles();
        }

        private void LoadRecipeFiles()
        {
            // this is where your recipe folder path will be located.
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes");


            try
            {
                // This retrieves all the JSON files in the folder
                string[] filePaths = Directory.GetFiles(folderPath, "*.json");

                foreach (string filePath in filePaths)
                {
                    // This will read the file's contents and deserialize them.
                    string fileContents = File.ReadAllText(filePath);
                    Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);

                    // Using data from the file path, this generates a new recipe file object and adds it to the collection.

                    RecipeFile recipeFile = new RecipeFile
                    {
                        RecipeName = Path.GetFileNameWithoutExtension(filePath),
                        FilePath = filePath,
                        DateCreated = File.GetCreationTime(filePath),
                        DateModified = File.GetLastWriteTime(filePath),
                        Calorie = recipe.Calorie,
                        FoodGroup = recipe.FoodGroup
                    };
                    recipeList.Add(recipeFile);
                }
                //This sorts the recipes in our datagrid in alphabetical order.
                recipeList = new ObservableCollection<RecipeFile>(recipeList.OrderBy(r => r.RecipeName));
                filteredRecipeList = recipeList;
                recipeDataGrid.ItemsSource = filteredRecipeList;
            }
            catch (Exception ex)
            {
                //These will give you an error message if the page can't find your folder or if the JSON information is wrong
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"An error has occurred while loading recipe files: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }
        //This will display a message box for the recipe that you have selected as soon as you double click it.
        private void recipeDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-clicked recipe file
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                // Accesses the filepath 
                string filePath = selectedRecipe.FilePath;

                // Read the file contents and display in a user-friendly format
                try
                {
                    string fileContents = File.ReadAllText(filePath);
                    Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);
                                        // Build the recipe details message
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("Recipe Name: " + recipe.Name);

                    // Ingredients
                    message.AppendLine("Ingredients:");
                    foreach (Ingredient ingredient in recipe.Ingredients)
                    {
                        string ingredientDetails = $"{ingredient.Amount} {ingredient.Unit} of {ingredient.IngName}";
                        message.AppendLine("- " + ingredientDetails);
                    }

                    // Instructions
                    message.AppendLine("Instructions:");
                    foreach (string step in recipe.Instructions)
                    {
                        message.AppendLine("- " + step);
                    }

                    // Display the message box
                    MessageBox.Show(message.ToString(), "Recipe Details", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error has occurred while reading the file: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        //This allows you to edit the contents of your JSON file 
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                // Access the file path
                string filePath = selectedRecipe.FilePath;

                
                Process.Start("notepad.exe", filePath);
            }
        }
        //This button deletes the file in the folder
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                // Access the file path
                string filePath = selectedRecipe.FilePath;

                try
                {
                    // Delete the file
                    File.Delete(filePath);

                    // Remove the deleted recipe file from the collection
                    recipeList.Remove(selectedRecipe);
                    filteredRecipeList.Remove(selectedRecipe);

                    MessageBox.Show("The file was delete successfully.", "File Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"An error has occurred while loading recipe file '{filePath}': {ex}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                }

            }
        }
        //This will allow you to search for a recipe using the name,calorie or food group
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            searchText = searchTextBox.Text.ToLower();
            ApplyFilter();
        }
        //This method is your filter code that will implement the search filter logic for your datagrid.
        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredRecipeList = recipeList;
            }
            else
            {
                filteredRecipeList = new ObservableCollection<RecipeFile>(
                    recipeList.Where(r =>
                        r.RecipeName.ToLower().Contains(searchText) ||
                        r.Calorie.ToString().Contains(searchText) ||
                        r.FoodGroup.ToLower().Contains(searchText)
                    ));
            }

            recipeDataGrid.DataContext = filteredRecipeList; // Update the DataContext of the DataGrid
        }
        //This code allows us to contain a folder that may contain JSON files for recipes
        private void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a Folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder",
                Filter = "Folders|*.none",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Multiselect = false,
                ValidateNames = false
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);

                // Get all JSON files in the selected folder
                string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

                // Load the JSON files
                var recipes = new List<Recipe>();
                foreach (var jsonFile in jsonFiles)
                {
                    string json = File.ReadAllText(jsonFile);
                    var recipe = Newtonsoft.Json.JsonConvert.DeserializeObject<Recipe>(json);
                    recipes.Add(recipe);
                }

                // Update the data grid
                recipeDataGrid.ItemsSource = recipes;
            }
        }
    }

    // this class holds the recipe information from your JSON which will be displayed in the datagrid.
    public class RecipeFile
    {
        public string RecipeName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int Calorie { get; set; }
        public string FoodGroup { get; set; }
    }

    // This recipe class is places ysour JSON file in the right structure 
    public class Recipe
    {
        public string Name { get; set; }
        public int Calorie { get; set; }
        public string FoodGroup { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public List<string> Instructions { get; set; }
    }

    // Define the Ingredient class to hold ingredient information
    public class Ingredient
    {
        public string Amount { get; set; }
        public string Unit { get; set; }
        public string IngName { get; set; }
    }
}
