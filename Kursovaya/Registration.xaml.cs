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

namespace Kursovaya
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Page
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<TextBox> Fields = new List<TextBox> { userFullName, userLogin, userPassword, userEmail, userPhone };
            foreach (TextBox item in Fields) {
                if (item.Text == null) {
                    WarningText.Text = "Пожалуйста заполните все поля";
                    return;
                }
            }

            // - privet 
            // - poka


            using (DramaTheaterTestEntities context = new DramaTheaterTestEntities())
            {
                Users newUser = new Users
                {
                    Fullname = Fields[0].Text,
                    Login = Fields[1].Text,
                    Password = Fields[2].Text,
                    Email = Fields[3].Text,
                    Phone = Fields[4].Text,
                    TypeOfUserID = 1
                };
                try
                {
                    context.Users.Add(newUser);
                    context.SaveChanges();
                } catch
                {
                    WarningText.Text = "Такой логин уже существует";
                    return;
                }
                LoginSuccess(newUser);
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
    }
}
