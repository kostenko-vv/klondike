using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace klondike
{
    public partial class CardMapping : PictureBox
    {
        private int _columnNumber = -1;

        public int columnNumber
        {
            get; set;
        }

        public CardMapping()
        {
            InitializeComponent();
            BorderStyle = BorderStyle.FixedSingle;
            AllowDrop = true;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
