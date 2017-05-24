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
using System.Drawing;
using Microsoft.Win32;

using System.Configuration;
using System.Data.SqlClient;
using System.Data;
namespace List_of_movies
{
    /// <summary>
    /// Логика взаимодействия для Films_description.xaml
    /// </summary>
    public partial class Films_description : Window
    {
        
        bool isAdmin,isLoad=false,isPoster=false;
        //isAdmin - режим входа, с авторизацией или нет
        //isLoad- флаг, показывающий, что графический интерфейс загрузился
        //isPoster - флаг, указывающий, есть ли у фильма постер
        String connectionString;// строка подключения к БД
        String[] listGenre = new String[] { "Аниме", "Боевик", "Вестерн", "Детектив", "Документальный", "Драма", "Исторический", "Комедия", "Мелодрама", "Мультфильм", "Мюзикл", "Триллер", "Ужасы", "Фантастика", "Фентези" };
       //список жанров
        int rate=1,id=-1;
        //rate- текущий рейтинг фильма
        //id - номер фильма в базе, значение -1 означает, что мы добавляем новый фильм,которого нет в базе
        public Films_description( bool isAdmin,DataRow dr)//параметры режим входа и данные фильма, если добавляем новый фильм, то передаем null
        {
            this.isAdmin = isAdmin;          
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString; 
            InitializeComponent();
            if(isAdmin)
            {
                //установка графического интерфейса для админа и заполнение интерфейса даннными о фильме
                LName.Visibility = Visibility.Collapsed;
                LGenre.Content = "Жанр: ";
                Add.Visibility = Visibility.Visible;
                TBlDescption.Visibility = Visibility.Collapsed;
                for (int i = 0; i < listGenre.Length; i++)
                    CBGenre.Items.Add(listGenre[i]);
                if(dr!=null)
                {
                    id = Convert.ToInt16(dr.ItemArray[0]);
                    TBName.Text = dr.ItemArray[1].ToString();
                    setRate(Convert.ToInt16(dr.ItemArray[2]));
                    CBGenre.SelectedIndex = Convert.ToInt16(dr.ItemArray[3]);
                    if (dr.ItemArray[5].ToString() != "")
                    {
                        byte[] bytes = (byte[])dr.ItemArray[5];
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

                        Poster.Source = bi;
                        Add.Content = "Изменить постер";
                        isPoster = true;
                        Delete.Visibility = Visibility.Visible;
                    }
                    if (dr.ItemArray[4].ToString() != "")
                    {
                        TBDescription.Text = dr.ItemArray[4].ToString();
                    }
                }
                else
                {
                    BDelFilm.Visibility = Visibility.Collapsed;
                    Save.Content = "Добавить фильм";
                }
            }
            else
            {
                //установка графического интерфейса для пользователя без авторизации и заполнение интерфейса даннными о фильме
                W.Height = 350;
                TBName.Visibility = Visibility.Collapsed;
                CBGenre.Visibility = Visibility.Collapsed;
                TBDescription.Visibility = Visibility.Collapsed;
                BDelFilm.Visibility = Visibility.Collapsed;
                LName.Content=dr.ItemArray[1];
                LGenre.Content=listGenre[Convert.ToInt16(dr.ItemArray[3])];
                setRate(Convert.ToInt16(dr.ItemArray[2]));
                if (dr.ItemArray[5].ToString()!="")
                {
                    byte[] bytes = (byte[])dr.ItemArray[5];                
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
                        Poster.Source = bi;
                }
                if(dr.ItemArray[4].ToString()!="")
                {
                    TBlDescption.Text = dr.ItemArray[4].ToString();
                }
            }
            isLoad = true; 
        }
        private void setRate(int needRate)
        {
            //установка нового рейтинга
            if (needRate > rate)
                //если выбранный рейтинг больше текушего
                for (int i = rate; i < needRate;i++)
                {
                    Image im = (Image)SPRate.Children[i];
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("rating.png", UriKind.Relative);
                    bi.EndInit();
                    im.Source = bi;
                }
            if(needRate<rate)
                //если выбранный рейтинг меньше текущего
                for (int i = needRate; i < rate; i++)
                {
                    Image im = (Image)SPRate.Children[i];
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("rnot.png", UriKind.Relative);
                    bi.EndInit();
                    im.Source = bi;
                }
           rate = needRate;
           LRate.Content = needRate + "/10";
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //событие, которое запускает смену рейтинга, запускает при шелчке по звезде, рейтинг которой, он хочет установить
            if(isAdmin)
            {
                Image im = (Image)sender;
                setRate(Convert.ToInt16(im.Tag));
                if (isLoad && !Save.IsEnabled)
                {
                    Save.IsEnabled = true;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //событие, сохранения изменений
            Save_data();
            if (id == -1)
                //если мы добавляли фильм, то закрыть окно
                this.Close();
        }
        private void Save_data()
        {
            //сохранение изменений
            byte[] poster = null;
            if (id == -1)
                //дабвление нового фильма
            {
                if (isPoster)
                {
                    //преобразование постера в двоичнрые данные для хранения в базе
                    BitmapImage bi = (BitmapImage)Poster.Source;
                    MemoryStream memoryStream = new MemoryStream();
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bi));
                    encoder.Save(memoryStream);
                    poster = memoryStream.ToArray();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    //сохранение данных в базе
                    {
                        connection.Open();
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO Films(Film_name,Film_rate,Film_genre,Film_picture,Film_description) "+
                             "VALUES (N\'" + TBName.Text + "\'," + rate + "," + CBGenre.SelectedIndex + ",(@poster),N\'" + TBDescription.Text + "\')", connection);
                        SqlParameter sqlParameter = new SqlParameter("@poster", SqlDbType.VarBinary);
                        sqlParameter.Value = poster;
                        sqlCommand.Parameters.Add(sqlParameter);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        //сохранение данных в базе без постера
                        connection.Open();
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO Films(Film_name,Film_rate,Film_genre,Film_description) " +
                             "VALUES (N\'" + TBName.Text + "\'," + rate + "," + CBGenre.SelectedIndex + ",N\'" + TBDescription.Text + "\')", connection);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                Save.IsEnabled = false;
            }
            else
            {
                //изменение параметров старого
                if (isPoster)
                {
                    //изменение данных, если есть постер
                    BitmapImage bi = (BitmapImage)Poster.Source;
                    MemoryStream memoryStream = new MemoryStream();
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bi));
                    encoder.Save(memoryStream);
                    poster = memoryStream.ToArray();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand sqlCommand = new SqlCommand("UPDATE Films" +
                       " Set Film_name=N\'" + TBName.Text + "\',Film_rate=" + rate + ",Film_genre=" + CBGenre.SelectedIndex + ",Film_picture=(@poster),Film_description=N\'" + TBDescription.Text + "\'" +
                       " WHERE Film_id=" + id, connection);
                        SqlParameter sqlParameter = new SqlParameter("@poster", SqlDbType.VarBinary);
                        sqlParameter.Value = poster;
                        sqlCommand.Parameters.Add(sqlParameter);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    //изменение данных, если его нет
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand sqlCommand = new SqlCommand("UPDATE Films" +
                       " Set Film_name=N\'" + TBName.Text + "\',Film_rate=" + rate + ",Film_genre=" + CBGenre.SelectedIndex + ",Film_picture=null,Film_description=N\'" + TBDescription.Text + "\'" +
                       " WHERE Film_id=" + id, connection);
                        sqlCommand.ExecuteNonQuery();
                    }
                }

