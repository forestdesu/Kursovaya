using System;
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

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SelectGenres.xaml
    /// </summary>
    public partial class SelectGenres : UserControl
    {
        public List<string> SelectedGenres { get; } = new List<string>();
        public SelectGenres()
        {
            InitializeComponent();
        }
        public void SelectGenresButton_Click()
        {
            SelectedGenres.Clear();
            foreach (CheckBox checkBox in GenresListBox.Children)
            {
                if (checkBox.IsChecked == true)
                {
                    SelectedGenres.Add(checkBox.Content.ToString());
                    // Выбран жанр, используйте его в вашей логике
                }
            }
            
            // Скройте UserControl после выбора
            Visibility = Visibility.Collapsed;
        }

    }
}
