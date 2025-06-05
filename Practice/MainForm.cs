using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
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

                    // Получаем список всех столбцов
                    DataTable schema = conn.GetSchema("Columns", new[] { null, null, tableName });
                    List<string> columns = new List<string>();
                    foreach (DataRow row in schema.Rows)
                    {
                        columns.Add($"[{row["COLUMN_NAME"]}]");
                    }

                    // Формируем запрос с явным указанием столбцов
                    string query = $"SELECT {string.Join(", ", columns)} FROM [{tableName}]";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvDoctors.DataSource = dt;
                    dgvDoctors.AutoResizeColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string idColumn = GetIdColumnName(tableName);
            int id = Convert.ToInt32(dgvDoctors.CurrentRow.Cells[idColumn].Value);

            // Получаем текущие данные
            DataRow currentRow = ((DataRowView)dgvDoctors.CurrentRow.DataBoundItem).Row;

            // Открываем универсальную форму
            AddForm form = new AddForm(
                connectionString,
                tableName,
                currentRow,
                idColumn
            );

            if (form.ShowDialog() == DialogResult.OK)
            {
                btnRefresh.PerformClick();
            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem == null) return;
            string tableName = cmbTables.SelectedItem.ToString();

            // Правильное создание новой записи
            DataTable dataTable = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT TOP 1 * FROM [{tableName}]", conn);
                adapter.Fill(dataTable);
            }

            DataRow newRow = dataTable.NewRow();

            // Устанавливаем ID как DBNull для автоинкремента
            string idColumn = GetIdColumnName(tableName);
            if (dataTable.Columns.Contains(idColumn))
            {
                newRow[idColumn] = DBNull.Value;
            }

            AddForm form = new AddForm(
                connectionString,
                tableName,
                newRow,
                idColumn
            );

            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadTableData(tableName);
            }
        }

        // Исправление определения ID колонки
        private string GetIdColumnName(string tableName)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    DataTable schema = conn.GetSchema("Columns", new string[] { null, null, tableName });

                    // Приоритетные варианты названий ID
                    string[] possibleIds = { "id", "код", "code", "ид" };

                    foreach (DataRow row in schema.Rows)
                    {
                        string columnName = row["COLUMN_NAME"].ToString().ToLower();
                        if (possibleIds.Any(id => columnName.Contains(id)))
                            return row["COLUMN_NAME"].ToString();
                    }

                    return schema.Rows[0]["COLUMN_NAME"].ToString();
                }
            }
            catch
            {
                return "ID";
            }
        }

        // Улучшенное удаление записей
        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvDoctors.CurrentRow == null) return;

            string tableName = cmbTables.SelectedItem?.ToString();
            string idColumn = GetIdColumnName(tableName);
            int id = Convert.ToInt32(dgvDoctors.CurrentRow.Cells[idColumn].Value);

            // Формируем читаемое название записи
            string recordName = "запись";
            foreach (DataGridViewCell cell in dgvDoctors.CurrentRow.Cells)
            {
                if (!cell.OwningColumn.Name.Equals(idColumn, StringComparison.OrdinalIgnoreCase))
                {
                    recordName = cell.Value?.ToString() ?? recordName;
                    break;
                }
            }

            if (MessageBox.Show($"Удалить '{recordName}'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}