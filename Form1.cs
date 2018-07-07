

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.RegularExpressions;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.LudnMonoLibUsb;


namespace deneme1
{
    public partial class Form1 : Form
    {

      

        #region tanimlamalar




            public string ReceivedData;
            ArrayList UdpSendDataTemp = new ArrayList();
            public byte[][] UdpSendData1 = new byte[16][];
            public byte[] UdpReceivedData;

            int mouse_timer_tick1 = 1;




            const byte Chan_A = 1;
            const byte Chan_B = 2;
            const byte Chan_A_B = 3;


            const byte PaketAlindi = 0;
            const byte LedIslemi = 1;
            const byte IslemButton = 2;
            const byte BaglantiIslemi = 3;
            const byte AdcDataIslemi = 4;
            const byte SinyalYakalamaAyarlari = 5;
            const byte BootIslemi = 6;
            const byte BootBasla = 7;
            const byte AdcOrnekGonder = 8;
            const byte FrekansSec = 9;
            const byte AdcTestZaman = 10;
            const byte AdcGainSet = 11;
            const byte DataVar = 12;
            const byte channel_sec = 13;
            const byte channel_bit_gain = 14;
            const byte Fpga_read_test = 15;
            const byte Mustafa_kontrol = 16;



        public bool TekrarGonder = true, TekrarGonderme = false;

            Graphics g;
            Pen penR, penB, penY, penC, penB_Dot;
            SolidBrush BrushRed;
            SolidBrush BrushBlue;
            public Font font1;
            Bitmap flag = new Bitmap(1100, 600);

            string hedefAdi = "C:\\ultracq\\hedef.txt";
            string BootHedefAdi = "C:\\ultracq\\boot.txt";
            string BootKaynakAdi = "C:\\ultracq\\hedef.txt";

            double yanki_süresi;
            double malzeme_kalinligi = 0;

            int[] ses_hizi = new int[5] { 6320, 5918, 0, 0, 0 };
            int[] maximum_noktalar = new int[5];




            #endregion tanimlamalar

            #region form




            public Form1()
            {
                CheckForIllegalCrossThreadCalls = false;
                InitializeComponent();
                //timer3.Start();
                //this.Hide();
                

            }


            int SimdikiWidth = 1366;

            int SimdikiHeight = 768;


            private void Form1_Load(object sender, EventArgs e)

            {



            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;


            Rectangle ClientCozunurluk = new Rectangle();

                ClientCozunurluk = Screen.GetBounds(ClientCozunurluk);

                float OranWidth = ((float)ClientCozunurluk.Width / (float)SimdikiWidth);

                float OranHeight = ((float)ClientCozunurluk.Height / (float)SimdikiHeight);


                this.Scale(OranWidth, OranHeight);




            lnp_cb.Items.Add("3 dB");

            lnp_cb.Items.Add("12 dB");

            lnp_cb.Items.Add("18 dB");

            lnp_cb.Items.Add("22 dB");

            lnp_cb.Items.Add("PDL");

            lnp_cb.Items.Add("PDALL");

            lnp_cb.SelectedIndex = 0;

            channel_bit_gain_cb.SelectedIndex = 0;
                FrekansSec_cb.SelectedIndex = 0;
                Chan_set_cb.SelectedIndex = 2;


                font1 = new System.Drawing.Font("Arial", 10F, FontStyle.Bold);

                g = this.cizim_pb.CreateGraphics();
                penR = new Pen(Brushes.Red, 2);
                penB = new Pen(Brushes.Blue, 2);
                penB_Dot = new Pen(Brushes.Blue, 1);
                penB_Dot.DashStyle = DashStyle.Dot;
                penY = new Pen(Brushes.Yellow, 2);
                BrushRed = new SolidBrush(Color.Red);
                BrushBlue = new SolidBrush(Color.Blue);
                penC = new Pen(Brushes.Crimson, 2);


                Data_Paketleri_comb_itemleri_yaz();



                //panel1.BackgroundImage.Equals("C:\\ultracq\\panel0.bmp");
                CreateBackGroundBitmap();

                BootFileArrange();

            //FrekansSec_cb.SelectedIndex = 5;


                

                rectifier_cb.SelectedIndex = 3;

                Trigger_cb.SelectedIndex = 1;

                pulse_freq_comb.SelectedIndex = 3;

                Pulseoutputchannelcb.SelectedIndex = 0;

                Read_channel_sellect_cb.SelectedIndex = 0;

                Ultrasonik_yontem_cb.SelectedIndex = 1;


                malzeme_cb.SelectedIndex = 0;



                vga_gain_tb.Text = Convert.ToString(trackBar2.Value) + "dB";




                // the write test data.

                //SoapHexBinary shb;
                //try
                //{
                //    string[] a = File.ReadAllLines(hedefAdi);

                //    if (Data_Paketleri_comb.SelectedIndex == -1) shb = SoapHexBinary.Parse(a[a.Count() - 1]);
                //    else shb = SoapHexBinary.Parse(a[Data_Paketleri_comb.SelectedIndex]);
                //    UdpReceivedData = shb.Value;

                //}
                //catch
                //{
                //    MessageBox.Show("Kayıtlı data yok!");
                //}


            }

            public void setpreconditionvals()
            {

                Chan_set_cb.SelectedIndex = 1;
                channel_bit_gain_cb.SelectedIndex = 0;
                lnp_cb.SelectedIndex = 4;

            }

            #endregion form

            #region usb

            public static UsbDevice MyUsbDevice;
            public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1240, 83);
            byte[] readBuffer = new byte[512];

