﻿using System;
using System.Windows.Forms;

namespace Bai04
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_Server_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms["frm_Server"];

            if (frm != null)
                frm.BringToFront();
            else
            {
                frm = new frm_Server();
                frm.Show();
            }
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            frm_Client newClient = new frm_Client();
            newClient.Show();
        }
    }
}
