using Practice;
using System;
using System.Data;
using System.Data.OleDb;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace Practice
{
    public partial class MainForm : Form
    {
        private string connectionString = "";
        private DataSet dataSet = new DataSet();
        private DataGridView dataGridView = new DataGridView();

        public MainForm()
        {
            InitializeComponent();
        }

        private void LoadTableList()
        {
            cmbTables.Items.Clear();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                // Получаем список таблиц
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

            string tableName = cmbTables.SelectedItem.ToString();
            LoadTableData(tableName);
        }

        private void LoadTableData(string tableName)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT * FROM [{tableName}]", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView.DataSource = dt;
            }
        }

        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Access Database (*.accdb)|*.accdb";
                openFileDialog.Title = "Выберите файл базы данных";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={openFileDialog.FileName};";
                    try
                    {
                        LoadTableList();
                        btnCloseConnection.Enabled = true;
                        btnRefresh.Enabled = true;
                        btnAdd.Enabled = true;
                        btnEdit.Enabled = true;
                        btnDelete.Enabled = true;
                        MessageBox.Show("Подключение к базе данных успешно установлено!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка подключения: {ex.Message}");
                    }
                }
            }
        }

        private void btnCloseConnection_Click_1(object sender, EventArgs e)
        {
            connectionString = "";
            dataGridView.DataSource = null;
            cmbTables.Items.Clear();
            btnCloseConnection.Enabled = false;
            btnRefresh.Enabled = false;
            btnAdd.Enabled = false;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            MessageBox.Show("Соединение с базой данных закрыто");
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
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования!");
                return;
            }

            string tableName = cmbTables.SelectedItem?.ToString();

            if (tableName == "Врачи")
            {
                int doctorId = Convert.ToInt32(dataGridView.CurrentRow.Cells["id_Врач"].Value);
                AddForm form = new AddForm(connectionString, doctorId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    btnRefresh.PerformClick();
                }
            }
            else
            {
                MessageBox.Show("Редактирование для этой таблицы пока не реализовано");
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
                    btnRefresh.PerformClick(); // Обновляем данные
                }
            }
            else
            {
                // Для других таблиц можно использовать универсальную форму
                MessageBox.Show("Добавление для этой таблицы пока не реализовано");
            }
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления!");
                return;
            }

            string tableName = cmbTables.SelectedItem?.ToString();
            string idColumn = dataGridView.Columns[0].Name;
            int id = Convert.ToInt32(dataGridView.CurrentRow.Cells[0].Value);
            string name = dataGridView.CurrentRow.Cells[1].Value?.ToString() ?? "запись";

            if (MessageBox.Show($"Вы действительно хотите удалить {name}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }
    }
}