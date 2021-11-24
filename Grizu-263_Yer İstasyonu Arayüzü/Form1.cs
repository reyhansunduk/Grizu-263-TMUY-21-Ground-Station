using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using Microsoft.WindowsAPICodePack.Shell;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using LumenWorks.Framework.IO.Csv;
using Kitware.VTK;
using System.Net;
using System.Threading;
using VisioForge.Types.OutputFormat;
using VisioForge.Types.VideoEffects;
using VisioForge.Types;

namespace Grizu_263_Yer_İstasyonu_Arayüzü 
    
{
    public partial class Form1 : Form
    {
       

        #region global_variables
        string[] ports = SerialPort.GetPortNames();
        string genel;
        string[] verileri_böl;
        string dosyaya_yazdır = @"TMUY2021_79108_TLM.csv";
        string dosyaya_yazdır_yedek= @"TMUY2021_79108_TLM_yedek.csv";
        int basinc_dosyasi = 0;
        int counter = 0;
        vtkTransform transform;
        vtkRenderer renderer;
        vtkRenderWindow renderWindow;
        #endregion

        public Form1()
        {
            InitializeComponent();
            videoCapture2.OnError += OnError;
            videoCapture1.OnError += OnError;
            Control.CheckForIllegalCrossThreadCalls = false;
            
        }
        private void OnError(object sender, ErrorsEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
                comboBox1.SelectedIndex = 0; 
            }
            
            comboBox2.Items.Add("9600");

            comboBox2.SelectedIndex = 0;

            textBox1.Text = "ftp://192.168.1.3";
            textBox2.Text = "pi";
            textBox3.Text = "raspberry";
            textBox3.PasswordChar = '*';

