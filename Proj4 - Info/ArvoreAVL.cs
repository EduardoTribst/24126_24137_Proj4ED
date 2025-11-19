using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


public class ArvoreAVL<Dado>
             where Dado : IComparable<Dado>, IRegistro, new()
{
    private NoArvoreAVL<Dado> raiz,   // raiz da árvore; nó principal
                            atual,  // ponteiro para o nó visitado atualmente
                            antecessor;  // ponteiro para o "pai" do nó noAtual

    public NoArvoreAVL<Dado> Raiz { get => raiz; set => raiz = value; }
    public NoArvoreAVL<Dado> Atual { get => atual; }
    public NoArvoreAVL<Dado> Antecessor { get => antecessor; }

    public ArvoreAVL()
    {
        raiz = atual = antecessor = null;
    }

    public void VisitarEmOrdem(ref List<Dado> lista)
    {
        lista = new List<Dado>();
        VisitarEmOrdem(raiz, lista);
    }

    private void VisitarEmOrdem(NoArvoreAVL<Dado> atual, List<Dado> lista)
    {
        if (atual != null)    // se existe um nó
        {
            VisitarEmOrdem(atual.Esq, lista);             // chamada 1
            if (!atual.NoMarcadoParaMorrer)
            {
                lista.Add(atual.Info);                       // seu dado é exibido
            }
            VisitarEmOrdem(atual.Dir, lista);             // chamada 2
        }
    }

    public void Desenhar(Control tela)
    {
        if (Raiz == null) return;

        Graphics g = tela.CreateGraphics();
        g.Clear(Color.White);
        DesenharNo(g, Raiz, tela.Width*9 / 20, 20, tela.Width / 5, 50);
    }

    private void DesenharNo(Graphics g, NoArvoreAVL<Dado> no, int x, int y, int deslocamentoX, int deslocamentoY)
    {
        if (no == null) return;

        Rectangle rect = new Rectangle(x - 15, y - 15, 40, 30);
        if (no.NoMarcadoParaMorrer)
        {
            g.FillEllipse(Brushes.Red, rect);
        }
        else
        {
            g.FillEllipse(Brushes.LightBlue, rect);
        }
            g.DrawEllipse(Pens.Black, rect);
        g.DrawString(no.Info.ToString(), new Font("Arial", 8), Brushes.Black, x - 10, y - 10);

        if (no.Esq != null)
        {
            g.DrawLine(Pens.Black, x, y + 15, x - deslocamentoX, y + deslocamentoY);
            DesenharNo(g, no.Esq, x - deslocamentoX, y + deslocamentoY, deslocamentoX / 2, deslocamentoY);
        }

        if (no.Dir != null)
        {
            g.DrawLine(Pens.Black, x, y + 15, x + deslocamentoX, y + deslocamentoY);
            DesenharNo(g, no.Dir, x + deslocamentoX, y + deslocamentoY, deslocamentoX / 2, deslocamentoY);
        }
    }

    public bool Existe(Dado procurado)
    {
        antecessor = null;    // a raiz não tem um antecessor
        atual = raiz;         // posiciona ponteiro de percurso no 1o nó da árvore
        while (atual != null)
        {
            if (procurado.CompareTo(atual.Info) == 0 && !atual.NoMarcadoParaMorrer)
                return true;    // achamos e noAtual aponta o nó do procurado

            antecessor = atual; // mudaremos para o nível debaixo deste
            if (procurado.CompareTo(atual.Info) < 0)
                atual = atual.Esq;  // à esquerda, ficam os menores que noAtual
            else
                atual = atual.Dir;  // à direita, ficam os maiores que noAtual
        }
        return false;
    }

    public bool IncluirNovoDado(Dado novoDado)  // inclui em ABB sem balancear
    {
        if (Existe(novoDado)) // achou!
            return false;       // não incluiu pois já existe

        if (raiz == null)
            raiz = new NoArvoreAVL<Dado>(novoDado);
        else
          // não achou, mas o método Existe() ajustou antecessor e noAtual
          // antecessor é o pai do novo nó a incluir, decidimos para que
          // lado será feita a ligação com o nó antecessor
          if (novoDado.CompareTo(antecessor.Info) < 0)
            antecessor.Esq = new NoArvoreAVL<Dado>(novoDado);  // liga à esquerda
        else
            antecessor.Dir = new NoArvoreAVL<Dado>(novoDado);  // liga à direita
        return true;    // feita a inclusão
    }

    public bool Excluir(Dado dadoAExcluir)    // não balanceia, mas marca para exclusão futura!!!
    {
        if (Existe(dadoAExcluir)) // achou e já ajustou os ponteiros atual e antecessor
        {
            atual.NoMarcadoParaMorrer = true;
            return true;
        }
        return false;
    }

    public bool ExcluirNaoBalanceado(Dado dadoAExcluir)    // não balanceia!!!
    {
        if (Existe(dadoAExcluir)) // achou e já ajustou os ponteiros atual e antecessor
        {
            // se atual (nó procurado e encontrado) é a raiz, e a raiz
            // não tem filhos, simplesmente removemos essa raiz colocando
            // null no seu ponteiro (atributo raiz)
            if (atual == raiz && raiz.Esq == null && raiz.Dir == null)
            {
                raiz = null;    // removemos o único nó dessa árvore
                return true;
            }

            // atual não é a raiz 
            // se o nó a excluir é uma folha:
            if (atual.Esq == null && atual.Dir == null)   // é folha
            {
                // descobrimos de que lado do antecessor o atual está
                if (atual.Info.CompareTo(antecessor.Info) < 0)  // está à esquerda do seu pai?
                    antecessor.Esq = null;  // filho esquerdo é desligado do pai
                else
                    antecessor.Dir = null;  // filho direito é desligado do pai

                return true;  // nó desejado foi removido
            }

            // se o fluxo de execução chegar aqui, é porque o nó atual
            // tem, pelo menos, um filho

            // 2o caso: nó a excluir com um único filho
            if ((atual.Esq != null) != (atual.Dir != null))  // um único filho
            {
                // descobrir de que lado o atual (filho) é apontado pelo
                // antecessor (pai do nó a excluir)
                if (atual.Info.CompareTo(antecessor.Info) < 0)  // atual à esquerda do seu pai
                {
                    if (atual.Esq == null)
                        antecessor.Esq = atual.Dir;
                    else
                        antecessor.Esq = atual.Esq;
                }
                else   // pai aponta o atual (filho) à direita
                {                                 // e terá de apontar o neto
                    if (atual.Esq == null)
                        antecessor.Dir = atual.Dir;
                    else
                        antecessor.Dir = atual.Esq;
                }
                return true;  // excluiu nó com um único filho
            }

            // se o fluxo de execução chegou aqui:
            // 3o caso: nó a excluir tem os dois filhos (Esq e Dir)
            // apontamos o filho esquerdo do nó a excluir:
            NoArvoreAVL<Dado> antMaior = atual;
            var maiorDosMenores = atual.Esq;

            // agora vamos todo o possível para a direita para acharmos
            // a maior das chaves menores que a do nó a excluir (atual)
            while (maiorDosMenores.Dir != null)
            {
                antMaior = maiorDosMenores;
                maiorDosMenores = maiorDosMenores.Dir;  // à direita estão as maiores chaves
            }

            // substitui a informação do atual pela informação do nó com
            // a maior das menores chaves em relação ao atual
            atual.Info = maiorDosMenores.Info;
            if (antMaior.Esq == maiorDosMenores)  // só tinha um filho à esquerda sem filhos à direita
                antMaior.Esq = maiorDosMenores.Esq; // quando há só um nó à esquerda do nó a excluir
                                                    // esse nó pode ou não ter filhos à sua esquerda
            else
                antMaior.Dir = maiorDosMenores.Esq;   // neto do pai do nó excluído
            return true;
        }
        return false;
    }

    public void LerArquivoDeRegistros(string nomeArquivo)
    {
        raiz = null;    // arvore fica vazia
        Dado dado = new Dado();
        var origem = new FileStream(nomeArquivo, FileMode.OpenOrCreate);
        var arquivo = new BinaryReader(origem);
        int posicaoFinal = (int)origem.Length / dado.TamanhoRegistro - 1;
        Particionar(0, posicaoFinal, ref raiz);
        origem.Close();
        void Particionar(long inicio, long fim, ref NoArvoreAVL<Dado> noAtual)
        {
            if (inicio <= fim)
            {
                long meio = (inicio + fim) / 2;
                dado = new Dado(); // cria um objeto para armazenar os dados
                dado.LerRegistro(arquivo, meio); // 
                noAtual = new NoArvoreAVL<Dado>(dado);
                var novoEsq = noAtual.Esq;
                Particionar(inicio, meio - 1, ref novoEsq); // Particiona à esquerda 
                noAtual.Esq = novoEsq;
                var novoDir = noAtual.Dir;
                Particionar(meio + 1, fim, ref novoDir); // Particiona à direita 
                noAtual.Dir = novoDir;
            }
        }
    }

    public void GravarArquivoDeRegistros(string nomeArquivo)
    {
        var destino = new FileStream(nomeArquivo, FileMode.Create);
        var arquivo = new BinaryWriter(destino);
        GravarInOrdem(raiz);
        arquivo.Close();

        void GravarInOrdem(NoArvoreAVL<Dado> noAtual)
        {
            if (noAtual != null)
            {
                GravarInOrdem(noAtual.Esq);
                noAtual.Info.GravarRegistro(arquivo);
                GravarInOrdem(noAtual.Dir);
            }
        }
    }

    public int getAltura(NoArvoreAVL<Dado> no)
    {
        if (no != null)
            return no.Altura;
        return -1;
    }

    public void InserirBalanceado(Dado item)
    {
        raiz = InserirBalanceado(item, raiz);
    }

    private NoArvoreAVL<Dado> InserirBalanceado(Dado item, NoArvoreAVL<Dado> noAtual)
    {
        if (noAtual == null)
            noAtual = new NoArvoreAVL<Dado>(item);
        else
        {
            if (item.CompareTo(noAtual.Info) < 0)
            {
                noAtual.Esq = InserirBalanceado(item, noAtual.Esq);
                if (getAltura(noAtual.Esq) - getAltura(noAtual.Dir) == 2) // getAltura testa nulo
                    if (item.CompareTo(noAtual.Esq.Info) < 0)
                        noAtual = RotacaoSimplesComFilhoEsquerdo(noAtual);
                    else
                        noAtual = RotacaoDuplaComFilhoEsquerdo(noAtual);
            }
            else
            if (item.CompareTo(noAtual.Info) > 0)
            {
                noAtual.Dir = InserirBalanceado(item, noAtual.Dir);
                if (getAltura(noAtual.Dir) - getAltura(noAtual.Esq) == 2) // getAltura testa nulo
                    if (item.CompareTo(noAtual.Dir.Info) > 0)
                        noAtual = RotacaoSimplesComFilhoDireito(noAtual);
                    else
                        noAtual = RotacaoDuplaComFilhoDireito(noAtual);
            }
            //else ; - não faz nada, valor duplicado
            noAtual.Altura = Math.Max(getAltura(noAtual.Esq), getAltura(noAtual.Dir)) + 1;
        }
        return noAtual;
    }

    private NoArvoreAVL<Dado> RotacaoSimplesComFilhoEsquerdo(NoArvoreAVL<Dado> no)
    {
        NoArvoreAVL<Dado> temp = no.Esq;
        no.Esq = temp.Dir;
        temp.Dir = no;
        no.Altura = Math.Max(getAltura(no.Esq), getAltura(no.Dir)) + 1;
        temp.Altura = Math.Max(getAltura(temp.Esq), getAltura(no)) + 1;
        return temp;
    }
    private NoArvoreAVL<Dado> RotacaoSimplesComFilhoDireito(NoArvoreAVL<Dado> no)
    {
        NoArvoreAVL<Dado> temp = no.Dir;
        no.Dir = temp.Esq;
        temp.Esq = no;
        no.Altura = Math.Max(getAltura(no.Esq), getAltura(no.Dir)) + 1;
        temp.Altura = Math.Max(getAltura(temp.Dir), getAltura(no)) + 1;
        return temp;
    }
    private NoArvoreAVL<Dado> RotacaoDuplaComFilhoEsquerdo(NoArvoreAVL<Dado> no)
    {
        no.Esq = RotacaoSimplesComFilhoDireito(no.Esq);
        return RotacaoSimplesComFilhoEsquerdo(no);
    }
    private NoArvoreAVL<Dado> RotacaoDuplaComFilhoDireito(NoArvoreAVL<Dado> no)
    {
        no.Dir = RotacaoSimplesComFilhoEsquerdo(no.Dir);
        return RotacaoSimplesComFilhoDireito(no);
    }

}