            public void USB_transfer(bool read, bool write)
            {

                ErrorCode ec = ErrorCode.None;
                try
                {

                    // Find and open the usb device.
                    MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                    // If the device is open and ready

                    if (MyUsbDevice == null || !btnbaglantı.Checked)
                    {


                        usbconnection_tb.ForeColor = System.Drawing.Color.Red;
                        usbconnection_tb.Text = "USB Bağlantısı Yok";

                    }
                    else
                    {
                        if (btnbaglantı.Checked)

                        {

                            usbconnection_tb.ForeColor = System.Drawing.Color.Green;
                            usbconnection_tb.Text = "USB Bağlandı.";

                        }

                    }

                    // open read endpoint 1.
                    UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                    // open write endpoint 1.
                    UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                    ErrorCode ecWrite;
                    ErrorCode ecRead;
                    int transferredOut;
                    int transferredIn;
                    UsbTransfer usbWriteTransfer;
                    UsbTransfer usbReadTransfer;


                    int adet = 0;
                    byte[] SendData = new byte[2];

                    adet = UdpSendDataTemp.Count;
                    SendData = new byte[adet + 4];

                    SendData[0] = 0xff;
                    SendData[1] = 16;
                    SendData[2] = Convert.ToByte(adet / 256);
                    SendData[3] = Convert.ToByte(adet % 256);


                string dosya_yolu = @"C:\\a\\metinbelgesi.txt";
                //İşlem yapacağımız dosyanın yolunu belirtiyoruz.
                FileStream fs = new FileStream(dosya_yolu, FileMode.OpenOrCreate, FileAccess.Write);
                //Bir file stream nesnesi oluşturuyoruz. 1.parametre dosya yolunu,
                //2.parametre dosya varsa açılacağını yoksa oluşturulacağını belirtir,
                //3.parametre dosyaya erişimin veri yazmak için olacağını gösterir.
                StreamWriter sw = new StreamWriter(fs);
                //Yazma işlemi için bir StreamWriter nesnesi oluşturduk.
                sw.WriteLine("Veri:");
                for (int i = 0; i < adet; i++) {
                     SendData[i + 4] = Convert.ToByte(UdpSendDataTemp[i]);
                    sw.WriteLine(Convert.ToByte(UdpSendDataTemp[i]));

                }
                //Dosyaya ekleyeceğimiz iki satırlık yazıyı WriteLine() metodu ile yazacağız.
                sw.Flush();
                //Veriyi tampon bölgeden dosyaya aktardık.
                sw.Close();
                fs.Close();

                adet += 4;

                    if (write == true)
                    {
                        ecWrite = writer.SubmitAsyncTransfer(SendData, 0, adet, 100, out usbWriteTransfer);
                        if (ecWrite != ErrorCode.None)
                        {

                            MessageBox.Show("Submit Async Write Failed.");
                        }

                        WaitHandle.WaitAll(new WaitHandle[] { usbWriteTransfer.AsyncWaitHandle }, 20, false);
                        if (!usbWriteTransfer.IsCompleted) usbWriteTransfer.Cancel();
                        ecWrite = usbWriteTransfer.Wait(out transferredOut);
                        usbWriteTransfer.Dispose();
                        //MessageBox.Show(transferredOut.ToString(), ecWrite.ToString());
                    }

                    // Create and submit transfer




                    if (read == true)
                    {
                        ecRead = reader.SubmitAsyncTransfer(readBuffer, 0, readBuffer.Length, 200, out usbReadTransfer);

                        if (ecRead != ErrorCode.None) MessageBox.Show("Submit Async Read Failed.");

                        WaitHandle.WaitAll(new WaitHandle[] { usbReadTransfer.AsyncWaitHandle }, 200, false);
                        if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();
                        ecRead = usbReadTransfer.Wait(out transferredIn);
                        usbReadTransfer.Dispose();

                        int dataadet = readBuffer[1] * 256 + readBuffer[2] + 3;
                        UdpReceivedData = new byte[dataadet + 100];

                        if (dataadet < 512) Array.Copy(readBuffer, UdpReceivedData, dataadet);

                        if (readBuffer[3] == 0x20)
                        {
                            int packetsayisi = readBuffer[4];
                            int temppacketsaiyisi = 0;
                            UdpReceivedData = new byte[dataadet + 500];

                            do
                            {
                                ecRead = reader.SubmitAsyncTransfer(readBuffer, 0, readBuffer.Length, 200, out usbReadTransfer);

                                if (ecRead != ErrorCode.None) MessageBox.Show("Submit Async Read Failed.");

                                WaitHandle.WaitAll(new WaitHandle[] { usbReadTransfer.AsyncWaitHandle }, 200, false);
                                if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();
                                ecRead = usbReadTransfer.Wait(out transferredIn);
                                usbReadTransfer.Dispose();

                                Array.Copy(readBuffer, 0, UdpReceivedData, temppacketsaiyisi * 500, 500);
                                temppacketsaiyisi++;
                            }
                            while (temppacketsaiyisi < packetsayisi + 1);

                        }




                        islemler();


                    }



                    //usbtxt.Text += ("Data  :" + Encoding.Default.GetString(readBuffer, 0, transferredIn) + "\r\n");

                    //Console.WriteLine("Read  :{0} ErrorCode:{1}", transferredIn, ecRead);



                    // MessageBox.Show(transferredOut.ToString(), ecWrite.ToString());

                    //Console.WriteLine("\r\nDone!\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                    UdpSendDataTemp.Clear();
                    if (MyUsbDevice != null)
                    {
                        if (MyUsbDevice.IsOpen)
                        {
                            MyUsbDevice.Close();
                        }
                        MyUsbDevice = null;

                        // Free usb resources
                        UsbDevice.Exit();
                    }
                }
            }

            #endregion

            #region yardimci_fonksiyonlar


            private void Data_Paketleri_comb_itemleri_yaz()
            {
                Data_Paketleri_comb.Items.Clear();
                Data_Paketleri_comb.Text = "";
                int a = File.ReadAllLines(@hedefAdi).Count();
                for (int i = 1; i <= a; i++)
                {
                    Data_Paketleri_comb.Items.Add(i);
                }
            }

            public string LocalIPAddress()
            {
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                return localIP;
            }

            private void ShowMessageMethod(string message)
            {
                string a = message;


            }



            private void KonuEkle(byte konu) { UdpSendDataTemp.Add(konu); }

            private void DataEkle(byte data) { UdpSendDataTemp.Add(data); }



        #endregion yardimci_fonksiyonlar

