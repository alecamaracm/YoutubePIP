using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;

namespace YoutubePIP
{
    public partial class Form1 : Form
    {
        string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0";

        Size oldSize;
        bool IsMaximized = false;
        bool TheaterClicked = false;

        PointF realSize;

        Point startLocation;

        Rectangle previousPosition = Rectangle.Empty;

        KeyboardHook hook = new KeyboardHook();

        PointF speed = new PointF(0, 0);

        Queue<PointF> points = new Queue<PointF>();

        DateTime startTime;

        bool gravityEnabled = false;

        int moved = 0;
        public Form1()
        {
            InitializeComponent();           
            this.TopMost = true;
            oldSize = this.Size;
            this.Size = new Size(0, 0);
            // register the event that is fired after the key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(getURL);
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(YoutubePIP.ModifierKeys.Alt,  Keys.F6);

            this.DoubleBuffered = true;
            webView1.ScriptNotify += sc;
            webView1.IsScriptNotifyAllowed = true;

            
        }

        void injectTwtich(string channel)
        {
            WebClient client = new WebClient();
            webView1.Navigate("https://player.twitch.tv/?allowfullscreen&playsinline&player=twitch_everywhere&targetOrigin=https%3A%2F%2Fembed.twitch.tv&channel=" + channel + "&origin=https%3A%2F%2Fembed.twitch.tv");
            // webView1.InvokeScript("eval", "document.onclick = function(event){window.external.notify(event.clientX+';'+event.clientY);}");
            webView1.NavigationCompleted += SetMouseEvents;
            Task.Run(() => {
                Thread.Sleep(1000);
                try
                {
                    SetMouseEvents(null, null);//In case something didn´t work
                }
                catch { }
                Thread.Sleep(1000);
                try
                {
                    SetMouseEvents(null, null);//In case something didn´t work
                }
                catch { }
            });
        }

        private void SetMouseEvents(object sender, WebViewControlNavigationCompletedEventArgs e)
        {//.getElementById('video-playback') twitch
            webView1.InvokeScript("eval", "document.onmousedown = function(event){window.external.notify('mdn'+';'+event.clientX+';'+event.clientY);}");
            webView1.InvokeScript("eval", "document.onmouseup = function(event){window.external.notify('mup'+';'+event.clientX+';'+event.clientY);}");
            webView1.InvokeScript("eval", "document.onmousemove = function(event){window.external.notify('mmm'+';'+event.clientX+';'+event.clientY);}");
            webView1.InvokeScript("eval", "document.ondblclick = function(event){window.external.notify('dbl'+';'+event.clientX+';'+event.clientY);}");
            webView1.InvokeScript("eval", "document.onwheel = function(event){window.external.notify('whl'+';'+event.deltaY);}");

            webView1.InvokeScript("eval", "document.onkeydown = function(event){window.external.notify('kkk'+';'+event.keyCode);}");
        }

        private void sc(object sender, WebViewControlScriptNotifyEventArgs e)
        {            
            if(e.Value.StartsWith("mdn"))
            {
                Console.WriteLine("Mouse down");
                mouseDownF(int.Parse(e.Value.Split(';')[1]), int.Parse(e.Value.Split(';')[2]));
            }
            else if (e.Value.StartsWith("mup"))
            {
                Console.WriteLine("Mouse up");                
                mouseUpF(int.Parse(e.Value.Split(';')[1]), int.Parse(e.Value.Split(';')[2]));
            }
            else if (e.Value.StartsWith("mmm"))
            {
                Console.WriteLine("Mouse move");
                mouseMoveF(int.Parse(e.Value.Split(';')[1]), int.Parse(e.Value.Split(';')[2]));
            }
            else if (e.Value.StartsWith("dbl"))
            {
                Console.WriteLine("Double click");
                mouseDBLF(int.Parse(e.Value.Split(';')[1]), int.Parse(e.Value.Split(';')[2]));
            }
            else if (e.Value.StartsWith("kkk"))
            {
                Console.WriteLine("Key");
                keyF(e.Value.Split(';')[1]);
            }else if (e.Value.StartsWith("whl"))
            {
                Console.WriteLine("Wheel");
                resizeWheel(double.Parse(e.Value.Split(';')[1],CultureInfo.InvariantCulture));
            }
        }

