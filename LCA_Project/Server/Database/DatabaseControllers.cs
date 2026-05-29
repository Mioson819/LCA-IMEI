using DocumentFormat.OpenXml.EMMA;
using Guna.UI2.WinForms;
using LCA_Project.Utilities;
using Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
namespace LCA_Project.Database
{
    public class DatabaseControllers
    {
        private static readonly object _lock = new object();
        private static DatabaseControllers _instance = null;
        public static DatabaseControllers Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseControllers();
                    }
                    return _instance;
                }
            }
        }
        private DatabaseControllers()
        {
            // Private constructor to prevent instantiation from outside  
        }
        public void LoadDatabase(Guna2DataGridView dgvt, string tableName, string condition)
        {
            // Corrected the instantiation of SqlConnection  
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    string query = $"SELECT * FROM {tableName}";
                    if (!string.IsNullOrEmpty(condition) || condition != null)
                    {
                        query += $" WHERE Controller = '{condition}'";
                    }
                    var cmd = new System.Data.SqlClient.SqlCommand(query, con);
                    var adapter = new System.Data.SqlClient.SqlDataAdapter(cmd);
                    var dt = new System.Data.DataTable();
                    adapter.Fill(dt);
                    dgvt.DataSource = dt;
                    dgvt.Columns[0].Visible = false;
                    dgvt.Columns[0].ReadOnly = true;
                }
                catch (Exception ex)
                {
                    if (ex is System.Data.SqlClient.SqlException sqlEx)
                    {
                        string query = $"SELECT TOP 4 * FROM {tableName}";
                        var cmd = new System.Data.SqlClient.SqlCommand(query, con);
                        var adapter = new System.Data.SqlClient.SqlDataAdapter(cmd);
                        var dt = new System.Data.DataTable();
                        adapter.Fill(dt);
                        dgvt.DataSource = dt;
                        dgvt.Columns[0].Visible = false;
                        dgvt.Columns[0].ReadOnly = true;
                        return;
                    }
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }
        public void UpdateDatabase(DataGridView dgvt, string tableName)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    string query = $"SELECT * FROM {tableName}";
                    var cmd = new System.Data.SqlClient.SqlCommand(query, con);
                    SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd);
                    SqlCommandBuilder builder = new System.Data.SqlClient.SqlCommandBuilder(adapter);
                    var dt = (System.Data.DataTable)dgvt.DataSource;
                    adapter.Update(dt);
                    MessageBox.Show("Database updated successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating database: " + ex.Message);
                }
            }
        }
        public Dictionary<string, string> GetParamater()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                string query = "SELECT Name,Ip from Controllers";
                var cmd = new SqlCommand(query, con);
                cmd.CommandTimeout = 60;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string Name = reader["Name"].ToString();
                    string IP = reader["IP"].ToString();
                    if (!values.ContainsKey(Name))
                    {
                        values.Add(Name, IP);
                    }
                }
            }
            return values;
        }
        public Dictionary<string, string> GetRegister_ControllerParameterInputs()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                string query = "SELECT Name,Register from ControllerParameterInputsSignal ";
                var cmd = new SqlCommand(query, con);
                //   cmd.Parameters.AddWithValue("@controller", controller);
                cmd.CommandTimeout = 10;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader[0].ToString();
                    string register = reader[1].ToString();
                    if (!values.ContainsKey(name))
                    {
                        values.Add(name, register);
                    }
                }
            }
            return values;
        }
        public List<string> GetName_Controller()
        {
            List<string> values = new List<string>();
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                string query = "SELECT Name from Controllers";
                var cmd = new SqlCommand(query, con);
                cmd.CommandTimeout = 10;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader["Name"].ToString();
                    if (!values.Contains(name))
                    {
                        values.Add(name);
                    }
                }
            }
            return values;
        }
        public DataforTagControl GetDataByKey(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM ControllerTag  WHERE Controller = @key", con);
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new DataforTagControl();
                    Type type = typeof(DataforTagControl);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
            }
            return null;
        }
        public DataforUnload GetDataForUnload(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT nXULoadNGNow,mYULoadNGNow,RsOUTlca,RsClassifylca,TakePointNG,ResetTrayNG,nXloadnow,nYloadnow  FROM ControllerParameterInputsResults WHERE Station = @key", con);
                cmd.CommandTimeout = 30;
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new DataforUnload();
                    Type type = typeof(DataforUnload);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
                return null;
            }
        }
        public Dataforload GetDataload(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT nXLoadOK,mYLoadOK,TakePointOK,ResetTrayOK FROM ControllerParameterInputsResults  WHERE Station = @key", con);
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new Dataforload();
                    Type type = typeof(Dataforload);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
                return null;
            }
        }
        public DataforloadImei GetDataloadImei(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT nXULoadNGNow,mYULoadNGNow,TakePointNG,ResetTrayNG FROM ControllerParameterInputsResults  WHERE Station = @key", con);
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new DataforloadImei();
                    Type type = typeof(DataforloadImei);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
                return null;
            }
        }
        public DataforNG4 GetDataNG4(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT nXULoadNG4,mXULoadNG4,RsOUTlca,RsClassifylca,TakePointNG4,ResetTrayNG4,CurPosZ,nXloadnow,nYloadnow FROM ControllerParameterInputsResults  WHERE Station = @key", con);
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new DataforNG4();
                    Type type = typeof(DataforNG4);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
                return null;
            }
        }
        public DataforInputResults GetDataByInputResults(string key)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM ControllerParameterInputsResults  WHERE Station = @key", con);
                cmd.Parameters.AddWithValue("@key", key);
                using (var reader = cmd.ExecuteReader())
                {
                    var data = new DataforInputResults();
                    Type type = typeof(DataforInputResults);
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string colName = reader.GetName(i);
                            PropertyInfo prop = type.GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && !reader.IsDBNull(i))
                            {
                                prop.SetValue(data, reader.GetValue(i).ToString());
                            }
                        }
                        return data;
                    }
                }
            }
            return null;
        }
        public void LoadDataNameModel(Guna2ComboBox cbx, string NameStation)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT NameModel FROM FolderPort  WHERE IdPort = @NameStation", con);
                cmd.Parameters.AddWithValue("@NameStation", NameStation);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var s = reader["NameModel"].ToString();
                        cbx.Invoke((MethodInvoker)(() =>
                        {
                            cbx.Items.Add(s);
                        }));
                    }
                }
            }
        }
        public void LoadDataNameModel2(DataGridViewComboBoxCell cbx, string NameStation)
        {
            using (var con = SqlConnect.GetConnection)
            {
                cbx.Items.Clear();
                con.Open();
                var cmd = new SqlCommand("SELECT NameModel FROM FolderPort  WHERE IdPort = @NameStation", con);
                cmd.Parameters.AddWithValue("@NameStation", NameStation);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var s = reader["NameModel"].ToString();
                        cbx.Items.Add(s);
                    }
                }
            }
        }
        public List<string> LoadDataNameModel3(List<string> _list, string NameStation)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT NameModel FROM FolderPort  WHERE IdPort = @NameStation", con);
                cmd.Parameters.AddWithValue("@NameStation", NameStation);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var s = reader["NameModel"].ToString();
                        _list.Add(s);
                    }
                }
                return _list;
            }
        }
        public string LoadDataFolder(string NameStation, string PORT)
        {
            if (NameStation == null) return "";
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("SELECT FolderPath FROM FolderPort  WHERE NameModel = @NameStation and IdPort=@PORT", con);
                cmd.Parameters.AddWithValue("@NameStation", NameStation);
                cmd.Parameters.AddWithValue("@PORT", PORT);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["FolderPath"].ToString();
                    }
                    return null;
                }
            }
        }
        public string LoadDataFolderPathOFFMESS(string NameStation, string PORT)
        {
            try
            {
                if (NameStation == null) return "";
                using (var con = SqlConnect.GetConnection)
                {
                    con.Open();
                    var cmd = new SqlCommand("SELECT FolderPath FROM FolderPort  WHERE NameModel = @NameStation and IdPort=@PORT", con);
                    cmd.Parameters.AddWithValue("@NameStation", NameStation);
                    cmd.Parameters.AddWithValue("@PORT", PORT);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["FolderPath"].ToString();
                        }
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        // Đọc PcType ("Nano" hoặc "Pamtech") từ bảng FolderPort
        // theo NameModel + IdPort (Port1..Port4).
        // Trả về "Nano" mặc định nếu không tìm thấy.
        public string GetPcType(string nameModel, string port)
        {
            try
            {
                if (string.IsNullOrEmpty(nameModel) || string.IsNullOrEmpty(port))
                    return "Nano";
                using (var con = SqlConnect.GetConnection)
                {
                    con.Open();
                    var cmd = new SqlCommand(
                        "SELECT PcType FROM FolderPort WHERE NameModel = @NameModel AND IdPort = @Port",
                        con);
                    cmd.Parameters.AddWithValue("@NameModel", nameModel);
                    cmd.Parameters.AddWithValue("@Port", port);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string val = reader["PcType"]?.ToString();
                            return string.IsNullOrEmpty(val) ? "Nano" : val;
                        }
                    }
                }
            }
            catch { }
            return "Nano";
        }
        public string[] GetnXnY(string nameStation, string model)
        {
            using (var con = SqlConnect.GetConnection)
            {
                string[] s = new string[3];
                con.Open();
                var cmd = new SqlCommand("SELECT nX,nY,nYNG4 FROM FolderPort  WHERE NameModel = @model and IdPort=@NameStation", con);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@NameStation", nameStation);
                cmd.CommandTimeout = 30;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            s[0] = reader["nX"]?.ToString();
                            s[1] = reader["nY"]?.ToString();
                            s[2] = reader["nYNG4"]?.ToString();
                        }
                        return s;
                    }
                }
                return null;
            }
        }
        public void InsertDataNG(string NGCode, string NameStation)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("INSERT INTO NgLog (Port,NGCode) VALUES (@NameStation,@NGCode)", con);
                cmd.Parameters.AddWithValue("@NameStation", NameStation);
                cmd.Parameters.AddWithValue("@NGCode", NGCode);
                cmd.ExecuteNonQuery();
            }
        }
        public void InsertDataPortSummaInput(string Port, int Input)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE PortSummary SET  Input= @Input WHERE Port=@Port", con);
                //   cmd.Parameters.AddWithValue("@Ups", Ups);
                cmd.Parameters.AddWithValue("@Input", Input);
                // cmd.Parameters.AddWithValue("@OK", OK);
                // cmd.Parameters.AddWithValue("@NG", NG);
                cmd.Parameters.AddWithValue("@Port", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public int SelectDataPortSummaInput(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select  Input from PortSummary  WHERE Port=@Port", con);
                cmd.Parameters.AddWithValue("@Port", Port);
                return int.Parse(cmd.ExecuteScalar().ToString());
            }
        }
        public void InsertDataPortSummaOK(string Port, int OK)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE PortSummary SET  OK = @OK WHERE Port=@Port", con);
                //   cmd.Parameters.AddWithValue("@Ups", Ups);
                //  cmd.Parameters.AddWithValue("@Input", Input);
                cmd.Parameters.AddWithValue("@OK", OK);
                // cmd.Parameters.AddWithValue("@NG", NG);
                cmd.Parameters.AddWithValue("@Port", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public int SelectDataPortSummaOk(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select  OK from PortSummary  WHERE Port=@Port", con);
                cmd.Parameters.AddWithValue("@Port", Port);
                return int.Parse(cmd.ExecuteScalar().ToString());
            }
        }
        public void InsertDataPortSummaNG(string Port, int OK)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE PortSummary SET  NG = @NG WHERE Port=@Port", con);
                //   cmd.Parameters.AddWithValue("@Ups", Ups);
                //  cmd.Parameters.AddWithValue("@Input", Input);
                cmd.Parameters.AddWithValue("@NG", OK);
                // cmd.Parameters.AddWithValue("@NG", NG);
                cmd.Parameters.AddWithValue("@Port", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public int SelectDataPortSummaNG(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select  NG from PortSummary  WHERE Port=@Port", con);
                cmd.Parameters.AddWithValue("@Port", Port);
                return int.Parse(cmd.ExecuteScalar().ToString());
            }
        }
        public void DeleteNGCODE(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM NgLog WHERE Port=@Port", con);
                cmd.Parameters.AddWithValue("@Port", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteNG(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE PortSummary SET  Ups=@value1,Input=@value2,OK=@value3,NG=@value4 WHERE Port=@NamePort", con);
                cmd.Parameters.AddWithValue("@value1", 0);
                cmd.Parameters.AddWithValue("@value2", 0);
                cmd.Parameters.AddWithValue("@value3", 0);
                cmd.Parameters.AddWithValue("@value4", 0);
                cmd.Parameters.AddWithValue("@NamePort", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public string IdModel(string nameStation, string NameModel)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("Select IdModel FROM FolderPort WHERE IdPort=@nameStation and NameModel=@NameModel", con);
                    cmd.Parameters.AddWithValue("@nameStation", nameStation);
                    cmd.Parameters.AddWithValue("@NameModel", NameModel);
                    return cmd.ExecuteScalar().ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                    return null;
                }
            }
        }
        public string NameModel(string nameStation, int Id)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("Select NameModel FROM FolderPort WHERE IdPort=@nameStation and IdModel=@Id", con);
                    cmd.Parameters.AddWithValue("@nameStation", nameStation);
                    cmd.Parameters.AddWithValue("@Id", Id);
                    return cmd.ExecuteScalar().ToString();
                }
                catch
                {
                    //MessageBox.Show("Error : " + ex.Message);
                    return "";
                }
            }
        }
        public string SelectTimerRunTime(string NamePort)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select  RunTime from Timer  WHERE NamePort=@Port", con);
                cmd.Parameters.AddWithValue("@Port", NamePort);
                return cmd.ExecuteScalar()?.ToString();
            }
        }
        public string SelectTimerErrTime(string NamePort)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select  ErrTime from Timer  WHERE NamePort=@Port", con);
                cmd.Parameters.AddWithValue("@Port", NamePort);
                return cmd.ExecuteScalar()?.ToString();
            }
        }
        public string SelectTimerIdleTime(string NamePort)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("Select IdleTime from Timer  WHERE NamePort=@Port", con);
                cmd.Parameters.AddWithValue("@Port", NamePort);
                return cmd.ExecuteScalar()?.ToString();
            }
        }
        public void InsertTimerRunTime(string NamePort, string Time)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE Timer SET  RunTime = @RunTime WHERE NamePort=@NamePort", con);
                cmd.Parameters.AddWithValue("@NamePort", NamePort);
                cmd.Parameters.AddWithValue("@RunTime", Time);
                cmd.ExecuteNonQuery();
            }
        }
        public void InsertTimerErrTime(string NamePort, string Time)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE Timer SET  ErrTime = @RunTime WHERE NamePort=@NamePort", con);
                cmd.Parameters.AddWithValue("@NamePort", NamePort);
                cmd.Parameters.AddWithValue("@RunTime", Time);
                cmd.ExecuteNonQuery();
            }
        }
        public void InsertTimerIdleTime(string NamePort, string Time)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE Timer SET  IdleTime = @RunTime WHERE NamePort=@NamePort", con);
                cmd.Parameters.AddWithValue("@NamePort", NamePort);
                cmd.Parameters.AddWithValue("@RunTime", Time);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteCurrentValue(string Port)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE CurrentValue SET  Input=@value1,OK=@value2,NG=@value3 WHERE Port=@NamePort", con);
                cmd.Parameters.AddWithValue("@value1", 0);
                cmd.Parameters.AddWithValue("@value2", 0);
                cmd.Parameters.AddWithValue("@value3", 0);
                cmd.Parameters.AddWithValue("@NamePort", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public void UpdateCurrentValue(string Port, int Input, int OK, int NG)
        {
            using (var con = SqlConnect.GetConnection)
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE CurrentValue SET  Input=@value1,OK=@value2,NG=@value3 WHERE Port=@NamePort", con);
                cmd.Parameters.AddWithValue("@value1", Input);
                cmd.Parameters.AddWithValue("@value2", OK);
                cmd.Parameters.AddWithValue("@value3", NG);
                cmd.Parameters.AddWithValue("@NamePort", Port);
                cmd.ExecuteNonQuery();
            }
        }
        public string[] GetCurrentValue(string nameStation)
        {
            using (var con = SqlConnect.GetConnection)
            {
                string[] s = new string[3];
                con.Open();
                var cmd = new SqlCommand("SELECT Input,OK,NG FROM CurrentValue  WHERE Port = @NameStation", con);
                cmd.Parameters.AddWithValue("@NameStation", nameStation);
                cmd.CommandTimeout = 30;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            s[0] = reader["Input"]?.ToString();
                            s[1] = reader["OK"]?.ToString();
                            s[2] = reader["NG"]?.ToString();
                        }
                        return s;
                    }
                }
                return null;
            }
        }
        public string GetIdModel(string PortLeft, string PortRight, string ModelNamePortLeft, string ModelNamePortRight)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("Select IdModel  from Model  WHERE PortLeft=@PortLeft AND PortRight=@PortRight AND ModelNamePortLeft AND ModelNamePortRight=@ModelNamePortRight", con);
                    cmd.Parameters.AddWithValue("@PortLeft", PortLeft);
                    cmd.Parameters.AddWithValue("@PortRight", PortRight);
                    cmd.Parameters.AddWithValue("@ModelNamePortLeft", ModelNamePortLeft);
                    cmd.Parameters.AddWithValue("@ModelNamePortRight", ModelNamePortRight);
                    return cmd.ExecuteScalar()?.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }
        public bool GetPasswordMaster(string Password)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("Select Id  from [User]  WHERE Id=@id and _User=@Password", con);
                    cmd.Parameters.AddWithValue("@id", 1);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    if (cmd.ExecuteScalar()?.ToString() != string.Empty || cmd.ExecuteScalar()?.ToString() != null)
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool GetPassword(int value, string Password)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("Select Id from [User]   WHERE Id=@id and _User=@Password", con);
                    cmd.Parameters.AddWithValue("@id", value);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    if (cmd.ExecuteScalar()?.ToString() != null)
                    {
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool UpdatePassword(int id, string Password)
        {
            using (var con = SqlConnect.GetConnection)
            {
                try
                {
                    con.Open();
                    var cmd = new SqlCommand("UPDATE [User] SET  _User=@Password WHERE Id=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}