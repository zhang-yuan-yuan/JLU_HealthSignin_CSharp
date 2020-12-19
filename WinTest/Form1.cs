using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JLUReportOperator;

namespace WinTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        string path = @"..\..\test1.json";
        int hour1, hour2, minute1, minute2;
        int campus, dormitory;
        private void button1_Click(object sender, EventArgs e)
        {
            
            using (StreamWriter file = new StreamWriter(path, true))
            {
                file.WriteLine("{");
                file.WriteLine("\"transaction\": \"BKSMRDK\",");
                file.WriteLine("\"users\": [");
                file.WriteLine("{");
                file.WriteLine("\"username\":\"" + textBox1.Text+"\",");
                file.WriteLine("\"password\":\"" + textBox2.Text + "\",");
                file.WriteLine("\"fields\":{");
                file.WriteLine("\"fieldSQxq\": \""+ campus +"\",");
                file.WriteLine("\"fieldSQgyl\": \""+ dormitory + "\",");
                file.WriteLine("\"fieldSQqsh\":\"" + textBox3.Text + "\",");
                file.WriteLine("\"fieldSQnj\":\"" + textBox1.Text.Substring(textBox1.Text.Length - 4, 4) + "\",");
                file.WriteLine("\"fieldSQnj_Name\":\"20" + textBox1.Text.Substring(textBox1.Text.Length - 2, 2) + "\",");
                int x = 880 + Convert.ToInt32(textBox4.Text)-26;
                file.WriteLine("\"fieldSQbj\": \""+ x +"\",");
                file.WriteLine("\"fieldSQbj_Name\":\"" + textBox1.Text.Substring(textBox1.Text.Length - 4, 4)+ textBox4.Text+"\"");
                file.WriteLine("}");
                file.WriteLine("}");
                file.WriteLine("]");
                file.WriteLine("}");
                file.Close();
            }
            hour1 = Convert.ToInt32(textBox7.Text.Substring(0, 2));
            minute1 = Convert.ToInt32(textBox7.Text.Substring(3, 2));//+ new Random().Next(0, 10);
            hour2 = Convert.ToInt32(textBox8.Text.Substring(0, 2));
            minute2 = Convert.ToInt32(textBox8.Text.Substring(3, 2));//+ new Random().Next(0, 10);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString()) //获取选择的内容
            {

                case "中心校区": campus = 1; break;
                case "南岭校区": campus = 2; break;
                case "新民校区": campus = 3; break;
                case "南湖校区": campus = 4; break;
                case "和平校区": campus = 5; break;
                case "朝阳校区": campus = 6; break;
                case "前卫北区": campus = 7; break;

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedItem.ToString()) //获取选择的内容
            {

                case "北苑1公寓": dormitory = 1; break;
                case "北苑2公寓": dormitory = 2; break;
                case "南苑1公寓": dormitory = 3; break;
                case "南苑2公寓": dormitory = 4; break;
                case "南苑3公寓": dormitory = 5; break;
                case "南苑4公寓": dormitory = 6; break;
                case "南苑5公寓A区": dormitory = 7; break;
                case "南苑5公寓B区": dormitory = 8; break;
                case "南苑5公寓C区": dormitory = 9; break;

            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        bool flag = false;
        private void timer1_Tick(object sender, EventArgs e)
        {    
            if ((DateTime.Now.Hour == hour1 && DateTime.Now.Minute == minute1 && flag == false)
            ||  (DateTime.Now.Hour == hour2 && DateTime.Now.Minute == minute2 && flag == false))
            {
                //textBox6.Text = (DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));
                try
                {
                    if (Report.jlusign_in(path) == 0)
                    {
                        textBox5.Text = ("打卡成功");
                        flag = true;
                        textBox6.Text = (DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));
                    }
                }
                catch (Exception ex)
                {
                    textBox5.Text = ("打卡失败,失败原因：\r\n" + ex.Message);
                    //textBox6.Text = (DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));
                }

            }
            if (flag == true)
            {
                DateTime current = DateTime.Now;
                while (current.AddMilliseconds(60000) > DateTime.Now)
                {
                    Application.DoEvents();
                }
                flag = false;
            }
        }
    }
}
