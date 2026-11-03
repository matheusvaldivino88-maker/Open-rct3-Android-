
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace RCT3AndroidTest;

[Activity(Label = "Modo Sandbox")]
public class SandboxActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var tela = new LinearLayout(this);
        tela.Orientation = Orientation.Vertical;
        tela.SetPadding(20, 20, 20, 20);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));

        var titulo = new TextView(this);
        titulo.Text = "MEU PARQUE";
        titulo.TextSize = 25;
        titulo.Gravity = GravityFlags.Center;
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));

        var parque = new ParkView(this);

        var voltar = new Button(this);
        voltar.Text = "Voltar ao menu";
        voltar.Click += (s, e) => Finish();

        tela.AddView(titulo);
        tela.AddView(parque, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1));
        tela.AddView(voltar);

        SetContentView(tela);
    }

    public class ParkView : View
    {
        public ParkView(Android.Content.Context context) : base(context)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            float w = Width;
            float h = Height;

            canvas.DrawColor(Color.ParseColor("#75B957"));

            var caminho = new Paint
            {
                Color = Color.ParseColor("#D8BE91"),
                StrokeWidth = 38,
                AntiAlias = true
            };

            canvas.DrawLine(w * 0.1f, h * 0.75f,
                w * 0.9f, h * 0.75f, caminho);

            canvas.DrawLine(w * 0.5f, h * 0.75f,
                w * 0.5f, h * 0.25f, caminho);

            var lago = new Paint
            {
                Color = Color.ParseColor("#45B9DC"),
                AntiAlias = true
            };

            canvas.DrawCircle(w * 0.78f, h * 0.35f,
                Math.Min(w, h) * 0.12f, lago);

            var trilho = new Paint
            {
                Color = Color.ParseColor("#444444"),
                StrokeWidth = 9,
                AntiAlias = true
            };

            trilho.SetStyle(Paint.Style.Stroke);

            canvas.DrawLine(w * 0.08f, h * 0.42f,
                w * 0.25f, h * 0.18f, trilho);

            canvas.DrawLine(w * 0.25f, h * 0.18f,
                w * 0.42f, h * 0.42f, trilho);

            canvas.DrawLine(w * 0.42f, h * 0.42f,
                w * 0.58f, h * 0.22f, trilho);

            canvas.DrawLine(w * 0.58f, h * 0.22f,
                w * 0.68f, h * 0.42f, trilho);

            DesenharArvore(canvas, w * 0.15f, h * 0.88f);
            DesenharArvore(canvas, w * 0.88f, h * 0.58f);
            DesenharArvore(canvas, w * 0.12f, h * 0.12f);
            DesenharArvore(canvas, w * 0.68f, h * 0.88f);
        }

        private void DesenharArvore(Canvas canvas, float x, float y)
        {
            var tronco = new Paint
            {
                Color = Color.ParseColor("#754323"),
                AntiAlias = true
            };

            var folhas = new Paint
            {
                Color = Color.ParseColor("#176B35"),
                AntiAlias = true
            };

            canvas.DrawRect(x - 5, y, x + 5, y + 25, tronco);
            canvas.DrawCircle(x, y, 19, folhas);
        }
    }
}
