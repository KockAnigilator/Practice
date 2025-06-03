using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Practice
{
    public partial class AddForm : Form
    {
        private readonly string connectionString;
        private readonly int? doctorId;

        // Элементы управления
        private TextBox txtLastName = new TextBox();
        private TextBox txtFirstName = new TextBox();
        private TextBox txtMiddleName = new TextBox();

        private ComboBox cmbDepartment = new ComboBox();
        private ComboBox cmbPosition = new ComboBox();
        private ComboBox cmbSpecialty = new ComboBox();
        private ComboBox cmbHospital = new ComboBox();
        private ComboBox cmbEducation = new ComboBox();

        private Button btnSave = new Button();
        private Button btnCancel = new Button();

        public AddForm(string connString, int? id = null)
        {
            connectionString = connString;
            doctorId = id;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Настройка формы
            this.Text = doctorId.HasValue ? "Редактирование врача" : "Добавление врача";
            this.Size = new System.Drawing.Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем таблицу для компоновки
            TableLayoutPanel mainTable = new TableLayoutPanel();
            mainTable.Dock = DockStyle.Fill;
            mainTable.ColumnCount = 2;
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainTable.RowCount = 7;
            mainTable.Padding = new Padding(10);

            // Добавляем элементы управления
            AddControlWithLabel(mainTable, "Фамилия*:", txtLastName, 0);
            AddControlWithLabel(mainTable, "Имя:", txtFirstName, 1);
            AddControlWithLabel(mainTable, "Отчество:", txtMiddleName, 2);
            AddControlWithLabel(mainTable, "Отделение:", cmbDepartment, 3);
            AddControlWithLabel(mainTable, "Должность:", cmbPosition, 4);
            AddControlWithLabel(mainTable, "Специальность:", cmbSpecialty, 5);
            AddControlWithLabel(mainTable, "Больница:", cmbHospital, 6);
            AddControlWithLabel(mainTable, "Образование:", cmbEducation, 7);

            // Кнопки
            TableLayoutPanel buttonPanel = new TableLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 40;
            buttonPanel.ColumnCount = 2;
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            btnSave.Text = "Сохранить";
            btnSave.Dock = DockStyle.Fill;
            btnSave.Click += btnSave_Click;

            btnCancel.Text = "Отмена";
            btnCancel.Dock = DockStyle.Fill;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.Add(btnSave, 0, 0);
            buttonPanel.Controls.Add(btnCancel, 1, 0);

            // Добавляем на форму
            this.Controls.Add(mainTable);
            this.Controls.Add(buttonPanel);

            // Загружаем данные
            LoadComboBoxes();
            if (doctorId.HasValue)
            {
                LoadDoctorData();
            }
        }

        private void AddControlWithLabel(TableLayoutPanel panel, string labelText, Control control, int row)
        {
            Label label = new Label();
            label.Text = labelText;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            label.Dock = DockStyle.Fill;
            label.Padding = new Padding(0, 5, 10, 5);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 2, 0, 2);

            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(control, 1, row);
        }

        private void LoadComboBoxes()
        {
            LoadComboBox("SELECT id_Отделение, [Название отделения] FROM Отделения", cmbDepartment);
            LoadComboBox("SELECT id_Должность, Должность FROM Должность", cmbPosition);
            LoadComboBox("SELECT id_Специальность, Специальность FROM Специальности", cmbSpecialty);
            LoadComboBox("SELECT id_Больница, Адрес FROM Больницы", cmbHospital);
            LoadComboBox("SELECT id_Учебное заведение, [Что закончил] FROM [Учебные заведения]", cmbEducation);
        }

        private void LoadComboBox(string query, ComboBox comboBox)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox.DataSource = dt;
                    comboBox.DisplayMember = dt.Columns[1].ColumnName;
                    comboBox.ValueMember = dt.Columns[0].ColumnName;
                    comboBox.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочника: {ex.Message}");
            }
        }

        private void LoadDoctorData()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand(
                        "SELECT * FROM Врачи WHERE id_Врач = @id", conn);
                    cmd.Parameters.AddWithValue("@id", doctorId.Value);

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtLastName.Text = reader["Фамилия"].ToString();
                            txtFirstName.Text = reader["Имя"].ToString();
                            txtMiddleName.Text = reader["Отчество"].ToString();

                            SetComboBoxValue(cmbDepartment, reader["id_Отделение"]);
                            SetComboBoxValue(cmbPosition, reader["id_Должность"]);
                            SetComboBoxValue(cmbSpecialty, reader["id_Cпециальность"]);
                            SetComboBoxValue(cmbHospital, reader["id_Больница"]);
                            SetComboBoxValue(cmbEducation, reader["id_Учебное заведение"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных врача: {ex.Message}");
            }
        }

        private void SetComboBoxValue(ComboBox comboBox, object value)
        {
            if (value == DBNull.Value || value == null)
                return;

            foreach (DataRowView item in comboBox.Items)
            {
                if (item.Row[0].ToString() == value.ToString())
                {
                    comboBox.SelectedValue = value;
                    return;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Фамилия обязательна для заполнения!");
                return;
            }

            try
            {
                string query;
                if (doctorId.HasValue)
                {
                    query = @"UPDATE Врачи SET 
                              Фамилия = @ln, Имя = @fn, Отчество = @mn,
                              id_Отделение = @dep, id_Должность = @pos,
                              id_Cпециальность = @spec, id_Больница = @hosp,
                              id_Учебное заведение = @edu
                              WHERE id_Врач = @id";
                }
                else
                {
                    query = @"INSERT INTO Врачи 
                             (Фамилия, Имя, Отчество, id_Отделение, id_Должность, 
                              id_Cпециальность, id_Больница, id_Учебное заведение)
                             VALUES (@ln, @fn, @mn, @dep, @pos, @spec, @hosp, @edu)";
                }

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ln", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@fn", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@mn", txtMiddleName.Text ?? "");

                        cmd.Parameters.AddWithValue("@dep", GetComboBoxValue(cmbDepartment));
                        cmd.Parameters.AddWithValue("@pos", GetComboBoxValue(cmbPosition));
                        cmd.Parameters.AddWithValue("@spec", GetComboBoxValue(cmbSpecialty));
                        cmd.Parameters.AddWithValue("@hosp", GetComboBoxValue(cmbHospital));
                        cmd.Parameters.AddWithValue("@edu", GetComboBoxValue(cmbEducation));

                        if (doctorId.HasValue)
                            cmd.Parameters.AddWithValue("@id", doctorId.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private object GetComboBoxValue(ComboBox comboBox)
        {
            return comboBox.SelectedValue ?? DBNull.Value;
        }
    }
}