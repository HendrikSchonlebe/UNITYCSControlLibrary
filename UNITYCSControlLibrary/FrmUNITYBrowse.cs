using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UNITYCSControlLibrary
{
    public partial class FrmUNITYBrowse : Form
    {
        public DataTable myClients;
        public String searchName;

        public FrmUNITYBrowse()
        {
            InitializeComponent();
        }

        private void FrmUNITYBrowse_Load(object sender, EventArgs e)
        {
            this.Text = "UNITY Clearing Sale System - Client Browse";
            dgClients.Rows.Clear();
            for (int i = 0; i < myClients.Rows.Count; i++)
            {
                dgClients.Rows.Add(Convert.ToInt32(myClients.Rows[i]["cs_unity_id"]), myClients.Rows[i]["cs_unity_sname"].ToString(), myClients.Rows[i]["cs_unity_name1"].ToString().Trim() + " " + myClients.Rows[i]["cs_unity_name2"].ToString().Trim(), myClients.Rows[i]["cs_unity_property"].ToString().Trim() + "," + myClients.Rows[i]["cs_unity_street"].ToString().Trim() + "," + myClients.Rows[i]["cs_unity_city"].ToString().Trim() + "," + myClients.Rows[i]["cs_unity_state"].ToString().Trim() + "," + myClients.Rows[i]["cs_unity_postcode"].ToString().Trim());
            }
            for (int i = 0; i < dgClients.Rows.Count; i++)
            {
                if (String.Compare(dgClients.Rows[i].Cells["ShortName"].Value.ToString().Trim().Substring(0, searchName.Trim().Length), searchName, true) >= 0)
                {
                    dgClients.Rows[i].Selected = true;
                    dgClients.Rows[i].Cells[1].Selected = true;
                    dgClients.CurrentCell = dgClients[1, i];
                    dgClients.FirstDisplayedScrollingRowIndex = dgClients.Rows[i].Index;
                    dgClients.PerformLayout();
                    break;
                }
                else
                {
                    dgClients.Rows[i].Selected = false;
                    dgClients.Rows[i].Cells[0].Selected = false;
                }
            }

            dgClients.Focus();
        }
        private void FrmUNITYBrowse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Hide();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                searchName = dgClients.CurrentRow.Cells["ShortName"].Value.ToString();
                this.DialogResult = DialogResult.Yes;
                this.Hide();
            }
        }
    }
}
