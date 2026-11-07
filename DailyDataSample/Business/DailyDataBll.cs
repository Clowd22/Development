using System;
using System.Data.SqlClient;
using DailyDataSample.Data;
using DailyDataSample.Models;

namespace DailyDataSample.Business
{
    public sealed class DailyDataBll
    {
        private readonly string _connectionString;
        private readonly DailyDataDal _dailyDataDal;

        public DailyDataBll(string connectionString)
        {
            _connectionString = connectionString;
            _dailyDataDal = new DailyDataDal(connectionString);
        }

        public DailyDataDto ReadData(int dataId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var data = _dailyDataDal.FindById(connection, transaction, dataId);
                        transaction.Commit();
                        return data;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void RegisterOrUpdate(DailyDataDto data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // UIから受け取った同じオブジェクトのプロパティをBLLで直接加工する。
            data.Remarks = (data.Remarks ?? string.Empty).Trim();
            data.WorkingHours = decimal.Round(data.WorkingHours, 2, MidpointRounding.AwayFromZero);
            data.WorkDate = data.WorkDate.Date;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var rowsAffected = _dailyDataDal.Update(connection, transaction, data);

                        if (rowsAffected == 0)
                        {
                            throw new OptimisticConcurrencyException();
                        }

                        transaction.Commit();

                        // DALのUPDATEでVersion = Version + 1されたため、画面側のDTOも同期する。
                        data.Version++;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
