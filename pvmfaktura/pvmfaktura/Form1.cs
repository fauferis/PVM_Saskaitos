using System;
using System.Data;
using System.Data.SqlClient;
using BCrypt.Net;

namespace pvmfaktura
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=localhost;Initial Catalog=pvmfakt;User ID=sa;Password=sa";
        public Form1()
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

            if (VerifyLogin(username, password))
                {
                    MessageBox.Show("Prisijungta sekmingai.");
                    this.Hide();
                    Form3 form3 = new Form3();
                    form3.ShowDialog();
            }
            else
                {
                    MessageBox.Show("Invalid username or password.");
                }
        }

        private bool VerifyLogin(string username, string password)
        {
            string query = "SELECT slapt FROM vartotojas WHERE vart_vard = @Username";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string storedHash = result.ToString();

                            // lyginamas slaptazodis su hash slaptazodziu naudojant idiegta biblioteka bcrypt.net
                            return BCrypt.Net.BCrypt.Verify(password, storedHash);
                        }
                        else
                        {
                            // vartotojas nerastas
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    MessageBox.Show("Prie duomenu bazes prisijungta sekmingai!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaþkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }
    }
}
