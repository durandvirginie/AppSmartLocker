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
            if(this.isControlled)
            {
                SqlConnection AppSmartCo = new SqlConnection(this.ConnectionString);
                string requete = "UPDATE TempsDefini SET [Duree_blocage] = " + dureeBlocage.Value + ", [Lundi] = "+ dureeLundi.Value + 
                    ", [Lundi_actif] = '" + ActifLundi.Checked+"', [Mardi] = "+ dureeMardi.Value+", [Mardi_actif] = '"+ ActifMardi.Checked+"', [Mercredi] = "+dureeMercredi.Value+
                    ", [Mercredi_actif] = '"+ ActifMercredi.Checked +"', [Jeudi] = "+ dureeJeudi.Value +", [Jeudi_actif] = '"+ActifJeudi.Checked+"', [Vendredi] = "+dureeVendredi.Value+
                    ", [Vendredi_actif] = '"+ActifVendredi.Checked +"', [Samedi] = "+dureeSamedi.Value+", [Samedi_actif]= '"+ActifSamedi.Checked+"',[Dimanche] = "+dureeDimanche.Value+
                    ", [Dimanche_actif] = '"+ActifDimanche.Checked+ "' where [Id_app] = (SELECT id_app FROM ApplicationControlable WHERE Nom_app = '" + selectedApp + "')";
                SqlCommand command2 = new SqlCommand(requete, AppSmartCo);
                AppSmartCo.Open();
                command2.ExecuteNonQuery();
                AppSmartCo.Close();


                //retour à l'accueil :
                AppSmartLockAccueil Accueil = new AppSmartLockAccueil();
                Accueil.Show();
                this.Hide();
            } else
            {
                //insert into ApplicationControllee + Controllable + TempsDefini
            }

        }
    }
}
