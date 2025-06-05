using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;

namespace Practice
{
    public partial class AddForm : Form
    {
        private readonly string connectionString;
        private readonly string tableName;
        private readonly DataRow dataRow;
        private readonly string idColumn;
        private TableLayoutPanel mainPanel = new TableLayoutPanel();
        private Dictionary<string, Control> inputControls = new Dictionary<string, Control>();
        private List<string> columnNames = new List<string>();
        private Dictionary<string, string> columnTypes = new Dictionary<string, string>(); // Добавлен словарь типов

        public AddForm(string connString, string tblName, DataRow row, string idCol)
        {
            InitializeComponent();
            connectionString = connString;
            tableName = tblName;
            dataRow = row;
            idColumn = idCol;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Настройка формы
            Text = $"Редактирование: {tableName}";
            Size = new Size(500, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // Основная панель
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 2;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainPanel.AutoScroll = true;

            // Контейнер для правильного размещения
            TableLayoutPanel container = new TableLayoutPanel();
            container.Dock = DockStyle.Fill;
            container.RowCount = 2;
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Кнопки
            Button btnSave = new Button { Text = "Сохранить", Anchor = AnchorStyles.Right };
            Button btnCancel = new Button { Text = "Отмена", Anchor = AnchorStyles.Left };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                Dock = DockStyle.Fill
            };

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            // Собираем интерфейс
            container.Controls.Add(mainPanel, 0, 0);
            container.Controls.Add(buttonPanel, 0, 1);
            Controls.Add(container);

            // Создаем поля для каждого столбца
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                DataTable schema = conn.GetSchema("Columns", new string[] { null, null, tableName });

                int rowIndex = 0;
                foreach (DataRow colRow in schema.Rows)
                {
                    string columnName = colRow["COLUMN_NAME"].ToString();
                    string dataType = colRow["DATA_TYPE"].ToString();

                    // Пропускаем ID-колонку
                    if (columnName == idColumn) continue;

                    // Сохраняем информацию о столбце
                    columnNames.Add(columnName);
                    columnTypes[columnName] = dataType; // Сохраняем тип столбца

                    // Добавляем метку
                    Label lbl = new Label
                    {
                        Text = columnName + ":",
                        TextAlign = ContentAlignment.MiddleRight,
                        Anchor = AnchorStyles.Right,
                        Dock = DockStyle.Fill,
                        Name = $"lbl_{columnName}"
                    };

                    // Добавляем поле ввода
                    Control inputControl;
                    if (dataType == "DBTYPE_BOOL")
                    {
                        CheckBox chk = new CheckBox
                        {
                            Checked = dataRow.Table.Columns.Contains(columnName) &&
                                      !dataRow.IsNull(columnName) ?
                                      (bool)dataRow[columnName] : false,
                            Dock = DockStyle.Fill
                        };
                        inputControl = chk;
                    }
                    else
                    {
                        string textValue = "";
                        if (dataRow.Table.Columns.Contains(columnName) &&
                            !dataRow.IsNull(columnName))
                        {
                            if (dataType == "DBTYPE_DATE")
                            {
                                textValue = ((DateTime)dataRow[columnName]).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                textValue = dataRow[columnName].ToString();
                            }
                        }

                        TextBox txt = new TextBox
                        {
                            Dock = DockStyle.Fill,
                            Text = textValue,
                            Name = $"txt_{columnName}"
                        };

                        inputControl = txt;
                    }

                    // Сохраняем ссылку на контрол
                    inputControls[columnName] = inputControl;

                    // Добавляем на панель
                    mainPanel.RowCount++;
                    mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
                    mainPanel.Controls.Add(lbl, 0, rowIndex);
                    mainPanel.Controls.Add(inputControl, 1, rowIndex);
                    rowIndex++;
                }
            }
        }

        private bool IsUniqueColumn(string columnName)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                DataTable schema = conn.GetSchema("Indexes", new[] { null, null, tableName, null });

                foreach (DataRow row in schema.Rows)
                {
                    string indexColumn = row["column_name"].ToString();
                    bool isUnique = (bool)row["UNIQUE"];

                    if (indexColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase) && isUnique)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                bool isNew = dataRow[idColumn] is DBNull || Convert.ToInt32(dataRow[idColumn]) == 0;

                // Обновляем DataRow значениями из формы
                foreach (string columnName in columnNames)
                {
                    Control inputControl = inputControls[columnName];
                    object value = GetControlValue(inputControl, columnName);

                    if (value != null)
                    {
                        dataRow[columnName] = value;
                    }
                    else
                    {
                        dataRow[columnName] = DBNull.Value;
                    }
                }

