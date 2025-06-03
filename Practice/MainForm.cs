using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Practice
{
    public partial class MainForm : Form
    {
        private string connectionString = "";
        private DataSet dataSet = new DataSet();

        public MainForm()
        {
            InitializeComponent();

            // Настройка DataGridView из дизайнера
            dgvDoctors.ReadOnly = true;
            dgvDoctors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDoctors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDoctors.AllowUserToAddRows = false;
        }

        private void LoadTableList()
        {
            cmbTables.Items.Clear();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow row in schemaTable.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    if (!tableName.StartsWith("MSys") && !tableName.StartsWith("~"))
                    {
                        cmbTables.Items.Add(tableName);
                    }
                }
            }

            if (cmbTables.Items.Count > 0)
            {
                cmbTables.SelectedIndex = 0;
            }
        }

        private void cmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem == null) return;
            LoadTableData(cmbTables.SelectedItem.ToString());
        }

        private void LoadTableData(string tableName)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT * FROM [{tableName}]", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Используем dgvDoctors из дизайнера
                    dgvDoctors.DataSource = dt;

                    // Автонастройка ширины столбцов после загрузки данных
                    dgvDoctors.AutoResizeColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Access Databases (*.accdb, *.mdb)|*.accdb;*.mdb";
                openFileDialog.Title = "Выберите файл базы данных";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    string ext = System.IO.Path.GetExtension(filePath).ToLower();

                    if (ext == ".accdb")
                    {
                        connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
                    }
                    else if (ext == ".mdb")
                    {
                        connectionString = $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};";
                    }

                    try
                    {
                        using (OleDbConnection testConn = new OleDbConnection(connectionString))
                        {
                            testConn.Open();
                        }

                        LoadTableList();
                        btnCloseConnection.Enabled = true;
                        btnRefresh.Enabled = true;
                        btnAdd.Enabled = true;
                        btnEdit.Enabled = true;
                        btnDelete.Enabled = true;
                        MessageBox.Show("Подключение успешно установлено!");
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Ошибка подключения: {ex.Message}";
                        if (ex.Message.Contains("ACE.OLEDB"))
                        {
                            errorMsg += "\n\nУстановите Microsoft Access Database Engine:\n" +
                                        "https://www.microsoft.com/en-us/download/details.aspx?id=54920";
                        }
                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCloseConnection_Click_1(object sender, EventArgs e)
        {
            connectionString = "";
            dgvDoctors.DataSource = null;
            cmbTables.Items.Clear();
            btnCloseConnection.Enabled = false;
            btnRefresh.Enabled = false;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            MessageBox.Show("Соединение закрыто");
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem != null)
            {
                LoadTableData(cmbTables.SelectedItem.ToString());
            }
        }

        private void btnEdit_Click_1(object sender, EventArgs e)
        {
            if (dgvDoctors.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись!");
                return;
            }

            string tableName = cmbTables.SelectedItem?.ToString();
            if (tableName == "Врачи")
            {
                int id = Convert.ToInt32(dgvDoctors.CurrentRow.Cells["id_Врач"].Value);
                AddForm form = new AddForm(connectionString, id);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    btnRefresh.PerformClick();
                }
            }
            else
            {
                MessageBox.Show("Редактирование доступно только для врачей");
            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem == null) return;

            string tableName = cmbTables.SelectedItem.ToString();
            if (tableName == "Врачи")
            {
                AddForm form = new AddForm(connectionString);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    btnRefresh.PerformClick();
                }
            }
            else
            {
                MessageBox.Show("Добавление доступно только для врачей");
            }
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvDoctors.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись!");
                return;
            }

            string tableName = cmbTables.SelectedItem?.ToString();
            string idColumn = dgvDoctors.Columns[0].Name;
            int id = Convert.ToInt32(dgvDoctors.CurrentRow.Cells[0].Value);
            string name = dgvDoctors.CurrentRow.Cells[1].Value?.ToString() ?? "запись";

            if (MessageBox.Show($"Удалить {name}?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        OleDbCommand cmd = new OleDbCommand(
                            $"DELETE FROM [{tableName}] WHERE [{idColumn}] = @id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    btnRefresh.PerformClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        // Этот обработчик можно оставить пустым или удалить
        private void dgvDoctors_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Не требуется для базового функционала
        }

        // ДОБАВЬТЕ ЭТОТ ОБРАБОТЧИК ДЛЯ ДВОЙНОГО КЛИКА
        private void dgvDoctors_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Клик по строке данных
            {
                btnEdit.PerformClick();
            }
        }
    }
}