
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content;
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

        AtualizarPainel();

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

        parque.SetParkOpen(parqueAberto);
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
        readonly List<ParkItem> objetos = new();
        readonly System.Action<float, float> aoTocar;

        readonly Paint tinta = new()
        {
            AntiAlias = true
        };

        const float Celula = 48f;

        bool aberto = false;
        string ferramenta = "Caminho";

        class ParkItem
        {
            public string Tipo = "";
            public float X;
            public float Y;
        }

        public ParkView(Context context,
            System.Action<float, float> callback) : base(context)
        {
            aoTocar = callback;
        }

        public void SetTool(string nome)
        {
            ferramenta = nome;
            Invalidate();
        }

        public void SetParkOpen(bool valor)
        {
            aberto = valor;
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
                        DesenharBrinquedo(canvas, obj.X, obj.Y);
                        break;

                    case "Montanha-russa":
                        DesenharMontanha(canvas, obj.X, obj.Y);
                        break;
                }
            }

            if (aberto)
                DesenharVisitantes(canvas, w, h);

            // Atualiza a animação visual dos brinquedos e visitantes.
            PostInvalidateDelayed(100);
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
                x - Celula / 2,
                y - Celula / 2,
                x + Celula / 2,
                y + Celula / 2);

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

        void DesenharBrinquedo(Canvas canvas, float x, float y)
        {
            long tempo = Android.OS.SystemClock.UptimeMillis();
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

        void DesenharMontanha(Canvas canvas, float x, float y)
        {
            tinta.Color = Color.ParseColor("#444444");
            tinta.StrokeWidth = 8;
            tinta.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(x - 35, y + 20,
                x - 15, y - 30, tinta);

            canvas.DrawLine(x - 15, y - 30,
                x + 5, y + 20, tinta);

            canvas.DrawLine(x + 5, y + 20,
                x + 25, y - 20, tinta);

            tinta.SetStyle(Paint.Style.Fill);

            tinta.Color = Color.ParseColor("#A52A2A");
            canvas.DrawCircle(x - 15, y - 30, 5, tinta);
        }

        void DesenharVisitantes(Canvas canvas, float w, float h)
        {
            long tempo = Android.OS.SystemClock.UptimeMillis();

            for (int i = 0; i < 5; i++)
            {
                float x = (tempo / 15f + i * 85) % Math.Max(w, 1);
                float y = h * 0.85f + (i % 2) * 12;

                tinta.Color = Color.ParseColor("#3157B7");
                canvas.DrawCircle(x, y, 7, tinta);

                tinta.Color = Color.ParseColor("#F0C49A");
                canvas.DrawCircle(x, y - 10, 6, tinta);

                tinta.Color = Color.ParseColor("#333333");
                canvas.DrawLine(x, y + 5, x - 4, y + 14, tinta);
                canvas.DrawLine(x, y + 5, x + 4, y + 14, tinta);
            }
        }
    }
}
