using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Graphics;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace CubeKing.Core
{
    public abstract class BaseScene : INotifyPropertyChanged, IDisposable
    {
        #region DrawingSurface

        public readonly DrawingSurface _drawingSurface;
        protected readonly ContentManager contentManager;

        protected Vector2 viewportSize;
        protected float aspectRatio;

        void _drawingSurface_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            viewportSize = new Vector2((float)_drawingSurface.ActualWidth, (float)_drawingSurface.ActualHeight);
            aspectRatio = (float)(_drawingSurface.ActualWidth / _drawingSurface.ActualHeight);
        }

        #endregion

        #region Properties

        public ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return GraphicsDeviceManager.Current.GraphicsDevice;
            }
        }

        #endregion

        #region Initialization

        public BaseScene(DrawingSurface drawingSurface)
        {
            _drawingSurface = drawingSurface;

            // Register for size changed to update the aspect ratio
            _drawingSurface.SizeChanged += _drawingSurface_SizeChanged;

            // Get a content manager to access content pipeline
            contentManager = new ContentManager(null)
            {
                RootDirectory = "Content"
            };

            this.Initialize();
        }

        protected virtual void Initialize()
        {


        }

        #endregion

        #region Methods

        public virtual void Draw()
        {

        }

        public void Dispose()
        {
            _drawingSurface.SizeChanged -= _drawingSurface_SizeChanged;
        }

        #endregion

        public void RaisePropertyChanged(string propertyName)
        {
            this._drawingSurface.Dispatcher.BeginInvoke(() =>
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
