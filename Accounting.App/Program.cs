// Program.cs
using System;
using System.Windows.Forms;

namespace Accounting.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new Form1()); // ← sửa ở đây
        }
    }
}
