namespace MergePdf;

/// <summary>
/// 應用程式進入點，配置全域例外處理並啟動主視窗。
/// </summary>
static class Program
{
    [STAThread]
    static void Main()
    {
        // 全域例外處理
        Application.ThreadException += (_, e) =>
        {
            MessageBox.Show(
                $"發生未預期的錯誤：\n{e.Exception.Message}",
                "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"發生嚴重錯誤：\n{ex.Message}",
                    "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
