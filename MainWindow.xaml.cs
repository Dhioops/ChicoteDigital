using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChicoteDigital
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = null!;
        private List<Node> nodes = new List<Node>();
        
        private List<Line> sombraCorda = new List<Line>();
        private List<Line> interiorCorda = new List<Line>();
        private Path pontaChicote = null!;
        private Canvas caboVisual = null!;
        private RotateTransform rotacaoCabo = new RotateTransform(0);
        
        private int numNodes = 22;
        private double segmentLength = 14.0;
        private bool isHoldingWhip = false;

        // Novas variáveis para controlar o centro do cabo e o giro no scroll
        private Point posicaoMao;
        private double anguloManualCabo = 0.0; 
        private double distanciaCentroAteBase = 25.0; // Distância do meio do cabo até o anel onde a corda prende

        public MainWindow()
        {
            InitializeComponent();
            
            // Ativa o evento do Scroll do mouse via código
            this.MouseWheel += Window_MouseWheel;
            this.KeyDown += Window_KeyDown;

            this.Loaded += (s, e) => 
            {
                InitializeWhip();

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(16);
                timer.Tick += PhysicsLoop;
                timer.Start();
            };
        }

        private void InitializeWhip()
        {
            // O cabo começa no centro da tela
            posicaoMao = new Point(this.ActualWidth / 2, this.ActualHeight / 3);
            Point baseInicialCorda = CalcularPosicaoBaseCabo();

            // 1. Cria os nós da física partindo da base do cabo
            for (int i = 0; i < numNodes; i++)
            {
                nodes.Add(new Node 
                { 
                    Current = new Point(baseInicialCorda.X, baseInicialCorda.Y + i * segmentLength), 
                    Old = new Point(baseInicialCorda.X, baseInicialCorda.Y + i * segmentLength) 
                });
            }

            // 2. Cria os vetores da corda
            for (int i = 0; i < numNodes - 1; i++)
            {
                double espessura = Math.Max(2.5, 14.0 * (1.0 - (double)i / numNodes));

                Line borda = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(45, 20, 5)),
                    StrokeThickness = espessura + 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                sombraCorda.Add(borda);
                GameCanvas.Children.Add(borda);

                Color corCouro = (i % 2 == 0) ? Color.FromRgb(139, 69, 19) : Color.FromRgb(160, 82, 45);
                Line miolo = new Line
                {
                    Stroke = new SolidColorBrush(corCouro),
                    StrokeThickness = espessura,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                interiorCorda.Add(miolo);
                GameCanvas.Children.Add(miolo);
            }

            // 3. Fitinha da ponta (Cracker)
            pontaChicote = new Path
            {
                Stroke = Brushes.Crimson,
                StrokeThickness = 2,
                StrokeEndLineCap = PenLineCap.Triangle
            };
            GameCanvas.Children.Add(pontaChicote);

            // 4. Desenha o CABO com eixo de rotação exatamente no CENTRO (0.5, 0.5)
            caboVisual = new Canvas
            {
                Width = 24,
                Height = 70,
                Cursor = Cursors.Hand,
                RenderTransform = rotacaoCabo,
                RenderTransformOrigin = new Point(0.5, 0.5) // Giro e clique centralizados!
            };

            Rectangle corpoCabo = new Rectangle
            {
                Width = 16, Height = 55,
                RadiusX = 5, RadiusY = 5,
                Fill = new SolidColorBrush(Color.FromRgb(60, 30, 10)),
                Stroke = new SolidColorBrush(Color.FromRgb(30, 15, 5)),
                StrokeThickness = 2
            };
            Canvas.SetLeft(corpoCabo, 4);
            Canvas.SetTop(corpoCabo, 10);

            Ellipse pomoTopo = new Ellipse
            {
                Width = 22, Height = 16,
                Fill = Brushes.Goldenrod,
                Stroke = Brushes.DarkGoldenrod,
                StrokeThickness = 2
            };
            Canvas.SetLeft(pomoTopo, 1);
            Canvas.SetTop(pomoTopo, 0);

            Rectangle anelBase = new Rectangle
            {
                Width = 18, Height = 8,
                RadiusX = 2, RadiusY = 2,
                Fill = Brushes.Goldenrod,
                Stroke = Brushes.DarkGoldenrod,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(anelBase, 3);
            Canvas.SetTop(anelBase, 56);

            caboVisual.Children.Add(corpoCabo);
            caboVisual.Children.Add(anelBase);
            caboVisual.Children.Add(pomoTopo);

            GameCanvas.Children.Add(caboVisual);
        }

        // Calcula onde está a ponta inferior do cabo com base no giro do Scroll
        private Point CalcularPosicaoBaseCabo()
        {
            double radianos = (anguloManualCabo + 90) * Math.PI / 180.0;
            double baseX = posicaoMao.X + Math.Cos(radianos) * distanciaCentroAteBase;
            double baseY = posicaoMao.Y + Math.Sin(radianos) * distanciaCentroAteBase;
            return new Point(baseX, baseY);
        }

        private void PhysicsLoop(object? sender, EventArgs e)
        {
            // O nó 0 (início da corda) fica sempre preso na ponta do cabo que gira!
            nodes[0].Old = nodes[0].Current;
            nodes[0].Current = CalcularPosicaoBaseCabo();

            // 1. Gravidade e Inércia na corda
            for (int i = 1; i < numNodes; i++)
            {
                Point temp = nodes[i].Current;
                double vx = (nodes[i].Current.X - nodes[i].Old.X) * 0.95;
                double vy = (nodes[i].Current.Y - nodes[i].Old.Y) * 0.95;

                double nextX = nodes[i].Current.X + vx;
                double nextY = nodes[i].Current.Y + vy + 1.1;

                if (nextY > this.ActualHeight - 10) nextY = this.ActualHeight - 10;

                nodes[i].Current = new Point(nextX, nextY);
                nodes[i].Old = temp;
            }

            // 2. Restrições de distância
            for (int iteration = 0; iteration < 15; iteration++)
            {
                for (int i = 0; i < numNodes - 1; i++)
                {
                    double dx = nodes[i + 1].Current.X - nodes[i].Current.X;
                    double dy = nodes[i + 1].Current.Y - nodes[i].Current.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    
                    if (distance == 0) continue;

                    double difference = segmentLength - distance;
                    double percent = difference / distance / 2.0;

                    double offsetX = dx * percent;
                    double offsetY = dy * percent;

                    if (i == 0)
                    {
                        nodes[i + 1].Current = new Point(nodes[i + 1].Current.X + offsetX * 2, nodes[i + 1].Current.Y + offsetY * 2);
                    }
                    else
                    {
                        nodes[i].Current = new Point(nodes[i].Current.X - offsetX, nodes[i].Current.Y - offsetY);
                        nodes[i + 1].Current = new Point(nodes[i + 1].Current.X + offsetX, nodes[i + 1].Current.Y + offsetY);
                    }
                }
            }

            // 3. Atualiza o desenho vetorial da corda
            for (int i = 0; i < numNodes - 1; i++)
            {
                Point p1 = nodes[i].Current;
                Point p2 = nodes[i + 1].Current;

                sombraCorda[i].X1 = p1.X;
                sombraCorda[i].Y1 = p1.Y;
                sombraCorda[i].X2 = p2.X;
                sombraCorda[i].Y2 = p2.Y;

                interiorCorda[i].X1 = p1.X;
                interiorCorda[i].Y1 = p1.Y;
                interiorCorda[i].X2 = p2.X;
                interiorCorda[i].Y2 = p2.Y;
            }

            // 4. Desenha a pontinha vermelha final
            Point penultimo = nodes[numNodes - 2].Current;
            Point ultimo = nodes[numNodes - 1].Current;
            double dirX = ultimo.X - penultimo.X;
            double dirY = ultimo.Y - penultimo.Y;
            pontaChicote.Data = new LineGeometry(ultimo, new Point(ultimo.X + dirX * 0.8, ultimo.Y + dirY * 0.8));

            // 5. Posiciona o CENTRO do cabo exatamente onde o mouse está e aplica a rotação do Scroll
            Canvas.SetLeft(caboVisual, posicaoMao.X - (caboVisual.Width / 2));
            Canvas.SetTop(caboVisual, posicaoMao.Y - (caboVisual.Height / 2));
            rotacaoCabo.Angle = anguloManualCabo;
        }

        // --- Controles do Mouse e Scroll ---

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Gira 18 graus a cada "tracinho" do scroll do mouse (funciona segurando ou solto)
            if (e.Delta > 0)
                anguloManualCabo -= 18;
            else
                anguloManualCabo += 18;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Activate();
            isHoldingWhip = true;
            Mouse.Capture(this);
            posicaoMao = e.GetPosition(GameCanvas);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
            Mouse.Capture(null); // Solta o mouse por segurança antes de fechar
            Application.Current.Shutdown(); // Encerra o aplicativo imediatamente
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isHoldingWhip)
            {
                posicaoMao = e.GetPosition(GameCanvas);
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isHoldingWhip)
            {
                isHoldingWhip = false;
                Mouse.Capture(null);
            }
        }
    }

    public class Node
    {
        public Point Current { get; set; }
        public Point Old { get; set; }
    }
}