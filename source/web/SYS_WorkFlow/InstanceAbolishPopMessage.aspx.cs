﻿using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using PlatForm.DBUtility;
using PlatForm.Functions;


public partial class SYS_WorkFlow_ayf_InstanceAbolishPopMessage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtPACKNO.Text = Request["InstanceID"];
            txtDATEM.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
            txtOPT_TYPE.Text = "prescribir";
            txtMEMBER_NAME.Text=Session["MemberName"].ToString();
            tdPackDesc.InnerText = DBOpt.dbHelper.ExecuteScalar("select f_desc from DMIS_SYS_PACK where F_NO=" + Request["InstanceID"]).ToString();

            object obj;
            obj = DBOpt.dbHelper.ExecuteScalar("select f_packtypeno from dmis_sys_pack where f_no=" + txtPACKNO.Text);
            if (obj != null) txtF_PACKTYPENO.Text = obj.ToString();
            obj = DBOpt.dbHelper.ExecuteScalar("select f_packname from dmis_sys_pack where f_no=" + txtPACKNO.Text);
            if (obj != null) txtF_PACKTYPENAME.Text = obj.ToString();
        }
    }

    protected void btnOK_Click(object sender, EventArgs e)
    {
        if (txtREASON.Text.Trim() == "")
        {
            JScript.Alert(GetGlobalResourceObject("WebGlobalResource", "ItemNotNull").ToString());//"某项不允许为空！"
            return;
        }
        //先保存操作记录
        string re = CustomControlSave.CustomControlSaveByTableNameReturnS(this.Page, "DMIS_SYS_WK_OPT_HISTORY");
        if (re != "")
        {
            JScript.Alert(re);
            return;
        }
        //再把业务的状态设置为作废
        string sql = "update dmis_sys_pack set f_status='4' where f_no=" + txtPACKNO.Text;
        if (DBOpt.dbHelper.ExecuteSql(sql) > 0)
        {
            JScript.CloseWin("refreshPage");
        }
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        JScript.CloseWindow();
    }

}
