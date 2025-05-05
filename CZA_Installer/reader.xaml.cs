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

namespace CZA_Installer
{
    /// <summary>
    /// Interaction logic for reader.xaml
    /// </summary>
    public partial class reader : Window
    {
        public reader(string data)
        {
            InitializeComponent();
            Box.Text = data;
        }
        public reader(List<string> files)
        {
            InitializeComponent();
            Title = $"{files.Count} Files";
            Box.Text = string.Join("\n", files);

        }
    }
}
