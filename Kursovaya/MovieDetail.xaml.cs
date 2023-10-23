using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZXing.QrCode.Internal;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для MovieDetail.xaml
    /// </summary>
    public partial class MovieDetail : Page
    {

        private string[] imagePaths = { @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\deadpool-11.jpg", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\16742908331746990.jpg", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\deadpool-11.jpg", };
        private int currentImageIndex = 0;
        private DispatcherTimer timer;
        private bool isAnimating = false;
        ListBoxItem selectedListBoxItem;

        public MovieDetail()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // Интервал между сменой изображений
            timer.Tick += Timer_Tick;
            timer.Start();
            CalendarLoad();
            LoadImage();           

            AddActorCard("Actor 1", "Character A", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
            AddActorCard("Actor 1", "Character A", @"C:\Users\Work\source\repos\Kursovaya\Kursovaya\img\Rayan.jpg");
        }

        private void LoadImage()
        {
            if (currentImageIndex < imagePaths.Length)
            {
                BitmapImage image = new BitmapImage(new Uri(imagePaths[currentImageIndex]));
                imageControl.Source = image;
            }
        }

        private void CalendarLoad()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                DateTime earliestDate = context.Sessions.Min(session => session.DateBegin);
                DateTime latestDate = context.Sessions.Max(session => session.DateBegin);
                CalendarDateRange allowedDates = new CalendarDateRange(
                    earliestDate,
                    latestDate
                );
                calendar.DisplayDateStart = earliestDate;
                calendar.DisplayDateEnd = latestDate;
                calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            }
                
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isAnimating)
            {
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
                AnimateImageTransition();
            }
        }

        private void AnimateImageTransition()
        {
            isAnimating = true;
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, e) =>
            {
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
                LoadImage();

                DoubleAnimation fadeInAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1)
                };
                imageControl.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, fadeInAnimation);
                isAnimating = false;
            };
            imageControl.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, animation);
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendar.SelectedDate.Value.Date >= calendar.DisplayDateStart.Value.Date && calendar.SelectedDate.Value <= calendar.DisplayDateEnd.Value.Date)
            {
                LoadSeans(calendar.SelectedDate.Value.Date);
            }
            
        }

        private void LoadSeans(DateTime selectedDateWithoutTime)
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                List<ListBoxItem> seansItems = new List<ListBoxItem>();
                var sessionsOnSelectedDate = context.Sessions.Where(session => EntityFunctions.TruncateTime(session.DateBegin) == selectedDateWithoutTime).ToList();
                foreach (var item in sessionsOnSelectedDate)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Content = $"Сеанс на {item.DateBegin}";
                    listBoxItem.Tag = item.ID; // Здесь устанавливаем Tag на объект Session или на его идентификатор, в зависимости от ваших потребностей.
                    seansItems.Add(listBoxItem);
                }
                sessionListBox.ItemsSource = seansItems;
            }            
        }


        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {

            if (selectedListBoxItem is null)
            {
                MessageBox.Show("Выберите сеанс перед покупкой билета.");
            }
            else
            {
                MainWindow.idSelectSeans = (int)selectedListBoxItem.Tag;
                Window parentWindow = Window.GetWindow(this);
                Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
                frame.Navigate(new SelectTicket());
            }
        }

        private void AddActorCard(string actorName, string characterName, string imagePath)
        {
            // Создайте контейнер для карточки актера
            StackPanel actorCard = new StackPanel
            {
                Width = 200,
                Height = 300,
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.LightGray,
            };

            // Создайте изображение актера
            System.Windows.Controls.Image actorImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new System.Uri(imagePath, System.UriKind.RelativeOrAbsolute)),
                Width = 150,
                Height = 200,
                Margin = new Thickness(10),
            };

            // Создайте текстовые блоки для имени актера и роли
            TextBlock actorNameText = new TextBlock
            {
                Text = actorName,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold
            };

            TextBlock characterNameText = new TextBlock
            {
                Text = characterName,
                TextAlignment = TextAlignment.Center
            };

            // Добавьте элементы в карточку актера
            actorCard.Children.Add(actorImage);
            actorCard.Children.Add(actorNameText);
            actorCard.Children.Add(characterNameText);

            // Добавьте карточку актера в WrapPanel
            actorWrapPanel.Children.Add(actorCard);
        }

        private void sessionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedListBoxItem = (ListBoxItem)sessionListBox.SelectedItem;
        }
    }
}