        #region AlinanDataIslemleri
        int durum_enkoder = 0;
            void islemler()
            {
                switch (UdpReceivedData[3])
                {

                    case IslemButton:

                        if (UdpReceivedData[4] % 2 == 0) { DskButon0.Text = "On"; DskButon0.BackColor = Color.Green; } else { DskButon0.Text = "Off"; DskButon0.BackColor = Color.Red; }
                        if ((UdpReceivedData[4] >> 1) % 2 == 0) { DskButon1.Text = "On"; DskButon1.BackColor = Color.Green; } else { DskButon1.Text = "Off"; DskButon1.BackColor = Color.Red; }
                        if ((UdpReceivedData[4] >> 2) % 2 == 0) { DskButon2.Text = "On"; DskButon2.BackColor = Color.Green; } else { DskButon2.Text = "Off"; DskButon2.BackColor = Color.Red; }
                        if ((UdpReceivedData[4] >> 3) % 2 == 0) { DskButon3.Text = "On"; DskButon3.BackColor = Color.Green; } else { DskButon3.Text = "Off"; DskButon3.BackColor = Color.Red; }

                        break;

                    case AdcDataIslemi:


                        SoapHexBinary shb = new SoapHexBinary(UdpReceivedData);
                        using (StreamWriter writers = new StreamWriter(hedefAdi, true))
                            writers.WriteLine(shb.ToString());
                        Data_Paketleri_comb.Items.Add(File.ReadAllLines(@hedefAdi).Count());

                        if (SurekliAnaliz_cb.Checked) { PreparePage(false, true); }
                    if (checkBox1.Checked && durum_enkoder == 1) {
                        durum_enkoder = 0;
                        string idosya_yolu = @"C:\\ultracq\\veriler.txt";
                        StreamWriter iekle = File.AppendText(idosya_yolu);
                        iekle.WriteLine("ADC:" + shb.ToString());
                        iekle.Close();
                    }

                    break;

                    case BootIslemi:

                        Boot_Handle(UdpReceivedData[4] * 256 + UdpReceivedData[5] + 1);

                        break;

                    case FrekansSec:
                        if (readBuffer[4] == 1) frqDurum_lb.Text = FrekansSec_cb.SelectedItem.ToString();
                        durum_tb.Text = "Frekans Seçildi";


                        break;

                    case channel_sec:

                        if (readBuffer[4] == 1) durum_tb.Text = "Kanal Seçildi";

                        break;

                    case BootBasla:

                        //BootBitti_txt.Text = ("Yükleme Tamamlandi");
                        break;

                    case SinyalYakalamaAyarlari:

                        if (readBuffer[4] == 1)
                        {
                            durum_tb.Text = "Sinyal Ayarları Gönderildi";
                            durum_tb.ForeColor = Color.Green;
                        }

                        break;

                    case AdcGainSet:

                        if (readBuffer[4] == 1)
                        {
                            durum_tb.Text = "Gain Değeri Gönderildi";
                        vga_gain_tb.Text = Convert.ToString(trackBar2.Value + lnpgain[lnp_cb.SelectedIndex] + channel_bit_gain_cb.SelectedIndex * 6) + "dB";
                            durum_tb.ForeColor = Color.Green;
                        }

                        break;

                    case channel_bit_gain:

                        if (readBuffer[4] == 1)
                        {
                            durum_tb.Text = "kanal bit kazancı seçildi";
                            vga_gain_tb.Text = Convert.ToString(trackBar2.Value + lnpgain[lnp_cb.SelectedIndex] + channel_bit_gain_cb.SelectedIndex * 6) + "dB";
                            durum_tb.ForeColor = Color.Green;
                        }
                        break;

                    case DataVar:


                        break;

                case Mustafa_kontrol:
                    if (UdpReceivedData[10] != 'M') return;
                    int x = UdpReceivedData[4] + 256 * (int)UdpReceivedData[5];
                    int y = UdpReceivedData[6] + 256 * (int)UdpReceivedData[7];
                    int z = UdpReceivedData[8] + 256 * (int)UdpReceivedData[9];
                    label32.Text = "X = " + Convert.ToString(x);
                    label33.Text = "Y = " + Convert.ToString(y);
                    label34.Text = "Z = " + Convert.ToString(z);

                    string dosya_yolu = @"C:\\ultracq\\veriler.txt";
                    StreamWriter ekle = File.AppendText(dosya_yolu);
                    ekle.WriteLine("X:" + Convert.ToString(x) + ";Y:" + Convert.ToString(y) + ";Z:" + Convert.ToString(z) + ";");
                    ekle.Close();

                    if (checkBox1.Checked) {
                        timer2.Start();
                        durum_enkoder = 1;
                    }
                    break;
                    default: break;
                }
            }

            #endregion AlinanDataIslemleri

            #region GonderilenDataIslemleri

            public void baglanti_kontrol()
            {
                KonuEkle(BaglantiIslemi);

                DataEkle(0);

                //UdpPaketGonder(true,TekrarGonderme);
            }

            #region sinyalayarları


