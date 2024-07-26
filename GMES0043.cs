using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using System.IO;

using JPlatform.Client.Library6.interFace;
using JPlatform.Client;
using JPlatform.Client.Controls6;
using JPlatform.Client.JBaseForm6;
using JPlatform.Client.JERPBaseForm6;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors;
using System.Globalization;
using DevExpress.Utils;

namespace CSI.MES.P
{
    public partial class GMES0043 : JERPBaseForm
    {
        public double valid_date = 0;
        public DataTable _dtStyle = null;
        public bool _firstLoad = true, _is_search = false;

        public GMES0043()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AddButton = false;
            DeleteRowButton = false;
            //SaveButton = true;
            DeleteButton = false;
            PreviewButton = false;
            PrintButton = false;
            SaveButton = true;

            cboDate.BackColor = Color.FromArgb(255, 228, 225);
            cboPlant.BackColor = Color.FromArgb(255, 228, 225);
            cboGrade.BackColor = Color.FromArgb(255, 228, 225);
            cboArea.BackColor = Color.FromArgb(255, 228, 225);
            cboStyle.BackColor = Color.FromArgb(255, 228, 225);
            cboPo.BackColor = Color.FromArgb(255, 228, 225);
            cboPoItem.BackColor = Color.FromArgb(255, 228, 225);
            txtStyleName.BackColor = Color.FromArgb(255, 228, 225);
            _is_search = false;

            _firstLoad = true;
            loadControl();
            _firstLoad = false;
        }

        public override void QueryClick()
        {
            JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait frmSplash = new JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait();

            try
            {
                frmSplash.Show();

                InitControls(grdBase);
                grdBase.DataSource = null;
                gvwBase.Columns.Clear();

                InitControls(grdBase_Detail);
                grdBase_Detail.DataSource = null;
                gvwBase_Detail.Columns.Clear();

                DataTable _dtSource = GetData("Q");

                if (_dtSource != null && _dtSource.Rows.Count > 0)
                {
                    SetData(grdBase_Detail, _dtSource);
                    Formart_Grid_Summary(grdBase_Detail, gvwBase_Detail);
                }

                frmSplash.Close();

                DeleteRowButton = false;
            }
            catch
            {
                frmSplash.Close();
            }
        }

