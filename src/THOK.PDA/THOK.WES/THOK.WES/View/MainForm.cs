using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.WES;
using THOK.WES.View;

namespace THOK.PDA.View
{
    public partial class MainForm : Form
    {
        List<string> listBill = null;//��ȡ������
        string billNo = string.Empty;//ѡ������
        WaveData wave = new WaveData();

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            this.ReadMasterBill("1", "HITSHELF"); //�ϼ�
        }
        private void btnOut_Click(object sender, EventArgs e)
        {
            //this.ReadMasterBill("5", "STOCKMOVE");//�¼�

            listBill = wave.ScanNewBill("ScanNewBill", "5");
            switch (listBill.Count)
            {
                case 0:
                    billNo = "";
                    break;
                case 1:
                    billNo = listBill[0];
                    break;
                default:
                    SelectDialog selectDialog = new SelectDialog(listBill);
                    if (selectDialog.ShowDialog() == DialogResult.OK)
                    {
                        billNo = selectDialog.SelectedBillID;
                    }
                    break;
            }
        }
        private void btnCheck_Click(object sender, EventArgs e)
        {
            this.ReadMasterBill("2", "STOCKTAKE");//�̵�
        }
        private void btnMove_Click(object sender, EventArgs e)
        {
            this.ReadMasterBill("3", "STOCKOUT"); //��λ
        }
        private void ReadMasterBill(string billType, string billString)
        {
            MasterForm masterForm = new MasterForm(billType, billString);
            masterForm.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}