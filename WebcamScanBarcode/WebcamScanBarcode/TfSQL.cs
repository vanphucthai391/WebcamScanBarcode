using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebcamScanBarcode
{
    class TfSQL
    {
        string conStringTesterDb = @"Server=192.168.145.12;Port=5432;User Id=pqm;Password=dbuser;Database=pqmdb; CommandTimeout=100; Timeout=100;";
        bool flagSQLConnection = true;
        public bool getStatusSQLConnection()
        {
            return flagSQLConnection;
        }
        public void sqlDataAdapterFillDatatableFromTesterDb(string sql, ref DataTable dt)
        {
            try
            {
                NpgsqlConnection connection = new NpgsqlConnection(conStringTesterDb);
                NpgsqlCommand command = new NpgsqlCommand();

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter())
                {
                    command.CommandText = sql;
                    command.Connection = connection;
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }

            }
            catch
            {
                flagSQLConnection = false;
                MessageBox.Show("No Internet. Please check your connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        public bool CheckTableExist(string tableName)
        {
            NpgsqlConnection connection = new NpgsqlConnection(conStringTesterDb);
            string cmd = "SELECT EXISTS (SELECT * FROM " + tableName + ")";
            NpgsqlCommand command = new NpgsqlCommand(cmd, connection);
            connection.Open();
            try
            {
                int result = command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch
            {
                //MessageBox.Show(ex.Message);
                connection.Close();
                return false;
            }
        }
        public byte[] getImageUser(string name, string section, string emp_no)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(conStringTesterDb))
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"SELECT image FROM bgp_0372_usermaster WHERE emp_no = @emp_no and name=@name and section=@section";
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("section", section);
                    cmd.Parameters.AddWithValue("emp_no", emp_no);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["image"] != DBNull.Value)
                                return (byte[])reader["image"];
                        }
                    }
                }
                connection.Close();
            }
            return null;
        }
    }
}
