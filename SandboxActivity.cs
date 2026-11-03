using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content;
using System;
using System.Collections.Generic;

namespace RCT3AndroidTest;

[Activity(Label = "Modo Sandbox")]
public class SandboxActivity : Activity
{
    int dinheiro = 1000, visitantes = 0, precoIngresso = 20, satisfacao = 80;
    bool parqueAberto;
    string ferramenta = "Caminho";
    TextView saldo = null!, painel = null!, ferramentaTexto = null!;
    Button botaoEntrada = null!;
    ParkView parque = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var tela = new LinearLayout(this) { Orientation = Orientation.Vertical };
        tela.SetPadding(5, 5, 5, 5);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));
        var titulo = new TextView(this) { Text = "MEU PARQUE", TextSize = 24, Gravity = GravityFlags.Center };
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));
        saldo = CriarTexto(17); painel = CriarTexto(13); ferramentaTexto = CriarTexto(13);

        var scroll = new HorizontalScrollView(this);
        var lista = new LinearLayout(this) { Orientation = Orientation.Vertical };
        AdicionarLinha(lista, "Caminho", "Fila", "Árvore", "Remover");
        AdicionarLinha(lista, "Brinquedo", "Montanha-russa", "Roda-gigante", "Barco viking");
        AdicionarLinha(lista, "Carrossel", "Xícaras", "Torre de queda", "Bate-bate");
        AdicionarLinha(lista, "Loja de comida", "Loja de bebidas", "Banheiro");
        AdicionarLinha(lista, "Trilho reto", "Curva", "Subida", "Estação");
        scroll.AddView(lista);

        botaoEntrada = new Button(this) { Text = "Abrir parque" };
        botaoEntrada.Click += (s, e) => AlternarParque();
        var preco = new Button(this) { Text = "Alterar preço do ingresso" };
        preco.Click += (s, e) => AlterarPreco();
        var voltar = new Button(this) { Text = "Voltar ao menu" };
        voltar.Click += (s, e) => Finish();

        parque = new ParkView(this, AdicionarObjeto, AtualizarEconomia);
        tela.AddView(titulo); tela.AddView(saldo); tela.AddView(painel);
        tela.AddView(ferramentaTexto); tela.AddView(scroll); tela.AddView(botaoEntrada);
        tela.AddView(preco);
        tela.AddView(parque, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1));
        tela.AddView(voltar);
        SetContentView(tela); AtualizarPainel();
    }

    TextView CriarTexto(float tamanho)
    {
        var t = new TextView(this) { TextSize = tamanho, Gravity = GravityFlags.Center };
        t.SetTextColor(Color.White); return t;
    }

    void AdicionarLinha(LinearLayout lista, params string[] nomes)
    {
        var linha = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        foreach (string nome in nomes)
        {
            var b = new Button(this) { Text = nome, TextSize = 10 };
            b.SetPadding(2, 2, 2, 2);
            linha.AddView(b, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1));
            b.Click += (s, e) => { ferramenta = nome; ferramentaTexto.Text = "Ferramenta: " + nome; parque.SetTool(nome); };
        }
        lista.AddView(linha);
    }

    void AlterarPreco()
    {
        string[] opcoes = { "10", "20", "30", "40", "50" };
        new AlertDialog.Builder(this).SetTitle("Preço do ingresso")
            .SetItems(opcoes, (s, e) => { precoIngresso = int.Parse(opcoes[e.Which]); AtualizarPainel(); })
            .SetNegativeButton("Cancelar", (s, e) => { }).Show();
    }

    void AlternarParque()
    {
        parqueAberto = !parqueAberto;
        botaoEntrada.Text = parqueAberto ? "Fechar parque" : "Abrir parque";
        if (parqueAberto)
        {
            int novos = parque.IniciarParque(12);
            visitantes = novos;
            dinheiro += novos * precoIngresso;
            Toast.MakeText(this, novos > 0 ? "Parque aberto!" : "Construa caminhos para receber visitantes.", ToastLength.Short).Show();
        }
        else { parque.FecharParque(); visitantes = 0; Toast.MakeText(this, "Parque fechado.", ToastLength.Short).Show(); }
        AtualizarPainel();
    }

    void AdicionarObjeto(float x, float y)
    {
        if (ferramenta == "Remover") { parque.RemoverMaisProximo(x, y); return; }
        int custo = ferramenta switch
        {
            "Caminho" => 20, "Fila" => 25, "Árvore" => 30, "Brinquedo" => 150,
            "Montanha-russa" => 300, "Roda-gigante" => 250, "Barco viking" => 220,
            "Carrossel" => 180, "Xícaras" => 160, "Torre de queda" => 280, "Bate-bate" => 200,
            "Loja de comida" => 120, "Loja de bebidas" => 100, "Banheiro" => 100,
            "Trilho reto" => 35, "Curva" => 40, "Subida" => 45, "Estação" => 60, _ => 0
        };
        if (dinheiro < custo) { Toast.MakeText(this, "Dinheiro insuficiente!", ToastLength.Short).Show(); return; }
        dinheiro -= custo; parque.Adicionar(ferramenta, x, y); AtualizarPainel();
    }

    void AtualizarEconomia(int receita, int qtd, int sat)
    {
        dinheiro += receita; visitantes = parqueAberto ? qtd : 0; satisfacao = sat; AtualizarPainel();
    }

    void AtualizarPainel()
    {
        saldo.Text = "Dinheiro: $ " + dinheiro;
        painel.Text = "Visitantes: " + visitantes + " | Ingresso: $" + precoIngresso +
            " | Satisfação: " + satisfacao + "%" + (parqueAberto ? " | Aberto" : " | Fechado");
    }

    public class ParkView : View
    {
        const float C = 48f;
        readonly List<Item> itens = new();
        readonly List<Guest> guests = new();
        readonly Action<float, float> toque;
        readonly Action<int, int, int> economia;
        readonly Paint p = new() { AntiAlias = true };
        readonly Random rng = new();
        bool aberto;
        string ferramenta = "Caminho";
        long ultimo, ultimoCiclo;
        int receitaAcumulada, satisfacao = 80;

        class Item
        {
            public string Tipo = "";
            public float X, Y;
            public int Capacidade = 6, Ocupacao, Preco = 8, Espera;
            public long ProximoCiclo;
        }
        class Guest
        {
            public float X, Y, DX, DY, TX, TY, Progresso;
            public int Estado, Alvo = -1, Humor = 80, Tempo;
            public float Vel = .55f;
        }

        public ParkView(Context c, Action<float, float> cb, Action<int, int, int> eco) : base(c)
        { toque = cb; economia = eco; }

        public void SetTool(string t) { ferramenta = t; Invalidate(); }
        float Snap(float v, int limite) => Math.Clamp((float)(Math.Round(v / C) * C), C / 2, Math.Max(C / 2, limite - C / 2));

        public void Adicionar(string tipo, float x, float y)
        {
            float gx = Snap(x, Width), gy = Snap(y, Height);
            itens.Add(new Item { Tipo = tipo, X = gx, Y = gy,
                Capacidade = tipo == "Montanha-russa" ? 12 : tipo == "Roda-gigante" ? 10 : 6,
                Preco = tipo == "Montanha-russa" ? 15 : 8 });
            Invalidate();
        }

        public void RemoverMaisProximo(float x, float y)
        {
            int idx = -1; double menor = C * C;
            for (int i = 0; i < itens.Count; i++)
            {
                double dx = itens[i].X - x, dy = itens[i].Y - y, d = dx * dx + dy * dy;
                if (d < menor) { menor = d; idx = i; }
            }
            if (idx >= 0) itens.RemoveAt(idx);
            else Toast.MakeText(Context, "Nenhum objeto próximo.", ToastLength.Short).Show();
            Invalidate();
        }

        bool Caminho(float x, float y, bool fila = false)
        {
            foreach (var a in itens)
                if ((a.Tipo == "Caminho" || (fila && a.Tipo == "Fila")) &&
                    Math.Abs(a.X - x) < 2 && Math.Abs(a.Y - y) < 2) return true;
            return false;
        }

        List<(float x, float y)> Vizinhos(float x, float y)
        {
            var r = new List<(float, float)>();
            float[,] d = { { C, 0 }, { -C, 0 }, { 0, C }, { 0, -C } };
            for (int i = 0; i < 4; i++) if (Caminho(x + d[i, 0], y + d[i, 1], true)) r.Add((x + d[i, 0], y + d[i, 1]));
            return r;
        }

        public int IniciarParque(int quantidade)
        {
            aberto = true; guests.Clear();
            var caminhos = new List<Item>();
            foreach (var a in itens) if (a.Tipo == "Caminho") caminhos.Add(a);
            if (caminhos.Count == 0) { aberto = false; return 0; }
            int n = Math.Min(quantidade, Math.Max(3, caminhos.Count * 2));
            for (int i = 0; i < n; i++)
            {
                var a = caminhos[rng.Next(caminhos.Count)];
                guests.Add(new Guest { X = a.X, Y = a.Y, DX = a.X, DY = a.Y, TX = a.X, TY = a.Y });
            }
            ultimo = Android.OS.SystemClock.UptimeMillis(); ultimoCiclo = ultimo;
            return guests.Count;
        }

        public void FecharParque()
        {
            aberto = false; guests.Clear();
            foreach (var a in itens) { a.Ocupacao = 0; a.Espera = 0; }
            Invalidate();
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e?.Action == MotionEventActions.Down) { toque(e.GetX(), e.GetY()); return true; }
            return true;
        }

        bool EhAtracao(Item a) => a.Tipo == "Brinquedo" || a.Tipo == "Montanha-russa" ||
            a.Tipo == "Roda-gigante" || a.Tipo == "Barco viking" || a.Tipo == "Carrossel" ||
            a.Tipo == "Xícaras" || a.Tipo == "Torre de queda" || a.Tipo == "Bate-bate";

        int CaminhoPerto(Item a, bool aceitarFila)
        {
            for (int i = 0; i < itens.Count; i++)
                if ((itens[i].Tipo == "Caminho" || (aceitarFila && itens[i].Tipo == "Fila")) &&
                    Math.Abs(itens[i].X - a.X) <= C * 1.5f && Math.Abs(itens[i].Y - a.Y) <= C * 1.5f) return i;
            return -1;
        }

        int AtracaoPerto(Guest g)
        {
            for (int i = 0; i < itens.Count; i++)
                if (EhAtracao(itens[i]) && CaminhoPerto(itens[i], true) >= 0 &&
                    Math.Abs(itens[i].X - g.X) <= C * 1.5f && Math.Abs(itens[i].Y - g.Y) <= C * 1.5f) return i;
            return -1;
        }

        void Atualizar(float dt, long agora)
        {
            if (!aberto) return;
            foreach (var a in itens)
            {
                if (EhAtracao(a) && a.Ocupacao > 0 && agora >= a.ProximoCiclo)
                {
                    a.Ocupacao = 0; a.Espera = 0; a.ProximoCiclo = agora + 2500;
                }
            }

            foreach (var g in guests)
            {
                if (g.Estado == 2)
                {
                    g.Tempo--;
                    if (g.Tempo <= 0)
                    {
                        if (g.Alvo >= 0 && g.Alvo < itens.Count) itens[g.Alvo].Ocupacao = Math.Max(0, itens[g.Alvo].Ocupacao - 1);
                        g.Estado = 0; g.Alvo = -1; g.Humor = Math.Min(100, g.Humor + 4);
                    }
                    continue;
                }
                if (g.Estado == 3)
                {
                    g.Tempo--;
                    if (g.Tempo <= 0) { g.Estado = 0; g.Humor = Math.Min(100, g.Humor + 3); }
                    continue;
                }

                g.Progresso += dt * g.Vel;
                if (g.Progresso >= 1)
                {
                    g.X = g.TX; g.Y = g.TY;
                    int alvo = AtracaoPerto(g);
                    if (alvo >= 0 && rng.Next(100) < 22)
                    {
                        var a = itens[alvo];
                        int fila = CaminhoPerto(a, true);
                        bool temFila = fila >= 0 && itens[fila].Tipo == "Fila";
                        if (a.Ocupacao < a.Capacidade && (temFila || a.Ocupacao == 0))
                        {
                            a.Ocupacao++; a.ProximoCiclo = agora + (a.Tipo == "Montanha-russa" ? 7000 : 5000);
                            g.Estado = 2; g.Alvo = alvo; g.Tempo = a.Tipo == "Montanha-russa" ? 220 : 160;
                            receitaAcumulada += a.Preco; g.Humor = Math.Min(100, g.Humor + 2);
                            continue;
                        }
                        a.Espera++; g.Humor = Math.Max(25, g.Humor - 1);
                    }

                    int loja = -1;
                    for (int i = 0; i < itens.Count; i++)
                        if ((itens[i].Tipo == "Loja de comida" || itens[i].Tipo == "Loja de bebidas" || itens[i].Tipo == "Banheiro") &&
                            Math.Abs(itens[i].X - g.X) <= C && Math.Abs(itens[i].Y - g.Y) <= C) { loja = i; break; }
                    if (loja >= 0 && rng.Next(100) < 10)
                    {
                        var l = itens[loja];
                        if (l.Tipo == "Banheiro") { g.Tempo = 90; g.Estado = 3; }
                        else { receitaAcumulada += l.Tipo == "Loja de comida" ? 12 : 7; g.Tempo = 45; g.Estado = 3; }
                        continue;
                    }

                    var viz = Vizinhos(g.X, g.Y);
                    if (viz.Count > 0)
                    {
                        var prox = viz[rng.Next(viz.Count)];
                        g.DX = g.X; g.DY = g.Y; g.TX = prox.x; g.TY = prox.y; g.Progresso = 0;
                    }
                    else { g.Progresso = 0; g.Humor = Math.Max(20, g.Humor - 1); }
                }
                float q = Math.Clamp(g.Progresso, 0, 1);
                g.X = g.DX + (g.TX - g.DX) * q; g.Y = g.DY + (g.TY - g.DY) * q;
            }

            if (agora - ultimoCiclo > 5000)
            {
                int soma = 0;
                foreach (var g in guests) soma += g.Humor;
                satisfacao = guests.Count == 0 ? 80 : soma / guests.Count;
                int r = receitaAcumulada; receitaAcumulada = 0;
                economia(r, guests.Count, satisfacao);
                ultimoCiclo = agora;
            }
        }

        protected override void OnDraw(Canvas c)
        {
            base.OnDraw(c);
            long agora = Android.OS.SystemClock.UptimeMillis();
            float dt = ultimo == 0 ? .016f : Math.Clamp((agora - ultimo) / 1000f, 0, .05f);
            ultimo = agora; Atualizar(dt, agora);
            c.DrawColor(Color.ParseColor("#75B957"));
            p.Color = Color.ParseColor("#558A4C"); p.StrokeWidth = 1; p.SetStyle(Paint.Style.Stroke);
            for (float x = 0; x < Width; x += C) c.DrawLine(x, 0, x, Height, p);
            for (float y = 0; y < Height; y += C) c.DrawLine(0, y, Width, y, p);
            p.SetStyle(Paint.Style.Fill);
            p.Color = Color.ParseColor("#45B9DC"); c.DrawCircle(Width * .82f, Height * .18f, Math.Min(Width, Height) * .09f, p);

            foreach (var a in itens) DesenharItem(c, a, agora);
            if (aberto) foreach (var g in guests)
            {
                p.Color = g.Estado == 2 ? Color.ParseColor("#F4D27A") : Color.ParseColor("#3157B7");
                c.DrawCircle(g.X, g.Y, 7, p); p.Color = Color.ParseColor("#F0C49A"); c.DrawCircle(g.X, g.Y - 10, 5, p);
            }
            PostInvalidateDelayed(40);
        }

        void Bloco(Canvas c, Item a, string cor)
        {
            p.Color = Color.ParseColor(cor); c.DrawRect(a.X - 23, a.Y - 23, a.X + 23, a.Y + 23, p);
        }

        void DesenharItem(Canvas c, Item a, long t)
        {
            switch (a.Tipo)
            {
                case "Caminho": Bloco(c, a, "#D8BE91"); break;
                case "Fila":
                    Bloco(c, a, "#D6A84A"); p.Color = Color.ParseColor("#704A20"); p.StrokeWidth = 3; p.SetStyle(Paint.Style.Stroke);
                    c.DrawRect(a.X - 15, a.Y - 15, a.X + 15, a.Y + 15, p); p.SetStyle(Paint.Style.Fill); break;
                case "Árvore":
                    p.Color = Color.ParseColor("#754323"); c.DrawRect(a.X - 5, a.Y, a.X + 5, a.Y + 25, p);
                    p.Color = Color.ParseColor("#176B35"); c.DrawCircle(a.X, a.Y, 20, p); break;
                case "Loja de comida": Loja(c, a, "#D64B37", "COMIDA"); break;
                case "Loja de bebidas": Loja(c, a, "#36A9C7", "BEBIDA"); break;
                case "Banheiro": Loja(c, a, "#3979D4", "WC"); break;
                case "Montanha-russa":
                    p.Color = Color.ParseColor("#444444"); p.StrokeWidth = 5; p.SetStyle(Paint.Style.Stroke);
                    c.DrawLine(a.X - 25, a.Y + 15, a.X - 12, a.Y - 22, p); c.DrawLine(a.X - 12, a.Y - 22, a.X, a.Y + 15, p);
                    c.DrawLine(a.X, a.Y + 15, a.X + 12, a.Y - 22, p); c.DrawLine(a.X + 12, a.Y - 22, a.X + 25, a.Y + 15, p);
                    p.SetStyle(Paint.Style.Fill); p.Color = Color.ParseColor("#D92727");
                    float ang = (t % 4000) / 4000f * (float)(Math.PI * 2);
                    c.DrawCircle(a.X + (float)Math.Cos(ang) * 22, a.Y + (float)Math.Sin(ang) * 17, 7, p); break;
                case "Roda-gigante":
                    p.Color = Color.ParseColor("#555555"); p.StrokeWidth = 4; p.SetStyle(Paint.Style.Stroke); c.DrawCircle(a.X, a.Y - 5, 25, p); p.SetStyle(Paint.Style.Fill);
                    for (int i = 0; i < 8; i++) { double z = i * Math.PI / 4 + t / 1000.0; p.Color = Color.ParseColor("#D94B36"); c.DrawCircle(a.X + (float)Math.Cos(z) * 25, a.Y - 5 + (float)Math.Sin(z) * 25, 5, p); } break;
                default:
                    p.Color = Color.ParseColor("#E8B52E"); c.DrawCircle(a.X, a.Y, 23, p);
                    p.Color = Color.ParseColor("#D94B36"); c.DrawCircle(a.X, a.Y, 9, p); break;
            }
            if (EhAtracao(a))
            {
                p.Color = Color.White; p.TextSize = 11; p.TextAlign = Paint.Align.Center;
                c.DrawText(a.Ocupacao + "/" + a.Capacidade, a.X, a.Y + 38, p);
            }
        }

        void Loja(Canvas c, Item a, string cor, string nome)
        {
            p.Color = Color.ParseColor(cor); c.DrawRect(a.X - 20, a.Y - 18, a.X + 20, a.Y + 18, p);
            p.Color = Color.White; p.TextSize = 9; p.TextAlign = Paint.Align.Center; c.DrawText(nome, a.X, a.Y + 3, p);
        }
    }
}
Content;
using System;
using System.Collections.Generic;

