using AstmDriver.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BaseLibraries.Ado.Helpers;
using System.Linq;

namespace AstmDriver.Core
{
    public static class Dal
    {
        static string _connectionString;

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            
        }

        public static long WriteLog(string ip, string text, int port)
        {

            var cmd = new SqlCommand("InsertLog");
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@text", text);
            cmd.Parameters.AddWithValue("@port", port);
            var result = cmd.ExecuteScalar(_connectionString);
            return long.Parse(result.ToString());
        }

        public static long WriteLog(int analyzerId, string ip, string text, int port)
        {
            //TODO реализовать новую логику вставки в dbo.Log 
            string sql = $"insert into Log values (@analyzerId, @ip, @text, @port)";
            var cmd = new SqlCommand("InsertLog");
            cmd.Parameters.Add("@analyzerId", SqlDbType.Int).Value = analyzerId;
            cmd.Parameters.Add("@analyzerId", SqlDbType.Int).Value = analyzerId;
            
            var result = cmd.ExecuteScalar(_connectionString);
            return long.Parse(result.ToString());
        }

        internal static IEnumerable<TaskResult> GetDataForExport(int backAnalyzerID)
        {
            var cmd = new SqlCommand(@"GetResultsForExport");
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idm_analyzer", backAnalyzerID);
            
            return cmd.ExecuteReader<TaskResult>(_connectionString);
        }

        public static List<Analysis> AnalyzerSEND_GetCoderByBarcode(string barcode, string ip, string port)
        {
            barcode = barcode.Replace(" ", "");
            var cmd = new SqlCommand("Analyer_GetCodesByBarcode");
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@barcode", barcode);
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@port", port);
            return cmd.ExecuteReader<Analysis>(_connectionString);
        }

        public static Analyzer GetAnalyzer(string ip, int port)
        {
            Analyzer res = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand comm = new SqlCommand("GetAnalyzerByIpPort", connection);
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@ip", ip);
                comm.Parameters.AddWithValue("@port", port);
                SqlDataReader sdr = comm.ExecuteReader();
                if (sdr.Read())
                {
                    res = new Analyzer
                        (

                        Int32.Parse(sdr["id"].ToString()),
                        sdr["ip"].ToString(),
                        sdr["text"].ToString(),
                        sdr["port"].ToString(),
                        sdr["Header"].ToString(),
                        sdr["doSend"].ToString(),
                        sdr["astm_system_name"].ToString(),
                        sdr["astm_name"].ToString(),
                        sdr["astm_pass"].ToString(),
                        sdr["server_type"].ToString()

                        );
                }
            }

