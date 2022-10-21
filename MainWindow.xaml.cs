using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Keystone_Asm_GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAssemble_Click(object sender, RoutedEventArgs e)
        {
            const string toolPath = ".\\Toolz\\kstool.exe";

            CheckIfToolIsCorrupted(toolPath);

            var arch = ((ComboBoxItem)ComboBoxArch.SelectedItem).Content.ToString();

            arch = arch?[..(arch.IndexOf(':'))];

            string arguments = arch + " \"" + TextBoxSourceCode.Text + "\"";

            Process process = new()
            {
                StartInfo = CreateProcessStartInfo(toolPath, arguments)
            };
            process.Start();

            string machineCode = process.StandardOutput.ReadToEnd();

            string rawBytes;

            if (machineCode.Contains('['))
            {
                int startIndex = machineCode.IndexOf('[');
                int endIndex = machineCode.IndexOf(']');

                rawBytes = machineCode.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            else
            {
                rawBytes = machineCode;
            }

            TextBoxMachineCode.Text = rawBytes.Trim();

            if(rawBytes.ToLowerInvariant().Contains("error") == false)
            {
                var splitted = rawBytes.Split(' ').Where(a => string.IsNullOrWhiteSpace(a) == false).ToList();

                TextBytesCount.Text = splitted.LongCount().ToString() + " bytes";
            }
            else
            {
                TextBytesCount.Text = "Error";
            }

            process.WaitForExit();
        }

        private ProcessStartInfo CreateProcessStartInfo(string toolPath, string arguments)
        {
            ProcessStartInfo processStartInfo = new();
            processStartInfo.FileName = toolPath;
            processStartInfo.Arguments = arguments;

            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;

            return processStartInfo;
        }

        private bool CheckIfToolIsCorrupted(string path)
        {
            var bytes = File.ReadAllBytes(path);

            var hash = ComputeSha256Hash(bytes);

            if (hash.Equals("4c5153b57fa954c051ecd485f455bae5cd57913bc3ae4d1d2ebfa1170da372b6") == false)
            {
                MessageBox.Show("Keystone Tool (kstool.exe) can be corrupted.",
                    "Warning", MessageBoxButton.OK);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ComputeSha256Hash(byte[] rawData)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(rawData);

            StringBuilder builder = new();

            foreach (var item in bytes)
                builder.Append(item.ToString("x2"));

            return builder.ToString();
        }
    }
}
