using System;
using System.Data;
using System.Data.SqlClient;
using DailyDataSample.Models;

namespace DailyDataSample.Data
{
    public sealed class DailyDataDal
    {
        private readonly string _connectionString;

        public DailyDataDal(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DailyDataDto FindById(SqlConnection connection, SqlTransaction transaction, int dataId)
        {
            const string sql = @"
SELECT DataId, EmployeeId, WorkDate, WorkingHours, Remarks, Version
FROM DailyData
WHERE DataId = @DataId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@DataId", SqlDbType.Int).Value = dataId;

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new DailyDataDto
                    {
                        DataId = reader.GetInt32(reader.GetOrdinal("DataId")),
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        WorkDate = reader.GetDateTime(reader.GetOrdinal("WorkDate")),
                        WorkingHours = reader.GetDecimal(reader.GetOrdinal("WorkingHours")),
                        Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("Remarks")),
                        Version = reader.GetInt32(reader.GetOrdinal("Version"))
                    };
                }
            }
        }

        public int Update(SqlConnection connection, SqlTransaction transaction, DailyDataDto data)
        {
            const string sql = @"
UPDATE DailyData
SET EmployeeId = @EmployeeId,
    WorkDate = @WorkDate,
    WorkingHours = @WorkingHours,
    Remarks = @Remarks,
    Version = Version + 1
WHERE DataId = @DataId
  AND Version = @Version";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@DataId", SqlDbType.Int).Value = data.DataId;
                command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = data.EmployeeId;
                command.Parameters.Add("@WorkDate", SqlDbType.Date).Value = data.WorkDate.Date;

                var workingHours = command.Parameters.Add("@WorkingHours", SqlDbType.Decimal);
                workingHours.Precision = 5;
                workingHours.Scale = 2;
                workingHours.Value = data.WorkingHours;

                command.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value =
                    (object)data.Remarks ?? DBNull.Value;
                command.Parameters.Add("@Version", SqlDbType.Int).Value = data.Version;

                return command.ExecuteNonQuery();
            }
        }
    }
}
