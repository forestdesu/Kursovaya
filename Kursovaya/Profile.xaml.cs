﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        int idUser = MainWindow.sessionUser.ID;
        Users UserData;
        bool StatusTextChanged = false;
        public Profile()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                UserData = context.Users.Where(p => p.ID == idUser).FirstOrDefault();
                this.DataContext = UserData;
            }
        }

        private void BTNSave(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(userLogin.Text);
            Console.WriteLine(UserData.Login);
            Console.WriteLine(userLogin.Text != UserData.Login);
            if (StatusTextChanged)
            {
                UserData.Login = userLogin.Text;
                UserData.Password = userPassword.Text;
                UserData.Fullname = userFullName.Text;
                UserData.Phone = userPhone.Text;
                UserData.Email = userEmail.Text;

                using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
                {
                    try
                    {
                        context.Entry(UserData).State = EntityState.Modified;
                        context.SaveChanges();
                        MessageBox.Show("Данные успешно обновлены!");
                    } catch
                    {
                        MessageBox.Show("Такой логин уже есть!");
                    }                   
                }
            }    
            else
            {
                MessageBox.Show("Измените хотя бы одно поле!");
            }
            StatusTextChanged = false;
        }

        private void BTNDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить свой аккаунт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
                {
                    var userTickets = context.Tickets.Where(p => p.UserID == idUser).ToList();
                    if (userTickets.Any())
                    {
                        context.Tickets.RemoveRange(userTickets);
                    }
                    context.Users.Remove(context.Users.FirstOrDefault(u => u.ID == idUser));
                    context.SaveChanges();
                }
                MainWindow.sessionUser = null;
                Authorization authorization = new Authorization();
                authorization.Show();

                Window parentWindow = Window.GetWindow(this);

                if (parentWindow != null)
                {
                    // Закрыть родительское окно
                    parentWindow.Close();
                }
            }          
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            StatusTextChanged = true;
        }
    }
}
