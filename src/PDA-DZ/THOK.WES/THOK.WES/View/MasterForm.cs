using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.WES.Util;
using THOK.WES;
using THOK.WES.View;

namespace THOK.PDA.View
{
    public partial class MasterForm : Form
    {
        List<string> listBill = null;//��ȡ������
        WaveData wave = new WaveData();
        private string billType = "";

        public MasterForm(string billType)
        {
            InitializeComponent();
            this.billType = billType;
        }

        private List<string> ReadMasterBill(string billtype)
        {
            billType = billtype;
            listBill = wave.ScanNewBill("ScanNewBill", billType);
            return listBill;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            WaitCursor.Set();
            try
            {
                BaseTaskForm baseTaskForm = new BaseTaskForm(billType, this.lbInfo.SelectedValue.ToString());
                baseTaskForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                WaitCursor.Restore();
                MessageBox.Show("��ȡ����ʧ��!" + ex.Message);
            }
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {
            switch (billType)
            {
                case "1":
                    this.label2.Text = "��������ݺ�";
                    break;
                case "2":
                    this.label2.Text = "���������ݺ�";
                    break;
                case "3":
                    this.label2.Text = "�ƿ������ݺ�";
                    break;
                case "4":
                    this.label2.Text = "�̵������ݺ�";
                    break;
            }
            this.lbInfo.ValueMember = "BILLNO";
            this.lbInfo.DisplayMember = "BILLNO";

            this.lbInfo.DataSource = ReadMasterBill(this.billType);
            if (lbInfo.Items.Count>0)
            {
                this.btnNext.Enabled = false;
            }
            WaitCursor.Restore();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Visible = true;
        }
    }
}