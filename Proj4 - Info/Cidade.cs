using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Proj4
{
    public class Cidade : IComparable<Cidade>, IRegistro
    {
        string nome;
        double x, y;
        ListaSimples<Ligacao> ligacoes = new ListaSimples<Ligacao>();

        const int tamanhoNome = 25;
        const int tamanhoRegistro = tamanhoNome + (2 * sizeof(double));

        public string Nome
        {
            get => nome;
            set => nome = value.PadRight(tamanhoNome, ' ').Substring(0, tamanhoNome);
        }

        public Cidade(string nome, double x, double y)
        {
            this.Nome = nome;
            this.x = x;
            this.y = y;
            ligacoes = new ListaSimples<Ligacao>();
        }
        public override string ToString()
        {
            return Nome.TrimEnd() + " (" + ligacoes.QuantosNos + ")";
        }

        public Cidade()
        {
            this.Nome = "";
            this.x = 0;
            this.y = 0;
            ligacoes = new ListaSimples<Ligacao>();
        }

        public Cidade(string nome)
        {
            this.Nome = nome;
        }

        public int CompareTo(Cidade outraCid)
        {
            return Nome.CompareTo(outraCid.Nome);
        }

        public int TamanhoRegistro { get => tamanhoRegistro; }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }

        public void LerRegistro(BinaryReader arquivo, long qualRegistro)
        {
            if (arquivo != null)
            {
                try
                {
                    long qtosBytesAPular = qualRegistro * TamanhoRegistro;

                    arquivo.BaseStream.Seek(qtosBytesAPular, SeekOrigin.Begin);

                    char[] umNome = new char[tamanhoNome];

                    umNome = arquivo.ReadChars(tamanhoNome);
                    string nomeLido = "";
                    for (int i = 0; i < tamanhoNome; i++)
                        nomeLido += umNome[i];
                    this.Nome = nomeLido;

                    this.X = arquivo.ReadDouble();
                    this.Y = arquivo.ReadDouble();

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void GravarRegistro(BinaryWriter arquivo)
        {
            if (arquivo != null)
            {
                char[] nome = Nome.ToCharArray();
                arquivo.Write(nome);
                arquivo.Write(X);
                arquivo.Write(y);
            }
            else
            {
                throw new Exception("Arquivo não aberto");
            }
        }

        public List<Ligacao> ListarLigacoes()
        {
            List<Ligacao> listaDeLigacoes = new List<Ligacao>();
            NoLista<Ligacao> atual = ligacoes.Primeiro;
            while (atual != null)
            {
                listaDeLigacoes.Add(atual.Info);
                atual = atual.Prox;
            }
            return listaDeLigacoes;
        }

        public void SalvarLigacoes(StreamWriter arquivo)
        {
            foreach (Ligacao ligacao in ListarLigacoes())
            {
                arquivo.WriteLine(ligacao.ToString());
            }
        }

        // criar e excluir dado -> garantir a integridade da árvore de cidades e
        // das ligações da outra cidade é responsabilidade do main

        public bool CriarLigacao(string destino, int distancia)
        {
            Ligacao novaLigacao = new Ligacao(this.Nome, destino, distancia);
            if (ligacoes.ExisteDado(novaLigacao))
            {
                return false;
            }
            else
            {
                ligacoes.InserirAposFim(novaLigacao);
                return true;
            }
        }
        public bool ExcluirLigacao(string destino)
        {
            Ligacao ligacaoParaExcluir = new Ligacao(this.Nome, destino, 0);

            return ligacoes.RemoverDado(ligacaoParaExcluir);
        }
    }
}
