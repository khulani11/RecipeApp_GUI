using RecipeApp_GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using RecipeApp_GUI.Utilities;
using System.Xml;
using System.Windows.Markup;
using Microsoft.Win32;
using Newtonsoft.Json;
namespace RecipeApp_GUI.Views
{
   //This is the AddRecipe class that will allow us to collect your data and then save it to a JSON File.
    public partial class AddRecipe : UserControl
    {
        //These two lines of code incriments your labels for our ingredient number and step number
        private int ingredientCount = 1;
        private int stepCount = 1;
        //this method adds the datacontet to the AddRecipe XAML and uses the object of AddRecipeVM using the name rep
        public AddRecipe()
        {
            InitializeComponent(); 
            this.DataContext = rep;
            
        }
        List<Ingredient> ingredientList = new List<Ingredient>();
        AddRecipeVM rep = new AddRecipeVM();
        Ingredient ing = new Ingredient();
        stepDescription stpD = new stepDescription();
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // this Resets recipe name textbox
            recipeNameBox.Text = string.Empty;

            // Removes additional ingredient GUI elements
            while (ingredientsContainer.Children.Count > 1)
            {
                ingredientsContainer.Children.RemoveAt(ingredientsContainer.Children.Count - 1);
            }

            // Resets the ingredient GUI element to default state
            Border ingredientBorder = ingredientsContainer.Children[0] as Border;
            Grid ingredientGrid = ingredientBorder.Child as Grid;
            if (ingredientGrid != null)
            {
                TextBox nameTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "TextBox2");
                ComboBox foodGroupComboBox = ingredientGrid.Children.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "fGroup");
                TextBox amountTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "amountBox");
                TextBox unitTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "unitBox");
                TextBox calorieTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "calorieBox");

                if (nameTextBox != null && foodGroupComboBox != null && amountTextBox != null && unitTextBox != null && calorieTextBox != null)
                {
                    nameTextBox.Text = string.Empty;
                    foodGroupComboBox.SelectedItem = null;
                    amountTextBox.Text = string.Empty;
                    unitTextBox.Text = string.Empty;
                    calorieTextBox.Text = string.Empty;
                }
            }

            // Removes any additional step GUI elements
            while (stepContainer.Children.Count > 1)
            {
                stepContainer.Children.RemoveAt(stepContainer.Children.Count - 1);
            }

            // Resets the step GUI element to default state
            Border stepBorder = stepContainer.Children[0] as Border;
            Grid stepGrid = stepBorder.Child as Grid;
            if (stepGrid != null)
            {
                TextBox stepTextBox = stepGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "stepBox");
                if (stepTextBox != null)
                {
                    stepTextBox.Text = string.Empty;
                }
            }
        }
        //This button saves the data to a user-selected external JSON file.
        //It is advised that the user choose a folder for the files to go in; by default, this is the project folder.
        private void save_Button(object sender, RoutedEventArgs e)
        {

            // Retrieve recipe name
            string recipeName = recipeNameBox.Text;

            // Create an instance of the AddRecipeVM class
            AddRecipeVM recipe = new AddRecipeVM
            {
                recName = recipeName,
            
                stepDescriptions = new stepDescription()
            };

            // Prepare the object to be serialized as JSON
            var recipeData = new
            {
                RecipeName = recipe.recName,
                Ingredients = GetIngredientsData(),
                Steps = GetStepDescriptionsData()
            };

            // Convert the object to JSON
            string json = JsonConvert.SerializeObject(recipeData, Newtonsoft.Json.Formatting.Indented);
            string initialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes");
            // Show the SaveFileDialog to allow the user to choose the file location and name
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
            saveFileDialog.InitialDirectory = initialDirectory;
            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Save the JSON to the selected file
                    File.WriteAllText(filePath, json);

                    // Show a message box indicating successful save
                    MessageBox.Show("Recipe was saved successfully!", "Save Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    // Show an error message box if there's an issue with saving the file
                    MessageBox.Show($"Error saving the file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        //This button duplicates the GUI element for the ingredient box
        private void addIng(object sender, RoutedEventArgs e)
        {
            //This var re-colors the new element and adds the same styling as the previous element.
            var newIngredient = new Border
            {
                Background = Brushes.SeaGreen,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 10, 0, 0),
                Height = 120
            };
            //Then a new instance of the Grid is created.
            var grid = new Grid();

            // Copy the content of the existing GUI element to the new GUI element
            foreach (UIElement child in ingGrid.Children)
            {
                grid.Children.Add(CloneUIElement(child));
            }
            Label ingredientLabel = grid.Children.OfType<Label>().FirstOrDefault();
            if (ingredientLabel != null)
            {
                int ingredientCount = ingredientsContainer.Children.Count + 1;
                ingredientLabel.Content = $"INGREDIENT {ingredientCount}";
            }
            Button addButton = grid.Children.OfType<Button>().FirstOrDefault(b => b.Content.ToString() == "+");
            if (addButton != null)
            {
                addButton.Click += addIng; // Attach the addIng event handler to the addButton
            }

            Button removeButton = grid.Children.OfType<Button>().FirstOrDefault(b => b.Content.ToString() == "-");
            if (removeButton != null)
            {
                removeButton.Click += removeIng; // Attach the removeIng event handler to the removeButton
            }
            newIngredient.Child = grid;

            // Add the new GUI element to the container
            ingredientsContainer.Children.Add(newIngredient);
           


        }
        //This button removes the GUI element duplicates unless there is only one left
        private void removeIng(object sender, RoutedEventArgs e)
        {
           //this condition removes any number of GUI Children as long as theres more than one
            if (ingredientsContainer.Children.Count > 1)
            {
                ingredientsContainer.Children.RemoveAt(ingredientsContainer.Children.Count - 1);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
        //These lines of code is what allows our XAML to duplicate the elements
        private UIElement CloneUIElement(UIElement element)
        {
            // XAML serialization is used to duplicate the UI element.
            var xaml = XamlWriter.Save(element);
            var reader = new StringReader(xaml);
            var xmlReader = XmlReader.Create(reader);
            return XamlReader.Load(xmlReader) as UIElement;
        }
        //This button duplicates the GUI element for step box
        private void addStep(object sender, RoutedEventArgs e)
        {
            var newStep = new Border
            {
                Background = Brushes.LightGreen,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 10, 0, 0),
                Height = 80
            };

            var grid = new Grid();

            // Copy the content of the existing GUI element to the new GUI element
            foreach (UIElement child in stepGrid.Children)
            {
                grid.Children.Add(CloneUIElement(child));
            }

            Label stepLabel = grid.Children.OfType<Label>().FirstOrDefault();
            if (stepLabel != null)
            {
                int stepCount = stepContainer.Children.Count + 1;
                stepLabel.Content = $"STEP {stepCount}";
            }

            Button addButton = grid.Children.OfType<Button>().FirstOrDefault(b => b.Content.ToString() == "+");
            if (addButton != null)
            {
                addButton.Click += addStep;
            }

            Button removeButton = grid.Children.OfType<Button>().FirstOrDefault(b => b.Content.ToString() == "-");
            if (removeButton != null)
            {
                removeButton.Click += removeStep;
            }

            newStep.Child = grid;

            // Add the new GUI element to the container
            stepContainer.Children.Add(newStep);
        }
        //This button removes the GUI element duplicates unless there is only one left
        private void removeStep(object sender, RoutedEventArgs e)
        {
            if (stepContainer.Children.Count > 1)
            {
                stepContainer.Children.RemoveAt(stepContainer.Children.Count - 1);
            }
        }

        private List<object> GetIngredientsData()
        {
            List<object> ingredientsData = new List<object>();

            foreach (Border ingredientBorder in ingredientsContainer.Children)
            {
                Grid ingredientGrid = ingredientBorder.Child as Grid;
                if (ingredientGrid != null)
                {
                    Label ingredientLabel = ingredientGrid.Children.OfType<Label>().FirstOrDefault();
                    if (ingredientLabel != null)
                    {
                        string ingredientName = ingredientLabel.Content.ToString();

                        TextBox nameTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "TextBox2");
                        ComboBox foodGroupComboBox = ingredientGrid.Children.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "fGroup");
                        TextBox amountTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "amountBox");
                        TextBox unitTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "unitBox");
                        TextBox calorieTextBox = ingredientGrid.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "calorieBox");

                        if (nameTextBox != null && foodGroupComboBox != null && amountTextBox != null && unitTextBox != null && calorieTextBox != null)
                        {
                            string name = nameTextBox.Text;
                            string foodGroup = foodGroupComboBox.SelectedItem?.ToString();
                            string amount = amountTextBox.Text;
                            string unit = unitTextBox.Text;
                            string calorie = calorieTextBox.Text;

                            var ingredientData = new
                            {
                                Name = name,
                                FoodGroup = foodGroup,
                                Amount = amount,
                                Unit = unit,
                                Calorie = calorie
                            };

                            ingredientsData.Add(ingredientData);
                        }
                    }
                }
            }

            return ingredientsData;
        }

        private List<string> GetStepDescriptionsData()
        {
            List<string> stepDescriptionsData = new List<string>();

            foreach (Border stepBorder in stepContainer.Children)
            {
                Grid stepGrid = stepBorder.Child as Grid;
                if (stepGrid != null)
                {
                    Label stepLabel = stepGrid.Children.OfType<Label>().FirstOrDefault();
                    if (stepLabel != null)
                    {
                        string stepDescription = stepLabel.Content.ToString();
                        stepDescriptionsData.Add(stepDescription);
                    }
                }
            }

            return stepDescriptionsData;
        }
    }

}
