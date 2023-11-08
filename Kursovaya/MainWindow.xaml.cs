using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Globalization;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static int idSelectMovie { get; set; }
        public static int idSelectSeans { get; set; }
        public static Users sessionUser { get; set; }
        public static bool previusPage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            previusPage = false;
            // // Вызываем метод для создания мест в зале
            MainFrame.Navigate(new SelectMovie());
            Manager.MainFrame = MainFrame;
            idSelectMovie = 2;
        }

        private void Movies(object sender, RoutedEventArgs e)
        {
            
            Manager.MainFrame.Navigate(new SelectMovie());
        }
        private void Profile(object sender, RoutedEventArgs e)
        {
            previusPage = false;
            Manager.MainFrame.Navigate(new Profile());
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine($"ВОТ ТУТ: {Manager.MainFrame.Content is SelectTicket} - {Manager.MainFrame.Content.ToString()}");

            if (!previusPage && !(Manager.MainFrame.Content.GetType() == typeof(SelectMovie)))
            {
                if (Manager.MainFrame.CanGoBack)
                {
                    Manager.MainFrame.GoBack();
                }                
            }
            /*
            if (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.GoBack();
                Console.WriteLine(Manager.MainFrame.Content.GetType());
                Console.WriteLine(Manager.MainFrame.Content.GetType() == typeof(SelectTicket));
                if (Manager.MainFrame.Content.GetType() == typeof(SelectTicket))
                {
                    Manager.MainFrame.GoBack();
                }
            }*/
            /*
            if (!(previusPage == false))
            {
                previusPage = Manager.MainFrame.CanGoBack;
                if (Manager.MainFrame.CanGoBack)
                {
                    Manager.MainFrame.GoBack();
                }
            }
            if (Manager.MainFrame.Content is SelectTicket)
            {
                previusPage = false;
            }*/
        }

        private void Tickets(object sender, RoutedEventArgs e)
        {
            previusPage = false;
            Manager.MainFrame.Navigate(new MyTickets());
        }
    }
}
