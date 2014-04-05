using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.WES.Dal;
using THOK.WES;
using System.Collections;
using System.Threading;

namespace THOK.WES.View
{
    public partial class BaseTaskForm : THOK.AF.View.ToolbarForm
    {
        public delegate string TimerStateInMainThread();
        private ConfigUtil configUtil = new ConfigUtil();      
        private string url = @"http://59.61.87.212:8090/Task";
       
        /// <summary>
        /// 1:�ϼ��嵥,2:�̵��嵥,3:��λ�嵥,4:�����嵥,5:�¼��嵥
        /// </summary>
        protected string BillTypes = "";
        protected string BillString = "";
        protected string ReturnString = "";
        private ReadRfid rRfid = new ReadRfid();
        //ѡ���������
        string billNo = string.Empty;
        
     
        /// <summary>
        /// ʹ�õ��ӱ�ǩ = 0����ʹ�ã�1��ʹ�ã�
        /// </summary>
        private string UseTag = "";

        /// <summary>
        /// ʹ��Rfid  = 0����ʹ�ã�1���ֶ�ʹ�ã�2���Զ�ʹ�ã�
        /// </summary>
        private string UseRfid = "";

        /// <summary>
        /// ��ȡ������RFID�ţ�
        /// </summary>
        private string RfidCode = "";

        /// <summary>
        /// ������Ϣ��
        /// </summary>
        private string errInfo;

        /// <summary>
        /// ���ڣ�
        /// </summary>
        private string port;

