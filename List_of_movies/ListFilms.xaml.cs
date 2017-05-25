using System;
using System.IO;
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
using System.Windows.Shapes;

using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace List_of_movies
{
    /// <summary>
 
    /// </summary>
    public partial class ListFilms : Window
    {
        //список жанров
        String[] listGenre = new String[] {"Аниме","Боевик","Вестерн","Детектив","Документальный","Драма","Исторический","Комедия","Мелодрама","Мультфильм","Мюзикл","Триллер","Ужасы","Фантастика","Фентези"};
        bool isAdmin,FR=true,isLoad=false,isSeach=false;
        //isAdmin-флаг, показывающий, зашел пользователь под авторизацией или нет
        //FR- запрет на переприменение параметров рейтинга фильтров, если изменение произошло ошибочное изменение рейтинга 
        //isLoad - флаг, сообщающий, что графический интерфейс загрузился
        //isSeach - флаг, отображающий режим вывода записей, false -обычный вывод ,true-вывод по параметрам поиска
        List<int> filtrGenre = new List<int>();
        //список, для хранения номеров выводимых фильтром жанров жильмов
        String connectionString;//строка подключения к БД
        int start = 0,finish=9;//номера значений выводимого фильтром рейтинга

        string sql;//переменная, для хранения запроса к БД
        SqlDataAdapter adapter;
        DataSet ds,ds1;//ds- переменная для хранения списка фильмов в БД, ds1-переменная для хранения списка фильмов , удовлетворяющих параметрам поиска
        public ListFilms(bool mode)
        {
            isAdmin = mode;
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            InitializeComponent();
            if (isAdmin)
                BAdd.Visibility = Visibility.Visible;
            for(int i=1;i<11;i++)//заполнение списка рейтинга
            {
                CBStartF.Items.Add(i);
                CBFinishF.Items.Add(i);
            }
            //установка начальных значений для фильтра рейтинга
            CBStartF.SelectedIndex = 0;
            CBFinishF.SelectedIndex = 9;
            for (int i = listGenre.Count()-1; i >=0; i--)
                //заполнение списка фильтра жанров
            {
                CheckBox ch = new CheckBox();
                ch.IsChecked = true;
                ch.Content=listGenre[i];
                ch.Tag = i;
                ch.Visibility = Visibility.Collapsed;
                ch.HorizontalAlignment=HorizontalAlignment.Left;
                ch.Margin = new Thickness(30,0,0,0);
                filtrGenre.Add(i);
                ch.Checked += CheckBox_Checked;
                ch.Unchecked += CheckBox_Unchecked;
                SPFiltr.Children.Insert(7,ch);
            }
            setDS();
            isLoad = true;
            PrintFilms(ds);//вывод списка фильмов пользователю
        }
        private void setDS()
            //получение из БД списка фильмов
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                sql = "SELECT f.* From Films f";
                adapter = new SqlDataAdapter(sql, connection);
                ds = new DataSet();
                adapter.Fill(ds);
            }
        }
        private void PrintFilms(DataSet ds)//ds-множество в,котором хранятся фильмы, которые надо вывести
            //выод списка фильмов на экран
        {
            if(isLoad)
            {
            WP.Children.Clear();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (IsFiltr(ds.Tables[0].Rows[i]))//проверка подходит ли фильм под фильтры                   
                {
                    //задание контейнера под вывод данных фильма
                    StackPanel SP = new StackPanel();
                    SP.Orientation = Orientation.Vertical;
                    SP.MaxWidth = 140;
                    SP.MinWidth = 140;
                    SP.Tag = i;
                    //задание изображения под вывод постера
                    Image poster = new Image();
                    poster.MaxWidth = 140;
                    poster.MaxHeight = 150;
                    if (ds.Tables[0].Rows[i].ItemArray[5].ToString() != "" )
                    {
                        //преобразование двоичных данных в изображение и вывод в качестве постера
                        String s = ds.Tables[0].Rows[i].ItemArray[5].ToString();
                        byte[] bytes = (byte[])ds.Tables[0].Rows[i].ItemArray[5];
                            BitmapImage bi = new BitmapImage();
                            using (var ms = new MemoryStream(bytes))
                            {
                                bi.BeginInit();
                                bi.CacheOption = BitmapCacheOption.OnLoad;
                                bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                bi.StreamSource = ms;
                                bi.EndInit();
                                bi.Freeze();
                            }
                            poster.Source = bi;
                    }
                    else
                    {
                        //вывод постера, если его нет
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("no_poster.jpg", UriKind.Relative);
                        bi.EndInit();
                        poster.Source = bi;
                    }
                    //задания поля с названием фильма
                    Label lName = new Label();
                    lName.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lName.Padding = new Thickness(0);
                    
                    if(ds.Tables[0].Rows[i].ItemArray[1].ToString().Length>22)
                        //если название слишком длинное, то укоротить его и добавить многоточие
                        lName.Content = ds.Tables[0].Rows[i].ItemArray[1].ToString().Substring(0,20)+"...";
                    else
                        lName.Content = ds.Tables[0].Rows[i].ItemArray[1].ToString();
                    //задание поля с названием жанра фильма
                    Label lGenre = new Label();
                    lGenre.HorizontalContentAlignment = HorizontalAlignment.Center;
                    lGenre.Padding = new Thickness(0);
                    lGenre.Content = listGenre[Convert.ToInt16(ds.Tables[0].Rows[i].ItemArray[3])];
                    //задание панели с рейтингом фильма
                    StackPanel SPRate = new StackPanel();
                    SPRate.Orientation = Orientation.Horizontal;
                    SPRate.MaxWidth = 100;
                    SPRate.MinWidth = 100;
                    for (int j = 0; j < 10; j++)
                        //заполнение данных рейтинга
                    {
                        Image star = new Image();
                        star.MaxWidth = 10;
                        star.MaxHeight = 10;
                        BitmapImage bIm = new BitmapImage();
                        bIm.BeginInit();
                        if (j < Convert.ToInt16(ds.Tables[0].Rows[i].ItemArray[2]))
                            bIm.UriSource = new Uri("rating.png", UriKind.Relative);
                        else
                            bIm.UriSource = new Uri("rnot.png", UriKind.Relative);
                        bIm.EndInit();
                        star.Source = bIm;
                        SPRate.Children.Add(star);
                    }
                    SP.Children.Add(poster);
                    SP.Children.Add(lName);
                    SP.Children.Add(lGenre);
                    SP.Children.Add(SPRate);
                    //добавление рамки вокруг контейнера
                    Border b = new Border();
                    b.BorderBrush = Brushes.Blue;
                    b.BorderThickness = new Thickness(0);
                    b.Margin = new Thickness(5);
                    //добавление событий
                     b.MouseEnter += Border_MouseEnter;
                     b.MouseLeave += Border_MouseLeave;
                     SP.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                    b.Child = SP;
                    WP.Children.Add(b);
                }
            }
            }
        }
        private bool IsFiltr(DataRow dr)//dr-строка данных о фильме
        {
            //проверка фильма на то, что он подходит под параметры фильмов
            if (CBFiltr.IsChecked == false)
                //автоматический вывод, что фильм подходит, если использование фильтров отключено
                return true;
            else
            {
                if((int)dr.ItemArray[2]>=start+1&&(int)dr.ItemArray[2]<=finish+1)
                    //проверка подходит ли рейтинг
                {
                    //проверка жанра
                    if (ChAllGenre.IsChecked == true)
                        //выбрано, что подходят все жанры
                        return true;
                    else
                    {
                        //поиск если жанр фильма в списке выбранных жанров
                        for (int i = 0; i < filtrGenre.Count; i++)
                            if (filtrGenre[i] == (int)dr.ItemArray[3])
                                return true;
                        return false;
                    }
                }
                else
                    return false;
            }
        }
        private void BOpenClose_Click(object sender, RoutedEventArgs e)
        {
            //открытие/ закрытие панели фильтров
            if (BOpenClose.Tag.ToString().CompareTo("1")==0)
            {
                GBFiltr.Visibility = Visibility.Visible;
                BOpenClose.Content = " Закрыть параметры ";
                BOpenClose.Tag = "0";
            }else
            {
                GBFiltr.Visibility = Visibility.Collapsed;
                BOpenClose.Content = " Открыть параметры ";
                BOpenClose.Tag = "1";
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //включение выбора отдельного жанра фильма, какой это жанр фильма определяется по параметру Tag
            CheckBox ch = (CheckBox)sender;
            filtrGenre.Add(Convert.ToInt16(ch.Tag));
            if (CBFiltr.IsChecked == true)
            {
                //вывод фильмов согластно измененному фильтру
                if (isSeach)
                    PrintFilms(ds1);
                else
                    PrintFilms(ds);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //отмена выбора отдельного жанра фильма, какой это жанр фильма определяется по параметру Tag
            CheckBox ch = (CheckBox)sender;
            filtrGenre.Remove(Convert.ToInt16(ch.Tag));
            if (CBFiltr.IsChecked == true)
            {
                //вывод фильмов согластно измененному фильтру
                if (isSeach)
                    PrintFilms(ds1);
                else
                    PrintFilms(ds);
            }
        }

        private void ChAllGenre_Checked(object sender, RoutedEventArgs e)
        {
            //нажатие на отметку, что выбраны должны быть все жанры фильмов
            for(int i=7;i<SPFiltr.Children.Count;i++)
            {
                UIElement UI = (UIElement) SPFiltr.Children[i];
                UI.Visibility = Visibility.Collapsed;
            }
            if (CBFiltr.IsChecked == true)
            {
                //вывод фильмов согластно измененному фильтру
                if (isSeach)
                    PrintFilms(ds1);
                else
                    PrintFilms(ds);
            }
        }

        private void ChAllGenre_Unchecked(object sender, RoutedEventArgs e)
        {
            //отключение выбора всех жанров фильмов
            for (int i = 7; i < SPFiltr.Children.Count; i++)
            {
                UIElement UI = (UIElement)SPFiltr.Children[i];
                UI.Visibility = Visibility.Visible;
            }
            if (CBFiltr.IsChecked==true)
            {
                //вывод фильмов согластно измененному фильтру
                if (isSeach)
                    PrintFilms(ds1);
                else
                    PrintFilms(ds);
            }
        }

        private void CBStartF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //изменение выбора начального необходимого рейтинга в списке параметров фильтров
            if(CBStartF.SelectedIndex>finish)
            {
                //ошибка,если начальный рейтинг больше конечного
                FR = false;
                CBStartF.SelectedIndex = start;
                LErRate.Content = "Ошибка выбора рейтинга";                
            }
            else
            {
                if (FR)
                {
                    
                    LErRate.Content = "";
                    start = CBStartF.SelectedIndex;
                    if (CBFiltr.IsChecked == true)
                    {
                        //вывод фильмов согластно измененному фильтру
                        if (isSeach)
                            PrintFilms(ds1);
                        else
                            PrintFilms(ds);
                    }
                }
                else
                    FR = true;
            }
        }

        private void CBFinishF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //изменение выбора конечного необходимого рейтинга в списке параметров фильтров
            if(CBFinishF.SelectedIndex<start)
            {
                //ошибка,если конечный рейтинг меньше начального
                FR = false;
                CBFinishF.SelectedIndex = finish;
                LErRate.Content = "Ошибка выбора рейтинга";
            }
            else
            {
                if (FR)
                {
                    
                LErRate.Content = "";
                finish = CBFinishF.SelectedIndex;
                if (CBFiltr.IsChecked == true)
                {
                    ////вывод фильмов согластно измененному фильтру
                    if (isSeach)
                        PrintFilms(ds1);
                    else
                        PrintFilms(ds);
                }
                }
                else
                    FR = true;
            }
        }

        private void BAll_Click(object sender, RoutedEventArgs e)
            //шелчек по кнопке выбрать все в списке параметров фильтров
        {
            for(int i=7;i<SPFiltr.Children.Count-2;i++)
            {
                CheckBox cb = (CheckBox)SPFiltr.Children[i];
                if (cb.IsChecked == false)
                    cb.IsChecked = true;
            }
        }

        private void BNothing_Click(object sender, RoutedEventArgs e)
            //шелчек по кнопке снять выделение в списке параметров фильтров
        {
            for (int i = 7; i < SPFiltr.Children.Count - 2; i++)
            {
                CheckBox cb = (CheckBox)SPFiltr.Children[i];
                if (cb.IsChecked == true)
                    cb.IsChecked = false;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
            //появление рамки при наведении на фильм
        {
            Border b = (Border)sender;
            b.BorderThickness = new Thickness(2);
            b.Margin = new Thickness(3);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
            //отключение рамки, при снянии наведжения на фильм
        {
            Border b = (Border)sender;
            b.BorderThickness = new Thickness(0);
            b.Margin = new Thickness(5);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //нажатие пользователя на фильме(вход в окно свойств фильма)
            StackPanel SP=(StackPanel)sender;
            Films_description FD ;
            //установка параметров окна, в зависимости от того зарегистрирован он, или зашел без авторизации
            if(isSeach)
                FD = new Films_description(isAdmin, ds1.Tables[0].Rows[Convert.ToInt16(SP.Tag)]);
            else
                FD= new Films_description(isAdmin,ds.Tables[0].Rows[Convert.ToInt16(SP.Tag)]);
            FD.ShowDialog();
            if (isAdmin)//принятие изменений, которые сделал админ
            {
                setDS();
                if (isSeach)
                {
                    Seach();
                    PrintFilms(ds1);
                }
                else
                    PrintFilms(ds);
            }
        }

        private void CBFiltr_Checked(object sender, RoutedEventArgs e)
        {
            //включение использования фильтра
            if(isSeach)
                PrintFilms(ds1);
            else
                PrintFilms(ds);
        }

        private void CBFiltr_Unchecked(object sender, RoutedEventArgs e)
        {
            //отключение использования фильтра
            if (isSeach)
                PrintFilms(ds1);
            else
                PrintFilms(ds);
        }

        private void BBack_Click(object sender, RoutedEventArgs e)
            //нажатие на кнопку Назад(выход из режима поиска)
        {
            PrintFilms(ds);
            BBack.Visibility = Visibility.Collapsed;
            TBSeach.Text = "";
            isSeach = false;
        }

        private void BSeach_Click(object sender, RoutedEventArgs e)
            //нажатие на кнопку поика
        {
            Seach();
        }
        private void Seach()//поиск фильмов, подходящих условиям поиска
        {
            if (TBSeach.Text == "")
                //если в поле поиска не было ничего введено, то отменить режим поиска
            {
                PrintFilms(ds);
                BBack.Visibility = Visibility.Collapsed;
                isSeach = false;
            }
            else
            {
                //поиск
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    sql = "SELECT f.* From Films f Where f.Film_Name like N\'%" + TBSeach.Text + "%\'";
                    //запрос для выбора из базы фильмов, удовлетворяющих поиску
                    adapter = new SqlDataAdapter(sql, connection);
                    ds1 = new DataSet();
                    adapter.Fill(ds1);
                }
                PrintFilms(ds1);
                BBack.Visibility = Visibility.Visible;
                isSeach = true;

            }
        }
        private void BAdd_Click(object sender, RoutedEventArgs e)
            //вход в окно добавления нового фильма
        {
            Films_description FD = new Films_description(isAdmin,null);
            FD.ShowDialog();
            //вывод списка фильмов с добавленным новым фильмом
            setDS();
            if (isSeach)
                PrintFilms(ds1);
            else
                PrintFilms(ds);
            
        }

        private void LBack_MouseEnter(object sender, MouseEventArgs e)
            //событие при наведении на стрелку назад
        {
            LBack.Foreground = Brushes.Blue;
        }

        private void LBack_MouseLeave(object sender, MouseEventArgs e)
            //событие при снятии наведения на стрелку назад
        {
            LBack.Foreground = Brushes.Black;
        }

        private void LBack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            //возвращение к окну входа в систему
        {
            MainWindow MW = new MainWindow();
            MW.Show();
            this.Close();
        }
    }
}
