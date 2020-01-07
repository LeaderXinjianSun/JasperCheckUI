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
using System.Data;
using OfficeOpenXml;
using System.IO;

namespace JasperCheckUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime lastInputDatetime;
        List<int> typesetting;
        public MainWindow()
        {
            InitializeComponent();           
        }

        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }

        private void Apploaded(object sender, RoutedEventArgs e)
        {
            try
            {
                lastInputDatetime = DateTime.Now;
                string ExIoExcelPath = System.Environment.CurrentDirectory + "\\排版.xlsx";
                if (File.Exists(ExIoExcelPath))
                {
                    typesetting = new List<int>();
                    FileInfo existingFile = new FileInfo(ExIoExcelPath);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        // get the first worksheet in the workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[2];
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 12; j++)
                            {
                                typesetting.Add(int.Parse(worksheet.Cells[i + 1, j + 1].Value.ToString()));
                            }
                        }
                    }
                }
                textBox1.Focus();
                AddMessage("软件加载完成");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((DateTime.Now - lastInputDatetime).TotalSeconds > 1)
            {
                textBox1.Text = "";
            }
            lastInputDatetime = DateTime.Now;
            if (e.Key == Key.Enter)
            {
                CheckFromDs(textBox1.Text);
            }
        }
        void AddMessage(string str)
        {
            string[] s = MsgTextBox.Text.Split('\n');
            if (s.Length > 1000)
            {
                MsgTextBox.Text = "";
            }
            if (MsgTextBox.Text != "")
            {
                MsgTextBox.Text += "\r\n";
            }
            MsgTextBox.Text += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }

        private void CheckButtonClicked(object sender, RoutedEventArgs e)
        {
            CheckFromDs(textBox1.Text);
            //productRectangle.SetValue(Canvas.TopProperty,(double)100);
        }
        private void Item_GotFocus(object sender, RoutedEventArgs e)
        {
            var item = (DataGridRow)sender;
            FrameworkElement objElement = dataGrid.Columns[3].GetCellContent(item);
            string bodbar = bodBarcodeTextBlock.Text = ((TextBlock)objElement).Text;
            objElement = dataGrid.Columns[7].GetCellContent(item);
            string pcsser = ((TextBlock)objElement).Text;
            if (pcsser != "")
            {
                AddMessage("板条码:" + bodbar + " PCSSER:" + pcsser);
                int aa = 0;
                int bb = int.Parse(pcsser);
                for (int i = 0; i < typesetting.Count; i++)
                {
                    if (typesetting[i] == bb)
                    {
                        aa = i;
                        break;
                    }
                }
                productRectangle.SetValue(Canvas.TopProperty, 46.0 + 36.5 * (aa / 12));
                productRectangle.SetValue(Canvas.LeftProperty, 34.5 + 28.18 * (aa % 12));
            }
            else
            {
                AddMessage("PCSSER:空");
            }
        }
        void CheckFromDs(string barcode)
        {
            if (barcode == "")
            {
                AddMessage("条码为空");
            }
            else
            {
                //string StrMySQL = "Server=192.168.100.229;Database=leaderb;Uid=sunxinjian;Pwd=*963/852;pooling=false;CharSet=utf8;port=3306";
                Mysql mysql = new Mysql();
                //if (mysql.Connect(StrMySQL))
                if (mysql.Connect())
                {
                    string stm = "SELECT * FROM BARBIND WHERE SCBARCODE = '" + barcode + "'ORDER BY SIDATE DESC";
                    DataSet ds = mysql.Select(stm);
                    DataTable dt = ds.Tables["table0"];
                    dataGrid.ItemsSource = dt.DefaultView;
                    AddMessage("找到" + dt.Rows.Count.ToString() + "条记录");
                }
                else
                {
                    AddMessage("数据库连接失败");
                }
                mysql.DisConnect();
            }
        }
    }
}
