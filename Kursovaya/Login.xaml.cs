﻿using System;
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
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Показать анимацию загрузки
            loadingControl.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() =>
                {
                    using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {

                            var DataUser = context.Users.Where(p => p.Login == userLogin.Text && p.Password == userPassword.Text).FirstOrDefault();

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
                loadingControl.Visibility = Visibility.Collapsed;
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
    }
}