            Simülasyon();
           
        }
        private void Simülasyon()
        {
        

            try
            {
                vtkSTLReader reader = vtkSTLReader.New();
                reader.SetFileName(@"C:\Users\reyha\Desktop\Grizu-263 Uzay Takımı Yer İstasyonu Arayüzü\Grizu-263_Yer İstasyonu Arayüzü\bin\Debug\mekanik2.stl");
                reader.Update();
                vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
                vtkRenderWindowInteractor vtkWidget = new vtkRenderWindowInteractor();
                mapper.SetInputConnection(reader.GetOutputPort());
                vtkActor actor = vtkActor.New();
                actor.SetMapper(mapper);
                transform = new vtkTransform();
                transform.RotateX(0);
                transform.RotateY(0);
                transform.RotateZ(0);

                vtkTransformPolyDataFilter f11 = new vtkTransformPolyDataFilter();
                f11.SetTransform(transform);
                f11.SetInputConnection(reader.GetOutputPort());
                f11.Update();
                mapper.SetInputConnection(f11.GetOutputPort());
                actor.SetUserTransform(transform);
                renderWindow = vtkRenderWindow.RenderWindow;
                renderer = renderWindow.GetRenderers().GetFirstRenderer();
                renderer.SetBackground(0.1, 0.1, 0.1); 
                renderer.AddActor(actor);
            }
            catch( Exception )
            {
               
            }
        }
        public string Username;
        public string Filename;
        public string Fullname;
        public string Server;
        public string Password;
        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Multiselect = true, ValidateNames = true, Filter = "All Files|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    Username = textBox2.Text;
                    Password = textBox3.Text;
                    Server = textBox1.Text;
                    Filename = fi.Name;
                    Fullname = fi.FullName;
                }
            }
            backgroundWorker1.RunWorkerAsync();  
            Thread.Sleep(1000);         
        }
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
            
            if (serialPort1.IsOpen==false)
            {
                if (comboBox1.Text == "")
                    return;
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt16(comboBox2.Text);
                try
                {
                    serialPort1.Open();
                }
                catch (Exception hata)
                {
                    MessageBox.Show("Hata:" + hata.Message);
                }
                butonları_aktif_et();
              
            }
        }
        private void butonları_aktif_et()
        {
            button2.Enabled = true; 
            button3.Enabled = true; 
            button4.Enabled = true; 
            button5.Enabled = true; 
            button7.Enabled = true; 
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            ımu_kalibre.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            if (serialPort1.IsOpen==true)
            {
                serialPort1.Close();
            }
          
            videoCapture1.Stop();
            videoCapture2.Stop();
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                File.AppendAllText(dosyaya_yazdır, genel, Encoding.GetEncoding("utf-8"));     
                File.AppendAllText(dosyaya_yazdır_yedek, genel, Encoding.GetEncoding("utf-8"));
                genel = serialPort1.ReadLine();
                verileri_böl = genel.Split(',');
                serialPort1.Encoding = Encoding.GetEncoding("utf-8");

                verileri_listele();
                genel_harita(); sekme_harita();
                videoCapture2.Update(); videoCapture1.Update();

            }
            catch(System.IO.IOException)
            {

            }
        }
        private void verileri_listele()
        {
            
            try
            {
                switch (verileri_böl[11])
                {
                    case "Ucus Bekleniyor":
                        listView3.Items[0].ForeColor = Color.Green; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[1].ForeColor = Color.White;
                        break;
                    case "Model Uydu Yukselmekte":
                        listView3.Items[0].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[1].ForeColor = Color.Green;
                        break;
                    case "Model Uydu Inise Gecti":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[2].ForeColor = Color.Green;
                        break;
                    case "Gorev Yuku Tasiyicidan Ayrildi":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[3].ForeColor = Color.Green;
                        break;
                        case "Gorev Yuku Yavaslatiliyor":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[4].ForeColor = Color.Green;
                        break;
                    case "Gorev Yuku Askida":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[5].ForeColor = Color.Green;
                        break;
                    case "Gorev Yuku Inise Devam Etmekte":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[6].ForeColor = Color.Green;
                        break;
                    case "Bonus Gorev Iptal Edildi":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[7].ForeColor = Color.Green;
                        break;
                    case "Gorev Yuku Kurtarilmayi Bekliyor":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White; listView3.Items[9].ForeColor = Color.White;
                        listView3.Items[8].ForeColor = Color.Green;
                        break;
                    case "Gorev Tamamlandi":
                        listView3.Items[1].ForeColor = Color.White; listView3.Items[2].ForeColor = Color.White; listView3.Items[3].ForeColor = Color.White; listView3.Items[4].ForeColor = Color.White; listView3.Items[5].ForeColor = Color.White; listView3.Items[6].ForeColor = Color.White; listView3.Items[7].ForeColor = Color.White; listView3.Items[8].ForeColor = Color.White; listView3.Items[0].ForeColor = Color.White;
                        listView3.Items[9].ForeColor = Color.Green;
                        break;
                    default:
                        break;

                }

                switch (verileri_böl[11])
                {
                    case "0":
                        verileri_böl[11] = listView3.Items[0].Text; break;
                    case "1":
                        verileri_böl[11] = listView3.Items[1].Text; break;
                    case "2":
                        verileri_böl[11] = listView3.Items[2].Text; break;
                    case "3":
                        verileri_böl[11] = listView3.Items[3].Text; break;
                    case "4":
                        verileri_böl[11] = listView3.Items[4].Text; break;
                    case "5":
                        verileri_böl[11] = listView3.Items[5].Text; break;
                    case "6":
                        verileri_böl[11] = listView3.Items[6].Text; break;
                    case "7":
                        verileri_böl[11] = listView3.Items[7].Text; break;
                    case "8":
                        verileri_böl[11] = listView3.Items[8].Text; break;
                    case "9":
                        verileri_böl[11] = listView3.Items[9].Text; break;
                    default:
                        break;
                }

                genel = "";
                genel += verileri_böl[0];
                genel += ",";
                genel += verileri_böl[1];
                genel += ",";
                genel += verileri_böl[2];
                genel += ",";
                genel += verileri_böl[3];
                genel += ",";
                genel += verileri_böl[4];
                genel += ",";
                genel += verileri_böl[5];
                genel += ",";
                genel += verileri_böl[6];
                genel += ",";
                genel += verileri_böl[7];
                genel += ",";
                genel += verileri_böl[8];
                genel += ",";
                genel += verileri_böl[9];
                genel += ",";
                genel += verileri_böl[10];
                genel += ",";
                genel += verileri_böl[11];
                genel += ",";
                genel += verileri_böl[12];
                genel += ",";
                genel += verileri_böl[13];
                genel += ",";
                genel += verileri_böl[14];
                genel += ",";
                genel += verileri_böl[15];
                genel += ",";
                genel += verileri_böl[16];
                genel += ",";
                genel += verileri_böl[17];
             

                label9.Text = verileri_böl[7];
                label16.Text = verileri_böl[1];
                label17.Text = verileri_böl[2];
                label13.Text = verileri_böl[12];
                label14.Text = verileri_böl[13];
                label15.Text = verileri_böl[14];

                listView1.View = View.Details;
                ListViewItem item1 = new ListViewItem(verileri_böl[0]);
                item1.SubItems.Add(verileri_böl[1]);
                item1.SubItems.Add(verileri_böl[2]);
                item1.SubItems.Add(verileri_böl[3]);
                item1.SubItems.Add(verileri_böl[4]);
                item1.SubItems.Add(verileri_böl[5]);
                item1.SubItems.Add(verileri_böl[6]);
                item1.SubItems.Add(verileri_böl[7]);
                item1.SubItems.Add(verileri_böl[8]);
                item1.SubItems.Add(verileri_böl[9]);
                item1.SubItems.Add(verileri_böl[10]);
                item1.SubItems.Add(verileri_böl[11]);
                item1.SubItems.Add(verileri_böl[12]);
                item1.SubItems.Add(verileri_böl[13]);
                item1.SubItems.Add(verileri_böl[14]);
                item1.SubItems.Add(verileri_böl[15]);
                item1.SubItems.Add(verileri_böl[16]);
                item1.SubItems.Add(verileri_böl[17]);

                listView1.Items.Add(item1);
                this.listView1.Items[this.listView1.Items.Count - 1].EnsureVisible(); 
                listView2.View = View.Details;
                ListViewItem item2 = new ListViewItem(verileri_böl[0]);
                item2.SubItems.Add(verileri_böl[1]);
                item2.SubItems.Add(verileri_böl[2]);
                item2.SubItems.Add(verileri_böl[3]);
                item2.SubItems.Add(verileri_böl[4]);
                item2.SubItems.Add(verileri_böl[5]);
                item2.SubItems.Add(verileri_böl[6]);
                item2.SubItems.Add(verileri_böl[7]);
                item2.SubItems.Add(verileri_böl[8]);
                item2.SubItems.Add(verileri_böl[9]);
                item2.SubItems.Add(verileri_böl[10]);
                item2.SubItems.Add(verileri_böl[11]);
                item2.SubItems.Add(verileri_böl[12]);
                item2.SubItems.Add(verileri_böl[13]);
                item2.SubItems.Add(verileri_böl[14]);
                item2.SubItems.Add(verileri_böl[15]);
                item2.SubItems.Add(verileri_böl[16]);
                item2.SubItems.Add(verileri_böl[17]);

                listView2.Items.Add(item2);

                this.listView2.Items[this.listView2.Items.Count - 1].EnsureVisible(); 

                chart1.Series["BASINÇ"].Points.AddXY(verileri_böl[2], verileri_böl[3]);
                chart1.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart1.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);

                this.chart2.Series["YÜKSEKLİK"].Points.AddXY(verileri_böl[2], verileri_böl[4]);
                chart2.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart2.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);

                this.chart3.Series["İNİŞ HIZI"].Points.AddXY(verileri_böl[2], verileri_böl[5]);
                chart3.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart3.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);

                this.chart4.Series["SICAKLIK"].Points.AddXY(verileri_böl[2], verileri_böl[6]);
                chart4.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart4.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);

                this.chart5.Series["PİL GERİLİMİ"].Points.AddXY(verileri_böl[2], verileri_böl[7]);
                chart5.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart5.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);

                this.chart6.Series["DÖNÜŞ SAYISI"].Points.AddXY(verileri_böl[2], verileri_böl[15]);
                chart6.ChartAreas[0].AxisX.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
                chart6.ChartAreas[0].AxisY.ScaleView.Scroll(System.Windows.Forms.DataVisualization.Charting.ScrollType.Last);
            }
            catch (IndexOutOfRangeException)
            {
             
            }
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            serialPort1.Write("1");
        }
        private void button7_Click(object sender, EventArgs e)
        {
            serialPort1.Write("3");
        }
        private void button8_Click(object sender, EventArgs e)
        {
            serialPort1.Write("4");
        }
        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackColor = Color.Green;
        }
        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.White;
        }
        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            button2.BackColor = Color.Red;
        }
        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.BackColor = Color.White;
        }
        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            button4.BackColor = Color.Green;
        }
        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackColor = Color.White;
        }
        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            button3.BackColor = Color.Green;
        }
        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.BackColor = Color.White;
        }
        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            button7.BackColor = Color.Green;
        }
        private void button8_MouseLeave(object sender, EventArgs e)
        {
            button8.BackColor = Color.White;
        }
        private void button7_MouseLeave(object sender, EventArgs e)
        {
            button7.BackColor = Color.White;
        }
        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            button8.BackColor = Color.Red;
        }
        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            button5.BackColor = Color.Green;
        }
        private void button5_MouseLeave(object sender, EventArgs e)
        {
            button5.BackColor = Color.White;
        }
        private void sekme_harita()
        {
            try
            {
                
               

                
                double lat = Convert.ToDouble(verileri_böl[8].ToString().Replace(".", ",")); 
                double longe = Convert.ToDouble(verileri_böl[9].ToString().Replace(".", ","));

                gMapControl2.Manager.Mode = AccessMode.ServerAndCache;

                gMapControl2.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                gMapControl2.DragButton = MouseButtons.Right;
                gMapControl2.Position = new GMap.NET.PointLatLng(lat, longe);
                gMapControl2.MinZoom = 10;   //11
                gMapControl2.MaxZoom = 150;
                gMapControl2.Zoom = 15;   //17

                gMapControl2.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
                gMapControl2.CanDragMap = true;
                gMapControl1.MouseWheelZoomEnabled = true;
                gMapControl2.IgnoreMarkerOnMouseWheel = true;

                gMapControl2.ShowCenter = false;  

                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat, longe), GMarkerGoogleType.green);

                gMapControl2.Overlays.Clear();
                markersOverlay.Markers.Add(marker);
                gMapControl2.Overlays.Add(markersOverlay);
                gMapControl2.Refresh();
                gMapControl2.Update();
            }
            catch (IndexOutOfRangeException)
            {
           
            }
           
            catch (FormatException)
            {

            }
        }
        private void genel_harita()
        { 
            try
            {
                double lat1 = Convert.ToDouble(verileri_böl[8].ToString().Replace(".", ",")); 
                double longe2 = Convert.ToDouble(verileri_böl[9].ToString().Replace(".", ","));
                gMapControl1.DragButton = MouseButtons.Right;
                gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                gMapControl1.Position = new GMap.NET.PointLatLng(lat1, longe2);
                gMapControl1.MinZoom = 10;
                gMapControl1.MaxZoom = 150;
                gMapControl1.Zoom = 15;

                gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
                gMapControl1.CanDragMap = true;
                gMapControl1.MouseWheelZoomEnabled = true;
                                                             
                gMapControl1.IgnoreMarkerOnMouseWheel = true;

                gMapControl1.ShowCenter = false;  

                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat1, longe2),  GMarkerGoogleType.green);

                gMapControl1.Overlays.Clear();
                markersOverlay.Markers.Add(marker);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
                gMapControl1.Update();
            }
            catch(IndexOutOfRangeException)
            {
             
            }
            catch(FormatException)
            {

            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label22.Text = $" {e.ProgressPercentage}%";
            label22.Update();
            progressBar1.Value = e.ProgressPercentage;
            progressBar1.Update();
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", Server, Filename)));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                Stream ftpstream = request.GetRequestStream();
                FileStream fs = File.OpenRead(Fullname);

                
                byte[] buffer = new byte[1024];
                double total = (double)fs.Length;
                int byteRead = 0;
                double read = 0;
                do
                {
                    byteRead = fs.Read(buffer, 0, 1024);
                    ftpstream.Write(buffer, 0, byteRead);
                    read += (double)byteRead;
                    double percentage = read / total * 100;
                    backgroundWorker1.ReportProgress((int)percentage);
                }
                while (byteRead != 0);
                fs.Close();
                ftpstream.Close();
            }
            catch(System.UriFormatException)
            { 
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
             label23.Text = "Gönderildi!";

         
        }
        private void button9_Click(object sender, EventArgs e)
        {
            serialPort1.Write("2");
        }
        int i = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            
               DialogResult secenek = MessageBox.Show("'TMUY2021_79108_TLM.csv' dosyasını temizlemek istiyor musunuz?", "Uyarı!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

               if (secenek == DialogResult.Yes)
               {
                serialPort1.Write("5");
                TextWriter tw = new StreamWriter(dosyaya_yazdır);
                tw.Write("");
                tw.Close();
                File.WriteAllText(dosyaya_yazdır, "TAKIM NO,PAKET NUMARASI,GONDERME SAATI,BASINC,YUKSEKLIK,INIS HIZI,SICAKLIK,PIL GERILIMI,GPS LATITUDE,GPS LONGITUDE,GPS ALTITUDE,UYDU STATUSU,PITCH,ROLL,YAW,DONUS SAYISI,VIDEO AKTARIM BILGISI, SON KOMUT\n");

               }
               else if (secenek == DialogResult.No)
               {
                serialPort1.Write("5");
               }

        }
        private void button6_Click_2(object sender, EventArgs e)
        {
            try
            {
               genel_kamera();
            }
            catch (Exception)
            {

            }
        }
        
        private void tabPage4_Click_1(object sender, EventArgs e)
        {
            try
            {
                sekme_kamera();
            }
            catch (Exception)
            {

            }
        }
      
        private void tabPage3_Click(object sender, EventArgs e)
        {
           // sekme_harita();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            serialPort1.Write("6");
        }

        double before_X = 0, X;
        double before_Y = 0, Y;
        double before_Z = 0, Z;
        private void timer1_Tick(object sender, EventArgs e)
        {
           
            try
            {
                if (genel!=null)
                {
                     X = double.Parse(verileri_böl[12], System.Globalization.CultureInfo.InvariantCulture);
                     Y = double.Parse(verileri_böl[13], System.Globalization.CultureInfo.InvariantCulture);


                    transform.RotateX(X - before_X);  ////ÖN ARKA
                    transform.RotateZ(before_Y - Y);   ///SAĞ SOL
                    transform.RotateY(0);          ////KENDİ EKSENİ

                  

                    before_X = X;
                    before_Y = Y;
                    before_Z = Z;

                    renderer.ResetCamera();
                    renderWindow.Render();
                }
               
            }
            catch(IndexOutOfRangeException)
            {
             
            }
            catch(FormatException)
            {

            }
            catch(NullReferenceException)
            {

            }
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            button9.BackColor = Color.Green;
        }

        private void button9_MouseLeave(object sender, EventArgs e)
        {
            button9.BackColor = Color.White;
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            button10.BackColor = Color.Green;
        }

        private void button10_MouseLeave(object sender, EventArgs e)
        {
            button10.BackColor = Color.White;
        }
        private void ımu_kalibre_Click(object sender, EventArgs e)
        {
            DialogResult secenek = MessageBox.Show("Sensörü kalibre etmek istediğinizden emin misiniz?", "Uyarı!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (secenek == DialogResult.Yes)
            {
                serialPort1.Write("7");
               

            }
            else if (secenek == DialogResult.No)
            {
            
            }

        }

        private void ımu_kalibre_MouseDown(object sender, MouseEventArgs e)
        {
            ımu_kalibre.BackColor = Color.Green;
        }

        private void ımu_kalibre_MouseLeave(object sender, EventArgs e)
        {
            ımu_kalibre.BackColor = Color.White;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (basinc_dosyasi == 1)
            {
                StreamReader yaz = new StreamReader(openFileDialog2.FileName);

                string satir = yaz.ReadLine();

                if (counter > 0)
                {
                    for (int i = 0; i < counter; i++)
                        satir = yaz.ReadLine();
                }

                if (satir != null)
                {
                    serialPort1.Write(satir + ",");
                    counter++;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            basinc_dosyasi = 1;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            serialPort1.Write("8");
        }

        private void label3_Click(object sender, EventArgs e)
        {
            TextWriter tw = new StreamWriter(dosyaya_yazdır);
            tw.Write("");
            tw.Close();
            File.WriteAllText(dosyaya_yazdır_yedek, "TAKIM NO,PAKET NUMARASI,GONDERME SAATI,BASINC,YUKSEKLIK,INIS HIZI,SICAKLIK,PIL GERILIMI,GPS LATITUDE,GPS LONGITUDE,GPS ALTITUDE,UYDU STATUSU,PITCH,ROLL,YAW,DONUS SAYISI,VIDEO AKTARIM BILGISI, SON KOMUT\n");
        }

        int kaydet = 0;
        private void genel_kamera()
        {
            videoCapture2.IP_Camera_Source = new VisioForge.Types.Sources.IPCameraSourceSettings() { URL = "http://192.168.1.3:8000/stream.mjpg", Type = VisioForge.Types.VFIPSource.Auto_LAV };
            videoCapture2.Audio_PlayAudio = videoCapture2.Audio_RecordAudio = true;

       
            videoCapture2.Output_Filename = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\TMUY2021_79108_VIDEO\\" + "(1)TMUY2021_79108_VIDEO.mp4";
            videoCapture2.Output_Format = new VFM4AOutput();
            

            videoCapture2.Mode = VisioForge.Types.VFVideoCaptureMode.IPCapture;  
            videoCapture2.Start();
            videoCapture2.Update();
        }
        int kaydet_yedek=0;
        private void sekme_kamera()
        {
            videoCapture1.IP_Camera_Source = new VisioForge.Types.Sources.IPCameraSourceSettings() { URL = "http://192.168.1.3:8000/stream.mjpg", Type = VisioForge.Types.VFIPSource.Auto_LAV };

            videoCapture1.Audio_PlayAudio = videoCapture1.Audio_RecordAudio = false;
            videoCapture1.Output_Filename = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\TMUY2021_79108_VIDEO_Yedek.wmv";   
            videoCapture1.Output_Format = new VFWMVOutput();  

            
            videoCapture1.Output_Filename = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\TMUY2021_79108_VIDEO_Yedek\\" + "(1)TMUY2021_79108_VIDEO.mp4";
            videoCapture1.Output_Format = new VFM4AOutput();
          

            videoCapture1.Mode = VisioForge.Types.VFVideoCaptureMode.IPCapture;   
            videoCapture1.Start();
            videoCapture1.Update();
        }
    }
}
