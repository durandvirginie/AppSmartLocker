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
        private List<String> appControlled;
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

            //initialise la liste des applications controlées :
            getApplicationControlled(this.ConnectionString);

            // Remplir la DataGridView
            RemplirLesProcess();

            
        }

        private void RemplirLesProcess()
        {
            // Example list.
            List<object[]> list = new List<object[]>();
            List<String> processN = new List<string>();
            foreach (Process p in Process.GetProcesses())
            {
                if (!processN.Contains(p.ProcessName))
                {
                    processN.Add(p.ProcessName);
                    list.Add(new Object[] { this.appControlled.Contains(p.ProcessName), p.ProcessName, "Details des parametres" });
                }
            }

            // Convert to DataTable.
            DataTable table = ConvertListToDataTable(list);
            dataGridView1.DataSource = table;

        }
       

        static DataTable ConvertListToDataTable(List<object[]> list)
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
            table.Columns.Add("Controlé ?", typeof(Boolean));
            table.Columns.Add("Nom du processus ");
            table.Columns.Add("Details ");

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
                if (this.appControlled.Contains(senderGrid.Rows[e.RowIndex].Cells[1].Value.ToString())) {
                    UpdateControlledApp(senderGrid.Rows[e.RowIndex].Cells[1].Value.ToString());
                    //réinitialise la liste des applications controlées :
                    getApplicationControlled(this.ConnectionString);
                } else
                {
                    UpdateNonControlledApp(senderGrid.Rows[e.RowIndex].Cells[1].Value.ToString());
                    //réinitialise la liste des applications controlées :
                    getApplicationControlled(this.ConnectionString);
                }

            } else if (e.ColumnIndex == 2){
                appSelect = senderGrid.Rows[e.RowIndex].Cells[1].Value.ToString();
                Details Details = new Details(appSelect, ConnectionString);
                Details.Show();
                this.Hide();
            }
        }

        //Défini la liste des noms des applications controllées en fonction de l'état du controle :
        private void getApplicationControlled(string conn)
        {
            this.appControlled = new List<string>();

            SqlConnection AppSmartConnection = new SqlConnection(conn);
            AppSmartConnection.Open();
            String request = "SELECT Nom_app FROM [dbo].[ApplicationControlable] ac LEFT JOIN ApplicationControlee ap ON ap.Id_app = ac.Id_app WHERE ap.Est_actif = 1 ";
            // Création d'un objet commande basé sur la requête reçue en paramètre.
            SqlCommand command = new SqlCommand(request, AppSmartConnection);
            
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    this.appControlled.Add(reader.GetString(0));
                }
            }
            reader.Close();
            AppSmartConnection.Close();
        }

        private void UpdateControlledApp(string nomApp)
        {
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
            string requete = "UPDATE ApplicationControlee SET [Est_actif] = 'false' WHERE [Id_app] = (SELECT Id_app FROM ApplicationControlable WHERE Nom_app = '"+nomApp+"')";
            SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
            AppSmartCo.Open();
            command2.ExecuteNonQuery();
            AppSmartCo.Close();
        }

        private void UpdateNonControlledApp(string nomApp)
        {
            int id = -1;
            string requete = "Select Id_app FROM ApplicationControlable WHERE Nom_app = '" + nomApp + "'";

            // Création d'un objet connexion en se basant sur la chaine de connexion.
            SqlConnection AppSmartConnection = new SqlConnection(this.ConnectionString);

            // Création d'un objet commande basé sur la requête reçue en paramètre.
            SqlCommand command = new SqlCommand(requete, AppSmartConnection);
            AppSmartConnection.Open();
            try
            {
                id = (Int32)command.ExecuteScalar();
            }
            catch { }
            AppSmartConnection.Close();
            //si l'id n'est pas dans la BDD, on doit inséré l'application dans nos 3 tables de controles :
            if (id == -1)
            {
                insertAppControlable(nomApp);
                insertAppControlee(nomApp);
                insertTempsDefini(nomApp);
            } else
            //si l'application est déjà controllée et connue de notre BDD, on met à jour les valeurs :
            {
                SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
                requete = "UPDATE ApplicationControlee SET [Est_actif] = 'true' WHERE [Id_app] = (SELECT Id_app FROM ApplicationControlable WHERE Nom_app = '" + nomApp + "')";
                SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();
            }
        }

        private void insertAppControlable(string nomApp)
        {
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);

            string requete = "INSERT INTO ApplicationControlable (Id_app, Nom_app) values (@id, @nomApp)";
            SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
            command2.Parameters.AddWithValue("@id", getSizeApp() + 1);
            command2.Parameters.AddWithValue("@nomApp", nomApp);
            AppSmartCo.Open();
            command2.ExecuteNonQuery();
            AppSmartCo.Close();
        }

        private void insertAppControlee(string nomApp)
        {
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);

            string requete = "INSERT INTO ApplicationControlee (Id_app, Est_actif, Tps_exe_restant, Tps_limite_atteinte) values (@idApp, @Actif,@tps, @tps_limite)";
            SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
            command2.Parameters.AddWithValue("@idApp", getIdApp(nomApp));
            command2.Parameters.AddWithValue("@Actif", true);
            command2.Parameters.AddWithValue("@tps", -1);
            command2.Parameters.AddWithValue("@tps_limite", new DateTime(1754, 1, 1, 0, 0, 0));

            AppSmartCo.Open();
            command2.ExecuteNonQuery();
            AppSmartCo.Close();
        }

        private void insertTempsDefini(string nomApp)
        {
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);

            string requete = "INSERT INTO TempsDefini (Id_app, Duree_blocage, Lundi, Lundi_actif,Mardi, Mardi_actif, Mercredi, Mercredi_actif, Jeudi, Jeudi_actif,Vendredi, Vendredi_actif, Samedi, Samedi_actif, Dimanche, Dimanche_actif)" +
                " values (@id, @dureeBlocage, @dureeLundi,@ActifLundi, @dureeMardi, @ActifMardi, @dureeMercredi, @ActifMercredi,@dureeJeudi, @ActifJeudi, @dureeVendredi, @ActifVendredi, @dureeSamedi, @ActifSamedi, @dureeDimanche, @ActifDimanche)";
            SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
            command2.Parameters.AddWithValue("@id", getIdApp(nomApp));
            command2.Parameters.AddWithValue("@dureeBlocage", 0);
            command2.Parameters.AddWithValue("@dureeLundi", 0);
            command2.Parameters.AddWithValue("@ActifLundi", 0);
            command2.Parameters.AddWithValue("@dureeMardi", 0);
            command2.Parameters.AddWithValue("@ActifMardi", 0);
            command2.Parameters.AddWithValue("@dureeMercredi", 0);
            command2.Parameters.AddWithValue("@ActifMercredi", 0);
            command2.Parameters.AddWithValue("@dureeJeudi", 0);
            command2.Parameters.AddWithValue("@ActifJeudi", 0);
            command2.Parameters.AddWithValue("@dureeVendredi", 0);
            command2.Parameters.AddWithValue("@ActifVendredi", 0);
            command2.Parameters.AddWithValue("@dureeSamedi", 0);
            command2.Parameters.AddWithValue("@ActifSamedi", 0);
            command2.Parameters.AddWithValue("@dureeDimanche", 0);
            command2.Parameters.AddWithValue("@ActifDimanche", 0);

            AppSmartCo.Open();
            command2.ExecuteNonQuery();
            AppSmartCo.Close();
        }

        //retourne la taille de la table AppControlable afin de définir un ID à la nouvelle application à controler
        private int getSizeApp()
        {
            string requete = "Select count(Id_app) FROM ApplicationControlable";
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
            SqlCommand newcommand = new SqlCommand(requete, AppSmartCo);
            AppSmartCo.Open();
            int id = (int)newcommand.ExecuteScalar();
            AppSmartCo.Close();

            return id;
        }

        private int getIdApp(String nomApp)
        {
            string requete = "Select Id_app FROM ApplicationControlable WHERE Nom_app = '" + nomApp + "'";
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
            SqlCommand newcommand = new SqlCommand(requete, AppSmartCo);
            AppSmartCo.Open();
            int id = (int)newcommand.ExecuteScalar();
            AppSmartCo.Close();

            return id;
        }
    }
}

