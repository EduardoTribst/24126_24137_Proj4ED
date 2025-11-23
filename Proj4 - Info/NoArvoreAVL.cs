// Eduardo 24126
// Júlio 24137

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class NoArvoreAVL<Dado> : IComparable<NoArvoreAVL<Dado>>
             where Dado : IComparable<Dado>, IRegistro, new()
{
    Dado info;           // informação armazenada neste nó da árvore
    private NoArvoreAVL<Dado> esq, dir;
    int altura;
    private bool noMarcadoParaMorrer;

    public NoArvoreAVL(Dado informacao)
    {
        info = informacao;
        esq = dir = null;
        noMarcadoParaMorrer = false;
        altura = 0;
    }

    public NoArvoreAVL(Dado dados, NoArvoreAVL<Dado> esquerdo, NoArvoreAVL<Dado> direito,
                       int altura)
    {
        this.Info = dados;
        this.Esq = esquerdo;
        this.Dir = direito;
        this.altura = altura;
        noMarcadoParaMorrer = false;
    }

    public Dado Info
    {
        get => info;
        set => info = value;
    }
    public NoArvoreAVL<Dado> Esq { get => esq; set => esq = value; }
    public NoArvoreAVL<Dado> Dir { get => dir; set => dir = value; }
    public int Altura { get => altura; set => altura = value; }
    public bool NoMarcadoParaMorrer { get => noMarcadoParaMorrer; set => noMarcadoParaMorrer = value; }

    public int CompareTo(NoArvoreAVL<Dado> o)
    {
        return Info.CompareTo(o.Info);
    }

    public bool Equals(NoArvoreAVL<Dado> o)
    {
        return this.Info.Equals(o.Info);
    }

}
