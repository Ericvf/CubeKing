using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Graphics;
using AnimationExtensions;
using Microsoft.Xna.Framework.Input;

namespace CubeKing
{
    public partial class MainPage
    {
        CubeScene scene;

        public MainPage()
        {
            InitializeComponent();

            HtmlPage.RegisterScriptableObject("Page", this);

            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
            {
                this.settings.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.warning.Visibility = System.Windows.Visibility.Collapsed;
                Mouse.RootControl = this;

                this.scene = new CubeScene(drawingSurface);
                this.LayoutRoot.DataContext = this.scene.ViewModel;
                this.drawingSurface.Draw += this.drawingSurface_Draw;

                if (!this.scene.ViewModel.ShowSettings)
                {
                    this.settings.Opacity = 0;
                }

                this.legend.Opacity = 0;
                this.keyboard.Opacity = 0;
            }

            this.KeyUp += new System.Windows.Input.KeyEventHandler(MainPage_KeyUp);
            this.drawingSurface.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(drawingSurface_MouseLeftButtonUp);

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            TogglePanels(this.leftItems, this.cubeSettings);
            TogglePanels(this.rightItems, this.newCube);
        }

        void drawingSurface_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Focus();
        }

        void MainPage_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (this.cbSettings.IsHitTestVisible)
                    this.cbSettings.IsChecked = !this.cbSettings.IsChecked;
            }
            else
            {
                this.scene.HandleInput(e.Key);
            }
        }

        private void drawingSurface_Draw(object sender, DrawEventArgs e)
        {
            this.scene.Draw();
            e.InvalidateSurface();
        }

        private void ButtonNewCube_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.scene.Reset(
                (float)this.upWidth.Value,
                (float)this.upHeight.Value,
                (float)this.upDepth.Value);
        }

        private void ButtonScramble_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.scene.Scramble();

        }

        private void ButtonUndo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.scene.Undo();
        }

        private void ScrambleText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.scene.Scramble((sender as TextBox).Text);
            }

            e.Handled = true;
        }

        private void ButtonTimer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.scene.StartPlaying();
        }

        internal void Save()
        {
            if (scene != null)
                this.scene.ViewModel.Save();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.settings == null)
                return;

            var cb = sender as CheckBox;
            this.ShowSettings(cb);
        }

        private void ShowSettings(CheckBox cb)
        {
            // Helpers to find all child controls
            var leftitems = this.leftItems.FindChilden<Border>();
            var rightitems = this.rightItems.FindChilden<Border>();

            // Create setup action (preconditions)
            Action setup = () =>
            {
                this.settings.Opacity = 1;
                cb.IsHitTestVisible = false;

                // Hide all panels
                leftitems.Hide();
                rightitems.Hide();

            };

            // Distances travelled
            var lw = 250;
            var rw = 250;

            // Seperate animations for LEFT and RIGHT
            var leftAx = leftitems.For((i, b) => b
                    .Wait(i * 150)
                    .Move(x: -lw)
                    .Fade(1, 500)
                    .Move(duration: 500, eq: Eq.OutBack)
            );

            var rightAx = rightitems.For((i, b) => b
                    .Wait(i * 150)
                    .Move(x: rw)
                    .Fade(1, 500)
                    .Move(duration: 500, eq: Eq.OutBack)
                );

            var toolbarAnimation = Ax.New()

                .Do((e) => setup()) // exec setup method
                .And(leftAx) // Do left animation
                .And(rightAx) // Do right animation 
                .And(this.history // Show history
                        .Move(0, -10)
                        .Then()
                        .Fade(1, 500, Eq.OutSine)
                        .Move(duration: 500, eq: Eq.OutSine))
                .And(this.legend.Fade(1, 500, Eq.OutSine)) // Show legend
                .And(this.keyboard.Fade(1, 500, Eq.OutSine)) // Show keyboard

                // Finally update ishittestvisible
                .ThenDo((f) =>
                    {
                        cb.IsHitTestVisible = true;
                        this.settings.IsHitTestVisible = true;
                    });


            toolbarAnimation.Play();
        }

        private void HideSettings(CheckBox cb)
        {
            var leftitems = this.leftItems.FindChilden<Border>();
            var rightitems = this.rightItems.FindChilden<Border>();

            // Create setup action (preconditions)
            Action setup = () =>
            {
                cb.IsHitTestVisible = false;
                //leftitems.Hide();
                //rightitems.Hide();
            };

            var lw = 250;
            var rw = 250;

            var leftAx = Ax.For(leftitems, (i, b) => b
                 .Wait(i * 150)
                 .Fade(0, 500)
                 .Move(x: -lw, duration: 500, eq: Eq.InBack)
             );

            var rightAx = Ax.For(rightitems, (i, b) => b
                 .Wait(i * 150)
                 .Fade(0, 500)
                 .Move(x: rw, duration: 500, eq: Eq.InBack)
             );

            Ax.New()
                .Do((e) => setup()) // exec setup method
                .And(leftAx)
                .And(rightAx)
                .And(this.history
                    .Fade(0, 500, Eq.OutSine)
                    .Move(0, 10, 500, Eq.OutSine))
                .And(this.legend.Fade(0, 500, Eq.OutSine))
                .And(this.keyboard.Fade(0, 500, Eq.OutSine))
                .ThenDo((f) =>
                    {
                        cb.IsHitTestVisible = true;
                        this.settings.IsHitTestVisible = false;
                        //this.settings.Visibility = System.Windows.Visibility.Collapsed;
                    })
                .Play();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.settings == null)
                return;

            var cb = sender as CheckBox;
            this.HideSettings(cb);
        }

        private void AppByFex_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("mailto:e.feggelen@gmail.com"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.scene.Solve();
        }

        private void btnResetColors_Click(object sender, RoutedEventArgs e)
        {
            this.scene.ViewModel.ResetColors();
        }

        private void btnBrowseTexture_Click(object sender, RoutedEventArgs e)
        {
            this.scene.Browse();
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var image = sender as Image;
            var grid = image.Tag as Grid;

            TogglePanels(this.leftItems, grid);
        }

        private void TogglePanels(StackPanel panel, Grid grid)
        {
            var grids = panel.FindChilden<Grid>();
            var ax = Ax.New();

            foreach (var g in grids)
            {
                if (grid == g)
                {
                    ax.And(g.Size(-1, grid.MaxHeight, 500, Eq.OutSine));
                }
                else
                {
                    ax.And(g.Size(-1, 0, 500, Eq.OutSine));
                }
            }

            ax.Play();
        }


        private void Image_MouseLeftButtonUp_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var image = sender as Image;
            var grid = image.Tag as Grid;

            TogglePanels(this.rightItems, grid);
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            var state = this.scene.GetConfiguration();
            var antiScramble = this.scene.GetAntiScramble();
            HtmlPage.Window.Invoke("GetSolve", state, this.scene.width, this.scene.height, this.scene.depth, antiScramble);
        }

        [ScriptableMember]
        public void Solve(string input)
        {
            if (this.scene != null)
                this.scene.Command(input, false);
        }
    }
}
