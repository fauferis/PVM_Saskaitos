using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Transactions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace pvmfaktura
{

    public partial class Form3 : Form
    {
        string connectionString = @"Data Source=localhost;Initial Catalog=pvmfakt;User ID=sa;Password=sa";

        private Dictionary<string, int> miestasTosalisMap = new Dictionary<string, int>();
        private Dictionary<int, string> salisIdToNameMap = new Dictionary<int, string>();

        public Form3()
        {
            InitializeComponent();
            comboBox1.SelectedIndexChanged += new EventHandler(comboBox1_SelectedIndexChanged);
            comboBox2.SelectedIndexChanged += new EventHandler(comboBox2_SelectedIndexChanged);
            LoadComboBoxData();
            LoadComboBoxData2();
            LoadComboBoxData6();
            LoadCheckedListBoxData();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    string queryMiestas = "SELECT id, pavadinimas, salis_ID FROM miestas";
                    string querySalis = "SELECT id, pavadinimas FROM salis";

                    using (SqlCommand cmdMiestas = new SqlCommand(queryMiestas, cnn))
                    {
                        using (SqlDataReader reader = cmdMiestas.ExecuteReader())
                        {
                            comboBox1.Items.Clear();
                            miestasTosalisMap.Clear();

                            while (reader.Read())
                            {
                                string miestasPav = reader["pavadinimas"].ToString();
                                int salisId = Convert.ToInt32(reader["salis_ID"]);
                                miestasTosalisMap[miestasPav] = salisId;
                            }
                        }
                    }


                    using (SqlCommand cmdSalis = new SqlCommand(querySalis, cnn))
                    {
                        using (SqlDataReader reader = cmdSalis.ExecuteReader())
                        {
                            comboBox2.Items.Clear();
                            salisIdToNameMap.Clear();

                            while (reader.Read())
                            {
                                int salisId = Convert.ToInt32(reader["id"]);
                                string salisPav = reader["pavadinimas"].ToString();
                                salisIdToNameMap[salisId] = salisPav;
                                comboBox2.Items.Add(salisPav);
                            }
                        }
                    }

                    foreach (var miestas in miestasTosalisMap.Keys)
                    {
                        comboBox1.Items.Add(miestas);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                string selectedSalis = comboBox2.SelectedItem.ToString();
                int selectedSalisId = salisIdToNameMap.FirstOrDefault(x => x.Value == selectedSalis).Key;
                PopulateComboBox1BySalis(selectedSalisId);
            }
        }

        private void PopulateComboBox1BySalis(int salisId)
        {
            try
            {
                comboBox1.Items.Clear();
                foreach (var miestas in miestasTosalisMap)
                {
                    if (miestas.Value == salisId)
                    {
                        comboBox1.Items.Add(miestas.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();


                    string selectedMiestas = comboBox1.SelectedItem?.ToString();
                    if (string.IsNullOrEmpty(selectedMiestas))
                    {
                        MessageBox.Show("Prašome pasirinkti miestą.", "Klaida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string query1 = "SELECT id FROM miestas WHERE pavadinimas = @miestasPavadinimas";
                    int miestasId;

                    using (SqlCommand cmd = new SqlCommand(query1, cnn))
                    {
                        cmd.Parameters.AddWithValue("@miestasPavadinimas", selectedMiestas);
                        object result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out miestasId))
                        {

                        }
                        else
                        {
                            MessageBox.Show("Nepavyko rasti pasirinkto miesto ID.", "Klaida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }


                    string query = "INSERT INTO pirkejas (imones_pav, miestas_ID, gatve, PVM_kod, imones_kod, tel_nr) VALUES (@imones_pav, @miestas_ID, @gatve, @PVM_kod, @imones_kod, @tel_nr)";

                    using (SqlCommand insertSQL = new SqlCommand(query, cnn))
                    {
                        insertSQL.Parameters.AddWithValue("@imones_pav", textBox1.Text);
                        insertSQL.Parameters.AddWithValue("@miestas_ID", miestasId);
                        insertSQL.Parameters.AddWithValue("@gatve", textBox4.Text);
                        insertSQL.Parameters.AddWithValue("@PVM_kod", textBox5.Text);
                        insertSQL.Parameters.AddWithValue("@imones_kod", textBox6.Text);
                        insertSQL.Parameters.AddWithValue("@tel_nr", textBox7.Text);

                        insertSQL.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Duomenys sekmingai irasyti!", "Pavyko", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "select imones_pav, miestas.pavadinimas as miestas, salis.pavadinimas as salis, gatve, PVM_kod, imones_kod, tel_nr from pirkejas inner join miestas on pirkejas.miestas_ID = miestas.id inner join salis on miestas.salis_ID = salis.id";
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "pirkejas");
                    dataGridView1.DataSource = ds.Tables["pirkejas"].DefaultView;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM pirkejas WHERE imones_kod like '%" + textBox2.Text + "%'";
                    using (SqlCommand deleteSQL = new SqlCommand(query, cnn))
                    {
                        deleteSQL.Parameters.Add("@imones_kod", SqlDbType.NChar).Value = textBox6.Text;

                        cnn.Open();
                        deleteSQL.ExecuteNonQuery();
                        button2.PerformClick();
                    }
                }

                MessageBox.Show("Duomenys sekmingai istrinti!", "Pavyko", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO prekes (pavadinimas, mat_vnt, kaina) VALUES (@pavadinimas, @mat_vnt, @kaina)";
                    using (SqlCommand insertSQL = new SqlCommand(query, cnn))
                    {
                        insertSQL.Parameters.Add("@pavadinimas", SqlDbType.NVarChar).Value = textBox3.Text;
                        insertSQL.Parameters.Add("@mat_vnt", SqlDbType.NVarChar).Value = textBox8.Text;
                        insertSQL.Parameters.Add("@kaina", SqlDbType.Money).Value = textBox9.Text;


                        cnn.Open();
                        insertSQL.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Duomenys sekmingai irasyti!", "Pavyko", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "SELECT id as prekes_numeris_sarase, pavadinimas, mat_vnt as matavimo_vienetas, kaina as kaina_e FROM prekes";
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "prekes");
                    dataGridView1.DataSource = ds.Tables["prekes"].DefaultView;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM prekes WHERE id = @id";
                    using (SqlCommand deleteSQL = new SqlCommand(query, cnn))
                    {
                        deleteSQL.Parameters.Add("@id", SqlDbType.Int).Value = textBox10.Text;

                        cnn.Open();
                        deleteSQL.ExecuteNonQuery();
                        button5.PerformClick();
                    }
                }

                MessageBox.Show("Duomenys sekmingai istrinti!", "Pavyko", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "SELECT imones_pav as imones_pavadinimas, adresas, PVM_kod, imones_kod, banko_sask as banko_saskaita, tel_nr as telefono_numeris FROM pardavejas";
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "pardavejas");
                    dataGridView1.DataSource = ds.Tables["pardavejas"].DefaultView;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadComboBoxData()
        {
            string query = "SELECT id, imones_pav FROM pirkejas";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    comboBox3.DisplayMember = "imones_pav";
                    comboBox3.ValueMember = "id";
                    comboBox3.DataSource = dataTable;

                    comboBox3.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void LoadCheckedListBoxData()
        {
            string query = "SELECT id, pavadinimas FROM prekes";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int prekesId = Convert.ToInt32(reader["id"]);
                        string prekesPavadinimas = reader["pavadinimas"].ToString();
                        checkedListBox1.Items.Add(prekesPavadinimas, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void LoadComboBoxData2()
        {
            string query = "SELECT id, reiksme FROM PVM";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    comboBox5.DisplayMember = "reiksme";
                    comboBox5.ValueMember = "id";
                    comboBox5.DataSource = dataTable;

                    comboBox5.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void LoadComboBoxData6()
        {
            string query = "SELECT id, imones_pav FROM pardavejas";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    comboBox6.DisplayMember = "imones_pav";
                    comboBox6.ValueMember = "id";
                    comboBox6.DataSource = dataTable;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }


        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();

                    if (comboBox5.SelectedValue == null || comboBox3.SelectedValue == null || comboBox6.SelectedValue == null)
                    {
                        MessageBox.Show("Prašome pasirinkti visus reikiamus laukus.", "Klaida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int reiksmeId = Convert.ToInt32(comboBox5.SelectedValue);
                    int imones_pavId = Convert.ToInt32(comboBox3.SelectedValue);
                    int pard_imones_pavId = Convert.ToInt32(comboBox6.SelectedValue);

                    string querypirk = "INSERT INTO pirkimas (pardavejas_ID, pirkejas_ID) OUTPUT INSERTED.ID VALUES (@pardavejas_ID, @pirkejas_ID)";
                    int pirkimasID;

                    using (SqlCommand insertSQL = new SqlCommand(querypirk, cnn))
                    {
                        insertSQL.Parameters.AddWithValue("@pardavejas_ID", pard_imones_pavId);
                        insertSQL.Parameters.AddWithValue("@pirkejas_ID", imones_pavId);
                        pirkimasID = (int)insertSQL.ExecuteScalar();
                    }

                    string querydetal = "INSERT INTO pirkimo_detales (prekes_ID, kiekis, PVM_ID, data, saskaitos_ser, pirkimas_ID) VALUES (@prekes_ID, @kiekis, @PVM_ID, @data, @saskaitos_ser, @pirkimas_ID)";

                    foreach (var checkedItem in checkedListBox1.CheckedItems)
                    {
                        using (SqlCommand insertSQL = new SqlCommand(querydetal, cnn))
                        {
                            string selectedPreke = checkedItem.ToString();
                            int prekesId;
                            string queryPrekesId = "SELECT id FROM prekes WHERE pavadinimas = @pavadinimas";
                            using (SqlCommand cmd = new SqlCommand(queryPrekesId, cnn))
                            {
                                cmd.Parameters.AddWithValue("@pavadinimas", selectedPreke);
                                prekesId = (int)cmd.ExecuteScalar();
                            }

                            insertSQL.Parameters.AddWithValue("@prekes_ID", prekesId);
                            insertSQL.Parameters.AddWithValue("@kiekis", Convert.ToInt32(textBox11.Text));
                            insertSQL.Parameters.AddWithValue("@PVM_ID", reiksmeId);
                            insertSQL.Parameters.AddWithValue("@data", dateTimePicker1.Value.Date);
                            insertSQL.Parameters.AddWithValue("@saskaitos_ser", textBox12.Text);
                            insertSQL.Parameters.AddWithValue("@pirkimas_ID", pirkimasID);

                            insertSQL.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Duomenys sekmingai irasyti!", "Pavyko", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "select pirkimo_detales.id, saskaitos_ser as saskaitos_serija, pirkimo_detales.data, pirkejas.imones_pav as pirkejo_imones_pavadinimas, pirkejas.imones_kod as pirkejos_imones_kodas, pirkejas.tel_nr as telefono_numeris, prekes.pavadinimas as prekes_pavadinimas, prekes.kaina, kiekis, PVM.reiksme as PVM from pirkimo_detales inner join pirkimas on pirkimo_detales.pirkimas_ID = pirkimas.id inner join pardavejas on pirkimas.pardavejas_ID = pardavejas.id inner join pirkejas on pirkimas.pirkejas_ID = pirkejas.id\r\ninner join PVM on pirkimo_detales.PVM_ID = PVM.id inner join prekes on pirkimo_detales.prekes_ID = prekes.id;";
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, "pirkimo_detales");
                    dataGridView1.DataSource = ds.Tables["pirkimo_detales"].DefaultView;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                string saskaitosSer = textBox13.Text.Trim();

                DataTable dataTable = GetPirkimoDetailsBySaskaitosSer(saskaitosSer);
                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Klaida nera duomenu.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Class2 calculator = new Class2();

                string directoryPath = "C:\\pdftest";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"Faktura_{saskaitosSer}.pdf");

                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        Document document = new Document(pdf);

                        Paragraph header = new Paragraph("PVM SASKAITA - FAKTURA")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20);
                        document.Add(header);
                        document.Add(new LineSeparator(new SolidLine()));

                        // Extract common invoice data from the first row
                        DataRow firstRow = dataTable.Rows[0];
                        string saskaitosSerija = firstRow["saskaitos_serija"].ToString();
                        string data = Convert.ToDateTime(firstRow["data"]).ToString("yyyy-MM-dd");
                        string pardavejoImonesPavadinimas = firstRow["pardavejo_imones_pavadinimas"].ToString();
                        string pardavejoImonesKodas = firstRow["imones_kod"].ToString();
                        string pardavejoAdresas = firstRow["adresas"].ToString();
                        string pardavejoPVMKod = firstRow["PVM_kod"].ToString();
                        string pardavejoBankoSask = firstRow["banko_sask"].ToString();
                        string pirkejoImonesPavadinimas = firstRow["pirkejo_imones_pavadinimas"].ToString();
                        string pirkejosImonesKodas = firstRow["pirkejos_imones_kodas"].ToString();
                        string telefonoNumeris = firstRow["telefono_numeris"].ToString();
                        string pirkejoGatve = firstRow["gatve"].ToString();
                        string miestasPavadinimas = firstRow["miestas_pavadinimas"].ToString();
                        string salisPavadinimas = firstRow["salis_pavadinimas"].ToString();

                        // Add common invoice data to the PDF
                        document.Add(new Paragraph($"Saskaitos Serija: {saskaitosSerija}"));
                        document.Add(new Paragraph($"Data: {data}"));
                        document.Add(new Paragraph($"Pardavejo Imones Pavadinimas: {pardavejoImonesPavadinimas}"));
                        document.Add(new Paragraph($"Pardavejo Imonės Kodas: {pardavejoImonesKodas}"));
                        document.Add(new Paragraph($"Pardavejo Adresas: {pardavejoAdresas}"));
                        document.Add(new Paragraph($"Pardavejo PVM Kodas: {pardavejoPVMKod}"));
                        document.Add(new Paragraph($"Pardavejo Banko Saskaita: {pardavejoBankoSask}"));
                        document.Add(new Paragraph($"Pirkejo Imones Pavadinimas: {pirkejoImonesPavadinimas}"));
                        document.Add(new Paragraph($"Pirkejo Imones Kodas: {pirkejosImonesKodas}"));
                        document.Add(new Paragraph($"Telefono Numeris: {telefonoNumeris}"));
                        document.Add(new Paragraph($"Pirkejo Gatve: {pirkejoGatve}"));
                        document.Add(new Paragraph($"Miestas: {miestasPavadinimas}"));
                        document.Add(new Paragraph($"Salis: {salisPavadinimas}"));
                        document.Add(new LineSeparator(new SolidLine()));

                        // Create table for items
                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1, 1, 1, 1 })).UseAllAvailableWidth();
                        table.AddHeaderCell("Prekes Pavadinimas");
                        table.AddHeaderCell("Kaina (be PVM)");
                        table.AddHeaderCell("Kiekis");
                        table.AddHeaderCell("PVM");
                        table.AddHeaderCell("Viso (su PVM)");

                        decimal totalWithoutPVM = 0;
                        decimal totalPVM = 0;
                        decimal totalWithPVM = 0;

                        foreach (DataRow row in dataTable.Rows)
                        {
                            string prekesPavadinimas = row["prekes_pavadinimas"].ToString();
                            decimal priceWithoutPVM = Convert.ToDecimal(row["kaina"]);
                            int kiekis = Convert.ToInt32(row["kiekis"]);
                            decimal pvmRate = Convert.ToDecimal(row["PVM"]);

                            decimal price = calculator.skaiciuoti(priceWithoutPVM, kiekis);
                            decimal pvm = calculator.CalculatePVM(priceWithoutPVM, pvmRate, kiekis);
                            decimal totalSum = calculator.CalculateTotal(priceWithoutPVM, pvm, kiekis);

                            totalWithoutPVM += price;
                            totalPVM += pvm;
                            totalWithPVM += totalSum;

                            table.AddCell(new Paragraph(prekesPavadinimas));
                            table.AddCell(new Paragraph(priceWithoutPVM.ToString("C")));
                            table.AddCell(new Paragraph(kiekis.ToString()));
                            table.AddCell(new Paragraph(pvm.ToString("C")));
                            table.AddCell(new Paragraph(totalSum.ToString("C")));
                        }

                        document.Add(table);

                        // Add totals
                        document.Add(new LineSeparator(new SolidLine()));
                        document.Add(new Paragraph($"Bendra suma (be PVM): {totalWithoutPVM:C}"));
                        document.Add(new Paragraph($"Bendra PVM suma: {totalPVM:C}"));
                        document.Add(new Paragraph($"Bendra suma (su PVM): {totalWithPVM:C}"));

                        // Page numbers
                        int n = pdf.GetNumberOfPages();
                        for (int i = 1; i <= n; i++)
                        {
                            document.ShowTextAligned(new Paragraph($"Page {i} of {n}"), 559, 806, i, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);
                        }

                        document.Close();
                    }
                }

                MessageBox.Show("PDF sugeneruotas sekmingai!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private DataTable GetPirkimoDetailsBySaskaitosSer(string saskaitosSer)
        {
            DataTable dataTable = new DataTable();
            string sql = @"SELECT saskaitos_ser AS saskaitos_serija, pirkimo_detales.data, pardavejas.imones_pav as pardavejo_imones_pavadinimas, 
                   pardavejas.imones_kod, pardavejas.adresas, pardavejas.PVM_kod, pardavejas.banko_sask, 
                   pirkejas.imones_pav AS pirkejo_imones_pavadinimas, pirkejas.imones_kod AS pirkejos_imones_kodas, 
                   pirkejas.tel_nr AS telefono_numeris, pirkejas.gatve, miestas.pavadinimas AS miestas_pavadinimas, 
                   salis.pavadinimas AS salis_pavadinimas, prekes.pavadinimas AS prekes_pavadinimas, 
                   prekes.kaina, kiekis, PVM.reiksme AS PVM 
                    FROM pirkimo_detales  
                    INNER JOIN pirkimas ON pirkimo_detales.pirkimas_ID = pirkimas.id  
                    INNER JOIN pardavejas ON pirkimas.pardavejas_ID = pardavejas.id  
                    INNER JOIN pirkejas ON pirkimas.pirkejas_ID = pirkejas.id  
                    INNER JOIN miestas ON pirkejas.miestas_ID = miestas.id
                    INNER JOIN salis ON miestas.salis_ID = salis.id
                    INNER JOIN PVM ON pirkimo_detales.PVM_ID = PVM.id  
                    INNER JOIN prekes ON pirkimo_detales.prekes_ID = prekes.id  
                    WHERE pirkimo_detales.saskaitos_ser = @saskaitosSer;";

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                da.SelectCommand.Parameters.AddWithValue("@saskaitosSer", saskaitosSer);
                da.Fill(dataTable);
            }

            return dataTable;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            InvoiceGenerator invoiceGenerator = new InvoiceGenerator();
            string invoiceNumber = invoiceGenerator.GenerateInvoiceNumber();
            textBox12.Text = invoiceNumber;
        }

        public class InvoiceGenerator
        {
            private const string connectionString = @"Data Source=localhost;Initial Catalog=pvmfakt;User ID=sa;Password=sa";
            private const string tableName = "serija";
            private const string prefix = "ABC";
            private const int numberLength = 5;

            public string GenerateInvoiceNumber()
            {
                string invoiceNumber = GetNextInvoiceNumber();
                StoreInvoiceNumberInDatabase(invoiceNumber);
                return invoiceNumber;
            }

            private string GetNextInvoiceNumber()
            {
                int nextNumber = 1;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = $"SELECT MAX(SUBSTRING(InvoiceNumber, 4, {numberLength})) FROM {tableName} WHERE InvoiceNumber LIKE '{prefix}%'";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            object result = command.ExecuteScalar();
                            if (result != DBNull.Value)
                            {
                                nextNumber = Convert.ToInt32(result) + 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error retrieving max invoice number from database: " + ex.Message);
                    }
                }

                return prefix + nextNumber.ToString().PadLeft(numberLength, '0');
            }

            private void StoreInvoiceNumberInDatabase(string invoiceNumber)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = $"INSERT INTO {tableName} (InvoiceNumber) VALUES (@InvoiceNumber)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error storing invoice number in database: " + ex.Message);
                    }
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a DataTable to hold all invoice details
                DataTable allInvoiceDetails = new DataTable();

                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"SELECT saskaitos_ser AS saskaitos_serija, pirkimo_detales.data, pardavejas.imones_pav as pardavejo_imones_pavadinimas, 
                    pardavejas.imones_kod, pardavejas.adresas, pardavejas.PVM_kod, pardavejas.banko_sask, 
                    pirkejas.imones_pav AS pirkejo_imones_pavadinimas, pirkejas.imones_kod AS pirkejos_imones_kodas, 
                    pirkejas.tel_nr AS telefono_numeris, pirkejas.gatve, miestas.pavadinimas AS miestas_pavadinimas, 
                    salis.pavadinimas AS salis_pavadinimas, prekes.pavadinimas AS prekes_pavadinimas, 
                    prekes.kaina, kiekis, PVM.reiksme AS PVM 
                     FROM pirkimo_detales  
                     INNER JOIN pirkimas ON pirkimo_detales.pirkimas_ID = pirkimas.id  
                     INNER JOIN pardavejas ON pirkimas.pardavejas_ID = pardavejas.id  
                     INNER JOIN pirkejas ON pirkimas.pirkejas_ID = pirkejas.id  
                     INNER JOIN miestas ON pirkejas.miestas_ID = miestas.id
                     INNER JOIN salis ON miestas.salis_ID = salis.id
                     INNER JOIN PVM ON pirkimo_detales.PVM_ID = PVM.id  
                     INNER JOIN prekes ON pirkimo_detales.prekes_ID = prekes.id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(allInvoiceDetails);
                        }
                    }
                }

                // Ensure the directory exists
                string directoryPath = "C:\\pdftest";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Define the file path
                string filePath = Path.Combine(directoryPath, $"visos_fakturos.pdf");

                // Create PDF writer
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        Document document = new Document(pdf);

                        // Track current page number
                        int currentPageNumber = 1;

                        // Group rows by 'saskaitos_serija'
                        var groupedRows = allInvoiceDetails.AsEnumerable()
                            .GroupBy(row => row["saskaitos_serija"].ToString());

                        foreach (var group in groupedRows)
                        {
                            // Start a new page for each invoice
                            if (currentPageNumber > 1)
                            {
                                pdf.AddNewPage();
                            }

                            var firstRow = group.First();
                            Class2 calculator = new Class2();

                            // Extract data from the first row of the group
                            string saskaitosSerija = firstRow["saskaitos_serija"].ToString();
                            string data = Convert.ToDateTime(firstRow["data"]).ToString("yyyy-MM-dd");
                            string pirkejoImonesPavadinimas = firstRow["pirkejo_imones_pavadinimas"].ToString();
                            string pirkejosImonesKodas = firstRow["pirkejos_imones_kodas"].ToString();
                            string telefonoNumeris = firstRow["telefono_numeris"].ToString();
                            string pirkejoGatve = firstRow["gatve"].ToString();
                            string miestasPavadinimas = firstRow["miestas_pavadinimas"].ToString();
                            string salisPavadinimas = firstRow["salis_pavadinimas"].ToString();

                            // Add data to the PDF
                            document.Add(new Paragraph($"Saskaitos Serija: {saskaitosSerija}"));
                            document.Add(new Paragraph($"Data: {data}"));
                            document.Add(new Paragraph($"Pirkejo Imones Pavadinimas: {pirkejoImonesPavadinimas}"));
                            document.Add(new Paragraph($"Pirkejo Imones Kodas: {pirkejosImonesKodas}"));
                            document.Add(new Paragraph($"Telefono Numeris: {telefonoNumeris}"));
                            document.Add(new Paragraph($"Pirkejo Gatve: {pirkejoGatve}"));
                            document.Add(new Paragraph($"Miestas: {miestasPavadinimas}"));
                            document.Add(new Paragraph($"Salis: {salisPavadinimas}"));

                            // Create table for items
                            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1, 1, 1, 1 })).UseAllAvailableWidth();
                            table.AddHeaderCell("Prekes Pavadinimas");
                            table.AddHeaderCell("Kaina (be PVM)");
                            table.AddHeaderCell("Kiekis");
                            table.AddHeaderCell("PVM");
                            table.AddHeaderCell("Viso (su PVM)");

                            decimal totalWithoutPVM = 0;
                            decimal totalPVM = 0;
                            decimal totalWithPVM = 0;

                            foreach (var row in group)
                            {
                                string prekesPavadinimas = row["prekes_pavadinimas"].ToString();
                                decimal priceWithoutPVM = Convert.ToDecimal(row["kaina"]);
                                int kiekis = Convert.ToInt32(row["kiekis"]);
                                decimal pvmRate = Convert.ToDecimal(row["PVM"]);

                                
                                decimal price = calculator.skaiciuoti(priceWithoutPVM, kiekis);
                                decimal pvm = calculator.CalculatePVM(priceWithoutPVM, pvmRate, kiekis);
                                decimal totalSum = calculator.CalculateTotal(priceWithoutPVM, pvm, kiekis);

                                totalWithoutPVM += price;
                                totalPVM += pvm;
                                totalWithPVM += totalSum;

                                table.AddCell(new Paragraph(prekesPavadinimas));
                                table.AddCell(new Paragraph(priceWithoutPVM.ToString("C")));
                                table.AddCell(new Paragraph(kiekis.ToString()));
                                table.AddCell(new Paragraph(pvm.ToString("C")));
                                table.AddCell(new Paragraph(totalSum.ToString("C")));
                            }

                            // Add the table to the document
                            document.Add(table);

                            // Add totals to the document
                            document.Add(new Paragraph($"Viso be PVM: {totalWithoutPVM:C}"));
                            document.Add(new Paragraph($"PVM suma: {totalPVM:C}"));
                            document.Add(new Paragraph($"Viso su PVM: {totalWithPVM:C}"));

                            // Add a line separator
                            document.Add(new LineSeparator(new SolidLine()));

                            // Add page number
                            document.ShowTextAligned(new Paragraph($"Page {currentPageNumber} of {groupedRows.Count()}"), 559, 806, TextAlignment.RIGHT);

                            // Increment current page number
                            currentPageNumber++;
                        }
                    }
                }

                MessageBox.Show("Suformatuotas PDF!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating all invoices document: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



    }
}
