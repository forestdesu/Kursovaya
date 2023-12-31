﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using ZXing;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для MyTickets.xaml
    /// </summary>
    public partial class MyTickets : Page
    {
        int idMovie;
        int idUser;
        private List<Grid> gridList = new List<Grid>();
        private List<Grid> CountList1 = new List<Grid>();
        private List<List<Grid>> CountList2 = new List<List<Grid>>();
        private bool isDragging = false;
        private Point offset;
        public MyTickets()
        {
            InitializeComponent();

            gridList.AddRange(new List<Grid> { Parter1, Parter3, Amfitheater1, Amfitheater3, Amfitheater5, Mezzaine1, Mezzaine2, Mezzaine4, Mezzaine6, Mezzaine7, Balcony1, Balcony2, Balcony4, Balcony6, Balcony7 });
            CountList1.AddRange(new List<Grid> { Parter1, Amfitheater3, Mezzaine4, Balcony4 });
            CountList2.AddRange(new List<List<Grid>> { new List<Grid> { Parter2 }, new List<Grid> { Amfitheater2, Amfitheater4 }, new List<Grid> { Mezzaine3, Mezzaine5 }, new List<Grid> { Balcony3, Balcony5 } });

            idUser = MainWindow.sessionUser.ID;
            GenerateTickets();
        }

        private void SetCenterCanvasGrid()
        {
            
            double left = (myCanvas.ActualWidth - CanvasGrid.ActualWidth) / 2;
            Canvas.SetLeft(CanvasGrid, left);

            double top = (myCanvas.ActualHeight - CanvasGrid.ActualHeight) / 2;
            Canvas.SetTop(CanvasGrid, top);
        }

        private void GenerateTickets()
        {
            CreateRowsAndColumns();
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                var myTickets = context.Tickets
                    .Where(p => p.UserID == idUser);
                //Для логически правильной работы нужно
                //было бы использовать этот запрос, но
                //тогда придется каждый раз обновлять данные
                //поэтому обойдемся этим
                //var myTickets = context.Tickets
                //    .Where(p => p.UserID == idUser &&
                //    p.Sessions.DateEnd >= DateTime.Now);

                foreach (var item in myTickets)
                {
                    Grid mainGrid = new Grid
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(20, 15, 20, 15),
                        MaxWidth = 600
                    };

                    // Создаем сетку для контента
                    Grid contentGrid = new Grid
                    {
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x11, 0x76)),
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
                    contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Star) });
                    contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25, GridUnitType.Star) });
                    contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Star) });
                    contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Star) });

                    // Создаем StackPanel для текста
                    StackPanel textStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10, 5, 10, 5)
                    };

                    TextBlock titleText = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 30,
                        Text = item.Sessions.Performance.Name,
                        FontWeight = FontWeights.Bold,
                        TextWrapping= TextWrapping.Wrap,
                    };
                    textStackPanel.Children.Add(titleText);

                    TextBlock priceText = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0x37, 0x87)),
                        FontSize = 15,
                        Text = Convert.ToString(Convert.ToInt32(item.TotalPrice)) + " RUB",
                        Margin = new Thickness(0, 0, 10, 10),
                        FontWeight = FontWeights.Bold
                    };
                    textStackPanel.Children.Add(priceText);

                    TextBlock dateText = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 22,
                        Text = item.Sessions.DateBegin.ToString("dd MMM HH:mm"),
                        FontWeight = FontWeights.Bold
                    };
                    textStackPanel.Children.Add(dateText);

                    TextBlock locationText = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x53, 0x9D, 0xC0)),
                        FontSize = 12,
                        Text = item.Place.Halls.Name,
                        FontWeight = FontWeights.Bold
                    };
                    textStackPanel.Children.Add(locationText);

                    // Создаем сетку для информации о билете
                    Grid ticketInfoGrid = new Grid
                    {
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    for (int i = 0; i < 7; i += 2)
                    {
                        ticketInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        ticketInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
                    }

                    string[] labels = { "SECTOR", item.Place.Sectors.Name.Remove(item.Place.Sectors.Name.Length - 2), "GROUP", item.Place.Sectors.Name.Substring(item.Place.Sectors.Name.Length - 1), "ROW", Convert.ToString(item.Place.Row), "SEAT", Convert.ToString(item.Place.Column) };

                    for (int i = 1; i < labels.Length; i += 2)
                    {
                        StackPanel labelStackPanel = new StackPanel();
                        TextBlock label1 = new TextBlock
                        {
                            TextAlignment = TextAlignment.Center,
                            FontSize = i - 1 % 2 == 0 ? 13 : 12,
                            Foreground = (i - 1) % 2 == 0 ? new SolidColorBrush(Color.FromArgb(0xFF, 0x7F, 0x26, 0xAE)) : Brushes.White,
                            Text = labels[i - 1],
                            FontWeight = FontWeights.Bold
                        };
                        TextBlock label2 = new TextBlock
                        {
                            TextAlignment = TextAlignment.Center,
                            FontSize = i % 2 == 0 ? 13 : 12,
                            Foreground = i % 2 == 0 ? new SolidColorBrush(Color.FromArgb(0xFF, 0x7F, 0x26, 0xAE)) : Brushes.White,
                            Text = labels[i],
                            FontWeight = FontWeights.Bold
                        };
                        labelStackPanel.Children.Add(label1);
                        labelStackPanel.Children.Add(label2);
                        labelStackPanel.SetValue(Grid.ColumnProperty, i - 1);
                        ticketInfoGrid.Children.Add(labelStackPanel);
                    }

                    textStackPanel.Children.Add(ticketInfoGrid);
                    // Создаем изображение штрих-кода
                    Image barcodeImage = new Image
                    {
                        Name = $"barcodeImage_{item.ID}",
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    RotateTransform rotateTransform = new RotateTransform(90);
                    barcodeImage.LayoutTransform = rotateTransform;
                    GenerateBarcodeImage(barcodeImage, Convert.ToString(item.ID));

                    Border imageBorder = new Border
                    {
                        Margin = new Thickness(20, 10, 20, 10),
                        Background = Brushes.White
                    };

                    imageBorder.Child = barcodeImage;

                    Grid.SetColumn(textStackPanel, 1);
                    Grid.SetColumn(imageBorder, 3);

                    // Добавляем элементы в contentGrid
                    contentGrid.Children.Add(textStackPanel);
                    contentGrid.Children.Add(imageBorder);
                    mainGrid.Children.Add(contentGrid);
                    // Создаем круги и прямоугольники
                    SolidColorBrush mainColor = (SolidColorBrush)FindResource("BackgroundColour");
                    

                    Ellipse BigEl1 = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Fill = mainColor,
                        Margin = new Thickness(0, -15, 110, 0),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    Ellipse BigEl2 = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Fill = mainColor,
                        Margin = new Thickness(0, 0, 110, -15),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom
                    };

                    mainGrid.Children.Add(BigEl1);
                    mainGrid.Children.Add(BigEl2);
                    mainGrid.MouseLeftButtonUp += (sender, e) => ViewPlace(item.SessionsID, item.Place);

                    mainGrid.Loaded += (sender, e) =>
                    {
                        double marginCircle = mainGrid.ActualHeight / 5 - 4.2;
                        for (int i = 0; i < 6; i++)
                        {
                            Ellipse ellipseLeft = new Ellipse
                            {
                                Width = 20,
                                Height = 20,
                                Fill = mainColor,
                                Margin = new Thickness(0, i * marginCircle, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top
                            };

                            Ellipse ellipseRight = new Ellipse
                            {
                                Width = 20,
                                Height = 20,
                                Fill = mainColor,
                                Margin = new Thickness(0, i * marginCircle, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Top
                            };

                            mainGrid.Children.Add(ellipseLeft);
                            mainGrid.Children.Add(ellipseRight);
                        }

                        for (int i = 0; i < ((mainGrid.ActualHeight - 50) / 15 + 1); i++)
                        {
                            Rectangle rectangle = new Rectangle
                            {
                                Width = 5,
                                Height = 10,
                                Fill = mainColor,
                                Margin = new Thickness(0, 0, 134, 35 + i * 15),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom
                            };

                            Grid.SetColumn(rectangle, 3);

                            mainGrid.Children.Add(rectangle);
                        }
                    };
                    
                    
                    
                    Tickets.Children.Add(mainGrid);

                }
            }
        }

        private void CreateRowsAndColumns()
        {
            foreach (var grid in gridList)
            {
                grid.Children.Clear();
                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();

            }
            foreach (var gridList in CountList2)
            {
                foreach (var grid in gridList)
                {
                    grid.Children.Clear();
                    grid.ColumnDefinitions.Clear();
                    grid.RowDefinitions.Clear();
                }
            }
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
                    var PriceCategory = context.Sectors.ToList();
                    var result = PriceCategory
                        .Where(p => p.ID == item.Subgroup)
                        .First();

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
                    Console.WriteLine($"{gridList[item.Subgroup - 1].GetValue(FrameworkElement.NameProperty)} - {gridList[item.Subgroup - 1].RowDefinitions.Count} - {gridList[item.Subgroup - 1].ColumnDefinitions.Count}");

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
                                Width = 24,
                                Height = 24,
                                TextAlignment= TextAlignment.Center,
                                FontSize = 18
                            };
                            item3.Children.Add(numblock);
                            Grid.SetRow(numblock, i);
                        }
                    }
                }
            }
        }
        private void ViewPlace(int SessionID, Place place)
        {
            idMovie = SessionID;
            CreateRowsAndColumns();
            LoadSeatLayout(idMovie, place);
            SetCenterCanvasGrid();
        }

        private void LoadSeatLayout(int idMovie, Place place)
        {
            
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                Sessions session = context.Sessions
                    .Where(p => p.ID == idMovie)
                    .FirstOrDefault();
                var results = context.Place.ToList();

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
                    seatButton.IsEnabled= false ;
                    //seatButton.Tag = $"{item.Row};{item.Column};{Convert.ToInt32(item.Sectors.PriceCategory.Price)};{item.SectorID};{item.ID}";
                    if (place.ID == item.ID)
                    {
                        seatButton.Foreground = new SolidColorBrush(Color.FromRgb((byte)colorPlac[item.Sectors.PriceCategoryID - 1][0], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][1], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][2])); ;
                        seatButton.Content = "✔";
                        seatButton.Background = Brushes.Transparent;
                        seatButton.Resources["ButtonBorderBrush"] = new SolidColorBrush(Color.FromRgb((byte)colorPlac[item.Sectors.PriceCategoryID - 1][0], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][1], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][2]));
                    }
                    else
                    {
                        seatButton.Content = $"{item.Column}";
                        seatButton.Background = new SolidColorBrush(Color.FromRgb((byte)colorPlac[item.Sectors.PriceCategoryID - 1][0], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][1], (byte)colorPlac[item.Sectors.PriceCategoryID - 1][2]));
                    }
                    
                    gridList[item.SectorID - 1].Children.Add(seatButton);

                    var CountRowColumn = context.Place.Where(p => p.SectorID == item.SectorID).Where(p => p.Row == item.Row).Count();
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
                            Console.WriteLine($"({gridList[item.SectorID - 1].ColumnDefinitions.Count} - {CountRowColumn}) + {item.Column} - 1)");
                            Grid.SetColumn(seatButton, coll2);
                            break;
                    }
                    Grid.SetRow(seatButton, item.Row - 1);
                }
            }
        }

        private void GenerateBarcodeImage(Image item, string ID)
        {
            var barcodeBitmap = GenerateBarcodeBitmap(ID);

            if (barcodeBitmap != null)
            {
                item.Source = ConvertBitmapToImageSource(barcodeBitmap);
            }
        }

        private BitmapSource ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap); // Освободим ресурсы

            return source;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Drawing.Bitmap GenerateBarcodeBitmap(string content)
        {
            var barcode = new ZXing.BarcodeWriter();
            barcode.Format = ZXing.BarcodeFormat.CODE_128;
            barcode.Options = new ZXing.Common.EncodingOptions
            {
                Width = 200, // Устанавливает ширину изображения
                Height = 50, // Устанавливает высоту изображения
                Margin = 0, // Устанавливает поля вокруг штрих-кода
            };
            barcode.Options.PureBarcode = true;
            barcode.Options.NoPadding = true;
            try
            {
                var bitmap = barcode.Write(content);
                return bitmap;
            }
            catch (Exception ex)
            {
                // Обработайте исключение, если что-то пошло не так
                MessageBox.Show("Ошибка при создании штрих-кода: " + ex.Message);
                return null;
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // Увеличение масштаба Canvas
            myCanvas.LayoutTransform = new ScaleTransform(myCanvas.LayoutTransform.Value.M11 * 1.1, myCanvas.LayoutTransform.Value.M22 * 1.1);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            // Уменьшение масштаба Canvas
            myCanvas.LayoutTransform = new ScaleTransform(myCanvas.LayoutTransform.Value.M11 / 1.1, myCanvas.LayoutTransform.Value.M22 / 1.1);

        }
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Начало перетаскивания
            isDragging = true;

            // Сохранение смещения относительно курсора
            offset = e.GetPosition(CanvasGrid);
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Окончание перетаскивания
            isDragging = false;
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Перемещение элемента в соответствии с движением курсора
                Point currentPosition = e.GetPosition(myCanvas);
                Canvas.SetLeft(CanvasGrid, currentPosition.X - offset.X);
                Canvas.SetTop(CanvasGrid, currentPosition.Y - offset.Y);
            }
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            // Курсор покинул область Canvas, прекратить перемещение
            isDragging = false;
        }
    }
}
