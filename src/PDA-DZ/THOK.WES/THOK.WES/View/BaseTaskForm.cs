using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.WES.Util;
using THOK.PDA.View;

namespace THOK.WES.View
{
    public partial class BaseTaskForm : Form
    {
        private string billType = "";
        private string billId = "";
        private WaveData wave = new WaveData();
        List<string> listBill = null;//��ȡ������
        private DataTable detailTable = null;
        public int index;
        protected string BillString = "";

        public BaseTaskForm(string billType, string billId)
        {
            InitializeComponent();
            this.billType = billType;
            this.billId = billId;
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void BaseTaskForm_Load(object sender, EventArgs e)
        {
            this.label2.Text = billId;
            switch (billType)
            {
                case "1":
                    this.label2.Text += "(���)";
                    break;
                case "2":
                    this.label2.Text += "(����)";
                    break;
                case "3":
                    this.label2.Text += "(�ƿ�)";
                    break;
                case "4":
                    this.label2.Text += "(�̵�)";
                    break;
            }
            DataTable tempTable = null;
            tempTable = wave.ImportData(BillString, billId).Tables["DETAIL"];
            detailTable = tempTable;
            if (tempTable != null && tempTable.Rows.Count != 0)
            {
                dgInfo.DataSource = tempTable;
            }
            else
            {
                this.btnNext.Enabled = false;
                dgInfo.DataSource = null;
            }

            DataGridTableStyle gridStyle = new DataGridTableStyle();
            gridStyle.MappingName = tempTable.TableName;
            dgInfo.TableStyles.Add(gridStyle);
            GridColumnStylesCollection columnStyles = this.dgInfo.TableStyles[0].GridColumnStyles;


            columnStyles["bb_cargo_no"].HeaderText = "   ��  λ";
            columnStyles["bb_cargo_no"].Width = 100;
            columnStyles["bb_brand_name"].HeaderText = "   ��  ��";
            columnStyles["bb_brand_name"].Width = 120;
            columnStyles["bb_operate_type"].HeaderText = "  ��  ��";
            columnStyles["bb_operate_type"].Width = 50;
            columnStyles["bb_detail_id"].HeaderText = "���ݺ�";
            //columnStyles["operateStorageName"].HeaderText = "   ��  λ";
            //columnStyles["operateStorageName"].Width = 100;
            //columnStyles["operateProductName"].HeaderText = "   ��  ��";
            //columnStyles["operateProductName"].Width = 120;
            //columnStyles["operateName"].HeaderText = "  ��  ��";
            //columnStyles["operateName"].Width = 50;
            //columnStyles["StatusName"].HeaderText = "  ״̬";
            //columnStyles["StatusName"].Width = 50;
            //columnStyles["DetailID"].HeaderText = "���ݺ�";
            //����ʾ�������Ϊ0
            columnStyles["operateName"].Width = 0;
            columnStyles["DetailID"].Width = 0;
            columnStyles["operatePieceQuantity"].Width = 0;
            columnStyles["operateBarQuantity"].Width = 0;
            columnStyles["targetStorageName"].Width = 0;

            if (tempTable.Rows.Count != 0)
            {
                if (tempTable.Rows.Count <= index)
                {
                    index = tempTable.Rows.Count - 1;
                }
                dgInfo.Select(index);
                dgInfo.CurrentRowIndex = index;
                dgInfo.Focus();
            }
            WaitCursor.Restore();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            WaitCursor.Set();
            try
            {
                MasterForm billMasterForm = new MasterForm(billType);
                billMasterForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                WaitCursor.Restore();
                MessageBox.Show(ex.Message);
            }
        }

        private void dgInfo_CurrentCellChanged(object sender, EventArgs e)
        {
            this.dgInfo.Select(this.dgInfo.CurrentCell.RowNumber);
            this.index = this.dgInfo.CurrentCell.RowNumber;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            WaitCursor.Set();
            try
            {
                DetailForm billDetailForm = new DetailForm(billType, this.dgInfo[this.dgInfo.CurrentCell.RowNumber, 0].ToString(), billId, detailTable);
                billDetailForm.Index = this.index;
                billDetailForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                WaitCursor.Restore();
                MessageBox.Show(ex.Message);
            }
        }

        private void BaseTaskForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "Escape")
            {
                this.btnBack_Click(null, null);
            }
            if (e.KeyCode.ToString() == "Return")
            {
                this.btnNext_Click(null, null);
            }
        }

    }
}