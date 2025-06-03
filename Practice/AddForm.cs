using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Practice
{
    public partial class AddForm : Form
    {
        public AddForm(string connString, int? id = null)
        {
            InitializeComponent();

        }
    }
}