        object antiTh = new object();

        private void resizeWheel(double v)
        {
            lock(antiTh)
            {
                double toAddY = v / -1f;
                double toAddX = 1.7777777f * toAddY;

                if ((realSize.X > Screen.PrimaryScreen.Bounds.Width * 0.8 && toAddX > 0) || (realSize.Y > Screen.PrimaryScreen.Bounds.Height * 0.8 && toAddY > 0))
                {
                    toAddX = 0;
                    toAddY = 0;
                    realSize.X = (float)(Screen.PrimaryScreen.Bounds.Width * 0.85);
                    realSize.Y = (float)(Screen.PrimaryScreen.Bounds.Height * 0.85);
                }
                if ((realSize.X < Screen.PrimaryScreen.Bounds.Width * 0.15 && toAddX < 0) || (realSize.Y < Screen.PrimaryScreen.Bounds.Height * 0.15 && toAddY < 0))
                {
                    toAddX = 0;
                    toAddY = 0;
                    realSize.X = (float)(Screen.PrimaryScreen.Bounds.Width * 0.10);
                    realSize.Y = (float)(Screen.PrimaryScreen.Bounds.Height * 0.10);
                }

                realSize.X += (float)toAddX;
                realSize.Y += (float)toAddY;



               

                if (toAddX != 0)
                {
                    if (realLocation.X == 10)
                    {

                    }
                    else if (realLocation.X+Width >= Screen.PrimaryScreen.Bounds.Width -12)
                    {
                        realLocation.X -= (float)toAddX;
                    }
                    else
                    {
                        realLocation.X -= (float)toAddX / 2;
                    }
                }

                if (toAddY != 0)
                {
                    if (realLocation.Y == 10)
                    {

                    }
                    else if (realLocation.Y+Height >= Screen.PrimaryScreen.Bounds.Height - 12)
                    {
                        realLocation.Y -= (float)toAddY;
                    }
                    else
                    {
                        realLocation.Y -= (float)toAddY / 2;
                    }

                }

            


                Location = new Point((int)realLocation.X, (int)realLocation.Y);
                Width = (int)realSize.X;
                Height = (int)realSize.Y;
                //  if (realLocation.X > Screen.PrimaryScreen.Bounds.Width / 2) realLocation.X -= toAddX;
                //  if (realLocation.X > Screen.PrimaryScreen.Bounds.Height / 2) realLocation.Y -= toAddY;
            }

        }

        void keyF(string key)
        {
            if(key=="70")
            {
                SetWindowState(this.IsMaximized ? FormWindowState.Normal : FormWindowState.Maximized, false);
            }
           
            if (key=="27")
            {
                SetWindowState(FormWindowState.Normal, false);
            }

            if(key=="71")
            {
                gravityEnabled = !gravityEnabled;
            }
        }

        private void getURL(object sender, KeyPressedEventArgs e)
        {
            string url = Clipboard.GetText();
            if(url.Contains("youtube.com"))
            {
                int indexOfV = url.IndexOf("?v=");
                if(indexOfV!=-1)
                {
                    url = url.Substring(indexOfV + 3);
                    if(url.Contains("&"))
                    {
                        url = url.Split('&')[0];
                    }

                    webView1.Navigate("https://www.youtube.com/embed/"+url);
                    Task.Run(() => {
                        Thread.Sleep(1000);
                        try
                        {
                            SetMouseEvents(null, null);//In case something didn´t work
                        }
                        catch { }
                        Thread.Sleep(1000);
                        try
                        {
                            SetMouseEvents(null, null);//In case something didn´t work
                        }
                        catch { }
                    });
                    this.Visible = true;
                 
                }

            }
            else if(url.Contains("twitch.tv"))
            {
                if(url.Contains("?")==false)
                {
                    url = url.Split('/')[3];
                    injectTwtich(url);
                    this.Visible = true;
                }
            }

        }
        
     
        
        bool mouseDown = false;
        float screeenX = 0;
        float screeenY = 0;


