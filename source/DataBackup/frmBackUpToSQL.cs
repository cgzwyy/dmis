using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PlatForm.DBUtility;
using PlatForm.Functions;
using System.IO;


namespace PlatForm
{
    public partial class frmBackUpToSQL : Form
    {
         private string _sql;

        public frmBackUpToSQL()
        {
            InitializeComponent();
        }

        private void frmBackUpToSQL_Load(object sender, EventArgs e)
        {
            dtpStart.Value = Convert.ToDateTime(DateTime.Now.ToString("yyyy") + "-01-01");
            dtpEnd.Value = DateTime.Now;
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN")
            {
                cbbTimeType.Items.Add(new ComboxItem("全部", "0"));
                cbbTimeType.Items.Add(new ComboxItem("按年", "1"));
                cbbTimeType.Items.Add(new ComboxItem("按月", "2"));
                cbbTimeType.Items.Add(new ComboxItem("按日", "3"));
            }
            else
            {
                cbbTimeType.Items.Add(new ComboxItem("cuan", "0"));
                cbbTimeType.Items.Add(new ComboxItem("año", "1"));
                cbbTimeType.Items.Add(new ComboxItem("mes", "2"));
                cbbTimeType.Items.Add(new ComboxItem("día", "3"));
            }

            cbbTimeType.SelectedIndex = 0;
            initDataBase();
        }

        private void initDataBase()
        {
            //DataTable dt;
            if (DBHelper.databaseType == "Oracle")
            {
                //_sql = "select username from all_users order by user_id";
                //cbbDataBase.Items.Add("DF_DMIS");
                cbbDataBase.Items.Add("WEBDMIS");
            }
            else if (DBHelper.databaseType == "SqlServer")
            {
                //_sql = "select name from master.dbo.sysdatabases order by name";
                cbbDataBase.Items.Add("XOPENSODB");
                cbbDataBase.Items.Add("WEBDMIS");
            }
            else if (DBHelper.databaseType == "Sybase")
            {
                //_sql = "select name from master.dbo.sysdatabases order by name";
                cbbDataBase.Items.Add("XOPENSODB");
                cbbDataBase.Items.Add("WEBDMIS");
            }
            else
            {
            }
        }

        private void cbbDataBase_SelectedIndexChanged(object sender, EventArgs e)
        {
            trvTables.Nodes.Clear();

            if (DBHelper.databaseType == "Oracle")
            {
                _sql = "select table_name from all_tables where owner='" + cbbDataBase.SelectedItem.ToString() + "' order by table_name";
            }
            else if (DBHelper.databaseType == "SqlServer")
            {
            }
            else if (DBHelper.databaseType == "Sybase")
            {
            }
            else
            {
            }

            DataTable dt = DBOpt.dbHelper.GetDataTable(_sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                trvTables.Nodes.Add(dt.Rows[i][0].ToString());
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in trvTables.Nodes)
                node.Checked = true;
        }

