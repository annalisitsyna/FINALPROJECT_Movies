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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace List_of_movies
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String connectionString;//сторока подключения к БД
        string sql;//переменная для хранения запроса
        SqlDataAdapter adapter;
        DataSet ds;//переменная для хранения данных полученных из БД
        bool isLoad = false;//флаг, указывающий, что глафический интерфейс загрузился
        public MainWindow()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            InitializeComponent();
            isLoad = true;
        }

        private void Avt_Click(object sender, RoutedEventArgs e)
            //функция входа с авторизацией
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                sql = "SELECT a.Admin_password " +
                "FROM Admins a " +
                "Where a.Admin_login like\'" + TB_login.Text+"\'";// запрос для получения пароля пользователя с набранным паролем 
                adapter = new SqlDataAdapter(sql, connection);
                ds = new DataSet();
                adapter.Fill(ds);//получение данных по запросу
                if(ds.Tables[0].Rows.Count==0)//проверка существует ли такой пользователь
                    LError.Content = "*пароль или логин не верны";//ошибка,если пользователя с таким именем не существует
                else
                    if(String.Compare(TB_password.Password.ToString(),ds.Tables[0].Rows[0].ItemArray[0].ToString(),false)==0)
                        //сравнение паролей
                    {
                        //если пароли совпали, то войти в систему
                        ListFilms lf = new ListFilms(true);
                        lf.Show();
                        this.Close();
                    }
                    else
                        LError.Content = "*пароль или логин не верны";//ошибка,если пароли не совпали
            }
        }

        private void NotAvt_Click(object sender, RoutedEventArgs e)
            //вход в систему без авторизации
        {
            ListFilms lf = new ListFilms(false);
            lf.Show();
            this.Close();
        }

        private void TB_login_TextChanged(object sender, TextChangedEventArgs e)
        {
            //убрать сообщение об ошибке, если польватель начал ввод другого имени пользователя
            if(isLoad)
                LError.Content = "";
        }

        private void TB_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //убрать сообщение об ошибке, если пользователь начал ввод другого пароля
            if(isLoad)
                LError.Content = "";
        }

    }
}