        private void Form1_Shown(object sender, EventArgs e)
        {                  
            previousPosition = this.Bounds;
            this.Visible = false;
            this.Size = oldSize;
            realSize = new PointF(Size.Width, Size.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                SetWindowState(this.WindowState, false);
            }
            else if (!this.IsMaximized)
            {
                this.previousPosition = this.Bounds;
            }
        }

        private void SetWindowState(FormWindowState state, bool setSize)
        {
            if (state == FormWindowState.Maximized)
            {
                this.IsMaximized = true;
                if (setSize) this.previousPosition = this.Bounds;
                this.WindowState = FormWindowState.Normal;
                this.Location = Point.Empty;
                this.Size = System.Windows.Forms.Screen.FromHandle(this.Handle).Bounds.Size;
            }
            else
            {
              
                this.Bounds = this.previousPosition;
                //realSize = new PointF(Size.Width, Size.Height);
                this.IsMaximized = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            base.ProcessCmdKey(ref msg, keyData);
            if (keyData == Keys.F11)
            {
                SetWindowState(this.IsMaximized ? FormWindowState.Normal : FormWindowState.Maximized, true);
                return true;
            }else if(keyData==Keys.Escape)
            {
                SetWindowState(FormWindowState.Normal, false);
                return true;
            }
            else
            {
                return false;
            }
        }

      

        void mouseDownF(int x,int y)
        {
            
            if (IsMaximized == false && Height-y>50)
            {
                screeenX = x;
                screeenY = y;
                mouseDown = true;
                moved = 0;
                startLocation = Location;
                startTime = DateTime.Now;
                startLocation = Location;
                points.Clear();
                realLocation = Location;
                points.Enqueue(realLocation);
            }



            /*if (geckoWebBrowser1.Url.Host.Contains("youtu"))
            {
                GeckoHtmlElement elm = (GeckoHtmlElement)e.Target.CastToGeckoElement();
                switch (elm.ClassName)
                {
                    case "ytp-fullscreen-button ytp-button":
                        if (this.geckoWebBrowser1.Document.GetElementsByClassName("ytp-size-button ytp-button").FirstOrDefault() is GeckoHtmlElement theater)
                        {
                            if (this.TheaterClicked == false)
                            {
                                theater.Click();
                                this.TheaterClicked = true;
                            }
                        }
                        break;
                    case "ytp-size-button ytp-button":
                        this.TheaterClicked = !this.TheaterClicked;
                        break;
                    default:
                        break;
                }
            }*/
        }

        void mouseUpF(int x,int y)
        {
            if(moved<=1 && Width-x<60 && Height-y<60)
            {
                keyF("70");//fullscreen
            }

            if (moved > 1 && points.Count>0)
            {
                try
                {
    webView1.InvokeScript("eval", "document.elementFromPoint("+x+", "+y+").click();");
                }catch
                {

                }

                PointF lastPoint = points.Dequeue();
                double ms = points.Count/10000.0;
                int diffX = Location.X - (int)lastPoint.X;
                int diffY = Location.Y - (int)lastPoint.Y;
                double sqrt = Math.Sqrt(Math.Pow(diffX,2)+Math.Pow(diffY,2));
                double multiplier = sqrt * ms;
                speed = new PointF((float)(diffX * multiplier), (float)(diffY * multiplier));
            }
            mouseDown = false;
        }

        
        bool XboxIn=false;
        bool secBoxIn = false;
        bool safelyOutside = true;

        void mouseMoveF(int X,int Y)
        {
            if(Width-X<32 && Y<32) //Inside the X
            {
                secBoxIn = true;

            }else 
            {
                secBoxIn = false;
            }

            if (Width - X > 48 && Y > 48) //Sure that the mouse is away from the X
            {
                safelyOutside = true;

            }
            else
            {
                safelyOutside = false;
            }

            if (mouseDown)
            {
                moved++;
                
                this.realLocation = new Point((int)(Cursor.Position.X - screeenX), (int)(Cursor.Position.Y - screeenY));

                speed = PointF.Empty;
                ApplyBounds();

                points.Enqueue(realLocation);
                if (points.Count > 10) points.Dequeue();
            }
        }

        private void ApplyBounds()
        {
            if (IsMaximized) return;
            if (realLocation.X < 10)
            {
                if (Math.Abs(speed.X) < 1)
                {
                    realLocation.X =  10;
                    if (speed.X < 0) speed.X = 0;
                }
                else
                {
                    if (speed.X < 0) speed.X = -speed.X * 0.8f;
                }
            }

            if (realLocation.Y < 10)
            {
                if (Math.Abs(speed.Y) < 1)
                {
                    realLocation.Y =  10;
                    if (speed.Y < 0) speed.Y = 0;
                }
                else
                {
                    if (speed.Y < 0) speed.Y = -speed.Y * 0.8f;
                }
            }



            if (realLocation.X + Width > Screen.PrimaryScreen.Bounds.Width - 10)
            {
                if (Math.Abs(speed.X) < 1)
                {
                    realLocation.X = Screen.PrimaryScreen.Bounds.Width - 10 - Width;
                    if (speed.X > 0) speed.X = 0;
                }
                else
                {
                    if (speed.X > 0) speed.X = -speed.X * 0.8f;
                }
            }

            if (realLocation.Y + Height > Screen.PrimaryScreen.Bounds.Height - 10)
            {
                if (Math.Abs(speed.Y) < 2.75f )
                {
                    realLocation.Y = Screen.PrimaryScreen.Bounds.Height - 10 - Height;
                    if (speed.Y > 0) speed.Y = 0;
                }
                else
                {
                    if (speed.Y > 0) speed.Y = -speed.Y * 0.8f;
                }
                   
             
            }
           
            this.Location = new Point((int)realLocation.X, (int)realLocation.Y);
        }

        void mouseDBLF(int x,int y)
        {
            SetWindowState(this.IsMaximized ? FormWindowState.Normal : FormWindowState.Maximized, false);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            realLocation = Location;
           
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            float gravityAccel = gravityEnabled ? 2.9f : 0;


            float accelX = 0.085f*(float)Math.Sqrt(Math.Pow(Math.Abs(speed.X),1.65));
            float accelY = 0.085f * (float)Math.Sqrt(Math.Pow(Math.Abs(speed.Y), 1.65) );
            float newSpX = speed.X;
            float newSpY = speed.Y;

            if (Screen.PrimaryScreen.Bounds.Height-Height-10 <= Location.Y + gravityAccel) gravityAccel = 0;

            if(mouseDown==false)newSpY += gravityAccel;

            if (float.IsNaN(accelX)) accelX = 0;
            if (float.IsNaN(accelY)) accelY = 0;
            if (float.IsNaN(newSpX)) newSpX = 0;
            if (float.IsNaN(newSpY)) newSpY = 0;

            bool shouldShow = (secBoxIn || XboxIn)&&!safelyOutside;
            if(shouldShow)
            {
                if (panel1.Top < 0) panel1.Top+=3;
            }else
            {
                if (panel1.Top > -32) panel1.Top-=3;
            }
            

            if (newSpX>0)
            {
                if (newSpX < accelX)
                {
                    newSpX = 0;
                }
                else
                {
                    newSpX -= accelX;
                }
            }
            else
            {
                if (newSpX > -accelX)
                {
                    newSpX = 0;
                }
                else
                {
                    newSpX += accelX;
                }
            }

            if (newSpY > 0)
            {
                if (newSpY < accelY)
                {
                    newSpY = 0;
                }
                else
                {
                    newSpY -= accelY;
                }
            }
            else
            {
                if (newSpY > -accelY)
                {
                    newSpY = 0;
                }
                else
                {
                    newSpY += accelY;
                }
            }

            speed = new PointF(newSpX, newSpY);


            realLocation = new PointF(realLocation.X + speed.X, realLocation.Y + speed.Y);
            if (float.IsNaN(realLocation.X)) realLocation.X = 0;
            if (float.IsNaN(realLocation.Y)) realLocation.Y = 0;

            ApplyBounds();

            if(points.Count>1)points.Dequeue();
        }

        PointF realLocation = new PointF(0, 0);

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            XboxIn = true;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            XboxIn = false;
        }

        private void pictureBox1_DragLeave(object sender, EventArgs e)
        {
            XboxIn = false;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            XboxIn = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            webView1.Navigate("about:blank");
            this.Visible = false;
        }
    }
}
