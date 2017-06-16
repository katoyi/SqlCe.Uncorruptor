using System;
using System.Data.SqlServerCe;
using System.Text;

namespace SqlCe.Uncorruptor
{
    public class CheckDataBase
    {
        private readonly string _connectionString;
        private readonly string _password;
        private SqlCeEngine Engine { get; set; }
        public CheckDataBase(string fileName, string password)
        {
            _connectionString = fileName;
            _password = password;
        }
        public bool Check()
        {
            if (string.IsNullOrEmpty(_connectionString))
                return true;
            var sb = new StringBuilder();
            sb.Append(string.Format("Data Source = {0}", _connectionString));
            if (!string.IsNullOrEmpty(_password))
                sb.Append(string.Format("; Password = {0}", _password));

            Engine = new SqlCeEngine(sb.ToString());
            return Engine.Verify();
        }

        public void Correct(LocalRepairOption? option)
        {
            if (Engine == null || option == null)
                return;
            try
            {
                Engine.Repair(null, (RepairOption)option);
            }
            catch (SqlCeException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public enum LocalRepairOption
        {
            DeleteCorruptedRows = 0,
            RecoverCorruptedRows = 1,
            RecoverAllPossibleRows = 1,
            RecoverAllOrFail = 2
        }
    }
}