                Save.IsEnabled = false;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //удаление постера и изменение его на картинку, сообщающую,что постера нет
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("no_poster.jpg", UriKind.Relative);
            bi.EndInit();
            Poster.Source = bi;
            isPoster = false;
            Delete.Visibility = Visibility.Hidden;
            if (!Save.IsEnabled)
            {
                Save.IsEnabled = true;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //добавить изображение
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Изображения(*.JPG;*.GIF;*.PNG)|*.JPG;*.GIF;*.PNG";
            myDialog.CheckFileExists = true;
            //вызов проводника
            // Открываем окно проводника для выбора изображения.
            if (myDialog.ShowDialog() == true)
                //если в проводнике был выбран файл, то естановить его
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(myDialog.FileName, UriKind.Absolute);
                bi.EndInit();
                Poster.Source = bi;
                isPoster = true;
                Add.Content = "Изменить постер";
                Delete.Visibility = Visibility.Visible;
                if (!Save.IsEnabled)
                {
                    Save.IsEnabled = true;
                }
            } 
        }

        private void TBName_TextChanged(object sender, TextChangedEventArgs e)
            //появление кнопки сохранения изменений при изменении названия
        {

            if (isLoad && !Save.IsEnabled)
            {
                Save.IsEnabled = true;
            }
        }

        private void CBGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //появление кнопки сохранения изменений при изменении жнра
            if (isLoad && !Save.IsEnabled)
            {
                Save.IsEnabled = true;
            }
        }

        private void TBDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            //появление кнопки сохранения изменений при изменении описания
            if (isLoad && !Save.IsEnabled)
            {
                Save.IsEnabled = true;
            }
        }

        private void BDelFilm_Click(object sender, RoutedEventArgs e)
        {
            
            CheckW CW = new CheckW("Вы действительно хотите удалить фильм?");
            CW.ShowDialog();
            //вызов подтверждения пользователя о том, что он хочет удалить фильм
            if (CW.vibor)
            {
                //удаление фильма из базы
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand sqlCommand = new SqlCommand("DELETE Films Where Film_id=" + id, connection);
                    sqlCommand.ExecuteNonQuery();
                }
                Save.IsEnabled = false;
                this.Close();
            }
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            //вызов вопроса сохранения изменения, если пользователь закрыл окно, но есть не сохраненные изменения
        {
            if (Save.IsEnabled)
            {
                CheckW CW = new CheckW("Сохранить сделанные изменения?");
                CW.ShowDialog();
                if (CW.vibor)
                {

                    Save_data();
                }
            }
        }

    }
}
