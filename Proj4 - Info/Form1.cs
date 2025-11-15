using apGrafoDaSilva;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Proj4
{
    public partial class Form1 : Form
    {
        ArvoreAVL<Cidade> arvore;
        Grafo grafoCaminhos;

        public Form1()
        {
            InitializeComponent();
            arvore = new ArvoreAVL<Cidade>();
            grafoCaminhos = new Grafo();
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
                string cidadeAtual = null;
                int distancia;
                while (arquivoLeitura.EndOfStream == false)
                {
                    linha = arquivoLeitura.ReadLine();
                    
                    var valores = linha.Split(';');
                    cidadeOrigem = valores[0];
                    cidadeDestino = valores[1];
                    distancia = Convert.ToInt32((string)valores[2]);

                    // debug
                    Console.WriteLine(linha);
                    Console.WriteLine(cidadeOrigem);
                    Console.WriteLine(cidadeDestino);
                    Console.WriteLine(distancia);

                    if(cidadeAtual != cidadeOrigem)
                    {
                        arvore.Existe(new Cidade(cidadeOrigem, 0, 0));
                        cidadeAtual = cidadeOrigem;
                    }
                    if (!arvore.Atual.Info.CriarLigacao(cidadeDestino, distancia))
                    {
                        MessageBox.Show("Erro ao adicionar a ligação: " + cidadeOrigem + " - " + cidadeDestino);
                    }
                    else
                    {
                        grafoCaminhos.NovoVertice(cidadeDestino);
                    }
                }
                arquivoLeitura.Close();
            }
        }
        void SalvarArquivos()
        {
            dlgAbrir.Title = "Selecione o arquivo binário de cidades para salvar";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoCidades = dlgAbrir.FileName;

                arvore.GravarArquivoDeRegistros(nomeArquivoCidades);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }

            dlgAbrir.Title = "Selecione o arquivo texto de ligações para salvar";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoLigacoes = dlgAbrir.FileName;

                SalvarArquivoLigacoes(nomeArquivoLigacoes);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }

            void SalvarArquivoLigacoes(string nomeArquivo)
            {
                StreamWriter arquivoEscrita = new StreamWriter(nomeArquivo);
                List<Cidade> listaCidades = new List<Cidade>();
                arvore.VisitarEmOrdem(ref listaCidades);
                foreach (Cidade cidade in listaCidades)
                {
                    cidade.SalvarLigacoes(arquivoEscrita);
                }
                arquivoEscrita.Close();
            }
        }

        private void pbMapa_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            double larguraAtual = pbMapa.Width;
            double alturaAtual = pbMapa.Height;
            const double larguraOriginal = 2560.0;
            const double alturaOriginal = 1600.0;

            // lista das cidades na arvore
            List<Cidade> listaCidades = new List<Cidade>();
            arvore.VisitarEmOrdem(ref listaCidades);

            Pen corCaminhos = new Pen(Color.DarkGray, 2);
            Brush corCidade = new SolidBrush(Color.Red);
            Font fonte = new Font("Arial", 10, FontStyle.Bold);

            // desenha os caminhos
            foreach (Cidade cidade in listaCidades)
            {
                PointF origem = Converter(cidade.X, cidade.Y, larguraAtual, alturaAtual);

                foreach (Ligacao ligacao in cidade.ListarLigacoes())
                {
                    arvore.Existe(new Cidade(ligacao.Destino, 0, 0));
                    Cidade destinoObj = arvore.Atual.Info;
                    if (destinoObj == null)
                        continue;

                    PointF destino = Converter(destinoObj.X, destinoObj.Y, larguraAtual, alturaAtual);

                    g.DrawLine(corCaminhos, origem, destino);
                }
            }
            
            // desenha as ciades
            foreach (Cidade cidade in listaCidades)
            {
                PointF p = Converter(cidade.X, cidade.Y, larguraAtual, alturaAtual);

                g.FillEllipse(corCidade, p.X - 4, p.Y - 4, 8, 8);
                g.DrawString(cidade.Nome, fonte, Brushes.Black, p.X + 6, p.Y - 6);
            }

            // funcao para manter a proporcao das coordenadas
            PointF Converter(double x, double y, double largAtual, double altAtual)
            {
                float novoX = (float)((x / larguraOriginal) * largAtual);
                float novoY = (float)((y / alturaOriginal) * altAtual);
                return new PointF(novoX, novoY);
            }
        }
    }
}
