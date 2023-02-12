using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenGL;
using System.Runtime.InteropServices; 

namespace myOpenGL
{
    public partial class Form1 : Form
    {
        cOGL cGL;
        System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        public Form1()
        {

            InitializeComponent();
            cGL = new cOGL(panel1);

            Application.Idle += Application_Idle;
            Application.ApplicationExit += Application_ApplicationExit;

            myTimer.Tick += new EventHandler(TimerEventProcessor);
            // Sets the timer interval to 60 fps
            myTimer.Interval = 1000 / 60;
            myTimer.Start();

        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            cGL.Draw();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            cGL.Draw();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;
        }

        private void timerRepaint_Tick(object sender, EventArgs e)
        {
            cGL.Draw();
            //MessageBox.Show("tick");
            timerRepaint.Enabled = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            cGL.launch();
            timerRUN.Enabled = true;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            cGL.restart();
            timerRUN.Enabled = false;
            cGL.Draw();

        }

        private void changeBallImage_Click(object sender, EventArgs e)
        {
           cGL.changeBallColor();
        }

        private void timerRUN_Tick(object sender, EventArgs e)
        {
            cGL.Draw(); 
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            //timerRUN.Interval = hScrollBar1.Value;
            cGL.force = hScrollBar1.Value;
            label1.Text = "Force = " + hScrollBar1.Value;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            cGL.Draw();
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            cGL.OnResize();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            string s=((RadioButton)sender).Name;
            cGL.intOptionA = int.Parse(s.Substring(11));
            cGL.Draw();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //cGL.is2x3 = checkBox1.Checked;
            cGL.Draw();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }



        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            cGL.keyboardDown(e.KeyCode);
            cGL.Draw();

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            cGL.keyboardUp(e.KeyCode);
            cGL.Draw();
        }


        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}