        /// <summary>
        /// Real: ʵʱ���⣻NoReal: ��ʵʱ���⣻
        /// </summary>
        private string OperateType = "";

       
        private WaveData wave = new WaveData();
        List<string> listBill = null;//��ȡ������
        public BaseTaskForm()
        {
            InitializeComponent();
            pnlData.Visible = true;
            pnlData.Dock = DockStyle.Fill;
            pnlChart.Visible = false;
            pnlChart.Dock = DockStyle.Fill;

            url = configUtil.GetConfig("URL")["URL"];
            UseRfid = configUtil.GetConfig("RFID")["USEDRFID"];
            port = configUtil.GetConfig("RFID")["PORT"];

            if (configUtil.GetConfig("DeviceType")["Device"] == "0")
            {
                this.dgvMain.ColumnHeadersHeight = 40;
                this.dgvMain.RowTemplate.Height = 40;
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 16);
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 13);
                UseTag = "0";
            }
            else if (configUtil.GetConfig("DeviceType")["Device"] == "1")
            {
                this.dgvMain.ColumnHeadersHeight = 40;
                this.dgvMain.RowTemplate.Height = 40;
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 16);
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 13);
                //this.btnBatConfirm.Visible = false;
                UseTag = "1";
            }
            else
            {
                //dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvMain.Columns[0].Width = 150;
                dgvMain.Columns[1].Width = 150;
                dgvMain.Columns[2].Width = 60;
                dgvMain.Columns[3].Width = 120;
                dgvMain.Columns[4].Width = 100;
                dgvMain.Columns[5].Width = 200;
                dgvMain.Columns[6].Width = 60;
                dgvMain.Columns[7].Width = 60;
                this.dgvMain.ColumnHeadersHeight = 30;
                this.dgvMain.RowTemplate.Height = 40;
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 10);
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 14);                
                UseTag = "1";
            }
        }

        //��ѯ
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                this.VisibleWailt(false);
                listBill = wave.ScanNewBill("ScanNewBill", BillTypes);
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
                this.VisibleWailt(true);
                RefreshData();
            }
            catch (Exception ex)
            {
                THOKUtil.ShowError("��ȡ����ʧ�ܣ�ԭ��" + ex.Message);
                this.VisibleWailt(false);
                return;
            }
        }

        //ȷ��
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string getRFID = "";

            if (dgvMain.SelectedRows.Count != 0)
            {
                string bb_area_type = dgvMain.Rows[0].Cells["bb_area_type"].Value.ToString();

                if (bb_area_type == "0")                    //0:��������,1:�������,2:��������
                {
                    getRFID = ScanningRFID(bb_area_type);   //��ȡRFID

                    if (getRFID == null)
                    {
                        THOKUtil.ShowError("����ʧ�ܣ�");
                        this.VisibleWailt(false);
                        return;
                    }
                }
                else if (bb_area_type == "1")
                {
                    getRFID = "";
                }
                else
                {
                    THOKUtil.ShowError("�ö�������[������]��[��������]");
                    this.VisibleWailt(false);
                    return;
                }
                try
                {
                    this.VisibleWailt(true);

                    DataSet ds = GenerateEmptyTables();
                    foreach (DataGridViewRow row in dgvMain.SelectedRows)
                    {
                        DataRow detailRow = ds.Tables["DETAIL"].NewRow();
                        //������Ϣ
                        detailRow["bb_result_info"] = "0";
                        detailRow["bb_type"] = BillString;
                        detailRow["bb_order_id"] = row.Cells["bb_order_id"].Value.ToString();
                        detailRow["bb_pda_device_id"] = Environment.MachineName;
                        detailRow["bb_confirmor_name"] = Environment.MachineName;
                        detailRow["bb_confirm_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        detailRow["bb_corporation_id"] = row.Cells["bb_corporation_id"].Value.ToString();
                        detailRow["bb_corporation_name"] = row.Cells["bb_corporation_name"].Value.ToString();
                        //ϸ����Ϣ
                        detailRow["bb_detail_id"] = row.Cells["bb_detail_id"].Value.ToString();
                        detailRow["bb_operate_type"] = row.Cells["bb_operate_type"].Value.ToString();
                        detailRow["bb_pallet_move_flg"] = row.Cells["bb_pallet_move_flg"].Value.ToString();
                        detailRow["bb_cargo_no"] = row.Cells["bb_cargo_no"].Value.ToString();
                        if (BillTypes == "1")
                        {
                            detailRow["bb_pallet_no"] = getRFID;//�˱�ǩ��ΪRFID����ֵ
                        }
                        else
                        {
                            detailRow["bb_pallet_no"] = row.Cells["bb_pallet_no"].Value.ToString();
                        }
                        detailRow["bb_brand_id"] = row.Cells["bb_brand_id"].Value.ToString();
                        detailRow["bb_brand_name"] = row.Cells["bb_brand_name"].Value.ToString();
                        detailRow["bb_handle_num"] = Convert.ToDecimal(row.Cells["bb_handle_num"].Value.ToString());
                        detailRow["bb_inventory_num"] = Convert.ToDecimal(row.Cells["bb_inventory_num"].Value.ToString());
                        detailRow["bb_unit"] = row.Cells["bb_unit"].Value.ToString();
                        detailRow["bb_operator_name"] = Environment.UserName;
                        detailRow["bb_operate_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        ConfirmDialog confirmForm = new ConfirmDialog(BillTypes, row.Cells["bb_cargo_no"].Value.ToString(), row.Cells["bb_cargo_no"].Value.ToString(), row.Cells["bb_operate_type"].Value.ToString(), row.Cells["bb_brand_name"].Value.ToString());
                        confirmForm.Piece = Convert.ToInt32(row.Cells["bb_handle_num"].Value.ToString());
                        confirmForm.Bar = 0;
                        if (confirmForm.ShowDialog() == DialogResult.OK)
                        {
                            if (BillTypes == "2")
                            {
                                detailRow["bb_inventory_num"] = confirmForm.Piece;
                            }

                            ds.Tables["DETAIL"].Rows.Add(detailRow);

                            try
                            {
                                if (BillTypes == "1")
                                {
                                    wave.confirmData(ds.Tables["DETAIL"], BillTypes);
                                }
                                else
                                {
                                    if (getRFID == detailRow["bb_pallet_no"].ToString() || detailRow["bb_pallet_no"].ToString() == "" || detailRow["bb_pallet_no"].ToString() == null)
                                    {
                                        try
                                        {
                                            wave.confirmData(ds.Tables["DETAIL"], BillTypes);
                                            THOKUtil.ShowInfo("�������");
                                            this.RefreshData();
                                        }
                                        catch (Exception ex)
                                        {
                                            THOKUtil.ShowError("����ʧ�ܣ�Catch��" + ex.Message);
                                            this.VisibleWailt(false);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        THOKUtil.ShowError("RFID��ƥ�䣡��ѡ����ȷ�Ļ�λ�����³��⣡");
                                        this.VisibleWailt(false);
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                THOKUtil.ShowError("ִ���˳�confirmDataʧ�ܣ�ԭ��" + ex.Message);
                                this.VisibleWailt(false);
                                return;
                            }
                        }
                        else
                        {
                            this.VisibleWailt(false);
                            return;
                        }
                    }
                    this.VisibleWailt(false);
                }
                catch (Exception ex)
                {
                    THOKUtil.ShowError("ִ��ʧ�ܣ�ԭ��" + ex.Message);
                    this.VisibleWailt(false);
                    return;
                }
            }
            else
            {
                THOKUtil.ShowInfo("��ǰ����ʧ�ܣ�ԭ��û��ѡ�����ݣ���ѡ��");
                this.VisibleWailt(false);
                return;
            }
        }

        //��ȡRFID
        private string ScanningRFID(string bb_area_type)
        {
            List<string> listRfid = new List<string>();

            if (bb_area_type == "0")
            {
                this.VisibleWailt(true);
                try
                {
                    listRfid = rRfid.LoadTagList(port, 115200, out errInfo);
                }
                catch (Exception ex)
                {
                    this.VisibleWailt(false);
                    THOKUtil.ShowError("ɨ��RFIDʧ�ܣ����鴮���Ƿ�ƥ�䣡ԭ��" + ex.Message + "," + RfidCode);
                    return null;
                }

                Application.DoEvents();

                if (listRfid != null)
                {
                    if (listRfid.Count > 0)
                    {
                        //MessageBox.Show("[" + RfidCode + "]");
                        RfidCode = listRfid[0].ToString();
                        return RfidCode;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //�˳�
        private void btnExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        //ˢ������
        private void RefreshData()
        {
            if (listBill == null)
            {
                dgvMain.DataSource = null;
                return;
            }
            sslBillID.Text = "���ݺţ�" + billNo + "                              ";
            sslOperator.Text = "����Ա��" + Environment.MachineName;

            this.VisibleWailt(true);
            DataTable billTable = wave.ImportData(BillString, billNo).Tables["DETAIL"];
            InTask = true;
            if (billTable != null && billTable.Rows.Count != 0)
            {
                dgvMain.DataSource = billTable;
                InTask = false;
                this.VisibleWailt(false);

                //double sum = 0;
                //foreach (DataRow row in billTable.Rows)
                //{
                //    sum += double.Parse(row["bb_handle_num"].ToString());
                //}
                //string billType = "";
                //switch (BillTypes)
                //{
                //    case "1": billType = "�ϼ��嵥"; break;
                //    case "2": billType = "�̵��嵥"; break;
                //    case "3": billType = "��λ�嵥"; break;
                //    case "4": billType = "�¼��嵥"; break;
                //    default: billType = "�쳣����"; break;
                //}
                //THOKUtil.ShowInfo("��ǰ��" + billType + "��������������" + sum);
            }
            else
            {
                dgvMain.DataSource = null;
                InTask = false;
                this.VisibleWailt(false);
                THOKUtil.ShowError("��ǰû�����ݣ�");
            }
        }
        private string getBrandInfo()
        {
            List<string> listRfid = new List<string>();
            this.VisibleWailt(true);
            string brandInfo = string.Empty;
            try
            {
                listRfid = rRfid.LoadTagList(port, 115200, out errInfo);
                brandInfo = listRfid[0].ToString();
                Application.DoEvents();
                QuantityDialog quantityForm = new QuantityDialog();
                if (quantityForm.ShowDialog() == DialogResult.OK)
                {
                    if (BillTypes == "1")
                    {
                        brandInfo = quantityForm.Piece + ";" + brandInfo;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("��ȡ����������ˣ�" + e.Message);
            }

            return brandInfo;
        }

        public void VisibleWailt(bool b)
        {
            if (b == true)
            {
                this.plWailt.Visible = true;
                this.plWailt.Left = (this.dgvMain.Width - this.plWailt.Width) / 2;
                this.plWailt.Top = (this.dgvMain.Height - this.plWailt.Height) / 2;
            }
            else
            {
                this.plWailt.Visible = false;
            }
        }

        private DataSet GenerateEmptyTables()
        {
            DataSet ds = new DataSet();
            DataTable detail = ds.Tables.Add("DETAIL");

            detail.Columns.Add("bb_result_info");
            detail.Columns.Add("bb_type");
            detail.Columns.Add("bb_order_id");
            detail.Columns.Add("bb_pda_device_id");
            detail.Columns.Add("bb_confirmor_name");
            detail.Columns.Add("bb_confirm_date");
            detail.Columns.Add("bb_corporation_id");
            detail.Columns.Add("bb_corporation_name");

            detail.Columns.Add("bb_detail_id");
            detail.Columns.Add("bb_operate_type");
            detail.Columns.Add("bb_pallet_move_flg");
            detail.Columns.Add("bb_cargo_no");
            detail.Columns.Add("bb_pallet_no");
            detail.Columns.Add("bb_brand_id");
            detail.Columns.Add("bb_brand_name");
            detail.Columns.Add("bb_handle_num");
            detail.Columns.Add("bb_inventory_num");
            detail.Columns.Add("bb_unit");
            detail.Columns.Add("bb_operator_name");
            detail.Columns.Add("bb_operate_date");

            return ds;
        }
        private void btnOpType_Click(object sender, EventArgs e)
        {
            if (btnOpType.Text != "����")
            {
                btnOpType.Text = "����";
                OperateType = "NoReal";

                dgvMain.DataSource = wave.ImportData(BillString, billNo).Tables["DETAIL"];
            }
            else
            {
                btnOpType.Text = "ʵʱ";
                OperateType = "Real";

                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += new EventHandler(CyleTimer_Tick);
                timer.Start();
            }
        }

        /// <summary>��ó���ʣ�������</summary>
        private DataTable GetStockOut()
        { 
            DataTable dt = new DataTable();
            string sql = @"SELECT CIGARETTECODE,COUNT(CIGARETTECODE) FROM AS_STOCK_OUT WHERE STATE=0
                           GROUP BY CIGARETTECODE HAVING count(*)<10";
            dt = SQLHelper.GetDataTable(sql);
            return dt;
        }
        /// <summary>�����ַ�����ȡ���ַ���������String</summary>
        private string MakeString(DataTable dt, string field)
        {
            string list = "''";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list += ",'" + row["" + field + ""].ToString() + "'";
                }
            }
            return list;
        }
        private void Real()
        {
            dgvMain.DataSource = null;

            DataTable dt = null;
            DataTable dtLc = this.GetTest();// wave.ImportData(BillString, billNo).Tables["DETAIL"];
            DataTable dtStockOut = this.GetStockOut();
            if (dtStockOut.Rows.Count > 0 && dtLc.Rows.Count > 0)
            {
                string codeList = this.MakeString(dtStockOut, "CIGARETTECODE");
                dt = new DataTable();
                dt.Columns.Add("CIGARETTECODE", typeof(string));
                dt.Columns.Add("QUANTITY", typeof(Int32));

                foreach (DataRow row in dtStockOut.Rows)
                {
                    DataRow[] row_2 = dtLc.Select("CIGARETTECODE IN (" + row["CIGARETTECODE"] + ")");
                    if (row_2.Length > 0)
                    {
                        dt.Rows.Add(row_2[0].ItemArray);
                    }
                }
                dgvMain.DataSource = dt;
            }
        }
        private void CyleTimer_Tick(object sender, EventArgs e)
        {
            if (OperateType == "Real")
            {
                if (BillTypes == "4")
                {
                    this.Real();
                    this.btnOpType.Text = DateTime.Now.ToString("mm:ss");
                }
            }
        }
        private DataTable GetTest()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CIGARETTECODE", typeof(string));
            dt.Columns.Add("QUANTITY", typeof(Int32));

            DataRow dr1 = dt.NewRow();
            dr1["CIGARETTECODE"] = "5301043";
            dr1["QUANTITY"] = "10";
            DataRow dr2 = dt.NewRow();
            dr2["CIGARETTECODE"] = "5301043";
            dr2["QUANTITY"] = "20";
            DataRow dr3 = dt.NewRow();
            dr3["CIGARETTECODE"] = "3101009";
            dr3["QUANTITY"] = "30";
            DataRow dr4 = dt.NewRow();
            dr4["CIGARETTECODE"] = "3101009";
            dr4["QUANTITY"] = "40";

            dt.Rows.Add(dr1);
            dt.Rows.Add(dr2);
            dt.Rows.Add(dr3);
            dt.Rows.Add(dr4);

            return dt;
        }
    }
}

