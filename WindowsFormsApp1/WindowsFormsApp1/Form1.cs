using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RasGIS.CoreFunctions;
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //选择面txt文件
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFile = new OpenFileDialog();
            pOpenFile.Filter = "Shapefile(*.txt)|*.txt";
            if (pOpenFile.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = pOpenFile.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog pOpenFile = new OpenFileDialog();
            pOpenFile.Filter = "Shapefile(*.txt)|*.txt";
            if (pOpenFile.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = pOpenFile.FileName;
            }
        }
        //游程法
        private void button2_Click(object sender, EventArgs e)
        {
            CreateCenterLines pCreateCenterLines = new CreateCenterLines();
            string Message = string.Empty;
            pCreateCenterLines.CenterLineCompt2(this.textBox1.Text,this.textBox3.Text, ref  Message);
            MessageBox.Show(Message);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CreateCenterLines pCreateCenterLines = new CreateCenterLines();
            string Message = string.Empty;
            pCreateCenterLines.CenterLineCompt(this.textBox2.Text, ref Message);
            MessageBox.Show(Message);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog pFolder = new FolderBrowserDialog();
            if (pFolder.ShowDialog() == DialogResult.OK)
            {
                //this.textBox3.Text = pFolder.SelectedPath + "\\" + DateTime.Now.Year.ToString("D2") + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2") + DateTime.Now.Second.ToString("D2");
                this.textBox3.Text = pFolder.SelectedPath;
            }
        }
    }
}
