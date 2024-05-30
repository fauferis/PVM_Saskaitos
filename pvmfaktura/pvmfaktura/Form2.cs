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
using BCrypt.Net;


namespace pvmfaktura
{
    public partial class Form2 : Form
    {
        string connectionString = @"Data Source=localhost;Initial Catalog=pvmfakt;User ID=sa;Password=sa";
        public Form2()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(textBox2.Text);
            string query = "INSERT INTO vartotojas (vart_vard, slapt) VALUES (@Username, @PasswordHash)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    try
                    {
                        
                        connection.Open();

                        
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Vartotojas pridetas sekmingai");
                        }
                        else
                        {
                            MessageBox.Show("Insertion failed.");
                        }
                    }
                    catch (Exception ex)
                    {
                      
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
            }
        }
    }
}