            private void OrneklemeAyarlari_btn_Click(object sender, EventArgs e)
            {
                try
                {

                    if (Convert.ToInt32(KapiBaslangic_tb.Text) >= Convert.ToInt32(Kapi_bitis_tb.Text))
                    {
                        MessageBox.Show("Başlangıç Bitişten Büyük veya Eşit Olamaz!");
                    }
                    else
                    {
                        KonuEkle(SinyalYakalamaAyarlari);

                        //0   buffersize settings          2 byte
                        //2   kapılama başlangıç settings  2 byte
                        //4   kapılama bitiş settings      2 byte
                        //6   row data settings            1 byte
                        //7   rectifier settings           1 byte
                        //8   trigger settings             1 byte
                        //9   clock settings               1 byte
                        //10  chan settings                1 byte
                        //11  zarf katsayisi               1 byte
                        //12  PRF                          1 byte
                        //13  Filtre                       1 byte
                        //14  lnp Gain                     1 byte
                        //15  Vga Gain                     1 byte
                        //16  darbe freaknsı               1 byte
                        //17  darbe sayısı                 1 byte
                        //18  transducer tipi              1 byte
                        //19  hv genlik                    1 byte
                        //20  read_channel_sel             1 byte
                        //21  channel bit gain             1 byte
                        //21  ultraonik yöntem             1 byte

                        DataEkle(120);
                        DataEkle(Convert.ToByte(Convert.ToInt32(OrnekBoyutu_tb.Text) % 256));
                        DataEkle(Convert.ToByte(Convert.ToInt32(OrnekBoyutu_tb.Text) / 256));

                        DataEkle(Convert.ToByte(Convert.ToInt32(KapiBaslangic_tb.Text) % 256));
                        DataEkle(Convert.ToByte(Convert.ToInt32(KapiBaslangic_tb.Text) / 256));

                        DataEkle(Convert.ToByte(Convert.ToInt32(Kapi_bitis_tb.Text) % 256));
                        DataEkle(Convert.ToByte(Convert.ToInt32(Kapi_bitis_tb.Text) / 256));

                        if (RowData_cb.Checked == true) DataEkle(1); else DataEkle(0);

                        DataEkle(Convert.ToByte(rectifier_cb.SelectedIndex)); // 0-->full, 1-->negative, 2-->positive

                        DataEkle(Convert.ToByte(Trigger_cb.SelectedIndex)); // 0-->Ext, 1-->int

                        DataEkle(Convert.ToByte(FrekansSec_cb.SelectedIndex)); // 0-->125, 1-->62.5,2-->31...

                        DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex)); //  0-->A&B, 1-->A, 2-->B

                        DataEkle(Convert.ToByte(Convert.ToInt32(zarf_k_tb.Text)));// zarfın çözünürlüğünü ifade eder  buffersize/zarf katsayısı çözünürlüğü verir

                        DataEkle(Convert.ToByte(Convert.ToInt32(prf_tb.Text)));


                        if (fir_cb.Checked == true) DataEkle(1); else DataEkle(0);

                        DataEkle(Convert.ToByte(trackBar1.Value)); // trig

                        DataEkle(Convert.ToByte(lnp_cb.SelectedIndex));

                        DataEkle(Convert.ToByte(trackBar2.Value)); // vga

                        DataEkle(Convert.ToByte(pulse_freq_comb.SelectedIndex));

                        DataEkle(Convert.ToByte(Convert.ToInt32(darbe_sayisi.Text)));

                        DataEkle(Convert.ToByte(Pulseoutputchannelcb.SelectedIndex));

                        DataEkle(Convert.ToByte(Convert.ToInt32(hv_txt_bx.Text)));

                        DataEkle(Convert.ToByte(Read_channel_sellect_cb.SelectedIndex));

                        DataEkle(Convert.ToByte(channel_bit_gain_cb.SelectedIndex));

                        DataEkle(Convert.ToByte(Ultrasonik_yontem_cb.SelectedIndex));

                        USB_transfer(true, true);

                    }
                }

                catch
                {
                    MessageBox.Show("Lütfen Değer Giriniz.");
                }
            }

            #endregion


            Stopwatch stopwatch1 = new Stopwatch();
            private void AdcOrnekle_btn_Click(object sender, EventArgs e)
            {

                stopwatch1.Restart();
                KonuEkle(AdcOrnekGonder);
                DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex));
                if (int_trigg_cb.Checked) DataEkle(1);
                else DataEkle(0);
                USB_transfer(true, true);

                stopwatch1.Stop();
                label22.Text = ("stopwatch= " + stopwatch1.Elapsed.ToString());



            }

            private void FrekansSec_cb_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)
                {
                    KonuEkle(FrekansSec);
                    DataEkle(Convert.ToByte(FrekansSec_cb.SelectedIndex));
                    USB_transfer(true, true);

                }
            }

            private void channel_bit_gain_cb_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)
                {

                    KonuEkle(channel_bit_gain);
                    DataEkle(Convert.ToByte(channel_bit_gain_cb.SelectedIndex));
                    USB_transfer(true, true);

                }

            }
            private void Chan_set_cb_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)
                {
                    KonuEkle(channel_sec);
                    DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex));
                    USB_transfer(true, true);
                }
            }
            int[] lnpgain = new int[6] { 3, 12, 18, 22, 0, 0 };


        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (btnbaglantı.Checked)
            {

                if (mouse_timer_tick1 == 1)
                {
                    mouse_timer_tick1 = 0;
                    mouse_timer.Start();

                    //vga_gain_tb.Text = Convert.ToString(trackBar2.Value + lnpgain[lnp_cb.SelectedIndex]) + "dB";

                    KonuEkle(AdcGainSet);
                    DataEkle(Convert.ToByte(trackBar2.Value));
                    DataEkle(Convert.ToByte(lnp_cb.SelectedIndex));

                    USB_transfer(true, true);
                }
            }
        }
            #region led

            private void LedChange()
            {
                if (btnbaglantı.Checked)
                {
                    int led1 = 0;

                    if (Led1CBox.Checked == true) { led1 |= 1; } else { led1 &= ~1; }
                    if (Led2CBox.Checked == true) { led1 |= 2; } else { led1 &= ~2; }
                    if (Led3CBox.Checked == true) { led1 |= 4; } else { led1 &= ~4; }
                    if (Led4CBox.Checked == true) { led1 |= 8; } else { led1 &= ~8; }

                    KonuEkle(LedIslemi);
                    DataEkle(Convert.ToByte(led1));

                    USB_transfer(false, true);
                }
            }

            private void Led1CBox_CheckedChanged(object sender, EventArgs e) { LedChange(); }
            private void Led2CBox_CheckedChanged(object sender, EventArgs e) { LedChange(); }
            private void Led3CBox_CheckedChanged(object sender, EventArgs e) { LedChange(); }
            private void Led4CBox_CheckedChanged(object sender, EventArgs e) { LedChange(); }
            #endregion led

            private void AdcZaman_cb_CheckedChanged(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)
                {

                    if (AdcZaman_cb.Checked)
                    {

                        timer1.Start();
                    }
                    else
                    {
                        timer1.Stop();

                    }
                }
            }
            #endregion GonderilenDataIslemleri

            #region çizim

            PictureBox pictureBox1 = new PictureBox();

            public void CreateBackGroundBitmap()
            {


                Graphics flagGraphics = Graphics.FromImage(flag);


                flagGraphics.Clear(Color.LightGray);
                int Widthstart = 40, HeightStart = 40;

                int WidthEnd = 1040;
                int HeightEnd = 552;

                //    Point0y            Pointxy
                //       ------------------
                //       -                -
                //       -                -
                //       -                -
                //       -                -
                //       -                -
                //       -                -
                //       ------------------
                //    Point00            Point0x

                System.Drawing.Point Point00 = new System.Drawing.Point(Widthstart, HeightEnd);
                System.Drawing.Point Point0x = new System.Drawing.Point(WidthEnd, HeightEnd);
                System.Drawing.Point Pointy0 = new System.Drawing.Point(Widthstart, HeightStart);
                System.Drawing.Point Pointyx = new System.Drawing.Point(WidthEnd, HeightStart);

                System.Drawing.Point Pt_XStr = new System.Drawing.Point(Pointy0.X - 20, Pointy0.Y - 30);      // genlik
                System.Drawing.Point Pt_YStr = new System.Drawing.Point(Point0x.X - 50, Point0x.Y + 30);    //zaman yazma noktası

                System.Drawing.Point Pt_XInt = new System.Drawing.Point(Point00.X - 5, Point00.Y + 10);
                System.Drawing.Point Pt_YInt = new System.Drawing.Point(Point00.X - 30, Point00.Y - 10);

                System.Drawing.Point PointTemp;

                int[] saha = new int[13] { 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };

                PointTemp = Pointy0;
                PointTemp.X -= 10; PointTemp.Y += 10;

                flagGraphics.DrawLine(penB, Point00, Point0x);
                flagGraphics.DrawLine(penB, Pointy0, PointTemp);
                PointTemp.X += 20;
                flagGraphics.DrawLine(penB, Pointy0, PointTemp);



                flagGraphics.DrawLine(penB, Point00, Pointy0);

                PointTemp = Point0x;
                PointTemp.X -= 10; PointTemp.Y -= 10;

                flagGraphics.DrawLine(penB, Point0x, PointTemp);
                PointTemp.Y += 20;
                flagGraphics.DrawLine(penB, Point0x, PointTemp);



                string y_axis_name;

                int adim;
                int adimUzunlugu;


                if (RowData_cb.Checked == true)
                {
                    y_axis_name = "GENLİK";
                    adim = 16;
                    adimUzunlugu = (HeightEnd - HeightStart) / adim;
                    flagGraphics.DrawString(y_axis_name, font1, BrushRed, Pt_XStr);

                    for (int i = 0; i <= adim; i++)
                    {
                        PointTemp = Pt_YInt;
                        PointTemp.Y -= (adimUzunlugu * i);
                        flagGraphics.DrawString((i * 16).ToString(), font1, BrushRed, PointTemp);
                        flagGraphics.DrawLine(penB_Dot, Widthstart, HeightEnd - (adimUzunlugu * i), WidthEnd, HeightEnd - (adimUzunlugu * i));
                    }
                }
                else
                {
                    y_axis_name = "GENLİK";
                    adim = 16;
                    adimUzunlugu = (HeightEnd - HeightStart) / adim;
                    flagGraphics.DrawString(y_axis_name, font1, BrushRed, Pt_XStr);

                    for (int i = 0; i <= adim; i++)
                    {
                        PointTemp = Pt_YInt;
                        PointTemp.Y -= (adimUzunlugu * i);
                        flagGraphics.DrawString((i * 16).ToString(), font1, BrushRed, PointTemp);
                        flagGraphics.DrawLine(penB_Dot, Widthstart, HeightEnd - (adimUzunlugu * i), WidthEnd, HeightEnd - (adimUzunlugu * i));
                    }

                }


                if (hScrollBar1.Value >= 7) flagGraphics.DrawString("ZAMAN (uS)", font1, BrushRed, WidthEnd - 75, HeightEnd + 30);
                else flagGraphics.DrawString("ZAMAN (nS)", font1, BrushRed, WidthEnd - 75, HeightEnd + 30);

                for (int i = 1; i <= 10; i++) //sürelerin değerleri yazılır
                {
                    PointTemp = Pt_XInt;
                    PointTemp.X += i * 100;

                    if (hScrollBar1.Value >= 7) flagGraphics.DrawString((i * saha[hScrollBar1.Value] / 1000).ToString(), font1, BrushRed, PointTemp);
                    else flagGraphics.DrawString((i * saha[hScrollBar1.Value]).ToString(), font1, BrushRed, PointTemp);
                    flagGraphics.DrawLine(penB_Dot, Widthstart + i * 100, HeightEnd, Widthstart + i * 100, HeightStart);

                }




                cizim_pb.BackgroundImage = flag;

                switch (hScrollBar1.Value)
                {
                    case 0: flag.Save("C:\\ultracq\\panel0.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 1: flag.Save("C:\\ultracq\\panel1.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 2: flag.Save("C:\\ultracq\\panel2.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 3: flag.Save("C:\\ultracq\\panel3.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 4: flag.Save("C:\\ultracq\\panel4.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 5: flag.Save("C:\\ultracq\\panel5.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 6: flag.Save("C:\\ultracq\\panel6.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 7: flag.Save("C:\\ultracq\\panel7.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 8: flag.Save("C:\\ultracq\\panel8.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 9: flag.Save("C:\\ultracq\\panel9.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 10: flag.Save("C:\\ultracq\\panel10.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 11: flag.Save("C:\\ultracq\\panel11.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 12: flag.Save("C:\\ultracq\\panel12.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;
                    case 13: flag.Save("C:\\ultracq\\panel13.bmp", System.Drawing.Imaging.ImageFormat.Bmp); break;



                }

            }

            private void Data_Paketleri_comb_SelectedIndexChanged(object sender, EventArgs e)
            {
                stopwatch1.Restart();
                string[] a = File.ReadAllLines(hedefAdi);
                SoapHexBinary shb = SoapHexBinary.Parse(a[Data_Paketleri_comb.SelectedIndex]);
                UdpReceivedData = shb.Value;

                PreparePage(false, true);

            }

            public void PreparePage(bool sinus, bool SigalStartSet)
            {
                try
                {
                    //stopwatch1.Restart();
                    hScrollBar2.Maximum = UdpReceivedData[1] * 256 + UdpReceivedData[2] - 55;
                    hScrollBar2.Minimum = 0;
                    cizim_pb.Refresh();

                    int Widthstart = 40, HeightStart = 40;

                    int WidthEnd = 1040;
                    int HeightEnd = 552;

                    System.Drawing.Point[] points;

                    //    Point0y            Pointxy
                    //       ------------------
                    //       -                -
                    //       -                -
                    //       -                -
                    //       -                -
                    //       -                -
                    //       -                -
                    //       ------------------
                    //    Point00            Point0x

                    System.Drawing.Point Point00 = new System.Drawing.Point(Widthstart, HeightEnd);
                    System.Drawing.Point Point0x = new System.Drawing.Point(WidthEnd, HeightEnd);
                    System.Drawing.Point Pointy0 = new System.Drawing.Point(Widthstart, HeightStart);
                    System.Drawing.Point Pointyx = new System.Drawing.Point(WidthEnd, HeightStart);




                    int[] saha = new int[13] { 5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000 };


                    int ornekSuresi = 1 << UdpReceivedData[14];

                    double pikselAdimi = (double)ornekSuresi * 10 * 10 / saha[hScrollBar1.Value]; //10 ile çarparak nS yi belirtiyoruz 100MHZ için 10, 50mhz için 20 ns piksel adımı oluşuyor

                    int OrnekBaslangic = 0;
                    int OrnekBitis = UdpReceivedData[6] * 256 + UdpReceivedData[5];
                    int OrnekBoyutu = OrnekBitis - OrnekBaslangic;
                    int Ornekfrekansi = UdpReceivedData[14];

                    ornek_adimi_tb.Text = "Örnek Freq= " + FrekansSec_cb.Items[UdpReceivedData[14]].ToString() + "MHz" + "   Örnek Adımı= " + (ornekSuresi * 10).ToString() + "nS";
                    PaketUzunlugu_tb.Text = (OrnekBoyutu * ornekSuresi * 10).ToString() + "nS";
                    sinyal_baslangic_txt.Text = "       " + (ornekSuresi * 10 * hScrollBar2.Value).ToString() + "nS";


                    points = new System.Drawing.Point[16000];
                    int tempCount = 0;
                    int j = 0;
                    int z = 0;

                    int ofset = 50 + hScrollBar2.Value;
                    int sa;



                    int max_val = 0;
                    int max_counter = 1;
                    int paket_counter = 0;
                    int max_trigger_level = 144;




                    for (int i = OrnekBaslangic; i < OrnekBitis - hScrollBar2.Value; i++)
                    {
                        sa = UdpReceivedData[i + ofset];


                        if (rectifier_cb.SelectedIndex == 0)//full wave rectifier
                        {

                            sa = sa - 128;
                            if (sa < 0) sa = sa * (-1);

                            sa = sa + 128;
                            //max_trigger_level = 32;
                        }

                        else if (rectifier_cb.SelectedIndex == 1)
                        {
                            sa = sa - 128;
                            if (sa < 0) sa = sa * (-1);
                            else sa = 0;

                            sa = sa + 128;

                            // max_trigger_level = 32;
                        }
                        else if (rectifier_cb.SelectedIndex == 2)
                        {
                            sa = sa - 128;
                            if (sa < 0) sa = 0;

                            sa = sa + 128;
                            //max_trigger_level = 32;
                        }


                        points[i].X = (int)(Point00.X + (i * pikselAdimi));
                        points[i].Y = Point00.Y - sa * 2;


                        if (DegerGoster_cb.Checked == true)
                        {
                            g.DrawString(sa.ToString("X2"), font1, BrushRed, points[i].X - 10, points[i].Y - 25);
                            g.DrawRectangle(penR, points[i].X - 1, points[i].Y - 1, 2, 2);
                        }
                        maximum_noktalar[0] = 0;
                        if (kalınlıkolcum_cb.Checked == true)
                        {
                            if (max_counter < 4)
                            {
                                if ((sa > max_val) && (sa > max_trigger_level))
                                {
                                    if ((i > 200) || (hScrollBar2.Value > 200))
                                        max_val = sa;
                                    maximum_noktalar[max_counter] = i;

                                    paket_counter = 0;
                                }

                                paket_counter++;

                                if (paket_counter > Convert.ToInt32(kapitxtbox.Text))
                                {

                                    if (max_val > max_trigger_level)
                                    {
                                        g.DrawRectangle(penB, points[maximum_noktalar[max_counter]].X - 4, points[maximum_noktalar[max_counter]].Y - 4, 8, 8);
                                        //g.DrawRectangle(penB, points[maximum_noktalar[max_counter]].X - 1, 500, 50, 50);
                                        paket_counter = 0;
                                        max_counter++;
                                        max_val = 0;
                                    }
                                    else paket_counter = 0;
                                }
                            }
                        }


                        if (degernum_cb.Checked == true)
                        {
                            if (j % 20 == 0)
                            {
                                g.DrawString(j.ToString(), font1, BrushBlue, points[i].X - 10, 500 - z * 8);
                                g.DrawRectangle(penB, points[i].X - 1, 500, 2, 2);
                                z++;
                            }
                            j++;
                        }


                        tempCount = i;
                        if (points[i].X > (Point0x.X))
                            break;


                    }


                    g.DrawString(tempCount.ToString(), font1, BrushRed, points[tempCount].X - 20, points[tempCount].Y - 35);


                    Array.Resize(ref points, tempCount);


                    //label22.Text = "Görünen data= " + (tempCount).ToString();

                    g.DrawLines(penC, points);


                    darbe_freq_lbl.Text = "Darbe Freq:" + pulse_freq_comb.Items[UdpReceivedData[22]];

                    if (kalınlıkolcum_cb.Checked == true)
                    {

                        if (hScrollBar2.Value < 100)
                        {

                            yanki_süresi = (maximum_noktalar[1]) * ornekSuresi * 5;
                            //yanki_süresi = (maximum_noktalar[1] - maximum_noktalar[0]) * 5;
                            malzeme_kalinligi = ((yanki_süresi - Convert.ToDouble(delay_distance_tb.Text)) * ses_hizi[malzeme_cb.SelectedIndex]) / 1000000;//mm cinsinden kalınlık
                                                                                                                                                           //malzeme_kalinligi -= Convert.ToDouble(delay_distance_tb.Text);
                            Kalinlik_tbx.Text = malzeme_kalinligi.ToString();
                            //SonucKalinlik_txt.Text = "Kalınlık= " + malzeme_kalinligi.ToString() + " mm";

                        }
                        else
                        {
                            if (maximum_noktalar[2] > maximum_noktalar[1])
                            {

                                yanki_süresi = (maximum_noktalar[2] - maximum_noktalar[1]) * ornekSuresi * 5;
                                malzeme_kalinligi = (yanki_süresi * ses_hizi[malzeme_cb.SelectedIndex]) / 1000000;//mm cinsinden kalınlık
                                Kalinlik_tbx.Text = malzeme_kalinligi.ToString();
                                //SonucKalinlik_txt.Text = "Kalınlık= " + malzeme_kalinligi.ToString() + " mm";
                            }
                        }


                    }

                    kalinlikadimi_lb.Text = ((yanki_süresi / ornekSuresi) / 5).ToString();

                    stopwatch1.Stop();
                    label22.Text = ("stopwatch= " + stopwatch1.Elapsed.ToString());



                }
                catch
                {
                    MessageBox.Show("Data yok.");
                }
            }

            #endregion

            #region boot

            private void Boot_Handle(int BootPaketSirasi)
            {
                string[] a = File.ReadAllLines(BootHedefAdi);

                if (BootPaketSirasi < a.Count())
                {
                    SoapHexBinary shb = SoapHexBinary.Parse(a[BootPaketSirasi]);
                    byte[] c = shb.Value;

                    for (int i = 0; i < shb.Value.Count(); i++)
                    {
                        UdpSendDataTemp.Add(shb.Value[i]);
                    }

                    //BootGidenPaket_txt.Text = BootPaketSirasi.ToString() + ("/") + a.Count().ToString();

                    // UdpPaketGonder(false, false);

                }
                else
                {
                    //BootBitti_txt.Text = ("Gönderme Tamamlandi");
                    UdpSendDataTemp.Add(7);
                    //UdpPaketGonder(true, false);

                }

            }


            private void BootFileArrange()
            {
                /* try
                 {
                     string[] BootDatasi = File.ReadAllLines(BootKaynakAdi);
                     string[] BootDatasi2 = new string[2000];

                     if (BootDatasi[0] != "ok")
                     {
                         int a = BootDatasi.Count();
                         int j = 1;

                         BootDatasi2[0] = "ok";
                         BootDatasi2[j] = BootIslemi.ToString("X2") + j.ToString("X4") + "000000";

                         for (int i = 0; i < a - 1; i++)
                         {



                             if (BootDatasi[i][0] != '@')
                             {
                                 if (BootDatasi2[j].Length > 500)
                                 {
                                     j++;
                                     BootDatasi2[j] = BootIslemi.ToString("X2") + j.ToString("X4") + "000000"; ;
                                 }

                                 SoapHexBinary shb = SoapHexBinary.Parse(BootDatasi[i]);
                                 BootDatasi2[j] += shb.ToString();
                             }
                             else
                             {
                                 j++;
                                 int n = Int32.Parse(BootDatasi[i].Remove(0, 1), System.Globalization.NumberStyles.HexNumber);

                                 BootDatasi2[j] = BootIslemi.ToString("X2") + j.ToString("X4") +n.ToString("X6");

                             }


                         }

                         File.WriteAllText(BootHedefAdi, String.Empty);
                         using (StreamWriter writers = new StreamWriter(BootHedefAdi, true))
                             for (int i = 0; i <= j; i++)
                                 writers.WriteLine(BootDatasi2[i]);
                     }
                 }
                 catch (FileNotFoundException)
                 {
                     MessageBox.Show("dosya .txt olmalıdır");
                 }
                 */
            }


            private void btn_boot_Click(object sender, EventArgs e)
            {

                Boot_Handle(1);
            }

            #endregion boot

            #region çizim düzenleme

            private void tabPage4_Click(object sender, EventArgs e)
            {
                // UdpReceivedData = sine_wave;
                // PreparePage(true,false);
            }

            private void hScrollBar1_Scroll(object sender, EventArgs e)
            {
                CreateBackGroundBitmap();
                //panel1.BackgroundImage.Equals("C:\\ultracq\\panel0.bmp");

                SoapHexBinary shb;
                try
                {
                    if ((UdpReceivedData == null) || (UdpReceivedData.Count() < 20))
                    {
                        string[] a = File.ReadAllLines(hedefAdi);

                        if (Data_Paketleri_comb.SelectedIndex == -1) shb = SoapHexBinary.Parse(a[a.Count() - 1]);
                        else shb = SoapHexBinary.Parse(a[Data_Paketleri_comb.SelectedIndex]);
                        UdpReceivedData = shb.Value;
                    }
                }
                catch
                {
                    MessageBox.Show("Kayıtlı data yok!");
                }
                if (GrafikTest_cb.Checked) PreparePage(true, false);
                else PreparePage(false, false);
            }

            private void hScrollBar2_Scroll(object sender, EventArgs e)
            {
                SoapHexBinary shb;
                try
                {
                    if (UdpReceivedData == null)
                    {
                        string[] a = File.ReadAllLines(hedefAdi);

                        if (Data_Paketleri_comb.SelectedIndex == -1) shb = SoapHexBinary.Parse(a[a.Count() - 1]);
                        else shb = SoapHexBinary.Parse(a[Data_Paketleri_comb.SelectedIndex]);
                        UdpReceivedData = shb.Value;
                    }

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (GrafikTest_cb.Checked) PreparePage(true, false);
                    else PreparePage(false, false);

                    stopwatch.Stop();
                    Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);



                }
                catch
                {
                    MessageBox.Show("Kayıtlı data yok!");
                }

            }





            //private void tabPage4_MouseClick(object sender, MouseEventArgs e)
            //{    
            //    PreparePage(true);
            //    SoapHexBinary shb = new SoapHexBinary(sine_wave);

            //    using (StreamWriter writers = new StreamWriter(hedefAdi, true))
            //    writers.WriteLine(shb.ToString());
            //    Data_Paketleri_comb.Items.Add(File.ReadAllLines(@hedefAdi).Count());               
            //}


            private void HepsiniSil_btn_Click(object sender, EventArgs e)
            {
                File.WriteAllText(hedefAdi, String.Empty);
                Data_Paketleri_comb_itemleri_yaz();
            }

            private void OrnekSil_btn_Click(object sender, EventArgs e)
            {
                try
                {
                    int tempitem = Data_Paketleri_comb.SelectedIndex;
                    string[] a = File.ReadAllLines(hedefAdi);
                    a = a.Where(w => w != a[Data_Paketleri_comb.SelectedIndex]).ToArray();
                    File.WriteAllText(hedefAdi, String.Empty);
                    File.WriteAllLines(hedefAdi, a);
                    Data_Paketleri_comb_itemleri_yaz();
                    Data_Paketleri_comb.SelectedIndex = tempitem - 1;
                }
                catch { }

            }

            #endregion



            private void usbconnection_tb_Click(object sender, EventArgs e)
            {

            }

            private void btnbaglantı_CheckedChanged_1(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)

                    AdcZaman_cb.Enabled = true;
                ////
                else
                {
                    AdcZaman_cb.Enabled = false;
                    AdcZaman_cb.Checked = false;
                    timer1.Stop();
                }
                USB_transfer(false, false);
            }

        private void malzeme_cb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            KonuEkle(16);
            DataEkle(Convert.ToByte(0));
            USB_transfer(true, true); // read, write
        }

        private void DskButon0_Click(object sender, EventArgs e)
        {

        }

        private void buton0_Click(object sender, EventArgs e)
        {
            KonuEkle(IslemButton);
            DataEkle(Convert.ToByte(0));
            USB_transfer(true, true); // read, write
        }

        private void pulse_freq_comb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            KonuEkle(16);
            DataEkle(72); // H
            USB_transfer(true, true); // read, write
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            KonuEkle(AdcOrnekGonder);
            DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex));
            USB_transfer(true, true);
            timer2.Stop();
        }

        private void label32_Click(object sender, EventArgs e)
        {
            KonuEkle(16);
            DataEkle(88); // X
            USB_transfer(true, true); // read, write
            timer2.Start();
        }

        private void label33_Click(object sender, EventArgs e)
        {
            KonuEkle(16);
            DataEkle(89); // Y
            USB_transfer(true, true); // read, write
            timer2.Start();
        }

        private void label34_Click(object sender, EventArgs e)
        {
            KonuEkle(16);
            DataEkle(90); // Z
            USB_transfer(true, true); // read, write
            timer2.Start();
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void OrnekBoyutu_tb_TextChanged(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void prf_tb_TextChanged(object sender, EventArgs e)
        {

        }

        private void darbe_sayisi_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void vga_gain_tb_TextChanged(object sender, EventArgs e)
        {

        }

        private void RowData_cb_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rectifier_cb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Trigger_cb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            /*try
            {
                if (Convert.ToInt32(KapiBaslangic_tb.Text) >= Convert.ToInt32(Kapi_bitis_tb.Text))
                {
                    MessageBox.Show("Başlangıç Bitişten Büyük veya Eşit Olamaz!");
                }
                else
                {
                    KonuEkle(SinyalYakalamaAyarlari);

                    //0   buffersize settings          2 byte
                    //2   kapılama başlangıç settings  2 byte
                    //4   kapılama bitiş settings      2 byte
                    //6   row data settings            1 byte
                    //7   rectifier settings           1 byte
                    //8   trigger settings             1 byte
                    //9   clock settings               1 byte
                    //10  chan settings                1 byte
                    //11  zarf katsayisi               1 byte
                    //12  PRF                          1 byte
                    //13  Filtre                       1 byte
                    //14  lnp Gain                     1 byte
                    //15  Vga Gain                     1 byte
                    //16  darbe freaknsı               1 byte
                    //17  darbe sayısı                 1 byte
                    //18  transducer tipi              1 byte
                    //19  hv genlik                    1 byte
                    //20  read_channel_sel             1 byte
                    //21  channel bit gain             1 byte
                    //21  ultraonik yöntem             1 byte

                    DataEkle(120);
                    DataEkle(Convert.ToByte(Convert.ToInt32(OrnekBoyutu_tb.Text) % 256));
                    DataEkle(Convert.ToByte(Convert.ToInt32(OrnekBoyutu_tb.Text) / 256));

                    DataEkle(Convert.ToByte(Convert.ToInt32(KapiBaslangic_tb.Text) % 256));
                    DataEkle(Convert.ToByte(Convert.ToInt32(KapiBaslangic_tb.Text) / 256));

                    DataEkle(Convert.ToByte(Convert.ToInt32(Kapi_bitis_tb.Text) % 256));
                    DataEkle(Convert.ToByte(Convert.ToInt32(Kapi_bitis_tb.Text) / 256));

                    if (RowData_cb.Checked == true) DataEkle(1); else DataEkle(0);

                    DataEkle(Convert.ToByte(rectifier_cb.SelectedIndex)); // 0-->full, 1-->negative, 2-->positive

                    DataEkle(Convert.ToByte(Trigger_cb.SelectedIndex)); // 0-->Ext, 1-->int

                    DataEkle(Convert.ToByte(FrekansSec_cb.SelectedIndex)); // 0-->125, 1-->62.5,2-->31...

                    DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex)); //  0-->A&B, 1-->A, 2-->B

                    DataEkle(Convert.ToByte(Convert.ToInt32(zarf_k_tb.Text)));// zarfın çözünürlüğünü ifade eder  buffersize/zarf katsayısı çözünürlüğü verir

                    DataEkle(Convert.ToByte(Convert.ToInt32(prf_tb.Text)));


                    if (fir_cb.Checked == true) DataEkle(1); else DataEkle(0);

                    DataEkle(Convert.ToByte(trackBar1.Value));

                    DataEkle(Convert.ToByte(lnp_cb.SelectedIndex));

                    DataEkle(Convert.ToByte(trackBar2.Value));

                    DataEkle(Convert.ToByte(pulse_freq_comb.SelectedIndex));

                    DataEkle(Convert.ToByte(Convert.ToInt32(darbe_sayisi.Text)));

                    DataEkle(Convert.ToByte(Pulseoutputchannelcb.SelectedIndex));

                    DataEkle(Convert.ToByte(Convert.ToInt32(hv_txt_bx.Text)));

                    DataEkle(Convert.ToByte(Read_channel_sellect_cb.SelectedIndex));

                    DataEkle(Convert.ToByte(channel_bit_gain_cb.SelectedIndex));

                    DataEkle(Convert.ToByte(Ultrasonik_yontem_cb.SelectedIndex));

                    USB_transfer(true, true);
                    if (System.Windows.Forms.Application.MessageLoop)
                    {
                        // WinForms app
                        System.Windows.Forms.Application.Exit();
                    }
                    else
                    {
                        // Console app
                        System.Environment.Exit(1);
                    }

                }
            } catch(Exception E)
            {

            } */
            timer3.Stop();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
            {


                KonuEkle(AdcOrnekGonder);
                DataEkle(Convert.ToByte(Chan_set_cb.SelectedIndex));
                USB_transfer(true, true);
            }


            private void button1_Click(object sender, EventArgs e)
            {
                if (btnbaglantı.Checked)
                {
                    string filename = "";

                    OpenFileDialog ofd = new OpenFileDialog();
                    DialogResult dr = ofd.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        filename = ofd.FileName;
                    }
                    BootKaynakAdi = filename;

                    BootFileArrange();


                }
            }
            private void mouse_timer_Tick(object sender, EventArgs e)
            {

                mouse_timer_tick1 = 1;
                mouse_timer.Stop();

                // vga_gain_tb.Text = Convert.ToString(trackBar2.Value + lnpgain[lnp_cb.SelectedIndex]) + "dB";

                KonuEkle(AdcGainSet);
                DataEkle(Convert.ToByte(trackBar2.Value));
                DataEkle(Convert.ToByte(lnp_cb.SelectedIndex));
                USB_transfer(true, true);


            }


            private void Kalibre_btn_Click(object sender, EventArgs e)
            {


                //yanki_süresi = (maximum_noktalar[1] - maximum_noktalar[0]) * 5;
                // malzeme_kalinligi = (yanki_süresi * ses_hizi[malzeme_cb.SelectedIndex]) / 1000000;//mm cinsinden kalınlık
                //SonucKalinlik_txt.Text = "Kalınlık= " + malzeme_kalinligi.ToString() + " mm";
                //delay_distance_tb

                malzeme_kalinligi = Convert.ToDouble(Kalinlik_tbx.Text);
                double sure = malzeme_kalinligi * 1000000 / ses_hizi[malzeme_cb.SelectedIndex];
                delay_distance_tb.Text = (yanki_süresi - sure).ToString();
                //ses_hizi[malzeme_cb.SelectedIndex]=Convert.ToInt32((malzeme_kalinligi * 1000000) / yanki_süresi);
            }

            private void button3_Click(object sender, EventArgs e)
            {


                KonuEkle(Fpga_read_test);
                DataEkle(Convert.ToByte(textBox2.Text));

                USB_transfer(false, true);
            }






        }
    }


       