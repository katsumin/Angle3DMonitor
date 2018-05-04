using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace Angle3DMonitor
{
	public class Charts : Application
	{
		bool movementsEnabled;
		Scene scene;
        Node plotNode;
        Node cameraNode;
		Camera camera;
		Octree octree;

		public Bar SelectedBar { get; private set; }

		[Preserve]
		public Charts(ApplicationOptions options = null) : base(options) { }

		static Charts()
		{
            UnhandledException += (s, e) =>
			{
				if (Debugger.IsAttached)
					Debugger.Break();
				e.Handled = true;
			};
		}

		protected override void Start ()
		{
			base.Start ();
			CreateScene ();
			SetupViewport ();
		}

		async void CreateScene ()
		{
			scene = new Scene ();
			octree = scene.CreateComponent<Octree> ();

			plotNode = scene.CreateChild();
			var baseNode = plotNode.CreateChild().CreateChild();
			var plane = baseNode.CreateComponent<StaticModel>();
            plane.Model = CoreAssets.Models.LinePrimitives.Basis;
            plotNode.Rotate(new Quaternion(0.0f, 180.0f, 0.0f));

            cameraNode = scene.CreateChild ();
			camera = cameraNode.CreateComponent<Camera>();
            //cameraNode.Position = new Vector3(10, 15, 10) / 1.75f;
            cameraNode.Position = new Vector3(10, 15, 10) / 1.75f;
            cameraNode.Rotation = new Quaternion(-0.121f, 0.878f, -0.305f, -0.35f);

			Node lightNode = cameraNode.CreateChild();
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 100;
			light.Brightness = 1.3f;

            // axis
            int k = 3;
			int size = 1;
            baseNode.Scale = new Vector3(size * 1.5f * k, size * 1.5f * k, size * 1.5f * k);
            var xText = new TextComponent(baseNode);
            xText.Value = "+X";
            xText.Position = new Vector3(1.0F, 0, 0);
            var yText = new TextComponent(baseNode);
            yText.Value = "+Y";
            yText.Position = new Vector3(0, 0, 1.0F);
            var zText = new TextComponent(baseNode);
            zText.Value = "+Z";
            zText.Position = new Vector3(0, 1.0F, 0);

            // angle object
			var boxNode = plotNode.CreateChild();
            boxNode.Scale = new Vector3(1 * k, 1 * k, 1 * k);
            boxNode.Position = new Vector3(0 , (float)(-k / 2.0), 0);
			var box = new Bar(new Color(RandomHelper.NextRandom(), RandomHelper.NextRandom(), RandomHelper.NextRandom(), 0.9f));
			boxNode.AddComponent(box);
            box.SetValueWithAnimation(1);
            SelectedBar = box;
			SelectedBar.Select();

			try
			{
				await plotNode.RunActionsAsync(new EaseBackOut(new RotateBy(2f, 0, 360, 0)));
			}
			catch (OperationCanceledException) {}
			movementsEnabled = true;
		}

		protected override void OnUpdate(float timeStep)
		{
			if (Input.NumTouches == 1 && movementsEnabled) {
				var touch = Input.GetTouch(0);
                plotNode.Rotate(new Quaternion(0, -touch.Delta.X, 0), TransformSpace.Local);
            } else if (Input.NumTouches > 1 && movementsEnabled) {
                float preDistance = IntVector2.Distance(Input.GetTouch(0).LastPosition, Input.GetTouch(1).LastPosition);
                float curDistance = IntVector2.Distance(Input.GetTouch(0).Position, Input.GetTouch(1).Position);
                var diff = curDistance - preDistance;
                double fullScale = Math.Max(Graphics.Width, Graphics.Height);
                camera.Zoom *= (float)((fullScale + diff) / fullScale);
                var zoom = camera.Zoom;
                //Debug.WriteLine("zoom: " + zoom + ", preDistance:" + preDistance + ", curDistance:" + curDistance + ", diff:" + diff);
            }
			base.OnUpdate(timeStep);
		}

        public void Rotate(float toValueX,float toValueY,float toValueZ)
		{
            SelectedBar.Rotate(toValueX, toValueZ, toValueY);
		}
		
		void SetupViewport ()
		{
			var renderer = Renderer;
			var vp = new Viewport(Context, scene, camera, null);
			renderer.SetViewport (0, vp);
		}
	}

    public class TextComponent : Component
    {
        Node textNode;
        Text3D text3D;

        public TextComponent(Node parentNode)
        {
            textNode = parentNode.CreateChild();
            textNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
            //textNode.Position = new Vector3(0, 10, 0);
            text3D = textNode.CreateComponent<Text3D>();
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 10);
            text3D.TextEffect = TextEffect.Stroke;
        }

        public string Value
        {
            get { return text3D.Text; }
            set { text3D.Text = value; }
        }

        public Vector3 Position
        {
            get { return textNode.Position; }
            set { textNode.Position = value; }
        }

        public float TextSize
        {
            get { return text3D.FontSize; }
            set { text3D.FontSize = value; }
        }
    }

	public class Bar : Component
	{
		Node barNode;
		Color color;
		float lastUpdateValue;

		public float Value
		{
			get { return barNode.Scale.Y; }
			set { barNode.Scale = new Vector3(1, value < 0.3f ? 0.3f : value, 1); }
		}

		public void SetValueWithAnimation(float value) => barNode.RunActionsAsync(new EaseBackOut(new ScaleTo(3f, 1, value, 1)));

		public Bar(Color color)
		{
			this.color = color;
			ReceiveSceneUpdates = true;
		}

		public override void OnAttachedToNode(Node node)
		{
			barNode = node.CreateChild();
			barNode.Scale = new Vector3(1, 0, 1); //means zero height
            var box = barNode.CreateComponent<Cylinder>();
			box.Color = color;
			base.OnAttachedToNode(node);
		}

		protected override void OnUpdate(float timeStep)
		{
			var pos = barNode.Position;
			var scale = barNode.Scale;
			barNode.Position = new Vector3(pos.X, scale.Y / 2f, pos.Z);
		}

		public void Deselect()
		{
			barNode.RemoveAllActions();//TODO: remove only "selection" action
			barNode.RunActions(new EaseBackOut(new TintTo(1f, color.R, color.G, color.B)));
		}

		public void Select()
		{
			Selected?.Invoke(this);
			// "blinking" animation
			barNode.RunActions(new RepeatForever(new TintTo(0.3f, 1f, 1f, 1f), new TintTo(0.3f, color.R, color.G, color.B)));
		}

        public void Rotate(float x,float y,float z)
        {
            Quaternion q = new Quaternion(x, y, z);
            //Debug.WriteLine(string.Format("{0},{1},{2} : {3}",x, y, z, q));
            barNode.Rotation = q;
            //Debug.WriteLine(barNode.Rotation);
        }

		public event Action<Bar> Selected;
	}
}