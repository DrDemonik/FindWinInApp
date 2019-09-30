using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FindWinInApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Proc> dataProcs;
        List<IntPtr> dataWIN;

        public MainWindow()
        {
            InitializeComponent();
            dataProcs = new List<Proc>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var procs = Process.GetProcesses();
            foreach (var proc in procs)
            {
                if(proc.MainWindowHandle!=IntPtr.Zero)
                    dataProcs.Add( new Proc() { WinHanler=proc.MainWindowHandle, WinName = proc.MainWindowTitle, ProcessName = proc.ProcessName });
            }
            this.ProcessGrid.ItemsSource = dataProcs;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var temp = this.ProcessGrid.SelectedItem as Proc;
            if (temp != null)
            {
                this.LCurrentProc.Content = temp.ProcessName;
                 dataWIN = new WindowHandleInfo(temp.WinHanler).GetAllChildHandles();
                this.ChildsWinGrid.ItemsSource = dataWIN;
            }            
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
    }

    public class Proc
    {
        public IntPtr WinHanler { get; set; }
        public string WinName { get; internal set; }
        public string ProcessName { get; internal set; }
    }

    public class WindowHandleInfo
    {
        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private IntPtr _MainHandle;

        public WindowHandleInfo(IntPtr handle)
        {
            this._MainHandle = handle;
        }

        public List<IntPtr> GetAllChildHandles()
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }
    }
}
