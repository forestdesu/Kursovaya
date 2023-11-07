using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
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
        int idSelectMovie = MainWindow.idSelectMovie;
        List<string> imagePaths = new List<string>();
        private int currentImageIndex = 0;
        private DispatcherTimer timer;
        private bool isAnimating = false;
        int? selectedSeansId;

        public MovieDetail()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // Интервал между сменой изображений
            timer.Tick += Timer_Tick;
            timer.Start();
            LoadData();
            CalendarLoad();
            LoadSeans(DateTime.Now.Date);
            LoadImage();
            LoadActorCard();
        }

        public class Seans
        {
            public int Id { get; set; }
            public string Time { get; set; }
            public string Place { get; set; }
            public string Hall { get; set; }
            public string Price { get; set; }
        }

        private void LoadImage()
        {
            if (currentImageIndex < imagePaths.Count)
            {
                string FullPath = AppDomain.CurrentDomain.BaseDirectory;
                FullPath = FullPath.Substring(0, FullPath.Length - 10);
                BitmapImage image = new BitmapImage(new Uri(FullPath + imagePaths[currentImageIndex]));
                imageControl.Source = image;               
            }
        }

        private string FormatTime(TimeSpan Time)
        {
            string formattedHours = Time.Hours == 1 ? "час" : "часов";
            string formattedMinutes = Time.Minutes == 1 ? "минута" : "минут";

            string formattedTime = $"{Time.Hours} {formattedHours} {Time.Minutes} {formattedMinutes}";

            return formattedTime;
        }

        private void LoadData()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                var curPer = context.Performance.Where(p => p.ID == idSelectMovie).FirstOrDefault();
                MovieTitle.Text = curPer.Name;
                Desc.Text = curPer.Description;
                imagePaths.Add(curPer.Img);
                imagePaths.AddRange(curPer.Script.Select(p => p.Img));
                TBDuration.Text = FormatTime(curPer.Duration);
                TBSeason.Text = curPer.Seasons.Name;
                TBAge.Text = curPer.Age.Name;
                TBGenres.Text = string.Join(", ", curPer.Genres.ToList().Select(p => p.Name));
            }
        }

        private void CalendarLoad()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                var curSes = context.Sessions.Where(p => p.PerformanceID == idSelectMovie);
                if (curSes.Any())
                {
                    textBlockNoSessions.Visibility = Visibility.Collapsed;
                    DateTime earliestDate = curSes.Min(session => session.DateBegin);
                    DateTime latestDate = curSes.Max(session => session.DateBegin);
                    CalendarDateRange allowedDates = new CalendarDateRange(
                        earliestDate,
                        latestDate
                    );
                    calendar.DisplayDateStart = earliestDate;
                    calendar.DisplayDateEnd = latestDate;
                    calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
                }
                else
                {
                    calendar.IsEnabled = false;
                    textBlockNoSessions.Visibility = Visibility.Visible;
                }

            }
                
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isAnimating)
            {
                currentImageIndex = (currentImageIndex + 1) % imagePaths.Count;
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
                currentImageIndex = currentImageIndex % imagePaths.Count;
                Console.WriteLine(currentImageIndex);
                Console.WriteLine(imagePaths.Count);
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
                var sessionsOnSelectedDate = context.Sessions.Where(session => session.PerformanceID == idSelectMovie && EntityFunctions.TruncateTime(session.DateBegin) == selectedDateWithoutTime).ToList();
                List<Seans> dataForGrid = new List<Seans>();
                foreach (var item in sessionsOnSelectedDate)
                {

                    int price = (int)context.Place
                            .Where(place => !context.Tickets.Any(ticket => ticket.PlaceID == place.ID))
                            .OrderBy(place => place.Sectors.PriceCategory.Price)
                            .Select(place => place.Sectors.PriceCategory.Price)
                            .FirstOrDefault();
                    dataForGrid.Add(new Seans
                    {
                        Id = item.ID,
                        Time = item.DateBegin.ToString("t"),
                        Place = $"{item.Halls.Place.Count() - item.Tickets.Count()}/{item.Halls.Place.Count()}",
                        Hall = item.Halls.Name,
                        Price = price != null ? Convert.ToString(price) : "-",
                    });              
                }
                SeansGrid.ItemsSource= dataForGrid;    
            }            
        }


        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {

            if (selectedSeansId is null)
            {
                MessageBox.Show("Выберите сеанс перед покупкой билета.");
            }
            else
            {
                MainWindow.idSelectSeans = (int)selectedSeansId;
                Window parentWindow = Window.GetWindow(this);
                Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
                frame.Navigate(new SelectTicket());
            }
        }

        private void LoadActorCard()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                var dataa = context.Performance.Where(p => p.ID == idSelectMovie).FirstOrDefault().Sessions.FirstOrDefault();
                if (dataa == null)
                {
                    return;
                }
                var curPer = dataa.Personal.Where(p => p.JobID == 1 || p.JobID == 2).OrderBy(p => p.JobID).ToList();
                string FullPath = AppDomain.CurrentDomain.BaseDirectory;
                FullPath = FullPath.Substring(0, FullPath.Length - 10);
                foreach (var actor in curPer)
                {

                    Border borderActor = new Border();
                    if (actor.JobID == 1)
                    {
                        borderActor.BorderBrush = Brushes.Gray;
                        borderActor.BorderThickness = new Thickness(0, 0, 2, 0);
                    }

                    // Создайте контейнер для карточки актера
                    StackPanel actorCard = new StackPanel {
                        Width = 150,
                        Margin = new Thickness(20, 5, 20, 5),
                    };

                    // Создайте изображение актера
                    Border actorImage = new Border
                    {
                        Background = new ImageBrush
                        {
                            ImageSource = new BitmapImage(new Uri((FullPath + actor.Img))),
                            Stretch = Stretch.UniformToFill
                        },
                        Width = 150,
                        Height = 150,
                    };
                    actorImage.CornerRadius = new CornerRadius(actorImage.Width / 2);

                    // Создайте текстовые блоки для имени актера и роли
                    TextBlock actorNameText = new TextBlock
                    {
                        Margin = new Thickness(0, 10, 0, 0),
                        Text = (actor.Fullname).Replace(" ", "\n"),
                        TextAlignment = TextAlignment.Center,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16
                    };

                    // Добавьте элементы в карточку актера
                    actorCard.Children.Add(actorImage);
                    actorCard.Children.Add(actorNameText);
                    borderActor.Child = actorCard;
                    // Добавьте карточку актера в WrapPanel
                    actorWrapPanel.Children.Add(borderActor);
                }            
            }               
        }

        private void SeansGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (SeansGrid.SelectedItem != null)
            {
                Seans selectedSeans = SeansGrid.SelectedItem as Seans;

                if (selectedSeans != null)
                {
                    selectedSeansId = selectedSeans.Id;
                }
            }
        }
    }
}
