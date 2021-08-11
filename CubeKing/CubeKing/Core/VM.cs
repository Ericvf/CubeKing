using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace CubeKing.Core
{
    public class VM : INotifyPropertyChanged
    {
        public Collection<string> commands = new Collection<string>();

        readonly public List<Vector4> ChannelColors = new List<Vector4>()
        {
            new Vector4(1, 1, 1, 1), // white
            new Vector4(1, 1, 0, 1), // yellow
            new Vector4(0, 1, 0, 1), // green
            new Vector4(0, 0, 1, 1), // blue
            new Vector4(1, 0, 0, 1), // red
            new Vector4(1, 0.5f, 0, 1), // orange
        };

        private static List<EasingEquation> eqs = new List<EasingEquation>()
            {
                Eqs.Linear,
                Eqs.OutSine,
                Eqs.OutBack,
                Eqs.OutCubic,
                Eqs.OutElastic
            };

        private CubeScene cubeScene;

        public VM(CubeScene cubeScene)
        {
            this.cubeScene = cubeScene;
            this.width = 3;
            this.height = 3;
            this.depth = 3;
            this.backReflection = 3;
            this.downReflection = 3;
            this.leftReflection = 3;

            this.cameraZoom = 1.5f;
            this.cubieSize = 0.95f;
            this.bigStickers = true;
            this.useStickers = true;
            this.useCenters = true;
            this.useCorners = true;
            this.useEdges = true;
            this.hideCubies = true;

            this.turnAnimationIndex = 2;
            this.twistAnimationIndex = 2;
            this.scrambleAnimationIndex = 2;

            this.turnAnimationSpeed = 300;
            this.twistAnimationSpeed = 300;
            this.scrambleAnimationSpeed = 200;

            this.ShowSettings = true;
            this.useDoubleTurns = true;
            this.useInvertedTurns = true;
            this.scrambleSize = 25;
        }

        public VM()
        {

        }

        private System.Windows.Media.Color color0;
        public System.Windows.Media.Color Color0
        {
            get
            {
                return color0;
            }
            set
            {
                this.ChannelColors[0] = new Vector4((float)color0.R / 255, (float)color0.G / 255, (float)color0.B / 255, 1);
                this.color0 = value;
                this.RaisePropertyChanged("Color0");
            }
        }

        private System.Windows.Media.Color color1;
        public System.Windows.Media.Color Color1
        {
            get
            {
                return color1;
            }
            set
            {
                this.ChannelColors[1] = new Vector4((float)color1.R / 255, (float)color1.G / 255, (float)color1.B / 255, 1);
                this.color1 = value;
                this.RaisePropertyChanged("Color1");
            }
        }

        private System.Windows.Media.Color color2;
        public System.Windows.Media.Color Color2
        {
            get
            {
                return color2;
            }
            set
            {
                this.ChannelColors[2] = new Vector4((float)color2.R / 255, (float)color2.G / 255, (float)color2.B / 255, 1);
                this.color2 = value;
                this.RaisePropertyChanged("Color2");
            }
        }

        private System.Windows.Media.Color color3;
        public System.Windows.Media.Color Color3
        {
            get
            {
                return color3;
            }
            set
            {
                this.ChannelColors[3] = new Vector4((float)color3.R / 255, (float)color3.G / 255, (float)color3.B / 255, 1);
                this.color3 = value;
                this.RaisePropertyChanged("Color3");
            }
        }

        private System.Windows.Media.Color color4;
        public System.Windows.Media.Color Color4
        {
            get
            {
                return color4;
            }
            set
            {
                this.ChannelColors[4] = new Vector4((float)color4.R / 255, (float)color4.G / 255, (float)color4.B / 255, 1);
                this.color4 = value;
                this.RaisePropertyChanged("Color4");
            }
        }

        private System.Windows.Media.Color color5;
        public System.Windows.Media.Color Color5
        {
            get
            {
                return color5;
            }
            set
            {
                this.ChannelColors[5] = new Vector4((float)color5.R / 255, (float)color5.G / 255, (float)color5.B / 255, 1);
                this.RaisePropertyChanged("Color5");
                this.color5 = value;
            }
        }

        private int moves;
        [XmlIgnore]
        public int Moves
        {
            get
            {
                return this.moves;
            }
            set
            {
                this.moves = value;
                this.RaisePropertyChanged("Moves");
            }
        }

        private string time;
        [XmlIgnore]
        public string Time
        {
            get
            {
                return this.time;
            }
            set
            {
                this.time = value;
                this.RaisePropertyChanged("Time");
            }
        }

        private int width;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
                this.RaisePropertyChanged("Width");
            }
        }

        private int height;
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
                this.RaisePropertyChanged("Height");
            }
        }

        private int depth;
        public int Depth
        {
            get
            {
                return this.depth;
            }
            set
            {
                this.depth = value;
                this.RaisePropertyChanged("Depth");
            }
        }

        private bool useCorners;
        public bool UseCorners
        {
            get
            {
                return this.useCorners;
            }
            set
            {
                this.useCorners = value;
                this.RaisePropertyChanged("UseCorners");
            }
        }

        private bool useEdges;
        public bool UseEdges
        {
            get
            {
                return this.useEdges;
            }
            set
            {
                this.useEdges = value;
                this.RaisePropertyChanged("UseEdges");
            }
        }

        private bool useCenters;
        public bool UseCenters
        {
            get
            {
                return this.useCenters;
            }
            set
            {
                this.useCenters = value;
                this.RaisePropertyChanged("UseCenters");
            }
        }

        private bool hideCubies;
        public bool HideCubies
        {
            get
            {
                return this.hideCubies;
            }
            set
            {
                this.hideCubies = value;
                this.RaisePropertyChanged("HideCubies");
            }
        }

        private bool useReflections = true;
        public bool UseReflections
        {
            get
            {
                return this.useReflections;
            }
            set
            {
                if (!value && this.cubeScene != null)
                    this.cubeScene.reflectionAlpha = 0;

                this.useReflections = value;
                this.RaisePropertyChanged("UseReflections");
            }
        }

        private bool useStickers = false;
        public bool UseStickers
        {
            get
            {
                return this.useStickers;
            }
            set
            {
                this.useStickers = value;
                this.RaisePropertyChanged("UseStickers");
            }
        }

        private bool bigStickers;
        public bool BigStickers
        {
            get
            {
                return this.bigStickers;
            }
            set
            {
                if (this.bigStickers != value)
                {
                    this.bigStickers = value;

                    if (cubeScene != null)
                        this.RaisePropertyChanged("BigStickers");
                }
            }
        }

        private int backReflection;
        public int BackReflection
        {
            get
            {
                return this.backReflection;
            }
            set
            {
                this.backReflection = value;
                this.RaisePropertyChanged("BackReflection");
            }
        }

        private int leftReflection;
        public int LeftReflection
        {
            get
            {
                return this.leftReflection;
            }
            set
            {
                this.leftReflection = value;
                this.RaisePropertyChanged("LeftReflection");
            }
        }

        private int downReflection;
        public int DownReflection
        {
            get
            {
                return this.downReflection;
            }
            set
            {
                this.downReflection = value;
                this.RaisePropertyChanged("DownReflection");
            }
        }

        private bool useLights = true;
        public bool UseLights
        {
            get
            {
                return this.useLights;
            }
            set
            {
                this.useLights = value;
                this.RaisePropertyChanged("UseLights");
            }
        }

        private bool isTexture;
        public bool IsTexture
        {
            get
            {
                return this.isTexture;
            }
            set
            {
                this.isTexture = value;
                this.RaisePropertyChanged("IsTexture");
            }
        }

        private double scramblePercentage;
        public double ScramblePercentage
        {
            get
            {
                return this.scramblePercentage;
            }
            set
            {
                this.scramblePercentage = value;
                this.RaisePropertyChanged("ScramblePercentage");
            }
        }

        private bool onlyShowPaintedFaces;
        public bool OnlyShowPaintedFaces
        {
            get
            {
                return this.onlyShowPaintedFaces;
            }
            set
            {
                this.onlyShowPaintedFaces = value;
                this.RaisePropertyChanged("OnylShowPaintedFaces");
            }
        }

        private bool onlyShowFrontFaces;
        public bool OnlyShowFrontFaces
        {
            get
            {
                return this.onlyShowFrontFaces;
            }
            set
            {
                this.onlyShowFrontFaces = value;
                this.RaisePropertyChanged("OnlyShowFrontFaces");
            }
        }

        private float depth3d = 0f;
        public float Depth3d
        {
            get
            {
                return this.depth3d;
            }
            set
            {
                this.depth3d = value;
                this.RaisePropertyChanged("Depth3d");
            }
        }

        private bool use3d;
        public bool Use3d
        {
            get
            {
                return this.use3d;
            }
            set
            {
                this.use3d = value;
                this.RaisePropertyChanged("Use3d");
            }
        }

        private bool swap3d;
        public bool Swap3d
        {
            get
            {
                return this.swap3d;
            }
            set
            {
                this.swap3d= value;
                this.RaisePropertyChanged("Swap3d");
            }
        }


        private float cameraZoom = 1.5f;
        public float CameraZoom
        {
            get
            {
                return this.cameraZoom;
            }
            set
            {
                this.cameraZoom = value;
                this.RaisePropertyChanged("CameraZoom");
            }
        }

        private float cubieSize = 0.95f;
        public float CubieSize
        {
            get
            {
                return this.cubieSize;
            }
            set
            {
                this.cubieSize = value;
                this.RaisePropertyChanged("CubieSize");
            }
        }

        private string scrambleText;
        public string ScrambleText
        {
            get
            {
                return this.scrambleText;
            }
            set
            {
                this.scrambleText = value;
                this.RaisePropertyChanged("ScrambleText");
            }
        }

        private bool useDoubleTurns;
        public bool UseDoubleTurns
        {
            get
            {
                return this.useDoubleTurns;
            }
            set
            {
                this.useDoubleTurns = value;
                this.RaisePropertyChanged("UseDoubleTurns");
            }
        }

        private bool useInvertedTurns;
        public bool UseInvertedTurns
        {
            get
            {
                return this.useInvertedTurns;
            }
            set
            {
                this.useInvertedTurns = value;
                this.RaisePropertyChanged("UseInvertedTurns");
            }
        }

        private int scrambleSize;
        public int ScrambleSize
        {
            get
            {
                return this.scrambleSize;
            }
            set
            {
                this.scrambleSize = value;
                this.RaisePropertyChanged("ScrambleSize");
            }
        }

        private bool showSettings;
        public bool ShowSettings
        {
            get
            {
                return this.showSettings;
            }
            set
            {
                this.showSettings = value;
                this.RaisePropertyChanged("ShowSettings");
            }
        }

        public string CommandHistory
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var command in this.commands.Reverse().Take(10).Reverse())
                {
                    sb.Append(command);
                    sb.Append(" ");
                }

                return sb.ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (this.cubeScene == null || this.cubeScene._drawingSurface == null)
                return;

            this.cubeScene._drawingSurface.Dispatcher.BeginInvoke(() =>
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        private int turnAnimationIndex;
        public int TurnAnimationIndex
        {
            get
            {
                return this.turnAnimationIndex;
            }
            set
            {
                this.turnAnimationIndex = value;
                this.RaisePropertyChanged("TurnAnimation");
            }
        }

        private int twistAnimationIndex;
        public int TwistAnimationIndex
        {
            get
            {
                return this.twistAnimationIndex;
            }
            set
            {
                this.twistAnimationIndex = value;
                this.RaisePropertyChanged("TwistAnimation");
            }
        }

        private int scrambleAnimationIndex;
        public int ScrambleAnimationIndex
        {
            get
            {
                return this.scrambleAnimationIndex;
            }
            set
            {
                this.scrambleAnimationIndex = value;
                this.RaisePropertyChanged("ScrambleAnimation");
            }
        }

        private int turnAnimationSpeed;
        public int TurnAnimationSpeed
        {
            get
            {
                return this.turnAnimationSpeed;
            }
            set
            {
                this.turnAnimationSpeed = value;
                this.RaisePropertyChanged("TurnAnimationSpeed");
            }
        }

        private int twistAnimationSpeed;
        public int TwistAnimationSpeed
        {
            get
            {
                return this.twistAnimationSpeed;
            }
            set
            {
                this.twistAnimationSpeed = value;
                this.RaisePropertyChanged("TwistAnimationSpeed");
            }
        }

        private int scrambleAnimationSpeed;
        public int ScrambleAnimationSpeed
        {
            get
            {
                return this.scrambleAnimationSpeed;
            }
            set
            {
                this.scrambleAnimationSpeed = value;
                this.RaisePropertyChanged("ScrambleAnimationSpeed");
            }
        }

        [XmlIgnore]
        public EasingEquation TurnAnimation
        {
            get
            {
                return eqs[this.turnAnimationIndex];
            }
        }


        [XmlIgnore]
        public EasingEquation TwistAnimation
        {
            get
            {
                return eqs[this.TwistAnimationIndex];
            }
        }


        [XmlIgnore]
        public EasingEquation ScrambleAnimation
        {
            get
            {
                return eqs[this.scrambleAnimationIndex];
            }
        }

        public static VM Load(CubeScene scene)
        {
            VM vm = XmlSerializationHelper.DeserializeFile<VM>("ViewModel.xml");
            if (vm != null)
            {
                vm.cubeScene = scene;
                vm.InitColors();
                vm.IsTexture = false;
                return vm;
            }
            else
            {
                vm = new VM(scene);
                vm.ResetColors();
                return vm;
            }
        }

        private void InitColors()
        {
            this.ChannelColors[0] = new Vector4((float)color0.R / 255, (float)color0.G / 255, (float)color0.B / 255, 1);
            this.ChannelColors[1] = new Vector4((float)color1.R / 255, (float)color1.G / 255, (float)color1.B / 255, 1);
            this.ChannelColors[2] = new Vector4((float)color2.R / 255, (float)color2.G / 255, (float)color2.B / 255, 1);
            this.ChannelColors[3] = new Vector4((float)color3.R / 255, (float)color3.G / 255, (float)color3.B / 255, 1);
            this.ChannelColors[4] = new Vector4((float)color4.R / 255, (float)color4.G / 255, (float)color4.B / 255, 1);
            this.ChannelColors[5] = new Vector4((float)color5.R / 255, (float)color5.G / 255, (float)color5.B / 255, 1);
        }

        internal void ResetColors()
        {
            Color0 = System.Windows.Media.Colors.White;
            Color1 = System.Windows.Media.Colors.Yellow;
            Color2 = System.Windows.Media.Color.FromArgb(255, 0, 255, 0);
            Color3 = System.Windows.Media.Colors.Blue;
            Color4 = System.Windows.Media.Colors.Red;
            Color5 = System.Windows.Media.Color.FromArgb(255, 255, 128, 0);
            this.InitColors();
        }

        internal void Save()
        {
            XmlSerializationHelper.Serialize(this, "ViewModel.xml");
        }
    }

}