            return res;
        }

        internal static Tube GetTube(string barcode)
        {
            var cmd = new SqlCommand("select * from TUBE where barcode = @barcode");
            cmd.Parameters.AddWithValue("@barcode", barcode);
            return cmd.ExecuteReader<Tube>(_connectionString).FirstOrDefault();
        }

        internal static void SetDateExport(int idm_tasks_analysis)
        {
            var cmd = new SqlCommand("update TASKS_ANALYSIS set date_export = GETDATE() where id = @id;");
            cmd.Parameters.AddWithValue("@id", idm_tasks_analysis);
            cmd.ExecuteNonQuery(_connectionString);
        }

        internal static bool LinkOrder(Order order)
        {
            var tube = order.Barcode;
            try
            {
                
                CreatePatient(order.Patient);

                int idm_tube = CreateTube(order.Barcode);
                int idm_task = CreateTask(order);

                CreateTaskAnalysis(idm_task, order);
                UpdateTube(idm_tube, idm_task, order.Biomaterial);

                order.TaskID = idm_task.ToString();

                return true;


            }catch(Exception ex)
            {
                
            }

            return false;
        }

        private static void CreatePatient(Patient patient)
        {
            var chkCommand = new SqlCommand("select * from PATIENT WITH(NOLOCK) where ext_id = @ext_id");
            chkCommand.Parameters.AddWithValue("ext_id", patient.ext_id);
            var data = chkCommand.ExecuteReader<Patient>(_connectionString);
            if (data.Any())
            {
                patient.id = data.FirstOrDefault().id;
            }
            else
            {
                var insertCommand = new SqlCommand(@"insert into PATIENT (first_name,last_name,date_b,gender,ext_id) OUTPUT INSERTED.Id values (@first_name,@last_name,@date_b,@sex,@ext_id);");
                insertCommand.Parameters.AddWithValue("@first_name", (object)patient.first_name ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@last_name", (object)patient.last_name ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@date_b", (object)patient.date_b ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@sex", (object)patient.gender ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@ext_id", (object)patient.ext_id ?? DBNull.Value);
                
                var result = insertCommand.ExecuteScalar(_connectionString);
                if (result != null)
                {
                    patient.id = (int)result;
                }

            }
        }

        private static int CreateTube(string barcode) 
        {
            barcode = barcode.Trim();
            var cmd = new SqlCommand("BarCodeId");
            cmd.Parameters.AddWithValue("@barcode", barcode);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            return (int)cmd.ExecuteScalar(_connectionString);
        }

        private static void UpdateTube(int idm_tube, int idm_task, string bio_type)
        {
            var cmd = new SqlCommand(@"update TUBE set 
	                                        idm_tasks = @idm_task,
	                                        date_admission_lab = GETDATE(),
	                                        idm_color = @idm_color,
	                                        idm_biomaterial = @idm_bio,
	                                        color_id = @idm_color
                                        where id = @idm_tube;");

            int idm_bio = bio_type.ToUpper() == "SERUM" ? 1 : 2;
            int idm_color = bio_type.ToUpper() == "SERUM" ? 1 : 3;

            cmd.Parameters.AddWithValue("@idm_tube", idm_tube);
            cmd.Parameters.AddWithValue("@idm_task", idm_task);
            cmd.Parameters.AddWithValue("@idm_color", idm_color);
            cmd.Parameters.AddWithValue("@idm_bio", idm_bio);

            cmd.ExecuteNonQuery(_connectionString);

        }
        private static int CreateTask(Order order)
        {
            var cmd = new SqlCommand(@"CreateTask");
            cmd.Parameters.AddWithValue("@mis_task_id", order.Number);
            cmd.Parameters.AddWithValue("@mis_task_date", order.Date);
            cmd.Parameters.AddWithValue("@user_in", "Metascan");
            cmd.Parameters.AddWithValue("@sex", (object)order.Patient.gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@date_b", (object)order.Patient.date_b ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mis_patient_id", (object)order.Patient.ext_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idm_patient", (object)order.Patient.id ?? DBNull.Value);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            return (int)cmd.ExecuteScalar(_connectionString);
        }

        private static List<AnalysisExtraCode> GetAnalyses(int idm_analyzer, int idm_bio)
        {
            var cmd = new SqlCommand(@"select AEC.idm_analysis,AEC.code,AEC.assy_num 
                                        from ANALYSIS_EXTRA_CODE AEC 
                                        inner join ANALYSIS A on A.id = AEC.idm_analysis
                                        where AEC.idm_analyzer = @idm_analyzer and A.idm_biomaterial = @idm_bio and active = 1");
            cmd.Parameters.AddWithValue("@idm_analyzer", idm_analyzer);
            cmd.Parameters.AddWithValue(@"idm_bio", idm_bio);

            return cmd.ExecuteReader<AnalysisExtraCode>(_connectionString);
        }
        private static void CreateTaskAnalysis(int idm_task, Order order)
        {
            int idm_bio = order.Biomaterial.ToUpper() == "SERUM" ? 1 : 2;

            var analyses = GetAnalyses(order.AnalyzerID,idm_bio);

            var values = new List<string>();

            foreach (var row in order.Analysis)
            {
                var code = row.Replace("^", "").Trim();
                if (analyses.Any(x => x.code == code))
                {
                    var idm_analysis = analyses.FirstOrDefault(x => x.code == code).idm_analysis;

                    values.Add($"({idm_task},{idm_analysis},1,'Metascan')");
                }
            }

            if (values.Any())
            {
                var query = @"
                    DROP TABLE IF EXISTS #temp_ta;
                    create table #temp_ta (idm_tasks int,idm_analysis int,status int,user_in varchar(20));
                    insert into #temp_ta values ";
                query += string.Join(",", values);
                query += @";

                    MERGE TASKS_ANALYSIS AS target 
                    USING (SELECT * FROM #temp_ta) AS source 
                    ON (target.idm_tasks = source.idm_tasks AND target.idm_analysis = source.idm_analysis) 
                    WHEN NOT MATCHED 
                        THEN INSERT (idm_tasks,idm_analysis,status,user_in) VALUES (source.idm_tasks, source.idm_analysis,source.status,source.user_in);

                    drop table #temp_ta;
                    ";

                var cmd = new SqlCommand(query);
                cmd.ExecuteNonQuery(_connectionString);
            }

        }

        internal static List<MiuraOrder> GetMiuraOrders()
        {
            var query = @"Miura_GetWorkList";
            var cmd = new SqlCommand(query);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return cmd.ExecuteReader<MiuraOrder>(_connectionString);
        }

        internal static void SetTaskAnaysisStatus(IEnumerable<int> ids)
        {
            var splitChar = ",";
            var query = $"update TASKS_ANALYSIS set status = 2 where id in ({string.Join(splitChar,ids)})";
            var cmd = new SqlCommand(query);
            cmd.ExecuteNonQuery(_connectionString);
        }
        
    
        
    }

}
