using SparseMatrixAnalysis.Plots;
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

using SparseMatrixAnalysis.Tests;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace SparseMatrixAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Results.Initialize();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            Results.runs++;
            Results.categoryAxis.Labels.Add("Матрица " + Results.runs);

            Console.WriteLine($"\n################ Run {Results.runs} ################\n");

            MainTest.Run(fileTextBox.Text);
            NumericsTest.Run(fileTextBox.Text);

            if (!Results.isShown)
            {
                Results.barplot.Show();
                Results.isShown = true;
            }
            Results.perfomanceComparisonBarView.PerfomanceComparisonModel.InvalidatePlot(true);
        }

        private void FileChooser_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "TXT Files (*.txt)|*.txt";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                fileTextBox.Text = filename;
            }
        }
    }
}
