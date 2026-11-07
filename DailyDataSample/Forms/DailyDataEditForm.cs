using System;
using System.Drawing;
using System.Windows.Forms;
using DailyDataSample.Business;
using DailyDataSample.Models;

namespace DailyDataSample.Forms
{
    public sealed class DailyDataEditForm : BaseForm
    {
        private readonly int _targetId;
        private readonly DailyDataBll _dailyDataBll;
        private DailyDataDto _formData;
        private bool _isInitializing;

        private readonly TextBox _dataIdTextBox = new TextBox();
        private readonly TextBox _employeeIdTextBox = new TextBox();
        private readonly DateTimePicker _workDatePicker = new DateTimePicker();
        private readonly NumericUpDown _workingHoursInput = new NumericUpDown();
        private readonly TextBox _remarksTextBox = new TextBox();
        private readonly Button _saveButton = new Button();
        private readonly Button _clearButton = new Button();
        private readonly Button _closeButton = new Button();

        public DailyDataEditForm(int targetId, DailyDataBll dailyDataBll)
        {
            _targetId = targetId;
            _dailyDataBll = dailyDataBll ?? throw new ArgumentNullException(nameof(dailyDataBll));

            InitializeComponent();
            Load += DailyDataEditForm_Load;
        }

        private void InitializeComponent()
        {
            Text = "日次データ編集";
            ClientSize = new Size(420, 270);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var dataIdLabel = CreateLabel("データID", 20, 20);
            var employeeIdLabel = CreateLabel("従業員ID", 20, 55);
            var workDateLabel = CreateLabel("勤務日", 20, 90);
            var workingHoursLabel = CreateLabel("勤務時間", 20, 125);
            var remarksLabel = CreateLabel("備考", 20, 160);

            ConfigureTextBox(_dataIdTextBox, 130, 17, 120, true);
            ConfigureTextBox(_employeeIdTextBox, 130, 52, 120, false);
            ConfigureTextBox(_remarksTextBox, 130, 157, 250, false);
            _remarksTextBox.Multiline = true;
            _remarksTextBox.Height = 45;

            _workDatePicker.Location = new Point(130, 87);
            _workDatePicker.Width = 150;
            _workDatePicker.Format = DateTimePickerFormat.Short;

            _workingHoursInput.Location = new Point(130, 122);
            _workingHoursInput.Width = 120;
            _workingHoursInput.DecimalPlaces = 2;
            _workingHoursInput.Increment = 0.25M;
            _workingHoursInput.Maximum = 24;

            ConfigureButton(_saveButton, "登録/更新", 130, 220);
            ConfigureButton(_clearButton, "クリア", 225, 220);
            ConfigureButton(_closeButton, "終了", 320, 220);

            _saveButton.Click += SaveButton_Click;
            _clearButton.Click += ClearButton_Click;
            _closeButton.Click += CloseButton_Click;

            Controls.AddRange(new Control[]
            {
                dataIdLabel,
                employeeIdLabel,
                workDateLabel,
                workingHoursLabel,
                remarksLabel,
                _dataIdTextBox,
                _employeeIdTextBox,
                _workDatePicker,
                _workingHoursInput,
                _remarksTextBox,
                _saveButton,
                _clearButton,
                _closeButton
            });
        }

        private static Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(left, top + 3)
            };
        }

        private static void ConfigureTextBox(TextBox textBox, int left, int top, int width, bool readOnly)
        {
            textBox.Location = new Point(left, top);
            textBox.Width = width;
            textBox.ReadOnly = readOnly;
        }

        private static void ConfigureButton(Button button, string text, int left, int top)
        {
            button.Text = text;
            button.Location = new Point(left, top);
            button.Width = 85;
        }

        private void DailyDataEditForm_Load(object sender, EventArgs e)
        {
            InitialDisplay();
        }

        protected override void InitialDisplay()
        {
            ReadData();
        }

        private void ReadData()
        {
            try
            {
                SetInputEnabled(false);
                _isInitializing = true;

                _formData = _dailyDataBll.ReadData(_targetId);

                if (_formData == null)
                {
                    ShowError("対象の日次データが存在しません。");
                    Close();
                    return;
                }

                BindDataToControls(_formData);
            }
            catch (Exception ex)
            {
                ShowError("初期データの読み込みに失敗しました。\r\n" + ex.Message);
                Close();
            }
            finally
            {
                _isInitializing = false;
                SetInputEnabled(true);
            }
        }

        private void BindDataToControls(DailyDataDto data)
        {
            _dataIdTextBox.Text = data.DataId.ToString();
            _employeeIdTextBox.Text = data.EmployeeId.ToString();
            _workDatePicker.Value = data.WorkDate;
            _workingHoursInput.Value = data.WorkingHours;
            _remarksTextBox.Text = data.Remarks ?? string.Empty;
        }

        // 画面入力値と、ReadDataで取得したVersionを1つのDTOへ詰める。
        private DailyDataDto CreateFormData()
        {
            int employeeId;
            if (!int.TryParse(_employeeIdTextBox.Text, out employeeId))
            {
                throw new FormatException("従業員IDを正しく入力してください。");
            }

            return new DailyDataDto
            {
                DataId = _targetId,
                EmployeeId = employeeId,
                WorkDate = _workDatePicker.Value.Date,
                WorkingHours = _workingHoursInput.Value,
                Remarks = _remarksTextBox.Text,
                Version = _formData.Version
            };
        }

        protected override void RegisterOrUpdate()
        {
            if (_isInitializing)
            {
                return;
            }

            try
            {
                SetInputEnabled(false);

                // DTOはUI側で生成し、そのオブジェクト参照をBLLへ渡す。
                _formData = CreateFormData();
                _dailyDataBll.RegisterOrUpdate(_formData);

                BindDataToControls(_formData);
                ShowInformation("登録/更新しました。");
                DialogResult = DialogResult.OK;
            }
            catch (OptimisticConcurrencyException ex)
            {
                ShowError(ex.Message);
            }
            catch (FormatException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("登録/更新に失敗しました。\r\n" + ex.Message);
            }
            finally
            {
                SetInputEnabled(true);
            }
        }

        protected override void Clear()
        {
            if (_formData == null)
            {
                return;
            }

            BindDataToControls(_formData);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            RegisterOrUpdate();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void SetInputEnabled(bool enabled)
        {
            _employeeIdTextBox.Enabled = enabled;
            _workDatePicker.Enabled = enabled;
            _workingHoursInput.Enabled = enabled;
            _remarksTextBox.Enabled = enabled;
            _saveButton.Enabled = enabled;
            _clearButton.Enabled = enabled;
            _closeButton.Enabled = enabled;
        }
    }
}
