
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.Collections.Generic;

namespace RCT3AndroidTest;

[Activity(Label = "Modo Sandbox")]
public class SandboxActivity : Activity
{
    int dinheiro = 1000;
    string ferramenta = "Caminho";
    TextView saldo;
    ParkView parque;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var tela = new LinearLayout(this);
        tela.Orientation = Orientation.Vertical;
        tela.SetPadding(12, 12, 12, 12);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));

        var titulo = new TextView(this);
        titulo.Text = "MEU PARQUE";
        titulo.TextSize = 25;
        titulo.Gravity = GravityFlags.Center;
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));

        saldo = new TextView(this);
        saldo.TextSize = 18;
        saldo.Gravity = GravityFlags.Center;
        saldo.SetTextColor(Color.White);
        AtualizarSaldo();

        parque = new ParkView(this, AdicionarObjeto);

        var ferramentas = new LinearLayout(this);
        ferramentas.Orientation = Orientation.Horizontal;

        AdicionarBotao(ferramentas, "Caminho", 20);
        AdicionarBotao(ferramentas, "Árvore", 30);
        AdicionarBotao(ferramentas, "Brinquedo", 150);
        AdicionarBotao(ferramentas, "Montanha-russa", 300);
        AdicionarBotao(ferramentas, "Remover", 0);

        var voltar = new Button(this);
        voltar.Text = "Voltar ao menu";
        voltar.Click += (s, e) => Finish();

        tela.AddView(titulo);
        tela.AddView(saldo);
        tela.AddView(ferramentas);
        tela.AddView(parque, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1));
        tela.AddView(voltar);

        SetContentView(tela);
    }

    void AdicionarBotao(LinearLayout layout, string nome, int preco)
    {
        var botao = new Button(this);
        botao.Text = nome;
        botao.TextSize = 10;
        botao.SetPadding(2, 2, 2, 2);
        layout.AddView(botao, new LinearLayout.LayoutParams(
            0, ViewGroup.LayoutParams.WrapContent, 1));

        botao.Click += (s, e) =>
        {
            ferramenta = nome;
            parque.Invalidate();
        };
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
            Toast.MakeText(this, "Dinheiro insuficiente!", ToastLength.Short).Show();
            return;
        }

        dinheiro -= preco;
        parque.Adicionar(ferramenta, x, y);
        AtualizarSaldo();
    }

    void AtualizarSaldo()
    {
        saldo.Text = $"Dinheiro: $ {dinheiro}";
    }

    public class ParkView : View
    {
        readonly List<Objeto> objetos = new();
        readonly System.Action<float, float> aoTocar;
        readonly Paint tinta = new() { AntiAlias = true };

        class Objeto
        {
            public string Tipo = "";
            public float X;
            public float Y;
        }

        public ParkView(Android.Content.Context context,
            System.Action<float, float> callback) : base(context)
        {
            aoTocar = callback;
        }

        public void Adicionar(string tipo, float x, float y)
        {
            objetos.Add(new Objeto { Tipo = tipo, X = x, Y = y });
            Invalidate();
        }

        public void RemoverMaisProximo(float x, float y)
        {
            if (objetos.Count == 0) return;

            int indice = -1;
            double menor = 65 * 65;

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
                objetos.RemoveAt(indice);

            Invalidate();
        }

        public override bool OnTouchEvent(Android.Views.MotionEvent? e)
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

            canvas.DrawColor(Color.ParseColor("#75B957"));

            tinta.Color = Color.ParseColor("#D8BE91");
            tinta.StrokeWidth = 38;
            tinta.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(0, Height * 0.75f,
                Width, Height * 0.75f, tinta);

            tinta.SetStyle(Paint.Style.Fill);

            foreach (var obj in objetos)
            {
                switch (obj.Tipo)
                {
                    case "Caminho":
                        tinta.Color = Color.ParseColor("#D8BE91");
                        canvas.DrawCircle(obj.X, obj.Y, 20, tinta);
                        break;

                    case "Árvore":
                        tinta.Color = Color.ParseColor("#754323");
                        canvas.DrawRect(obj.X - 5, obj.Y,
                            obj.X + 5, obj.Y + 25, tinta);

                        tinta.Color = Color.ParseColor("#176B35");
                        canvas.DrawCircle(obj.X, obj.Y, 20, tinta);
                        break;

                    case "Brinquedo":
                        tinta.Color = Color.ParseColor("#E8B52E");
                        canvas.DrawCircle(obj.X, obj.Y, 25, tinta);

                        tinta.Color = Color.White;
                        canvas.DrawCircle(obj.X, obj.Y, 10, tinta);
                        break;

                    case "Montanha-russa":
                        tinta.Color = Color.ParseColor("#444444");
                        tinta.StrokeWidth = 8;
                        tinta.SetStyle(Paint.Style.Stroke);

                        canvas.DrawLine(obj.X - 35, obj.Y + 20,
                            obj.X - 10, obj.Y - 30, tinta);

                        canvas.DrawLine(obj.X - 10, obj.Y - 30,
                            obj.X + 15, obj.Y + 20, tinta);

                        canvas.DrawLine(obj.X + 15, obj.Y + 20,
                            obj.X + 35, obj.Y - 20, tinta);

                        tinta.SetStyle(Paint.Style.Fill);
                        break;
                }
            }
        }
    }
}