        private void btnSelectClean_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in trvTables.Nodes)
                node.Checked = false;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtPath.Text = dlg.SelectedPath;
        }

        private void btnGen_Click(object sender, EventArgs e)
        {
            DateTime start = dtpStart.Value;
            DateTime end = dtpEnd.Value;
            DateTime cur;
            int curMonth, curYear;
            Object obj;

            lsbMsg.Items.Clear();

            string path = txtPath.Text.Trim();

            if (start > end)
            {
                errorProvider1.SetError((Control)dtpEnd, " ");
                //MessageBox.Show("起始日期不能大于终止日期！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                errorProvider1.SetError((Control)dtpEnd, "");
            }
            if(path.Length<1)
            {
                errorProvider1.SetError((Control)txtPath, " ");
                //MessageBox.Show("请先选择存放备份文件的目录!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                errorProvider1.SetError((Control)txtPath, "");
            }
            if (!Directory.Exists(path))
            {
                errorProvider1.SetError((Control)txtPath, " ");
                //MessageBox.Show("目录:" + txtPath.Text.Trim() + " 不存在!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                errorProvider1.SetError((Control)txtPath, "");
            }

            Directory.SetCurrentDirectory(path);

            for (int i = 0; i < trvTables.Nodes.Count; i++)
            {
                if (!trvTables.Nodes[i].Checked) continue;

                //找对应的时间列
                _sql = "select QUERY_COL from DMIS_SYS_TABLES where OWNER='" + cbbDataBase.SelectedItem.ToString() + "' and NAME='" + trvTables.Nodes[i].Text + "'";
                obj = DBOpt.dbHelper.ExecuteScalar(_sql);
                if (obj == null || obj.ToString().Trim() == "")
                {
                    if (DBHelper.databaseType == "Oracle")
                        _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text;
                    else if (DBHelper.databaseType == "SqlServer" || DBHelper.databaseType == "Sybase")
                        _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + ".dbo." + trvTables.Nodes[i].Text;
                    else
                        _sql = "";

                    GenSql(_sql, trvTables.Nodes[i].Text, "");
                }
                else
                {
                    ComboxItem ci =(ComboxItem)cbbTimeType.SelectedItem;
                    //if(ci==null) 
                    if (DBHelper.databaseType == "Oracle")
                    {
                        if (ci.Value == "0" || ci.Value == "")
                        {
                            _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where to_char(" + obj.ToString() + ",'YYYYMMDD')>='" + start.ToString("yyyyMMdd") + "' and to_char(" + obj.ToString() + ",'YYYYMMDD')<='" + end.ToString("yyyyMMdd") + "'";
                            GenSql(_sql, trvTables.Nodes[i].Text, start.ToString("yyyyMMdd") +"-"+ end.ToString("yyyyMMdd"));
                        }
                        else if (ci.Value == "3")
                        {
                            cur = start;
                            while (cur <= end)
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where to_char(" + obj.ToString() + ",'YYYYMMDD')='" + cur.ToString("yyyyMMdd") + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyyMMdd"));
                                cur = cur.AddDays(1);
                            }
                        }
                        else if (ci.Value == "2")
                        {
                            cur = start;
                            curMonth=Convert.ToInt32(start.ToString("yyyyMM"));
                            while (curMonth <= Convert.ToInt32(end.ToString("yyyyMM")))
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where to_char(" + obj.ToString() + ",'YYYYMM')='" + cur.ToString("yyyyMM") + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyyMM"));
                                cur = cur.AddMonths(1);
                                curMonth = Convert.ToInt32(cur.ToString("yyyyMM"));
                            }
                        }
                        else if (ci.Value == "1")
                        {
                            cur = start;
                            curYear = start.Year;
                            while (curYear <= end.Year)
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where to_char(" + obj.ToString() + ",'YYYY')='" + cur.Year + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyy"));
                                cur = cur.AddYears(1);
                                curYear = cur.Year;
                            }
                        }
                    }
                    else if (DBHelper.databaseType == "SqlServer" || DBHelper.databaseType == "Sybase")
                    {
                        if (cbbTimeType.SelectedValue.ToString() == "0")
                        {
                            _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where convert(char(8)," + obj.ToString() + ",112)>='" + start.ToString("yyyyMMdd") + "' and convert(char(8)," + obj.ToString() + ",112)<='" + end.ToString("yyyyMMdd") + "'";
                            GenSql(_sql, trvTables.Nodes[i].Text, start.ToString("yyyyMMdd") + "-" + end.ToString("yyyyMMdd"));
                        }
                        else if (cbbTimeType.SelectedValue.ToString() == "3")
                        {
                            cur = start;
                            while (cur <= end)
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where convert(char(8)," + obj.ToString() + ",112)='" + cur.ToString("yyyyMMdd") + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyyMMdd"));
                                cur = cur.AddDays(1);
                            }
                        }
                        else if (cbbTimeType.SelectedValue.ToString() == "2")
                        {
                            cur = start;
                            curMonth = Convert.ToInt16(start.ToString("yyyyMM"));
                            while (curMonth <= Convert.ToInt16(end.ToString("yyyyMM")))
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where convert(char(6)," + obj.ToString() + ",112)='" + cur.ToString("yyyyMM") + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyyMM"));
                                cur = cur.AddMonths(1);
                                curMonth = Convert.ToInt16(cur.ToString("yyyyMM"));
                            }
                        }
                        else if (cbbTimeType.SelectedValue.ToString() == "1")
                        {
                            cur = start;
                            curYear = start.Year;
                            while (curYear <= end.Year)
                            {
                                _sql = "select * from " + cbbDataBase.SelectedItem.ToString() + "." + trvTables.Nodes[i].Text + " where convert(char(4)," + obj.ToString() + ",112)='" + cur.Year + "'";
                                GenSql(_sql, trvTables.Nodes[i].Text, cur.ToString("yyyy"));
                                cur = cur.AddYears(1);
                                curYear = cur.Year;
                            }
                        }
                    }
                    else
                    {
                        _sql = "";
                    }
                }
            }
        }

        private void btnSaveMessage_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfg = new SaveFileDialog();
            sfg.Filter = "Text files (*.txt)|*.txt";
            if (sfg.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sfg.FileName);
                for (int i = 0; i < lsbMsg.Items.Count; i++) 
                    sw.WriteLine(lsbMsg.Items[i]);
                sw.Close();
            }
        }

        /// <summary>
        /// 生成的文件格式如下:  表名(YYYYMMDD)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName"></param>
        /// <param name="ymd">YYYYMMDD YYYYMM YYYY</param>
        private void GenSql(string sql, string tableName,string ymd)
        {
            int counts = 1;
            DataTable result;
            result = DBOpt.dbHelper.GetDataTable(sql);
            if (result == null || result.Rows.Count < 1) return;

            DateTime temp;
            string sqls;
            StreamWriter sw;
            StringBuilder cols = new StringBuilder();
            StringBuilder vals = new StringBuilder();

            if (ymd != "")
                sw = new StreamWriter(tableName + "(" + ymd + ").txt");
            else
                sw = new StreamWriter(tableName + ".txt");

            foreach (DataRow r in result.Rows)
            {
                for (int j = 0; j < result.Columns.Count; j++)
                {
                    if (r[j] is System.DBNull) continue;
                    cols.Append(result.Columns[j].ColumnName + ",");

                    if (result.Columns[j].DataType.Name == "String" || result.Columns[j].DataType.Name == "Char")
                    {
                        vals.Append("'" + r[j].ToString() + "',");
                    }
                    else if (result.Columns[j].DataType.Name == "DateTime")
                    {
                        temp = Convert.ToDateTime(r[j]);
                        if (DBHelper.databaseType == "Oracle")
                            vals.Append("TO_DATE('" + temp.ToString("yyyy-MM-dd HH:mm:ss") + "','yyyy-mm-dd hh24:mi:ss'),");
                        else
                            vals.Append("'" + temp.ToString("yyyy-MM-dd HH:mm:ss") + "',");
                    }
                    else
                    {
                        vals.Append(r[j].ToString() + ",");
                    }
                }
                counts++;
                if (DBHelper.databaseType == "Oracle")
                {
                    sqls = "insert into " + tableName + "(" + cols.Remove(cols.Length - 1, 1).ToString() + ") values(" + vals.Remove(vals.Length - 1, 1).ToString() + ");";
                    sw.WriteLine(sqls);
                    if (counts % 10 == 0) sw.WriteLine("commit;");
                }
                else if (DBHelper.databaseType == "Sybase" || DBHelper.databaseType == "SqlServer")
                {
                    sqls = "insert into " + tableName + "(" + cols.Remove(cols.Length - 1, 1).ToString() + ") values(" + vals.Remove(vals.Length - 1, 1).ToString() + ")";
                    sw.WriteLine(sqls);
                    if (counts % 10 == 0) sw.WriteLine("go");
                }
                vals.Remove(0, vals.Length);
                cols.Remove(0, cols.Length);
            }

            //最后一行也写入commit;
            if (DBHelper.databaseType == "Oracle")
                sw.WriteLine("commit;");
            else if (DBHelper.databaseType == "Sybase" || DBHelper.databaseType == "SqlServer")
                sw.WriteLine("go");
            else
                sw.WriteLine("");

            sw.Flush();
            sw.Close();
            if(ymd!="")
                lsbMsg.Items.Add(tableName  + "(" + ymd + ")   " + result.Rows.Count);
            else
                lsbMsg.Items.Add(tableName + "   " + result.Rows.Count);

        }

 

 
    }
}