using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Flogging.Data
{
    public class Sproc
    {
        public string StoredProcName { get; }
        public List<SqlParameter> Inputs { get; }

        private SqlCommand Command { get; set; }

        public Sproc(SqlConnection db, string procName, int timeoutSeconds = 30)
        {
            StoredProcName = procName;
            Inputs = new List<SqlParameter>();
            Command = new SqlCommand(procName, db) { CommandType = CommandType.StoredProcedure };

            if (timeoutSeconds != 30)
                Command.CommandTimeout = timeoutSeconds;

        }

        public void SetParam(string paramName, object value)
        {
            var param = new SqlParameter(paramName, value ?? DBNull.Value);

            Inputs.Add(param);
            Command.Parameters.Add(param);
        }

        public object ExecNonQuery()
        {
            try
            {
                return Command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new SprocException("SQL Exception occurred!!", StoredProcName, InputString, sqlEx);
            }
        }

        public object ExecScalar()
        {
            try
            {
                return Command.ExecuteScalar();
            }
            catch (SqlException sqlEx)
            {
                throw new SprocException("SQL Exception occurred!!", StoredProcName, InputString, sqlEx);
            }
        }
        public List<string> ExecForList()
        {
            try
            {
                var results = new List<string>();
                var reader = Command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(Convert.ToString(reader[0]));
                }
                return results;
            }
            catch (SqlException sqlEx)
            {
                throw new SprocException("SQL Exception occurred!!", StoredProcName, InputString, sqlEx);
            }
        }

        public void Execute<T>(out List<T> outList)
        {
            try
            {
                var ds = new DataSet();
                using (var sqlDataAdapter = new SqlDataAdapter(Command))
                {
                    sqlDataAdapter.Fill(ds);
                    outList = CollectionHelper.BuildCollection<T>(typeof(T), ds.Tables[0]);
                }
            }
            catch (SqlException sqlEx)
            {
                throw new SprocException("SQL Exception occurred!!", StoredProcName, InputString, sqlEx);
            }
        }

        public string InputString
        {
            get
            {
                var inString = new StringBuilder();
                foreach (var param in Inputs)
                {
                    inString.Append(string.Format("{0}=", param.ParameterName));
                    var dt = param.Value as DataTable;
                    if (dt == null)
                        inString.Append(string.Format("{0}|", param.Value));
                    else
                    {
                        // assert -- must be a data table
                        var cols = (from DataColumn col in dt.Columns select col.ColumnName).ToList();

                        var ind = 0;
                        var rowDtls = new StringBuilder();
                        foreach (DataRow row in dt.Rows)
                        {
                            rowDtls.Clear();
                            rowDtls.Append(string.Format(@"[{0}]::", ind++));
                            foreach (var col in cols)
                            {
                                rowDtls.Append(string.Format("{0}:{1},", col, row[col]));
                            }
                            inString.Append(rowDtls);
                        }
                    }
                }

                return inString.Length > 2000 ? inString.ToString().Substring(0, 2000) : inString.ToString();
            }
        }
    }
}
