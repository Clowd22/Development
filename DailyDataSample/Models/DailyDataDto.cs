namespace DailyDataSample.Models
{
    public sealed class DailyDataDto
    {
        public int DataId { get; set; }
        public int EmployeeId { get; set; }
        public System.DateTime WorkDate { get; set; }
        public decimal WorkingHours { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public sealed class OptimisticConcurrencyException : System.Exception
    {
        public OptimisticConcurrencyException()
            : base("他のユーザーによってデータが更新されています。最新データを再読み込みしてください。")
        {
        }
    }
}
