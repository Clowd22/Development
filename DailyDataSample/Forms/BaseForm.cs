using System;
using System.Windows.Forms;

namespace DailyDataSample.Forms
{
    public abstract class BaseForm : Form
    {
        protected BaseForm()
        {
            StartPosition = FormStartPosition.CenterParent;
        }

        protected virtual void InitialDisplay()
        {
        }

        protected virtual void Search()
        {
        }

        protected virtual void RegisterOrUpdate()
        {
        }

        protected virtual void Clear()
        {
        }

        protected virtual void Exit()
        {
            Close();
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(this, message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowInformation(string message)
        {
            MessageBox.Show(this, message, "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
