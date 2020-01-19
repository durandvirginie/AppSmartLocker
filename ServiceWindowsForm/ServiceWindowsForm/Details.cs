using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceWindowsForm
{
    public partial class Details : Form
    {
        private String selectedApp;
        private String ConnectionString;
        private Boolean isControlled;
        public Details(String selectApp, String coString)
        {
            InitializeComponent();
            this.selectedApp = selectApp;
            this.ConnectionString = coString;
            label1.Text = "Détails du controle de " + selectApp;
        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AppSmartLockAccueil Accueil = new AppSmartLockAccueil();
            Accueil.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Details_Load(object sender, EventArgs e)
        {
            string requete = "SELECT * FROM TempsDefini tps WHERE tps.Id_app = (SELECT Id_app FROM ApplicationControlable WHERE Nom_app = '" + selectedApp + "')";
            // Création d'un objet connexion en se basant sur la chaine de connexion.

            SqlConnection AppSmartConnection = new SqlConnection(this.ConnectionString);

            // Création d'un objet commande basé sur la requête reçue en paramètre.
            SqlCommand command = new SqlCommand(requete, AppSmartConnection);
            AppSmartConnection.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    dureeBlocage.Value = reader.GetInt32(1);
                    dureeLundi.Value = reader.GetInt32(2);
                    ActifLundi.Checked = reader.GetBoolean(3);
                    dureeMardi.Value = reader.GetInt32(4);
                    ActifMardi.Checked = reader.GetBoolean(5);
                    dureeMercredi.Value = reader.GetInt32(6);
                    ActifMercredi.Checked = reader.GetBoolean(7);
                    dureeJeudi.Value = reader.GetInt32(8);
                    ActifJeudi.Checked = reader.GetBoolean(9);
                    dureeVendredi.Value = reader.GetInt32(10);
                    ActifVendredi.Checked = reader.GetBoolean(11);
                    dureeSamedi.Value = reader.GetInt32(12);
                    ActifSamedi.Checked = reader.GetBoolean(13);
                    dureeDimanche.Value = reader.GetInt32(14);
                    ActifDimanche.Checked = reader.GetBoolean(15);
                    this.isControlled = true;
                }
            }
            else
            {
                this.isControlled = false;
                Console.WriteLine("No rows found.");
            }
            reader.Close();
            AppSmartConnection.Close();

        }

        //changement à faire dans la bdd en fonction des nouveaux éléments :
        private void save_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //si l'application est déjà controllée et connue de notre BDD, on met à jour les valeurs :
            if (this.isControlled)
            {
                SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
                string requete = "UPDATE TempsDefini SET [Duree_blocage] = " + dureeBlocage.Value + ", [Lundi] = " + dureeLundi.Value +
                    ", [Lundi_actif] = '" + ActifLundi.Checked + "', [Mardi] = " + dureeMardi.Value + ", [Mardi_actif] = '" + ActifMardi.Checked + "', [Mercredi] = " + dureeMercredi.Value +
                    ", [Mercredi_actif] = '" + ActifMercredi.Checked + "', [Jeudi] = " + dureeJeudi.Value + ", [Jeudi_actif] = '" + ActifJeudi.Checked + "', [Vendredi] = " + dureeVendredi.Value +
                    ", [Vendredi_actif] = '" + ActifVendredi.Checked + "', [Samedi] = " + dureeSamedi.Value + ", [Samedi_actif]= '" + ActifSamedi.Checked + "',[Dimanche] = " + dureeDimanche.Value +
                    ", [Dimanche_actif] = '" + ActifDimanche.Checked + "' where [Id_app] = (SELECT id_app FROM ApplicationControlable WHERE Nom_app = '" + selectedApp + "')";
                SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();


                //retour à l'accueil :
                AppSmartLockAccueil Accueil = new AppSmartLockAccueil();
                Accueil.Show();
                this.Hide();
            }
            // il faut créer l'application dans toutes les tables :
            else
            {

                insertAppControlable(this.selectedApp);
                insertAppControlee(this.selectedApp);
                insertTempsDefini(this.selectedApp);
                //retour à l'accueil :
                AppSmartLockAccueil Accueil = new AppSmartLockAccueil();
                Accueil.Show();
                this.Hide();
            }

        }
        private void insertAppControlable(string nomApp)
        {
            SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);

            string requete = "INSERT INTO ApplicationControlable (Id_app, Nom_app) values (@id, @nomApp)";
            SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
            command2.Parameters.AddWithValue("@id", getSizeApp()+1);
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
            command2.Parameters.AddWithValue("@dureeBlocage", dureeBlocage.Value);
            command2.Parameters.AddWithValue("@dureeLundi", dureeLundi.Value);
            command2.Parameters.AddWithValue("@ActifLundi", ActifLundi.Checked);
            command2.Parameters.AddWithValue("@dureeMardi", dureeMardi.Value);
            command2.Parameters.AddWithValue("@ActifMardi", ActifMardi.Checked);
            command2.Parameters.AddWithValue("@dureeMercredi", dureeMercredi.Value);
            command2.Parameters.AddWithValue("@ActifMercredi", ActifMercredi.Checked);
            command2.Parameters.AddWithValue("@dureeJeudi", dureeJeudi.Value);
            command2.Parameters.AddWithValue("@ActifJeudi", ActifJeudi.Checked);
            command2.Parameters.AddWithValue("@dureeVendredi", dureeVendredi.Value);
            command2.Parameters.AddWithValue("@ActifVendredi", ActifVendredi.Checked);
            command2.Parameters.AddWithValue("@dureeSamedi", dureeSamedi.Value);
            command2.Parameters.AddWithValue("@ActifSamedi", ActifSamedi.Checked);
            command2.Parameters.AddWithValue("@dureeDimanche", dureeDimanche.Value);
            command2.Parameters.AddWithValue("@ActifDimanche", ActifDimanche.Checked);

            AppSmartCo.Open();
            command2.ExecuteNonQuery();
            AppSmartCo.Close();
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

        //si l'admin veut supprimer les parametres de controle, on supprime les données dessus présentes dans la BDD :
        private void delete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.isControlled)
            {
                SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);

                //suppression dans la table temps :
                string requete = "DELETE FROM TempsDefini where [Id_app] = "+getIdApp(this.selectedApp);
                SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();

                //suppression dans la table ApplicationControlee :
                requete = "DELETE FROM ApplicationControlee where [Id_app] = " + getIdApp(this.selectedApp);
                command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();

                //suppression dans la table ApplicationControlable :
                requete = "DELETE FROM ApplicationControlable where [Nom_app] = '" + this.selectedApp+"'";
                command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();

                //retour à l'accueil :
                AppSmartLockAccueil Accueil = new AppSmartLockAccueil();
                Accueil.Show();
                this.Hide();
            }
        }
    }
}
