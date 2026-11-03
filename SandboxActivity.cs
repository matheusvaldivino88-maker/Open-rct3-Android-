
using Android.App;
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
    int dinheiro = 1000;
    int visitantes = 0;
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

        var tela = new LinearLayout(this);
        tela.Orientation = Orientation.Vertical;
        tela.SetPadding(8, 8, 8, 8);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));

        var titulo = new TextView(this);
        titulo.Text = "MEU PARQUE";
        titulo.TextSize = 25;
        titulo.Gravity = GravityFlags.Center;
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));

        saldo = CriarTexto(18);
        painel = CriarTexto(14);
        ferramentaTexto = CriarTexto(14);

        parque = new ParkView(this, AdicionarObjeto);

        var linha1 = CriarLinha();
        AdicionarBotao(linha1, "Caminho");
        AdicionarBotao(linha1, "Árvore");
        AdicionarBotao(linha1, "Brinquedo");

        var linha2 = CriarLinha();
        AdicionarBotao(linha2, "Montanha-russa");
        AdicionarBotao(linha2, "Remover");

        botaoEntrada = new Button(this);
        botaoEntrada.Text = "Abrir parque";
        botaoEntrada.Click += (s, e) => AlternarParque();

        var voltar = new Button(this);
        voltar.Text = "Voltar ao menu";
        voltar.Click += (s, e) => Finish();

        tela.AddView(titulo);
        tela.AddView(saldo);
        tela.AddView(painel);
        tela.AddView(ferramentaTexto);
        tela.AddView(linha1);
        tela.AddView(linha2);
        tela.AddView(botaoEntrada);

        tela.AddView(parque, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1));

        tela.AddView(voltar);

        SetContentView(tela);
        AtualizarPainel();
    }

    TextView CriarTexto(float tamanho)
    {
        var texto = new TextView(this);
        texto.TextSize = tamanho;
        texto.Gravity = GravityFlags.Center;
        texto.SetTextColor(Color.White);
        return texto;
    }

    LinearLayout CriarLinha()
    {
        var linha = new LinearLayout(this);
        linha.Orientation = Orientation.Horizontal;
        return linha;
    }

    void AdicionarBotao(LinearLayout linha, string nome)
    {
        var botao = new Button(this);
        botao.Text = nome;
        botao.TextSize = 11;
        botao.SetPadding(2, 2, 2, 2);

        linha.AddView(botao, new LinearLayout.LayoutParams(
            0, ViewGroup.LayoutParams.WrapContent, 1));

        botao.Click += (s, e) =>
        {
            ferramenta = nome;
            ferramentaTexto.Text = "Ferramenta: " + ferramenta;
            parque.SetTool(nome);
        };
    }

    void AlternarParque()
    {
        parqueAberto = !parqueAberto;

        if (parqueAberto)
        {
            visitantes = 5;
            dinheiro += visitantes * 20;
            botaoEntrada.Text = "Fechar parque";

            Toast.MakeText(this,
                "Parque aberto! Ingressos vendidos.",
                ToastLength.Short).Show();
        }
        else
        {
            visitantes = 0;
            botaoEntrada.Text = "Abrir parque";

            Toast.MakeText(this,
                "Parque fechado.",
                ToastLength.Short).Show();
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
            "Árvore" => 30,
            "Brinquedo" => 150,
            "Montanha-russa" => 300,
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
        painel.Text = "Visitantes: " + visitantes +
            (parqueAberto ? "  |  Parque aberto" : "  |  Parque fechado");
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
            public float X;
            public float Y;
            public float DeX;
            public float DeY;
            public float ParaX;
            public float ParaY;
            public float Progresso;
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
            {
                CriarVisitantes(quantidade);
            }

            ultimoTempo = Android.OS.SystemClock.UptimeMillis();
            Invalidate();
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

        float Snap(float valor, int limite)
        {
            float ajustado =
                (float)(Math.Round(valor / Celula) * Celula);

            return Math.Clamp(ajustado, Celula / 2,
                Math.Max(Celula / 2, limite - Celula / 2));
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
                if (obj.Tipo == "Caminho" &&
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

                var pessoa = new Visitante
                {
                    X = inicio.X,
                    Y = inicio.Y,
                    DeX = inicio.X,
                    DeY = inicio.Y,
                    ParaX = inicio.X,
                    ParaY = inicio.Y,
                    Progresso = 1f
                };

                pessoas.Add(pessoa);
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

                    var vizinhos = CaminhosVizinhos(pessoa.X, pessoa.Y);

                    if (vizinhos.Count > 0)
                    {
                        var proximo = vizinhos[aleatorio.Next(vizinhos.Count)];

                        pessoa.DeX = pessoa.X;
                        pessoa.DeY = pessoa.Y;
                        pessoa.ParaX = proximo.x;
                        pessoa.ParaY = proximo.y;
                        pessoa.Progresso = 0f;
                    }
                    else
                    {
                        pessoa.Progresso = 1f;
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
                : Math.Clamp((agora - ultimoTempo) / 1000f, 0f, 0.05f);

            ultimoTempo = agora;

            AtualizarVisitantes(delta);

            canvas.DrawColor(Color.ParseColor("#75B957"));

            DesenharGrade(canvas, w, h);
            DesenharLago(canvas, w, h);

            foreach (var obj in objetos)
            {
                switch (obj.Tipo)
                {
                    case "Caminho":
                        DesenharCaminho(canvas, obj.X, obj.Y);
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

        void DesenharCaminho(Canvas canvas, float x, float y)
        {
            tinta.Color = Color.ParseColor("#D8BE91");

            var area = new RectF(
                x - Celula / 2, y - Celula / 2,
                x + Celula / 2, y + Celula / 2);

            canvas.DrawRect(area, tinta);

            tinta.Color = Color.ParseColor("#B99D72");
            tinta.StrokeWidth = 2;
            tinta.SetStyle(Paint.Style.Stroke);
            canvas.DrawRect(area, tinta);
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

        void DesenharMontanha(Canvas canvas, float x, float y, long tempo)
        {
            // Trilhos do circuito
            tinta.Color = Color.ParseColor("#444444");
            tinta.StrokeWidth = 7;
            tinta.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(x - 32, y + 15, x - 20, y - 25, tinta);
            canvas.DrawLine(x - 20, y - 25, x, y + 15, tinta);
            canvas.DrawLine(x, y + 15, x + 20, y - 25, tinta);
            canvas.DrawLine(x + 20, y - 25, x + 32, y + 15, tinta);

            tinta.SetStyle(Paint.Style.Fill);

            // Carrinho percorre o circuito em loop
            float progresso = (tempo % 5000) / 5000f;
            float angulo = progresso * (float)(Math.PI * 2);

            float cx = x + (float)Math.Cos(angulo) * 27f;
            float cy = y + (float)Math.Sin(angulo) * 20f;

            tinta.Color = Color.ParseColor("#D92727");
            canvas.DrawRoundRect(
                new RectF(cx - 10, cy - 7, cx + 10, cy + 5),
                4, 4, tinta);

            // Passageiros dentro do carrinho
            tinta.Color = Color.ParseColor("#F5D0A0");
            canvas.DrawCircle(cx - 5, cy - 8, 3, tinta);
            canvas.DrawCircle(cx + 4, cy - 8, 3, tinta);

            tinta.Color = Color.ParseColor("#3157B7");
            canvas.DrawCircle(cx - 5, cy - 3, 2, tinta);
            canvas.DrawCircle(cx + 4, cy - 3, 2, tinta);
        }

        void DesenharVisitantes(Canvas canvas)
        {
            foreach (var pessoa in pessoas)
            {
                // Corpo
                tinta.Color = Color.ParseColor("#3157B7");
                canvas.DrawCircle(pessoa.X, pessoa.Y, 7, tinta);

                // Cabeça
                tinta.Color = Color.ParseColor("#F0C49A");
                canvas.DrawCircle(pessoa.X, pessoa.Y - 10, 5, tinta);

                // Pernas
                tinta.Color = Color.ParseColor("#333333");
                tinta.StrokeWidth = 2;
                canvas.DrawLine(
                    pessoa.X, pessoa.Y + 5,
                    pessoa.X - 4, pessoa.Y + 13, tinta);

                canvas.DrawLine(
                    pessoa.X, pessoa.Y + 5,
                    pessoa.X + 4, pessoa.Y + 13, tinta);
            }
        }
    }
}
