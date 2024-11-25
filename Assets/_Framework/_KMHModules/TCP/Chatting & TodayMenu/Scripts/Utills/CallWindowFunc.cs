using System;
using System.Runtime.InteropServices;
// using Microsoft.Toolkit.Uwp.Notifications; // ��ġ �ʿ�

public class CallWindowFunc
{
    [DllImport("user32.dll")]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static void ShowPopup(string title, string message)
    {
        // MessageBox â�� ���
        MessageBox(IntPtr.Zero, message, title, 0);
    }

    public static void ShowToast(string title, string message)
    {
        // new ToastContentBuilder()
        //     .AddText(title)
        //     .AddText(message)
        //     .Show(); // �˸� ǥ��
    }
}