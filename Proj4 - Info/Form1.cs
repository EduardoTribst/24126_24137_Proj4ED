using apGrafoDaSilva;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Proj4
{
    public partial class Form1 : Form
    {
        ArvoreAVL<Cidade> arvore;
        Grafo grafoCaminhos;
        List<string> caminhoDestacado; // usado para pintar o caminho no mapa
        bool processoInclusaoCidade;

        public Form1()
        {
            InitializeComponent();
            arvore = new ArvoreAVL<Cidade>();
            grafoCaminhos = new Grafo();
            caminhoDestacado = null;
            processoInclusaoCidade = false;
        }

        private void tpCadastro_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LerArquivos();
        }

        private void pnlArvore_Paint(object sender, PaintEventArgs e)
        {
            arvore.Desenhar(pnlArvore);
        }

        private void txtNomeCidade_Leave(object sender, EventArgs e)
        {
            if (processoInclusaoCidade)
            {
                string nomeCidade = txtNomeCidade.Text.Trim();
                if (nomeCidade == "")
                {
                    MessageBox.Show("Nome da cidade não pode ser vazio.");
                    TerminarInclusao();
                    return;
                }

                if (arvore.Existe(new Cidade(nomeCidade, 0, 0)))
                {
                    MessageBox.Show("Cidade já existe.");
                    TerminarInclusao();
                }
                else
                {
                    MessageBox.Show("Clique no ponto do mapa onde a cidade será incluída.");
                    pbMapa.Cursor = Cursors.Cross;
                }
            }
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

                    if (cidadeAtual != cidadeOrigem && arvore.Existe(new Cidade(cidadeDestino, 0, 0)))
                    {
                        arvore.Existe(new Cidade(cidadeOrigem, 0, 0));
                        cidadeAtual = cidadeOrigem;
                    }
                    if (arvore.Atual != null)
                    {
                        if (!arvore.Atual.Info.CriarLigacao(cidadeDestino, distancia))
                        {
                            MessageBox.Show("Erro ao adicionar a ligação: " + cidadeOrigem + " - " + cidadeDestino);
                        }
                        else
                        {
                            grafoCaminhos.NovoVertice(cidadeDestino);
                        }
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

            // lista das cidades na arvore
            List<Cidade> listaCidades = new List<Cidade>();
            arvore.VisitarEmOrdem(ref listaCidades);

            Pen corCaminhos = new Pen(Color.DarkGray, 2);
            Brush corCidade = new SolidBrush(Color.Red);
            Font fonte = new Font("Arial", 10, FontStyle.Bold);

            foreach (Cidade cidade in listaCidades)
            {
                // calcula a posicao no mapa
                PointF origem = Converter(cidade.X, cidade.Y, larguraAtual, alturaAtual);

                // desenha as ligacoes
                foreach (Ligacao ligacao in cidade.ListarLigacoes())
                {
                    arvore.Existe(new Cidade(ligacao.Destino, 0, 0));
                    if (arvore.Atual == null)
                        continue;

                    PointF destino = Converter(arvore.Atual.Info.X, arvore.Atual.Info.Y, larguraAtual, alturaAtual);

                    g.DrawLine(corCaminhos, origem, destino);
                }

                //desenha a cidade
                g.FillEllipse(corCidade, origem.X - 4, origem.Y - 4, 8, 8);
                g.DrawString(cidade.Nome, fonte, Brushes.Black, origem.X + 6, origem.Y - 6);

                Console.WriteLine($"{cidade.Nome}: {cidade.X}, {cidade.Y}");
            }

            if (caminhoDestacado != null && caminhoDestacado.Count > 1)
            {
                Pen corDestaque = new Pen(Color.Blue, 4);

                for (int i = 0; i < caminhoDestacado.Count - 1; i++)
                {
                    string nomeA = caminhoDestacado[i];
                    string nomeB = caminhoDestacado[i + 1];

                    arvore.Existe(new Cidade(nomeA, 0, 0));
                    Cidade cidA = arvore.Atual.Info;

                    arvore.Existe(new Cidade(nomeB, 0, 0));
                    Cidade cidB = arvore.Atual.Info;

                    if (cidA == null || cidB == null)
                        continue;

                    PointF p1 = Converter(cidA.X, cidA.Y, larguraAtual, alturaAtual);
                    PointF p2 = Converter(cidB.X, cidB.Y, larguraAtual, alturaAtual);

                    g.DrawLine(corDestaque, p1, p2);
                }
            }

            // funcao para manter a proporcao das coordenadas
            PointF Converter(double x, double y, double largAtual, double altAtual)
            {
                float novoX = (float)(x * largAtual);
                float novoY = (float)(y * altAtual);
                return new PointF(novoX, novoY);
            }
        }

        private void btnBuscarCaminho_Click(object sender, EventArgs e)
        {
            string origem = txtNomeCidade.Text;
            string destino = cbxCidadeDestino.Text;

            (List<(String, int)>, int) resultadoCaminho = grafoCaminhos.CaminhosComDistancias(origem, destino);

            int distTotal = resultadoCaminho.Item2;
            List<(String, int)> caminho = resultadoCaminho.Item1;

            // limpar grid
            dgvRotas.Rows.Clear();

            // preencher grid
            foreach (var item in caminho)
            {
                string nomeCidade = item.Item1;
                int dist = item.Item2;

                dgvRotas.Rows.Add(nomeCidade, dist);
            }

            lbDistanciaTotal.Text = $"Distância total: {distTotal.ToString()} km";

            // salva o caminho e atualiza o mapa
            caminhoDestacado = caminho.Select(c => c.Item1).ToList();
            pbMapa.Invalidate();
        }

        private void btnIncluirCidade_Click(object sender, EventArgs e)
        {
            if (!processoInclusaoCidade)
            {
                IniciarInclusao();
            }
            else
            {
                TerminarInclusao();
            }
        }

        private void IniciarInclusao()
        {
            processoInclusaoCidade = true;
            MessageBox.Show("Processo de inclusão iniciado.\n" +
                "Digite o nome da cidadade no campo e clique no mapa para definir a localização.\n" +
                "Para cancelar o processo, clique novamente no botão de incluir cidade.");

            udX.Value = 0;
            udY.Value = 0;
            txtNomeCidade.Text = "";

            btnBuscarCaminho.Enabled = false;
            btnAlterarCidade.Enabled = false;
            btnExcluirCaminho.Enabled = false;
        }

        private void TerminarInclusao()
        {
            processoInclusaoCidade = false;
            MessageBox.Show("Processo de inclusão terminado");

            btnBuscarCidade.Enabled = true;
            btnAlterarCidade.Enabled = true;
            btnExcluirCidade.Enabled = true;
        }

        private void pbMapa_MouseClick(object sender, MouseEventArgs e)
        {
            if (processoInclusaoCidade)
            {
                // verifica se o usuario ainda nao digitou o nome da cidade e buscou se existe
                if (txtNomeCidade.Text.Trim() == "")
                {
                    MessageBox.Show("Digite o nome da cidade antes de clicar no mapa.");
                    return;
                }

                // pega as coordenadas do click proporcionais
                double xProporcional = (double)e.X / pbMapa.Width;
                double yProporcional = (double)e.Y / pbMapa.Height;

                udX.Value = (decimal)xProporcional;
                udY.Value = (decimal)yProporcional;

                if (MessageBox.Show("Confirma a inclusão da cidade " + txtNomeCidade.Text + "?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // insere na árvore
                    Cidade novaCidade = new Cidade(txtNomeCidade.Text.Trim(), xProporcional, yProporcional);
                    arvore.InserirBalanceado(novaCidade);

                    // desenha a arvore
                    TerminarInclusao();
                    pbMapa.Invalidate();
                }
                else
                {
                    TerminarInclusao();
                }
            }
        }
    }
}
