using System;
using System.Windows.Forms;

namespace Proj4
{
    public partial class Form1 : Form
    {
        Arvore<Cidade> arvore = new Arvore<Cidade>();
        public Form1()
        {
            InitializeComponent();
        }

        private void tpCadastro_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pnlArvore_Paint(object sender, PaintEventArgs e)
        {
            arvore.Desenhar(pnlArvore);
        }

        private void txtNomeCidade_Leave(object sender, EventArgs e)
        {

        }

    }
}
