using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
namespace GiaiCaptcha
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cmd("adb exec-out screencap -p > screen.png"); ;
            var aa = VerifyCaptchaTiktok.VeryCaptcha("screen.png");
            

            MessageBox.Show($"Kết quả: {aa.Result}\nTòa độ bắt đầu X: {aa.X1} - Y: {aa.Y1}\nTòa độ kết thúc X: {aa.X2} - Y: {aa.Y2} \nTimeSwip: {aa.TimeSwipe}");

        }
        public void cmd(string arg)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + @"\cmd.exe";
            startInfo.Arguments = "/c " + arg;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
