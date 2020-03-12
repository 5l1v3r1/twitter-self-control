using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Eklenenler
using HtmlAgilityPack;
using System.IO;
using System.Net;
using System.Threading;



namespace Twitter_Self_Control
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // değişkenler
        string htmlKod = null;
        string htmlKod2 = "";
        string bagliHesap = null;
        DateTime guncelZaman;

        int takipciSayisi = 0;
        string toplamTakipci = null;
        int kalanTakipci = 0;
        string islemdekiProfil = null;
        int basarisizTakip = 0;
        bool tarayiciDurdur = false;
        string kaydedilenZaman = null;
        string kaydedilenGecenZaman = null;
        bool gZaman = false;
        string kaydedilenGecenZaman2 = null;
        string islemdekiProfil2 = null;
        int basarisizCikarma = 0;
        bool tarayiciDurdur2 = false;
        bool gZaman2 = false;
        Thread thread_TakipciToplamaIslemi;
        Thread thread_TakipEtmeIslemi;
        Thread thread_takipEttiklerimToplamaIslemi;
        Thread thread_TakipBirakmaIslemi;
        bool islemDurumu = false;
        int takiptenCikarilanToplam = 0;


        #region Veri Ayıklama Fonksiyon
        public string ayiklananVeri;
        void veriAyiklama(string kaynakKod, string ilkVeri, int ilkVeriKS, string sonVeri)
        {
            try
            {
                string gelen = kaynakKod;
                int titleIndexBaslangici = gelen.IndexOf(ilkVeri) + ilkVeriKS;
                int titleIndexBitisi = gelen.Substring(titleIndexBaslangici).IndexOf(sonVeri);
                ayiklananVeri = gelen.Substring(titleIndexBaslangici, titleIndexBitisi);
            }
            catch //(Exception ex)
            {
                //MessageBox.Show("Hata: " + ex.Message, "Hata;", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region SefLink Fonksiyon
        public string sefLink(string link)
        {
            string don = link.ToLower();
            don = don.Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace(".", "").Replace(",", "").Replace(" ", "-");
            don = don.Replace("+", "plus").Replace("#", "sharp").Replace("ü", "u").Replace("ö", "o").Replace("?", "");
            return don;
        }
        #endregion

        #region FORM_LOAD
        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread Çalıştırma
            CheckForIllegalCrossThreadCalls = false;
        }
        #endregion

        #region FORM_SHOWN - Form açıldığında
        private void Form1_Shown(object sender, EventArgs e)
        {
            // Bilgi Mesajı
            statusLabel.ForeColor = Color.Blue;
            statusLabel.Text = "Program başlatıldı.";
           listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Program başlatıldı.");

            // Bilgi Mesajı 
            statusLabel.ForeColor = Color.Blue;
            statusLabel.Text = "Twitter'a bağlanılıyor. Lütfen bekleyin..";
           listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter'a bağlanılıyor. Lütfen bekleyin..");
        }
        #endregion

        #region FORM_CLOSED & FORM_CLOSING
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region webBrowser İşlemleri - webBrowser_DocumentCompleted

        #region webBrowser 1
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                // webBrowser Kaynak Kod
                htmlKod = webBrowser1.Document.Body.InnerHtml.ToString();
                richTextBox1.Text = htmlKod;

                // webBrowser Dinamik URL
                textBox_webBrowser_URL.Text = webBrowser1.Url.ToString();


                Thread.Sleep(int.Parse(textBox2.Text + "000"));

                // İşlemler - Giriş Yapma işlemi
                if (htmlKod.IndexOf("Giriş yap") != -1)
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Twitter'a bağlanıldı. Giriş Yapabilirsiniz.";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter'a bağlanıldı. Giriş Yapabilirsiniz.");
                    label_durum.ForeColor = Color.DarkGreen;
                    label_durum.Text = "Giriş Yapabilirsiniz";
                    box_Twitter_GirisYap.Enabled = true;
                }
                else if (htmlKod.IndexOf("<H2>Zaman akışın</H2>") != -1)
                {
                    webBrowser2.Navigate(textBox_TwitterURL.Text + "account");
                }
                else if (htmlKod.IndexOf("<TD class=me><A title=Ben") != -1)
                {
                    webBrowser2.Navigate(textBox_TwitterURL.Text + "account");
                }
                else if (htmlKod.IndexOf("<INPUT type=submit value=\"Takip et\" name=commit>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        try
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkBlue;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // takip et butonu
                            HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                            foreach (HtmlElement el2 in elc2)
                            {
                                if (el2.GetAttribute("value").Equals("Takip et"))
                                {
                                    el2.InvokeMember("Click");
                                }
                            }
                        }
                        catch
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkRed;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            basarisizTakip = basarisizTakip + 1;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // thread başlatma
                            thread_TakipEtmeIslemi = new Thread(delegate()
                            {
                                hesabiTakipEt("");
                            });
                            thread_TakipEtmeIslemi.Start();
                        }
                    }
                }
                else if (htmlKod.IndexOf("<INPUT type=submit value=\"İstek gönder\" name=commit>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        try
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkBlue;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // takip et butonu
                            HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                            foreach (HtmlElement el2 in elc2)
                            {
                                if (el2.GetAttribute("value").Equals("İstek gönder"))
                                {
                                    el2.InvokeMember("Click");
                                }
                            }
                        }
                        catch
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkRed;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            basarisizTakip = basarisizTakip + 1;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // thread başlatma
                            thread_TakipEtmeIslemi = new Thread(delegate()
                            {
                                hesabiTakipEt("");
                            });
                            thread_TakipEtmeIslemi.Start();
                        }
                    }
                }
                else if (htmlKod.IndexOf("<INPUT type=submit value=\"Takip ediliyor\" name=commit>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkGreen;
                        statusLabel.Text = islemdekiProfil + " Twitter hesabı takip ediliyor.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip ediliyor.");
                        listBox_Takip_Etme_Listesi.Items.Insert(0, islemdekiProfil + " twitter hesabı takip ediliyor.");

                        int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                        int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                        kalanHesap = kalanHesap - takipEdilen;

                        label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                        label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                        label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                        // thread başlatma
                        thread_TakipEtmeIslemi = new Thread(delegate()
                        {
                            hesabiTakipEt("");
                        });
                        thread_TakipEtmeIslemi.Start();
                    }
                }
                else if (htmlKod.IndexOf("<INPUT type=submit value=\"İsteği iptal et\" name=commit>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkGreen;
                        statusLabel.Text = islemdekiProfil + " Twitter hesabı takip ediliyor.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip ediliyor.");
                        listBox_Takip_Etme_Listesi.Items.Insert(0, islemdekiProfil + " twitter hesabı takip ediliyor.");

                        int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                        int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                        kalanHesap = kalanHesap - takipEdilen;

                        label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                        label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                        label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                        // thread başlatma
                        thread_TakipEtmeIslemi = new Thread(delegate()
                        {
                            hesabiTakipEt("");
                        });
                        thread_TakipEtmeIslemi.Start();
                    }
                }
                else if (htmlKod.IndexOf("Çok fazla denemede bulundun.") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkRed;
                        statusLabel.Text = "Twitter çok fazla denemede bulunulduğu için engelledi. Bir kaç dakika sonra işlem devam edecek.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter çok fazla denemede bulunulduğu için engelledi. Bir kaç dakika sonra işlem devam edecek.");


                        kaydedilenZaman = guncelZaman.AddSeconds(double.Parse(textBox1.Text)).ToString();
                    }
                }
                else if (htmlKod.IndexOf("<DIV class=title>Üzgünüz, böyle bir sayfa yok</DIV>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkRed;
                        statusLabel.Text = islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.");

                        int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                        int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                        kalanHesap = kalanHesap - takipEdilen;

                        basarisizTakip = basarisizTakip + 1;

                        label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                        label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                        label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                        // thread başlatma
                        thread_TakipEtmeIslemi = new Thread(delegate()
                        {
                            hesabiTakipEt("");
                        });
                        thread_TakipEtmeIslemi.Start();
                    }
                }
                else if (htmlKod.IndexOf("<H1 class=confirm_title>Bu hesabı takip etmek istiyor musun?</H1>") != -1)
                {
                    if (tarayiciDurdur == false)
                    {
                        try
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkBlue;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip etme işlemi başladı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // takip et butonu
                            HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                            foreach (HtmlElement el2 in elc2)
                            {
                                if (el2.GetAttribute("value").Equals("Takip et"))
                                {
                                    el2.InvokeMember("Click");
                                }
                            }
                        }
                        catch
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkRed;
                            statusLabel.Text = islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil + " Twitter hesabı takip edilemedi! Diğerine atlandı.");

                            int takipEdilen = listBox_Takip_Etme_Listesi.Items.Count;
                            int kalanHesap = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count;

                            kalanHesap = kalanHesap - takipEdilen;

                            basarisizTakip = basarisizTakip + 1;

                            label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: " + takipEdilen;
                            label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + kalanHesap;
                            label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: " + basarisizTakip;

                            // thread başlatma
                            thread_TakipEtmeIslemi = new Thread(delegate()
                            {
                                hesabiTakipEt("");
                            });
                            thread_TakipEtmeIslemi.Start();
                        }
                    }
                }

            }
        }
        #endregion

        #region webBrowser 2
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                // webBrowser Kaynak Kod
                htmlKod2 = webBrowser2.Document.Body.InnerHtml.ToString();
                richTextBox2.Text = htmlKod2;

                // webBrowser Dinamik URL
                textBox_webBrowser2_URL.Text = webBrowser2.Url.ToString();

                if (htmlKod2.IndexOf("<INPUT type=submit value=Beğenilerin name=commit>") != -1 && htmlKod2.IndexOf("<DIV class=fullname>") != -1)
                {
                    try
                    {
                        // HtmlDocument sınıf tanımlama
                        HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                        dokuman.LoadHtml(htmlKod2);

                        HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//span[@class='screen-name']");
                        foreach (var veri in XPath)
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkGreen;
                            statusLabel.Text = veri.InnerText + " olarak bağlanıldı!";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + veri.InnerText + " olarak bağlanıldı!");
                            label_durum.ForeColor = Color.DarkGreen;
                            label_durum.Text = "Giriş Yapılmış";
                            box_Twitter_GirisYap.Enabled = false;
                            tabControl1.Enabled = true;

                            bagliHesap = veri.InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkRed;
                        statusLabel.Text = "Twitter'a giriş yaparken bir hata oluştu! Hata: " + ex.Message;
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter'a giriş yaparken bir hata oluştu! Hata: " + ex.Message);
                    }
                }
                else if (htmlKod2.IndexOf("<INPUT type=submit value=\"Takibi bırak\" name=commit>") != -1)
                {
                    if (tarayiciDurdur2 == false)
                    {
                        try
                        {
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkBlue;
                            statusLabel.Text = islemdekiProfil2 + " Twitter hesabı takipten çıkarılıyor.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " Twitter hesabı takipten çıkarılıyor.");

                            // takipten çıkar butonu
                            HtmlElementCollection elc2 = webBrowser2.Document.GetElementsByTagName("input");
                            foreach (HtmlElement el2 in elc2)
                            {
                                if (el2.GetAttribute("value").Equals("Takibi bırak"))
                                {
                                    el2.InvokeMember("Click");
                                }
                            }
                        }
                        catch
                        {
                            
                            // Bilgi Mesajı
                            statusLabel.ForeColor = Color.DarkRed;
                            statusLabel.Text = islemdekiProfil2 + " Twitter hesabı takipten çıkarmasında sorun oluştu! 1 Diğerine atlandı.";
                            listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " Twitter hesabı takipten çıkarmasında sorun oluştu! 1 Diğerine atlandı.");

                            listBox_Takipten_Cikarma.Items.Add(islemdekiProfil2 + " hesabında sorun oluştu.");

                            int takipBirakilan = listBox_Takipten_Cikarma.Items.Count;
                            int kalanHesap2 = listBox_Takip_Ettiklerim_Listesi.Items.Count;

                            kalanHesap2 = kalanHesap2 - takipBirakilan;

                            basarisizTakip = basarisizTakip + 1;

                            label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + kalanHesap2;
                            label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: " + basarisizCikarma;

                            // thread başlatma
                            thread_TakipBirakmaIslemi = new Thread(delegate()
                            {
                                hesabiCikar("");
                            });
                            thread_TakipBirakmaIslemi.Start();
                        }
                    }
                }
                else if (
                    htmlKod2.IndexOf("<INPUT type=submit value=\"İstek gönder\" name=commit>") != -1 ||
                    htmlKod2.IndexOf("<DIV class=title>Üzgünüz, böyle bir sayfa yok</DIV>") != -1 ||
                    htmlKod2.IndexOf("<INPUT type=submit value=\"Takip et\" name=commit>") != -1
                    )
                {

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkBlue;
                    statusLabel.Text = islemdekiProfil2 + " Twitter hesabı takipten çıkarıldı.";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " Twitter hesabı takipten çıkarıldı");


                    listBox_Takipten_Cikarma.Items.Add(islemdekiProfil2 + " takipten çıkarıldı.");

                    takiptenCikarilanToplam = takiptenCikarilanToplam + 1;
                    int takipBirakilan = listBox_Takipten_Cikarma.Items.Count;
                    int kalanHesap2 = listBox_Takip_Ettiklerim_Listesi.Items.Count;

                    kalanHesap2 = kalanHesap2 - takipBirakilan;

                    label_TakiptenCikarma_CikarilanHesaplar.Text = "Toplam Takipten Çıkarılan Hesap: " + takiptenCikarilanToplam.ToString();
                    label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + kalanHesap2;
                    label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: " + basarisizCikarma;

                    // thread başlatma
                    thread_TakipBirakmaIslemi = new Thread(delegate()
                    {
                        hesabiCikar("");
                    });
                    thread_TakipBirakmaIslemi.Start();
                }
                else if (htmlKod.IndexOf("Çok fazla denemede bulundun.") != -1)
                {
                    if (tarayiciDurdur2 == false)
                    {
                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkRed;
                        statusLabel.Text = "Twitter çok fazla denemede bulunulduğu için engelledi. Bir kaç dakika sonra işlem devam edecek.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter çok fazla denemede bulunulduğu için engelledi. Bir kaç dakika sonra işlem devam edecek.");


                        kaydedilenZaman = guncelZaman.AddSeconds(double.Parse(textBox1.Text)).ToString();
                    }
                }

                if (islemDurumu == true)
                {
                    if (htmlKod2.IndexOf("<SPAN class=follows-you>Seni Takip Ediyor</SPAN>") != -1)
                    {

                        // Bilgi Mesajı
                        statusLabel.ForeColor = Color.DarkBlue;
                        statusLabel.Text = islemdekiProfil2 + " seni takip ediyor. Diğerine atlandı.";
                        listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " seni takip ediyor. Diğerine atlandı.");

                        listBox_Takipten_Cikarma.Items.Add(islemdekiProfil2 + " seni takip ediyor.");

                        int kalanHesap2 = listBox_Takip_Ettiklerim_Listesi.Items.Count;

                        kalanHesap2 = kalanHesap2 - listBox_Takipten_Cikarma.Items.Count;

                        label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + kalanHesap2;
                        label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: " + basarisizCikarma;


                        // thread başlatma
                        thread_TakipBirakmaIslemi = new Thread(delegate()
                        {
                            hesabiCikar("");
                        });
                        thread_TakipBirakmaIslemi.Start();
                    }
                    else
                    {
                        if (tarayiciDurdur2 == false)
                        {
                            try
                            {
                                /*
                                // Bilgi Mesajı
                                statusLabel.ForeColor = Color.DarkBlue;
                                statusLabel.Text = islemdekiProfil2 + " Twitter hesabı incelemesi başladı.";
                                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " Twitter hesabı incelemesi başladı.");
                                */

                                int takipBirakilan = listBox_Takipten_Cikarma.Items.Count;
                                int kalanHesap2 = listBox_Takip_Ettiklerim_Listesi.Items.Count;

                                kalanHesap2 = kalanHesap2 - takipBirakilan;

                                label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + kalanHesap2;
                                label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: " + basarisizCikarma;

                                // takipten çıkar butonu
                                HtmlElementCollection elc2 = webBrowser2.Document.GetElementsByTagName("input");
                                foreach (HtmlElement el2 in elc2)
                                {
                                    if (el2.GetAttribute("value").Equals("Takip ediliyor"))
                                    {
                                        el2.InvokeMember("Click");
                                    }
                                }
                            }
                            catch
                            {
                                /*
                                // Bilgi Mesajı
                                statusLabel.ForeColor = Color.DarkRed;
                                statusLabel.Text = islemdekiProfil2 + " Twitter hesabı incelemesinde sorun oluştu! Diğerine atlandı.";
                                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + islemdekiProfil2 + " Twitter hesabı incelemesinde sorun oluştu! Diğerine atlandı.");
                                */
                                int takipBirakilan = listBox_Takipten_Cikarma.Items.Count;
                                int kalanHesap2 = listBox_Takip_Ettiklerim_Listesi.Items.Count;

                                kalanHesap2 = kalanHesap2 - takipBirakilan;

                                basarisizTakip = basarisizTakip + 1;

                                label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + kalanHesap2;
                                label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: " + basarisizCikarma;

                                // thread başlatma
                                thread_TakipBirakmaIslemi = new Thread(delegate()
                                {
                                    hesabiCikar("");
                                });
                                thread_TakipBirakmaIslemi.Start();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Twitter Giriş Yapma İşlemleri

        #region 1- Giriş Yap Butonu
        private void button_Twitter_GirisYap_Click(object sender, EventArgs e)
        {
            try
            {
                // Kullanıcı adı
                webBrowser1.Document.GetElementById("session[username_or_email]").InnerText = textBox_Twitter_Kadi.Text;

                // Şifre
                webBrowser1.Document.GetElementById("session[password]").InnerText = textBox_Twitter_Sifre.Text;

                // Giriş Yap butonu
                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement el2 in elc2)
                {
                    if (el2.GetAttribute("name").Equals("commit"))
                    {
                        el2.InvokeMember("Click");
                    }
                }
            }
            catch (Exception ex)
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = "Twitter'a giriş yaparken bir hata oluştu! Hata: " + ex.Message;
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Twitter'a giriş yaparken bir hata oluştu! Hata: " + ex.Message);
            }
        }
        #endregion

        #endregion

        #region Takipçi Toplama Sekmesi

        #region 1- Kaynak Profil Listesi İşlemleri

        #region Profil Listesi Yükle Butonu
        private void button_Profil_Listesi_Yukle_Click(object sender, EventArgs e)
        {
            // Profil aktar ayarları
            OpenFileDialog yukle = new OpenFileDialog();
            yukle.Title = "Profil Aktar";
            yukle.Filter = "Txt dosyası|*.txt";

            // Dosya yükleme için onay
            if (yukle.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Dosya içeriğini oku ve aktar
                    StreamReader aktar = new StreamReader(yukle.FileName);
                    string hesap = aktar.ReadLine();
                    while (hesap != null)
                    {
                        // Profillerin Takipçi Sayılarını Bulma
                        profilTopTakipci(hesap);
                        hesap = aktar.ReadLine();
                    }
                    aktar.Close();

                    // Dosya içindeki toplam hesap
                    string topProfil = listBox_Profil_Listesi.Items.Count.ToString();
                    label_Profil_Listesi_Toplam.Text = "Toplam Profil: " + topProfil;

                    // Nesne aktifleştirme
                    groupBox_Takipci_Toplama.Enabled = true;

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Profil Listesi Aktarıldı! Toplam Profil: " + topProfil;
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Profil Listesi Aktarıldı! Toplam Profil: " + topProfil);
                    MessageBox.Show("Profil Listesi Aktarıldı! Toplam Profil: " + topProfil, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkRed;
                    statusLabel.Text = "Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!";
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!");
                    MessageBox.Show("Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.Blue;
                statusLabel.Text = "Profil aktarmayı iptal ettiniz.";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Profil aktarmayı iptal ettiniz.");
                MessageBox.Show("Profil aktarmayı iptal ettiniz.","Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Profil Listesi Temizle Butonu
        private void button_Profil_Listesi_Temizle_Click(object sender, EventArgs e)
        {
            // Temizleme sorusu
            DialogResult soru = MessageBox.Show("Profil listesi temizlensin mi?","Soru",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // liste temizle
                listBox_Profil_Listesi.Items.Clear();
                label_Profil_Listesi_Toplam.Text = "Toplam Profil: null";
                label_Profil_Takipci_Sayisi.Text = "Takipçi Sayısı: null ";
                takipciSayisi = 0;
                toplamTakipci = null;
                kalanTakipci = 0;

                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Profil Listesi Temizlendi.";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Profil Listesi Temizlendi.");
                MessageBox.Show("Profil Listesi Temizlendi. ", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Profilin Takipçi Sayısını Bulan Fonksiyon
        void profilTopTakipci(string hesap)
        {
            try
            {
                // Client sınıf tanımlama
                WebClient client = new WebClient();
                // Client kodlama dili belirme
                client.Encoding = Encoding.UTF8;
                // Siteye bağlanıp kaynak kodlarını alma
                string kaynakKod = client.DownloadString(textBox_TwitterURL.Text + hesap);
                // HtmlDocument sınıf tanımlama
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                // Sınıfa html kodlarını yükleme
                dokuman.LoadHtml(kaynakKod);
                // Html kodları arasında XPath ile veri yakalama
                HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//td[@class='stat stat-last']/a/div[1]");
                // Html tagları arasında döngü oluşturma
                foreach (var veri in XPath)
                {
                    // verileri çekme
                    string sayi = veri.InnerText.Replace(",", "");

                    // Dosyadaki profili aktarma
                    listBox_Profil_Listesi.Items.Add(hesap + " (" + sayi + ")");

                    // takipcileri toplama
                    takipciSayisi = takipciSayisi + int.Parse(sayi);
                    label_Profil_Takipci_Sayisi.Text = "Takipçi Sayısı: " + takipciSayisi.ToString();
                    toplamTakipci = takipciSayisi.ToString();
                }
            }
            catch
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = hesap + " profilin bilgileri çekilirken hata oluştur!";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + hesap + " profilin bilgileri çekilirken hata oluştur!");
                MessageBox.Show(hesap + " profilin bilgileri çekilirken hata oluştur!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #endregion

        #region 2- Takipçi Toplama İşlemleri

        #region Takipçi Toplama Fonksiyonu
        void takipciToplama(string hesap, string cursor = "-1")
        {
            try
            {
                // hesap url düzenleme
                string hesapURL = textBox_Twitter_TakipcilerURL.Text.Replace("hesap_kadi", hesap) + cursor;

                // Client sınıf tanımlama
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                string kaynakKod = client.DownloadString(hesapURL);

                // HtmlDocument sınıf tanımlama
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(kaynakKod);

                // Takipçileri alma
                try
                {
                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//span[@class='username']");
                    foreach (var veri in XPath)
                    {
                        // veri düzenleme
                        string profil = veri.InnerText;
                        profil = profil.Substring(1, profil.Length - 1);

                        if (listBox_Takipci_Toplama_Takipci_Listesi.Items.Contains(profil) == false)
                        {
                            // kalan takipçi belirleme
                            kalanTakipci = kalanTakipci - 1;
                            label_Takipci_Toplama_Kalan.Text = "Kalan: " + kalanTakipci.ToString();
                            
                            // takipçiyi listeye ekleme
                            if (textBox_Twitter_Kadi.Text.IndexOf(profil) != -1 || bagliHesap.IndexOf(profil) != -1)
                            {

                            }
                            else
                            {
                                listBox_Takipci_Toplama_Takipci_Listesi.Items.Add(profil);
                            }

                            // toplam takipçi belirleme
                            label_Takipci_Toplama_Toplam.Text = "Toplam: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();

                        }
                    }
                }
                catch
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkRed;
                    statusLabel.Text = hesap + " profilin takipçileri çekilemiyor! burda2";
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + hesap + " profilin takipçileri çekilemiyor! burda2");
                }

                // cursor alma
                try
                {
                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='w-button-more']/a");
                    foreach (var veri in XPath)
                    {
                        string link = veri.Attributes["href"].Value + "#";
                        veriAyiklama(link, "?cursor=", 8, "#");

                        // eğer bir sonraki sayfa varsa fonksiyonu tekrar çağır
                        thread_TakipciToplamaIslemi = new Thread(delegate()
                        {
                            takipciToplama(hesap, ayiklananVeri);
                        });
                        thread_TakipciToplamaIslemi.Start();
                    }
                }
                catch
                {
                    // eğer bu profilin takipçisi bu kadarsa diğerine geç
                    if (listBox_Profil_Listesi.Items.Count == listBox_Profil_Listesi.SelectedIndex + 1)
                    {
                        // Bilgi Mesajı
                        label_Takipci_Toplama_Toplam.Text = "Toplam: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();

                        statusLabel.ForeColor = Color.DarkGreen;
                        statusLabel.Text = "Takipçi Toplama işlemi tamamlandı! Bulunan Toplam Takipçi Sayısı: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();
                       listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama işlemi tamamlandı! Bulunan Toplam Takipçi Sayısı: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString());
                        MessageBox.Show("Takipçi Toplama işlemi tamamlandı!\nBulunan Toplam Takipçi Sayısı: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Nesneleri pasifleştir
                        button_Takipci_Toplama_Durdur.Enabled = false;

                        // Nesneleri aktifleştir
                        button_Takipci_Toplama_Baslat.Enabled = true;
                        button_Takipci_Toplama_ListeTemizle.Enabled = true;

                        button_Profil_Listesi_Yukle.Enabled = true;
                        button_Profil_Listesi_Temizle.Enabled = true;
                        listBox_Profil_Listesi.Enabled = true;

                        // bilgileri sıfırlama
                        kalanTakipci = 0;

                        // thread durdurma
                        thread_TakipciToplamaIslemi.Abort();
                    }
                    else
                    {
                        // bir diğer profile geç

                        // veri düzenleme
                        listBox_Profil_Listesi.SelectedIndex = listBox_Profil_Listesi.SelectedIndex + 1;
                        string[] profil = listBox_Profil_Listesi.Text.Split(' ');

                        // Thread başlatma
                        thread_TakipciToplamaIslemi = new Thread(delegate()
                        {
                            takipciToplama(profil[0]);
                        });
                        thread_TakipciToplamaIslemi.Start();
                    }
                }

            }
            catch
            {
                /*
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = hesap + " profilin takipçileri çekilemiyor! burda1";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + hesap + " profilin takipçileri çekilemiyor! burda1");
                */
            }
        }
        #endregion

        #region Takipçi Toplama Başlat Butonu
        private void button_Takipci_Toplama_Baslat_Click(object sender, EventArgs e)
        {
            if (listBox_Profil_Listesi.Items.Count > 0)
            {
                // başlatma sorusu
                DialogResult soru = MessageBox.Show("Takipçi Toplama işlemi başlatılsın mı?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // Başlatılırsa

                    // sıfırlama işlemleri
                    listBox_Takipci_Toplama_Takipci_Listesi.Items.Clear();
                    label_Takipci_Toplama_Toplam.Text = "Toplam: null";
                    label_Takipci_Toplama_Kalan.Text = "Kalan: null";

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Takipçi Toplama işlemi başlatıldı!";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama işlemi başlatıldı!");
                    MessageBox.Show("Takipçi Toplama işlemi başlatıldı!\n\nBu işlem internet bağlantı hızı ve bilgisayarınızın performansına göre değişkenlik gösterebilir.\n\nDilerseniz işlemi durdurabilir ve toplanan Takipçi Listesini kaydedebilirsiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Nesneleri pasifleştir
                    button_Takipci_Toplama_Baslat.Enabled = false;
                    button_Takipci_Toplama_ListeTemizle.Enabled = false;

                    button_Profil_Listesi_Yukle.Enabled = false;
                    button_Profil_Listesi_Temizle.Enabled = false;
                    listBox_Profil_Listesi.Enabled = false;

                    // Nesneleri aktifleştir
                    button_Takipci_Toplama_Durdur.Enabled = true;

                    // bilgiler
                    label_Takipci_Toplama_Kalan.Text = "Kalan: " + toplamTakipci;
                    kalanTakipci = int.Parse(toplamTakipci);

                    // başlat
                    listBox_Profil_Listesi.SelectedIndex = 0;
                    string[] profil = listBox_Profil_Listesi.Text.Split(' ');

                    // thread başlatma
                    thread_TakipciToplamaIslemi = new Thread(delegate()
                    {
                        takipciToplama(profil[0]);
                    });
                    thread_TakipciToplamaIslemi.Start();
                }

            }
            else
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = "İşlem başlatılamıyor çünkü Kaynak Profil Listesinde hesap yok.";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] İşlem başlatılamıyor çünkü Kaynak Profil Listesinde hesap yok.");
                MessageBox.Show("İşlem başlatılamıyor çünkü Kaynak Profil Listesinde hesap yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Takipçi Toplama Durdur Butonu
        private void button_Takipci_Toplama_Durdur_Click(object sender, EventArgs e)
        {
            // başlatma sorusu
            DialogResult soru = MessageBox.Show("Takipçi Toplama işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Başlatılırsa

                // Bilgi Mesajı
                label_Takipci_Toplama_Toplam.Text = "Toplam: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();

                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Takipçi Toplama işlemi durduruldu!";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama işlemi durduruldu!");
                MessageBox.Show("Takipçi Toplama işlemi durduruldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_Takipci_Toplama_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                button_Takipci_Toplama_Baslat.Enabled = true;
                button_Takipci_Toplama_ListeTemizle.Enabled = true;

                button_Profil_Listesi_Yukle.Enabled = true;
                button_Profil_Listesi_Temizle.Enabled = true;
                listBox_Profil_Listesi.Enabled = true;

                // bilgileri sıfırlama
                kalanTakipci = 0;

                // thread durdurma
                thread_TakipciToplamaIslemi.Abort();

            }
        }
        #endregion

        #region Takipçi Toplama Listeyi Kaydet Butonu
        private void button_Takipci_Toplama_ListeyiKaydet_Click(object sender, EventArgs e)
        {
            // Kontrolü göster
            SaveFileDialog kaydet = new SaveFileDialog();
            kaydet.Title = "Dosyayı kaydet";
            kaydet.Filter = "Txt dosyası|*.txt";

            if (kaydet.ShowDialog() == DialogResult.OK)
            {
                string yol = kaydet.FileName;

                //Yeni bir dosya oluştur
                try
                {
                    //Dosyayı appendText ile yazmak için açtık
                    StreamWriter dosyaAc = File.AppendText(yol);
                    // Dosya.WriteLine ile dosyaya verileri ekledik.
                    foreach (var item in listBox_Takipci_Toplama_Takipci_Listesi.Items)
                    {
                        dosyaAc.WriteLine(item);
                    }
                    // Dosya yı kapattık.
                    dosyaAc.Close();

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Toplanan Takipçi Listesi başarıyla kaydedildi.";
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplanan Takipçi Listesi başarıyla kaydedildi.");
                    MessageBox.Show("Toplanan Takipçi Listesi başarıyla kaydedildi.\n\nDosya Yolu: " + yol, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch 
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkRed;
                    statusLabel.Text = "Toplanan Takipçi Listesi kaydedilemedi bir hata oluştu!";
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplanan Takipçi Listesi kaydedilemedi bir hata oluştu!");
                    MessageBox.Show("Toplanan Takipçi Listesi kaydedilemedi bir hata oluştu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Takipçi Toplama Listeyi Temizle Butonu
        private void button_Takipci_Toplama_ListeTemizle_Click(object sender, EventArgs e)
        {
            // Temizleme sorusu
            DialogResult soru = MessageBox.Show("Takipçi Toplama listesi temizlensin mi?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // liste temizle
                listBox_Takipci_Toplama_Takipci_Listesi.Items.Clear();
                label_Takipci_Toplama_Toplam.Text = "Toplam: null";
                label_Takipci_Toplama_Kalan.Text = "Kalan: null ";
                kalanTakipci = 0;

                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Takipçi Toplama Listesi Temizlendi.";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama Listesi Temizlendi.");
                MessageBox.Show("Takipçi Toplama Listesi Temizlendi. ", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Takipçi Aktarma
        private void button_Takipci_Toplama_ListeYukle_Click(object sender, EventArgs e)
        {
            // Profil aktar ayarları
            OpenFileDialog yukle = new OpenFileDialog();
            yukle.Title = "Profil Aktar";
            yukle.Filter = "Txt dosyası|*.txt";

            // Dosya yükleme için onay
            if (yukle.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Dosya içeriğini oku ve aktar
                    StreamReader aktar = new StreamReader(yukle.FileName);
                    string hesap = aktar.ReadLine();
                    while (hesap != null)
                    {
                        // Profillerin Takipçi Sayılarını Bulma
                        // takipçiyi listeye ekleme
                        if (textBox_Twitter_Kadi.Text.IndexOf(hesap) != -1 || bagliHesap.IndexOf(hesap) != -1)
                        {

                        }
                        else
                        {
                            listBox_Takipci_Toplama_Takipci_Listesi.Items.Add(hesap);
                        }
                        hesap = aktar.ReadLine();
                    }
                    aktar.Close();

                    // Dosya içindeki toplam hesap
                    string topProfil = listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();
                    label_Takipci_Toplama_Toplam.Text = "Toplam: " + topProfil;

                    // nesne aktifleştirme
                    button_Takipci_Toplama_ListeTemizle.Enabled = true;

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Takipçi Listesi Aktarıldı! Toplam Profil: " + topProfil;
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Listesi Aktarıldı! Toplam Profil: " + topProfil);
                    MessageBox.Show("Takipçi Listesi Aktarıldı! Toplam Profil: " + topProfil, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkRed;
                    statusLabel.Text = "Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!";
                   listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!");
                    MessageBox.Show("Dosya aktarılırken bir hata oluştur. Tekrar Deneyin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.Blue;
                statusLabel.Text = "Takipçi aktarmayı iptal ettiniz.";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Profil aktarmayı iptal ettiniz.");
                MessageBox.Show("Takipçi aktarmayı iptal ettiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #endregion

        #region 3- Takipçi Takip Etme İşlemleri

        #region Başlat Butonu
        private void button_Takip_Etme_Baslat_Click(object sender, EventArgs e)
        {
            if (listBox_Takipci_Toplama_Takipci_Listesi.Items.Count > 1)
            {
                // başlatma sorusu
                DialogResult soru = MessageBox.Show("Toplu Takip Etme işlemi başlatılsın mı?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // Başlatılırsa

                    // sıfırlama işlemleri
                    listBox_Takip_Etme_Listesi.Items.Clear();
                    label_Takip_Etme_ToplamTakipEdilen.Text = "Toplam Takip Edilen Hesap: null";
                    label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: null";
                    label_Takip_Etme_Basarisiz.Text = "Başarısız Takip İşlemi: null";
                    label_Takip_Etme_GecenZaman.Text = "[00:00:00]";

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Toplu Takip Etme işlemi başlatıldı!";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takip Etme işlemi başlatıldı!");
                    MessageBox.Show("Toplu Takip Etme işlemi başlatıldı!\n\nBu işlem internet bağlantı hızı ve bilgisayarınızın performansına göre değişkenlik gösterebilir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Nesneleri pasifleştir
                    groupBox_Kaynak_Profil_Listesi.Enabled = false;
                    groupBox_Takipci_Toplama.Enabled = false;
                    button_Takip_Etme_Baslat.Enabled = false;

                    // Nesneleri aktifleştir
                    button_Takip_Etme_Durdur.Enabled = true;

                    // bilgiler
                    label_Takip_Etme_Kalan.Text = "Kalan Takip Edilecek Hesap: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();
                    listBox_Takipci_Toplama_Takipci_Listesi.SelectedIndex = -1;
                    basarisizTakip = 0;

                    tarayiciDurdur = false;

                    gZaman = true;
                    kaydedilenGecenZaman = guncelZaman.ToString();

                    // thread başlatma
                    thread_TakipEtmeIslemi = new Thread(delegate()
                    {
                        hesabiTakipEt("");
                    });
                    thread_TakipEtmeIslemi.Start();
                }
            }
            else
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = "İşlem başlatılamıyor çünkü Takipçi Listesinde hesap yok.";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] İşlem başlatılamıyor çünkü Takipçi Listesinde hesap yok.");
                MessageBox.Show("İşlem başlatılamıyor çünkü Takipçi Listesinde hesap yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Durdur Butonu
        private void button_Takip_Etme_Durdur_Click(object sender, EventArgs e)
        {
            // başlatma sorusu
            DialogResult soru = MessageBox.Show("Toplu Takip Etme işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Başlatılırsa

                // Bilgi Mesajı
                label_Takipci_Toplama_Toplam.Text = "Toplam: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();

                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Toplu Takip Etme işlemi durduruldu!";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takip Etme işlemi durduruldu!");
                MessageBox.Show("Toplu Takip Etme işlemi durduruldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_Takip_Etme_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                groupBox_Kaynak_Profil_Listesi.Enabled = true;
                groupBox_Takipci_Toplama.Enabled = true;
                button_Takip_Etme_Baslat.Enabled = true;

                gZaman = false;
                kaydedilenGecenZaman = null;

                // thread durdurma
                thread_TakipEtmeIslemi.Abort();
                webBrowser1.Stop();
                tarayiciDurdur = true;
                gZaman = false;
                kaydedilenGecenZaman = null;
            }
        }
        #endregion

        #region Takip Etme Fonksiyonu
        void hesabiTakipEt(string profil = null)
        {
            // takipçi listesi tamamlanırsa
            if (listBox_Takipci_Toplama_Takipci_Listesi.Items.Count == listBox_Takipci_Toplama_Takipci_Listesi.SelectedIndex + 1)
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Toplu Takip Etme işlemi tamamlandı! Toplam Takip Edilen Hesap: " + listBox_Takip_Etme_Listesi.Items.Count.ToString();
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takip Etme işlemi tamamlandı! Toplam Takip Edilen Hesap: " + listBox_Takip_Etme_Listesi.Items.Count.ToString());
                MessageBox.Show("Toplu Takip Etme işlemi tamamlandı!\nToplam Takip Edilen Hesap: " + listBox_Takip_Etme_Listesi.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_Takip_Etme_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                groupBox_Kaynak_Profil_Listesi.Enabled = true;
                groupBox_Takipci_Toplama.Enabled = true;
                button_Takip_Etme_Baslat.Enabled = true;

                // thread durdurma
                thread_TakipEtmeIslemi.Abort();
                webBrowser1.Stop();
                tarayiciDurdur = true;
                gZaman = false;
                kaydedilenGecenZaman = null;
            }
            else
            {

                // profil alma
                listBox_Takipci_Toplama_Takipci_Listesi.SelectedIndex = listBox_Takipci_Toplama_Takipci_Listesi.SelectedIndex + 1;
                profil = listBox_Takipci_Toplama_Takipci_Listesi.Text;

                // url oluşturma
                string adres = textBox_TwitterURL.Text + profil;
                webBrowser1.Navigate(adres);
                islemdekiProfil = profil;
            }
        }
        #endregion

        #endregion

        #region 4- Zamanlayıcı İşlemleri
        private void timer1_Tick(object sender, EventArgs e)
        {
            // güncel zaman
            guncelZaman = DateTime.Now;

            #region Twitter engellediğinde bekleme işlemi
            // Bekleme süresi bittiğinde
            if (guncelZaman.ToString() == kaydedilenZaman)
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = "Bekleme süresi tamamlandı. Takip etme işlemi devam ediyor.";
               listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Bekleme süresi tamamlandı. Takip etme işlemi devam ediyor.");

                // thread başlatma
                thread_TakipEtmeIslemi = new Thread(delegate()
                {
                    hesabiTakipEt("");
                });
                thread_TakipEtmeIslemi.Start();
            }
            #endregion

            #region Geçen Zaman Hesaplama
            if (gZaman == true)
            {
                string tarih1 = kaydedilenGecenZaman;
                string tarih2 = guncelZaman.ToString();

                TimeSpan gecenSure = (Convert.ToDateTime(tarih1) - Convert.ToDateTime(tarih2));
                label_Takip_Etme_GecenZaman.Text = "[" + gecenSure.ToString().Replace("-","") + "]";
                //label_Takip_Etme_GecenZaman.Text = "[" + gecenSure.Hours.ToString("H") + ":" + gecenSure.Minutes.ToString("m") + ":" + gecenSure.Seconds.ToString("s") + "]";
            }
            #endregion

            #region Geçen Zaman Hesaplama 2
            if (gZaman2 == true)
            {
                string tarih11 = kaydedilenGecenZaman2;
                string tarih21 = guncelZaman.ToString();

                TimeSpan gecenSure2 = (Convert.ToDateTime(tarih11) - Convert.ToDateTime(tarih21));
                label_TakiptenCikarma_GecenSure.Text = "[" + gecenSure2.ToString().Replace("-", "") + "]";
            }
            #endregion
        }
        #endregion

        #endregion

        #region Takipçi Çıkarma Sekmesi

        #region 1- Takip Ettiğim Profilleri Listeleme

        #region Başlat butonu
        private void button_Takip_Ettiklerim_Baslat_Click(object sender, EventArgs e)
        {
            // başlatma sorusu
            DialogResult soru = MessageBox.Show("Takipçi Toplama işlemi başlatılsın mı?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Başlatılırsa

                // sıfırlama işlemleri
                listBox_Takip_Ettiklerim_Listesi.Items.Clear();
                label_Takip_Ettiklerim_Toplam.Text = "Toplam Profil: null";

                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Takip Ettiğim Takipçiler Toplama işlemi başlatıldı!";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takip Ettiğim Takipçiler Toplama işlemi başlatıldı!");
                MessageBox.Show("Takip Ettiğim Takipçiler Toplama işlemi başlatıldı!\n\nBu işlem internet bağlantı hızı ve bilgisayarınızın performansına göre değişkenlik gösterebilir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Pasifleşen nesneler
                button_Takip_Ettiklerim_Baslat.Enabled = false;

                // Aktifleşen nesneler
                button_Takip_Ettiklerim_Durdur.Enabled = true;

                // başlat
                thread_takipEttiklerimToplamaIslemi = new Thread(delegate()
                {
                    takipEttiklerimToplama(bagliHesap);
                });
                thread_takipEttiklerimToplamaIslemi.Start();
            }
        }
        #endregion

        #region Durdur butonu
        private void button_Takip_Ettiklerim_Durdur_Click(object sender, EventArgs e)
        {
            // başlatma sorusu
            DialogResult soru = MessageBox.Show("Takipçi Toplama işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Başlatılırsa

                // Bilgi Mesajı
                label_Takip_Ettiklerim_Toplam.Text = "Toplam Profil: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString();

                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Takipçi Toplama işlemi durduruldu!";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama işlemi durduruldu!");
                MessageBox.Show("Takipçi Toplama işlemi durduruldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_Takip_Ettiklerim_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                button_Takip_Ettiklerim_Baslat.Enabled = true;

                // thread durdurma
                thread_takipEttiklerimToplamaIslemi.Abort();
            }
        }
        #endregion

        #region Takipçi Toplama Fonksiyonu
        void takipEttiklerimToplama(string hesap, string cursor = "-1")
        {
            try
            {
                // hesap url düzenleme
                string hesapURL = textBox_Twitter_TakipEdilenURL.Text.Replace("hesap_kadi", hesap) + cursor;

                // Client sınıf tanımlama
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                string kaynakKod = client.DownloadString(hesapURL);

                // HtmlDocument sınıf tanımlama
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(kaynakKod);

                // Takipçileri alma
                try
                {
                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//span[@class='username']");
                    foreach (var veri in XPath)
                    {
                        // veri düzenleme
                        string profil = veri.InnerText;
                        profil = profil.Substring(1, profil.Length - 1);

                        if (listBox_Takip_Ettiklerim_Listesi.Items.Contains(profil) == false)
                        {
                            // takipçiyi listeye ekleme
                            if (textBox_Twitter_Kadi.Text.IndexOf(profil) != -1 || bagliHesap.IndexOf(profil) != -1)
                            {

                            }
                            else
                            {
                                listBox_Takip_Ettiklerim_Listesi.Items.Add(profil);
                            }

                            // toplam takipçi belirleme
                            label_Takip_Ettiklerim_Toplam.Text = "Toplam Profil: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString();
                        }
                    }
                }
                catch
                {
                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkRed;
                    statusLabel.Text = hesap + " profilin takip ettikleri çekilemiyor!";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] " + hesap + " profilin takip ettikleri çekilemiyor!");
                }

                // cursor alma
                try
                {
                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='w-button-more']/a");
                    foreach (var veri in XPath)
                    {
                        string link = veri.Attributes["href"].Value + "#";
                        veriAyiklama(link, "?cursor=", 8, "#");

                        // eğer bir sonraki sayfa varsa fonksiyonu tekrar çağır
                        thread_takipEttiklerimToplamaIslemi = new Thread(delegate()
                        {
                            takipEttiklerimToplama(hesap, ayiklananVeri);
                        });
                        thread_takipEttiklerimToplamaIslemi.Start();
                    }
                }
                catch
                {
                    // Bilgi Mesajı
                    label_Takip_Ettiklerim_Toplam.Text = "Toplam Profil: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString();

                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Takipçi Toplama işlemi tamamlandı! Bulunan Toplam Takipçi Sayısı: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString();
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Takipçi Toplama işlemi tamamlandı! Bulunan Toplam Takipçi Sayısı: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString());
                    MessageBox.Show("Takipçi Toplama işlemi tamamlandı!\nBulunan Toplam Takipçi Sayısı: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // thread durdurma
                    thread_takipEttiklerimToplamaIslemi.Abort();
                }

            } catch {  }
        }
        #endregion

        #endregion

        #region 2- Takip Etmeyi Bırak 

        #region Başlat butonu
        private void button_TakipBırak_Baslat_Click(object sender, EventArgs e)
        {
            if (listBox_Takip_Ettiklerim_Listesi.Items.Count > 1)
            {
                // başlatma sorusu
                DialogResult soru = MessageBox.Show("Toplu Takipten Çıkarma işlemi başlatılsın mı?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // Başlatılırsa

                    // sıfırlama işlemleri
                    listBox_Takipten_Cikarma.Items.Clear();
                    label_TakiptenCikarma_CikarilanHesaplar.Text = "Toplam Takipten Çıkarılan Hesap: null";
                    label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: null";
                    label_TakiptenCikarma_Basarisizlar.Text = "Başarısız Takipten Çıkarma İşlemi: null";
                    label_TakiptenCikarma_GecenSure.Text = "[00:00:00]";

                    // Bilgi Mesajı
                    statusLabel.ForeColor = Color.DarkGreen;
                    statusLabel.Text = "Toplu Takipten Çıkarma işlemi başlatıldı!";
                    listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takipten Çıkarma başlatıldı!");
                    MessageBox.Show("Toplu Takipten Çıkarma işlemi başlatıldı!\n\nBu işlem internet bağlantı hızı ve bilgisayarınızın performansına göre değişkenlik gösterebilir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Nesneleri pasifleştir
                    groupBox_Takip_Ettiklerim.Enabled = false;
                    button_TakipBırak_Baslat.Enabled = false;

                    // Nesneleri aktifleştir
                    button_TakipBırak_Durdur.Enabled = true;

                    // bilgiler
                    label_TakiptenCikarma_KalanHesaplar.Text = "Kalan Hesaplar: " + listBox_Takip_Ettiklerim_Listesi.Items.Count.ToString();
                    listBox_Takipten_Cikarma.SelectedIndex = -1;

                    basarisizCikarma = 0;
                    tarayiciDurdur2 = false;
                    gZaman2 = true;
                    takiptenCikarilanToplam = 0;

                    kaydedilenGecenZaman2 = guncelZaman.ToString();

                    // durum
                    islemDurumu = true;

                    // thread başlatma
                    thread_TakipBirakmaIslemi = new Thread(delegate()
                    {
                        hesabiCikar("");
                    });
                    thread_TakipBirakmaIslemi.Start();
                }
            }
            else
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkRed;
                statusLabel.Text = "İşlem başlatılamıyor çünkü Takip Ettiğim Profiller Listesinde hesap yok.";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] İşlem başlatılamıyor çünkü Takip Ettiğim Profiller Listesinde hesap yok.");
                MessageBox.Show("İşlem başlatılamıyor çünkü Takip Ettiğim Profiller Listesinde hesap yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Durdur butonu
        private void button_TakipBırak_Durdur_Click(object sender, EventArgs e)
        {
            // başlatma sorusu
            DialogResult soru = MessageBox.Show("Toplu Takipten Çıkarma işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Başlatılırsa

                // Bilgi Mesajı
                //label_Takipci_Toplama_Toplam.Text = "Toplam: " + listBox_Takipci_Toplama_Takipci_Listesi.Items.Count.ToString();

                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Toplu Takipten Çıkarma işlemi durduruldu!";
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takip Etme işlemi durduruldu!");
                MessageBox.Show("Toplu Takipten Çıkarma işlemi durduruldu!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_TakipBırak_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                groupBox_Takip_Ettiklerim.Enabled = true;
                button_TakipBırak_Baslat.Enabled = true;

                // thread durdurma
                thread_TakipBirakmaIslemi.Abort();
                webBrowser2.Stop();
                tarayiciDurdur2 = true;
                gZaman2 = false;
                kaydedilenGecenZaman2 = null;
                islemDurumu = false;
                takiptenCikarilanToplam = 0;
            }
        }
        #endregion

        #region Takip Etme Fonksiyonu
        void hesabiCikar(string profil = null)
        {
            // takipçi listesi tamamlanırsa
            if (listBox_Takip_Ettiklerim_Listesi.Items.Count == listBox_Takip_Ettiklerim_Listesi.SelectedIndex + 1)
            {
                // Bilgi Mesajı
                statusLabel.ForeColor = Color.DarkGreen;
                statusLabel.Text = "Toplu Takipten Çıkarma işlemi tamamlandı! Toplam Takipten Çıkarılan Hesap: " + listBox_Takipten_Cikarma.Items.Count.ToString();
                listBox_durum.Items.Insert(0, "[" + DateTime.Now + "] Toplu Takipten Çıkarma işlemi tamamlandı! Toplam Takipten Çıkarılan Hesap: " + listBox_Takipten_Cikarma.Items.Count.ToString());
                MessageBox.Show("Toplu Takipten Çıkarma işlemi tamamlandı!\nToplam Takipten Çıkarılan Hesap: " + listBox_Takipten_Cikarma.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Nesneleri pasifleştir
                button_TakipBırak_Durdur.Enabled = false;

                // Nesneleri aktifleştir
                groupBox_Takip_Ettiklerim.Enabled = true;
                button_TakipBırak_Baslat.Enabled = true;

                // thread durdurma
                thread_TakipBirakmaIslemi.Abort();
                webBrowser2.Stop();
                tarayiciDurdur2 = true;
                gZaman2 = false;
                kaydedilenGecenZaman2 = null;
                islemDurumu = false;
                takiptenCikarilanToplam = 0;
            }
            else
            {

                // profil alma
                listBox_Takip_Ettiklerim_Listesi.SelectedIndex = listBox_Takip_Ettiklerim_Listesi.SelectedIndex + 1;
                profil = listBox_Takip_Ettiklerim_Listesi.Text;

                // url oluşturma
                string adres = textBox_TwitterURL.Text + profil;
                webBrowser2.Navigate(adres);
                islemdekiProfil2 = profil;
            }
        }
        #endregion


        #endregion

        #endregion






    }
}