        public override void NewClick()
        {
            base.NewClick();

            DataTable _dtSource = GetData("Q_VALID_DATE");

            if (_dtSource != null && _dtSource.Rows.Count > 0)
            {
                if (_dtSource.Rows[0]["VALID_YN"].ToString().Equals("N"))
                {
                    MessageBox.Show("Chỉ nhập dữ liệu vào ngày hiện tại!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                return;
            }

            if (cboStyle.EditValue.ToString().Equals("ALL"))
            {
                MessageBox.Show("Style không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboGrade.EditValue.ToString().Equals("ALL"))
            {
                MessageBox.Show("Grade không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboArea.EditValue.ToString().Equals("ALL"))
            {
                MessageBox.Show("Area không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            InitControls(grdBase);
            grdBase.DataSource = null;
            gvwBase.Columns.Clear();

            InitControls(grdBase_Detail);
            grdBase_Detail.DataSource = null;
            gvwBase_Detail.Columns.Clear();

            buildHeader();
        }

        public override void DeleteRowClick()
        {
            try
            {
                DataTable _dtSource = GetData("Q_VALID_DATE");

                if (_dtSource != null && _dtSource.Rows.Count > 0)
                {
                    if (_dtSource.Rows[0]["VALID_YN"].ToString().Equals("N"))
                    {
                        MessageBox.Show("Chỉ được xóa dữ liệu của ngày hiện tại!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    return;
                }

                DialogResult dlr = MessageBox.Show("Bạn có muốn Xóa không?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dlr == DialogResult.Yes)
                {
                    bool result = SaveData("Q_DELETE");
                    if (result)
                    {
                        if (gvwBase_Detail.FocusedRowHandle >= 0)
                        {
                            gvwBase_Detail.DeleteRow(gvwBase_Detail.FocusedRowHandle);
                        }
                        DeleteRowButton = false;
                        MessageBoxW("Save successfully!", IconType.Information);
                    }
                    else
                    {
                        MessageBoxW("Save failed!", IconType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                this.MessageBoxW("DeleteRowClick" + ex.Message);
            }
        }

        public override void SaveClick()
        {
            try
            {
                DataTable _dtSource = GetData("Q_VALID_DATE");

                if(_dtSource != null && _dtSource.Rows.Count > 0)
                {
                    if (_dtSource.Rows[0]["VALID_YN"].ToString().Equals("N"))
                    {
                        MessageBox.Show("Chỉ nhập dữ liệu vào ngày hiện tại!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    return;
                }

                string _po_num = cboPo.EditValue == null ? "" : cboPo.EditValue.ToString();
                string _po_item = cboPoItem.EditValue == null ? "" : cboPoItem.EditValue.ToString();

                if (cboStyle.EditValue.ToString().Equals("ALL"))
                {
                    MessageBox.Show("Style không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cboGrade.EditValue.ToString().Equals("ALL"))
                {
                    MessageBox.Show("Grade không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cboArea.EditValue.ToString().Equals("ALL"))
                {
                    MessageBox.Show("Area không được chọn All!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(_po_num))
                {
                    MessageBox.Show("PO Number không được để trống!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(_po_item))
                {
                    MessageBox.Show("PO Item không được để trống!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult dlr = MessageBox.Show("Bạn có muốn Save không?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dlr == DialogResult.Yes)
                {
                    bool result = SaveData("Q_SAVE");
                    DeleteRowButton = false;
                    if (result)
                    {
                        MessageBoxW("Save successfully!", IconType.Information);
                    }
                    else
                    {
                        MessageBoxW("Save failed!", IconType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                this.MessageBoxW("SaveClick" + ex.Message);
            }
        }

        public void Formart_Grid_Summary(GridControlEx gridControl, GridViewEx gridView)
        {
            try
            {
                gridControl.BeginUpdate();
                gridView.ColumnPanelRowHeight = 35;

                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    gridView.Columns[i].Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(gridView.Columns[i].GetCaption().Replace("_", " ").ToLower());
                    gridView.Columns[i].OptionsColumn.AllowEdit = false;
                    gridView.Columns[i].OptionsColumn.ReadOnly = true;
                    gridView.Columns[i].OptionsColumn.AllowMerge = DevExpress.Utils.DefaultBoolean.True;

                    gridView.Columns[i].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    gridView.Columns[i].AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                    gridView.Columns[i].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    gridView.Columns[i].AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                    gridView.Columns[i].AppearanceCell.TextOptions.WordWrap = WordWrap.Wrap;
                    gridView.Columns[i].AppearanceCell.Font = new System.Drawing.Font("Calibri", 12, FontStyle.Regular);

                    if(i < 3)
                    {
                        gridView.Columns[i].Visible = false;
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("LINE_NAME"))
                    {
                        gridView.Columns[i].Width = 100;
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("STYLE_CODE"))
                    {
                        gridView.Columns[i].Width = 100;
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("PO_NO"))
                    {
                        gridView.Columns[i].Width = 120;
                        gridView.Columns[i].Caption = "PO No.";
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("PO_ITEM"))
                    {
                        gridView.Columns[i].Width = 120;
                        gridView.Columns[i].Caption = "PO Item";
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("GRADE_NAME"))
                    {
                        gridView.Columns[i].Width = 100;
                    }

                    if (gridView.Columns[i].FieldName.ToString().Equals("QTY"))
                    {
                        gridView.Columns[i].Width = 70;
                        gridView.Columns[i].DisplayFormat.FormatString = "#,#0.#";
                        gridView.Columns[i].Caption = "Quantity";
                    }
                }

                gridView.RowHeight = 30;
                gridView.OptionsView.ColumnAutoWidth = false;
                gridControl.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private DataTable GetData(string argType, string searchValue = "")
        {
            try
            {
                SP_GMES0043_Q proc = new SP_GMES0043_Q();

                string _plant = cboPlant.EditValue == null ? "" : cboPlant.EditValue.ToString();
                string _grade = cboGrade.EditValue == null ? "" : cboGrade.EditValue.ToString();
                string _area = cboArea.EditValue == null ? "" : cboArea.EditValue.ToString();
                string _style = cboStyle.EditValue == null ? "" : cboStyle.EditValue.ToString();
                string _po_num = cboPo.EditValue == null ? "" : cboPo.EditValue.ToString();
                string _po_item = cboPoItem.EditValue == null ? "" : cboPoItem.EditValue.ToString();

                DataTable dtData = null;
                dtData = proc.SetParamData(dtData, argType, cboDate.yyyymmdd, _plant, _grade, _area, _style, _po_num, _po_item, searchValue);

                ResultSet rs = CommonCallQuery(ServiceInfo.LMESBizDB, dtData, proc.ProcName, proc.GetParamInfo(), false, 90000, "", true);
                if (rs == null || rs.ResultDataSet == null || rs.ResultDataSet.Tables.Count == 0 || rs.ResultDataSet.Tables[0].Rows.Count == 0)
                {
                    return null;
                }
                return rs.ResultDataSet.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public bool SaveData(string _type)
        {
            JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait frmSplash = new JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait();

            try
            {
                bool _result = true;
                DataTable dtData = null;
                SP_GMES0043_S proc = new SP_GMES0043_S();
                string machineName = $"{SessionInfo.UserName}|{ Environment.MachineName}|{GetIPAddress()}";
                int iUpdate = 0, iCount = 0;
                frmSplash.Show();

                switch (_type)
                {
                    case "Q_SAVE":
                        DataTable _dtf = BindingData(grdBase, true, false);
                        if (_dtf != null && _dtf.Rows.Count > 0)
                        {
                            for (int iCol = 1; iCol < _dtf.Columns.Count - 1; iCol++)
                            {
                                iUpdate++;
                                string _size_cd = _dtf.Columns[iCol].ColumnName.ToString().Trim();
                                string _qty = string.IsNullOrEmpty(_dtf.Rows[0][iCol].ToString()) ? "0" : _dtf.Rows[0][iCol].ToString();

                                dtData = proc.SetParamData(dtData,
                                                          _type,
                                                          cboDate.yyyymmdd,
                                                          cboPlant.EditValue.ToString(),
                                                          cboGrade.EditValue.ToString(),
                                                          cboArea.EditValue.ToString(),
                                                          cboStyle.EditValue.ToString(),
                                                          cboPo.EditValue.ToString(),
                                                          cboPoItem.EditValue.ToString(),
                                                          _size_cd,
                                                          _qty,
                                                          machineName,
                                                          "CSI.MES.PD.GMES0043A_S");

                                if (CommonProcessSave(ServiceInfo.LMESBizDB, dtData, proc.ProcName, proc.GetParamInfo(), grdBase))
                                {
                                    dtData = null;
                                    iCount++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (iUpdate == iCount)
                            {
                                _result = true;
                            }
                            else
                            {
                                _result = false;
                            }
                        }
                        break;
                    case "Q_DELETE":
                        string V_LINE_CD = gvwBase_Detail.GetRowCellValue(gvwBase_Detail.FocusedRowHandle, "LINE_CD").ToString().Trim();
                        string V_GRADE = gvwBase_Detail.GetRowCellValue(gvwBase_Detail.FocusedRowHandle, "GRADE_CD").ToString().Trim();
                        string V_STYLE_CODE = gvwBase_Detail.GetRowCellValue(gvwBase_Detail.FocusedRowHandle, "STYLE_CODE").ToString().Trim();
                        string V_AREA = gvwBase_Detail.GetRowCellValue(gvwBase_Detail.FocusedRowHandle, "AREA").ToString().Trim();

                        dtData = proc.SetParamData(dtData,
                                                    _type,
                                                    cboDate.yyyymmdd,
                                                    V_LINE_CD,
                                                    V_GRADE,
                                                    V_AREA,
                                                    V_STYLE_CODE,
                                                    "",
                                                    "",
                                                    "",
                                                    "",
                                                    machineName,
                                                    "CSI.MES.PD.GMES0043A_S");

                        _result = CommonProcessSave(ServiceInfo.LMESBizDB, dtData, proc.ProcName, proc.GetParamInfo(), grdBase);

                        break;
                    default:
                        break;
                }

                frmSplash.Close();

                return _result;
            }
            catch (Exception ex)
            {
                frmSplash.Close();
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void gvwBase_Detail_RowClick(object sender, RowClickEventArgs e)
        {
            JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait frmSplash = new JPlatform.Client.CSIGMESBaseform6.frmSplashScreenWait();

            try
            {
                if (grdBase_Detail.DataSource == null || gvwBase_Detail.RowCount < 1) return;
                DeleteRowButton = true;

                if (e.Clicks >= 2)
                {
                    frmSplash.Show();

                    InitControls(grdBase);
                    grdBase.DataSource = null;
                    gvwBase.Columns.Clear();

                    string V_LINE_CD = gvwBase_Detail.GetRowCellValue(e.RowHandle, "LINE_CD").ToString().Trim();
                    string V_GRADE = gvwBase_Detail.GetRowCellValue(e.RowHandle, "GRADE_CD").ToString().Trim();
                    string V_STYLE_CODE = gvwBase_Detail.GetRowCellValue(e.RowHandle, "STYLE_CODE").ToString().Trim();
                    string V_AREA = gvwBase_Detail.GetRowCellValue(e.RowHandle, "AREA").ToString().Trim();
                    string V_PO_NO = gvwBase_Detail.GetRowCellValue(e.RowHandle, "PO_NO").ToString().Trim();
                    string V_PO_ITEM = gvwBase_Detail.GetRowCellValue(e.RowHandle, "PO_ITEM").ToString().Trim();

                    SP_GMES0043_Q proc = new SP_GMES0043_Q();
                    DataTable dtData = null, _dtSource = null;
                    dtData = proc.SetParamData(dtData, "Q_DETAIL", cboDate.yyyymmdd, V_LINE_CD, V_GRADE, V_AREA, V_STYLE_CODE, V_PO_NO, V_PO_ITEM, "");

                    ResultSet rs = CommonCallQuery(ServiceInfo.LMESBizDB, dtData, proc.ProcName, proc.GetParamInfo(), false, 90000, "", true);
                    if (rs == null || rs.ResultDataSet == null || rs.ResultDataSet.Tables.Count == 0 || rs.ResultDataSet.Tables[0].Rows.Count == 0)
                    {
                        _dtSource = null;
                    }
                    _dtSource = rs.ResultDataSet.Tables[0];

                    _firstLoad = true;

                    if (_is_search)
                    {
                        LoadDataCbo(cboStyle, "Q_STYLE", "Style Name");
                        _is_search = false;
                    }
                    
                    cboStyle.EditValue = V_STYLE_CODE;
                    LoadDataCbo(cboPo, "Q_PO", "PO Num");
                    cboPo.EditValue = V_PO_NO;
                    LoadDataCbo(cboPoItem, "Q_PO_ITEM", "PO Item");
                    cboPoItem.EditValue = V_PO_ITEM;
                    _firstLoad = false;

                    if (_dtSource != null && _dtSource.Rows.Count > 0)
                    {
                        DataTable _dtf = new DataTable();

                        for (int iRow = 0; iRow < _dtSource.Rows.Count; iRow++)
                        {
                            _dtf.Columns.Add(_dtSource.Rows[iRow]["CS_SIZE"].ToString(), typeof(double));
                        }

                        _dtf.Rows.Add();

                        for (int iRow = 0; iRow < _dtSource.Rows.Count; iRow++)
                        {
                            if (string.IsNullOrEmpty(_dtSource.Rows[iRow]["QTY"].ToString()))
                            {
                                _dtf.Rows[_dtf.Rows.Count - 1][_dtSource.Rows[iRow]["CS_SIZE"].ToString()] = DBNull.Value;
                            }
                            else
                            {
                                _dtf.Rows[_dtf.Rows.Count - 1][_dtSource.Rows[iRow]["CS_SIZE"].ToString()] = _dtSource.Rows[iRow]["QTY"].ToString();
                            }
                        }
                        SetData(grdBase, _dtf);

                        for (int i = 0; i < gvwBase.Columns.Count; i++)
                        {
                            if (i == 0)
                            {
                                gvwBase.Columns[i].OptionsColumn.AllowEdit = false;
                                gvwBase.Columns[i].OptionsColumn.ReadOnly = true;
                            }
                            gvwBase.Columns[i].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                            gvwBase.Columns[i].MinWidth = 60;

                            if (i > 0)
                            {
                                gvwBase.Columns[i].MinWidth = 45;
                            }
                        }
                        gvwBase.OptionsView.ColumnAutoWidth = false;
                    }

                    frmSplash.Close();
                }
            }
            catch (Exception ex)
            {
                frmSplash.Close();
                this.MessageBoxW("gvwBase_Detail_RowClick: " + ex.Message);
            }
        }

        private void loadControl()
        {
            try
            {
                cboDate.EditValue = DateTime.Now.ToString();

                LoadDataCbo(cboPlant, "Q_PLANT", "Plant");
                LoadDataCbo(cboGrade, "Q_GRADE", "Grade");
                LoadDataCbo(cboArea, "Q_AREA", "Area");
                LoadDataCbo(cboStyle, "Q_STYLE", "Style Name");

                LoadDataCbo(cboPo, "Q_PO", "PO Num");
                LoadDataCbo(cboPoItem, "Q_PO_ITEM", "PO Item");
            }
            catch (Exception ex)
            {
                this.MessageBoxW("loadControl: " + ex.Message);
            }
        }

        private void LoadDataCbo(LookUpEditEx argCbo, string _type, string _cbo_nm, string _search = "")
        {
            try
            {
                SP_GMES0043_Q proc = new SP_GMES0043_Q();

                string _plant = cboPlant.EditValue == null ? "" : cboPlant.EditValue.ToString();
                string _grade = cboGrade.EditValue == null ? "" : cboGrade.EditValue.ToString();
                string _area = cboArea.EditValue == null ? "" : cboArea.EditValue.ToString();
                string _style = cboStyle.EditValue == null ? "" : cboStyle.EditValue.ToString();
                string _po_no = cboPo.EditValue == null ? "" : cboPo.EditValue.ToString();
                string _po_item = cboPoItem.EditValue == null ? "" : cboPoItem.EditValue.ToString();

                DataTable dtData = null;
                dtData = proc.SetParamData(dtData, _type, cboDate.yyyymmdd, _plant, _grade, _area, _style, _po_no, _po_item, _search);
                ResultSet rs = CommonCallQuery(ServiceInfo.LMESBizDB, dtData, proc.ProcName, proc.GetParamInfo(), false, 90000, "", true);

                if (rs == null || rs.ResultDataSet == null || rs.ResultDataSet.Tables.Count == 0 || rs.ResultDataSet.Tables[0].Rows.Count == 0)
                {
                    argCbo.Properties.Columns.Clear();
                    argCbo.Properties.DataSource = null;

                    if (_type.Equals("Q_STYLE"))
                    {
                        _dtStyle = null;
                        txtStyleName.Text = "";

                        cboPo.Properties.Columns.Clear();
                        cboPo.Properties.DataSource = null;
                        cboPoItem.Properties.Columns.Clear();
                        cboPoItem.Properties.DataSource = null;
                    }

                    return;
                }
                else
                {
                    DataTable dt = rs.ResultDataSet.Tables[0];

                    if (_type.Equals("Q_STYLE"))
                    {
                        _dtStyle = dt.Copy();
                    }

                    string columnCode = dt.Columns[0].ColumnName;
                    string columnName = dt.Columns[1].ColumnName;
                    string captionCode = "Code";
                    string captionName = _cbo_nm;

                    argCbo.Properties.Columns.Clear();
                    argCbo.Properties.DataSource = dt;
                    argCbo.Properties.ValueMember = columnCode;
                    argCbo.Properties.DisplayMember = columnName;
                    argCbo.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo(columnCode));
                    argCbo.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo(columnName));
                    argCbo.Properties.Columns[columnCode].Visible = _type.Equals("Q_STYLE") ? true : false;
                    argCbo.Properties.Columns[columnCode].Width = 10;
                    argCbo.Properties.Columns[columnCode].Caption = captionCode;
                    argCbo.Properties.Columns[columnName].Caption = captionName;
                    argCbo.SelectedIndex = 0;

                    if (_type.Equals("Q_PO") || _type.Equals("Q_PO_ITEM"))
                    {
                        if (dt.Rows.Count > 1)
                        {
                            argCbo.SelectedIndex = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buildHeader()
        {
            try
            {
                DataTable _dtSource = GetData("Q_SIZE");

                if(_dtSource != null && _dtSource.Rows.Count > 0)
                {
                    DataTable _dtf = new DataTable();
                    _dtf.Columns.Add("Total", typeof(decimal));

                    for(int iRow = 0; iRow < _dtSource.Rows.Count; iRow++)
                    {
                        _dtf.Columns.Add(_dtSource.Rows[iRow]["CODE"].ToString(), typeof(double));
                    }

                    _dtf.Rows.Add();
                    SetData(grdBase, _dtf);

                    for (int i = 0; i < gvwBase.Columns.Count; i++)
                    {
                        if (i == 0)
                        {
                            gvwBase.Columns[i].OptionsColumn.AllowEdit = false;
                            gvwBase.Columns[i].OptionsColumn.ReadOnly = true;
                        }
                        gvwBase.Columns[i].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                        gvwBase.Columns[i].MinWidth = 60;

                        if (i > 0)
                        {
                            gvwBase.Columns[i].MinWidth = 45;
                        }
                    }
                    gvwBase.OptionsView.ColumnAutoWidth = false;
                }
            }
            catch (Exception ex)
            {
                this.MessageBoxW("buildHeader(): " + ex.Message);

            }
        }

        #region Events

        private void cboStyle_EditValueChanged(object sender, EventArgs e)
        {
            if (!_firstLoad)
            {
                LoadDataCbo(cboPo, "Q_PO", "PO Num");
            }
        }

        private void cboPo_EditValueChanged(object sender, EventArgs e)
        {
            if (!_firstLoad)
            {
                LoadDataCbo(cboPoItem, "Q_PO_ITEM", "PO Item");
            }
        }

        private void txtStyleName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (txtStyleName.Text != null)
                {
                    if(txtStyleName.Text.ToString() == "")
                    {
                        _is_search = false;
                    }
                    else
                    {
                        _is_search = true;
                    }

                    LoadDataCbo(cboStyle, "Q_STYLE", "Style Name", txtStyleName.Text.ToString().Trim());
                }
                txtStyleName.Text = "";
            }
        }

        private void gvwBase_Detail_CellMerge(object sender, CellMergeEventArgs e)
        {
            try
            {
                if (grdBase_Detail.DataSource == null || gvwBase_Detail.RowCount < 1) return;

                e.Merge = false;
                e.Handled = true;

                if (e.Column.FieldName.ToString().Equals("LINE_NAME"))
                {
                    string _value1 = gvwBase_Detail.GetRowCellValue(e.RowHandle1, e.Column.FieldName.ToString()).ToString();
                    string _value2 = gvwBase_Detail.GetRowCellValue(e.RowHandle2, e.Column.FieldName.ToString()).ToString();

                    if (_value1 == _value2 && !string.IsNullOrEmpty(_value1))
                    {
                        e.Merge = true;
                    }
                }

                if (e.Column.FieldName.ToString().Equals("STYLE_CODE"))
                {
                    string _value1 = gvwBase_Detail.GetRowCellValue(e.RowHandle1, "LINE_NAME").ToString();
                    string _value2 = gvwBase_Detail.GetRowCellValue(e.RowHandle2, "LINE_NAME").ToString();
                    string _value3 = gvwBase_Detail.GetRowCellValue(e.RowHandle1, e.Column.FieldName.ToString()).ToString();
                    string _value4 = gvwBase_Detail.GetRowCellValue(e.RowHandle2, e.Column.FieldName.ToString()).ToString();

                    if (_value1 == _value2 && !string.IsNullOrEmpty(_value1) &&
                        _value3 == _value4 && !string.IsNullOrEmpty(_value3))
                    {
                        e.Merge = true;
                    }
                }
            }
            catch { }
        }

        private void gvwBase_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            try
            {
                if (grdBase.DataSource == null || gvwBase.RowCount < 1) return;

                if (e.Column.FieldName.ToString().ToUpper().Equals("TOTAL"))
                {
                    e.Appearance.BackColor = Color.LightYellow;
                }
            }
            catch { }
        }

        private void cboGrade_EditValueChanged(object sender, EventArgs e)
        {
            if (!_firstLoad)
            {
                LoadDataCbo(cboArea, "Q_AREA", "Area");
            }
        }

        #endregion

        #region Database

        public class SP_GMES0043_Q : BaseProcClass
        {
            public SP_GMES0043_Q()
            {
                // Modify Code : Procedure Name
                _ProcName = "MES.SP_GMES0043_Q";
                ParamAdd();
            }
            private void ParamAdd()
            {
                _ParamInfo.Add(new ParamInfo("@ARG_WORK_TYPE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_DATE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PLANT", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_GRADE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_AREA", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_STYLE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PO_NO", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PO_ITEM", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_SEARCH", "Varchar", 100, "Input", typeof(System.String)));
            }
            public DataTable SetParamData(DataTable dataTable,
                                        System.String ARG_WORK_TYPE,
                                        System.String ARG_DATE,
                                        System.String ARG_PLANT,
                                        System.String ARG_GRADE,
                                        System.String ARG_AREA,
                                        System.String ARG_STYLE,
                                        System.String ARG_PO_NO,
                                        System.String ARG_PO_ITEM,
                                        System.String ARG_SEARCH)
            {
                if (dataTable == null)
                {
                    dataTable = new DataTable(_ProcName);
                    foreach (ParamInfo pi in _ParamInfo)
                    {
                        dataTable.Columns.Add(pi.ParamName, pi.TypeClass);
                    }
                }
                // Modify Code : Procedure Parameter
                object[] objData = new object[] {
                    ARG_WORK_TYPE,
                    ARG_DATE,
                    ARG_PLANT,
                    ARG_GRADE,
                    ARG_AREA,
                    ARG_STYLE,
                    ARG_PO_NO,
                    ARG_PO_ITEM,
                    ARG_SEARCH
                };
                dataTable.Rows.Add(objData);
                return dataTable;
            }
        }

        public class SP_GMES0043_S : BaseProcClass
        {
            public SP_GMES0043_S()
            {
                // Modify Code : Procedure Name
                _ProcName = "MES.SP_GMES0043_S";
                ParamAdd();
            }
            private void ParamAdd()
            {
                _ParamInfo.Add(new ParamInfo("@ARG_TYPE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_DATE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PLANT", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_GRADE", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_AREA", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_STYLE", "Varchar2", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PO_NO", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_PO_ITEM", "Varchar", 100, "Input", typeof(System.String)));

                _ParamInfo.Add(new ParamInfo("@ARG_SIZE", "Varchar2", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_QTY", "Varchar2", 100, "Input", typeof(System.String)));

                _ParamInfo.Add(new ParamInfo("@ARG_CREATE_PC", "Varchar", 100, "Input", typeof(System.String)));
                _ParamInfo.Add(new ParamInfo("@ARG_CREATE_PROGRAM_ID", "Varchar", 100, "Input", typeof(System.String)));
            }
            public DataTable SetParamData(DataTable dataTable,
                                        System.String ARG_TYPE,
                                        System.String ARG_DATE,
                                        System.String ARG_PLANT,
                                        System.String ARG_GRADE,
                                        System.String ARG_AREA,
                                        System.String ARG_STYLE,
                                        System.String ARG_PO_NO,
                                        System.String ARG_PO_ITEM,
                                        System.String ARG_SIZE,
                                        System.String ARG_QTY,
                                        System.String ARG_CREATE_PC,
                                        System.String ARG_CREATE_PROGRAM_ID)
            {
                if (dataTable == null)
                {
                    dataTable = new DataTable(_ProcName);
                    foreach (ParamInfo pi in _ParamInfo)
                    {
                        dataTable.Columns.Add(pi.ParamName, pi.TypeClass);
                    }
                }
                // Modify Code : Procedure Parameter
                object[] objData = new object[] {
                    ARG_TYPE,
                    ARG_DATE,
                    ARG_PLANT,
                    ARG_GRADE,
                    ARG_AREA,
                    ARG_STYLE,
                    ARG_PO_NO,
                    ARG_PO_ITEM,
                    ARG_SIZE,
                    ARG_QTY,
                    ARG_CREATE_PC,
                    ARG_CREATE_PROGRAM_ID
                };
                dataTable.Rows.Add(objData);
                return dataTable;
            }
        }

        #endregion
    }
}
