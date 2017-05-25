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
using System.Windows.Shapes;

namespace List_of_movies
{
    /// <summary>
 
    /// </summary>
    public partial class CheckW : Window
    {
        public bool vibor = false;//флаг, характеризующий выбор пользователя
        //true -согласие false - отклонение 
        public CheckW(string text)
        {
            //окно для подтвреждения/отклонения действий text- тест описывающий событие, которое подтверждается
            InitializeComponent();
            LB.Content = text;
        }

        private void YesB_Click(object sender, RoutedEventArgs e)
        {
            //подтверждение действия
            vibor = true;
            this.Close();
        }

        private void NoB_Click(object sender, RoutedEventArgs e)
        {
            //отклонение действия
            vibor = false;
            this.Close();
        }
    }
}
