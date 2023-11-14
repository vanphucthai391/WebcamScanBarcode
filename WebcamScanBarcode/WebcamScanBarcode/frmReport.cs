using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebcamScanBarcode
{
    public partial class frmReport : Form
    {
        string tableThisMonthData;
        string tableLastMonthData;
        public frmReport()
        {
            InitializeComponent();
        }
        
        private void decideReferenceTable()
        {
            tableThisMonthData = "bgl_0246" + DateTime.Today.ToString("yyyyMM") + "data";
            tableLastMonthData = "bgl_0246" + ((VBS.Right(DateTime.Today.ToString("yyyyMM"), 2) != "01") ?
       (long.Parse(DateTime.Today.ToString("yyyyMM")) - 1).ToString() : (long.Parse(DateTime.Today.ToString("yyyy")) - 1).ToString() + "12") + "data";
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            decideReferenceTable();
            string checkInFrom = dtpFrom.Value.ToString();
            string checkInTo = dtpTo.Value.ToString();
            TfSQL tf = new TfSQL();
            string sql;
            DataTable dt = new DataTable();
            if (tf.CheckTableExist(tableLastMonthData) && tf.CheckTableExist(tableThisMonthData))
            {
                sql = string.Format("select serno, lot, inspectdate,judge from {0} where inspectdate >='{1}' and inspectdate <'{2}' UNION ALL select serno,lot, inspectdate,judge from {3} where inspectdate >='{1}' and inspectdate <'{2}' order by inspectdate DESC", tableThisMonthData, checkInFrom, checkInTo, tableLastMonthData);
            }
            else if (tf.CheckTableExist(tableLastMonthData) && !tf.CheckTableExist(tableThisMonthData))
            {
                sql = string.Format("select serno, lot, inspectdate, judge from {0} where inspectdate >='{1}' and inspectdate <'{2}' order by inspectdate DESC", tableLastMonthData, checkInFrom, checkInTo);
            }
            else
            {
                sql = string.Format("select serno, lot, inspectdate, judge from {0} where inspectdate >='{1}' and inspectdate <'{2}' order by inspectdate DESC", tableThisMonthData, checkInFrom, checkInTo);
            }
            tf.sqlDataAdapterFillDatatableFromTesterDb(sql, ref dt);
            dgvReport.DataSource = dt;
        }
        // Sub-sub procedure: Make DATETIMEPICKER the date 10 days ago
        private void dtpSet10daysBefore(DateTimePicker dtp)
        {
            DateTime dt = dtp.Value.Date.AddDays(-10);
            dtp.Value = dt;
        }
        private void dtpSetEndCurrentDay(DateTimePicker dtp)
        {
            DateTime dt = DateTime.Now.AddDays(0);
            DateTime endTime = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
            dtp.Value = endTime;
        }
        private void frmReport_Load(object sender, EventArgs e)
        {
            dtpSet10daysBefore(dtpFrom);
            dtpSetEndCurrentDay(dtpTo);
        }

        private void dgvReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvReport.Columns[e.ColumnIndex].Name == "judge" && e.Value != null && e.Value.ToString() == "1")
            {
                for(int i=0; i < dgvReport.Columns.Count; i++)
                {
                    dgvReport.Rows[e.RowIndex].Cells[i].Style.BackColor = Color.Red;
                    dgvReport.Rows[e.RowIndex].Cells[i].Style.ForeColor = Color.White;
                }

            }
        }
    }
}
