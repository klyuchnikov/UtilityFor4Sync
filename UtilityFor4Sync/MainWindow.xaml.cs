using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Text.RegularExpressions;


namespace klyuchnikovds.UtilityFor4Sync {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            listLB.DataContext = listDeleted;
        }

        List<string> listDeleted = new List<string>();
        System.Windows.Forms.DialogResult result;
        System.Windows.Forms.FolderBrowserDialog dlg;

        private void brouse_Click(object sender, RoutedEventArgs e) {
            dlg = new System.Windows.Forms.FolderBrowserDialog();
            result = dlg.ShowDialog(this.GetIWin32Window());
            if (result == System.Windows.Forms.DialogResult.OK)
                pathLabel.Content = dlg.SelectedPath;
        }

        void RecDeleteFilesDublicate(DirectoryInfo root) {
            var files = root.GetFiles();
            pathLabel.Content = root.FullName;
            var ff = files.Where(a => Regex.IsMatch(a.Name.Substring(0, a.Name.Length - a.Extension.Length), @".*_\d$")).ToArray();
            foreach (var a in ff) {
                var source = new FileInfo(a.DirectoryName + "\\" + Regex.Replace(a.Name, @"_\d" + a.Extension, a.Extension));
                if (source.Exists) {
                    if (source.Length == a.Length || a.Length == 0) {
                        listLB.Items.Add("Deleted " + a.FullName);
                        listLB.UpdateLayout();
                        a.Delete();
                    }
                } else {
                    File.Move(a.FullName, source.FullName);
                    listLB.Items.Add("Rename " + a.FullName + " to " + source.FullName);
                }
            }
            foreach (var folder in root.GetDirectories())
                RecDeleteFilesDublicate(folder);
        }

        private void DeleteDublicatesFiles_Click(object sender, RoutedEventArgs e) {
            if (result == System.Windows.Forms.DialogResult.OK) {
                var root = new DirectoryInfo(dlg.SelectedPath);
                RecDeleteFilesDublicate(root);
                MessageBox.Show(string.Format("Deleted {0} files.", listLB.Items.Count));
                listLB.Items.Clear();
            } else
                MessageBox.Show("select the folder");
        }

        private void ShowConflictedFiles_Click(object sender, RoutedEventArgs e) {
            if (result == System.Windows.Forms.DialogResult.OK) {
                listLB.Items.Clear();
                var root = new DirectoryInfo(dlg.SelectedPath);
                RecShowConflictedFiles(root);
                MessageBox.Show(string.Format("Conflicted {0} files.", listLB.Items.Count));
            } else
                MessageBox.Show("select the folder");
        }

        void RecShowConflictedFiles(DirectoryInfo root) {
            var files = root.GetFiles();
            pathLabel.Content = root.FullName;
            var ff = files.Where(a => a.Name.Contains("conflicted copy by")).ToArray();
            foreach (var a in ff) {
                listLB.Items.Add("Conflicted " + a.FullName);
                listLB.UpdateLayout();
            }
            foreach (var folder in root.GetDirectories())
                RecShowConflictedFiles(folder);
        }

        private void DeleteConflictedFiles_Click(object sender, RoutedEventArgs e) {
            // (conflicted copy by 
            if (result == System.Windows.Forms.DialogResult.OK) {
                listLB.Items.Clear();
                var root = new DirectoryInfo(dlg.SelectedPath);
                RecDeleteConflictedFiles(root);
                MessageBox.Show(string.Format("Deleted conflicted {0} files.", listLB.Items.Count));
            } else
                MessageBox.Show("select the folder");
        }

        void RecDeleteConflictedFiles(DirectoryInfo root) {
            var files = root.GetFiles();
            pathLabel.Content = root.FullName;
            var ff = files.Where(a => a.Name.Contains("(conflicted copy by ")).ToArray();
            foreach (var a in ff) {
                var source = new FileInfo(a.DirectoryName + "\\" + a.Name.Substring(0, a.Name.IndexOf("(conflicted copy by ")) + a.Extension);
                if (source.Exists) {
                    listLB.Items.Add("Deleted " + a.FullName);
                    listLB.UpdateLayout();
                    a.Delete();
                } else {
                    File.Move(a.FullName, source.FullName);
                    listLB.Items.Add("Rename " + a.FullName + " to " + source.FullName);
                }
            }
            var dirs = root.GetDirectories();
            var dd = dirs.Where(a => a.Name.Contains("(conflicted copy by ")).ToArray();
            foreach (var a in dd) {
                var source = new DirectoryInfo(a.FullName.Substring(0, a.FullName.IndexOf("(conflicted copy by ")));
                if (source.Exists) {
                    listLB.Items.Add("Deleted " + a.FullName);
                    listLB.UpdateLayout();
                    a.Delete(true);
                } else {
                    Directory.Move(a.FullName, source.FullName);
                    listLB.Items.Add("Rename " + a.FullName + " to " + source.FullName);
                }
            }
            foreach (var folder in root.GetDirectories())
                RecDeleteConflictedFiles(folder);
        }
    }
}