                // Используем DataAdapter для сохранения
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter();

                    // Создаем команды для адаптера
                    if (isNew)
                    {
                        adapter.InsertCommand = CreateInsertCommand(conn);
                    }
                    else
                    {
                        adapter.UpdateCommand = CreateUpdateCommand(conn);
                    }

                    DataTable dt = dataRow.Table.Clone();
                    dt.ImportRow(dataRow);

                    if (isNew)
                    {
                        adapter.Update(dt);
                        // Получаем новый ID
                        if (dt.Rows.Count > 0)
                        {
                            dataRow[idColumn] = dt.Rows[0][idColumn];
                        }
                    }
                    else
                    {
                        adapter.Update(dt);
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\n" +
                               $"Тип ошибки: {ex.GetType().Name}\n" +
                               $"Подробности: {ex.InnerException?.Message}",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private OleDbCommand CreateInsertCommand(OleDbConnection conn)
        {
            string columns = "";
            string values = "";
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            foreach (string columnName in columnNames)
            {
                // Пропускаем автоинкрементные поля
                if (columnName == idColumn && IsAutoIncrement(columnName)) continue;

                columns += $"[{columnName}], ";
                values += "?, ";
                cmd.Parameters.Add($"@{columnName}", GetOleDbType(columnTypes[columnName]));
            }

            columns = columns.TrimEnd(',', ' ');
            values = values.TrimEnd(',', ' ');

            cmd.CommandText = $"INSERT INTO [{tableName}] ({columns}) VALUES ({values})";

            // Для автоинкрементных полей получаем новый ID
            if (IsAutoIncrement(idColumn))
            {
                cmd.CommandText += $"; SELECT @@IDENTITY AS {idColumn};";
            }

            return cmd;
        }

        private OleDbCommand CreateUpdateCommand(OleDbConnection conn)
        {
            string setClause = "";
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            foreach (string columnName in columnNames)
            {
                if (columnName == idColumn) continue; // Не обновляем ключ

                setClause += $"[{columnName}] = ?, ";
                cmd.Parameters.Add($"@{columnName}", GetOleDbType(columnTypes[columnName]));
            }

            setClause = setClause.TrimEnd(',', ' ');
            cmd.CommandText = $"UPDATE [{tableName}] SET {setClause} WHERE [{idColumn}] = ?";

            // Добавляем параметр для WHERE
            cmd.Parameters.Add($"@{idColumn}", GetOleDbType(columnTypes[idColumn]));

            return cmd;
        }

        private OleDbType GetOleDbType(string dataType)
        {
            switch (dataType)
            {
                case "DBTYPE_BOOL": return OleDbType.Boolean;
                case "DBTYPE_DATE": return OleDbType.Date;
                case "DBTYPE_I4": return OleDbType.Integer;
                case "DBTYPE_I2": return OleDbType.SmallInt;
                case "DBTYPE_R4": return OleDbType.Single;
                case "DBTYPE_R8": return OleDbType.Double;
                case "DBTYPE_CY": return OleDbType.Currency;
                case "DBTYPE_NUMERIC": return OleDbType.Numeric;
                case "DBTYPE_WVARCHAR": return OleDbType.VarWChar;
                default: return OleDbType.VarChar;
            }
        }

        private bool IsAutoIncrement(string columnName)
        {
            // Проверяем, является ли поле автоинкрементным
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                DataTable schema = conn.GetSchema("Columns", new[] { null, null, tableName, columnName });

                if (schema.Rows.Count > 0)
                {
                    string autoIncrement = schema.Rows[0]["AUTOINCREMENT"]?.ToString();
                    return autoIncrement == "True" || autoIncrement == "Yes";
                }
            }
            return false;
        }

        // Обновленный метод с двумя параметрами
        private object GetControlValue(Control control, string columnName)
        {
            if (control is TextBox textBox)
            {
                // Для дат используем специальную обработку
                if (columnTypes.ContainsKey(columnName) &&
                    columnTypes[columnName] == "DBTYPE_DATE")
                {
                    if (DateTime.TryParse(textBox.Text, out DateTime date))
                        return date;
                    return DBNull.Value;
                }
                return textBox.Text;
            }
            if (control is CheckBox checkBox)
                return checkBox.Checked;
            return control.Text;
        }
    }
}