namespace RCT3AndroidTest;

[Activity(Label = "Modo Sandbox")]
public class SandboxActivity : Activity
{
    int dinheiro = 1000;
    int visitantes = 0;
    int precoIngresso = 20;
    int satisfacao = 80;
    bool parqueAberto = false;

    string ferramenta = "Caminho";

    TextView saldo = null!;
    TextView painel = null!;
    TextView ferramentaTexto = null!;
    Button botaoEntrada = null!;
    ParkView parque = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var tela = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        tela.SetPadding(5, 5, 5, 5);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));

        var titulo = new TextView(this)
        {
            Text = "MEU PARQUE",
            TextSize = 24,
            Gravity = GravityFlags.Center
        };
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));

        saldo = CriarTexto(17);
        painel = CriarTexto(13);
        ferramentaTexto = CriarTexto(13);

        var ferramentas = new HorizontalScrollView(this);
        var listaBotoes = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };

        AdicionarLinha(listaBotoes,
            "Caminho", "Fila", "Árvore", "Remover");

        AdicionarLinha(listaBotoes,
            "Brinquedo", "Montanha-russa",
            "Roda-gigante", "Barco viking");

        AdicionarLinha(listaBotoes,
            "Carrossel", "Xícaras", "Torre de queda",
            "Bate-bate");

        AdicionarLinha(listaBotoes,
            "Loja de comida", "Loja de bebidas",
            "Banheiro");

        AdicionarLinha(listaBotoes,
            "Trilho reto", "Curva",
            "Subida", "Estação");

        ferramentas.AddView(listaBotoes);

        botaoEntrada = new Button(this)
        {
            Text = "Abrir parque"
        };
        botaoEntrada.Click += (s, e) => AlternarParque();

        var preco = new Button(this)
        {
            Text = "Alterar preço do ingresso"
        };
        preco.Click += (s, e) => AlterarPreco();

        var voltar = new Button(this)
        {
            Text = "Voltar ao menu"
        };
        voltar.Click += (s, e) => Finish();

        parque = new ParkView(this, AdicionarObjeto);

        tela.AddView(titulo);
        tela.AddView(saldo);
        tela.AddView(painel);
        tela.AddView(ferramentaTexto);
        tela.AddView(ferramentas);
        tela.AddView(botaoEntrada);
        tela.AddView(preco);

        tela.AddView(parque, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1));

        tela.AddView(voltar);

        SetContentView(tela);
        AtualizarPainel();
    }

    TextView CriarTexto(float tamanho)
    {
        var texto = new TextView(this)
        {
            TextSize = tamanho,
            Gravity = GravityFlags.Center
        };
        texto.SetTextColor(Color.White);
        return texto;
    }

    void AdicionarLinha(LinearLayout lista, params string[] nomes)
    {
        var linha = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal
        };

        foreach (string nome in nomes)
        {
            var botao = new Button(this)
            {
                Text = nome,
                TextSize = 10
            };
            botao.SetPadding(2, 2, 2, 2);

            linha.AddView(botao, new LinearLayout.LayoutParams(
                0, ViewGroup.LayoutParams.WrapContent, 1));

            botao.Click += (s, e) =>
            {
                ferramenta = nome;
                ferramentaTexto.Text = "Ferramenta: " + nome;
                parque.SetTool(nome);
            };
        }

        lista.AddView(linha);
    }

    void AlterarPreco()
    {
        var opcoes = new string[] { "10", "20", "30", "40", "50" };

        var dialogo = new AlertDialog.Builder(this);
        dialogo.SetTitle("Preço do ingresso");

        dialogo.SetItems(opcoes, (s, e) =>
        {
            precoIngresso = int.Parse(opcoes[e.Which]);
            AtualizarPainel();
        });

        dialogo.SetNegativeButton("Cancelar", (s, e) => { });
        dialogo.Show();
    }

    void AlternarParque()
    {
        parqueAberto = !parqueAberto;

        if (parqueAberto)
        {
            visitantes = 5;
            dinheiro += visitantes * precoIngresso;
            botaoEntrada.Text = "Fechar parque";

            Toast.MakeText(this,
                "Parque aberto!", ToastLength.Short).Show();
        }
        else
        {
            visitantes = 0;
            botaoEntrada.Text = "Abrir parque";

            Toast.MakeText(this,
                "Parque fechado.", ToastLength.Short).Show();
        }

        parque.SetParkOpen(parqueAberto, visitantes);
        AtualizarPainel();
    }

    void AdicionarObjeto(float x, float y)
    {
        if (ferramenta == "Remover")
        {
            parque.RemoverMaisProximo(x, y);
            return;
        }

        int preco = ferramenta switch
        {
            "Caminho" => 20,
            "Fila" => 25,
            "Árvore" => 30,
            "Brinquedo" => 150,
            "Montanha-russa" => 300,
            "Roda-gigante" => 250,
            "Barco viking" => 220,
            "Carrossel" => 180,
            "Xícaras" => 160,
            "Torre de queda" => 280,
            "Bate-bate" => 200,
            "Loja de comida" => 120,
            "Loja de bebidas" => 100,
            "Banheiro" => 100,
            "Trilho reto" => 35,
            "Curva" => 40,
            "Subida" => 45,
            "Estação" => 60,
            _ => 0
        };

        if (dinheiro < preco)
        {
            Toast.MakeText(this,
                "Dinheiro insuficiente!",
                ToastLength.Short).Show();
            return;
        }

        dinheiro -= preco;
        parque.Adicionar(ferramenta, x, y);
        AtualizarPainel();
    }

    void AtualizarPainel()
    {
        saldo.Text = "Dinheiro: $ " + dinheiro;

        painel.Text =
            "Visitantes: " + visitantes +
            " | Ingresso: $" + precoIngresso +
            " | Satisfação: " + satisfacao + "%" +
            (parqueAberto ? " | Aberto" : " | Fechado");
    }

    public class ParkView : View
    {
        const float Celula = 48f;

        readonly List<ParkItem> objetos = new();
        readonly List<Visitante> pessoas = new();
        readonly Action<float, float> aoTocar;
        readonly Paint tinta = new() { AntiAlias = true };
        readonly Random aleatorio = new();

        bool aberto;
        string ferramenta = "Caminho";
        long ultimoTempo;

        class ParkItem
        {
            public string Tipo = "";
            public float X;
            public float Y;
        }

        class Visitante
        {
            public float X, Y;
            public float DeX, DeY;
            public float ParaX, ParaY;
            public float Progresso = 1;
            public float Velocidade = 0.65f;
        }

        public ParkView(Context context, Action<float, float> callback)
            : base(context)
        {
            aoTocar = callback;
        }

        public void SetTool(string nome)
        {
            ferramenta = nome;
            Invalidate();
        }

        public void SetParkOpen(bool valor, int quantidade)
        {
            aberto = valor;
            pessoas.Clear();

            if (aberto)
                CriarVisitantes(quantidade);

            ultimoTempo = Android.OS.SystemClock.UptimeMillis();
            Invalidate();
        }

        float Snap(float valor, int limite)
        {
            float ajustado =
                (float)(Math.Round(valor / Celula) * Celula);

            return Math.Clamp(ajustado, Celula / 2,
                Math.Max(Celula / 2, limite - Celula / 2));
        }

        public void Adicionar(string tipo, float x, float y)
        {
            float gx = Snap(x, Width);
            float gy = Snap(y, Height);

            objetos.Add(new ParkItem
            {
                Tipo = tipo,
                X = gx,
                Y = gy
            });

            Invalidate();
        }

        public void RemoverMaisProximo(float x, float y)
        {
            int indice = -1;
            double menor = Celula * Celula;

            for (int i = 0; i < objetos.Count; i++)
            {
                double dx = objetos[i].X - x;
                double dy = objetos[i].Y - y;
                double distancia = dx * dx + dy * dy;

                if (distancia < menor)
                {
                    menor = distancia;
                    indice = i;
                }
            }

            if (indice >= 0)
            {
                objetos.RemoveAt(indice);
                Invalidate();
            }
            else
            {
                Toast.MakeText(Context,
                    "Nenhum objeto próximo.",
                    ToastLength.Short).Show();
            }
        }

        bool TemCaminho(float x, float y)
        {
            foreach (var obj in objetos)
            {
                if ((obj.Tipo == "Caminho" ||
                     obj.Tipo == "Fila") &&
                    Math.Abs(obj.X - x) < 2 &&
                    Math.Abs(obj.Y - y) < 2)
                    return true;
            }

            return false;
        }

        List<(float x, float y)> CaminhosVizinhos(float x, float y)
        {
            var resultado = new List<(float, float)>();

            float[,] direcoes =
            {
                { Celula, 0 },
                { -Celula, 0 },
                { 0, Celula },
                { 0, -Celula }
            };

            for (int i = 0; i < 4; i++)
            {
                float nx = x + direcoes[i, 0];
                float ny = y + direcoes[i, 1];

                if (TemCaminho(nx, ny))
                    resultado.Add((nx, ny));
            }

            return resultado;
        }

        void CriarVisitantes(int quantidade)
        {
            var caminhos = new List<ParkItem>();

            foreach (var obj in objetos)
            {
                if (obj.Tipo == "Caminho")
                    caminhos.Add(obj);
            }

            if (caminhos.Count == 0)
                return;

            for (int i = 0; i < quantidade; i++)
            {
                var inicio = caminhos[aleatorio.Next(caminhos.Count)];

                pessoas.Add(new Visitante
                {
                    X = inicio.X,
                    Y = inicio.Y,
                    DeX = inicio.X,
                    DeY = inicio.Y,
                    ParaX = inicio.X,
                    ParaY = inicio.Y
                });
            }
        }

        void AtualizarVisitantes(float delta)
        {
            if (!aberto)
                return;

            foreach (var pessoa in pessoas)
            {
                pessoa.Progresso += delta * pessoa.Velocidade;

                if (pessoa.Progresso >= 1f)
                {
                    pessoa.X = pessoa.ParaX;
                    pessoa.Y = pessoa.ParaY;

                    var vizinhos = CaminhosVizinhos(
                        pessoa.X, pessoa.Y);

                    if (vizinhos.Count > 0)
                    {
                        var proximo =
                            vizinhos[aleatorio.Next(vizinhos.Count)];

                        pessoa.DeX = pessoa.X;
                        pessoa.DeY = pessoa.Y;
                        pessoa.ParaX = proximo.x;
                        pessoa.ParaY = proximo.y;
                        pessoa.Progresso = 0;
                    }
                    else
                    {
                        pessoa.Progresso = 1;
                    }
                }

                float p = Math.Clamp(pessoa.Progresso, 0f, 1f);

                pessoa.X = pessoa.DeX +
                    (pessoa.ParaX - pessoa.DeX) * p;

                pessoa.Y = pessoa.DeY +
                    (pessoa.ParaY - pessoa.DeY) * p;
            }
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e?.Action == MotionEventActions.Down)
            {
                aoTocar(e.GetX(), e.GetY());
                return true;
            }

            return true;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            float w = Width;
            float h = Height;

            long agora = Android.OS.SystemClock.UptimeMillis();

            float delta = ultimoTempo == 0
                ? 0.016f
                : Math.Clamp((agora - ultimoTempo) / 1000f,
                    0f, 0.05f);

            ultimoTempo = agora;

            AtualizarVisitantes(delta);

            canvas.DrawColor(Color.ParseColor("#75B957"));

            DesenharGrade(canvas, w, h);
            DesenharLago(canvas, w, h);

            DesenharTrilhos(canvas, agora);

            foreach (var obj in objetos)
            {
                switch (obj.Tipo)
                {
                    case "Caminho":
                        DesenharBloco(canvas, obj.X, obj.Y,
                            "#D8BE91");
                        break;

                    case "Fila":
                        DesenharFila(canvas, obj.X, obj.Y);
                        break;

                    case "Árvore":
                        DesenharArvore(canvas, obj.X, obj.Y);
                        break;

                    case "Brinquedo":
                        DesenharBrinquedo(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Montanha-russa":
                        DesenharMontanha(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Roda-gigante":
                        DesenharRodaGigante(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Barco viking":
                        DesenharBarco(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Carrossel":
                        DesenharCarrossel(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Xícaras":
                        DesenharXicaras(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Torre de queda":
                        DesenharTorre(canvas, obj.X, obj.Y, agora);
                        break;

                    case "Bate-bate":
                        DesenharBateBate(canvas, obj.X, obj.Y);
                        break;

                    case "Loja de comida":
                        DesenharLoja(canvas, obj.X, obj.Y, "COMIDA");
                        break;

                    case "Loja de bebidas":
                        DesenharLoja(canvas, obj.X, obj.Y, "BEBIDAS");
                        break;

                    case "Banheiro":
                        DesenharLoja(canvas, obj.X, obj.Y, "WC");
                        break;
                }
            }

            if (aberto)
                DesenharVisitantes(canvas);

            PostInvalidateDelayed(30);
        }

        void DesenharGrade(Canvas canvas, float w, float h)
        {
            tinta.Color = Color.ParseColor("#558A4C");
            tinta.StrokeWidth = 1;
            tinta.SetStyle(Paint.Style.Stroke);

            for (float x = 0; x < w; x += Celula)
                canvas.DrawLine(x, 0, x, h, tinta);

            for (float y = 0; y < h; y += Celula)
                canvas.DrawLine(0, y, w, y, tinta);

            tinta.SetStyle(Paint.Style.Fill);
        }

        void DesenharLago(Canvas canvas, float w, float h)
        {
            tinta.Color = Color.ParseColor("#45B9DC");
            canvas.DrawCircle(w * 0.82f, h * 0.18f,
                Math.Min(w, h) * 0.09f, tinta);
        }

        void DesenharBloco(Canvas canvas, float x, float y, string cor)
        {
            tinta.Color = Color.ParseColor(cor);
            canvas.DrawRect(x - 24, y - 24, x + 24, y + 24, tinta);
        }

        void DesenharFila(Canvas canvas, float x, float y)
        {
            DesenharBloco(canvas, x, y, "#D6A84A");

            tinta.Color = Color.ParseColor("#704A20");
            tinta.StrokeWidth = 3;
            tinta.SetStyle(Paint.Style.Stroke);
            canvas.DrawRect(x - 15, y - 15, x + 15, y + 15, tinta);
            tinta.SetStyle(Paint.Style.Fill);
        }

        void DesenharArvore(Canvas canvas, float x, float y)
        {
            tinta.Color = Color.ParseColor("#754323");
            canvas.DrawRect(x - 5, y, x + 5, y + 25, tinta);

            tinta.Color = Color.ParseColor("#176B35");
            canvas.DrawCircle(x, y, 20, tinta);

            tinta.Color = Color.ParseColor("#278C43");
            canvas.DrawCircle(x - 5, y - 6, 10, tinta);
        }

        void DesenharBrinquedo(Canvas canvas, float x, float y, long tempo)
        {
            float angulo = (tempo % 3000) * 360f / 3000f;

            canvas.Save();
            canvas.Rotate(angulo, x, y);

            tinta.Color = Color.ParseColor("#E8B52E");
            canvas.DrawCircle(x, y, 25, tinta);

            tinta.Color = Color.ParseColor("#D94B36");
            tinta.StrokeWidth = 6;
            canvas.DrawLine(x - 23, y, x + 23, y, tinta);
            canvas.DrawLine(x, y - 23, x, y + 23, tinta);

            canvas.Restore();

            tinta.Color = Color.White;
            canvas.DrawCircle(x, y, 7, tinta);
        }

        void DesenharRodaGigante(Canvas canvas, float x, float y, long tempo)
        {
            float angulo = (tempo % 7000) * 360f / 7000f;

            tinta.Color = Color.ParseColor("#555555");
            tinta.StrokeWidth = 5;
            tinta.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(x - 20, y + 25, x, y - 25, tinta);
            canvas.DrawLine(x + 20, y + 25, x, y - 25, tinta);
            canvas.DrawCircle(x, y - 5, 27, tinta);

            tinta.SetStyle(Paint.Style.Fill);

            for (int i = 0; i < 8; i++)
            {
                float a = (float)(i * Math.PI / 4) +
                    angulo * (float)Math.PI / 180f;

                float px = x + (float)Math.Cos(a) * 27;
                float py = y - 5 + (float)Math.Sin(a) * 27;

                tinta.Color = Color.ParseColor("#D94B36");
                canvas.DrawCircle(px, py, 6, tinta);
            }

            tinta.Color = Color.White;
            canvas.DrawCircle(x, y - 5, 5, tinta);
        }

        void DesenharBarco(Canvas canvas, float x, float y, long tempo)
        {
            float movimento = (float)Math.Sin(tempo / 500.0) * 18;

            tinta.Color = Color.ParseColor("#444444");
            tinta.StrokeWidth = 5;
            canvas.DrawLine(x - 30, y + 20, x, y - 25, tinta);
            canvas.DrawLine(x + 30, y + 20, x, y - 25, tinta);

            tinta.Color = Color.ParseColor("#A74725");
            canvas.Save();
            canvas.Rotate(movimento, x, y);
            canvas.DrawOval(
                new RectF(x - 28, y - 8, x + 28, y + 8), tinta);
            canvas.Restore();
        }

        void DesenharCarrossel(Canvas canvas, float x, float y, long tempo)
        {
            tinta.Color = Color.ParseColor("#E7C23C");
            canvas.DrawCircle(x, y, 25, tinta);

            for (int i = 0; i < 5; i++)
            {
                float a = tempo / 700f + i * 1.25f;
                float px = x + (float)Math.Cos(a) * 18;
                float py = y + (float)Math.Sin(a) * 18;

                tinta.Color = Color.ParseColor("#D83D45");
                canvas.DrawCircle(px, py, 6, tinta);
            }
        }

        void DesenharXicaras(Canvas canvas, float x, float y, long tempo)
        {
            tinta.Color = Color.ParseColor("#E9D7A0");
            canvas.DrawCircle(x, y, 25, tinta);

            for (int i = 0; i < 4; i++)
            {
                float a = tempo / 400f + i * 1.57f;
                float px = x + (float)Math.Cos(a) * 15;
                float py = y + (float)Math.Sin(a) * 15;

                tinta.Color = Color.ParseColor("#A53BA8");
                canvas.DrawCircle(px, py, 8, tinta);
            }
        }

        void DesenharTorre(Canvas canvas, float x, float y, long tempo)
        {
            tinta.Color = Color.ParseColor("#555555");
            tinta.StrokeWidth = 7;
            canvas.DrawLine(x - 13, y - 28, x - 13, y + 28, tinta);
            canvas.DrawLine(x + 13, y - 28, x + 13, y + 28, tinta);

            float movimento = (float)Math.Abs(Math.Sin(tempo / 800.0)) * 35;

            tinta.Color = Color.ParseColor("#E34B37");
            canvas.DrawRect(x - 17, y - 25 + movimento,
                x + 17, y - 15 + movimento, tinta);
        }

        void DesenharBateBate(Canvas canvas, float x, float y)
        {
            tinta.Color = Color.ParseColor("#555555");
            canvas.DrawRect(x - 27, y - 20, x + 27, y + 20, tinta);

            tinta.Color = Color.ParseColor("#E8C53A");
            canvas.DrawRect(x - 18, y - 12, x - 3, y + 5, tinta);

            tinta.Color = Color.ParseColor("#3979D4");
            canvas.DrawRect(x + 3, y - 12, x + 18, y + 5, tinta);
        }

        void DesenharLoja(Canvas canvas, float x, float y, string nome)
        {
            tinta.Color = nome == "WC"
                ? Color.ParseColor("#3979D4")
                : nome == "COMIDA"
                    ? Color.ParseColor("#D64B37")
                    : Color.ParseColor("#36A9C7");

            canvas.DrawRect(x - 20, y - 18, x + 20, y + 18, tinta);

            tinta.Color = Color.White;
            tinta.TextSize = 9;
            tinta.TextAlign = Paint.Align.Center;
            canvas.DrawText(nome, x, y + 3, tinta);
        }

        void DesenharTrilhos(Canvas canvas, long tempo)
        {
            var trilhos = new List<ParkItem>();

            foreach (var obj in objetos)
            {
                if (obj.Tipo == "Trilho reto" ||
                    obj.Tipo == "Curva" ||
                    obj.Tipo == "Subida" ||
                    obj.Tipo == "Estação")
                {
                    trilhos.Add(obj);
                }
            }

            if (trilhos.Count == 0)
                return;

            tinta.Color = Color.ParseColor("#333333");
            tinta.StrokeWidth = 7;
            tinta.SetStyle(Paint.Style.Stroke);

            foreach (var trilho in trilhos)
            {
                canvas.DrawCircle(trilho.X, trilho.Y, 13, tinta);
            }

            for (int i = 1; i < trilhos.Count; i++)
            {
                canvas.DrawLine(
                    trilhos[i - 1].X, trilhos[i - 1].Y,
                    trilhos[i].X, trilhos[i].Y, tinta);
            }

            tinta.SetStyle(Paint.Style.Fill);

            if (trilhos.Count >= 2)
            {
                float progresso = (tempo % 4000) / 4000f;
                float posicao = progresso * (trilhos.Count - 1);

                int indice = Math.Min(
                    (int)posicao, trilhos.Count - 2);

                float mistura = posicao - indice;

                float cx = trilhos[indice].X +
                    (trilhos[indice + 1].X - trilhos[indice].X) * mistura;

                float cy = trilhos[indice].Y +
                    (trilhos[indice + 1].Y - trilhos[indice].Y) * mistura;

                tinta.Color = Color.ParseColor("#D92727");
                canvas.DrawRoundRect(
                    new RectF(cx - 9, cy - 7, cx + 9, cy + 7),
                    4, 4, tinta);
            }
        }

        void DesenharMontanha(Canvas canvas, float x, float y, long tempo)
        {
            tinta.Color = Color.ParseColor("#444444");
            tinta.StrokeWidth = 6;
            tinta.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(x - 30, y + 15, x - 18, y - 25, tinta);
            canvas.DrawLine(x - 18, y - 25, x, y + 15, tinta);
            canvas.DrawLine(x, y + 15, x + 18, y - 25, tinta);
            canvas.DrawLine(x + 18, y - 25, x + 30, y + 15, tinta);

            tinta.SetStyle(Paint.Style.Fill);

            float progresso = (tempo % 5000) / 5000f;
            float angulo = progresso * (float)(Math.PI * 2);

            float cx = x + (float)Math.Cos(angulo) * 27f;
            float cy = y + (float)Math.Sin(angulo) * 20f;

            tinta.Color = Color.ParseColor("#D92727");
            canvas.DrawRoundRect(
                new RectF(cx - 10, cy - 7, cx + 10, cy + 5),
                4, 4, tinta);

            tinta.Color = Color.ParseColor("#F5D0A0");
            canvas.DrawCircle(cx - 5, cy - 8, 3, tinta);
            canvas.DrawCircle(cx + 4, cy - 8, 3, tinta);
        }

        void DesenharVisitantes(Canvas canvas)
        {
            foreach (var pessoa in pessoas)
            {
                tinta.Color = Color.ParseColor("#3157B7");
                canvas.DrawCircle(pessoa.X, pessoa.Y, 7, tinta);

                tinta.Color = Color.ParseColor("#F0C49A");
                canvas.DrawCircle(pessoa.X, pessoa.Y - 10, 5, tinta);

                tinta.Color = Color.ParseColor("#333333");
                tinta.StrokeWidth = 2;

                canvas.DrawLine(pessoa.X, pessoa.Y + 5,
                    pessoa.X - 4, pessoa.Y + 13, tinta);

                canvas.DrawLine(pessoa.X, pessoa.Y + 5,
                    pessoa.X + 4, pessoa.Y + 13, tinta);
            }
        }
    }
}
