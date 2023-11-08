using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SelectMovie.xaml
    /// </summary>
    public partial class SelectMovie : Page
    {
        List<string> ListSelectedGenres = new List<string>();
        List<string> ListSelectedSeason = new List<string>();
        List<string> ListSelectedAge = new List<string>();
        string TextSearch;
        public SelectMovie()
        {
            InitializeComponent();
            GenerateMovies();
            GenerateGenresBoxes();
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Определяем максимальное количество символов в TextBox
            int maxLength = 5;

            // Регулярное выражение для допустимого формата времени (часы:минуты)
            string pattern = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$";
            Regex regex = new Regex(pattern);

            // Получите текст из TextBox
            string newText = timeTextBox.Text + e.Text;

            // Проверьте, соответствует ли введенный текст формату
            if (regex.IsMatch(newText) && newText.Length <= maxLength)
            {
                e.Handled = false; // Разрешить ввод допустимого текста
            }
            else
            {
                e.Handled = true; // Запретить ввод недопустимого текста
            }

            // Если введены 5 символов, обработайте автоматическое обнуление минут и увеличение часов
            if (newText.Length == maxLength)
            {
                string[] parts = newText.Split(':');
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);

                if (minutes >= 60)
                {
                    hours += minutes / 60;
                    minutes %= 60;
                }

                timeTextBox.Text = $"{hours:D2}:{minutes:D2}";
                e.Handled = true; // Запретить ввод после автоматической коррекции
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Border btn = (Border)sender;
            MainWindow.idSelectMovie = Convert.ToInt32(btn.Tag);

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            Manager.MainFrame.Navigate(new MovieDetail());
        }

        private async Task GenerateMovies()
        {
            Movies.Children.Clear();
            int minutes = (int)timeSlider.Value;
            TimeSpan time = TimeSpan.Parse($"{minutes / 12:D2}:{minutes * 5 % 60:D2}:00");

            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {

                var query = context.Performance.AsQueryable();

                if (TextSearch != null)
                {
                    query = query.Where(p => p.Name.Contains(TextSearch));
                }
                if (ListSelectedGenres != null && ListSelectedGenres.Any())
                {
                    foreach (var selectedGenre in ListSelectedGenres)
                    {
                        query = query.Where(p => p.Genres.Any(g => g.Name == selectedGenre));
                    }
                }
                if (ListSelectedSeason != null && ListSelectedSeason.Any())
                {
                    query = query.Where(p => ListSelectedSeason.Contains(p.Seasons.Name));
                }
                if (ListSelectedAge != null && ListSelectedAge.Any())
                {
                    query = query.Where(p => ListSelectedAge.Contains(p.Age.Name));
                }              
                if (minutes != 0)
                {                   
                    query = query.Where(p => p.Duration <= time);
                }

                var results = await query.ToListAsync();

                for (int i= 0; i < results.Count / 2;i++)
                {
                    RowDefinition colDef = new RowDefinition();
                    Movies.RowDefinitions.Add(colDef);
                }
                int j = 0;
                string FullPath = AppDomain.CurrentDomain.BaseDirectory;
                FullPath = FullPath.Substring(0, FullPath.Length - 10);
                foreach (var item in results)
                {
                    Border mainBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141414")),
                        Height = 300,
                        VerticalAlignment = VerticalAlignment.Top,
                        CornerRadius = new CornerRadius(5),
                        Margin = new Thickness(0, 0, 0, 25), 
                        Tag = item.ID,
                    };
                    mainBorder.MouseLeftButtonUp += Button_Click;
                    Grid mainGrid = new Grid();
                    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Star) });
                    mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Star) });
                    string test = (FullPath + "1.jpg");
                    Border imageBorder = new Border
                    {
                        CornerRadius = new CornerRadius(5, 0, 0, 5),
                        Background = new ImageBrush
                        {
                            ImageSource = new BitmapImage(new Uri((FullPath + item.Img))),
                            Stretch = Stretch.UniformToFill,
                            Viewbox = new Rect(0.25, 0, 0.5, 1) // Обрезка по середине
                        }
                    };
                    Grid.SetColumn(imageBorder, 0);

                    Grid textGrid = new Grid
                    {
                        Margin = new Thickness(20, 5, 20, 10)
                    };
                    Grid.SetColumn(textGrid, 1);

                    StackPanel textStackPanel = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    TextBlock titleTextBlock = new TextBlock
                    {
                        Text = item.Name,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold
                    };

                    TextBlock genreTextBlock = new TextBlock
                    {
                        Text = string.Join(", ", item.Genres.ToList().Select(p => p.Name)),
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9da2a6"))
                    };

                    TextBlock descriptionTextBlock = new TextBlock
                    {
                        Margin = new Thickness(0, 15, 0, 0),
                        VerticalAlignment = VerticalAlignment.Bottom,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 18,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9da2a6")),
                        Text = item.Description.Length > 200 ? item.Description.Substring(0, 200) : item.Description,
                        FontWeight = FontWeights.Thin
                    };

                    textStackPanel.Children.Add(titleTextBlock);
                    textStackPanel.Children.Add(genreTextBlock);
                    textStackPanel.Children.Add(descriptionTextBlock);

                    textGrid.Children.Add(textStackPanel);

                    mainGrid.Children.Add(imageBorder);
                    mainGrid.Children.Add(textGrid);

                    mainBorder.Child = mainGrid;

                    // Добавляем кнопку в окно
                    Movies.Children.Add(mainBorder);
                    Grid.SetColumn(mainBorder, j % 2 == 1 ? 2 : 0 );
                    Grid.SetRow(mainBorder, j / 2);
                    j++;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int minutes = (int)timeSlider.Value;
            timeTextBox.Text = $"{minutes / 12:D1}:{minutes * 5 % 60:D2}";
        }

        private void GenresFilter()
        {
            ListSelectedGenres.Clear();
            foreach (CheckBox checkBox in GenresListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    ListSelectedGenres.Add(checkBox.Content.ToString());
                }
            }
        }

        private void SeasonFilter()
        {
            ListSelectedSeason.Clear();
            foreach (CheckBox checkBox in SeasonListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    ListSelectedSeason.Add(checkBox.Content.ToString());
                }
            }
        }

        private void AgeFilter()
        {
            ListSelectedAge.Clear();
            foreach (CheckBox checkBox in AgeListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    ListSelectedAge.Add(checkBox.Content.ToString());
                }
            }
        }

        private async void ApplyFilters(object sender, RoutedEventArgs e)
        {
            GenresFilter();
            SeasonFilter();
            AgeFilter();
            TextSearch = BoxSearch.Text;
            await GenerateMovies();
        }

        private async void ResetFilters(object sender, RoutedEventArgs e)
        {
            ListSelectedGenres.Clear();
            ListSelectedSeason.Clear();
            ListSelectedAge.Clear();
            foreach (CheckBox checkBox in GenresListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    checkBox.IsChecked = false;
                }
            }
            foreach (CheckBox checkBox in SeasonListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    checkBox.IsChecked = false;
                }
            }
            foreach (CheckBox checkBox in AgeListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    checkBox.IsChecked = false;
                }
            }
            timeSlider.Value = 0;
            BoxSearch.Text = "";
            TextSearch = "";
            await GenerateMovies();
        }

        private async void GenerateGenresBoxes()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                var ListOfGenres = await context.Genres.ToListAsync();
                var ListOfSeason = await context.Seasons.ToListAsync();
                var ListOfAge = await context.Age.ToListAsync();

                foreach (var item in ListOfGenres)
                {
                    CheckBox genresBox = new CheckBox();
                    genresBox.Content = item.Name;
                    genresBox.Tag = item.ID;
                    GenresListBox.Children.Add(genresBox);
                }               
                foreach (var item in ListOfSeason)
                {
                    CheckBox seasonBox = new CheckBox();
                    seasonBox.Content = item.Name;
                    seasonBox.Tag = item.ID;
                    SeasonListBox.Children.Add(seasonBox);
                }              
                foreach (var item in ListOfAge)
                {
                    CheckBox ageBox = new CheckBox();
                    ageBox.Content = item.Name;
                    ageBox.Tag = item.ID;
                    AgeListBox.Children.Add(ageBox);
                }
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextSearch = ((TextBox)sender).Text;
                GenerateMovies();
            }
        }
    }
}
