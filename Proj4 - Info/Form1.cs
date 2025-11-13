using apGrafoDaSilva;
using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
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

        private void LerArquivos()
        {
            dlgAbrir.Title = "Selecione o arquivo binário de cidades";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoCidades = dlgAbrir.FileName;
                
                arvore.LerArquivoDeRegistros(nomeArquivoCidades);
                arvore.Desenhar(pnlArvore);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }

                dlgAbrir.Title = "Selecione o arquivo texto de ligações";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoLigacoes = dlgAbrir.FileName;

                LerArquivoLigacoes(nomeArquivoLigacoes);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }

            void LerArquivoLigacoes(string nomeArquivo)
            {
                StreamReader arquivoLeitura = new StreamReader(nomeArquivo);

                string linha, cidadeOrigem, cidadeDestino;
                int distancia;
                int indiceLinha = 0;
                while (arquivoLeitura.EndOfStream == false)
                {
                    linha = arquivoLeitura.ReadLine();

                    while (linha[indiceLinha] != ';')
                    {
                        indiceLinha++;
                    }

                    cidadeOrigem = linha.Substring(0, indiceLinha);
                    int fimCidadeOrigem = indiceLinha = indiceLinha + 1;

                    while (linha[indiceLinha] != ';')
                    {
                        indiceLinha++;
                    }

                    cidadeDestino = linha.Substring(fimCidadeOrigem, indiceLinha);

                    distancia = Convert.ToInt32(linha.Substring(indiceLinha + 1, linha.Length - 1));

                    // debug
                    Console.WriteLine(linha);
                    Console.WriteLine(cidadeOrigem);
                    Console.WriteLine(cidadeDestino);
                    Console.WriteLine(distancia);

                    arvore.Existe(new Cidade(cidadeOrigem, 0, 0));
                    if (!arvore.Atual.Info.CriarLigacao(cidadeDestino, distancia))
                    {
                        MessageBox.Show("Erro ao adicionar a ligação: " + cidadeOrigem + " - " + cidadeDestino);
                    }
                }
            }

            void SalvarArquivos()
            {

            }
        }

    }
}
