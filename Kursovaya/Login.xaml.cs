using System;
using System.Collections.Generic;
using System.Data;
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
using static System.Net.Mime.MediaTypeNames;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
            //
            //
            //
            //
            //
            //
            //
            //CreatePlace();
        }

        private List<int> RM(List<int> list)
        {
            List<int> myArray = new List<int> { };
            myArray = new List<int>(list);
            myArray.Reverse();

            return myArray;
        }

        private void CreatePlace()
        {
            List<int> parter = new List<int> { 12, 12, 12, 12, 12, 12, 11, 11, 11, 11, 11, 11, 11, 11 };
            List<int> amfitheatre = new List<int> { 5, 5, 5, 5, 5, 5, 5 };
            List<int> LMezzaine = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            List<int> Mezzaine = new List<int> { 3, 4, 5, 5, 5, 5 };
            List<List<int>> ListOfCols = new List<List<int>> { parter, parter, amfitheatre, new List<int> { 14, 14, 14, 12, 12, 12, 14 }, amfitheatre, LMezzaine, Mezzaine, new List<int> { 18, 16, 14, 14, 14, 14 }, Mezzaine, LMezzaine, RM(LMezzaine), RM(Mezzaine), new List<int> { 14, 14, 14, 14, 16, 18 }, RM(Mezzaine), RM(LMezzaine) };

            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                for (int sector = 0; sector < ListOfCols.Count; sector++)
                {
                    // Получаем сектор = [3, 4, 5, 5, 5, 5]
                    for (int row = 0; row < ListOfCols[sector].Count; row++)
                    {
                        // Количество колонок cols_count = 12
                        for (int seat = 0; seat < ListOfCols[sector][row]; seat++)
                        {
                            // Количество колонок 0 ... cols_count
                            var newPlace = new Place
                            {
                                HallsID = 1,
                                Row = row + 1,
                                Column = seat + 1,
                                SectorID = sector + 1,
                            };
                            context.Place.Add(newPlace);
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Показать анимацию загрузки
            //loadingControl.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() =>
                {
                    using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {

                            var DataUser = context.Users
                            .Where(p => p.Login == userLogin.Text && p.Password == userPassword.Password)
                            .FirstOrDefault();

                            if (DataUser != null)
                            {
                                // Вернуться на главный поток для обновления интерфейса
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    LoginSuccess(DataUser);
                                });
                            }
                            else
                            {
                                // Вернуться на главный поток для обновления интерфейса
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    WarningText.Text = "Ваш пароль или логин не правильны";
                                });
                            }
                        });
                    }
                });
            }
            finally
            {
                //loadingControl.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginSuccess(Users dataUser)
        {
            MainWindow.sessionUser = dataUser;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Window parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                // Закрыть родительское окно
                parentWindow.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new Registration());
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Button_Click(sender, e);
            }
        }

    }
}
