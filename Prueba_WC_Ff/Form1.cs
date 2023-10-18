using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using System.Threading;

namespace Prueba_WC_Ff
{
    public partial class Form1 : Form
    {

        delegate void Delegate_Act_Labels(string inf, string conf);
        delegate void Delegate_show_img(Bitmap img);

        private int Ganancia;
        private int TiempoExp;


        private bool Iniciar = false;

        static string[] labelClassification = { "A", "B", "C", "D", "E" };

        public Form1()
        {
            InitializeComponent();
            CustomizeDesing();
                        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //CargaDispositivos();        // Invocar el método
            btn_detener.Enabled = false;
            Size size_pb_video = new Size(100, 100);

            main(true);

        }

        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        // Mantener Funcionalidad Aero Snap
        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;   //Standar Title Bar - Snap Window

            // Remove border and keep snap window
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                return;
            }

            base.WndProc(ref m);
        }




        private void btn_iniciar_Click(object sender, EventArgs e)
        {


            Iniciar = true;
            main(true);

            btn_iniciar.Enabled = false;
            btn_detener.Enabled = true;

            ////// Clear camera list before releasing system
            ////camList.Clear();

            ////// Release system
            ////system.Dispose();

            ////Console.WriteLine("\nDone! Press Enter to exit...");
            ////Console.ReadLine();

            //string prueba = "123";

        }

        private void CustomizeDesing ()
        {
            panel_SubMenu_Dispositivos.Visible = false;
            panel_SubMenu_Propiedades.Visible = false;
        }

        private void HideSubMenu()
        {
            if (panel_SubMenu_Dispositivos.Visible == true)
                panel_SubMenu_Dispositivos.Visible = false;

            if (panel_SubMenu_Propiedades.Visible == true)
                panel_SubMenu_Propiedades.Visible = false;
        }

        private void ShowSubMenu(Panel subMenu)
        {
            if(subMenu.Visible == false)
            {
                HideSubMenu();
                subMenu.Visible = true;
            }
            else
                subMenu.Visible = false;
                
        }


        private void main (bool inicio)
        {
            captura = true;


            int result = 0;
            int index = 0;

            Form1 program = new Form1();

            // Retrieve singleton reference to system object
            ManagedSystem system = new ManagedSystem();

            // Retrieve list of cameras from the system
            ManagedCameraList camList = system.GetCameras();

            //IManagedCamera managedCamera = camList[0];

            foreach (IManagedCamera managedCamera in camList)
            {
                Console.WriteLine("Running example for camera {0}...", index);
                

                try
                {
                    if (inicio == true)
                    {
                        RunSingleCamera(managedCamera);
                    }
                    
                }
                catch (SpinnakerException ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    //result = -1;
                }

                Console.WriteLine("Camera {0} example complete...\n", index++);
            }



            //// Clear camera list before releasing system
            //camList.Clear();

            //// Release system
            //system.Dispose();

            //Console.WriteLine("\nDone! Press Enter to exit...");
            //Console.ReadLine();

        }





        // This function prints the device information of the camera from the
        // transport layer; please see NodeMapInfo_CSharp example for more
        // in-depth comments on printing device information from the nodemap.
        private int PrintDeviceInfo(INodeMap nodeMap)
        {
            int result = 0;

            try
            {

                ICategory category = nodeMap.GetNode<ICategory>("DeviceInformation");
                if (category != null && category.IsReadable)
                {
                    cb_Dispositivos.Items.Add(category.Children[5]);
                    cb_Dispositivos.Text = category.Children[5].ToString();
                                        
                }
                else
                {
                    cb_Dispositivos.Text = "Dispositivos no Encontrados";
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }


        private bool captura;
        private double relacion = 1.33333;
       
        void Act_Labels(string inf, string conf)
        {
            lbl_Inference.Text = inf;
            lbl_Confidence.Text = conf; 
        }


        void Show_Img(Bitmap img)
        {

            pb_video.Width = (int)(pb_video.Height * relacion);
            pb_video.Image = img;
            
            

        }


        // Actualizar Video en PictureBox
        public void Capturando(IManagedCamera cam)       // Evento que se ejecuta, por lo tanto recibe un sender
        {
           
            //for (int imageCnt = 0; imageCnt < NumImages; imageCnt++)
            while (captura == true)
            {

                try
                {
                    // Retrieve the next received images
                    using (IManagedImage rawImage = cam.GetNextImage(1000))
                    {

                        if (rawImage.IsIncomplete)
                        {
                            Console.WriteLine("Image incomplete with image status {0}...\n", rawImage.ImageStatus);
                        }
                        else
                        {
                            
                            uint width = rawImage.Width;
                            uint height = rawImage.Height;

                            
                            // Actualizar Labels
                            string inference = labelClassification[(int)rawImage.ChunkData.InferenceResult];
                            string confidence = Convert.ToString(Math.Round(rawImage.ChunkData.InferenceConfidence*100, 1))+"%";
                                       
                            this.Invoke(new Delegate_Act_Labels(Act_Labels), new object[] { inference, confidence });

                           // Actualizar Imagen
                            IManagedImage convertedImage = rawImage.Convert(PixelFormatEnums.Mono8);
                            
                            this.Invoke(new Delegate_show_img(Show_Img), new object[] { convertedImage.bitmap});


                            // Set Ganancia
                            cam.Gain.Value = Ganancia;

                            // Set Tiempod de exposición
                            cam.ExposureTime.Value = TiempoExp;


                        }
                    }
                }
                catch (SpinnakerException ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    //result = -1;
                }

                //lbl_Confidence.Text = confianza;

               
            }

            //cam.EndAcquisition();
        }

    


        // This function acquires and saves 10 images from a device; please see
        // Acquisition_CSharp example for more in-depth comments on the
        // acquisition of images.
        void AcquireImages(IManagedCamera cam, INodeMap nodeMap, INodeMap nodeMapTLDevice)
        {
            int result = 0;

            Console.WriteLine("\n*** IMAGE ACQUISITION ***\n");

            try
            {
                // Set acquisition mode to continuous
                IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
                if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (node retrieval). Aborting...\n");
                    //return -1;
                }

                IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
                if (iAcquisitionModeContinuous == null || !iAcquisitionMode.IsReadable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (entry retrieval). Aborting...\n");
                   // return -1;
                }

                
                iAcquisitionMode.Value = iAcquisitionModeContinuous.Symbolic;

                Console.WriteLine("Acquisition mode set to continuous...");

                // Begin acquiring images
                cam.BeginAcquisition();

                Console.WriteLine("Acquiring images...\n");

               

                StartTheThread(cam);
                


            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

           // return result;
        }


        public Thread StartTheThread(IManagedCamera cam) 
        { 
            var t = new Thread(() => Capturando(cam)); 
            t.Start(); 
            return t; 
        }



        // This function acts as the body of the example; please see
        // NodeMapInfo_CSharp example for more in-depth comments on setting up
        // cameras.
        void RunSingleCamera(IManagedCamera cam)
        {
            int result = 0;
            int err = 0;

            try
            {
                // Retrieve TL device nodemap and print device information
                INodeMap nodeMapTLDevice = cam.GetTLDeviceNodeMap();
                
                PrintDeviceInfo(nodeMapTLDevice);

                if (Iniciar == true)
                {

                    // Initialize camera
                    cam.Init();

                    // Retrieve GenICam nodemap
                    INodeMap nodeMap = cam.GetNodeMap();

                    // Get Ganancia
                    tb_Ganancia.Value = (int)cam.Gain.Value;
                    lbl_Ganancia.Text = Convert.ToString(cam.Gain.Value);

                    // Get Tiempo de eposición
                    tb_TiempoExp.Value = (int)cam.ExposureTime.Value;
                    lbl_TiempoExp.Text = Convert.ToString(cam.ExposureTime.Value);


                    AcquireImages(cam, nodeMap, nodeMapTLDevice);

                    // Deinitialize camera
                    cam.DeInit();

                }


                
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

           // return result;
        }

        private void btn_detener_Click(object sender, EventArgs e)
        {
            captura = false;
            btn_iniciar.Enabled = true;
            btn_detener.Enabled = false;

           
        }

        private void btn_Cerrar_Click(object sender, EventArgs e)
        {
            
            // Retrieve singleton reference to system object
            ManagedSystem system = new ManagedSystem();

            // Retrieve list of cameras from the system
            ManagedCameraList camList = system.GetCameras();

            // Clear camera list before releasing system
            camList.Clear();

            // Release system
            system.Dispose();


            Application.Exit();
        }

        private void btn_Dispositivos_Click(object sender, EventArgs e)
        {
            ShowSubMenu(panel_SubMenu_Dispositivos);
        }

        private void btn_Propiedades_Click(object sender, EventArgs e)
        {
            ShowSubMenu(panel_SubMenu_Propiedades);
        }

        private void btn_Maximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btn_Maximizar.Visible = false;
            btn_Restaurar.Visible = true;
        }

        private void btn_Minimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_Restaurar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            btn_Restaurar.Visible=false;
            btn_Maximizar.Visible = true;
        }

        private void BarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void label5_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void tb_Ganancia_Scroll(object sender, EventArgs e)
        {
            
            Ganancia = tb_Ganancia.Value;
            lbl_Ganancia.Text = Convert.ToString(Ganancia) + " bB";
        }

        private void tb_TiempoExp_Scroll(object sender, EventArgs e)
        {
            TiempoExp = tb_TiempoExp.Value;
            lbl_TiempoExp.Text = Convert.ToString(TiempoExp) + " ms";
        }
    }
}
