using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using Microsoft.AspNetCore.Components;
using static System.Collections.Specialized.BitVector32;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для SelectTicket.xaml
    /// </summary>
    public partial class SelectTicket : Page
    {
        int countTickets = 0;
        int allPrice = 0;
        int idSession = MainWindow.idSelectSeans;
        List<int> idReservedPlaces = new List<int>(); 
        private List<Grid> gridList = new List<Grid>();
        private List<Grid> CountList1 = new List<Grid>();
        private List<List<Grid>> CountList2 = new List<List<Grid>>();
        public SelectTicket()
        {
            InitializeComponent();
            gridList.AddRange(new List<Grid> { Parter1, Parter3, Amfitheater1, Amfitheater3, Amfitheater5, Mezzaine1, Mezzaine2, Mezzaine4, Mezzaine6, Mezzaine7, Balcony1, Balcony2, Balcony4, Balcony6, Balcony7 });
            CountList1.AddRange(new List<Grid> { Parter1, Amfitheater3, Mezzaine4, Balcony4 });
            CountList2.AddRange(new List<List<Grid>> { new List<Grid> { Parter2 }, new List<Grid> { Amfitheater2, Amfitheater4 }, new List<Grid> { Mezzaine3, Mezzaine5 }, new List<Grid> { Balcony3, Balcony5 } });
            //CreatePlace();
            //MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            CreateRowsAndColumns();
            LoadSeatLayout();
            

        }

        private List<int> RM(List<int> list)
        {
            List<int> myArray = new List<int>{ };
            myArray = new List<int> (list);
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
        private void CreateRowsAndColumns()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                List<Place> SubGroups = context.Place.ToList();
                var maxValuesBySubGroup = SubGroups
                    .GroupBy(s => s.SectorID)
                    .Select(g => new
                    {
                        Subgroup = g.Key,
                        MaxRow = g.Max(s => s.Row),
                        MaxColumn = g.Max(s => s.Column)
                    })
                    .ToList();
                foreach (var item in maxValuesBySubGroup)
                {
                    var PriceCat = context.Sectors.ToList();
                    var result = PriceCat.Where(p => p.ID == item.Subgroup).First();
                    for (int row = 0; row < item.MaxRow; row++)
                    {
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(25);
                        gridList[item.Subgroup - 1].RowDefinitions.Add(rowDef);
                    }
                    for (int col = 0; col < item.MaxColumn; col++)
                    {
                        ColumnDefinition colDef = new ColumnDefinition();
                        colDef.Width = new GridLength(25);
                        gridList[item.Subgroup - 1].ColumnDefinitions.Add(colDef);
                    }
                }

                for (int j = 0; j < CountList1.Count; j++)
                {

                    foreach (var item3 in CountList2[j])
                    {
                        //Console.WriteLine(CountList1[j].RowDefinitions.Count);
                        for (int i = 0; i < CountList1[j].RowDefinitions.Count; i++)
                        {
                            RowDefinition rowDef = new RowDefinition();
                            rowDef.Height = new GridLength(25);
                            item3.RowDefinitions.Add(rowDef);
                            TextBlock numblock = new TextBlock
                            {
                                Text = Convert.ToString(i + 1),
                                Width = 25,
                                Height = 25,
                                TextAlignment = TextAlignment.Center,
                                FontSize = 18
                            };
                            item3.Children.Add(numblock);
                            Grid.SetRow(numblock, i);
                        }
                    }
                }
            }
        }

        private void LoadSeatLayout()
        {
            foreach (var item in gridList)
            {
                item.Children.Clear();
            }
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {

                List<Sessions> sessions = context.Sessions.ToList();
                Sessions session = sessions.Where(p => p.ID == idSession).FirstOrDefault();
                TitleMovie.Text = Convert.ToString(session.Performance.Name);

                var query = context.Place.Where(p => p.HallsID == session.HallsID);
                var results = query.ToList();
                var reservedSeats = context.Tickets.Where(p => p.SessionsID == idSession).Select(p => p.Place.ID).ToList();
                //var results = CreatePlace();

                List<List<int>> colorPlac = new List<List<int>>
{
                    new List<int> {93, 193, 227},
                    new List<int> {165, 204, 67},
                    new List<int> {255, 89, 89},
                    new List<int> {255, 170, 0}
                };

                foreach (var item in results)
                {
                    Button seatButton = new Button();
                    seatButton.Style = (Style)FindResource("RoundedButtonStyle");
                    seatButton.Content = $"{item.Column}";
                    seatButton.Tag = $"{item.Row};{item.Column};{Convert.ToInt32(item.Sectors.PriceCategory.Price)};{item.SectorID};{item.ID}";
                    if (reservedSeats.Contains(item.ID))
                    {                       
                        seatButton.Background = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    }
                    else
                    {
                        seatButton.Background = new SolidColorBrush(Color.FromRgb((byte)colorPlac[item.Sectors.PriceCategoryID - 1][0], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][1], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][2]));
                        seatButton.Click += SeatButton_Click;
                    }
                    
                    gridList[item.SectorID - 1].Children.Add(seatButton);
                    var CountRowColumn = context.Place.Where(p => p.SectorID == item.SectorID).Where(p => p.Row == item.Row).Count();
                    Grid.SetColumn(seatButton, item.Column - 1);

                    switch (item.Sectors.Position)
                    {
                        case 0:                           
                            Grid.SetColumn(seatButton, item.Column - 1);
                            break;
                        case 1:
                            int coll1 = (gridList[item.SectorID - 1].ColumnDefinitions.Count - CountRowColumn) / 2 + item.Column - 1;
                            Grid.SetColumn(seatButton, coll1);
                            break;
                        case 2:
                            int coll2 = (gridList[item.SectorID - 1].ColumnDefinitions.Count - CountRowColumn) + item.Column - 1;
                            Grid.SetColumn(seatButton, coll2);                            
                            break;
                    }
                    Grid.SetRow(seatButton, item.Row - 1);

                }
            }
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            // Обработка клика на месте
            Button clickedSeat = (Button)sender;
            if (clickedSeat.Resources["ButtonBorderBrush"] != Brushes.Red && countTickets != 4)
            {
                clickedSeat.Resources["ButtonBorderBrush"] = Brushes.Red;
                CreateTicketsEl(clickedSeat.Tag.ToString(), clickedSeat.Background);
            }

        }

        private void CreateTicketsEl(string seatInfo, Brush color)
        {
            string[] seatData = seatInfo.Split(';');

            StackPanel innerStackPanel = new StackPanel();
            innerStackPanel.Orientation = Orientation.Horizontal;
            innerStackPanel.VerticalAlignment = VerticalAlignment.Center;
            innerStackPanel.Margin = new Thickness(15, 10, 15, 10);
            innerStackPanel.Tag = seatInfo;

            Border border = new Border();
            border.Margin = new Thickness(0, 0, 15, 0);
            border.Background = color;
            border.CornerRadius = new CornerRadius(10);


            // Создаем первый StackPanel
            StackPanel stackPanel1 = new StackPanel();
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = $"Ряд {seatData[0]}, Место {seatData[1]}";
            TextBlock textBlock2 = new TextBlock();
            textBlock2.Text = $"{seatData[2]} руб";
            stackPanel1.Children.Add(textBlock1);
            stackPanel1.Children.Add(textBlock2);

            // Создаем второй StackPanel
            StackPanel stackPanel2 = new StackPanel();
            stackPanel2.VerticalAlignment = VerticalAlignment.Center;
            stackPanel2.HorizontalAlignment = HorizontalAlignment.Left;
            stackPanel2.Margin = new Thickness(10, 0, 0, 0);
            Button deleteBtn = new Button();
            deleteBtn.Content = "X";
            deleteBtn.Background = Brushes.Transparent;
            deleteBtn.BorderBrush = Brushes.Transparent;
            deleteBtn.Click += DeleteTicketClick;
            stackPanel2.Children.Add(deleteBtn);

            // Добавляем stackPanel1 и stackPanel2 в innerStackPanel
            innerStackPanel.Children.Add(stackPanel1);
            innerStackPanel.Children.Add(stackPanel2);


            border.Child = innerStackPanel;
            TicketsGrid.Children.Add(border);

            // Добавляем mainStackPanel в окно
            countTickets += 1;
            // int number = 0;

            int price = Convert.ToInt32(seatData[2]);
            //int.TryParse(seatData[2], out number);
            allPrice += price;
            ChangePrice();

            idReservedPlaces.Add(Convert.ToInt32(seatData[4]));

        }

        private void ChangePrice()
        {
            PriceBtnText.Text = $"Далее: {allPrice} руб";
        }

        private void DeleteTicketClick(object sender, RoutedEventArgs e)
        {
            UIElement elementToRemove = (UIElement)sender; // Замените этот код на получение вашего элемента

            DependencyObject parent = VisualTreeHelper.GetParent(elementToRemove);
            StackPanel ticketPanel = (StackPanel)VisualTreeHelper.GetParent(parent);
            Border ticketPanelBorder = (Border)VisualTreeHelper.GetParent(ticketPanel);
            
            string ticketTag = (string)ticketPanel.Tag;
            TicketsGrid.Children.Remove(ticketPanelBorder);
            countTickets -= 1;
            string[] tagInfo = ((string)(ticketTag)).Split(';');

            int price = Convert.ToInt32(tagInfo[2]);
            //int price = (int)(((string)TicketsGrid.Tag).Split(';')[2]);
            allPrice -= price;
            ChangePrice();

            Grid gridElem = gridList[Convert.ToInt32(tagInfo[3]) - 1];
            Console.WriteLine(tagInfo[3]);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(gridElem); i++) {
            
                Button btnPlace = (Button)VisualTreeHelper.GetChild(gridElem, i);

                if (btnPlace.Tag == ticketTag)
                {
                    btnPlace.Resources["ButtonBorderBrush"] = Brushes.Transparent;
                    break;
                }
            }

            idReservedPlaces.Remove(Convert.ToInt32(tagInfo[4]));
        }

        private void BuyTickets(object sender, RoutedEventArgs e)
        {

            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                foreach (var id_rp in idReservedPlaces)
                {
                    Sessions curSes = context.Sessions.Where(p => p.ID == idSession).FirstOrDefault();
                    int totalPrice = Convert.ToInt32(curSes.PriceTime.Price + curSes.Performance.Price + context.Place.Where(p => p.ID == id_rp).FirstOrDefault().Sectors.PriceCategory.Price);

                    Tickets newTicket = new Tickets
                    {
                        SessionsID = idSession,
                        UserID = MainWindow.sessionUser.ID,
                        PlaceID = id_rp,
                        Date = DateTime.Now,
                        TotalPrice = totalPrice
                    };
                    context.Tickets.Add(newTicket);

                }

                context.SaveChanges();

            }

            Window parentWindow = Window.GetWindow(this);

            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            frame.Navigate(new SelectMovie());
            MainWindow.previusPage = true;
        }
    }
}
