using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceWindowsForm
{
    public partial class AppSmartLockAccueil : Form
    {

        private string ConnectionString;
        private MenuStrip mainMenu;
        private string appSelect;
        public AppSmartLockAccueil()
        {
            InitializeComponent();
        }

        private void LectureConfiguration()
        {
            // Lecture de la dernière chaine de connexion utilisée
            ConnectionStringSettings oCnxCfg = ConfigurationManager.ConnectionStrings["DerniereChaine"];

            if (oCnxCfg != null)
            {
                this.ConnectionString = oCnxCfg.ConnectionString;
            }
            else
            {
                // Recherche de la chaine de connexion partielle à la base de données dans le App.config
                oCnxCfg = ConfigurationManager.ConnectionStrings["ConnexionAppSmartLocker"];

                if (oCnxCfg != null)
                {
                    SqlConnectionStringBuilder oCnxBldr = new SqlConnectionStringBuilder(oCnxCfg.ConnectionString);

                    if (string.IsNullOrEmpty(oCnxBldr.InitialCatalog))
                    {
                        oCnxBldr.InitialCatalog = "AppSmartLocker";
                    }

                    this.ConnectionString = oCnxBldr.ConnectionString;
                }
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Lecture de la configuration
            LectureConfiguration();

            // Remplir la DataGridView
            RemplirLesProcess();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void RemplirLesProcess()
        {
            // Example list.
            List<string[]> list = new List<string[]>();
            List<String> processN = new List<string>();
            foreach (Process p in Process.GetProcesses())
            {
                if (!processN.Contains(p.ProcessName))
                {
                    processN.Add(p.ProcessName);
                    list.Add(new string[] { p.ProcessName, "Details des parametres" });
                }
            }

            // Convert to DataTable.
            DataTable table = ConvertListToDataTable(list);
            dataGridView1.DataSource = table;

        }
       
        private void tabPage1_Click(object sender, EventArgs e)
        {
            
        }

        static DataTable ConvertListToDataTable(List<string[]> list)
        {
            // New table.
            DataTable table = new DataTable();

            // Get max columns.
            int columns = 0;
            foreach (var array in list)
            {
                if (array.Length > columns)
                {
                    columns = array.Length;
                }
            }

            // Add columns.
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }

            // Add rows.
            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.ColumnIndex == 0 &&
                e.RowIndex >= 0)
            {
                Console.WriteLine( senderGrid.Rows[e.RowIndex].Cells[0].ToString());
            } else if (e.ColumnIndex == 2){
                appSelect = senderGrid.Rows[e.RowIndex].Cells[1].Value.ToString();
                Console.WriteLine(appSelect);
                Details Details = new Details(appSelect, ConnectionString);
                Details.Show();
                this.Hide();
            }
        }

    }
}

