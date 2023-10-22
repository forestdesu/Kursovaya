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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing;

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для MyTickets.xaml
    /// </summary>
    public partial class MyTickets : Page
    {
        int idMovie;
        private List<Grid> gridList = new List<Grid>();
        private List<Grid> CountList1 = new List<Grid>();
        private List<List<Grid>> CountList2 = new List<List<Grid>>();
        public MyTickets()
        {
            InitializeComponent();
            gridList.AddRange(new List<Grid> { Parter1, Parter3, Amfitheater1, Amfitheater3, Amfitheater5, Mezzaine1, Mezzaine2, Mezzaine4, Mezzaine6, Mezzaine7, Balcony1, Balcony2, Balcony4, Balcony6, Balcony7 });
            CountList1.AddRange(new List<Grid> { Parter1, Amfitheater3, Mezzaine4, Balcony4 });
            CountList2.AddRange(new List<List<Grid>> { new List<Grid> { Parter2 }, new List<Grid> { Amfitheater2, Amfitheater4 }, new List<Grid> { Mezzaine3, Mezzaine5 }, new List<Grid> { Balcony3, Balcony5 } });
            GenerateBarcodeImage();
        }

        private void CreateRowsAndColumns()
        {
            foreach (var grid in gridList)
            {
                grid.ClearValue(Grid.ColumnProperty);
                grid.ClearValue(Grid.RowProperty);
            }
            foreach (var grid in CountList1)
            {
                grid.ClearValue(Grid.ColumnProperty);
                grid.ClearValue(Grid.RowProperty);
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
                    var PriceCat = context.Sectors.ToList();
                    var result = PriceCat.Where(p => p.ID == item.Subgroup).First();
                    for (int row = 0; row < item.MaxRow; row++)
                    {
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Height = new GridLength(20);
                        gridList[item.Subgroup - 1].RowDefinitions.Add(rowDef);
                    }
                    for (int col = 0; col < item.MaxColumn; col++)
                    {
                        ColumnDefinition colDef = new ColumnDefinition();
                        colDef.Width = new GridLength(20);
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
                            rowDef.Height = new GridLength(20);
                            item3.RowDefinitions.Add(rowDef);
                            TextBlock numblock = new TextBlock
                            {
                                Text = Convert.ToString(i + 1),
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            item3.Children.Add(numblock);
                            Grid.SetRow(numblock, i);
                        }
                    }
                }
            }
        }
        private void ViewPlace(object sender, MouseButtonEventArgs e)
        {
            idMovie = 1;
            CreateRowsAndColumns();
            LoadSeatLayout();
        }

        private void LoadSeatLayout()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {

                List<Sessions> sessions = context.Sessions.ToList();
                Sessions session = sessions.Where(p => p.Script.FirstOrDefault().ID == idMovie).FirstOrDefault();

                var query = context.Place.Where(p => p.HallsID == session.HallsID);
                var results = query.ToList();
                var reservedSeats = context.Tickets.Select(p => p.Place.ID).ToList();
                //var results = CreatePlace();

                List<List<int>> colorPlac = new List<List<int>>
{
                    new List<int> {198, 65, 144},
                    new List<int> {133, 68, 152},
                    new List<int> {26, 110, 193},
                    new List<int> {74, 178, 169 }
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
                            Grid.SetColumn(seatButton, coll2);
                            break;
                    }
                    Grid.SetRow(seatButton, item.Row);

                }
            }
        }

        private void GenerateBarcodeImage()
        {
            var ticketNumber = "00000001"; // Замените на свой номер билета
            var barcodeBitmap = GenerateBarcodeBitmap(ticketNumber);

            if (barcodeBitmap != null)
            {
                barcodeImage.Source = ConvertBitmapToImageSource(barcodeBitmap);
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
    }
}
